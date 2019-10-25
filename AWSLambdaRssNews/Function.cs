using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Amazon.Lambda.Core;
using AwsLamdaRssCore;

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
                return await new RssGrabber().Grab();

            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }
    }
}
