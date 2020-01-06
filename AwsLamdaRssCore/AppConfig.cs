using System;
using System.Collections.Generic;
using System.Text;

namespace AwsLamdaRssCore
{
    public class AppConfig
    {
        public static readonly string OwnerChatId = Environment.GetEnvironmentVariable("ChatOwnerId");
        public static readonly string MonkeyJobBotToken = Environment.GetEnvironmentVariable("MonkeyJobBotToken");
        public static readonly string PrivateMotoChatId = Environment.GetEnvironmentVariable("PrivateMotoChatId");
        public static readonly string MotoNewsChatId = Environment.GetEnvironmentVariable("MotoNewsChatId");
        public static readonly string MotoNewsChatBotToken = Environment.GetEnvironmentVariable("MotoNewsChatBotToken");
    }
}
