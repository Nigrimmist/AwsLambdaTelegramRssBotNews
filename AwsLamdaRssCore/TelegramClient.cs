using System.Collections.Generic;
using System.Linq;
using Telegram.Bot;

namespace AwsLamdaRssCore
{
    public class TelegramClient
    {
        private readonly string _toChatId;
        private readonly string _botToken;

        public TelegramClient(string toChatId, string botToken)
        {
            _toChatId = toChatId;
            _botToken = botToken;
        }

        private List<ClientTelegramWrapper> _bots = null;

        public void SendMessage(string message)
        {
            if (message == null) return;

            if (_bots == null)
            {
                _bots = new List<ClientTelegramWrapper>();
                _bots.Add(new ClientTelegramWrapper()
                {
                    Token = _botToken,
                    ChatId = _toChatId,
                    ShowTitle = false
                });
            }

            if (_bots.Any())
            {
                foreach (var bot in _bots)
                {
                    if (bot.Bot == null && !bot.BotInited)
                    {

                        if (!string.IsNullOrEmpty(bot.Token))
                        {
                            bot.Bot = new TelegramBotClient(bot.Token);
                            SendMessage(bot.Bot, bot.ChatId, message);

                        }

                        bot.BotInited = true;
                    }
                }

            }

        }


        private void SendMessage(TelegramBotClient bot, string chatId, string message)
        {
            if (!chatId.StartsWith("@") && long.TryParse(chatId, out var id))
            {
                var result = bot.SendTextMessageAsync(id, message).Result;
            }
            else
            {
                var result = bot.SendTextMessageAsync(chatId, message).Result;
            }
        }
    }




    public class TelegramSettings
    {
        public List<TelegramBotSettings> Bots { get; set; }

        public TelegramSettings()
        {
            Bots = new List<TelegramBotSettings>();
        }
    }

    public class TelegramBotSettings
    {
        public string Token { get; set; }

        public bool ShowTitle { get; set; }

        public string ChatId { get; set; }
        public int Offset { get; set; }

        public TelegramBotSettings()
        {

        }
    }

    class ClientTelegramWrapper
    {
        public bool BotInited { get; set; }
        public string Token { get; set; }
        public int Offset { get; set; }
        public string ChatId { get; set; }
        public TelegramBotClient Bot { get; set; }
        public bool ShowTitle { get; set; }

        public ClientTelegramWrapper()
        {

        }


    }
}
