using AwsLamdaRssCore.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Humanizer;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace AwsLamdaRssCore.Managers
{
    public class TelegramSenderManager
    {
        private TelegramClientIntegration _ownerClient;
        private List<TelegramClientIntegration> _integrationClients;

        public TelegramSenderManager()
        {

        }

        public void Init()
        {
            var botConfiguration = new BotConfiguration()
            {
                ClientIntegrationSettings = new List<ClientIntegrationSettings>()
                {
                    new ClientIntegrationSettings("Owner", AppConfig.OwnerChatId, AppConfig.MonkeyJobBotToken,
                        ClientIntegrationType.Owner),
                    new ClientIntegrationSettings("MotoNews channel", AppConfig.MotoNewsChatId,
                        AppConfig.MotoNewsChatBotToken),
                    new ClientIntegrationSettings("Local chat", AppConfig.PrivateMotoChatId,
                        AppConfig.MonkeyJobBotToken)
                }
            };

            var ownerConf = botConfiguration.ClientIntegrationSettings
                .Where(x => x.IntegrationType == ClientIntegrationType.Owner).Single();
            var clientsConfs = botConfiguration.ClientIntegrationSettings
                .Where(x => x.IntegrationType == ClientIntegrationType.IntegrationClient).ToList();

            _ownerClient = new TelegramClientIntegration()
            {
                Client = new TelegramClient(ownerConf.ToChatId, ownerConf.BotToken),
                Settings = ownerConf
            };

            _integrationClients = clientsConfs.Select(x => new TelegramClientIntegration()
            {
                Client = new TelegramClient(x.ToChatId, x.BotToken),
                Settings = x
            }).ToList();

            Console.WriteLine("TelegramSenderManager inited");
        }

        public void SendUrlWithButtonsToOwner(string url)
        {

            var buttons = _integrationClients.Select(x => new InlineKeyboardButton()
            {
                Text = x.Settings.Name,
                CallbackData = x.Settings.Name
            }).ToArray();
            var myInlineKeyboard = new InlineKeyboardMarkup(new[] {buttons});
            _ownerClient.Client.SendMessage(url, myInlineKeyboard);
        }

        public void SendMessageToOwner(string message)
        {
            _ownerClient.Client.SendMessage(message);
        }


        public void SendMessageToChat(string chatId, string message)
        {
            new TelegramClient(chatId, AppConfig.MonkeyJobBotToken).SendMessage(message);
        }

        public void HandleWebhookUpdate(Update webhook)
        {
            switch (webhook.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.Message:
                case Telegram.Bot.Types.Enums.UpdateType.EditedMessage:
                {
                    var tgMessage = webhook.EditedMessage ?? webhook.Message;
                    var mention = tgMessage?.Entities?.SingleOrDefault(x =>
                        x.Type == Telegram.Bot.Types.Enums.MessageEntityType.Mention);
                    string message = tgMessage?.Text;
                    long? chatId = tgMessage?.Chat?.Id;

                    if (message != null && mention != null)
                    {
                        // to do : hide bot name
                        bool isBotMentioned = mention.Offset == 0 &&
                                              message.Substring(0, mention.Length) == "@MonkeyJobBot";
                        if (isBotMentioned)
                        {
                            List<KeyValuePair<string, Func<string>>> questions =
                                new List<KeyValuePair<string, Func<string>>>()
                                {
                                    new KeyValuePair<string, Func<string>>(
                                        "((сколько|когда).*(сезон).*?)|((а) (теперь|сейчас|уже|еще)+)",
                                        new Func<string>(
                                            () =>
                                            {
                                                int monthSeasonStart = 4;
                                                int monthSeasonEnd = 10;
                                                DateTime nextSeason = DateTime.Now;
                                                if (DateTime.Now.Month < monthSeasonStart ||
                                                    DateTime.Now.Month > monthSeasonEnd)
                                                {
                                                    DateTime seasonWillStart = DateTime.Now;
                                                    if (DateTime.Now.Month > monthSeasonEnd)
                                                    {
                                                        seasonWillStart = new DateTime(DateTime.Now.Year + 1,
                                                            monthSeasonStart, 1);

                                                    }
                                                    else if (DateTime.Now.Month < monthSeasonStart)
                                                    {
                                                        seasonWillStart = new DateTime(DateTime.Now.Year,
                                                            monthSeasonStart, 1);
                                                    }

                                                    TimeSpan remains = seasonWillStart - DateTime.Now;

                                                    return remains.Humanize(5, CultureInfo.GetCultureInfo("RU-ru"));
                                                }
                                                else return "Да катай уже, кто тебе мешает :)";
                                            }))
                                };


                            foreach (var condition in questions)
                            {
                                Regex regex = new Regex(condition.Key);
                                MatchCollection matches = regex.Matches(message);
                                if (matches.Any())
                                {
                                    string response = condition.Value();
                                    SendMessageToChat(chatId.ToString(),
                                        $"Эх, до апреля ещё {response} ");

                                }
                            }

                        }
                    }

                    break;
                }

                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                {
                    string integrationClientName = webhook.CallbackQuery.Data;

                    var integrationClient = _integrationClients.Where(x => x.Settings.Name == integrationClientName)
                        .Single();
                    integrationClient.Client.SendMessage(webhook.CallbackQuery.Message.Text);

                    break;
                }
            }
        }
    }
}
