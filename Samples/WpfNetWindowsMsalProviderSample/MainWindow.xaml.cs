// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using System.Windows;

namespace WpfNetWindowsMsalProviderSample
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
        }

        private async void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            if (e.NewState == ProviderState.SignedIn)
            {
                SignedInUserTextBlock.Text = "Signed in as...";

                var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();
                var me = await graphClient.Me.Request().GetAsync();

                SignedInUserTextBlock.Text = "Signed in as: " + me.DisplayName;
            }
            else
            {
                SignedInUserTextBlock.Text = "Please sign in.";
            }
        }
    }
}
