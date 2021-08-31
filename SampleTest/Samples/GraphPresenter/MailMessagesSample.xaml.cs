// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Text.RegularExpressions;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples.GraphPresenter
{
    public sealed partial class MailMessagesSample : Page
    {
        public IBaseRequestBuilder MessagesRequestBuilder { get; set; }

        public MailMessagesSample()
        {
            this.InitializeComponent();
            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
        }

        private void OnProviderUpdated(object sender, IProvider provider)
        {
            if (provider == null)
            {
                ClearRequestBuilders();
            }
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            if (e.NewState == ProviderState.SignedIn)
            {
                var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();

                MessagesRequestBuilder = graphClient.Me.Messages;
            }
            else
            {
                ClearRequestBuilders();
            }
        }

        private void ClearRequestBuilders()
        {
            MessagesRequestBuilder = null;
        }

        public static string RemoveWhitespace(string value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return Regex.Replace(value, @"\t|\r|\n", " ");
        }
    }
}
