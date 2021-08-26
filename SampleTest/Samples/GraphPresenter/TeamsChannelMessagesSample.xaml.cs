// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples.GraphPresenter
{
    public sealed partial class TeamsChannelMessagesSample : Page
    {
        public IBaseRequestBuilder TeamsChannelMessagesRequestBuilder { get; set; }

        public TeamsChannelMessagesSample()
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

                TeamsChannelMessagesRequestBuilder = graphClient.Teams["02bd9fd6-8f93-4758-87c3-1fb73740a315"].Channels["19:d0bba23c2fc8413991125a43a54cc30e@thread.skype"].Messages;
            }
            else
            {
                ClearRequestBuilders();
            }
        }

        private void ClearRequestBuilders()
        {
            TeamsChannelMessagesRequestBuilder = null;
        }
    }
}
