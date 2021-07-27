using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace SimpleDiscord
{
	/// <summary>
	/// Represents the ratelimit for one route of Discord's HTTP API.
	/// </summary>
	/// <remarks>
	/// <para>This class is used by </para>
	/// <para>Each bucket has three possible states:</para>
	/// <list type="bullet">
	/// <item>
	/// <term>Expired</term>
	/// <description>This is the initial state of the bucket. <see cref="IsActive"/> is <see langword="false"/>, and <see cref="firstRequest"/> is <see langword="null"/>. In this state, only one request is allowed to go through, and then the bucket switches to the second state.</description>
	/// </item>
	/// <item>
	/// <term>Awaiting first request</term>
	/// <description><see cref="IsActive"/> is <see langword="false"/>, and <see cref="firstRequest"/> is a task that, when completed, switches this bucket to another state. No requests are allowed to be made in this state; methods should await <see cref="firstRequest"/> before sending.</description>
	/// </item>
	/// <item>
	/// <term>Active</term>
	/// <description><see cref="IsActive"/> is <see langword="true"/>. Requests are only allowed to be made until <see cref="Remaining"/> is 0.</description>
	/// </item>
	/// </list>
	/// </remarks>
	/// <seealso href="https://discord.com/developers/docs/topics/rate-limits#header-format"/>
	public class RatelimitBucket
	{
		internal int limit = -1;
		internal int remaining;
		internal DateTime reset;

		// This task represents the first request that was sent to the endpoint that this bucket represents, since this bucket was last reset. Every other request should await this one, so that the ratelimits are known before more requests are sent.
		internal Task<DiscordRequestResult>? firstRequest;

		// If this is true, this bucket should no longer be used because another bucket is now being used for this ratelimit.
		internal bool isDuplicate = false;

		/// <summary>
		/// Gets the number of requests that can be made to this endpoint per time interval, or <c>-1</c>.
		/// </summary>
		/// <value>
		/// The number of requests that can be made to this endpoint per time interval. <c>-1</c> if that value is not known yet.
		/// </value>
		/// <remarks>
		/// <para>This number is zero if no request has been made yet to the endpoint that this bucket represents.</para>
		/// </remarks>
		public int Limit => limit;

		/// <summary>
		/// The number of remaining requests that can be made before the bucket resets.
		/// </summary>
		public int Remaining => remaining;

		/// <summary>
		/// Point in time at which the bucket resets.
		/// </summary>
		public DateTime Reset => reset;

		/// <summary>
		/// If <see langword="true"/>, this bucket still applies. If <see langword="false"/>, it has expired or never been used.
		/// </summary>
		public bool IsActive => reset > DateTime.UtcNow;

		/// <summary>
		/// Returns the amount of time until this bucket gets reset.
		/// </summary>
		public TimeSpan TimeLeft => reset - DateTime.UtcNow;

		internal const string idHeader = "x-ratelimit-limit";
		internal const string limitHeader = "x-ratelimit-limit";
		internal const string remainingHeader = "x-ratelimit-remaining";
		internal const string resetHeader = "x-ratelimit-reset";
	}
}
