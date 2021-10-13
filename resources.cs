// This file contains definitions for certain Discord API resources, that are not worthy of having their own file.

using OneOf;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SimpleDiscord
{
	#region Channels and roles

	public interface IChannel
	{
		/// <summary>
		/// Gets the name of the channel. Maximum length is <see cref="MaxNameLength"/>
		/// </summary>
		/// <value>
		/// The name of the channel.
		/// </value>
		string Name { get; }

		/// <summary>
		/// Gets the type of channel.
		/// </summary>
		/// <value>
		/// The type of channel.
		/// </value>
		ChannelType Type { get; }

		/// <summary>
		/// Gets the channel topic. Maximum length is <see cref="MaxTopicLength"/>.
		/// </summary>
		/// <value>
		/// The topic of the channel, or <see langword="null"/> if the channel does not have a topic.
		/// </value>
		string? Topic { get; }

		/// <summary>
		/// Gets the ID fo the parent category for this channel, or the ID of the text channel in which this thread was created.
		/// </summary>
		/// <value>
		/// The <see cref="string"/> representation of the ID of the parent channel of this channel.
		/// </value>
		string? ParentId { get; }

		/// <summary>
		/// Gets the sorting position of this channel.
		/// </summary>
		/// <value>
		/// The sorting position of this channel, with 0 being at the bottom.
		/// </value>
		int Position { get; }

		/// <summary>
		/// Gets the explicit permission overwrites for members and roles in this channel.
		/// </summary>
		/// <value>
		/// The permission overwrites of this channel.
		/// </value>
		IEnumerable<IPermissionOverwrite> PermissionOverwrites { get; }

		/// <summary>
		/// Gets a value indicating whether this channel is NSFW.
		/// </summary>
		/// <value>
		/// <see langword="true"/> is this channel is NSFW; otherwise, <see langword="false"/>.
		/// </value>
		bool IsNsfw { get; }

		/// <summary>
		/// Represents the maximum number of characters allowed by Discord in the <see cref="Name"/> field.
		/// </summary>
		public const int MaxNameLength = 100;

		/// <summary>
		/// Represents the maximum number of characters allowed by Discord in the <see cref="Topic"/> field.
		/// </summary>
		public const int MaxTopicLength = 1024;

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		public static readonly JsonEncodedText TopicProperty = JsonEncodedText.Encode("topic");
		public static readonly JsonEncodedText ParentIdProperty = JsonEncodedText.Encode("parent_id");
		public static readonly JsonEncodedText PositionProperty = JsonEncodedText.Encode("position");
		public static readonly JsonEncodedText PermissionOverwritesProperty = JsonEncodedText.Encode("permission_overwrites");
		public static readonly JsonEncodedText IsNsfwProperty = JsonEncodedText.Encode("nsfw");

		internal static void WriteToJson(IChannel channel, Utf8JsonWriter writer)
		{
			WriteToJson(writer, channel.Name, channel.Type, channel.Topic, channel.ParentId, channel.Position, channel.PermissionOverwrites, channel.IsNsfw);
		}

		internal static void WriteToJson(
			Utf8JsonWriter writer,
			string name,
			ChannelType type,
			string? topic = null,
			string? parentId = null,
			int position = 0,
			IEnumerable<IPermissionOverwrite>? permissionOverwrites = null,
			bool isNsfw = false
		)
		{
			writer.WriteString(NameProperty, name);
			writer.WriteNumber(TypeProperty, (int)type);
			if (topic is not null) writer.WriteString(TopicProperty, topic);
			if (parentId is not null) writer.WriteString(ParentIdProperty, parentId);
			if (position != 0) writer.WriteNumber(PositionProperty, position);
			if (isNsfw) writer.WriteBoolean(IsNsfwProperty, true);
			writer.WriteObjectArray(PermissionOverwritesProperty, permissionOverwrites, IPermissionOverwrite.WriteToJson);
		}
	}

	/// <summary>
	/// Represents the type of a channel.
	/// </summary>
	public enum ChannelType
	{
		/// <summary>
		/// Indicates a text channel within a guild.
		/// </summary>
		GuildText = 0,

		/// <summary>
		/// Indicates a direct message between users.
		/// </summary>
		Dm = 1,

		/// <summary>
		/// Indicates voice channel within a guild.
		/// </summary>
		GuildVoice = 2,

		/// <summary>
		/// Indicates a direct message between multiple users.
		/// </summary>
		GroupDm = 3,

		/// <summary>
		/// Indicates an <see href="https://support.discord.com/hc/en-us/articles/115001580171-Channel-Categories-101">organizational category</see> that contains up to 50 channels.
		/// </summary>
		GuildCategory = 4,

		/// <summary>
		/// Indicates a channel that <see href="https://support.discord.com/hc/en-us/articles/360032008192">users can follow and crosspost into their own server</see>.
		/// </summary>
		GuildNews = 5,

		/// <summary>
		/// Indicates a channel in which game developers can <see href="https://discord.com/developers/docs/game-and-server-management/special-channels">sell their game on Discord</see>.
		/// </summary>
		GuildStore = 6,

		/// <summary>
		/// Indicates a temporary sub-channel within a <see cref="GuildNews"/> channel.
		/// </summary>
		/// <remarks>
		/// Only available in API v9.
		/// </remarks>
		GuildNewsThread = 10,

		/// <summary>
		/// Indicates a temporary sub-channel within a <see cref="GuildText"/> channel.
		/// </summary>
		/// <remarks>
		/// Only available in API v9.
		/// </remarks>
		GuildPublicThread = 11,

		/// <summary>
		/// Indicates a temporary sub-channel within a <see cref="GuildText"/> channel that is only viewable by those invited and those with the MANAGE_THREADS permission.
		/// </summary>
		/// <remarks>
		/// Only available in API v9.
		/// </remarks>
		GuildPrivateThread = 12,

		/// <summary>
		/// Indicates a voice channel for <see href="https://support.discord.com/hc/en-us/articles/1500005513722">hosting events with an audience</see>.
		/// </summary>
		GuildStageVoice = 13
	}

	/// <summary>
	/// Represents the permission overwrites of one 
	/// </summary>
	public interface IPermissionOverwrite
	{
		/// <summary>
		/// Gets the ID of the role or user.
		/// </summary>
		/// <value>
		/// The ID of the role or user, depending on the value of <see cref="Type"/>.
		/// </value>
		string Id { get; }

		/// <summary>
		/// Gets the type of permission overwrite.
		/// </summary>
		/// <value>
		/// The type of permission overwrite.
		/// </value>
		PermissionOverwriteType Type { get; }

		/// <summary>
		/// Gets the bit set of granted permissions
		/// </summary>
		/// <value>
		/// The string representation of a <see cref="PermissionFlags"/> containing the granted permissions.
		/// </value>
		string Allow { get; }

		/// <summary>
		/// Gets the bit set of denied permissions
		/// </summary>
		/// <value>
		/// The string representation of a <see cref="PermissionFlags"/> containing the denied permissions.
		/// </value>
		string Deny { get; }

		public static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode("id");
		public static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		public static readonly JsonEncodedText AllowProperty = JsonEncodedText.Encode("allow");
		public static readonly JsonEncodedText DenyProperty = JsonEncodedText.Encode("deny");

		internal static void WriteToJson(IPermissionOverwrite overwrite, Utf8JsonWriter writer)
		{
			writer.WriteString(IdProperty, overwrite.Id);
			writer.WriteNumber(TypeProperty, (int)overwrite.Type);
			writer.WriteString(AllowProperty, overwrite.Allow);
			writer.WriteString(DenyProperty, overwrite.Deny);
		}
	}

	/// <summary>
	/// Represents the target of a permission overwrite.
	/// </summary>
	public enum PermissionOverwriteType
	{
		/// <summary>
		/// Indicates that a permission overwrite is for a role.
		/// </summary>
		Role,

		/// <summary>
		/// Indicates that a permission overwrite is for a guild member.
		/// </summary>
		Member
	}

	interface IDm
	{
		/// <summary>
		/// Gets the ID of the recipient of this DM.
		/// </summary>
		/// <value>
		/// A <see cref="string"/> representing the ID of the user that is the recipient of this DM.
		/// </value>
		string RecipientId { get; }

		public static readonly JsonEncodedText RecipientIdProperty = JsonEncodedText.Encode("recipient_id");
	}

	/// <summary>
	/// Represents a role on Discord.
	/// </summary>
	public interface IRole
	{
		/// <summary>
		/// Gets the name of this role.
		/// </summary>
		/// <value>
		/// The name of this role.
		/// </value>
		string? Name { get; }

		/// <summary>
		/// Gets the permissions enabled for this role.
		/// </summary>
		/// <value>
		/// The <see cref="string"/> representation of the bitwise value of the enabled permissions.
		/// </value>
		string? Permissions { get; }

		/// <summary>
		/// Gets the colour of this role.
		/// </summary>
		/// <value>
		/// The RGB colour value, or 0 for a role without colour.
		/// </value>
		int Colour { get; }

		/// <summary>
		/// Gets a value indicating whether this role is be pinned in the user listing.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this role should be pinned in the user listing; otherwise, <see langword="false"/>.
		/// </value>
		bool IsHoisted { get; }

		/// <summary>
		/// Gets a value indicating whether this role is mentionable.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this role should be mentionable; otherwise, <see langword="false"/>.
		/// </value>
		bool IsMentionable { get; }

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText PermissionsProperty = JsonEncodedText.Encode("permissions");
		public static readonly JsonEncodedText ColourProperty = JsonEncodedText.Encode("color");
		public static readonly JsonEncodedText IsHoistedProperty = JsonEncodedText.Encode("hoist");
		public static readonly JsonEncodedText IsMentionableProperty = JsonEncodedText.Encode("mentionable");

		internal static void WriteToJson(IRole role, Utf8JsonWriter writer)
		{
			WriteToJson(writer, role.Name, role.Permissions, role.Colour, role.IsHoisted, role.IsMentionable);
		}

		internal static void WriteToJson(Utf8JsonWriter writer, string? name = null, string? permissions = null, int? colour = null, bool? isHoisted = null, bool? isMentionable = null)
		{
			if (name is not null) writer.WriteString(NameProperty, name);
			if (permissions is not null) writer.WriteString(PermissionsProperty, permissions);
			if (colour is not null) writer.WriteNumber(ColourProperty, (int)colour);
			if (isHoisted is not null) writer.WriteBoolean(IsHoistedProperty, (bool)isHoisted);
			if (isMentionable is not null) writer.WriteBoolean(IsMentionableProperty, (bool)isMentionable);
		}
	}

	/// <summary>
	/// Represents a set of permissions.
	/// </summary>
	[Flags]
	public enum PermissionFlags : long
	{
		None = 0,
		CreateInstantInvite = 1L << 0,
		KickMembers = 1L << 1,
		BanMembers = 1L << 2,
		Administrator = 1L << 3,
		ManageChannels = 1L << 4,
		ManageGuild = 1L << 5,
		AddReactions = 1L << 6,
		ViewAuditLog = 1L << 7,
		PrioritySpeaker = 1L << 8,
		Stream = 1L << 9,
		ViewChannel = 1L << 10,
		SendMessages = 1L << 11,
		SendTtsMessages = 1L << 12,
		ManageMessages = 1L << 13,
		EmbedLinks = 1L << 14,
		AttachFiles = 1L << 15,
		ReadMessageHistory = 1L << 16,
		MentionEveryone = 1L << 17,
		UseExternalEmojis = 1L << 18,
		ViewGuildInsights = 1L << 19,
		Connect = 1L << 20,
		Speak = 1L << 21,
		MuteMembers = 1L << 22,
		DeafenMembers = 1L << 23,
		MoveMembers = 1L << 24,
		UseVoiceActivityDetection = 1L << 25,
		ChangeNickname = 1L << 26,
		ManageNicknames = 1L << 27,
		ManageRoles = 1L << 28,
		ManageWebhooks = 1L << 29,
		ManageEmojis = 1L << 30,
		UseSlashCommands = 1L << 31,
		RequestToSpeak = 1L << 32,
		ManageThreads = 1L << 34,
		UsePublicThreads = 1L << 35,
		UsePrivateThreads = 1L << 36
	}

	/// <summary>
	/// Represents the position of a role on Discord.
	/// </summary>
	public interface IRolePosition
	{
		/// <summary>
		/// Gets the ID of the role.
		/// </summary>
		/// <value>
		/// The ID of the role.
		/// </value>
		string Id { get; }

		/// <summary>
		/// Gets the position of the role.
		/// </summary>
		/// <value>
		/// The position of the role.
		/// </value>
		int Position { get; }

		public static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode("id");
		public static readonly JsonEncodedText PositionProperty = JsonEncodedText.Encode("position");

		internal static void WriteToJson(IRolePosition position, Utf8JsonWriter writer)
		{
			writer.WriteString(IdProperty, position.Id);
			writer.WriteNumber(PositionProperty, position.Position);
		}
	}

	/// <summary>
	/// Represents a message on Discord.
	/// </summary>
	/// <remarks>
	/// <para>This interface is incomplete and will likely be changed in the future.</para>
	/// </remarks>
	public interface IMessage
	{
		/// <summary>
		/// Gets the content of the message. Maximum length is <see cref="MaxContentLength"/>.
		/// </summary>
		/// <value>
		/// A <see cref="string"/> representing the content of the message, or <see langword="null"/> if this message has no content.
		/// </value>
		string? Content { get; }

		/// <summary>
		/// Gets the ID of the message that this message is a reply to.
		/// </summary>
		/// <value>
		/// The <see cref="string"/> representation of the ID of the replied message, or <see langword="null"/> if this message is not a reply.
		/// </value>
		string? ReferencedMessage { get; }

		/// <summary>
		/// Gets the embeds of this message.
		/// </summary>
		/// <value>
		/// The embeds of this message.
		/// </value>
		IEnumerable<IEmbed> Embeds { get; }

		/// <summary>
		/// Gets the components of this message.
		/// </summary>
		/// <value>
		/// The components of this message. Each subcollection represents one action row, and each element of that action row is a message component.
		/// </value>
		IEnumerable<IActionRowComponent> Components { get; }

		/// <summary>
		/// Gets the allowed mentions for this message.
		/// </summary>
		/// <value>
		/// The allowed mentions for this message, or <see langword="null"/> if all mentions are allowed.
		/// </value>
		IAllowedMentions? AllowedMentions { get; }

		/// <summary>
		/// Gets a value indicating whether this is a text to speech message.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this should be a text to speech message; otherwise, <see langword="false"/>.
		/// </value>
		bool IsTts { get; }

		/// <summary>
		/// Represents the maximum number of characters allowed by Discord in the <see cref="Content"/> field.
		/// </summary>
		public const int MaxContentLength = 2000;

		public static readonly JsonEncodedText ContentProperty = JsonEncodedText.Encode("content");
		public static readonly JsonEncodedText FlagsProperty = JsonEncodedText.Encode("flags");
		public static readonly JsonEncodedText ReferencedMessageProperty = JsonEncodedText.Encode("message_reference");
		public static readonly JsonEncodedText EmbedsProperty = JsonEncodedText.Encode("embeds");
		public static readonly JsonEncodedText ComponentsProperty = JsonEncodedText.Encode("components");
		public static readonly JsonEncodedText AllowedMentionsProperty = JsonEncodedText.Encode("allowed_mentions");
		public static readonly JsonEncodedText IsTtsProperty = JsonEncodedText.Encode("tts");

		internal static void WriteToJson(IMessage message, Utf8JsonWriter writer)
		{
			WriteToJson(writer, message.Content, null, message.ReferencedMessage, message.Embeds, message.Components, message.AllowedMentions, message.IsTts);
		}

		internal static void WriteToJson(
			Utf8JsonWriter writer,
			string? content = null,
			MessageFlags? flags = null,
			string? referencedMessageId = null,
			IEnumerable<IEmbed>? embeds = null,
			IEnumerable<IActionRowComponent>? components = null,
			IAllowedMentions? allowedMentions = null,
			bool isTts = false
		)
		{
			if (content is not null)
			{
				writer.WriteString(ContentProperty, content);
			}
			if (flags is not null)
			{
				writer.WriteNumber(FlagsProperty, (int)flags);
			}
			if (referencedMessageId is not null)
			{
				writer.WriteString(ReferencedMessageProperty, referencedMessageId);
			}
			if (isTts)
			{
				writer.WriteBoolean(IsTtsProperty, true);
			}

			writer.WriteObjectArray(EmbedsProperty, embeds, IEmbed.WriteToJson);
			writer.WriteObjectArray(ComponentsProperty, components, IActionRowComponent.WriteToJson);

			if (allowedMentions is not null)
			{
				writer.WriteStartObject(AllowedMentionsProperty);
				IAllowedMentions.WriteToJson(allowedMentions, writer);
				writer.WriteEndObject();
			}
		}
	}

	// https://discord.com/developers/docs/resources/channel#message-object-message-flags
	[Flags]
	public enum MessageFlags
	{
		None = 0,
		Crossposted = 1 << 0,
		IsCrosspost = 1 << 1,
		SuppressEmbeds = 1 << 2,
		SourceMessageDeleted = 1 << 3,
		Urgent = 1 << 4,
		HasThread = 1 << 5,
		Ephemeral = 1 << 6,
		Loading = 1 << 7
	}

	/// <summary>
	/// Controls what can be mentioned in a Discord message.
	/// </summary>
	public interface IAllowedMentions
	{
		/// <summary>
		/// Gets a list indicating which users that can be mentioned by this message.
		/// </summary>
		/// <value>
		/// The IDs of users that can be mentioned by this message; <see langword="null"/> if all users can be mentioned.
		/// </value>
		IEnumerable<string>? AllowedUserMentions { get; }

		/// <summary>
		/// Gets a list indicating which roles that can be mentioned by this message.
		/// </summary>
		/// <value>
		/// The IDs of roles that can be mentioned by this message; <see langword="null"/> if all roles can be mentioned.
		/// </value>
		IEnumerable<string>? AllowedRoleMentions { get; }

		/// <summary>
		/// Gets a value indicating whether <c>@everyone</c> and <c>@here</c> can be mentioned by this message.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if <c>@everyone</c> and <c>@here</c> can be mentioned by this message; othewise <see langword="false"/>.
		/// </value>
		bool AllowEveryoneMentions { get; }

		/// <summary>
		/// Gets a value indicating whether this message can mention the author of the replied message, if this message is a reply.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the author of the replied message can be mentioned by this message; othewise, <see langword="false"/>.
		/// </value>
		bool MentionRepliedUser { get; }

		public static readonly JsonEncodedText AllowedUserMentionsProperty = JsonEncodedText.Encode("users");
		public static readonly JsonEncodedText AllowedRoleMentionsProperty = JsonEncodedText.Encode("roles");
		public static readonly JsonEncodedText MentionRepliedUserProperty = JsonEncodedText.Encode("replied_user");
		public static readonly JsonEncodedText ParseProperty = JsonEncodedText.Encode("parse");
		public static readonly JsonEncodedText AllowAllUserMentionsValue = JsonEncodedText.Encode("users");
		public static readonly JsonEncodedText AllowAllRoleMentionsValue = JsonEncodedText.Encode("roles");
		public static readonly JsonEncodedText AllowEveryoneMentionsValue = JsonEncodedText.Encode("everyone");

		internal static void WriteToJson(IAllowedMentions allowedMentions, Utf8JsonWriter writer)
		{
			IEnumerable<string>? userMentions = allowedMentions.AllowedUserMentions;
			IEnumerable<string>? roleMentions = allowedMentions.AllowedRoleMentions;

			writer.WriteStartArray(ParseProperty);
			if (userMentions is null) writer.WriteStringValue(AllowAllUserMentionsValue);
			if (roleMentions is null) writer.WriteStringValue(AllowAllRoleMentionsValue);
			if (allowedMentions.AllowEveryoneMentions) writer.WriteStringValue(AllowEveryoneMentionsValue);
			writer.WriteEndArray();

			writer.WriteStringArray(AllowedUserMentionsProperty, userMentions);
			writer.WriteStringArray(AllowedRoleMentionsProperty, roleMentions);

			if (allowedMentions.MentionRepliedUser) writer.WriteBoolean(MentionRepliedUserProperty, true);
		}
	}

	#endregion

	#region Emoji

	/// <summary>
	/// Represents an emoji.
	/// </summary>
	/// <remarks>
	/// This interface is currently incomplete.
	/// </remarks>
	public interface IEmoji
	{
		/// <summary>
		/// Gets the ID of the emoji, if this is a custom emoji.
		/// </summary>
		/// <value>
		/// The snowflake ID of the custom emoji, or <see langword="null"/> if this is not a custom emoji.
		/// </value>
		public string? Id { get; }

		/// <summary>
		/// Gets the name of the emoji.
		/// </summary>
		/// <value>
		/// The name of the emoji. Can be <see langword="null"/> only in reaction emoji objects when custom emoji data is not available (for example, if it was deleted from the guild).
		/// </value>
		public string? Name { get; }

		/// <summary>
		/// Gets a value indicating whether this emoji is animated.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this emoji is animated; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsAnimated { get; }

		public static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode("id");
		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText IsAnimatedProperty = JsonEncodedText.Encode("animated");

		internal static void WriteToJson(IEmoji emoji, Utf8JsonWriter writer)
		{
			writer.WriteString(IdProperty, emoji.Id);
			writer.WriteString(NameProperty, emoji.Name);
			if (emoji.IsAnimated) writer.WriteBoolean(IsAnimatedProperty, true);
		}
	}

	#endregion

	#region Embeds

	/// <summary>
	/// Represents one embed in a message.
	/// </summary>
	/// <remarks>
	/// <para>In addition to per-property character limits, the combined character count of the <see cref="Title"/>, <see cref="Description"/>, <see cref="IEmbedField.Name"/>, <see cref="IEmbedField.Value"/>, <see cref="IEmbedFooter.Text"/> and <see cref="IEmbedAuthor.Name"/> properties must not exceed 6000 characters per embed.</para>
	/// <para>Leading and trailing whitespace is trimmed automatically by Discord and not included in the character count.</para>
	/// </remarks>
	public interface IEmbed
	{
		/// <summary>
		/// Gets the title of the embed.
		/// </summary>
		/// <remarks>
		/// <para>The title may include Markdown formatting.</para>
		/// </remarks>
		/// <value>
		/// The title of the embed; max 256 characters.
		/// </value>
		public string? Title { get; }

		/// <summary>
		/// Gets the description of the embed.
		/// </summary>
		/// <remarks>
		/// <para>The description may include Markdown formatting.</para>
		/// </remarks>
		/// <value>
		/// The description of the embed; max 4096 characters.
		/// </value>
		public string? Description { get; }

		/// <summary>
		/// Gets the URL of the embed.
		/// </summary>
		/// <value>
		/// The URL of the embed.
		/// </value>
		public string? Url { get; }

		/// <summary>
		/// Gets the timestamp of the embed.
		/// </summary>
		/// <value>
		/// The timestamp of the embed in UTC, or <see langword="null"/> to not display a timestamp.
		/// </value>
		public DateTime? Timestamp { get; }

		/// <summary>
		/// Gets the colour code of the embed.
		/// </summary>
		/// <value>
		/// The colour code of the embed, or 0 for no colour. Must be smaller than 16777216.
		/// </value>
		public int Colour { get; }

		/// <summary>
		/// Gets information about the footer of this embed.
		/// </summary>
		/// <value>
		/// An <see cref="IEmbedFooter"/> instance representing the footer, or <see cref="null"/> if there is no footer.
		/// </value>
		public IEmbedFooter? Footer { get; }

		/// <summary>
		/// Gets the URL of the image in this embed.
		/// </summary>
		/// <remarks>
		/// <para>Only supports HTTP(S) and attachments.</para>
		/// </remarks>
		/// <value>
		/// The string representation of the URL at which the image is located.
		/// </value>
		public string? ImageUrl { get; }

		/// <summary>
		/// Gets the URL of the thumbnail of this embed.
		/// </summary>
		/// <remarks>
		/// <para>Only supports HTTP(S) and attachments.</para>
		/// </remarks>
		/// <value>
		/// The string representation of the URL at which the thumbnail is located.
		/// </value>
		public string? ThumbnailUrl { get; }

		/// <summary>
		/// Gets information about the author of this embed.
		/// </summary>
		/// <value>
		/// An <see cref="IEmbedAuthor"/> instance representing the author, or <see langword="null"/> if there is none.
		/// </value>
		public IEmbedAuthor? Author { get; }

		/// <summary>
		/// Gets the fields of this embed.
		/// </summary>
		/// <value>
		/// A collection of this embed's fields; max 25 elements.
		/// </value>
		public IEnumerable<IEmbedField>? Fields { get; }

		internal static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		internal static readonly JsonEncodedText RichTypeValue = JsonEncodedText.Encode("rich");
		public static readonly JsonEncodedText TitleProperty = JsonEncodedText.Encode("title");
		public static readonly JsonEncodedText DescriptionProperty = JsonEncodedText.Encode("description");
		public static readonly JsonEncodedText UrlProperty = JsonEncodedText.Encode("url");
		public static readonly JsonEncodedText TimestampProperty = JsonEncodedText.Encode("timestamp");
		public static readonly JsonEncodedText ColourProperty = JsonEncodedText.Encode("color");
		public static readonly JsonEncodedText FooterProperty = JsonEncodedText.Encode("footer");
		public static readonly JsonEncodedText ImageProperty = JsonEncodedText.Encode("image");
		public static readonly JsonEncodedText ImageUrlProperty = JsonEncodedText.Encode("url");
		public static readonly JsonEncodedText ThumbnailProperty = JsonEncodedText.Encode("thumbnail");
		public static readonly JsonEncodedText ThumbnailUrlProperty = JsonEncodedText.Encode("url");
		public static readonly JsonEncodedText AuthorProperty = JsonEncodedText.Encode("author");
		public static readonly JsonEncodedText FieldsProperty = JsonEncodedText.Encode("fields");

		internal static void WriteToJson(IEmbed embed, Utf8JsonWriter writer)
		{
			WriteToJson(writer, embed.Title, embed.Description, embed.Url, embed.Timestamp, embed.Colour, embed.Author, embed.ImageUrl, embed.ThumbnailUrl, embed.Fields, embed.Footer);
		}

		internal static void WriteToJson(
			Utf8JsonWriter writer,
			string? title = null,
			string? description = null,
			string? url = null,
			DateTime? timestamp = null,
			int colour = 0,
			IEmbedAuthor? author = null,
			string? imageUrl = null,
			string? thumbnailUrl = null,
			IEnumerable<IEmbedField>? fields = null,
			IEmbedFooter? footer = null
		)
		{
			writer.WriteString(TypeProperty, RichTypeValue);

			if (title is not null) writer.WriteString(TitleProperty, title);
			if (description is not null) writer.WriteString(DescriptionProperty, description);
			if (url is not null) writer.WriteString(UrlProperty, url);
			if (timestamp is not null) writer.WriteString(TimestampProperty, ((DateTime)timestamp).ToString("o"));
			if (colour != 0) writer.WriteNumber(ColourProperty, colour);

			if (author is not null)
			{
				writer.WriteStartObject(AuthorProperty);
				IEmbedAuthor.WriteToJson(author, writer);
				writer.WriteEndObject();
			}

			if (imageUrl is not null)
			{
				writer.WriteStartObject(ImageProperty);
				writer.WriteString(ImageUrlProperty, imageUrl);
				writer.WriteEndObject();
			}

			if (thumbnailUrl is not null)
			{
				writer.WriteStartObject(ThumbnailProperty);
				writer.WriteString(ThumbnailUrlProperty, thumbnailUrl);
				writer.WriteEndObject();
			}

			if (fields is not null)
			{
				writer.WriteObjectArray(FieldsProperty, fields, IEmbedField.WriteToJson);
			}

			if (footer is not null)
			{
				writer.WriteStartObject(FooterProperty);
				IEmbedFooter.WriteToJson(footer, writer);
				writer.WriteEndObject();
			}
		}
	}

	public interface IHasIcon
	{
		/// <summary>
		/// Gets the URL of the icon to display.
		/// </summary>
		/// <remarks>
		/// <para>Only supports HTTP(S) and attachments.</para>
		/// </remarks>
		/// <value>
		/// The URL of the icon.
		/// </value>
		public string? IconUrl { get; }

		public static readonly JsonEncodedText IconUrlProperty = JsonEncodedText.Encode("icon_url");

		internal static void WriteToJson(IHasIcon icon, Utf8JsonWriter writer)
		{
			string? iconUrl = icon.IconUrl;
			if (iconUrl is not null)
			{
				writer.WriteString(IconUrlProperty, iconUrl);
			}
		}
	}

	/// <summary>
	/// Represents the author of an embed.
	/// </summary>
	public interface IEmbedAuthor : IHasIcon
	{
		/// <summary>
		/// Gets the name of the author.
		/// </summary>
		/// <value>
		/// The name of the author; max 256 characters.
		/// </value>
		public string? Name { get; }

		/// <summary>
		/// Gets the URL of the author.
		/// </summary>
		/// <value>
		/// The URL of the author.
		/// </value>
		public string? Url { get; }

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText UrlProperty = JsonEncodedText.Encode("url");

		internal static void WriteToJson(IEmbedAuthor author, Utf8JsonWriter writer)
		{
			string? name = author.Name;
			if (name is not null)
			{
				writer.WriteString(NameProperty, name);
			}
			string? url = author.Url;
			if (url is not null)
			{
				writer.WriteString(UrlProperty, url);
			}
			IHasIcon.WriteToJson(author, writer);
		}
	}

	/// <summary>
	/// Represents the footer of the embed.
	/// </summary>
	public interface IEmbedFooter : IHasIcon
	{
		/// <summary>
		/// Gets the text of the footer.
		/// </summary>
		/// <value>
		/// The text of the footer; max 2048 characters.
		/// </value>
		public string Text { get; }

		public static readonly JsonEncodedText TextProperty = JsonEncodedText.Encode("text");

		internal static void WriteToJson(IEmbedFooter footer, Utf8JsonWriter writer)
		{
			writer.WriteString(TextProperty, footer.Text);
			IHasIcon.WriteToJson(footer, writer);
		}
	}

	public interface IEmbedField
	{
		/// <summary>
		/// Gets the name of the field.
		/// </summary>
		/// <remarks>
		/// <para>The name may include Markdown formatting.</para>
		/// </remarks>
		/// <value>
		/// The name of the field (1-256 characters).
		/// </value>
		public string Name { get; }

		/// <summary>
		/// Gets the value of the field.
		/// </summary>
		/// <remarks>
		/// <para>The value may include Markdown formatting.</para>
		/// </remarks>
		/// <value>
		/// The value of the field (1-1024 characters).
		/// </value>
		public string Value { get; }

		/// <summary>
		/// Gets a value indicating whether or not this field should display inline.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this field should display inline; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsInline { get; }

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText ValueProperty = JsonEncodedText.Encode("value");
		public static readonly JsonEncodedText IsInlineProperty = JsonEncodedText.Encode("inline");

		internal static void WriteToJson(IEmbedField field, Utf8JsonWriter writer)
		{
			writer.WriteString(NameProperty, field.Name);
			writer.WriteString(ValueProperty, field.Value);
			if (field.IsInline) writer.WriteBoolean(IsInlineProperty, true);
		}
	}

	#endregion

	#region Message components

	/// <summary>
	/// Represents any component.
	/// </summary>
	/// <remarks>
	/// <para>This interface currently does not have any members.</para>
	/// </remarks>
	public interface IComponent
	{
		internal static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
	}

	/// <summary>
	/// Represents an action row component (component type 1).
	/// </summary>
	public interface IActionRowComponent : IComponent
	{
		/// <summary>
		/// Gets the components in this action row.
		/// </summary>
		IEnumerable<OneOf<IInteractionButtonComponent, ILinkButtonComponent, ISelectMenuComponent>> Components { get; }

		public static readonly JsonEncodedText ComponentsProperty = JsonEncodedText.Encode("components");

		internal static void WriteToJson(IActionRowComponent actionRow, Utf8JsonWriter writer)
		{
			writer.WriteNumber(TypeProperty, 1);

			writer.WriteObjectArray(ComponentsProperty, actionRow.Components, (component, writer) => component.Switch(
				component => IInteractionButtonComponent.WriteToJson(component, writer),
				component => ILinkButtonComponent.WriteToJson(component, writer),
				component => ISelectMenuComponent.WriteToJson(component, writer)
			));
		}
	}

	/// <summary>
	/// Represents any message component that can generate a message interaction (so any select menu or non-link button).
	/// </summary>
	public interface IInteractionComponent
	{
		/// <summary>
		/// Gets the custom identifier for the component.
		/// </summary>
		/// <value>
		/// A developer-defined identifier for the button; max 100 characters.
		/// </value>
		string CustomId { get; }

		public static readonly JsonEncodedText CustomIdProperty = JsonEncodedText.Encode("custom_id");

		internal static void WriteToJson(IInteractionComponent component, Utf8JsonWriter writer)
		{
			writer.WriteString(CustomIdProperty, component.CustomId);
		}
	}

	/// <summary>
	/// Represents any component that can be disabled, such as button or select menu components.
	/// </summary>
	public interface IDisableableComponent
	{
		/// <summary>
		/// Gets a value indicating whether the component is disabled.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the component is disabled; otherwise, <see langword="false"/>.
		/// </value>
		bool IsDisabled { get; }

		public static readonly JsonEncodedText IsDisabledProperty = JsonEncodedText.Encode("disabled");

		internal static void WriteToJson(IDisableableComponent component, Utf8JsonWriter writer)
		{
			if (component.IsDisabled)
			{
				writer.WriteBoolean(IsDisabledProperty, true);
			}
		}
	}

	/// <summary>
	/// Represents any UI element that has a label and optionally an accompanying emoji, such as buttons or select menu options.
	/// </summary>
	public interface ILabeledElement
	{
		/// <summary>
		/// Gets the UI element's label.
		/// </summary>
		/// <value>
		/// The text that appears on the UI element.
		/// </value>
		string Label { get; }

		/// <summary>
		/// Gets the emoji on this UI element.
		/// </summary>
		/// <value>
		/// A partial <see cref="IEmoji"/> object, which only needs to have the <see cref="IEmoji.Id"/>, <see cref="IEmoji.Name"/> and <see cref="IEmoji.IsAnimated"/> properties.
		/// </value>
		IEmoji? Emoji { get; }

		public static readonly JsonEncodedText LabelProperty = JsonEncodedText.Encode("label");
		public static readonly JsonEncodedText EmojiProperty = JsonEncodedText.Encode("emoji");

		internal static void WriteToJson(ILabeledElement element, Utf8JsonWriter writer)
		{
			writer.WriteString(LabelProperty, element.Label);

			IEmoji? emoji = element.Emoji;
			if (emoji is not null)
			{
				writer.WriteStartObject(EmojiProperty);
				IEmoji.WriteToJson(emoji, writer);
				writer.WriteEndObject();
			}
		}
	}

	/// <summary>
	/// Represents a button message component (component type 2).
	/// </summary>
	public interface IButtonComponent : IComponent, ILabeledElement, IDisableableComponent
	{
		internal static void WriteToJson(IButtonComponent component, Utf8JsonWriter writer)
		{
			writer.WriteNumber(TypeProperty, 2);
			ILabeledElement.WriteToJson(component, writer);
			IDisableableComponent.WriteToJson(component, writer);
		}
	}

	/// <summary>
	/// Represents a non-link button message component.
	/// </summary>
	public interface IInteractionButtonComponent : IButtonComponent, IInteractionComponent
	{
		/// <summary>
		/// Gets the style of the button.
		/// </summary>
		/// <value>
		/// Any valid <see cref="ButtonComponentStyle"/> value, other than <see cref="ButtonComponentStyle.Link"/> (for link button components, use <see cref="ILinkButtonComponent"/> instead).
		/// </value>
		ButtonComponentStyle Style { get; }

		public static readonly JsonEncodedText StyleProperty = JsonEncodedText.Encode("style");

		internal static void WriteToJson(IInteractionButtonComponent component, Utf8JsonWriter writer)
		{
			IButtonComponent.WriteToJson(component, writer);
			IInteractionComponent.WriteToJson(component, writer);
			writer.WriteNumber(StyleProperty, (int)component.Style);
		}
	}

	/// <summary>
	/// Represents the style of a <see cref="IButtonComponent"/>. Examples are shown in <see href="https://discord.com/developers/docs/interactions/message-components#button-object-button-styles">Discord's documentation</see>.
	/// </summary>
	public enum ButtonComponentStyle
	{
		/// <summary>
		/// Indicates a primary CTA (call to action) button; coloured blue in Discord's UI.
		/// </summary>
		Primary = 1,

		/// <summary>
		/// Indicates a secondary button; coloured grey in Discord's UI.
		/// </summary>
		Secondary = 2,

		/// <summary>
		/// Indicates a primary success button; coloured green in Discord's UI.
		/// </summary>
		Success = 3,

		/// <summary>
		/// Indicates a destruvtive button; coloured red in Discord's UI.
		/// </summary>
		Danger = 4,

		/// <summary>
		/// Represents a link button; coloured grey in the UI with an external link icon. Do not use this in an <see cref="IInteractionButtonComponent"/>; use an <see cref="ILinkButtonComponent"/> instead.
		/// </summary>
		Link = 5
	}

	/// <summary>
	/// Represents a component that opens an external link instead of generating an interaction.
	/// </summary>
	public interface ILinkButtonComponent : IButtonComponent
	{
		/// <summary>
		/// Gets the URL this link leads to.
		/// </summary>
		/// <value>
		/// The URL.
		/// </value>
		string Url { get; }

		public static readonly JsonEncodedText UrlProperty = JsonEncodedText.Encode("url");

		internal static void WriteToJson(ILinkButtonComponent component, Utf8JsonWriter writer)
		{
			IButtonComponent.WriteToJson(component, writer);
			writer.WriteNumber(IInteractionButtonComponent.StyleProperty, (int)ButtonComponentStyle.Link);
			writer.WriteString(UrlProperty, component.Url);
		}
	}

	/// <summary>
	/// Represents a select menu message component (component type 3).
	/// </summary>
	public interface ISelectMenuComponent : IComponent, IInteractionComponent, IDisableableComponent
	{
		/// <summary>
		/// Gets the possible choices in the select menu.
		/// </summary>
		/// <value>
		/// A collection of <see cref="ISelectMenuComponent"/> objects; max 25.
		/// </value>
		IEnumerable<ISelectMenuComponentOption> Options { get; }

		/// <summary>
		/// Gets a custom placeholder text if nothing is selected.
		/// </summary>
		/// <value>
		/// The custom placeholder; max 100 characters.
		/// </value>
		string? Placeholder { get; }

		/// <summary>
		/// Gets the minimum number of items that must be chosen.
		/// </summary>
		/// <value>
		/// The minimum number of items; default 1, min 0, max 25.
		/// </value>
		int MinValues { get; }

		/// <summary>
		/// Gets the maximum number of items that must be chosen.
		/// </summary>
		/// <value>
		/// The maximum number of items; default 1, max 25.
		/// </value>
		int MaxValues { get; }

		public static readonly JsonEncodedText OptionsProperty = JsonEncodedText.Encode("options");
		public static readonly JsonEncodedText PlaceholderProperty = JsonEncodedText.Encode("placeholder");
		public static readonly JsonEncodedText MinValuesProperty = JsonEncodedText.Encode("min_values");
		public static readonly JsonEncodedText MaxValuesProperty = JsonEncodedText.Encode("max_values");

		internal static void WriteToJson(ISelectMenuComponent component, Utf8JsonWriter writer)
		{
			writer.WriteNumber(TypeProperty, 3);
			IInteractionComponent.WriteToJson(component, writer);
			IDisableableComponent.WriteToJson(component, writer);
			writer.WriteObjectArray(OptionsProperty, component.Options, ISelectMenuComponentOption.WriteToJson);
			if (component.Placeholder is not null) writer.WriteString(PlaceholderProperty, component.Placeholder);
			if (component.MinValues != 1) writer.WriteNumber(MinValuesProperty, component.MinValues);
			if (component.MaxValues != 1) writer.WriteNumber(MaxValuesProperty, component.MaxValues);
		}
	}

	/// <summary>
	/// Represents an option in a <see cref="ISelectMenuComponent"/>.
	/// </summary>
	public interface ISelectMenuComponentOption : ILabeledElement
	{
		/// <summary>
		/// Gets the dev-define value of the option.
		/// </summary>
		/// <value>
		/// The value of the option; max 100 characters
		/// </value>
		string Value { get; }

		/// <summary>
		/// Gets an additional description of the option.
		/// </summary>
		/// <value>
		/// The description of this option; max 100 characters; or <see langword="null"/> if this option has no description.
		/// </value>
		string? Description { get; }

		/// <summary>
		/// Gets a value indicating whether this option is selected by default.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the option is selected by default; otherwise, <see langword="false"/>.
		/// </value>
		bool IsDefault { get; }

		public static readonly JsonEncodedText ValueProperty = JsonEncodedText.Encode("value");
		public static readonly JsonEncodedText DescriptionProperty = JsonEncodedText.Encode("description");
		public static readonly JsonEncodedText IsDefaultProperty = JsonEncodedText.Encode("default");

		internal static void WriteToJson(ISelectMenuComponentOption option, Utf8JsonWriter writer)
		{
			ILabeledElement.WriteToJson(option, writer);
			writer.WriteString(ValueProperty, option.Value);
			if (option.Description is not null) writer.WriteString(DescriptionProperty, option.Description);
			if (option.IsDefault) writer.WriteBoolean(IsDefaultProperty, true);
		}
	}

	#endregion

	#region Slash commands

	/// <summary>
	/// Represents any node in the command tree; including the root command.
	/// </summary>
	public interface IApplicationCommandNode
	{
		/// <summary>
		/// Gets the name of the command node.
		/// </summary>
		/// <value>
		/// The 1-32 character name of the command node. This should match <c>^[\w-]{1,32}$</c>.
		/// </value>
		string Name { get; }

		/// <summary>
		/// Gets the description of this command node.
		/// </summary>
		/// <value>
		/// The 1-100 character description of te command option.
		/// </value>
		string Description { get; }

		/// <summary>
		/// Gets the parameters for this command node, if applicable.
		/// </summary>
		/// <value>
		/// If this is a command, subcommand or subcommand group that has parameters, a collection of those parameters; otherwise, <see langword="null"/>.
		/// </value>
		IEnumerable<IApplicationCommandOption>? Options { get; }

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText DescriptionProperty = JsonEncodedText.Encode("description");
		public static readonly JsonEncodedText OptionsProperty = JsonEncodedText.Encode("options");

		internal static void WriteToJson(IApplicationCommandNode node, Utf8JsonWriter writer)
		{
			WriteToJson(writer, node.Name, node.Description, node.Options);
		}

		internal static void WriteToJson(Utf8JsonWriter writer, string name, string description, IEnumerable<IApplicationCommandOption>? options = null)
		{
			writer.WriteString(NameProperty, name);
			writer.WriteString(DescriptionProperty, description);
			writer.WriteObjectArray(OptionsProperty, options, IApplicationCommandOption.WriteToJson);
		}
	}

	/// <summary>
	/// Represents one application command.
	/// </summary>
	public interface IApplicationCommand : IApplicationCommandNode
	{
		/// <summary>
		/// Gets a value indicating whether the command is enabled by default when the app is added to a guild
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the command has default permission; otherwise, <see langword="false"/>.
		/// </value>
		bool HasDefaultPermission { get; }

		public static readonly JsonEncodedText HasDefaultPermissionProperty = JsonEncodedText.Encode("default_permission");

		internal static void WriteToJson(IApplicationCommand command, Utf8JsonWriter writer)
		{
			WriteToJson(writer, command.Name, command.Description, command.Options, command.HasDefaultPermission);
		}

		internal static void WriteToJson(Utf8JsonWriter writer, string name, string description, IEnumerable<IApplicationCommandOption>? options = null, bool hasDefaultPermission = true)
		{
			IApplicationCommandNode.WriteToJson(writer, name, description, options);
			if (!hasDefaultPermission) writer.WriteBoolean(HasDefaultPermissionProperty, false);
		}
	}

	/// <summary>
	/// Represents an application command option, as defined in <see href="https://discord.com/developers/docs/interactions/slash-commands#applicationcommandoption">Discord's documentation</see>.
	/// </summary>
	public interface IApplicationCommandOption : IApplicationCommandNode
	{
		/// <summary>
		/// Gets a value indicating type of this command option.
		/// </summary>
		/// <value>
		/// The type of this command option. This should be a defined value in the <see cref="ApplicationCommandOptionType"/> enum.
		/// </value>
		ApplicationCommandOptionType Type { get; }

		/// <summary>
		/// Gets the choices for <see cref="ApplicationCommandOptionType.String"/> and <see cref="ApplicationCommandOptionType.Integer"/> option types for the user to pick from, if applicable.
		/// </summary>
		/// <remarks>
		/// <para>If these choices exist for an option, they are the only valid values for a user to pick.</para>
		/// </remarks>
		/// <value>
		/// The choices for this command option. This should be <see langword="null"/> if any option is allowed, if <see cref="Type"/> is not <see cref="ApplicationCommandOptionType.String"/> or <see cref="ApplicationCommandOptionType.Integer"/>, or if <see cref="HasAutocompletion"/> is <see langword="true"/>.
		/// </value>
		IEnumerable<IApplicationCommandOptionChoice>? Choices { get; }

		/// <summary>
		/// Gets a value indicating whether this option should be autocompleted.
		/// </summary>
		/// <value>
		/// <see langword="true"/> is autocompletion should be enabled; otherwise, <see langword="false"/>. If <see langword="true"/>, <see cref="Choices"/> should be <see langword="null"/>.
		/// </value>
		bool HasAutocompletion { get; }

		/// <summary>
		/// Gets a value indicating whether the parameter is required or optional.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the parameter is required; <see langword="false"/> if the parameter is optional.
		/// </value>
		bool IsRequired { get; }

		/// <summary>
		/// Gets the channel types that this argument will be restricted to, if <see cref="Type"/> is <see cref="ApplicationCommandOptionType.Channel"/>.
		/// </summary>
		/// <value>
		/// The allowed channel types for this command option. This should be <see langword="null"/> if <see cref="Type"/> is not <see cref="ApplicationCommandOptionType.Channel"/>, or if any channel type is allowed.
		/// </value>
		IEnumerable<ChannelType>? ChannelTypes { get; }

		public static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		public static readonly JsonEncodedText ChoicesProperty = JsonEncodedText.Encode("choices");
		public static readonly JsonEncodedText HasAutocompletionProperty = JsonEncodedText.Encode("autocomplete");
		public static readonly JsonEncodedText IsRequiredProperty = JsonEncodedText.Encode("required");
		public static readonly JsonEncodedText ChannelTypesProperty = JsonEncodedText.Encode("channel_types");

		internal static void WriteToJson(IApplicationCommandOption option, Utf8JsonWriter writer)
		{
			if (option is null) throw new ArgumentException("commandOptions contains a null value.");

			writer.WriteNumber(TypeProperty, (int)option.Type);
			if (option.IsRequired) writer.WriteBoolean(IsRequiredProperty, true);
			if (option.HasAutocompletion) writer.WriteBoolean(HasAutocompletionProperty, true);

			IApplicationCommandNode.WriteToJson(option, writer);

			writer.WriteObjectArray(ChoicesProperty, option.Choices, IApplicationCommandOptionChoice.WriteToJson);

			IEnumerable<ChannelType>? channelTypes = option.ChannelTypes;
			if (channelTypes is not null)
			{
				writer.WriteStartArray(ChannelTypesProperty);
				foreach (ChannelType channelType in channelTypes)
				{
					writer.WriteNumberValue((int)channelType);
				}
				writer.WriteEndArray();
			}
		}
	}

	/// <summary>
	/// Represents a choice in an application command option, as defined in <see href="https://discord.com/developers/docs/interactions/slash-commands#applicationcommandoptionchoice">Discord's documentation</see>.
	/// </summary>
	public interface IApplicationCommandOptionChoice
	{
		/// <summary>
		/// Gets the name of this choice.
		/// </summary>
		/// <value>
		/// The 1-100 character choice name.
		/// </value>
		string Name { get; }

		/// <summary>
		/// Gets the value of the choice.
		/// </summary>
		/// <value>
		/// The <see cref="string"/> or <see cref="int"/> that is the value of this choice. If it's a <see cref="string"/>, it should be up to 100 characters long. Only <see cref="string"/> is valid for autocomplete interactions.
		/// </value>
		OneOf<string, int> Value { get; }

		public static readonly JsonEncodedText NameProperty = JsonEncodedText.Encode("name");
		public static readonly JsonEncodedText ValueProperty = JsonEncodedText.Encode("value");

		internal static void WriteToJson(IApplicationCommandOptionChoice choice, Utf8JsonWriter writer)
		{
			writer.WriteString(NameProperty, choice.Name);

			OneOf<string, int> value = choice.Value;
			if (value.IsT0) writer.WriteString(ValueProperty, value.AsT0);
			else if (value.IsT1) writer.WriteNumber(ValueProperty, value.AsT1);
		}
	}

	/// <summary>
	/// Represents the type of an application command option, as defined in <see href="https://discord.com/developers/docs/interactions/application-commands#application-command-object-application-command-option-type">Discord's documentation</see>.
	/// </summary>
	public enum ApplicationCommandOptionType
	{
		SubCommand = 1,
		SubCommandGroup = 2,
		String = 3,
		Integer = 4,
		Boolean = 5,
		User = 6,
		Channel = 7,
		Role = 8,
		Mentionable = 9,
		Number = 10
	}

	/// <summary>
	/// Represents a command permission.
	/// </summary>
	public interface IApplicationCommandPermission
	{
		/// <summary>
		/// Gets the ID of the role or user.
		/// </summary>
		/// <value>
		/// The ID of the role or user.
		/// </value>
		string Id { get; }

		/// <summary>
		/// Gets a value indicating what this permission is for.
		/// </summary>
		/// <value>
		/// The type of this command permission. This should be a defined value in the <see cref="ApplicationCommandPermissionType"/> enum.
		/// </value>
		ApplicationCommandPermissionType Type { get; }

		/// <summary>
		/// Gets a value indicating whether this role or user <em>can</em> or <em>cannot</em> use this command.
		/// </summary>
		/// <value>
		/// <see langword="true"/> to allow the command for this role or user; <see langword="false"/> to disallow it.
		/// </value>
		bool Permission { get; }

		public static readonly JsonEncodedText IdProperty = JsonEncodedText.Encode("id");
		public static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		public static readonly JsonEncodedText PermissionProperty = JsonEncodedText.Encode("permission");

		// I don't know where else to put this
		internal static readonly JsonEncodedText PermissionsProperty = JsonEncodedText.Encode("permissions");

		internal static void WriteToJson(IApplicationCommandPermission permission, Utf8JsonWriter writer)
		{
			writer.WriteString(IdProperty, permission.Id);
			writer.WriteNumber(TypeProperty, (int)permission.Type);
			writer.WriteBoolean(PermissionProperty, permission.Permission);
		}
	}

	/// <summary>
	/// Represents the type of an application command permission, as defined in <see href="https://discord.com/developers/docs/interactions/application-commands#application-command-permissions-object-application-command-permission-type">Discord's documentation</see>.
	/// </summary>
	public enum ApplicationCommandPermissionType
	{
		Role = 1,
		User = 2
	}

	#endregion

	#region Interaction responses

	interface IInteractionResponse
	{
		/// <summary>
		/// Gets a value indicating what type of interaction response this is.
		/// </summary>
		/// <value>
		/// The type of interaction response.
		/// </value>
		InteractionResponseType Type { get; }

		public static readonly JsonEncodedText TypeProperty = JsonEncodedText.Encode("type");
		public static readonly JsonEncodedText DataProperty = JsonEncodedText.Encode("data");

		internal static void WriteToJson(Utf8JsonWriter writer, InteractionResponseType type)
		{
			writer.WriteNumber(TypeProperty, (int)type);
		}
	}

	/// <summary>
	/// Represents the type of response to an interaction.
	/// </summary>
	/// <seealso href="https://discord.com/developers/docs/interactions/slash-commands#interaction-response-interactionresponsetype"/>
	public enum InteractionResponseType
	{
		/// <summary>
		/// Acknowledge a ping.
		/// </summary>
		Pong = 1,

		/// <summary>
		/// Acknowledge an interaction without sending a message, eating the user's input.
		/// </summary>
		/// <remarks>
		/// <para>Deprecated in pull request <see href="https://github.com/discord/discord-api-docs/pull/2615">#2615</see>.</para>
		/// </remarks>
		[Obsolete("Since March 2021, you can no longer acknowledge without source.")]
		Acknowledge = 2,

		/// <summary>
		/// Respond with a message, eating the user's input.
		/// </summary>
		/// <remarks>
		/// <para>Deprecated in pull request <see href="https://github.com/discord/discord-api-docs/pull/2615">#2615</see>.</para>
		/// </remarks>
		[Obsolete("Since March 2021, you can no longer send a message without source.")]
		ChannelMessage = 3,

		/// <summary>
		/// Respond with a message, showing the user's input.
		/// </summary>
		ChannelMessageWithSource = 4,

		/// <summary>
		/// Acknowledge an interaction and edit a response later, the user sees a loading state.
		/// </summary>
		/// <remarks>
		/// <para>Formely known as <c>AcknowledgeWithSource</c>.</para>
		/// </remarks>
		DeferredChannelMessageWithSource = 5,

		/// <summary>
		/// For components, acknowledge an interaction and edit the original message later; the user does not see a loading state.
		/// </summary>
		/// <remarks>
		/// <para>Only valid for component-based interactions.</para>
		/// </remarks>
		DeferredUpdateMessage = 6,

		/// <summary>
		/// For components, edit the message the component was attached to.
		/// </summary>
		/// <remarks>
		/// <para>Only valid for component-based interactions.</para>
		/// </remarks>
		UpdateMessage = 7,

		/// <summary>
		/// For autocomplete interactions.
		/// </summary>
		/// <remarks>
		/// <para>Only valid for application-command-based interactions.</para>
		/// </remarks>
		ApplicationCommandAutocompleteResult = 8
	}

	#endregion
}
