// This file contains definitions for certain Discord API resources, that are not worthy of having their own file.

using OneOf;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace SimpleDiscord
{
	#region Channels and roles

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
		public string Id { get; }

		/// <summary>
		/// Gets the position of the role.
		/// </summary>
		/// <value>
		/// The position of the role.
		/// </value>
		public int Position { get; }
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
		/// The name of the emoji. Can be null only in reaction emoji objects.
		/// </value>
		public string? Name { get; }

		/// <summary>
		/// Gets a value indicating whether this emoji is animated.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if this emoji is animated; otherwise, <see langword="false"/>.
		/// </value>
		public bool IsAnimated { get; }
	}

	#endregion

	#region Embeds

	/// <summary>
	/// Represents one embed in a message.
	/// </summary>
	public interface IEmbed
	{
		/// <summary>
		/// Gets the title of the embed.
		/// </summary>
		/// <remarks>
		/// <para>The title may include Markdown formatting.</para>
		/// </remarks>
		/// <value>
		/// The title of the embed.
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
		/// The timestamp of the embed, in UTC.
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
		/// A collection of this embed's fields.
		/// </value>
		public IEnumerable<IEmbedField>? Fields { get; }
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
		/// The name of the author.
		/// </value>
		public string? Name { get; }

		/// <summary>
		/// Gets the URL of the author.
		/// </summary>
		/// <value>
		/// The URL of the author.
		/// </value>
		public string? Url { get; }
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
		/// The text of the footer.
		/// </value>
		public string Text { get; }
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
	}

	#endregion

	#region Message components

	/// <summary>
	/// Represents an action row component (component type 1).
	/// </summary>
	public interface IActionRowComponent
	{
		/// <summary>
		/// Gets the components in this action row.
		/// </summary>
		IEnumerable<OneOf<IInteractionButtonComponent, ILinkButtonComponent, ISelectMenuComponent>> Components { get; }
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

		internal static void WriteToJson(ILabeledElement element, Utf8JsonWriter writer)
		{
			writer.WriteString("label", element.Label);

			IEmoji? emoji = element.Emoji;
			if (emoji is not null)
			{
				writer.WriteStartObject("emoji");
				writer.WriteString("id", emoji.Id);
				writer.WriteString("name", emoji.Name);
				if (emoji.IsAnimated) writer.WriteBoolean("animated", true);
				writer.WriteEndObject();
			}
		}
	}

	/// <summary>
	/// Represents a button message component (component type 2).
	/// </summary>
	public interface IButtonComponent : ILabeledElement, IDisableableComponent
	{
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
	}

	/// <summary>
	/// Represents a select menu message component (component type 3).
	/// </summary>
	public interface ISelectMenuComponent : IInteractionComponent, IDisableableComponent
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
		/// The choices for this command option. This should be <see langword="null"/> if <see cref="Type"/> is not <see cref="ApplicationCommandOptionType.String"/> or <see cref="ApplicationCommandOptionType.Integer"/>, or if any option is allowed.
		/// </value>
		IEnumerable<IApplicationCommandOptionChoice>? Choices { get; }

		/// <summary>
		/// Gets a value indicating whether the parameter is required or optional.
		/// </summary>
		/// <value>
		/// <see langword="true"/> if the parameter is required; <see langword="false"/> if the parameter is optional.
		/// </value>
		bool IsRequired { get; }
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
		/// The <see cref="string"/> or <see cref="int"/> that is the value of this choice. If it's a <see cref="string"/>, it should be up to 100 characters long.
		/// </value>
		OneOf<string, int> Value { get; }
	}

	/// <summary>
	/// Represents the type of an application command option, as defined in <see href="https://discord.com/developers/docs/interactions/slash-commands#applicationcommandoptiontype">Discord's documentation</see>.
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

	#endregion

	#region Interaction responses

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
		UpdateMessage = 7
	}

	#endregion
}
