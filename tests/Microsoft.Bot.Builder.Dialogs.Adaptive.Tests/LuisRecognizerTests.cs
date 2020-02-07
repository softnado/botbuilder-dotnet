﻿// Licensed under the MIT License.
// Copyright (c) Microsoft Corporation. All rights reserved.

using System.Collections.Generic;
using System.Threading.Tasks;
using AdaptiveExpressions.Properties;
using AdaptiveExpressions.Properties.Converters;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Luis;
using Microsoft.Bot.Builder.Dialogs.Adaptive.Recognizers;
using Microsoft.Bot.Builder.MockLuis;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

namespace Microsoft.Bot.Builder.Dialogs.Adaptive.Tests
{
    [TestClass]
    public class LuisRecognizerTests
    {
        private const string DynamicListJSon = @"[
                {
                    'entity': 'alphaEntity',
                    'list': [
                        {
                            'canonicalForm': 'a',
                            'synonyms': [
                                'a',
                                'aa'
                            ]
                        },
                        {
                            'canonicalForm': 'b',
                            'synonyms': [
                                'b',
                                'bb'
                            ]
}
                    ]
                },
                {
                    'entity': 'numberEntity',
                    'list': [
                        {
                            'canonicalForm': '1',
                            'synonyms': [
                                '1',
                                'one'
                            ]
                        },
                        {
                            'canonicalForm': '2',
                            'synonyms': [
                                '2',
                                'two'
                            ]
                        }
                    ]
                }
            ]";

        private const string RecognizerJson = @"{
            '$kind': 'Microsoft.LuisRecognizer',
            'applicationId': '=settings.luis.DynamicLists_test_en-us_lu',
            'endpoint': '=settings.luis.endpoint',
            'endpointKey': '=settings.luis.endpointKey', 'dynamicLists': " + DynamicListJSon + "}";

        private readonly string dynamicListsDirectory = PathUtils.NormalizePath(@"..\..\..\..\..\tests\Microsoft.Bot.Builder.Dialogs.Adaptive.Tests\Tests\LuisRecognizerTests");

        [TestMethod]
        public async Task DynamicLists()
        {
            var config = new ConfigurationBuilder()
                .UseLuisSettings(dynamicListsDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }

        [TestMethod]
        public async Task DynamicListsExpression()
        {
            var config = new ConfigurationBuilder()
                .UseLuisSettings(dynamicListsDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }

        [TestMethod]
        public async Task ExternalEntities()
        {
            var config = new ConfigurationBuilder()
                .UseLuisSettings(dynamicListsDirectory, "TestBot")
                .Build();
            await TestUtils.RunTestScript(configuration: config);
        }

        [TestMethod]
        public void DeserializeDynamicList()
        {
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeSerializedDynamicList()
        {
            var ol = JsonConvert.DeserializeObject<List<DynamicList>>(DynamicListJSon);
            var json = JsonConvert.SerializeObject(ol);
            var dl = JsonConvert.DeserializeObject<List<DynamicList>>(json);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeArrayExpression()
        {
            var ae = JsonConvert.DeserializeObject<ArrayExpression<DynamicList>>(DynamicListJSon, new ArrayExpressionConverter<DynamicList>());
            var dl = ae.GetValue(null);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }

        [TestMethod]
        public void DeserializeLuisAdaptiveRecognizer()
        {
            var recognizer = JsonConvert.DeserializeObject<LuisAdaptiveRecognizer>(RecognizerJson, new ArrayExpressionConverter<DynamicList>());
            var dl = recognizer.DynamicLists.GetValue(null);
            Assert.AreEqual(2, dl.Count);
            Assert.AreEqual("alphaEntity", dl[0].Entity);
            Assert.AreEqual(2, dl[0].List.Count);
        }
    }
}
