﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace AdaptiveExpressions.Properties.Converters
{
    /// <summary>
    /// Converter which allows json to be expression to object or static object.
    /// </summary>
    public class ValueExpressionConverter : JsonConverter<ValueExpression>
    {
        public ValueExpressionConverter()
        {
        }

        public override bool CanRead => true;

        public override ValueExpression ReadJson(JsonReader reader, Type objectType, ValueExpression existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                return new ValueExpression((string)reader.Value);
            }
            else
            {
                return new ValueExpression(JToken.Load(reader));
            }
        }

        public override void WriteJson(JsonWriter writer, ValueExpression value, JsonSerializer serializer)
        {
            if (value.Expression != null)
            {
                serializer.Serialize(writer, value.ToString());
            }
            else
            {
                serializer.Serialize(writer, value.Value);
            }
        }
    }
}
