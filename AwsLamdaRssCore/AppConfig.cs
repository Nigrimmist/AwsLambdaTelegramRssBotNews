﻿using System;
using System.Collections.Generic;
using System.Text;

namespace AwsLamdaRssCore
{
    public class AppConfig
    {
        /// <summary>
        /// Owner chat id, all news will be sent here to approve
        /// </summary>
        public static readonly string OwnerChatId = Environment.GetEnvironmentVariable("ChatOwnerId");
        /// <summary>
        /// Debug bot token to receive some debug info, errors and etc.
        /// </summary>
        public static readonly string DebugBotToken = Environment.GetEnvironmentVariable("DebugBotToken");
        /// <summary>
        /// Main bot to send messages for approve and to your chats after approve
        /// </summary>
        public static readonly string MonkeyJobBotToken = Environment.GetEnvironmentVariable("MonkeyJobBotToken");
        /// <summary>
        /// Another one chat source to receive news
        /// </summary>
        public static readonly string PrivateMotoChatId = Environment.GetEnvironmentVariable("PrivateMotoChatId");
        /// <summary>
        /// public channel to receive news
        /// </summary>
        public static readonly string MotoNewsChatId = Environment.GetEnvironmentVariable("MotoNewsChatId");
        /// <summary>
        /// public channel's bot to broadcast rss news
        /// </summary>
        public static readonly string MotoNewsChatBotToken = Environment.GetEnvironmentVariable("MotoNewsChatBotToken");
    }
}
