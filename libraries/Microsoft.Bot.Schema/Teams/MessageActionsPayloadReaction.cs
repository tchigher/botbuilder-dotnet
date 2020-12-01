﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.Bot.Schema.Teams
{
    using System.Linq;
    using Newtonsoft.Json;

    /// <summary>
    /// Represents the reaction of a user to a message.
    /// </summary>
    public partial class MessageActionsPayloadReaction
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadReaction"/> class.
        /// </summary>
        public MessageActionsPayloadReaction()
        {
            CustomInit();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MessageActionsPayloadReaction"/> class.
        /// </summary>
        /// <param name="reactionType">The type of reaction given to the
        /// message. Possible values include: 'like', 'heart', 'laugh',
        /// 'surprised', 'sad', 'angry'.</param>
        /// <param name="createdDateTime">Timestamp of when the user reacted to
        /// the message.</param>
        /// <param name="user">The user with which the reaction is
        /// associated.</param>
        public MessageActionsPayloadReaction(string reactionType = default(string), string createdDateTime = default(string), MessageActionsPayloadFrom user = default(MessageActionsPayloadFrom))
        {
            ReactionType = reactionType;
            CreatedDateTime = createdDateTime;
            User = user;
            CustomInit();
        }

        /// <summary>
        /// Gets or sets the type of reaction given to the message. Possible
        /// values include: 'like', 'heart', 'laugh', 'surprised', 'sad',
        /// 'angry'.
        /// </summary>
        [JsonProperty(PropertyName = "reactionType")]
        public string ReactionType { get; set; }

        /// <summary>
        /// Gets or sets timestamp of when the user reacted to the message.
        /// </summary>
        [JsonProperty(PropertyName = "createdDateTime")]
        public string CreatedDateTime { get; set; }

        /// <summary>
        /// Gets or sets the user with which the reaction is associated.
        /// </summary>
        [JsonProperty(PropertyName = "user")]
        public MessageActionsPayloadFrom User { get; set; }

        /// <summary>
        /// An initialization method that performs custom operations like setting defaults.
        /// </summary>
        partial void CustomInit();
    }
}