using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using AwsLamdaRssCore;
using AwsLamdaRssCore.Managers;
using AwsLamdaRssCore.Models;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace AWSLambdaRssNews
{
    public class Function
    {
        

        public async Task<string> FunctionHandler(ILambdaContext context)
        {
            Console.WriteLine("Function started");
            try
            {
                TelegramSenderManager sender = new TelegramSenderManager();
                sender.Init();
                try
                {
                    RssGrabber rssGrabber = new RssGrabber();
                    var urlsToPost = await rssGrabber.GrabUrls();

                    foreach(var url in urlsToPost)
                    {
                         sender.SendUrlWithButtonsToOwner(url);
                    }

                    return "done : "+ urlsToPost.Count;
                }
                catch (Exception e)
                {
                    sender.SendMessageToDebug(e.ToString());
                    throw;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
