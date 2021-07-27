namespace SimpleDiscord
{
	/// <summary>
	/// This error code is sometimes returned from an HTTP request along with an HTTP status code.
	/// </summary>
	/// <seealso href="https://discord.com/developers/docs/topics/opcodes-and-status-codes#json-json-error-codes"/>
	public enum JsonErrorCode
	{
		/// <summary>
		/// General error (such as a malformed request body, amongst other things).
		/// </summary>
		GeneralError = 0,

		/// <summary>
		/// Unknown account.
		/// </summary>
		UnknownAccount = 10001,

		/// <summary>
		/// Unknown application.
		/// </summary>
		UnknownApplication = 10002,

		/// <summary>
		/// Unknown channel.
		/// </summary>
		UnknownChannel = 10003,

		/// <summary>
		/// Unknown guild.
		/// </summary>
		UnknownGuild = 10004,

		/// <summary>
		/// Unknown integration.
		/// </summary>
		UnknownIntegration = 10005,

		/// <summary>
		/// Unknown invite.
		/// </summary>
		UnknownInvite = 10006,

		/// <summary>
		/// Unknown member.
		/// </summary>
		UnknownMember = 10007,

		/// <summary>
		/// Unknown message.
		/// </summary>
		UnknownMessage = 10008,

		/// <summary>
		/// Unknown permission overwrite.
		/// </summary>
		UnknownPermissionOverwrite = 10009,

		/// <summary>
		/// Unknown provider.
		/// </summary>
		UnknownProvider = 10010,

		/// <summary>
		/// Unknown role.
		/// </summary>
		UnknownRole = 10011,

		/// <summary>
		/// Unknown token.
		/// </summary>
		UnknownToken = 10012,

		/// <summary>
		/// Unknown user.
		/// </summary>
		UnknownUser = 10013,

		/// <summary>
		/// Unknown emoji.
		/// </summary>
		UnknownEmoji = 10014,

		/// <summary>
		/// Unknown webhook.
		/// </summary>
		UnknownWebhook = 10015,

		/// <summary>
		/// Unknown ban.
		/// </summary>
		UnknownBan = 10026,

		/// <summary>
		/// Unknown SKU.
		/// </summary>
		UnknownSku = 10027,

		/// <summary>
		/// Unknown Store Listing.
		/// </summary>
		UnknownStoreListing = 10028,

		/// <summary>
		/// Unknown entitlement.
		/// </summary>
		UnknownEntitlement = 10029,

		/// <summary>
		/// Unknown build.
		/// </summary>
		UnknownBuild = 10030,

		/// <summary>
		/// Unknown lobby.
		/// </summary>
		UnknownLobby = 10031,

		/// <summary>
		/// Unknown branch.
		/// </summary>
		UnknownBranch = 10032,

		/// <summary>
		/// Unknown redistributable.
		/// </summary>
		UnknownRedistributable = 10036,

		/// <summary>
		/// Unknown guild template.
		/// </summary>
		UnknownGuildTemplate = 10057,

		/// <summary>
		/// Unknown application command.
		/// </summary>
		UnknownApplicationCommand = 10063,

		/// <summary>
		/// Bots cannot use this endpoint.
		/// </summary>
		CannotUseBot = 20001,

		/// <summary>
		/// Only bots can use this endpoint.
		/// </summary>
		MustUseBot = 20002,

		/// <summary>
		/// This message cannot be edited due to announcement rate limits.
		/// </summary>
		MessageEditRatelimit = 20022,

		/// <summary>
		/// The channel you are writing has hit the write rate limit.
		/// </summary>
		ChannelRatelimit = 20028,

		/// <summary>
		/// Maximum number of guilds reached (100).
		/// </summary>
		GuildLimit = 30001,

		/// <summary>
		/// Maximum number of friends reached (1000).
		/// </summary>
		FriendsLimit = 30002,

		/// <summary>
		/// Maximum number of pins reached for the channel (50).
		/// </summary>
		PinsLimit = 30003,

		/// <summary>
		/// Maximum number of guild roles reached (250).
		/// </summary>
		GuildRoleLimit = 30005,

		/// <summary>
		/// Maximum number of webhooks reached (10).
		/// </summary>
		WebhookLimit = 30007,

		/// <summary>
		/// Maximum number of reactions reached (20).
		/// </summary>
		ReactionsLimit = 30010,

		/// <summary>
		/// Maximum number of guild channels reached (500).
		/// </summary>
		GuildChannelsLimit = 30013,

		/// <summary>
		/// Maximum number of attachments in a message reached (10).
		/// </summary>
		MessageAttachementLimit = 30015,

		/// <summary>
		/// Maximum number of invites reached (1000).
		/// </summary>
		MessageInviteLimit = 30016,

		/// <summary>
		/// Guild already has a template.
		/// </summary>
		GuildTemplateLimit = 30031,

		/// <summary>
		/// Unauthorized. Provide a valid token and try again.
		/// </summary>
		Unauthorized = 40001,

		/// <summary>
		/// You need to verify your account in order to perform this action.
		/// </summary>
		VerifyAccount = 40002,

		/// <summary>
		/// Request entity too large. Try sending something smaller in size.
		/// </summary>
		RequestEntityTooLarge = 40005,

		/// <summary>
		/// This feature has been temporarily disabled server-side.
		/// </summary>
		TemporarilyDisabled = 40006,

		/// <summary>
		/// The user is banned from this guild.
		/// </summary>
		UserBanned = 40007,

		/// <summary>
		/// This message has already been crossposted.
		/// </summary>
		AlreadyCrossposted = 40033,

		/// <summary>
		/// Missing access.
		/// </summary>
		MissingAccess = 50001,

		/// <summary>
		/// Invalid account type.
		/// </summary>
		InvalidAccountType = 50002,

		/// <summary>
		/// Cannot execute action on a DM channel.
		/// </summary>
		CannotExecuteDMAction = 50003,

		/// <summary>
		/// Guild widget disabled.
		/// </summary>
		GuildWidgetDisabled = 50004,

		/// <summary>
		/// Cannot edit a message authored by another user.
		/// </summary>
		CannotEditMessage = 50005,

		/// <summary>
		/// Cannot send an empty message.
		/// </summary>
		EmptyMessage = 50006,

		/// <summary>
		/// Cannot send messages to this user.
		/// </summary>
		CannotSendUserMessage = 50007,

		/// <summary>
		/// Cannot send messages in a voice channel.
		/// </summary>
		CannotSendVoiceChannel = 50008,

		/// <summary>
		/// Channel verification level is too high for you to gain access.
		/// </summary>
		ChannelVerificationFailed = 50009,

		/// <summary>
		/// OAuth2 application does not have a bot.
		/// </summary>
		ApplicationHasNoBot = 50010,

		/// <summary>
		/// OAuth2 application limit reached.
		/// </summary>
		ApplicationLimit = 50011,

		/// <summary>
		/// Invalid OAuth2 state.
		/// </summary>
		InvalidAuthState = 50012,

		/// <summary>
		/// You lack permissions to perform that action.
		/// </summary>
		MissingPermissions = 50013,

		/// <summary>
		/// Invalid authentication token provided.
		/// </summary>
		InvalidToken = 50014,

		/// <summary>
		/// Note was too long.
		/// </summary>
		NoteTooLong = 50015,

		/// <summary>
		/// Provided too few or too many messages to delete. Must provide at least 2 and fewer than 100 messages to delete.
		/// </summary>
		WrongMessageDeleteCount = 50016,

		/// <summary>
		/// A message can only be pinned to the channel it was sent in.
		/// </summary>
		WrongPinChannel = 50019,

		/// <summary>
		/// Invite code was either invalid or taken.
		/// </summary>
		InvalidInviteCode = 50020,

		/// <summary>
		/// Cannot execute action on a system message.
		/// </summary>
		CannotExecuteSystemMessageAction = 50021,

		/// <summary>
		/// Cannot execute action on this channel type.
		/// </summary>
		CannotExecuteChannelAction = 50024,

		/// <summary>
		/// Invalid OAuth2 access token provided.
		/// </summary>
		InvalidOAuthToken = 50025,

		/// <summary>
		/// Invalid webhook token provided.
		/// </summary>
		InvalidWebhookToken = 50027,

		/// <summary>
		/// "Invalid Recipient(s)".
		/// </summary>
		InvalidRecipients = 50033,

		/// <summary>
		/// A message provided was too old to bulk delete.
		/// </summary>
		MessageTooOld = 50034,

		/// <summary>
		/// Invalid form body (returned for both <c>application/json</c> and <c>multipart/form-data</c> bodies), or invalid <c>Content-Type</c> provided.
		/// </summary>
		InvalidFormBody = 50035,

		/// <summary>
		/// An invite was accepted to a guild the application's bot is not in.
		/// </summary>
		AcceptedInvalidInvite = 50036,

		/// <summary>
		/// Invalid API version provided.
		/// </summary>
		InvalidApiVersion = 50041,

		/// <summary>
		/// Cannot delete a channel required for Community guilds.
		/// </summary>
		CannotDeleteChannel = 50074,

		/// <summary>
		/// Invalid sticker sent.
		/// </summary>
		InvalidSticker = 50081,

		/// <summary>
		/// Reaction was blocked.
		/// </summary>
		ReactionBlocked = 90001,

		/// <summary>
		/// API resource is currently overloaded. Try again a little later.
		/// </summary>
		ApiOverloaded = 130000
	}
}
