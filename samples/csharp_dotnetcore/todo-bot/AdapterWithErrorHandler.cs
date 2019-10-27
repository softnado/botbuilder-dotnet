// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

namespace Microsoft.BotBuilderSamples
{
    using System;
    using System.IO;
    using Microsoft.Bot.Builder;
    using Microsoft.Bot.Builder.Dialogs.Debugging;
    using Microsoft.Bot.Builder.Integration.AspNet.Core;
    using Microsoft.Bot.Builder.LanguageGeneration;
    using Microsoft.Bot.Connector.Authentication;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using ActivityBuilder = Microsoft.Bot.Builder.Dialogs.Adaptive.Generators.ActivityGenerator;

    public class AdapterWithErrorHandler : BotFrameworkHttpAdapter
    {
        private TemplateEngine _lgEngine;
        /// <summary>
        /// Initializes a new instance of the <see cref="AdapterWithErrorHandler"/> class.
        /// </summary>
        /// <param name="credentialProvider"></param>
        /// <param name="logger"></param>
        /// <param name="storage"></param>
        /// <param name="userState"></param>
        /// <param name="conversationState"></param>
        /// <param name="configuration"></param>
        public AdapterWithErrorHandler(ICredentialProvider credentialProvider, ILogger<BotFrameworkHttpAdapter> logger, IStorage storage,
            UserState userState, ConversationState conversationState, IConfiguration configuration)
            : base(credentialProvider)
        {
            this.UseStorage(storage);
            this.UseState(userState, conversationState);
            this.Use(new RegisterClassMiddleware<Microsoft.Bot.Builder.Dialogs.Adaptive.IActivityGenerator>(new ActivityBuilder()));
            this.UseDebugger(configuration.GetValue<int>("debugport", 4712));

            string[] paths = { ".", "AdapterWithErrorHandler.lg" };
            string fullPath = Path.Combine(paths);
            this._lgEngine = new TemplateEngine().AddFile(fullPath);

            this.OnTurnError = async (turnContext, exception) =>
            {
                // Log any leaked exception from the application.
                logger.LogError($"Exception caught : {exception.Message}");

                // Send a catch-all apology to the user.
                await turnContext.SendActivityAsync(ActivityBuilder.GenerateFromLG(this._lgEngine.EvaluateTemplate("SomethingWentWrong", exception)));

                if (conversationState != null)
                {
                    try
                    {
                        // Delete the conversationState for the current conversation to prevent the
                        // bot from getting stuck in a error-loop caused by being in a bad state.
                        // ConversationState should be thought of as similar to "cookie-state" in a Web pages.
                        await conversationState.DeleteAsync(turnContext);
                    }
                    catch (Exception e)
                    {
                        logger.LogError($"Exception caught on attempting to Delete ConversationState : {e.Message}");
                    }
                }
            };
        }
    }
}
