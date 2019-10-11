using System.IO;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        private TemplateEngine _templateEngine;
        public RootDialog()
            : base(nameof(RootDialog))
        {
            var lgFile = Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg");
            _templateEngine = new TemplateEngine().AddFile(lgFile);
            // Create instance of adaptive dialog. 
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(_templateEngine),
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern()
                        {
                            Intent = "start",
                            Pattern = "start"
                        },
                        new IntentPattern()
                        {
                            Intent = "cancel",
                            Pattern = "cancel"
                        }
                    }
                },
                Triggers = new List<OnCondition>()
                {
                    new OnConversationUpdateActivity()
                    {
                        Actions = WelcomeUserAction()
                    },
                    new OnIntent()
                    {
                        Intent = "start",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("{GreetingReply()}")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "cancel",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("{GreetingReply()}")
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }
        private static List<Dialog> WelcomeUserAction()
        {
            return new List<Dialog>()
            {
                // Iterate through membersAdded list and greet user added to the conversation.
                new Foreach()
                {
                    ItemsProperty = "turn.activity.membersAdded",
                    Actions = new List<Dialog>()
                    {
                        // Note: Some channels send two conversation update events - one for the Bot added to the conversation and another for user.
                        // Filter cases where the bot itself is the recipient of the message. 
                        new IfCondition()
                        {
                            Condition = "dialog.foreach.value.name != turn.activity.recipient.name",
                            Actions = new List<Dialog>()
                            {
                                new SendActivity("[WelcomeUser]")
                            }
                        }
                    }
                }
            };

        }
    }
}
