// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Graph.RoamingSettings;
using System;
using System.Text.RegularExpressions;
using Windows.UI.Xaml.Controls;

namespace SampleTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime Today => DateTimeOffset.Now.Date.ToUniversalTime();

        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime ThreeDaysFromNow => Today.AddDays(3);

        private UserExtensionBase _roamingSettings = null;

        public MainPage()
        {
            this.InitializeComponent();

            ProviderManager.Instance.ProviderUpdated += (s, e) => this.OnGlobalProviderUpdated();
            OnGlobalProviderUpdated();
        }

        private async void OnGlobalProviderUpdated()
        {
            var globalProvider = ProviderManager.Instance.GlobalProvider;
            if (globalProvider != null && globalProvider.State == ProviderState.SignedIn)
            {
                var me = await globalProvider.Graph.Me.Request().GetAsync();
                _roamingSettings = new CustomRoamingSettings(me.Id);
            }
            else
            {
                _roamingSettings = null;
            }
        }

        public static string ToLocalTime(DateTimeTimeZone value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
            return value.ToDateTimeOffset().LocalDateTime.ToString("g");
        }

        public static string ToLocalTime(DateTimeOffset? value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return value?.LocalDateTime.ToString("g");
        }

        public static string RemoveWhitespace(string value)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2654
            return Regex.Replace(value, @"\t|\r|\n", " ");
        }

        public static bool IsTaskCompleted(int? percentCompleted)
        {
            return percentCompleted == 100;
        }

        public static IBaseRequestBuilder GetTeamsChannelMessagesBuilder(string team, string channel)
        {
            // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/3064
            return null;// ProviderManager.Instance.GlobalProvider.Graph.Teams[team].Channels[channel].Messages;
        }
        /*
        private async void GetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            GetValueTextBlock.Text = "";

            try
            {
                string key = GetKeyTextBox.Text;

                string value = await _roamingSettings.Get<string>(key);

                GetValueTextBlock.Text = value;
            }
            catch
            {
                GetValueTextBlock.Text = "Failure";
            }
        }

        private async void SetButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            SetResponseTextBlock.Text = "";

            string key = SetKeyTextBox.Text;
            string value = SetValueTextBox.Text;

            await _roamingSettings.Set(key, value);

            SetResponseTextBlock.Text = "Success!";
        }

        private async void CreateButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            CreateResponseTextBlock.Text = "";

            await _roamingSettings.Create();

            CreateResponseTextBlock.Text = "Success!";
        }

        private async void DeleteButton_Click(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            DeleteResponseTextBlock.Text = "";

            await _roamingSettings.Delete();

            DeleteResponseTextBlock.Text = "Success!";
        }*/
    }
}
