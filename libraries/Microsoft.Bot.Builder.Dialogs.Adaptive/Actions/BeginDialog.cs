﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Actions
{
    /// <summary>
    /// Action which begins executing another dialog, when it is done, it will return to the caller.
    /// </summary>
    public class BeginDialog : BaseInvokeDialog
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.BeginDialog";

        [JsonConstructor]
        public BeginDialog(string dialogIdToCall = null, object options = null, [CallerFilePath] string callerPath = "", [CallerLineNumber] int callerLine = 0)
            : base(dialogIdToCall, options)
        {
            this.RegisterSourceLocation(callerPath, callerLine);
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
        /// Gets or sets the property path to store the dialog result in.
        /// </summary>
        /// <value>
        /// The property path to store the dialog result in.
        /// </value>
        [JsonProperty("resultProperty")]
        public StringExpression ResultProperty { get; set; }

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

            var dialog = this.ResolveDialog(dc);

            // use bindingOptions to bind to the bound options
            var boundOptions = BindOptions(dc, options);

            // set the activity processed state (default is true)
            dcState.SetValue(TurnPath.ACTIVITYPROCESSED, this.ActivityProcessed);

            // start dialog with bound options passed in as the options
            return await dc.BeginDialogAsync(dialog.Id, options: boundOptions, cancellationToken: cancellationToken).ConfigureAwait(false);
        }

        public override async Task<DialogTurnResult> ResumeDialogAsync(DialogContext dc, DialogReason reason, object result = null, CancellationToken cancellationToken = default(CancellationToken))
        {
            var dcState = dc.GetState();

            if (this.ResultProperty != null)
            {
                dcState.SetValue(this.ResultProperty.GetValue(dcState), result);
            }

            // By default just end the current dialog and return result to parent.
            return await dc.EndDialogAsync(result, cancellationToken).ConfigureAwait(false);
        }
    }
}
