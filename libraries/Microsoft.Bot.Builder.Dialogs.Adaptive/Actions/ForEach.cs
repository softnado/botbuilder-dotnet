﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Executes a set of actions once for each item in an in-memory list or collection.
    /// </summary>
    public class Foreach : ActionScope
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.Foreach";

        private const string INDEX = "dialog.foreach.index";
        private const string VALUE = "dialog.foreach.value";

        [JsonConstructor]
        public Foreach([CallerFilePath] string sourceFilePath = "", [CallerLineNumber] int sourceLineNumber = 0)
            : base()
        {
            this.RegisterSourceLocation(sourceFilePath, sourceLineNumber);
        }

        /// <summary>
        /// Gets or sets an optional expression which if is true will disable this action.
        /// </summary>
        /// <example>
        /// "user.age > 18".
        /// </example>
        /// <value>
        /// A boolean expression. 
        /// </value>
        [JsonProperty("disabled")]
        public BoolExpression Disabled { get; set; } 

        /// <summary>
        /// Gets or sets property path expression to the collection of items.
        /// </summary>
        /// <value>
        /// Property path expression to the collection of items.
        /// </value>
        [JsonProperty("itemsProperty")]
        public StringExpression ItemsProperty { get; set; }

        public override async Task<DialogTurnResult> BeginDialogAsync(DialogContext dc, object options = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (options is CancellationToken)
            {
                throw new ArgumentException($"{nameof(options)} cannot be a cancellation token");
            }

            var dcState = dc.GetState();

            if (this.Disabled != null && this.Disabled.GetValue(dcState) == true)
            {
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }

            dcState.SetValue(INDEX, -1);
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnBreakLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnContinueLoopAsync(DialogContext dc, ActionScopeResult actionScopeResult, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected override async Task<DialogTurnResult> OnEndOfActionsAsync(DialogContext dc, object result = null, CancellationToken cancellationToken = default)
        {
            return await this.NextItemAsync(dc, cancellationToken).ConfigureAwait(false);
        }

        protected virtual async Task<DialogTurnResult> NextItemAsync(DialogContext dc, CancellationToken cancellationToken = default)
        {
            // Get list information
            var dcState = dc.GetState();
            var list = dcState.GetValue<JArray>(this.ItemsProperty.GetValue(dcState));
            var index = dcState.GetIntValue(INDEX);

            // Next item
            if (++index < list.Count)
            {
                // Persist index and value
                dcState.SetValue(VALUE, list[index]);
                dcState.SetValue(INDEX, index);

                // Start loop
                return await this.BeginActionAsync(dc, 0, cancellationToken).ConfigureAwait(false);
            }
            else
            {
                // End of list has been reached
                return await dc.EndDialogAsync(cancellationToken: cancellationToken).ConfigureAwait(false);
            }
        }

        protected override string OnComputeId()
        {
            return $"{this.GetType().Name}({this.ItemsProperty})";
        }
    }
}
