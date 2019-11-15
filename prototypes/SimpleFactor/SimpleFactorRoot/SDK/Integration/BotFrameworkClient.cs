using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Bot.Schema;
using Newtonsoft.Json;

namespace SimpleFactorRoot.SDK
{
    public class BotFrameworkClient
    {
        private HttpClient _httpClient;

        public BotFrameworkClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<InvokeResponse> PostActivityAsync(Uri endpoint, Activity activity, AppCredentials appCredentials, CancellationToken cancellationToken = default)
        {
            // Get token for the skill call
            var token = await appCredentials.GetTokenAsync().ConfigureAwait(false);

            // TODO use SkillConversation class here instead of hard coded encoding...
            // Encode original bot service URL and ConversationId in the new conversation ID so we can unpack it later.
            // var skillConversation = new SkillConversation() { ServiceUrl = activity.ServiceUrl, ConversationId = activity.Conversation.Id };
            // activity.Conversation.Id = skillConversation.GetSkillConversationId()
            activity.Conversation.Id = Convert.ToBase64String(Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(new[]
            {
                activity.Conversation.Id,
                activity.ServiceUrl
            })));
            activity.ServiceUrl = endpoint.ToString();

            using (var jsonContent = new StringContent(JsonConvert.SerializeObject(activity, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore }), Encoding.UTF8, "application/json"))
            {
                using (var httpRequestMessage = new HttpRequestMessage())
                {
                    httpRequestMessage.Method = HttpMethod.Post;
                    httpRequestMessage.RequestUri = endpoint;
                    httpRequestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    httpRequestMessage.Content = jsonContent;

                    var response = await _httpClient.SendAsync(httpRequestMessage, cancellationToken).ConfigureAwait(false);

                    var content = await response.Content.ReadAsStringAsync().ConfigureAwait(false);

                    return new InvokeResponse
                    {
                        Status = (int)response.StatusCode,
                        Body = content.Length > 0 ? JsonConvert.DeserializeObject(content) : null
                    };
                }
            }
        }
    }
}
