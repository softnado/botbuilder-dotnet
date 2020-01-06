using System.Collections.Generic;
using System.IO;
using Microsoft.Bot.Builder.AI.Luis;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA;
using Microsoft.Bot.Builder.Dialogs.Adaptive.QnA.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Expressions;

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
            var rootDialog = new AdaptiveDialog()
            {
                AutoEndDialog = false,
                Recognizer = new RegexRecognizer()
                {
                    Intents = new List<IntentPattern>()
                    {
                        new IntentPattern()
                        {
                            Intent = "none",
                            Pattern = "none"
                        }
                    }
                },
                Triggers = new List<OnCondition>() 
                {
                    new OnBeginDialog() 
                    {
                        Actions = new List<Dialog>() 
                        {
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("What is your name"),
                                Property = "user.name"
                            },
                            new SendActivity("my name is @{user.name}")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "none",
                        Actions = new List<Dialog>()
                        {
                            new BeginDialog()
                            {
                                IncludeActivity = true,
                                Dialog = new AdaptiveDialog()
                                {
                                    AutoEndDialog = false,
                                    Recognizer = new RegexRecognizer()
                                    {
                                        Intents = new List<IntentPattern>()
                                        {
                                            new IntentPattern()
                                            {
                                                Intent = "none",
                                                Pattern = "none"
                                            }
                                        }
                                    },
                                    Triggers = new List<OnCondition>()
                                    {
                                        new OnIntent()
                                        {
                                            Intent = "none",
                                            Actions = new List<Dialog>()
                                            {
                                                new SendActivity("you are in sub-dialog's none intent")
                                            }
                                        },
                                        new OnBeginDialog()
                                        {
                                            Actions = new List<Dialog>()
                                            {
                                                new SendActivity("you are in sub-dialog's on begin dialog")
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            };
 
            // Add named dialogs to the DialogSet. These names are saved in the dialog state.
            AddDialog(rootDialog);

            // The initial child Dialog to run.
            InitialDialogId = nameof(AdaptiveDialog);
        }

        private static Recognizer GetLUISApp()
        {
            var luisapplication = new LuisApplication()
            {
                ApplicationId = "063e7f98-fef5-4b60-a740-39a6d933dd09",
                EndpointKey = "a95d07785b374f0a9d7d40700e28a285",
                Endpoint = "https://westus.api.cognitive.microsoft.com"
            };
            var luisrecognizeroptions = new LuisRecognizerOptionsV2(luisapplication);
            return new LuisRecognizer(luisrecognizeroptions)
            {
                Id = "Root_LUIS"
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

        private static Recognizer QnARecognizer()
        {
            return new QnAMakerRecognizer()
            {
                Id = "Root_QnA",
                HostName = "'https://vk-test-qna.azurewebsites.net/qnamaker'",
                EndpointKey = "'8e744f2e-2f80-4c16-bb68-7eb2a088726f'",
                KnowledgeBaseId = "'206eab69-6573-4a8d-939b-63a1a2511d11'",
                Top = 10
            };
        }

        private static Recognizer MultiRecognizer()
        {
            return new CrossTrainedRecognizerSet()
            {
                Recognizers = new List<Recognizer>()
                {
                    QnARecognizer(),
                    GetLUISApp()
                }
            };
        }
    }
}
