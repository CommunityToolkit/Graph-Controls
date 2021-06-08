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
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ManualGraphRequestSample
{
    public sealed partial class MainPage : Page
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
                case ProviderState.Loading:
                    SignInButton.Content = "Loading...";
                    SignInLoadingRing.IsActive = true;
                    SignInButton.IsEnabled = false;
                    break;

                case ProviderState.SignedOut:
                    SignInButton.Content = "Sign In";
                    SignInLoadingRing.IsActive = false;
                    SignInButton.IsEnabled = true;

                    TaskCollection.Clear();
                    break;

                case ProviderState.SignedIn:
                    SignInButton.Content = "Sign Out";
                    SignInLoadingRing.IsActive = false;
                    SignInButton.IsEnabled = true;

                    IList<TodoTask> tasks = await GetDefaultTaskListAsync();
                    if (tasks != null)
                    {
                        foreach (var task in tasks)
                        {
                            TaskCollection.Add(task);
                        }
                    }
                    break;
            }
        }

        private async void OnSignInButtonClick(object sender, RoutedEventArgs e)
        {
            IProvider provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State == ProviderState.SignedOut)
            {
                await ProviderManager.Instance.GlobalProvider.SignInAsync();
            }
            else if (provider?.State == ProviderState.SignedIn)
            {
                await ProviderManager.Instance.GlobalProvider.SignOutAsync();
            }
        }

        private async Task<IList<TodoTask>> GetDefaultTaskListAsync()
        {
            var httpClient = new HttpClient();
            var requestUri = "https://graph.microsoft.com/v1.0/me/todo/lists/tasks/tasks";

            var getRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
            await ProviderManager.Instance.GlobalProvider.AuthenticateRequestAsync(getRequest);

            using (httpClient)
            {
                var response = await httpClient.SendAsync(getRequest);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var jObject = JObject.Parse(jsonResponse);
                    if (jObject.ContainsKey("value"))
                    {
                        var tasks = JsonConvert.DeserializeObject<List<TodoTask>>(jObject["value"].ToString());
                        return tasks;
                    }
                }
            }

            return null;
        }
    }
}
