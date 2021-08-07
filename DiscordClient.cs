using OneOf;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace SimpleDiscord
{
	/// <summary>
	/// Represents a client for a Discord bot account.
	/// </summary>
	/// <remarks>
	/// <para>This class contains functionality for maintaining a connection to a Discord bot (keeping it online), and provides methods for interacting with Discord's API. However, on its own, this class does not make the bot do anything. The intended usage of this class is to create a subclass that overrides the <see cref="HandleEvent(string, JsonElement)"/> method.</para>
	/// </remarks>
	public class DiscordClient : IDisposable
	{
		#region Fields and auto-implemented properties

		#region Static/constant fields

		// Some limits defined by Discord
		const int maxDiscordMessageLength = 2000;
		const int maxChannelNameLength = 100;
		const int maxChannelTopicLength = 1024;

		// Number of milliseconds to wait between sending stuff over the WebSocket connection.
		// The actual ratelimit is 120 commands per minute instead of 1 per half a second, but this is easier to code and I don't need to send that much over the connection anyway.
		const int webSocketRateLimit = 500;

		// Base URL for all HTTP requests.
		static readonly Uri httpApiBaseUrl = new Uri("https://discord.com/api/v8/");

		// Gateway URL for the WebSocket connection. Will be fetched when connecting for the first time, or when gatewayUrlExpires has passed.
		static Uri? gatewayUrl;

		// Will be set when the gateway URL is fetched.
		static DateTime gatewayUrlExpires;

		// The task that fetches the URL
		static Task? gatewayFetchTask;

		// Lock onto this before using the task
		static readonly object gatewayLockObject = new();

		#endregion

		readonly string token;

		// The connection to the gateway. Null if the client is disconnected.
		ClientWebSocket? webSocket;

		// Contains the data that still needs to be sent over the WebSocket connection, but hasn't been sent yet because isOnTimeout is true. The first item in the list will be sent first.
		// ALWAYS lock on this collection before using it!
		readonly LinkedList<ReadOnlyMemory<byte>> sendingQueue = new();

		bool isOnTimeout;

		Timer? sendingTimer;
		Timer? heartbeatTimer;

		// True if a heartbeat has been sent and the client is waiting for a HeartbeatAck opcode from the server.
		bool waitingForHeartbeatAck;

		// The last sequence number that was received from the WebSocket, or -1 if none has been received. Used for heartbeating.
		long lastSequenceNumber = -1;

		// Used for all HTTP requests to Discord.
		// This is not static because the constructor adds some client-specific default headers.
		readonly HttpClient httpClient = new HttpClient() { BaseAddress = httpApiBaseUrl };

		// Used to look up ratelimit buckets for HTTP requests.
		readonly Dictionary<string, RatelimitBucket> bucketsById = new();
		readonly Dictionary<(HttpMethod, string), RatelimitBucket> bucketsByRoute = new();

		/// <summary>
		/// Gets a value indicating whether or not this instance has been disposed and cannot be used for connecting.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this client has been disposed; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsDisposed { get; private set; }

		/// <summary>
		/// Gets the events that this client wishes to subscribe to, as specified in the constructor of this class.
		/// </summary>
		/// <value>
		/// The events that this client wishes to subscribe to.
		/// </value>
		protected GatewayIntents Intents { get; }

		/// <summary>
		/// Gets the ID of the current session, or the ID of the last session if that session was not ended.
		/// </summary>
		/// <remarks>
		/// <para>If this value is not <see langword="null"/> while connecting, the <see cref="Connect"/> method will attempt to resume the connection rather than starting a new session.</para>
		/// </remarks>
		/// <value>
		/// The ID of the current session. <see langword="null"/> if this <see cref="DiscordClient"/> instance has never connected before, or if the last session was ended.
		/// </value>
		protected string? SessionId { get; private set; }

		/// <summary>
		/// Gets the user ID of the bot account that this client is connected to. This value is only set when the client first receives the Ready event.
		/// </summary>
		/// <value>
		/// The string representation of the user ID of the bot, or <see langword="null"/> if that value is not known yet.
		/// </value>
		protected string? UserId { get; private set; }

		#endregion

		/// <summary>
		/// Constructs a client for a Discord bot account.
		/// </summary>
		/// <param name="token">The token of the bot account to use for authentication.</param>
		/// <param name="intents">The events that this client wishes to subscribe to.</param>
		/// <exception cref="ArgumentNullException"><paramref name="token"/> is <see langword="null"/>.</exception>
		public DiscordClient(string token, GatewayIntents intents)
		{
			if (token is null) throw new ArgumentNullException(nameof(token));

			if (token.StartsWith("Bot "))
			{
				this.token = token[4..];
			}
			else
			{
				this.token = token;
			}

			Intents = intents;

			httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", this.token);
		}

		#region WebSocket Connection

		/// <summary>
		/// Gets a value indicating whether this client is connected or connecting.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this client is connected or connecting; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsConnected => webSocket is not null;

		/// <summary>
		/// Connects the client to Discord and maintains the connection.
		/// </summary>
		/// <remarks>
		/// <para>This method makes the bot connect to Discord, keeps it online, and calls the <see cref="HandleEvent(string, JsonElement)"/> method whenever an event occurs. Make sure to override that method if you want to give the bot custom functionality; otherwise the bot will simply do nothing.</para>
		/// <para>You cannot call this method if this client is already connected or connecting; that is, when <see cref="IsConnected"/> is <see langword="true"/>.</para>
		/// </remarks>
		/// <returns>A <see cref="Task"/> that completes when the bot disconnects. The result is <see langword="true"/> if the caller should attempt to reconnect by calling <see cref="Connect"/> again.</returns>
		/// <exception cref="ObjectDisposedException">This object is disposed.</exception>
		/// <exception cref="InvalidOperationException">This client is already connected or connecting.</exception>
		/// <exception cref="WebSocketException">An error occurred in the WebSocket connection.</exception>
		public async Task<bool> Connect()
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
			if (IsConnected) throw new InvalidOperationException("This client is already connected or connecting.");

			webSocket = new ClientWebSocket();

			// This try/finally block makes sure the WebSocket is disposed when this method exits.
			try
			{
				// Ensure that we have a gateway URL
				Task? toAwait;
				lock (gatewayLockObject)
				{
					if (gatewayFetchTask is not null)
					{
						toAwait = gatewayFetchTask;
					}
					else if (gatewayUrl is null || gatewayUrlExpires < DateTime.UtcNow)
					{
						// Fetch & cache the URL to connect to
						toAwait = FetchGatewayUrl();
					}
					else toAwait = null;
				}
				if (toAwait is not null) await toAwait;

#if DEBUG
				Debug.Log("Connecting...");
#endif

				await webSocket.ConnectAsync(gatewayUrl!, CancellationToken.None);

				// This timer will be activated once there is data to send.
				if (sendingTimer is null) sendingTimer = new Timer(SendData, this, Timeout.Infinite, webSocketRateLimit);

				// Received bytes are written to this. The 4096 is an arbitrary guess, I'll look into it later.
				byte[] receiveBuffer = new byte[4096];
				ArraySegment<byte> receiveBufferSegment = new ArraySegment<byte>(receiveBuffer);

				// The receive loop
				while (true)
				{
					WebSocketReceiveResult receiveResult;
					try
					{
						receiveResult = await webSocket.ReceiveAsync(receiveBufferSegment, CancellationToken.None);
					}
#if DEBUG
					catch (WebSocketException exception)
					{
						Debug.Log($"An exception occurred in WebSocket connection: {exception}");
						Debug.Log($"Inner exception: {exception.InnerException}");
#else
					catch (WebSocketException)
					{
#endif
						Disconnect()?.Dispose();
						ResetSession();
						return true;
					}
					catch (OperationCanceledException)
					{
#if DEBUG
						Debug.Log("Receive operation was cancelled.");
#endif
						return false;
					}

					if (receiveResult.MessageType == WebSocketMessageType.Close)
					{
#if DEBUG
						Debug.Log($"Connection was closed with close status {receiveResult.CloseStatus}.");

						if (receiveResult.CloseStatusDescription is not null)
						{
							Debug.Log($"Close status description: \"{receiveResult.CloseStatusDescription}\"");
						}
#endif

						return true;
					}

					// Not sure what to do with binary messages; just skip them. Looks like Discord never sends binary messages anyway.
					if (receiveResult.MessageType != WebSocketMessageType.Text)
					{
#if DEBUG
						Debug.Warn($"Received non-text WebSocket message of {receiveResult.Count} bytes.");
#endif
						continue;
					}

					// Will be set to an object soon. This is here just so I can dispose it in the finally block.
					JsonDocument? receivedDocument = null;
					try
					{
						// Parse the JSON object. Documentation: https://discord.com/developers/docs/topics/gateway#payloads
						string received = Encoding.UTF8.GetString(receiveBuffer, 0, receiveResult.Count);
#if DEBUG
						Debug.Log($"Received: {received}");
#endif
						receivedDocument = JsonDocument.Parse(received);

						// Try get sequence number
						if (receivedDocument.RootElement.TryGetProperty("s", out JsonElement sequenceNumber) && sequenceNumber.ValueKind == JsonValueKind.Number)
						{
							lastSequenceNumber = sequenceNumber.GetInt64();
						}

						// Opcode. Reference: https://discord.com/developers/docs/topics/opcodes-and-status-codes#gateway-gateway-opcodes
						switch ((GatewayOpcode)receivedDocument.RootElement.GetProperty("op").GetInt32())
						{
							case GatewayOpcode.Hello:
								waitingForHeartbeatAck = false;

								// Start heartbeating
								int heartbeatInterval = receivedDocument.RootElement.GetProperty("d").GetProperty("heartbeat_interval").GetInt32();
								if (heartbeatTimer is null)
								{
									heartbeatTimer = new Timer(ScheduleHeartbeat, this, 0, heartbeatInterval);
								}
								else
								{
									heartbeatTimer.Change(0, heartbeatInterval);
								}

								// Not connected before: send Identify. Otherwise: send Resume.
								SendToGateway(Util.CreateJson(writer =>
								{
									writer.WriteNumber("op", (int)(SessionId is null ? GatewayOpcode.Identify : GatewayOpcode.Resume));

									writer.WriteStartObject("d");
									writer.WriteString("token", token);
									if (SessionId is null)
									{
										writer.WriteNumber("intents", (int)Intents);
										writer.WriteStartObject("properties");
										writer.WriteString("$os", Environment.OSVersion.VersionString);
										writer.WriteEndObject();
									}
									else
									{
										writer.WriteString("session_id", SessionId);
										writer.WriteNumber("seq", lastSequenceNumber);
									}
									writer.WriteEndObject();
								}));

								break;

							case GatewayOpcode.HeartbeatAck:
								waitingForHeartbeatAck = false;
								break;

							case GatewayOpcode.Reconnect:
								await GracefullyDisconnect(WebSocketCloseStatus.NormalClosure, "Reconnect was requested.");
								return true;

							//case GatewayOpcode.InvalidSession:
							//	await GracefullyDisconnect(WebSocketCloseStatus.NormalClosure, "Session is invalid.");
							//	return receivedDocument.RootElement.GetProperty("d").GetBoolean();

							case GatewayOpcode.Dispatch:
								JsonElement eventData = receivedDocument.RootElement.GetProperty("d");
								string eventName = receivedDocument.RootElement.GetProperty("t").GetString()!;

								if (eventName == "READY")
								{
									SessionId = eventData.GetProperty("session_id").GetString();
									UserId = eventData.GetProperty("user").GetProperty("id").GetString();

#if DEBUG
									Debug.Log($"Logged in as {eventData.GetProperty("user").GetProperty("username").GetString()} (id: {UserId})");
#endif
								}

								HandleEvent(eventName, eventData);

								// This is so that the document does not get disposed in the finally statement below.
								// The override of HandleEvent probably runs asynchronously, so it may still need the event data after the method exits.
								// HandleEvent can't dispose it for us because JsonElement does not implement IDisposable.
								// As far as I can tell, not disposing this will not cause memory leaks; only some arrays not being returned to the pool.
								// This is a performance deterioration I am willing to accept for now.
								receivedDocument = null;
								break;
						}
					}
#if DEBUG
					catch (Exception exception)
					{
						Debug.Warn("Exception occurred while interpreting response: " + exception);
					}
#else
					catch { }
#endif
					finally
					{
						receivedDocument?.Dispose();
					}
				}
			}
			finally
			{
				Disconnect()?.Dispose();
			}
		}

		/*
		/// <summary>
		/// Calls <see cref="HandleEvent(string, JsonElement)"/> with the appropriate values.
		/// </summary>
		/// <remarks>
		/// <para>The point of this method is to make sure that the <paramref name="receivedDocument"/> can be disposed only after <see cref="HandleEvent(string, JsonElement)"/> has completed,</para>
		/// </remarks>
		/// <param name="receivedDocument">The <see cref="JsonDocument"/> representing the JSON data received from the gateway.</param>
		private void HandleEvent(JsonDocument receivedDocument)
		{
			try
			{
				JsonElement eventData = receivedDocument.RootElement.GetProperty("d");
				string eventName = receivedDocument.RootElement.GetProperty("t").GetString()!;

				if (eventName == "READY")
				{
					SessionId = eventData.GetProperty("session_id").GetString();
					UserId = eventData.GetProperty("user").GetProperty("id").GetString();

#if DEBUG
					Debug.Log($"Logged in as {eventData.GetProperty("user").GetProperty("username").GetString()} (id: {UserId})");
#endif
				}

				await HandleEvent(eventName, eventData);
			}
			catch (Exception) { } // Ignore all exceptions from the HandleEvent method.
			finally
			{
				receivedDocument.Dispose();
			}
		}
		*/

		/// <summary>
		/// Performs bot-specific actions upon receiving gateway events. It is called from the <see cref="Connect"/> method.
		/// </summary>
		/// <remarks>
		/// <para>Your custom Discord client should override this method to add your bot's functionality. The base method does nothing.</para>
		/// <para>Note: the <paramref name="eventData"/> object will be disposed after the method has finished executing. This means that you can no longer use the object after any <see langword="await"/> calls.</para>
		/// <para>Exceptions thrown in this method will be caught and ignored in the <see cref="Connect"/> method.</para>
		/// </remarks>
		/// <param name="eventName">The name of the event. A full list of possible intents can be found in <see href="https://discord.com/developers/docs/topics/gateway#commands-and-events">Discord's documentation</see>.</param>
		/// <param name="eventData">The event-specific event data that was received. This document will not be disposed, although that behaviour may be changed in the future.</param>
		/// <exception cref="Exception">An uncaught exception occurred while handling the event.</exception>
		protected virtual void HandleEvent(string eventName, JsonElement eventData)
		{
#if DEBUG
			Debug.Log("The base HandleEvent method was called.");

			// This is a command just for testing ratelimits
			if (eventData.GetProperty("content").GetString()!.StartsWith("!spam") && int.TryParse(eventData.GetProperty("content").GetString()![5..].Trim(), out int spamCount))
			{
				for (int spamIteration = 0; spamIteration < spamCount; spamIteration++)
				{
					_ = SendChatMessage(eventData.GetProperty("channel_id").GetString()!, "Spam " + spamIteration).ContinueWith(task => task.Result.Dispose());
				}
			}
#endif
		}

		// Sets the gatewayUrl field to an appropriate value
		private async Task FetchGatewayUrl()
		{
			try
			{
				using HttpResponseMessage gatewayResponse = await httpClient.GetAsync("gateway", HttpCompletionOption.ResponseHeadersRead);

				using JsonDocument gatewayResponseDocument = JsonDocument.Parse(await gatewayResponse.Content.ReadAsStreamAsync());
				UriBuilder gatewayUrlBuilder = new UriBuilder(gatewayResponseDocument.RootElement.GetProperty("url").GetString()!);

				// Add a query to the URL, as specified in https://discord.com/developers/docs/topics/gateway#connecting-gateway-url-params
				NameValueCollection gatewayUrlQuery = HttpUtility.ParseQueryString(gatewayUrlBuilder.Query);
				gatewayUrlQuery["v"] = "8";
				gatewayUrlQuery["encoding"] = "json";
				gatewayUrlBuilder.Query = gatewayUrlQuery.ToString();

				gatewayUrl = gatewayUrlBuilder.Uri;

#if DEBUG
				Debug.Log("Gateway URL: " + gatewayUrl);
#endif

				if (gatewayResponse.Headers.CacheControl is not null && gatewayResponse.Headers.CacheControl.MaxAge is not null)
				{
					gatewayUrlExpires = DateTime.UtcNow + (TimeSpan)gatewayResponse.Headers.CacheControl.MaxAge;
				}
			}
#if DEBUG
			catch (Exception exception)
			{
				Debug.Warn($"Fetching the gateway URL failed: {exception}");
#else
			catch
			{
#endif
				// Use previous URL or default to the normal one.
				gatewayUrl ??= new Uri(@"wss://gateway.discord.gg/?v=8&encoding=json", UriKind.Absolute);
			}
		}

		/// <summary>
		/// Disconnects this client gracefully if it is connected, and resets the session.
		/// </summary>
		/// <param name="closeStatus">The WebSocket close status.</param>
		/// <param name="statusDescription">A description of the close status.</param>
		/// <returns>A <see cref="Task"/> that completes once the WebSocket connection has been closed.</returns>
		/// <seealso cref="AbruptlyDisconnect"/>
		public async Task GracefullyDisconnect(WebSocketCloseStatus closeStatus, string? statusDescription)
		{
			using ClientWebSocket? oldWebSocket = Disconnect();
			ResetSession();
			if (oldWebSocket is not null)
			{
				await oldWebSocket.CloseAsync(closeStatus, statusDescription, CancellationToken.None);
			}
		}

		/// <summary>
		/// Disconnects this client abruptly if it is connected, and resets the session.
		/// </summary>
		/// <seealso cref="GracefullyDisconnect"/>
		public void AbruptlyDisconnect()
		{
			Disconnect()?.Dispose();
			ResetSession();
		}

		/// <summary>
		/// Puts the client into a disconnected state if it is connected, but does not close the connection.
		/// </summary>
		/// <remarks>
		/// <para>This method leaves the <see cref="SessionId"/> intact, so that <see cref="Connect"/> will be able to resume the connection.</para>
		/// </remarks>
		/// <returns>The old value <see cref="webSocket"/>, so that the caller can properly close the connection. <see langword="null"/> if this client was already disconnected.</returns>
		private ClientWebSocket? Disconnect()
		{
			ClientWebSocket? oldWebSocket;
			lock (sendingQueue) // This lock is there so that nothing gets sent over the WebSocket while we're disconnecting.
			{
				oldWebSocket = webSocket;
				webSocket = null;
				sendingQueue.Clear();
				isOnTimeout = false;
			}

			sendingTimer?.Change(Timeout.Infinite, webSocketRateLimit);
			heartbeatTimer?.Change(Timeout.Infinite, 0);

			return oldWebSocket;
		}

		/// <summary>
		/// Resets the session. Should only be used while the client is disconnected.
		/// </summary>
		private void ResetSession()
		{
			SessionId = null;
			lastSequenceNumber = -1;
		}

		/// <summary>
		/// Schedules data to be sent to the gateway over the WebSocket connection.
		/// </summary>
		/// <param name="data">The data to be sent.</param>
		/// <param name="priority"><see langword="true"/> if this data should take priority over other data if constrained by ratelimits; otherwise, false.</param>
		private void SendToGateway(ReadOnlyMemory<byte> data, bool priority = false)
		{
			if (sendingTimer is null) throw new InvalidOperationException("Client is not connected.");

			lock (sendingQueue)
			{
				if (!IsConnected) return;

				if (priority)
				{
					sendingQueue.AddFirst(data);
				}
				else
				{
					sendingQueue.AddLast(data);
				}

				// If we are already on a timeout, then simply adding to the collection is enough as the sendingTimer will eventually send it. Otherwise, we need to restart the sendingTimer.
				if (!isOnTimeout)
				{
					isOnTimeout = true;
					sendingTimer.Change(0, webSocketRateLimit);
				}
			}
		}

		/// <summary>
		/// The <see cref="TimerCallback"/> for the <see cref="sendingTimer"/>. Sends the first item in the <see cref="sendingQueue"/>.
		/// </summary>
		/// <param name="state">The <see cref="DiscordClient"/> that should send the data.</param>
		private static async void SendData(object? state)
		{
			if (state is not DiscordClient client) throw new InvalidOperationException("Expected client instance.");

			ValueTask sendingTask;
			lock (client.sendingQueue)
			{
				if (!client.IsConnected) return;

				if (client.sendingQueue.Count == 0)
				{
					// Nothing left to send so pause the timer
					client.sendingTimer!.Change(Timeout.Infinite, webSocketRateLimit);
					client.isOnTimeout = false;
					return;
				}

				ReadOnlyMemory<byte> toSend = client.sendingQueue.First!.Value;
				client.sendingQueue.RemoveFirst();

#if DEBUG
				Debug.Log("Sending: " + Encoding.UTF8.GetString(toSend.Span));
#endif

				sendingTask = client.webSocket!.SendAsync(toSend, WebSocketMessageType.Text, true, CancellationToken.None);
			}

			// The await keyword makes no functional difference, but code analysis told me to await it so whatever.
			await sendingTask;
		}

		/// <summary>
		/// The <see cref="TimerCallback"/> for the <see cref="heartbeatTimer"/>. Schedules a heartbeat message to be sent.
		/// </summary>
		/// <param name="state">The <see cref="DiscordClient"/> instance that should send the heartbeat.</param>
		private static void ScheduleHeartbeat(object? state)
		{
			if (state is not DiscordClient client) throw new InvalidOperationException("Expected client instance.");

			// Disconnect if no acknowledgement was received
			if (client.waitingForHeartbeatAck)
			{
#if DEBUG
				Debug.Warn("Did not receive a heartbeat acknowledgement; disconnecting.");
#endif
				_ = client.GracefullyDisconnect(WebSocketCloseStatus.ProtocolError, "Last heartbeat was not acknowledged.");
				return;
			}

			// Send heartbeat message
			client.SendToGateway(Util.CreateJson(writer =>
			{
				writer.WriteNumber("op", (int)GatewayOpcode.Heartbeat);

				if (client.lastSequenceNumber == -1)
				{
					writer.WriteNull("d");
				}
				else
				{
					writer.WriteNumber("d", client.lastSequenceNumber);
				}
			}), true);

			client.waitingForHeartbeatAck = true;
		}

		#endregion

		#region HTTP API

		/// <summary>
		/// Sends an HTTP request, optionally taking ratelimits into account.
		/// </summary>
		/// <param name="method">The HTTP method to use.</param>
		/// <param name="endpoint">The identifier for the route that this HTTP request is for. This identifier should include major path variables (as defined in <see href="https://discord.com/developers/docs/topics/rate-limits">Discord's documentation</see>), but all minor path variables should be left out. For example, this could look like <c>channels/1234/messages/{0}</c>.</param>
		/// <param name="path">The complete path to which to send the HTTP request, which includes <em>all</em> path variables. This should be a relative path without a preceding <c>/</c>. For example, this could look like <c>channels/1234/messages/5678</c>. <see langword="null"/> indicats that this value is the same as <paramref name="endpoint"/>.</param>
		/// <param name="content">If not <see langword="null"/>, this will be the content of the request. This must be a JSON object encoded as UTF-8.</param>
		/// <param name="reason">If not <see langword="null"/>, this reason will be specified in the audit log.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>A <see cref="Task{DiscordRequestResult}"/> instance that completes once the entire HTTP request is complete. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="endpoint"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="method"/> is <see langword="null"/>.</exception>
		/// <exception cref="UriFormatException"><paramref name="path"/> is not a valid URI.</exception>
		protected async Task<DiscordRequestResult> SendHttpRequest(
			HttpMethod method,
			string endpoint,
			string? path,
			ReadOnlyMemory<byte>? content = null,
			string? reason = null,
			CancellationToken cancellationToken = default
		)
		{
			if (endpoint is null) throw new ArgumentNullException(nameof(endpoint));
			path ??= endpoint;

#if DEBUG
			Debug.Log($"Sending {method} request to {path}...");
#endif

			HttpRequestMessage request = new HttpRequestMessage(method, path);
			if (content is not null)
			{
				request.Content = new ReadOnlyMemoryContent((ReadOnlyMemory<byte>)content);
				request.Content.Headers.ContentType = new MediaTypeHeaderValue("application/json") { CharSet = "utf-8" };
			}
			if (reason is not null) request.Headers.Add("X-Audit-Log-Reason", reason);

			try
			{
				Task<DiscordRequestResult>? sendingTask = null;
				RatelimitBucket? bucket;
				
				(HttpMethod, string) key = (method, endpoint);

				// Get or create bucket for this route
				lock (bucketsByRoute)
				{
					if (!bucketsByRoute.TryGetValue(key, out bucket))
					{
						bucket = new();
						bucketsByRoute.Add(key, bucket);
					}
				}

				// Wait for a chance to start sending.
				while (true)
				{
					Task? waitTask = null; // In case we need to wait for something

					lock (bucket)
					{
						if (bucket.isDuplicate)
						{
							// Lock onto the new bucket instead. Ignore the compiler warning.
							bucket = bucketsByRoute[key];
							continue;
						}
						if (bucket.IsActive)
						{
							if (bucket.remaining > 0)
							{
								// Claim it (sending the request will happen outside the loop)
								bucket.remaining--;
								break;
							}
								
							// Wait for expiration
							waitTask = Task.Delay(bucket.TimeLeft, cancellationToken);
						}
						else if (bucket.firstRequest is null)
						{
							// Claim it
							sendingTask = ActuallySendHttpRequest(request, bucket, key, cancellationToken);
							bucket.firstRequest = sendingTask;
							break;
						}
						else
						{
							waitTask = bucket.firstRequest;
						}
					}

					if (waitTask is not null)
					{
						try
						{
							await waitTask;
						}
						catch { }
					}
				}

				sendingTask ??= ActuallySendHttpRequest(request, bucket, key, cancellationToken);

				return await sendingTask;
			}
			catch (Exception exception)
			{
#if DEBUG
				Debug.Warn($"An unexpected exception occurred while sending an HTTP request: {exception}");
#endif
				return new DiscordRequestResult(exception);
			}
		}

		/// <summary>
		/// Internal method for sending an HTTP request and updating the values of the bucket. This method is only called in the <see cref="SendHttpRequest"/> method.
		/// </summary>
		/// <param name="request">The request that should be made.</param>
		/// <param name="bucket">The ratelimit bucket that should be updated.</param>
		/// <param name="key">The key that contains <paramref name="bucket"/> in the <see cref="bucketsByRoute"/> dictionary.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="request"/> is <see langword="null"/>.</exception>
		/// <exception cref="InvalidOperationException">The request message was already sent by the <see cref="httpClient"/>.</exception>
		// The whole point of this method, by the way, is to make sure that the sending of the request and the updating of the values are all in one task, so that in the SendHttpRequest method the whole process can be awaited in one go.
		private async Task<DiscordRequestResult> ActuallySendHttpRequest(HttpRequestMessage request, RatelimitBucket bucket, (HttpMethod, string) key, CancellationToken cancellationToken = default)
		{
			//HttpResponseMessage response = await httpClient.SendAsync(request);

			HttpResponseMessage response;
			try
			{
				response = await httpClient.SendAsync(request, cancellationToken);
			}
			catch (Exception exception)
			{
				lock (bucket)
				{
					bucket.firstRequest = null;
				}
				return new DiscordRequestResult(exception);
			}

#if DEBUG
			if (response.StatusCode == HttpStatusCode.TooManyRequests)
			{
				Debug.Warn($"Exceeded a rate limit at {request.RequestUri}");
			}
#endif

			try
			{
				// Check the ID of the bucket
				RatelimitBucket? actualBucket;
				lock (bucket)
				{
					if (response.Headers.Contains(RatelimitBucket.idHeader))
					{
						string id = response.Headers.GetValues(RatelimitBucket.idHeader).First();
						lock (bucketsById)
						{
							if (bucketsById.TryGetValue(id, out actualBucket))
							{
								if (actualBucket != bucket)
								{
									lock (bucketsByRoute)
									{
										bucketsByRoute[key] = actualBucket;
									}
									bucket.isDuplicate = true;
								}
							}
							else
							{
								actualBucket = bucket;
								bucketsById.Add(id, bucket);
							}
						}
					}
					else actualBucket = bucket;

					bucket.firstRequest = null;
				}

				// Update the bucket info
				lock (actualBucket)
				{
					if (response.Headers.Contains(RatelimitBucket.limitHeader))
					{
						actualBucket.limit = int.Parse(response.Headers.GetValues(RatelimitBucket.limitHeader).First(), CultureInfo.InvariantCulture);
					}

					if (response.Headers.Contains(RatelimitBucket.remainingHeader))
					{
						int remaining = int.Parse(response.Headers.GetValues(RatelimitBucket.remainingHeader).First(), CultureInfo.InvariantCulture);
						if (!actualBucket.IsActive || remaining < actualBucket.remaining) // That's in case responses get handled out of order
						{
							actualBucket.remaining = remaining;
						}
					}

					if (response.Headers.Contains(RatelimitBucket.resetHeader))
					{
						// Again in case responses arrive in the wrong order
						DateTime newDate = DateTime.UnixEpoch + TimeSpan.FromSeconds(double.Parse(response.Headers.GetValues(RatelimitBucket.resetHeader).First(), CultureInfo.InvariantCulture));
						if (newDate > actualBucket.reset) actualBucket.reset = newDate;
					}

#if DEBUG
					Debug.Log($"Set a ratelimit bucket to: isDuplicate = {actualBucket.isDuplicate}, Limit = {actualBucket.limit}, Remaining = {actualBucket.remaining}, Reset = {actualBucket.reset}"); Debug.Log("Got response: " + response);

					/*
					if (response.Content.Headers.ContentLength is > 0)
					{
						Debug.Log(" "+await response.Content.ReadAsStringAsync());
					}
					//*/

					if (!response.IsSuccessStatusCode)
					{
						Debug.Log($"Received status code {(int)response.StatusCode} {response.ReasonPhrase} from {response.RequestMessage!.Method} {response.RequestMessage.RequestUri}");

						/*
						if (response.Content.Headers.ContentType is not null && response.Content.Headers.ContentType.MediaType == "application/json")
						{
							using JsonDocument error = JsonDocument.Parse(response.Content.ReadAsStream(CancellationToken.None));

							Debug.Warn($" Error: {error.RootElement.GetProperty("code").GetRawText()} {error.RootElement.GetProperty("message").GetString()}");

							if (error.RootElement.TryGetProperty("errors", out JsonElement errors))
							{
								Debug.Log(" " + errors.GetRawText());
							}
						}
						//*/
					}
#endif
				}
			}
			catch
#if DEBUG
				(Exception exception)
#endif
			{
				lock (bucket)
				{
					bucket.firstRequest = null;
				}
#if DEBUG
				Debug.Log($"An exception was thrown while parsing a request result: {exception}");
				Debug.Log($"Headers: {response.Headers}");
#endif
			}

			return new DiscordRequestResult(response, bucket);
		}

		/// <summary>
		/// Sends a chat message in a channel. Requires <c>MANAGE_MESSAGES</c> permission if in a guild.
		/// </summary>
		/// <remarks>
		/// <para>Before using this method, the client must be conneted and have received the Ready event.</para>
		/// <para>At least one of <paramref name="content"/> and <paramref name="embeds"/> must be specified.</para>
		/// </remarks>
		/// <param name="channelId">The ID of the channel.</param>
		/// <param name="content">The content of the message to send.</param>
		/// <param name="referencedMessageId">If not <see langword="null"/>, the posted message will be a reply to the message ID.</param>
		/// <param name="embeds">Lists the message's embeds.</param>
		/// <param name="components">Lists the message components. Each subcollection represents one action row, and each element of that action row is a message component.</param>
		/// <param name="disallowMentions"><see langword="true"/> if all mentions in the message should be suppressed; otherwise, <see langword="false"/>.</param>
		/// <param name="isTts"><see langword="true"/> if this should be a text to speech message; otherwise, <see langword="false"/>.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="referencedMessageId"/> is not <see langword="null"/> and not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="content"/> and <paramref name="embeds"/> are both <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="content"/> is specified but longer than the maximum allowed.</exception>
		/// <param name="isTts">If <see langword="true"/>, this will be a TTS message.</param>
		protected async Task<DiscordRequestResult> SendChatMessage(
			string channelId,
			string? content,
			string? referencedMessageId = null,
			IEnumerable<IEmbed>? embeds = null,
			IEnumerable<IActionRowComponent>? components = null,
			bool disallowMentions = false,
			bool isTts = false,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			Util.ThrowIfInvalidId(referencedMessageId, nameof(referencedMessageId), true);

			if (content is null)
			{
				if (embeds is null) throw new ArgumentException("Content and embeds are both null.");
			}
			else if (content.Length > maxDiscordMessageLength)
			{
				throw new ArgumentException("Message content cannot be longer than " + maxDiscordMessageLength + " characters.", nameof(content));
			}

			ReadOnlyMemory<byte> bodyContent = Util.CreateJson(writer =>
			{
				WriteMessageJson(writer, content, null, referencedMessageId, embeds, components, disallowMentions, isTts);
			});

#if DEBUG
			Debug.Log("SENDING MESSAGE CONTENT:");
			Debug.Log(Encoding.UTF8.GetString(bodyContent.Span));
#endif

			return await SendHttpRequest(HttpMethod.Post, $"channels/{channelId}/messages", null, bodyContent, null, cancellationToken);
		}

		/// <summary>
		/// Edits a chat message in a channel. Requires the <c>MANAGE_MESSAGES</c> permission when editing another user's message.
		/// </summary>
		/// <remarks>
		/// <para>For messages by other users, only <paramref name="flags"/> can be edited.</para>
		/// </remarks>
		/// <param name="channelId">The ID of the channel containing the message to edit.</param>
		/// <param name="messageId">The ID of the message to edit.</param>
		/// <param name="content">The new content of the message, or <see langword="null"/> to not edit the message's content.</param>
		/// <param name="embeds">The new list of embeds of the message, or <see langword="null"/> to not edit the message's embeds.</param>
		/// <param name="components">The new list of components of the message, or <see langword="null"/> to not edit the message's components.</param>
		/// <param name="flags">The new flags of the message, or <see langword="null"/> to not edit the message's flags. When specifying flags, ensure to include all previously set flags/bits in addition to ones that you are modifying. Currently, only <see cref="MessageFlags.SuppressEmbeds"/> can currently be set/unset.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="messageId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="messageId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> EditChatMessage(
			string channelId,
			string messageId,
			string? content = null,
			IEnumerable<IEmbed>? embeds = null,
			IEnumerable<IActionRowComponent>? components = null,
			MessageFlags? flags = null,
			bool disallowMentions = false,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			Util.ThrowIfInvalidId(messageId, nameof(messageId));

			string endpoint = $"channels/{channelId}/messages/{{0}}";

			return await SendHttpRequest(HttpMethod.Patch, endpoint, string.Format(endpoint, messageId), Util.CreateJson(writer =>
			{
				WriteMessageJson(writer, content, flags, null, embeds, components, disallowMentions);
			}), cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Deletes a chat message. If operating on a guild channel and trying to delete a message that was not sent by the current user, this endpoint requires the <c>MANAGE_MESSAGES</c> permission.
		/// </summary>
		/// <param name="channelId">The ID of the channel containing the message to delete.</param>
		/// <param name="messageId">The ID of the message to delete.</param>
		/// <param name="reason">The reason for deleting the message.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="messageId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="messageId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> DeleteChatMessage(string channelId, string messageId, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			Util.ThrowIfInvalidId(messageId, nameof(messageId));

			string endpoint = $"channels/{channelId}/messages/{{0}}";

			return await SendHttpRequest(HttpMethod.Delete, endpoint, string.Format(endpoint, messageId), null, reason, cancellationToken);
		}

		/// <summary>
		/// Pins a message to a channel. Requires <c>MANAGE_MESSAGES</c> permission.
		/// </summary>
		/// <param name="channelId">The channel in which to pin the message.</param>
		/// <param name="messageId">The message to pin.</param>
		/// <param name="reason">The reason for pinning the message.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="messageId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="messageId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> PinMessage(string channelId, string messageId, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			Util.ThrowIfInvalidId(messageId, nameof(messageId));

			string endpoint = $"channels/{channelId}/pins/{{0}}";

			return await SendHttpRequest(HttpMethod.Put, endpoint, string.Format(endpoint, messageId), null, reason, cancellationToken);
		}

		/// <summary>
		/// Creates a reaction for a message. Requires <c>READ_MESSAGE_HISTORY</c> permission, and if no one else has reacted to the message using this emoji, requires <c>ADD_REACTIONS</c> permission.
		/// </summary>
		/// <param name="channelId">The ID of the channel containing the message to react to.</param>
		/// <param name="messageId">The ID of the message to react to.</param>
		/// <param name="emoji">The emoji to add. For Unicode emoji, this is the string representation; for custom emoji, the format <c>name:id</c> is used.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="messageId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="messageId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="emoji"/> is <see langword="null"/>.</exception>
		protected async Task<DiscordRequestResult> CreateReaction(string channelId, string messageId, string emoji, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			Util.ThrowIfInvalidId(messageId, nameof(messageId));
			if (emoji is null) throw new ArgumentNullException(nameof(emoji));

			string endpoint = $"channels/{channelId}/messages/{{0}}/reactions/{{1}}/@me";

			return await SendHttpRequest(HttpMethod.Put, endpoint, string.Format(endpoint, messageId, Uri.EscapeDataString(emoji)), cancellationToken: cancellationToken);
		}

		/// <param name="emoji">The emoji to add.</param>
		/// <inheritdoc cref="CreateReaction(string, string, string, CancellationToken)"/>
		/// <exception cref="ArgumentException"><paramref name="emoji"/> returns <see langword="null"/> for both <see cref="IEmoji.Id"/> and <see cref="IEmoji.Name"/>.</exception>
		protected async Task<DiscordRequestResult> CreateReaction(string channelId, string messageId, IEmoji emoji, CancellationToken cancellationToken = default)
		{
			if (emoji is null) throw new ArgumentNullException(nameof(emoji));

			string? emojiText = emoji.Id;
			if (emojiText is null)
			{
				emojiText = emoji.Name;
				if (emojiText is null) throw new ArgumentException(nameof(emoji) + " does not have a name or ID.", nameof(emoji));
			}
			else
			{
				emojiText = $"{emoji.Name}:{emojiText}";
			}

			return await CreateReaction(channelId, messageId, emojiText, cancellationToken);
		}

		/// <summary>
		/// Creates a new channel in a guild. Requires the <c>MANAGE_CHANNELS</c> permission.
		/// </summary>
		/// <remarks>
		/// <para>If setting permission overwrites, only permissions your bot has in the guild can be allowed/denied. Setting <c>MANAGE_ROLES</c> permission in channels is only possible for guild administrators.</para>
		/// <para>Returns the new channel object on success. Fires a Channel Create Gateway event.</para>
		/// </remarks>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="name">The name of the channel (1-100 characters).</param>
		/// <param name="channelType">The type of channel.</param>
		/// <param name="topic">The topic of the channel (0-1024 characters)</param>
		/// <param name="parentId">ID of the parent category for a channel.</param>
		/// <param name="position">The sorting position of the channel, with 0 being at the bottom.</param>
		/// <param name="permissionOverwrites">The permission overwrites of the channel.</param>
		/// <param name="isNsfw"><see langword="true"/> if the channel is NSFW; otherwise, <see langword="false"/>.</param>
		/// <param name="reason">The reason for creating the channel.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="parentId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is an empty string.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> has more characters than allowed by Discord.</exception>
		/// <exception cref="ArgumentException"><paramref name="topic"/> has more characters than allowed by Discord.</exception>
		protected async Task<DiscordRequestResult> CreateChannel(
			string guildId,
			string name,
			ChannelType channelType,
			string? topic = null,
			string? parentId = null,
			int position = 0,
			IEnumerable<IPermissionOverwrite>? permissionOverwrites = null,
			bool isNsfw = false,
			string? reason = null,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			Util.ThrowIfInvalidId(parentId, nameof(parentId), true);

			if (name is null) throw new ArgumentNullException(nameof(name));
			if (name.Length == 0) throw new ArgumentException("Channel name is an empty string.", nameof(name));
			if (name.Length > maxChannelNameLength) throw new ArgumentException("Channel name is longer than " + maxChannelNameLength + " characters.", nameof(name));
			if (topic is not null && topic.Length > maxChannelTopicLength) throw new ArgumentException("Channel topic is longer than " + maxChannelTopicLength + " characters.", nameof(topic));

			return await SendHttpRequest(HttpMethod.Post, $"guilds/{guildId}/channels", null, Util.CreateJson(writer =>
			{
				writer.WriteString("name", name);
				writer.WriteNumber("type", (int)channelType);
				if (topic is not null) writer.WriteString("topic", topic);
				if (parentId is not null) writer.WriteString("parent_id", parentId);
				if (position != 0) writer.WriteNumber("position", position);
				if (permissionOverwrites is not null)
				{
					writer.WriteStartArray("permission_overwrites");
					foreach (IPermissionOverwrite overwrite in permissionOverwrites)
					{
						writer.WriteStartObject();
						writer.WriteString("id", overwrite.Id);
						writer.WriteNumber("type", (int)overwrite.Type);
						writer.WriteString("allow", overwrite.Allow);
						writer.WriteString("deny", overwrite.Deny);
						writer.WriteEndObject();
					}
					writer.WriteEndArray();
				}
				if (isNsfw) writer.WriteBoolean("nsfw", true);
			}), reason, cancellationToken);
		}

		/// <summary>
		/// Updates a channel's settings. Requires the <c>MANAGE_ROLES</c> permission for guild channels.
		/// </summary>
		/// <param name="channelId">The ID of the channel to modify.</param>
		/// <param name="name">The new name of the channel, or <see langword="null"/> to leave the channel name unmodified.</param>
		/// <param name="reason">The reason to modifying the channel.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="channelId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="channelId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> is an empty string.</exception>
		/// <exception cref="ArgumentException"><paramref name="name"/> has more characters than allowed by Discord.</exception>
		protected async Task<DiscordRequestResult> ModifyChannel(string channelId, string? name, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(channelId, nameof(channelId));
			if (name is not null)
			{
				if (name.Length == 0) throw new ArgumentException("Channel name is an empty string.", nameof(name));
				if (name.Length > maxChannelNameLength) throw new ArgumentException("Channel name is longer than " + maxChannelNameLength + " characters.", nameof(name));
			}

			return await SendHttpRequest(HttpMethod.Patch, $"channels/{channelId}", null, Util.CreateJson(writer =>
			{
				if (name is not null)
				{
					writer.WriteString("name", name);
				}
			}), reason, cancellationToken);
		}

		/// <summary>
		/// Creates a new DM channel or gets the DM channel with a user.
		/// </summary>
		/// <param name="userId">The ID of the recipient.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="userId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> CreateDm(string userId, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(userId, nameof(userId));
			return await SendHttpRequest(HttpMethod.Post, "users/@me/channels", null, Util.CreateJson(writer =>
			{
				writer.WriteString("recipient_id", userId);
			}), cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Creates a new role a guild. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="name">The name of the role.</param>
		/// <param name="permissions">String representation of the bitwise value of the enabled permissions.</param>
		/// <param name="colour">The RGB colour value, or 0 for a role without colour.</param>
		/// <param name="isHoisted"><see langword="true"/> if this role should be pinned in the user listing; otherwise, <see langword="false"/>.</param>
		/// <param name="isMentionable"><see langword="true"/> if this role should be mentionable; otherwise, <see langword="false"/>.</param>
		/// <param name="reason">The reason for creating the role.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> CreateRole(
			string guildId,
			string? name,
			string? permissions = null,
			int colour = 0,
			bool isHoisted = false,
			bool isMentionable = false,
			string? reason = null,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			return await SendHttpRequest(HttpMethod.Post, $"guilds/{guildId}/roles", null, Util.CreateJson(writer =>
			{
				if (name is not null) writer.WriteString("name", name);
				if (permissions is not null) writer.WriteString("permissions", permissions);
				if (colour != 0) writer.WriteNumber("color", colour);
				if (isHoisted) writer.WriteBoolean("hoist", true);
				if (isMentionable) writer.WriteBoolean("mentionable", true);
			}), reason, cancellationToken);
		}

		/// <summary>
		/// Gets all roles in a guild.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> GetGuildRoles(string guildId, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			return await SendHttpRequest(HttpMethod.Get, $"guilds/{guildId}/roles", null, cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Modifies the positions of a set of roles for the guild. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="positions">The new positions of the role.</param>
		/// <param name="reason">The reason for modifying the positions.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="positions"/> is <see langword="null"/>.</exception>
		protected async Task<DiscordRequestResult> ModifyGuildRolePositions(string guildId, IEnumerable<IRolePosition> positions, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			if (positions is null) throw new ArgumentNullException(nameof(positions));

			return await SendHttpRequest(HttpMethod.Patch, $"guilds/{guildId}/roles", null, Util.CreateJsonArray(writer =>
			{
				foreach (IRolePosition position in positions)
				{
					writer.WriteStartObject();
					writer.WriteString("id", position.Id);
					writer.WriteNumber("position", position.Position);
					writer.WriteEndObject();
				}
			}), reason, cancellationToken);
		}

		/// <summary>
		/// Modifies a guild role. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="name">The new name of the role, or <see langword="null"/> to leave the role name unmodified.</param>
		/// <param name="permissions">String representation of the bitwise value of the enabled permissions, or <see langword="null"/> to leave the permissions unmodified.</param>
		/// <param name="colour">The RGB colour value, or 0 for a role without colour, or <see langword="null"/> to leave the colour unmodified.</param>
		/// <param name="isHoisted"><see langword="true"/> if this role should be pinned in the user listing, <see langword="false"/> if it should not, or <see langword="null"/> to leave the value unmodified.</param>
		/// <param name="isMentionable"><see langword="true"/> if this role should be mentionable, <see langword="false"/> if it should not, or <see langword="null"/> to leave the value unmodified.</param>
		/// <param name="reason">The reason for modifying the role.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> ModifyGuildRole(
			string guildId,
			string roleId,
			string? name = null,
			string? permissions = null,
			int? colour = null,
			bool? isHoisted = null,
			bool? isMentionable = null,
			string? reason = null,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			Util.ThrowIfInvalidId(roleId, nameof(roleId));

			string endpoint = $"guilds/{guildId}/roles/{{0}}";

			return await SendHttpRequest(HttpMethod.Patch, endpoint, string.Format(endpoint, roleId), Util.CreateJson(writer =>
			{
				if (name is not null) writer.WriteString("name", name);
				if (permissions is not null) writer.WriteString("permissions", permissions);
				if (colour is not null) writer.WriteNumber("color", (int)colour);
				if (isHoisted is not null) writer.WriteBoolean("hoist", (bool)isHoisted);
				if (isMentionable is not null) writer.WriteBoolean("mentionable", (bool)isMentionable);
			}), reason, cancellationToken);
		}

		/// <summary>
		/// Deletes a guild role. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="roleId">The ID of the role to delete.</param>
		/// <param name="reason">The reason for deleting the role.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="roleId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="roleId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> DeleteGuildRole(string guildId, string roleId, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			Util.ThrowIfInvalidId(roleId, nameof(roleId));

			string endpoint = $"guilds/{guildId}/roles/{{0}}";

			return await SendHttpRequest(HttpMethod.Delete, endpoint, string.Format(endpoint, roleId), null, reason, cancellationToken);
		}

		/// <summary>
		/// Adds a role to a user in a guild. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="userId">The ID of the user.</param>
		/// <param name="roleId">The ID of the role.</param>
		/// <param name="reason">The reason for adding the role.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="userId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="roleId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="roleId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> AddRole(string guildId, string userId, string roleId, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			Util.ThrowIfInvalidId(userId, nameof(userId));
			Util.ThrowIfInvalidId(roleId, nameof(roleId));

			string endpoint = $"guilds/{guildId}/members/{{0}}/roles/{{1}}";

			return await SendHttpRequest(HttpMethod.Put, endpoint, string.Format(endpoint, userId, roleId), null, reason, cancellationToken);
		}

		/// <summary>
		/// Removes a role from a user. Requires the <c>MANAGE_ROLES</c> permission.
		/// </summary>
		/// <param name="guildId">The ID of the guild.</param>
		/// <param name="userId">The ID of the user.</param>
		/// <param name="roleId">The ID of the role.</param>
		/// <param name="reason">The reason for removing the role.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="userId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="roleId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="roleId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> RemoveRole(string guildId, string userId, string roleId, string? reason = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(guildId, nameof(guildId));
			Util.ThrowIfInvalidId(userId, nameof(userId));
			Util.ThrowIfInvalidId(roleId, nameof(roleId));

			string endpoint = $"guilds/{guildId}/members/{{0}}/roles/{{1}}";

			return await SendHttpRequest(HttpMethod.Delete, endpoint, string.Format(endpoint, userId, roleId), null, reason, cancellationToken);
		}

		/// <param name="responseType">The response type to use.</param>
		/// <param name="content">The content of the message to send or update, depending on the <paramref name="responseType"/>.</param>
		/// <param name="isEphemeral">If <see langword="true"/>, the message will be ephemeral.</param>
		/// <param name="embeds">Lists the message's embeds.</param>
		/// <param name="components">Lists the message components. Each subcollection represents one action row, and each element of that action row is a message component.</param>
		/// <param name="isTts"><see langword="true"/> if this should be a text to speech message; otherwise, <see langword="false"/>.</param>
		/// <inheritdoc cref="RespondToInteraction(string, string, ReadOnlyMemory{byte}, CancellationToken)"/>
		protected async Task<DiscordRequestResult> RespondToInteraction(
			string interactionId,
			string interactionToken,
			InteractionResponseType responseType,
			string? content,
			bool isEphemeral = false,
			IEnumerable<IEmbed>? embeds = null,
			IEnumerable<IActionRowComponent>? components = null,
			bool disallowMentions = false,
			bool isTts = false,
			CancellationToken cancellationToken = default
		)
		{
			return await RespondToInteraction(interactionId, interactionToken, Util.CreateJson(writer =>
			{
				writer.WriteNumber("type", (int)responseType);

				writer.WriteStartObject("data");
				WriteMessageJson(writer, content, isEphemeral ? MessageFlags.Ephemeral : MessageFlags.None, null, embeds, components, disallowMentions, isTts);
				writer.WriteEndObject();
			}), cancellationToken);
		}

		/// <remarks>
		/// <para>Some response types require a message to be sent, in which case, use the method <see cref="RespondToInteraction(string, string, InteractionResponseType, string?, bool, IEnumerable{IEmbed}?, IEnumerable{IActionRowComponent}?, bool, bool, CancellationToken)"/> instead.</para>
		/// </remarks>
		/// <param name="responseType">The response type to use.</param>
		/// <inheritdoc cref="RespondToInteraction(string, string, ReadOnlyMemory{byte}, CancellationToken)"/>
		protected async Task<DiscordRequestResult> RespondToInteraction(string interactionId, string interactionToken, InteractionResponseType responseType, CancellationToken cancellationToken = default)
		{
			return await RespondToInteraction(interactionId, interactionToken, Util.CreateJson(writer =>
			{
				writer.WriteNumber("type", (int)responseType);
			}), cancellationToken);
		}

		/// <summary>
		/// Responds to an interaction with the specified token.
		/// </summary>
		/// <param name="interactionId">The ID of the interaction.</param>
		/// <param name="interactionToken">The token of the interaction.</param>
		/// <param name="content">The HTTP content to send to Discord.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <exception cref="ArgumentNullException"><paramref name="interactionToken"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="interactionId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="interactionId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="interactionToken"/> is not a valid token.</exception>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		private Task<DiscordRequestResult> RespondToInteraction(string interactionId, string interactionToken, ReadOnlyMemory<byte> content, CancellationToken cancellationToken = default)
		{
			if (interactionToken is null) throw new ArgumentNullException(nameof(interactionToken));
			Util.ThrowIfInvalidId(interactionId, nameof(interactionId));
			if (interactionToken.Length == 0) throw new ArgumentException("Interaction token is empty.", nameof(interactionToken));

			const string endpoint = "interactions/{0}/{1}/callback";

			// this is not awaited because this method will be awaited in the other two overloads anyway
			return SendHttpRequest(HttpMethod.Post, endpoint, string.Format(endpoint, interactionId, interactionToken), content, null, cancellationToken);
		}

		/// <summary>
		/// Registers a command.
		/// </summary>
		/// <remarks>
		/// <para>You do not have to specify the <paramref name="userId"/> only if the client has connected at least once; then the client's known user ID will be used.</para>
		/// </remarks>
		/// <param name="guildId">The ID of the guild for which to register the command, or <see langword="null"/> to register a global command.</param>
		/// <param name="commandName">The 1-32 character name of the command. This should match <c>^[\w-]{1,32}$</c>.</param>
		/// <param name="commandDescription">The description of the command.</param>
		/// <param name="commandOptions">The parameters of the command.</param>
		/// <param name="hasDefaultPermission"><see langword="true"/> if the command is enabled by default when the app is added to a guild; otherwise, false.</param>
		/// <param name="userId">The ID of the application/client. If not specified, the cached ID will be used. Must be specified if this client has not been connected yet.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="userId"/> is not a valid ID.</exception>
		/// <exception cref="InvalidOperationException">You have not connected the client at least once, or specified a user ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="commandName"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="commandDescription"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		/// <exception cref="ArgumentException"><paramref name="commandOptions"/> contains a <see langword="null"/> value.</exception>
		protected async Task<DiscordRequestResult> RegisterApplicationCommand(
			string? guildId,
			string commandName,
			string commandDescription,
			IEnumerable<IApplicationCommandOption>? commandOptions = null,
			bool hasDefaultPermission = true,
			string? userId = null,
			CancellationToken cancellationToken = default
		)
		{
			Util.ThrowIfInvalidId(userId, nameof(userId), true);

			userId ??= UserId ?? throw new InvalidOperationException("You must either have connected the client at least once, or specify a user ID.");

			if (commandName is null) throw new ArgumentNullException(nameof(commandName));
			if (commandDescription is null) throw new ArgumentNullException(nameof(commandDescription));

			Util.ThrowIfInvalidId(guildId, nameof(guildId), true);

			string endpoint = guildId is null ? "applications/{0}/commands" : "applications/{0}/guilds/{1}/commands";

			return await SendHttpRequest(HttpMethod.Post, endpoint, string.Format(endpoint, userId, guildId), Util.CreateJson(writer =>
			{
				writer.WriteString("name", commandName);
				writer.WriteString("description", commandDescription);

				if (!hasDefaultPermission) writer.WriteBoolean("default_permission", false);

				// Recursively write options
				writer.WriteObjectArray("options", commandOptions, WriteOneApplicationCommandOption);

				void WriteOneApplicationCommandOption(IApplicationCommandOption option)
				{
					if (option is null) throw new ArgumentNullException(nameof(commandOptions));

					writer.WriteNumber("type", (int)option.Type);
					writer.WriteString("name", option.Name);
					writer.WriteString("description", option.Description);
					if (option.IsRequired) writer.WriteBoolean("required", option.IsRequired);

					writer.WriteObjectArray("choices", option.Choices, choice =>
					{
						writer.WriteString("name", choice.Name);

						OneOf<string, int> value = choice.Value;
						if (value.IsT0) writer.WriteString("value", value.AsT0);
						else if (value.IsT1) writer.WriteNumber("value", value.AsT1);
					});

					writer.WriteObjectArray("options", option.Options, WriteOneApplicationCommandOption);
				}
			}), cancellationToken: cancellationToken);
		}

		/// <summary>
		/// Fetches all commands of the application.
		/// </summary>
		/// <param name="guildId">The ID of the guild for which to get the command, or <see langword="null"/> to get only global commands.</param>
		/// <param name="userId">The ID of the application/client. If not specified, the cached ID will be used. Must be specified if this client has not been connected yet.</param>
		/// <param name="cancellationToken">The cancellation token to cancel the operation.</param>
		/// <returns>The result of the request. You must dispose this instance as soon as you have processed the result.</returns>
		/// <exception cref="ArgumentNullException"><paramref name="userId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="userId"/> is not a valid ID.</exception>
		/// <exception cref="InvalidOperationException">You have not connected the client at least once, or specified a user ID.</exception>
		/// <exception cref="ArgumentNullException"><paramref name="guildId"/> is <see langword="null"/>.</exception>
		/// <exception cref="ArgumentException"><paramref name="guildId"/> is not a valid ID.</exception>
		protected async Task<DiscordRequestResult> GetApplicationCommands(string? guildId, string? userId = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(userId, nameof(userId), true);

			userId ??= UserId ?? throw new InvalidOperationException("You must either have connected the client at least once, or specify a user ID.");

			Util.ThrowIfInvalidId(guildId, nameof(guildId), true);

			string endpoint = guildId is null ? "applications/{0}/commands" : "applications/{0}/guilds/{1}/commands";

			return await SendHttpRequest(HttpMethod.Get, endpoint, string.Format(endpoint, userId, guildId), cancellationToken: cancellationToken);
		}

		protected async Task<DiscordRequestResult> DeleteApplicationCommand(string? guildId, string commandId, string? userId = null, CancellationToken cancellationToken = default)
		{
			Util.ThrowIfInvalidId(userId, nameof(userId), true);

			userId ??= UserId ?? throw new InvalidOperationException("You must either have connected the client at least once, or specify a user ID.");

			Util.ThrowIfInvalidId(guildId, nameof(guildId), true);
			Util.ThrowIfInvalidId(commandId, nameof(commandId), true);

			string endpoint = guildId is null ? "applications/{0}/commands/{2}" :"applications/{0}/guilds/{1}/commands/{2}";

			return await SendHttpRequest(HttpMethod.Delete, endpoint, string.Format(endpoint, userId, guildId, commandId), cancellationToken: cancellationToken);
		}

		// Used in various HTTP requests to write message data
		private static void WriteMessageJson(Utf8JsonWriter writer, string? content, MessageFlags? flags, string? referencedMessageId, IEnumerable<IEmbed>? embeds, IEnumerable<IActionRowComponent>? components, bool disallowMentions = false, bool isTts = false)
		{
			// Create message JSON object
			if (content is not null)
			{
				writer.WriteString("content", content);
			}
			if (flags is not null)
			{
				writer.WriteNumber("flags", (int)flags);
			}
			if (referencedMessageId is not null)
			{
				writer.WriteString("message_reference", referencedMessageId);
			}
			if (isTts)
			{
				writer.WriteBoolean("tts", true);
			}

			// Write embeds
			writer.WriteObjectArray("embeds", embeds, embed =>
			{
				writer.WriteString("type", "rich");
				if (embed.Title is not null) writer.WriteString("title", embed.Title);
				if (embed.Description is not null) writer.WriteString("description", embed.Description);
				if (embed.Url is not null) writer.WriteString("url", embed.Url);
				if (embed.Timestamp is not null) writer.WriteString("timestamp", ((DateTime)embed.Timestamp).ToString("o"));
				if (embed.Colour != 0) writer.WriteNumber("color", embed.Colour);

				if (embed.Author is not null)
				{
					writer.WriteStartObject();
					if (embed.Author.Name is not null) writer.WriteString("name", embed.Author.Name);
					if (embed.Author.Url is not null) writer.WriteString("url", embed.Author.Url);
					if (embed.Author.IconUrl is not null) writer.WriteString("icon_url", embed.Author.IconUrl);
					writer.WriteEndObject();
				}

				if (embed.ImageUrl is not null)
				{
					writer.WriteStartObject("image");
					writer.WriteString("url", embed.ImageUrl);
					writer.WriteEndObject();
				}

				if (embed.ThumbnailUrl is not null)
				{
					writer.WriteStartObject("thumbnail");
					writer.WriteString("url", embed.ThumbnailUrl);
					writer.WriteEndObject();
				}

				if (embed.Fields is not null)
				{
					writer.WriteObjectArray("fields", embed.Fields, field =>
					{
						writer.WriteString("name", field.Name);
						writer.WriteString("value", field.Value);
						if (field.IsInline) writer.WriteBoolean("inline", true);
					});
				}

				if (embed.Footer is not null)
				{
					writer.WriteStartObject("footer");
					writer.WriteString("text", embed.Footer.Text);
					if (embed.Footer.IconUrl is not null) writer.WriteString("icon_url", embed.Footer.IconUrl);
					writer.WriteEndObject();
				}
			});

			// Write components
			writer.WriteObjectArray("components", components, actionRow =>
			{
				writer.WriteNumber("type", 1);

				writer.WriteObjectArray("components", actionRow.Components, component => component.Switch(

					// Interaction button components
					component =>
					{
						writer.WriteNumber("type", 2);
						writer.WriteNumber("style", (int)component.Style);
						ILabeledElement.WriteToJson(component, writer);
						writer.WriteString("custom_id", component.CustomId);
						if (component.IsDisabled)
						{
							writer.WriteBoolean("disabled", true);
						}
					},

					// Link button components
					component =>
					{
						writer.WriteNumber("type", 2);
						writer.WriteNumber("style", (int)ButtonComponentStyle.Link);
						ILabeledElement.WriteToJson(component, writer);
						writer.WriteString("url", component.Url);
						if (component.IsDisabled)
						{
							writer.WriteBoolean("disabled", true);
						}
					},

					// Select menu components
					component =>
					{
						writer.WriteNumber("type", 3);
						writer.WriteString("custom_id", component.CustomId);
						writer.WriteObjectArray("options", component.Options, option =>
						{
							ILabeledElement.WriteToJson(option, writer);
							writer.WriteString("value", option.Value);
							if (option.Description is not null) writer.WriteString("description", option.Description);
							if (option.IsDefault) writer.WriteBoolean("default", true);
						});
						if (component.Placeholder is not null) writer.WriteString("placeholder", component.Placeholder);
						if (component.MinValues != 1) writer.WriteNumber("min_values", component.MinValues);
						if (component.MaxValues != 1) writer.WriteNumber("max_values", component.MaxValues);
						if (component.IsDisabled) writer.WriteBoolean("disabled", true);
					}
				));
			});

			if (disallowMentions)
			{
				writer.WriteStartObject("allowed_mentions");
				writer.WriteStartArray("parse");
				writer.WriteEndArray();
				writer.WriteEndObject();
			}
		}

		#endregion

		#region IDisposable implementation

		/// <summary>
		/// Abruptly disconnects this <see cref="DiscordClient"/> and disposes all managed resources.
		/// </summary>
		/// <remarks>
		/// <para>This method is idempotent.</para>
		/// </remarks>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		/// <summary>
		/// Abruptly disconnects this <see cref="DiscordClient"/> and optionally disposes all managed resources.
		/// </summary>
		/// <param name="disposing"><see langword="true"/> if this method has been called form <see cref="Dispose()"/>; <see langword="false"/> if this method has been called from the finalizer.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
			{
				Disconnect()?.Dispose();
				sendingTimer?.Dispose();
				heartbeatTimer?.Dispose();
				httpClient.Dispose();
			}
			else
			{
				webSocket = null;
			}

			sendingTimer = null;
			heartbeatTimer = null;
			SessionId = null;

			IsDisposed = true;
		}

		/// <summary>
		/// Finalizes a <see cref="DiscordClient"/>.
		/// </summary>
		~DiscordClient()
		{
			Dispose(false);
		}

		#endregion
	}
}
