using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Toolkit.Graph.Helpers
{
    public static class RoamingSettingsHelper
    {
        private static GraphServiceClient _graph => ProviderManager.Instance.GlobalProvider?.Graph;

        static RoamingSettingsHelper()
        {

        }

        public static async Task<T> Get<T>(this User user, string key)
        {
            try
            {
                string appName = "com.contoso";
                string extensionName = string.Format("{0}.roamingSettings", appName);
                string requestUrl = _graph.Users[user.Id].Request().RequestUrl + "extensions";

                var message = new HttpRequestMessage(HttpMethod.Get, requestUrl);

                await _graph.AuthenticationProvider.AuthenticateRequestAsync(message);

                HttpResponseMessage response = await _graph.HttpProvider.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var updatedUser = _graph.HttpProvider.Serializer.DeserializeObject<User>(content);
                    //return updatedUser;
                }
            }
            catch
            {

            }

            return default(T);
        }

        public static async Task<User> Update(this User user, string key, object value)
        {
            try
            {
                string appName = "com.contoso";
                string extensionName = string.Format("{0}.roamingSettings", appName);
                string requestUrl = _graph.Users[user.Id].Request().RequestUrl + "extensions";
                string json = "{" +
                        "\"@odata.type\" : \"microsoft.graph.openTypeExtension\"" +
                        "\"extensionName\" : \"" + extensionName + "\"" +
                        "\"" + key + "\" : \"" + value + "\"" +
                    "}";

                var message = new HttpRequestMessage(HttpMethod.Post, requestUrl)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json"),
                };

                await _graph.AuthenticationProvider.AuthenticateRequestAsync(message);

                HttpResponseMessage response = await _graph.HttpProvider.SendAsync(message);
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var updatedUser = _graph.HttpProvider.Serializer.DeserializeObject<User>(content);
                    return updatedUser;
                }
            }
            catch
            {

            }

            return null;
        }
    }
}
