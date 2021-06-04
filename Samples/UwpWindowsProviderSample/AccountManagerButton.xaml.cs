using CommunityToolkit.Authentication;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace UwpWindowsProviderSample
{
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
