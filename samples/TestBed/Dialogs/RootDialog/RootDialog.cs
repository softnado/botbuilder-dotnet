using System.IO;
using System.Collections.Generic;
using Microsoft.Bot.Builder.Dialogs;
using Microsoft.Bot.Builder.Dialogs.Adaptive;
using Microsoft.Bot.Builder.LanguageGeneration;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Input;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Actions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Templates;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.AI.Luis;

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

// ## 1.  Basic multi-turn conversation

// Basic scenario of multi-input form flow. Here the user answers the questions directly in all cases.

// | Who?  | Message                                                   |
// |------:|:----------------------------------------------------------|
// |User:  | Hi                                                        |
// |Bot:   | Hello, I'm the demo bot. What is your name?               |
// |User:  | vishwac                                                   |
// |Bot:   | Hello, I have your name as 'vishwac'                      |
// |Bot:   | What is your age?                                         |
// |User:  | I'm 36                                                    |
// |Bot:   | Thank you. I have your age as 36                          |    
var rootDialog = new AdaptiveDialog(nameof(AdaptiveDialog))
{
    Generator = new TemplateEngineLanguageGenerator(),
    Recognizer = new LuisRecognizer(GetLUISApp()),
    Triggers = new List<OnCondition>()
    {
        new OnConversationUpdateActivity()
        {
            Actions = WelcomeUserAction()
        },
        new OnIntent() {
            Intent = "GetUserProfile",
            Actions = new List<Dialog>() {
                new TextInput() {
                    Prompt = new ActivityTemplate("Hello, I'm the demo bot. What is your name?"),
                    Validations = new List<string>() {
                        "count(this.value) >= 2",
                        "count(this.value) <= 150"
                    },
                    InvalidPrompt = new ActivityTemplate("Sorry, '{this.value}' does not work. I'm looking for 2-150 characters. What is your name?"),
                    Property = "$userName",
                    MaxTurnCount = 3,
                    DefaultValue = "'Human'",
                    DefaultValueResponse = new ActivityTemplate("Sorry, I'm not getting it. For now, I'll set your name to '{%DefaultValue}'."),
                    Value = "@personName"
                },
                new SendActivity() {
                    Activity = new ActivityTemplate("Hello, I have your name as '{$userName}'")
                },
                new NumberInput() {
                    Prompt = new ActivityTemplate("What is your age?"),
                    Validations = new List<string>() {
                        "int(this.value) >= 1",
                        "int(this.value) <= 100"
                    },
                    InvalidPrompt = new ActivityTemplate("Sorry, '{this.value}' does not work. I'm looking for 1-150. What is your age?"),
                    Property = "$userAge",
                    UnrecognizedPrompt = new ActivityTemplate("Sorry, I do not recognize '{this.value}'. What is your age?"),
                    MaxTurnCount = 3,
                    DefaultValue = "30",
                    DefaultValueResponse = new ActivityTemplate("Sorry, I'm not getting it. For now, I'll set your age to '{%DefaultValue}'."),
                    Value = "@age"
                },
                new SendActivity() {
                    Activity = new ActivityTemplate("Thank you. I have your age as '{$userAge}'")
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
        private static LuisApplication GetLUISApp() {
            return new LuisApplication() {
                ApplicationId = "822278ff-e172-4e87-931f-d8bd5f40163e",
                EndpointKey = "a95d07785b374f0a9d7d40700e28a285",
                Endpoint = "https://westus.api.cognitive.microsoft.com/luis/api/v2.0"
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
                                new SendActivity("[WelcomeUser]")
                            }
                        }
                    }
                }
            };

        }
    }
}
