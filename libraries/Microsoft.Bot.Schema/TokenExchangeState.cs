﻿using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Microsoft.Bot.Schema
{
    public class TokenExchangeState
    {
        /// <summary>
        /// The connection name that was used
        /// </summary>
        [JsonProperty("connectionName")]
        public string ConnectionName { get; set; }

        /// <summary>
        /// A reference to the conversation
        /// </summary>
        [JsonProperty("conversation")]
        public ConversationReference Conversation { get; set; }

        /// <summary>
        /// The URL of the bot messaging endpoint
        /// </summary>
        [JsonProperty("botUrl")]
        public string BotUrl { get; set; }

        /// <summary>
        /// The bot's registered application ID
        /// </summary>
        [JsonProperty("msAppId")]
        public string MsAppId { get; set; }
    }
}
