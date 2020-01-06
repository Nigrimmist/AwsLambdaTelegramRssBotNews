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

namespace ConsoleApp4
{
    class Program
    {

        public static void Main(string[] args)
        {
            Environment.SetEnvironmentVariable("AwsAccessKey", "");
            Environment.SetEnvironmentVariable("AwsSecretKey", "");
            Environment.SetEnvironmentVariable("ChatOwnerId", "");
            Environment.SetEnvironmentVariable("MonkeyJobBotToken", "");
            Environment.SetEnvironmentVariable("MotoNewsChatBotToken", "");
            Environment.SetEnvironmentVariable("MotoNewsChatId", "");
            Environment.SetEnvironmentVariable("PrivateMotoChatId", "");
            Task t = MainAsync(args);
            t.Wait();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        public static async Task MainAsync(string[] args)
        {
            await new RssGrabber().GrabUrls();
        }
    }


    
}
