namespace SimpleDiscord
{
	// Of course with *very* helpful documentation comments.

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
		/// Unknown webhook service.
		/// </summary>
		UnknownWebhookService = 10016,

		/// <summary>
		/// Unknown session.
		/// </summary>
		UnknownSession = 10020,

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
		/// Unknown store directory layout.
		/// </summary>
		UnknownStoreDirectoryLayout = 10033,

		/// <summary>
		/// Unknown redistributable.
		/// </summary>
		UnknownRedistributable = 10036,

		/// <summary>
		/// Unknown gift code.
		/// </summary>
		UnknownGiftCode = 10038,

		/// <summary>
		/// Unknown stream.
		/// </summary>
		UnknownStream = 10049,

		/// <summary>
		/// Unknown premium server subscribe cooldown.
		/// </summary>
		UnknownPremiumServerSubscribeCooldown = 10050,

		/// <summary>
		/// Unknown guild template.
		/// </summary>
		UnknownGuildTemplate = 10057,

		/// <summary>
		/// Unknown discoverable server category.
		/// </summary>
		UnknownDiscoverableServerCategory = 10059,

		/// <summary>
		/// Unknown sticker.
		/// </summary>
		UnknownSticker = 10060,

		/// <summary>
		/// Unknown interaction.
		/// </summary>
		UnknownInteraction = 10062,

		/// <summary>
		/// Unknown application command.
		/// </summary>
		UnknownApplicationCommand = 10063,

		/// <summary>
		/// Unknown application command permissions.
		/// </summary>
		UnknownApplicationCommandPermissions = 10066,

		/// <summary>
		/// Unknown stage instance.
		/// </summary>
		UnknownStageInstance = 10067,

		/// <summary>
		/// Unknown guild member verification form.
		/// </summary>
		UnknownGuildMemberVerificationForm = 10068,

		/// <summary>
		/// Unknown guild welcome screen.
		/// </summary>
		UnknownGuildWelcomeScreen = 10069,

		/// <summary>
		/// Unknown guild scheduled event.
		/// </summary>
		UnknownGuildScheduledEvent = 10070,

		/// <summary>
		/// Unknown guild scheduled event user.
		/// </summary>
		UnknownGuildScheduledEventUser = 10071,

		/// <summary>
		/// Bots cannot use this endpoint.
		/// </summary>
		CannotUseBot = 20001,

		/// <summary>
		/// Only bots can use this endpoint.
		/// </summary>
		MustUseBot = 20002,

		/// <summary>
		/// Explicit content cannot be sent to the desired recipient(s).
		/// </summary>
		CannotSendExplicitContent = 20009,

		/// <summary>
		/// You are not authorized to perform this action on this application.
		/// </summary>
		UnauthorizedAction = 20012,

		/// <summary>
		/// This action cannot be performed due to slowmode rate limit.
		/// </summary>
		SlowmodeRatelimit = 20016,

		/// <summary>
		/// Only the owner of this account can perform this action.
		/// </summary>
		MustBeOwner = 20018,

		/// <summary>
		/// This message cannot be edited due to announcement rate limits.
		/// </summary>
		MessageEditRatelimit = 20022,

		/// <summary>
		/// The channel you are writing has hit the write rate limit.
		/// </summary>
		ChannelRatelimit = 20028,

		/// <summary>
		/// Your stage topic, server name, server description, or channel names contain words that are not allowed.
		/// </summary>
		DisallowedWords = 20031,

		/// <summary>
		/// Guild premium subscription level too low.
		/// </summary>
		GuildPremiumSubscriptionLevel = 20035,

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
		/// Maximum number of recipients reached (10).
		/// </summary>
		RecipientsLimit = 30004,

		/// <summary>
		/// Maximum number of guild roles reached (250).
		/// </summary>
		GuildRoleLimit = 30005,

		/// <summary>
		/// Maximum number of webhooks reached (10).
		/// </summary>
		WebhookLimit = 30007,

		/// <summary>
		/// Maximum number of emojis reached.
		/// </summary>
		EmojiLimit = 30008,

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
		/// Maximum number of animated emojis reached.
		/// </summary>
		AnimatedEmojiLimit = 30018,

		/// <summary>
		/// Maximum number of server members reached.
		/// </summary>
		ServerMemberLimit = 30019,

		/// <summary>
		/// Maximum number of server categories has been reached (5).
		/// </summary>
		ServerCategoryLimit = 30030,

		/// <summary>
		/// Guild already has a template.
		/// </summary>
		GuildTemplateLimit = 30031,

		/// <summary>
		/// Maximum number of thread participants has been reached.
		/// </summary>
		ThreadParticipantLimit = 30033,

		/// <summary>
		/// Maximum number of bans for non-guild members have been exceeded.
		/// </summary>
		NonGuildMemberBanLimit = 30035,

		/// <summary>
		/// Maximum number of bans fetches has been reached.
		/// </summary>
		BansFetchLimit = 30037,

		/// <summary>
		/// Maximum number of stickers reached.
		/// </summary>
		StickerLimit = 30039,

		/// <summary>
		/// Maximum number of prune requests has been reached. Try again later.
		/// </summary>
		PruneRequestLimit = 30040,

		/// <summary>
		/// Maximum number of guild widget settings updates has been reached. Try again later.
		/// </summary>
		GuildWidgedSettingsUpdateLimit = 30042,

		/// <summary>
		/// Unauthorized. Provide a valid token and try again.
		/// </summary>
		Unauthorized = 40001,

		/// <summary>
		/// You need to verify your account in order to perform this action.
		/// </summary>
		UnverifiedAccount = 40002,

		/// <summary>
		/// You are opening direct messages too fast.
		/// </summary>
		DirectMessageRatelimit = 40003,

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
		/// An application command with that name already exists.
		/// </summary>
		DuplicateApplicationCommandName = 40041,

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
		/// Missing required OAuth2 scope.
		/// </summary>
		MissingOAuthScope = 50026,

		/// <summary>
		/// Invalid webhook token provided.
		/// </summary>
		InvalidWebhookToken = 50027,

		/// <summary>
		/// Invalid role.
		/// </summary>
		InvalidRole = 50028,

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
		/// File uploaded exceeds the maximum size.
		/// </summary>
		FileTooLarge = 50045,

		/// <summary>
		/// Invalid file uploaded.
		/// </summary>
		InvalidFile = 50046,

		/// <summary>
		/// Cannot self-redeem this gift.
		/// </summary>
		CannotSelfRedeem = 50054,

		/// <summary>
		/// Payment source required to redeem gift.
		/// </summary>
		PaymentSourceRequired = 50070,

		/// <summary>
		/// Cannot delete a channel required for Community guilds.
		/// </summary>
		CannotDeleteChannel = 50074,

		/// <summary>
		/// Invalid sticker sent.
		/// </summary>
		InvalidSticker = 50081,

		/// <summary>
		/// Tried to perform an operation on an archived thread, such as editing a message or adding a user to the thread.
		/// </summary>
		ThreadIsArchived = 50083,

		/// <summary>
		/// Invalid thread notification settings.
		/// </summary>
		InvalidThreadNotificationSettings = 50084,

		/// <summary>
		/// <c>before</c> value is earlier than the thread creation date.
		/// </summary>
		BeforeTooEarly = 50085,

		/// <summary>
		/// This server is not available in your location.
		/// </summary>
		ServerNotAvailable = 50095,

		/// <summary>
		/// This server needs monetization enabled in order to perform this action.
		/// </summary>
		MonetisationNotEnabled = 50097,

		/// <summary>
		/// Server needs more boosts.
		/// </summary>
		InsufficientBoosts = 50101,

		/// <summary>
		/// Two factor is required for this operation.
		/// </summary>
		TwoFactorAuthenitactionRequired = 60003,

		/// <summary>
		/// No users with DiscordTag exist.
		/// </summary>
		InvalidDiscordTag = 80004,

		/// <summary>
		/// Reaction was blocked.
		/// </summary>
		ReactionBlocked = 90001,

		/// <summary>
		/// API resource is currently overloaded. Try again a little later.
		/// </summary>
		ApiOverloaded = 130000,

		/// <summary>
		/// The stage is already open.
		/// </summary>
		StageAlreadyOpen = 150006,

		/// <summary>
		/// Cannot reply without permission to read message history.
		/// </summary>
		MissingReadMessageHistoryPermission = 160002,

		/// <summary>
		/// A thread has already been created for this message.
		/// </summary>
		ThreadAlreadyCreated = 160004,

		/// <summary>
		/// Thread is locked.
		/// </summary>
		ThreadBlocked = 160005,

		/// <summary>
		/// Maximum number of active threads reached.
		/// </summary>
		ActiveThreadLimit = 160006,

		/// <summary>
		/// Maximum number of active announcement threads reached.
		/// </summary>
		ActiveAnnouncementThreadLimit = 160007,

		/// <summary>
		/// Invalid JSON for uploaded Lottie file.
		/// </summary>
		InvalidLottieFileJson = 170001,

		/// <summary>
		/// Uploaded Lotties cannot contain rasterized images such as PNG or JPEG.
		/// </summary>
		InvalidLottieFileType = 170002,

		/// <summary>
		/// Sticker maximum framerate exceeded.
		/// </summary>
		StickerFramerateLimit = 170003,

		/// <summary>
		/// Sticker frame count exceeds maximum of 1000 frames.
		/// </summary>
		StickerFrameCountLimit = 170004,

		/// <summary>
		/// Lottie animation maximum dimensions exceeded.
		/// </summary>
		LottieAnimationDimensionsLimit = 170005,

		/// <summary>
		/// Sticker frame rate is either too small or too large.
		/// </summary>
		StickerFramerate = 170006,

		/// <summary>
		/// Sticker animation duration exceeds maximum of 5 seconds.
		/// </summary>
		StickerAnimationDurationLimit = 170007
	}
}
