using Amazon;
using Amazon.DynamoDBv2;
using Amazon.DynamoDBv2.DataModel;
using Amazon.DynamoDBv2.DocumentModel;
using Amazon.DynamoDBv2.Model;
using Amazon.Runtime;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AwsLamdaRssCore;
using Telegram.Bot.Types.ReplyMarkups;
using System.IO;

namespace ConsoleApp4
{
    class Program
    {

        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("AwsAccessKey", "AKIAJFQDXIG3XDZ2CKGQ");
            Environment.SetEnvironmentVariable("AwsSecretKey", "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");
            Environment.SetEnvironmentVariable("ChatOwnerId", "101483786");
            Environment.SetEnvironmentVariable("MonkeyJobBotToken", "258763201:AAHseg57B9ld2wo9U5RKnZWbvbuPfXMrYNM");
            Environment.SetEnvironmentVariable("MotoNewsChatBotToken", "247995979:AAFRMNTilitQYypinYQ9epLOf7p8isU30k8");
            //Environment.SetEnvironmentVariable("MotoNewsChatId", "@motoNewsBy");
            Environment.SetEnvironmentVariable("MotoNewsChatId", "101483786");
            //Environment.SetEnvironmentVariable("PrivateMotoChatId", "-1001138372643");
            Environment.SetEnvironmentVariable("PrivateMotoChatId", "101483786");
            Task t = MainAsync(args);
            t.Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task MainAsync(string[] args)
        {
            
            string json = File.ReadAllText("./webhook.test.json");
        
            var s = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(json);            
            new AwsLambdaHandleTelegramWebhooks.Function().FunctionHandler(s,null);

        }
    }


    
}
