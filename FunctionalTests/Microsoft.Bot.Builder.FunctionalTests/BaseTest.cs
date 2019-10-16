// Copyright (c) Microsoft Corporation. All rights reserved.
// Licensed under the MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Bot.Builder.FunctionalTests.Configuration;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Microsoft.Rest;
using Moq;

namespace Microsoft.Bot.Builder.FunctionalTests
{
    /// <summary>
    /// WORKING WITH THESE TESTS
    ///   To run mocked and without recording:
    ///       1. Set HttpRecorderMode BaseTest.Mode to Playback
    ///
    ///   To run live or unmocked:
    ///       1. Set HttpRecorderMode to None
    ///       2. Set the following properties (all must be valid):
    ///           * ClientId (appId)
    ///           * ClientSecret (appPass)
    ///           * UserId (from slack)
    ///           * BotId (from slack)
    ///       3. Ensure the appId has Slack channel enabled and you've installed the bot in Slack
    ///
    ///    To re-record:
    ///      1. All from live/unmocked, except set HttpRecorderMode to Record.
    ///      2. Once done recording, copy recording sessions from ...Microsoft.Bot.Connector.Tests\bin\Debug\netcoreapp2.1\SessionRecords
    ///             to ...Microsoft.Bot.Connector.Tests\SessionRecords.
    /// </summary>
    public class BaseTest
    {
        private readonly string clientId;
        private readonly string clientSecret;
        private readonly string userId;
        private readonly string botId;

        public BaseTest()
        {
            clientId = EnvironmentConfig.TestAppId();
            clientSecret = EnvironmentConfig.TestAppPassword();
            userId = EnvironmentConfig.SlackUser();
            botId = EnvironmentConfig.SlackBot();

            Bot = new ChannelAccount() { Id = botId };
            User = new ChannelAccount() { Id = userId };
        }

        public ChannelAccount Bot { get; private set; }

        public ChannelAccount User { get; private set; }

        protected static Uri HostUri { get; set; } = new Uri("https://slack.botframework.com", UriKind.Absolute);

        #pragma warning disable 162
        public async Task AssertTracingFor(
            Func<Task> doTest,
            string tracedMethodName,
            bool isSuccesful = true,
            Func<HttpRequestMessage, bool> assertHttpRequestMessage = null)
        {
            tracedMethodName = tracedMethodName.EndsWith("Async") ? tracedMethodName.Remove(tracedMethodName.LastIndexOf("Async")) : tracedMethodName;

            var traceInterceptor = new Mock<IServiceClientTracingInterceptor>();
            var invocationIds = new List<string>();
            traceInterceptor.Setup(
                i => i.EnterMethod(It.IsAny<string>(), It.IsAny<object>(), tracedMethodName, It.IsAny<IDictionary<string, object>>()))
                .Callback((string id, object instance, string method, IDictionary<string, object> parameters) => invocationIds.Add(id));

            ServiceClientTracing.AddTracingInterceptor(traceInterceptor.Object);
            var wasTracingEnabled = ServiceClientTracing.IsEnabled;
            ServiceClientTracing.IsEnabled = true;

            await doTest();

            ServiceClientTracing.IsEnabled = wasTracingEnabled;

            var invocationId = invocationIds.Last();
            traceInterceptor.Verify(
                i => i.EnterMethod(invocationId, It.IsAny<object>(), tracedMethodName, It.IsAny<IDictionary<string, object>>()), Times.Once());
            traceInterceptor.Verify(
                i => i.SendRequest(invocationId, It.IsAny<HttpRequestMessage>()), Times.Once());
            traceInterceptor.Verify(
                i => i.ReceiveResponse(invocationId, It.IsAny<HttpResponseMessage>()), Times.Once());

            if (isSuccesful)
            {
                traceInterceptor.Verify(
                    i => i.ExitMethod(invocationId, It.IsAny<HttpOperationResponse>()), Times.Once());
            }
            else
            {
                traceInterceptor.Verify(
                    i => i.TraceError(invocationId, It.IsAny<Exception>()), Times.Once());
            }

            if (assertHttpRequestMessage != null)
            {
                traceInterceptor.Verify(
                    i => i.SendRequest(invocationId, It.Is<HttpRequestMessage>(h => assertHttpRequestMessage(h))), "HttpRequestMessage does not validate condition.");
            }
        }

        public async Task UseClientFor(Func<IConnectorClient, Task> doTest)
        {
            using var client = new ConnectorClient(HostUri, new MicrosoftAppCredentials(clientId, clientSecret));
            await doTest(client);
        }

        public async Task UseOAuthClientFor(Func<OAuthClient, Task> doTest)
        {
            using var oauthClient = new OAuthClient(HostUri, new MicrosoftAppCredentials(clientId, clientSecret));
            await doTest(oauthClient);
        }
    }
}
