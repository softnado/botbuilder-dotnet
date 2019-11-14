// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core;
using Bot.Builder.Community.Adapters.Alexa.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Adapters.Slack;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleRootBot.Middleware;

namespace SimpleRootBot
{
    public class SlackAdapterWithErrorHandler : SlackAdapter
    {
        public SlackAdapterWithErrorHandler(IConfiguration configuration)
            : base(configuration)
        {
            OnTurnError = async (context, exception) =>
            {
                await context.SendActivityAsync("Sorry, something went wrong");
            };
        }
    }
}
