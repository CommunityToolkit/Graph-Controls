// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using System;
using System.Windows;
using System.Windows.Controls;

namespace WpfMsalProviderSample
{
    /// <summary>
    /// A simple button for triggering the globally configured IProvider to sign in and out.
    /// </summary>
    public partial class LoginButton : UserControl
    {
        public LoginButton()
        {
            InitializeComponent();

            ProviderManager.Instance.ProviderStateChanged += (s, e) => UpdateState();
            UpdateState();
        }

        private void UpdateState()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null || provider.State == ProviderState.Loading)
            {
                MyButton.Content = "Sign in";
                IsEnabled = false;
                return;
            }

            switch (provider.State)
            {
                case ProviderState.SignedIn:
                    MyButton.Content = "Sign out";
                    break;

                case ProviderState.SignedOut:
                    MyButton.Content = "Sign in";
                    break;
            }

            IsEnabled = true;
        }

        private void MyButton_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider != null)
                {
                    switch (provider.State)
                    {
                        case ProviderState.SignedOut:
                            provider.SignInAsync();
                            break;

                        case ProviderState.SignedIn:
                            provider.SignOutAsync();
                            break;
                    }
                }
            }));
        }
    }
}
