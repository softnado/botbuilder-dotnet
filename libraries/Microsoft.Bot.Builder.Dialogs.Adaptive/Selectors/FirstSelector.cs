﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AdaptiveExpressions;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Conditions;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Selectors
{
    /// <summary>
    /// Select the first ordered by priority true OnCondition.
    /// </summary>
    public class FirstSelector : ITriggerSelector
    {
        [JsonProperty("$kind")]
        public const string DeclarativeType = "Microsoft.FirstSelector";

        private List<OnCondition> _conditionals;
        private bool _evaluate;

        [Newtonsoft.Json.JsonIgnore]
        public IExpressionParser Parser { get; set; } = new ExpressionEngine();

        public void Initialize(IEnumerable<OnCondition> conditionals, bool evaluate)
        {
            _conditionals = conditionals.ToList();
            _evaluate = evaluate;
        }

        public Task<IReadOnlyList<OnCondition>> Select(SequenceContext context, CancellationToken cancel)
        {
            OnCondition selection = null;
            var lowestPriority = int.MaxValue;
            if (_evaluate)
            {
                for (var i = 0; i < _conditionals.Count; i++)
                {
                    var conditional = _conditionals[i];
                    var expression = conditional.GetExpression(Parser);
                    var (value, error) = expression.TryEvaluate(context.GetState());
                    var eval = error == null && (bool)value;
                    if (eval == true)
                    {
                        var priority = conditional.CurrentPriority(context);
                        if (priority >= 0 && priority < lowestPriority)
                        {
                            selection = conditional;
                            lowestPriority = priority;
                        }
                    }
                }
            }
            else
            {
                foreach (var conditional in _conditionals)
                {
                    var priority = conditional.CurrentPriority(context);
                    if (priority >= 0 && priority < lowestPriority)
                    {
                        selection = conditional;
                        lowestPriority = priority;
                    }
                }
            }

            var result = new List<OnCondition>();
            if (selection != null)
            {
                result.Add(selection);
            }

            return Task.FromResult((IReadOnlyList<OnCondition>)result);
        }
    }
}
