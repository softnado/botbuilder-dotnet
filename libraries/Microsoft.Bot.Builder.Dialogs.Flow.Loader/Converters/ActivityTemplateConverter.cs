﻿// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using Microsoft.Bot.Builder.Dialogs.Flow.Loader.Parsers;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.Bot.Builder.Dialogs.Flow.Loader.Converters
{
    public class ActivityTemplateConverter : JsonConverter
    {
        public override bool CanRead => true;

        public override bool CanConvert(Type objectType)
        {
            return typeof(IActivityTemplate) == objectType;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.ValueType == typeof(string))
            {
                // inline template Example: "Hello {name}"
                string readerValue = reader.Value.ToString();
                return new ActivityTemplate((string)readerValue);
            }
            else
            {
                var activity = JToken.Load(reader).ToObject<Activity>();
                return new ActivityTemplate((Activity)activity);
            }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            serializer.Serialize(writer, value);
        }
    }
}