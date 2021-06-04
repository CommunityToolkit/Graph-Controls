// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace UwpWindowsProviderSample
{
    /// <summary>
    /// A simple button for triggering the globally configured WindowsProvider to manage account.
    /// </summary>
    public sealed partial class AccountManagerButton : UserControl
    {
        public AccountManagerButton()
        {
            this.InitializeComponent();
        }

        private async void ManagerButton_Click(object sender, RoutedEventArgs e)
        {
            if(ProviderManager.Instance.GlobalProvider is WindowsProvider provider)
            {
                await provider.ShowAccountManagementPaneAsync();
            }
        }
    }
}
