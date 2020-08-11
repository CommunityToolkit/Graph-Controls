// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Microsoft.Toolkit.Graph.Providers;
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

        public MainPage()
        {
            this.InitializeComponent();
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
            return ProviderManager.Instance.GlobalProvider.Graph.Teams[team].Channels[channel].Messages;
        }
    }
}
