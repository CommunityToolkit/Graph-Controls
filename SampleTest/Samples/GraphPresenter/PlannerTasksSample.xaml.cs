// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples.GraphPresenter
{
    public sealed partial class PlannerTasksSample : Page
    {
        public IBaseRequestBuilder PlannerTasksRequestBuilder;

        public PlannerTasksSample()
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

                PlannerTasksRequestBuilder = graphClient.Me.Planner.Tasks;
            }
            else
            {
                ClearRequestBuilders();
            }
        }

        private void ClearRequestBuilders()
        {
            PlannerTasksRequestBuilder = null;
        }

        public static bool IsTaskCompleted(int? percentCompleted)
        {
            return percentCompleted == 100;
        }
    }
}
