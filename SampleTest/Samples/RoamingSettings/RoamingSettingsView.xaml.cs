// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples
{
    /// <summary>
    /// A sample for demonstrating features in the RoamingSettings namespace.
    /// </summary>
    public sealed partial class RoamingSettingsView : Page
    {
        private RoamingSettingsViewModel _vm => DataContext as RoamingSettingsViewModel;

        public RoamingSettingsView()
        {
            InitializeComponent();
            DataContext = new RoamingSettingsViewModel();
        }

        private void CreateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.CreateCustomRoamingSettings();
        }

        private void DeleteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.DeleteCustomRoamingSettings();
        }

        private void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.SetValue();
        }

        private void GetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.GetValue();
        }

        private void ViewButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            _vm.SyncRoamingSettings();
        }

        private void AdditionalData_ItemClick(object sender, ItemClickEventArgs e)
        {
            if (e.ClickedItem is KeyValuePair<string, object> kvp)
            {
                _vm.KeyInputText = kvp.Key;
                _vm.ValueInputText = kvp.Value.ToString();
            }
        }
    }
}
