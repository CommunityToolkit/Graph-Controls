// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using System.Diagnostics;
using Windows.ApplicationModel.Activation;
using Windows.System;
using Windows.UI.ApplicationSettings;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpWindowsProviderSample
{
    sealed partial class App : Application
    {
        public App()
        {
            this.InitializeComponent();
        }

        /// <summary>
        /// Sets the global IProvider instance for authentication using native Windows Account Manager APIs.
        /// 
        /// Don't forget to associate your app with the store as well. The WindowsProvider leverages the 
        /// default app registration that comes with store assocation, no clientId required!
        /// 
        /// For now only consumer MSA accounts supported, however organizational AAD account support is planned and coming soon.
        /// </summary>
        void ConfigureGlobalProvider()
        {
            if (ProviderManager.Instance.GlobalProvider == null)
            {
                string[] scopes = new string[] { "User.Read" };
                var paneConfig = GetAccountsSettingsPaneConfig();
                ProviderManager.Instance.GlobalProvider = new WindowsProvider(scopes, accountsSettingsPaneConfig: paneConfig);
            }
        }

        AccountsSettingsPaneConfig GetAccountsSettingsPaneConfig()
        {
            void OnAccountCommandInvoked(WebAccountCommand command, WebAccountInvokedArgs args)
            {
                Debug.WriteLine($"Action: {args.Action}");
            }

            var accountCommandParameter = new WebAccountCommandParameter(
                OnAccountCommandInvoked,
                SupportedWebAccountActions.Manage | SupportedWebAccountActions.Remove);

            var addAccountHeaderText = "Login account";
            var manageAccountHeaderText = "Account management";

            return new AccountsSettingsPaneConfig(addAccountHeaderText, manageAccountHeaderText, accountCommandParameter: accountCommandParameter);
        }

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

                ConfigureGlobalProvider();
            }
        }
    }
}
