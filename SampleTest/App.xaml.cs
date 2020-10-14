// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.ApplicationModel;
using Microsoft.UI.Xaml;
using System;

namespace SampleTest
{
    sealed partial class App
    {
        private MainWindow m_window;

        public App()
        {
            WinRT.ComWrappersSupport.RegisterProjectionAssembly(typeof(App).Assembly);
            WinRT.ComWrappersSupport.RegisterProjectionAssembly(typeof(Microsoft.Xaml.Interactivity.Interaction).Assembly);
            WinRT.ComWrappersSupport.RegisterProjectionAssembly(typeof(Microsoft.Xaml.Interactions.Core.CallMethodAction).Assembly);

            foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                WinRT.ComWrappersSupport.RegisterProjectionAssembly(assembly);
            }

            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
        {
            m_window = new MainWindow();
            m_window.Activate();
        }

        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
        }
    }
}
