﻿using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Generators;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Testing.Actions
{
    public class AssertCondition : Dialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Test.AssertCondition";
        
        [JsonConstructor]
        public AssertCondition([CallerFilePath] string path = "", [CallerLineNumber] int line = 0)
        {
            RegisterSourceLocation(path, line);
        }

        /// <summary>
        /// Gets or sets condition which must be true.
        /// </summary>
        /// <value>
        /// Condition which must be true.
        /// </value>
        [JsonProperty("condition")]
        public string Condition { get; set; }

        /// <summary>
        /// Gets or sets description of assertion.
        /// </summary>
        /// <value>
        /// Description of assertion.
        /// </value>
        [JsonProperty("description")]
        public string Description { get; set; }

        public async override Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default)
        {
            var dcState = dc.GetState();

            var (result, error) = new ExpressionEngine().Parse(Condition).TryEvaluate(dcState);
            if ((bool)result == false)
            {
                var desc = await new TemplateEngineLanguageGenerator()
                    .Generate(dc.Context, this.Description, dcState)
                    .ConfigureAwait(false);
                throw new Exception(desc);
            }

            return await dc.EndDialogAsync().ConfigureAwait(false);
        }
    }
}
