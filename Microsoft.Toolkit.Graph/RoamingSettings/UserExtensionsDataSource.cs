using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;

namespace Microsoft.Toolkit.Graph.RoamingSettings
{
    public static class UserExtensionsDataSource
    {
        private static GraphServiceClient Graph => ProviderManager.Instance.GlobalProvider?.Graph;

        public static async Task<Extension> UpdateExtension(this Extension extension, string userId)
        {
            var updatedExtension = await Graph.Users[userId].Extensions[extension.Id].Request().UpdateAsync(extension);
            return updatedExtension;
        }

        public static async Task<Extension> GetExtension(string userId, string extensionId)
        {
            var extensions = await GetAllExtensions(userId);

            foreach (var ex in extensions)
            {
                if (ex.Id == extensionId)
                {
                    return ex;
                }
            }

            return null;
        }

        public static async Task<IList<Extension>> GetAllExtensions(string userId)
        {
            var extensions = await Graph.Users[userId].Extensions.Request().GetAsync();
            return extensions;
        }

        public static async Task<Extension> CreateExtension(string userId, string extensionId)
        {
            string requestUrl = Graph.Users[userId].Extensions.Request().RequestUrl;

            string json = "{" +
                    "\"@odata.type\": \"microsoft.graph.openTypeExtension\"," +
                    "\"extensionName\": \"" + extensionId + "\"," +
                "}";

            HttpRequestMessage hrm = new HttpRequestMessage(HttpMethod.Post, requestUrl);
            hrm.Content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
            await Graph.AuthenticationProvider.AuthenticateRequestAsync(hrm);
            HttpResponseMessage response = await Graph.HttpProvider.SendAsync(hrm);
            if (response.IsSuccessStatusCode)
            {
                // Deserialize into Extension object.
                var content = await response.Content.ReadAsStringAsync();
                var extension = Graph.HttpProvider.Serializer.DeserializeObject<Extension>(content);
                return extension;
            }

            return null;
        }

        public static async Task DeleteExtension(string userId, string extensionId)
        {
            await Graph.Users[userId].Extensions[extensionId].Request().DeleteAsync();
        }

        public static async Task<object> GetValue(this Extension extension, string key)
        {
            return extension.AdditionalData[key];
        }

        public static async Task SetValue(this Extension extension, string userId, string key, object value)
        {
            if (extension.AdditionalData == null)
            {
                extension.AdditionalData = new Dictionary<string, object>();
            }

            if (extension.AdditionalData.ContainsKey(key))
            {
                extension.AdditionalData[key] = value;
            }
            else
            {
                extension.AdditionalData.Add(key, value);
            }

            await UpdateExtension(extension, userId);
        }
    }
}
