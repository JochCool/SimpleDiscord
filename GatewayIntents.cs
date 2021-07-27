using System;

namespace SimpleDiscord
{
	/// <summary>
	/// Specifies which events a client wishes to subscribe to. See <see href="https://discord.com/developers/docs/topics/gateway#gateway-intents">the documentation</see> for an overview.
	/// </summary>
	/// <remarks>
	/// <para>Any events not defined in an intent are considered "passthrough" and will always be received regardless of the specified intents.</para>
	/// <para>Invalid or disallowed intents will cause the connection to fail.</para>
	/// </remarks>
	[Flags]
	public enum GatewayIntents
	{
		/// <summary>
		/// Indicates that the client does not wish to subscribe to any event.
		/// </summary>
		None = 0,

		/// <summary>
		/// Indicates all events related to guilds.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Guild Create</description>
		/// </item>
		/// <item>
		/// <description>Guild Update</description>
		/// </item>
		/// <item>
		/// <description>Guild Delete</description>
		/// </item>
		/// <item>
		/// <description>Guild Role Create</description>
		/// </item>
		/// <item>
		/// <description>Guild Role Delete</description>
		/// </item>
		/// <item>
		/// <description>Channel Create</description>
		/// </item>
		/// <item>
		/// <description>Channel Update</description>
		/// </item>
		/// <item>
		/// <description>Channel Delete</description>
		/// </item>
		/// <item>
		/// <description>Channel Pins Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		Guilds = 1 << 0,

		/// <summary>
		/// Indicates all events related to guild members.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Guild Member Add</description>
		/// </item>
		/// <item>
		/// <description>Guild Member Update</description>
		/// </item>
		/// <item>
		/// <description>Guild Member Remove</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildMembers = 1 << 1,

		/// <summary>
		/// Indicates all events related to guild bans.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Guild Ban Add</description>
		/// </item>
		/// <item>
		/// <description>Guild Ban Remove</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildBans = 1 << 2,

		/// <summary>
		/// Indicates the event related to guild emoji.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Guild Emoji Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildEmojis = 1 << 3,

		/// <summary>
		/// Indicates the event related to guild integrations.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Guild Integration Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildIntegrations = 1 << 4,

		/// <summary>
		/// Indicates the event related to guild webhooks.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Webhooks Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildWebhooks = 1 << 5,

		/// <summary>
		/// Indicates all events related to guild invites.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Invite Create</description>
		/// </item>
		/// <item>
		/// <description>Invite Delete</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildInvites = 1 << 6,

		/// <summary>
		/// Indicates the event related to guild voice states.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Voice State Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildVoiceStates = 1 << 7,

		/// <summary>
		/// Indicates the event related to guild presences.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Presence Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildPresences = 1 << 8,

		/// <summary>
		/// Indicates all events related to guild messages.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Message Create</description>
		/// </item>
		/// <item>
		/// <description>Message Update</description>
		/// </item>
		/// <item>
		/// <description>Message Delete</description>
		/// </item>
		/// <item>
		/// <description>Message Delete Bulk</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildMessages = 1 << 9,

		/// <summary>
		/// Indicates all events related to guild message reactions.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Message Reaction Add</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove All</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove Emoji</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildMessageReactions = 1 << 10,

		/// <summary>
		/// Indicates the event related to guild message typing.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Typing Start</description>
		/// </item>
		/// </list>
		/// </remarks>
		GuildMessageTyping = 1 << 11,

		/// <summary>
		/// Indicates all events related to direct messages.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Message Create</description>
		/// </item>
		/// <item>
		/// <description>Message Update</description>
		/// </item>
		/// <item>
		/// <description>Message Delete</description>
		/// </item>
		/// <item>
		/// <description>Channel Pins Update</description>
		/// </item>
		/// </list>
		/// </remarks>
		DirectMessages = 1 << 12,

		/// <summary>
		/// Indicates all events related to direct message reactions.
		/// </summary>
		/// <remarks>
		/// <para>The following events are included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Message Reaction Add</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove All</description>
		/// </item>
		/// <item>
		/// <description>Message Reaction Remove Emoji</description>
		/// </item>
		/// </list>
		/// </remarks>
		DirectMessageReactions = 1 << 13,

		/// <summary>
		/// Indicates the event related to direct message typing.
		/// </summary>
		/// <remarks>
		/// <para>The following event is included in this intent:</para>
		/// <list type="bullet">
		/// <item>
		/// <description>Typing Start</description>
		/// </item>
		/// </list>
		/// </remarks>
		DirectMessageTyping = 1 << 14
	}
}
