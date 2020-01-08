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
        private TelegramClientIntegration _debugClient;
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
                    new ClientIntegrationSettings("Owner", AppConfig.OwnerChatId, AppConfig.MonkeyJobBotToken, ClientIntegrationType.Owner),
                    new ClientIntegrationSettings("Debug", AppConfig.OwnerChatId, AppConfig.DebugBotToken, ClientIntegrationType.Debug),
                    new ClientIntegrationSettings("MotoNews channel", AppConfig.MotoNewsChatId, AppConfig.MotoNewsChatBotToken),
                    new ClientIntegrationSettings("Local chat", AppConfig.PrivateMotoChatId, AppConfig.MonkeyJobBotToken)
                }
            };

            var ownerConf = botConfiguration.ClientIntegrationSettings.Single(x => x.IntegrationType == ClientIntegrationType.Owner);
            var debugConf = botConfiguration.ClientIntegrationSettings.Single(x => x.IntegrationType == ClientIntegrationType.Debug);
            var clientsConfs = botConfiguration.ClientIntegrationSettings.Where(x => x.IntegrationType == ClientIntegrationType.IntegrationClient).ToList();

            _ownerClient = new TelegramClientIntegration()
            {
                Client = new TelegramClient(ownerConf.ToChatId, ownerConf.BotToken),
                Settings = ownerConf
            };

            _debugClient = new TelegramClientIntegration()
            {
                Client = new TelegramClient(debugConf.ToChatId, debugConf.BotToken),
                Settings = debugConf
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

        public void SendMessageToDebug(string message)
        {
            _debugClient.Client.SendMessage(message);
        }


        public void SendMessageToChat(string chatId, string message)
        {
            new TelegramClient(chatId, AppConfig.MonkeyJobBotToken).SendMessage(message);
        }

        public void HandleWebhookUpdate(Update webhook)
        {
            switch (webhook.Type)
            {
                case Telegram.Bot.Types.Enums.UpdateType.CallbackQuery:
                {
                    string integrationClientName = webhook.CallbackQuery.Data;

                    var integrationClient = _integrationClients.Single(x => x.Settings.Name == integrationClientName);
                    integrationClient.Client.SendMessage(webhook.CallbackQuery.Message.Text);

                    break;
                }
            }
        }
    }
}
