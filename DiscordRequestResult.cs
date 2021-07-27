using System;
using System.Threading.Tasks;
using System.Net.Http;
using System.Text.Json;

namespace SimpleDiscord
{
	/// <summary>
	/// Represents the result of an HTTP request to Discord. It contains either an <see cref="System.Exception"/> or an <see cref="HttpRequestMessage"/>.
	/// </summary>
	/// <remarks>
	/// <para>This class can only be constructed </para>
	/// </remarks>
	public class DiscordRequestResult : IDisposable
	{
		// If both are null, then IsDisposed is true.
		// They can't both be assigned a value.
		Exception? exception;
		HttpResponseMessage? response;

		RatelimitBucket? bucket;

		internal DiscordRequestResult(Exception exception, RatelimitBucket? bucket = null)
		{
			if (exception is null) throw new ArgumentNullException(nameof(exception));
			this.exception = exception;
			this.bucket = bucket;
		}

		internal DiscordRequestResult(HttpResponseMessage response, RatelimitBucket bucket)
		{
			if (response is null) throw new ArgumentNullException(nameof(response));
			if (bucket is null) throw new ArgumentNullException(nameof(bucket));
			this.response = response;
			this.bucket = bucket;
		}

		/// <summary>
		/// Gets a value indicating whether the request was successful.
		/// </summary>
		/// <remarks>
		/// <para>If <see langword="true"/>, <see cref="Response"/> is a 2xx status code response and <see cref="Exception"/> is <see langword="null"/>.</para>
		/// </remarks>
		/// <value>
		/// <see langword="true"/> if the request was successful; <see langword="false"/> if either an exception occurred while the request was being made, or a non-2xx status code was received.
		/// </value>
		/// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
		public bool WasSuccessful
		{
			get
			{
				if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
				return response is not null && response.IsSuccessStatusCode;
			}
		}

		/// <summary>
		/// Gets the exception that occurred while the request was being made, or <see langword="null"/> if no exception occurred.
		/// </summary>
		/// <remarks>
		/// <para>This exception can be an <see cref="HttpRequestException"/> if the request failed due to an underlying issue such as network connectivity, DNS failure, server certificate validation or timeout. It can be a <see cref="TaskCanceledException"/> if the request failed due to timeout.</para>
		/// <para>If this is not <see langword="null"/>, then <see cref="WasSuccessful"/> is <see langword="false"/>.</para>
		/// </remarks>
		/// <value>
		/// The exception that occurred while the request was being made, or <see langword="null"/> if no exception occurred.
		/// </value>
		/// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
		public Exception? Exception
		{
			get
			{
				if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
				return exception;
			}
		}

		/// <summary>
		/// Gets the HTTP message received from Discord in response to the request, or <see langword="null"/> if none was received.
		/// </summary>
		/// <value>
		/// The HTTP message received from Discord in response to the request, or <see langword="null"/> if none was received.
		/// </value>
		/// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
		public HttpResponseMessage? Response
		{
			get
			{
				if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
				return response;
			}
		}

		/// <summary>
		/// Gets the ratelimit bucket that was used for this request. May be <see langword="null"/> for requests where an exception occurred.
		/// </summary>
		/// <value>
		/// The ratelimit bucket used for this request, or <see langword="null"/> only if <see cref="Exception"/> is not <see langword="null"/>.
		/// </value>
		public RatelimitBucket? RatelimitBucket
		{
			get
			{
				if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
				return bucket;
			}
		}

		// probably useless
		/*
		/// <summary>
		/// Parses the response message from Discord as a JSON document.
		/// </summary>
		/// <returns>A <see cref="JsonDocument"/> object representing the response message.</returns>
		/// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
		/// <exception cref="InvalidOperationException">No response was received from the Discord.</exception>
		/// <exception cref="JsonException">The response does not represent a valid single JSON value.</exception>
		public JsonDocument GetReponseAsJsonDocument()
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
			if (response is null) throw new InvalidOperationException("No response was received from the Discord HTTP API request.");

			return JsonDocument.Parse(response.Content.ReadAsStream());
		}

		/// <summary>
		/// Asynchronously parses the response message from Discord as a JSON document.
		/// </summary>
		/// <returns>A <see cref="JsonDocument"/> object representing the response message.</returns>
		/// <exception cref="ObjectDisposedException">This object has been disposed.</exception>
		/// <exception cref="InvalidOperationException">No response was received from the Discord.</exception>
		/// <exception cref="JsonException">The response does not represent a valid single JSON value.</exception>
		public async Task<JsonDocument> GetResponseAsJsonDocumentAsync()
		{
			if (IsDisposed) throw new ObjectDisposedException(GetType().FullName);
			if (response is null) throw new InvalidOperationException("No response was received from the Discord HTTP API request.");

			return await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync());
		}
		*/

		#region IDisposable implementation

		public bool IsDisposed => response is null && exception is null;

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (IsDisposed) return;

			if (disposing)
			{
				response?.Dispose();
			}

			response = null;
			exception = null;
		}

		~DiscordRequestResult()
		{
			Dispose(false);
		}

		#endregion
	}
}
