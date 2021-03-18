// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Net.Authentication;
using CommunityToolkit.Net.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
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

        public IBaseRequestBuilder CalendarViewBuilder;
        public IBaseRequestBuilder MessagesBuilder;
        public IBaseRequestBuilder PlannerTasksBuilder;
        public IBaseRequestBuilder TeamsChannelMessagesBuilder;

        public MainPage()
        {
            this.InitializeComponent();

            ProviderManager.Instance.ProviderUpdated += this.OnProviderUpdated;
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            if (e.Reason == ProviderManagerChangedState.ProviderStateChanged 
                && sender is ProviderManager pm 
                && pm.GlobalProvider.State == ProviderState.SignedIn)
            {
                CalendarViewBuilder = ProviderManager.Instance.GlobalProvider.Graph().Me.CalendarView;
                MessagesBuilder = ProviderManager.Instance.GlobalProvider.Graph().Me.Messages;
                PlannerTasksBuilder = ProviderManager.Instance.GlobalProvider.Graph().Me.Planner.Tasks;
                TeamsChannelMessagesBuilder = ProviderManager.Instance.GlobalProvider.Graph().Teams["02bd9fd6-8f93-4758-87c3-1fb73740a315"].Channels["19:d0bba23c2fc8413991125a43a54cc30e@thread.skype"].Messages;
            }
            else
            {
                CalendarViewBuilder = null;
                MessagesBuilder = null;
                PlannerTasksBuilder = null;
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
    }
}
