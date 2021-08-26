// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Graph.Extensions;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples.GraphPresenter
{
    public sealed partial class CalendarViewSample : Page
    {
        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime Today => DateTimeOffset.Now.Date.ToUniversalTime();

        // Workaround for https://github.com/microsoft/microsoft-ui-xaml/issues/2407
        public DateTime ThreeDaysFromNow => Today.AddDays(3);

        public IBaseRequestBuilder CalendarViewRequestBuilder { get; set; }

        public CalendarViewSample()
        {
            this.InitializeComponent();

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
        }

        private void OnProviderUpdated(object sender, IProvider provider)
        {
            if (provider == null)
            {
                ClearRequestBuilders();
            }
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            if (e.NewState == ProviderState.SignedIn)
            {
                var graphClient = ProviderManager.Instance.GlobalProvider.GetClient();

                CalendarViewRequestBuilder = graphClient.Me.CalendarView;
            }
            else
            {
                ClearRequestBuilders();
            }
        }

        private void ClearRequestBuilders()
        {
            CalendarViewRequestBuilder = null;
        }
    }
}
