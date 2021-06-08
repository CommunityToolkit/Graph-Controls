// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using CommunityToolkit.Authentication;
using Microsoft.Toolkit.Uwp;
using Microsoft.Toolkit.Uwp.Helpers;
using Windows.System;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp
{
    /// <summary>
    /// A StateTrigger for detecting when the global authentication provider has been signed in.
    /// </summary>
    public class ProviderStateTrigger : StateTriggerBase
    {
        /// <summary>
        /// Identifies the <see cref="ActiveState"/> DependencyProperty.
        /// </summary>
        public static readonly DependencyProperty ActiveStateProperty =
            DependencyProperty.Register(nameof(ActiveState), typeof(ProviderState), typeof(ProviderStateTrigger), new PropertyMetadata(null, OnStateChanged));

        private static void OnStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is ProviderStateTrigger instance)
            {
                instance.UpdateState();
            }
        }

        private readonly DispatcherQueue _dispatcherQueue;

        /// <summary>
        /// Gets or sets the expected ProviderState.
        /// </summary>
        public ProviderState? ActiveState
        {
            get => (ProviderState?)GetValue(ActiveStateProperty);
            set => SetValue(ActiveStateProperty, value);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProviderStateTrigger"/> class.
        /// </summary>
        public ProviderStateTrigger()
        {
            _dispatcherQueue = DispatcherQueue.GetForCurrentThread();
            var weakEvent =
                new WeakEventListener<ProviderStateTrigger, object, ProviderUpdatedEventArgs>(this)
                {
                    OnEventAction = (instance, source, args) => OnProviderUpdated(source, args),
                    OnDetachAction = (weakEventListener) => ProviderManager.Instance.ProviderUpdated -= weakEventListener.OnEvent,
                };
            ProviderManager.Instance.ProviderUpdated += weakEvent.OnEvent;
            UpdateState();
        }

        private void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            _ = _dispatcherQueue.EnqueueAsync(UpdateState, DispatcherQueuePriority.Normal);
        }

        private void UpdateState()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (ActiveState != null && provider?.State != null)
            {
                SetActive(provider?.State == ActiveState);
            }
            else
            {
                SetActive(false);
            }
        }
    }
}