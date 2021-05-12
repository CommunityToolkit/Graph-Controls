// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using System.Windows;

namespace WpfMsalProviderSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ProviderManager.Instance.ProviderUpdated += this.OnProviderUpdated;
        }

        private async void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                SignedInUserTextBlock.Text = "Please sign in.";
            }
            else
            {
                SignedInUserTextBlock.Text = "Signed in as...";

                var graphClient = provider.GetClient();
                var me = await graphClient.Me.Request().GetAsync();

                SignedInUserTextBlock.Text = "Signed in as: " + me.DisplayName;
            }
        }
    }
}
