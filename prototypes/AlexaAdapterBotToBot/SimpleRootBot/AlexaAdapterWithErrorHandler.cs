// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Threading;
using System.Threading.Tasks;
using Bot.Builder.Community.Adapters.Alexa;
using Bot.Builder.Community.Adapters.Alexa.Integration.AspNet.Core;
using Bot.Builder.Community.Adapters.Alexa.Middleware;
using Microsoft.AspNetCore.Http;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SimpleRootBot.Middleware;

namespace SimpleRootBot
{
    public class AlexaAdapterWithErrorHandler : AlexaHttpAdapter
    {
        public AlexaAdapterWithErrorHandler() 
            : base(validateRequests: true)
        {
            OnTurnError = async (turnContext, exception) =>
            {
                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync($"Sorry, it looks like something went wrong. \r\n{exception}");
            };

            ShouldEndSessionByDefault = false;
            ConvertBotBuilderCardsToAlexaCards = false;

            // Register a couple of dummy middleware instances for testing.
            Use(new AlexaIntentRequestToMessageActivityMiddleware());
            Use(new DummyMiddleware("Instance 1"));
            Use(new DummyMiddleware("Instance 2"));
        }
    }
}
