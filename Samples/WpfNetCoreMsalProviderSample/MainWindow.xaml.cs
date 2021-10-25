﻿using System.Windows;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;

namespace WpfNetCoreMsalProviderSample
{
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
