# AwsLambdaTelegramRssBotNews
Small pet project using .net core, AWS Lambda and Telegram Bot api to send news from RSS feeds.

# Description

Reading rss channels -> Find interesting by predefined keywords (blacklist/whitelist) -> Send to Telegram channels by Telegram bots and executing it all in AWS Lambda by AWS CloudWatch. Deployment to aws in a few clicks by Visual Studio AWS Explorer menu.

# Technologies
.net core 2.1, C#

AWS Lambda - to run it every n hours (using AWS CloudWatch)

AWS DynamoDB - to store latest posted news (to not send twice)

Telegram bot official api for .net - to send urls in Telegram using bot

# Project requirements
Visual studio 2019

.net core 2.1 (aws requirement)

# Project structure
AWSLambdaRssNews project - main lambda project, calling AwsLamdaRssCore

AwsLamdaRssCore - core project containing all business code.

Console4 is a console project to testing locally, nothing special.

# PS
Be carefull, it is a demo. For example, it has not any optimisations,code ported from others pet projects, no any logic to clear db and etc and etc. Also, keep in mind, that you need to have environment variables with "secret" data, you can fill them in consoleApp4 project (for test purposes).

Pull Requests are welcome! Have fun!

# Help links
https://dev.to/nqcm/-building-a-telegram-bot-with-aws-api-gateway-and-aws-lambda-27fg
https://medium.com/devtechblogs/launch-a-c-net-core-lambda-function-in-aws-step-by-step-5e4636516758
