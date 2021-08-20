// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using CommunityToolkit.Authentication;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace SampleTest
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        // List of available authentication providers.
        private enum ProviderType
        {
            Mock,
            Msal,
            Windows
        }

        // Which provider should be used for authentication?
        private readonly ProviderType _providerType = ProviderType.Mock;

        /// <summary>
        /// Initialize the global authentication provider.
        /// </summary>
        private void InitializeGlobalProvider()
        {
            if (ProviderManager.Instance.GlobalProvider != null)
            {
                return;
            }

            // Provider config
            string clientId = "YOUR_CLIENT_ID_HERE";
            string[] scopes = { "User.Read", "User.ReadBasic.All", "People.Read", "Calendars.Read", "Mail.Read", "Group.Read.All", "ChannelMessage.Read.All" };
            bool autoSignIn = true;

            switch (_providerType)
            {
                // Mock provider
                case ProviderType.Mock:
                    ProviderManager.Instance.GlobalProvider = new MockProvider(signedIn: autoSignIn);
                    break;

                // Msal provider
                case ProviderType.Msal:
                    ProviderManager.Instance.GlobalProvider = new MsalProvider(clientId: clientId, scopes: scopes, autoSignIn: autoSignIn);
                    break;

                // Windows provider
                case ProviderType.Windows:
                    var webAccountProviderConfig = new WebAccountProviderConfig(WebAccountProviderType.Msa, clientId);
                    ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes, webAccountProviderConfig: webAccountProviderConfig, autoSignIn: autoSignIn);
                    break;
            }
        }

        /// <summary>
        /// Invoked when the application is launched normally by the end user.  Other entry points
        /// will be used such as when the application is launched to open a specific file.
        /// </summary>
        /// <param name="e">Details about the launch request and process.</param>
        protected override void OnLaunched(LaunchActivatedEventArgs e)
        {
            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                Window.Current.Content = rootFrame;
            }

            if (e.PrelaunchActivated == false)
            {
                if (rootFrame.Content == null)
                {
                    rootFrame.Navigate(typeof(MainPage), e.Arguments);
                }

                Window.Current.Activate();

                InitializeGlobalProvider();
            }
        }
    }
}
