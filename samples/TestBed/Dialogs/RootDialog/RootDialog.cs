using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
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
using Newtonsoft.Json.Linq;

namespace Microsoft.BotBuilderSamples
{
    public class RootDialog : ComponentDialog
    {
        private LGFile _lgFile;

        public RootDialog()
            : base(nameof(RootDialog))
        {
            _lgFile = LGParser.ParseFile(Path.Combine(".", "Dialogs", "RootDialog", "RootDialog.lg"));
            var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
            {
                Generator = new TemplateEngineLanguageGenerator(_lgFile),
                Recognizer = MultiRecognizer(),
                Triggers = new List<OnCondition>()
                {
                    new OnConversationUpdateActivity()
                    {
                        Actions = WelcomeUserAction()
                    },
                    new OnQnAMatch()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity()
                            {
                                Activity = new ActivityTemplate("Here's what I have from QnA Maker - @{@answer}"),
                            }
                        }
                    },
                    new OnQnAMatch()
                    {
                        Condition = "count(turn.recognized.answers[0].context.prompts) > 0",
                        Actions = new List<Dialog>()
                        {
                            new SetProperty()
                            {
                                Property = "dialog.qna.multiTurn.context",
                                Value = "turn.recognized.answers[0].context.prompts"
                            },
                            new TextInput()
                            {
                                Prompt = new ActivityTemplate("@{ShowMultiTurnAnswer()}"),
                                Property = "turn.qnaMultiTurnResponse",
                                AllowInterruptions = "false",
                            },
                            new SetProperty()
                            {
                                Property = "turn.qnaMatchFromContext",
                                Value = "where(dialog.qna.multiTurn.context, item, item.displayText == turn.qnaMultiTurnResponse)"
                            },
                            new IfCondition()
                            {
                                Condition = "turn.qnaMatchFromContext && count(turn.qnaMatchFromContext) > 0",
                                Actions = new List<Dialog>()
                                {
                                    new SetProperty()
                                    {
                                        Property = "turn.qnaId",
                                        Value = "turn.qnaMatchFromContext[0].qnaId"
                                    }
                                }
                            },
                            new EmitEvent()
                            {
                                EventName = AdaptiveEvents.ActivityReceived,
                                EventValue = "turn.activity",
                                BubbleEvent = true
                            }
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "Greeting",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I'm greeting you. LUIS recognizer won!"),
                            new SendActivity("Testing HTTp issue.."),
                            new CodeAction(GenerateHttpRequestBody),
                            new SendActivity("Back from code action... @{dialog.httpbody}"),
                            new SendActivity("URL:@{dialog.endpoint}/luis/api/v2.0/apps/@{dialog.appId}/settings"),
                            new HttpRequest()
                            {
                                Body = new JObject(
                                        new JProperty(
                                            "Name",
                                            "@{dialog.endpoint}"),
                                        new JProperty(
                                                "Properties",
                                                new JObject(
                                                    new JProperty(
                                                        "FavoriteAlbums",
                                                        new JArray(
                                                            new JObject(
                                                                new JProperty("Name", "@{dialog.endpoint}"),
                                                                new JProperty("Votes", "1"),
                                                                new JProperty("Users", null)))),
                                                    new JProperty(
                                                        "FavoriteSongs",
                                                        new JArray(
                                                            new JObject(
                                                                new JProperty("Name", "@{dialog.endpoint}"),
                                                                new JProperty("Votes", "1"),
                                                                new JProperty("Users", null)))),
                                                    new JProperty(
                                                        "Reviews",
                                                        new JArray(
                                                            new JValue("@{dialog.appId}"))))),
                                        new JProperty(
                                            "Votes", 
                                            "1"),
                                        new JProperty(
                                            "Submitter",
                                            "@{coalesce(dialog.appId, 'Anonymous')}")),
                                Url = "@{dialog.endpoint}/luis/api/v2.0/apps/@{dialog.appId}/settings",
                                Headers = new Dictionary<string, string>()
                                {
                                    { "Content-Type", "application/json" },
                                    { "Ocp-Apim-Subscription-Key", "@{dialog.key}" }
                                },
                                ResultProperty = "dialog.httpresult",
                                Method = HttpRequest.HttpMethod.PUT
                            },
                            new SendActivity("Http response = @{dialog.httpresult}")
                        }
                    },
                    new OnIntent()
                    {
                        Intent = "UserProfile",
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Let's get your user profile. LUIS recognizer won!")
                        }
                    },
                    new OnChooseIntent()
                    {
                        // This is nice so you can handle different ambiguous intents via different triggers
                        Intents = new List<string>()
                        {
                            "QnAMatch",
                            "Help"
                        },
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("Ambiguous! QnA and Help intent!"),
                            new SendActivity("[@{turn.recognized.intents.chooseIntent.QnAMatch.intents.QnAMatch.score}] Answer from KB: @{turn.recognized.intents.chooseIntent.QnAMatch.entities.answer[0]}"),
                            new SendActivity("[@{turn.recognized.intents.chooseIntent.Help.intents.Help.score}] LUIS intent: Help (there is no way to dynamically get this from recognizer result and needs to be hard coded)")
                            
                            //new SendActivity("@{renderDisambiguationChoices(turn.recognized.intents.ChooseIntent)}")
                        }
                    },
                    new OnUnknownIntent()
                    {
                        Actions = new List<Dialog>()
                        {
                            new SendActivity("I do not know how to do that!")
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

        private async Task<DialogTurnResult> GenerateHttpRequestBody(DialogContext dc, object options)
        {
            string artist = dc.GetState().GetValue<string>("dialog.artistName");
            string endpoint = "https://westus.api.cognitive.microsoft.com";
            string key = "a95d07785b374f0a9d7d40700e28a285";
            string appId = "fd0eefa3-4acd-4d1e-a18f-dae6a486d418";
            JObject body = new JObject(
                new JProperty("testproperty", "testvalue"),
                new JProperty(
                    "testobject",
                    new JObject(
                        new JProperty("child1", "value1"))));
            dc.GetState().SetValue("dialog.httpbody", body);
            dc.GetState().SetValue("dialog.endpoint", endpoint);
            dc.GetState().SetValue("dialog.appId", appId);
            dc.GetState().SetValue("dialog.key", key);
            var obj = dc.GetState().GetValue<object>("dialog.httpbody");
            return await dc.EndDialogAsync();
        }
    }
}
