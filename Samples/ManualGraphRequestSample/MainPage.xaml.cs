// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using ManualGraphRequestSample.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http;
using System.Threading.Tasks;

namespace ManualGraphRequestSample
{
    public sealed partial class MainPage : Windows.UI.Xaml.Controls.Page
    {
        public ObservableCollection<TodoTask> TaskCollection = new ObservableCollection<TodoTask>();

        public MainPage()
        {
            InitializeComponent();

            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
            ProviderManager.Instance.GlobalProvider = new WindowsProvider(new string[] { "User.Read", "Tasks.ReadWrite" });
        }

        private async void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            switch (e.NewState)
            {
                case ProviderState.SignedOut:
                    TaskCollection.Clear();
                    break;

                case ProviderState.SignedIn:
                    IList<TodoTask> tasks = await GetDefaultTaskListAsync();
                    if (tasks != default)
                    {
                        foreach (var task in tasks)
                        {
                            TaskCollection.Add(task);
                        }
                    }
                    break;
            }
        }

        private async Task<IList<TodoTask>> GetDefaultTaskListAsync()
        {
            return await GetResponseAsync<List<TodoTask>>("https://graph.microsoft.com/v1.0/me/todo/lists/tasks/tasks");
        }

        private async Task<T> GetResponseAsync<T>(string requestUri)
        {
            // Build the request
            HttpRequestMessage getRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

            // Authenticate the request
            await ProviderManager.Instance.GlobalProvider.AuthenticateRequestAsync(getRequest);

            var httpClient = new HttpClient();
            using (httpClient)
            {
                // Send the request
                var response = await httpClient.SendAsync(getRequest);

                if (response.IsSuccessStatusCode)
                {
                    // Handle the request response
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(jsonResponse);
                    if (jObject.ContainsKey("value"))
                    {
                        var result = JsonConvert.DeserializeObject<T>(jObject["value"].ToString());
                        return result;
                    }
                }
            }

            return default;
        }
    }
}
