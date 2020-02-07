﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using AdaptiveExpressions.TriggerTrees;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the most specific true rule implementation of <see cref="ITriggerSelector"/>.
    /// </summary>
    public class MostSpecificSelector : ITriggerSelector
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.MostSpecificSelector";

        private readonly TriggerTree _tree = new TriggerTree();

        /// <summary>
        /// Gets or sets optional rule selector to use when more than one most specific rule is true.
        /// </summary>
        /// <value>
        /// Optional rule selector to use when more than one most specific rule is true.
        /// </value>
        [JsonProperty("selector")]
        public ITriggerSelector Selector { get; set; }

        [Newtonsoft.Json.JsonIgnore]
        public IExpressionParser Parser { get; set; } = new ExpressionEngine(TriggerTree.LookupFunction);

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            foreach (var conditional in conditionals)
            {
                _tree.AddTrigger(conditional.GetExpression(Parser), conditional);
            }
        }

        public virtual async Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel)
        {
            var triggers = _tree.Matches(context.GetState());
            var matches = new List<OnCondition>();
            foreach (var trigger in triggers)
            {
                matches.Add(trigger.Action as OnCondition);
            }

            IReadOnlyList<OnCondition> selections = matches;
            if (Selector != null)
            {
                Selector.Initialize(matches, false);
                selections = await Selector.Select(context, cancel).ConfigureAwait(false);
            }

            return selections;
        }
    }
}
