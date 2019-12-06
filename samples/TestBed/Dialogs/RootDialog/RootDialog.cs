using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;

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
                            Pattern = "(?i)start"
                        },
                        new IntentPattern()
                        {
                            Intent = "cancel",
                            Pattern = "(?i)cancel"
                        },
                        new IntentPattern()
                        {
                            Intent = "set_name",
                            Pattern = "(?i)set name"
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
                        Intent = "set_name",
                        Actions = new List<Dialog>()
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("\\[set name]::What is your name?"),
                                Property = "$userName",
                                AlwaysPrompt = true
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("\\[set name]I have @{$userName} as your name")
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "start",
                        Actions = new List<Dialog>()
                        {
                            new InitProperty()
                            {
                                Property = "turn.recognized.entities.foo",
                                Type = "array"
                            },
                            new EditArray()
                            {
                                ItemsProperty = "turn.recognized.entities.foo",
                                Value = "'test'",
                                ChangeType = EditArray.ArrayChangeType.Push
                            },
                            new SetProperty()
                            {
                                Property = "@@bar",
                                Value = "coalesce(@foo, null)"
                            },
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("I have @{@@bar}")
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "cancel",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("@{WelcomeUser()}")
                        }
                    }
                }
            };

            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static LuisApplication GetLUISApp()
        {
            return new LuisApplication()
            {
                ApplicationId = "822278ff-e172-4e87-931f-d8bd5f40163e",
                EndpointKey = "a95d07785b374f0a9d7d40700e28a285",
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };
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
                                new SendActivity("@{WelcomeUser()}")
                            }
                        }
                    }
                }
            };
        }
    }
}
