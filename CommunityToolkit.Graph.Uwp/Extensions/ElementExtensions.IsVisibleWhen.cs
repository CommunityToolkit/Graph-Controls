// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Concurrent;
using CommunityToolkit.Authentication;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp
{
    /// <summary>
    /// IsVisibleWhen extension on FrameworkElement for decaring element visibility behavior in response to authentication changes.
    /// </summary>
    public static partial class ElementExtensions
    {
        private static readonly object _updateLock = new ();
        private static readonly ConcurrentDictionary<FrameworkElement, ProviderState> _listeningElements = new ();

        private static readonly DependencyProperty _isVisibleWhenProperty =
            DependencyProperty.RegisterAttached("IsVisibleWhen", typeof(ProviderState), typeof(FrameworkElement), new PropertyMetadata(null, OnIsVisibleWhenPropertyChanged));

        static ElementExtensions()
        {
            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
        }

        /// <summary>
        /// Sets a value indicating whether an element should update its visibility based on provider state changes.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <param name="value">The state in which to be visible.</param>
        public static void SetIsVisibleWhen(FrameworkElement element, ProviderState value)
        {
            element.SetValue(_isVisibleWhenProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether an element should update its visibility based on provider state changes.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <returns>The state in which to be visible.</returns>
        public static ProviderState GetIsVisibleWhen(FrameworkElement element)
        {
            return (ProviderState)element.GetValue(_isVisibleWhenProperty);
        }

        private static void OnIsVisibleWhenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (e.NewValue is ProviderState newState)
                {
                    RegisterElement(element, newState);
                }
                else
                {
                    DeregisterElement(element);
                }

                var providerState = ProviderManager.Instance.GlobalProvider?.State;
                UpdateElementVisibility(element, providerState);
            }
        }

        private static void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            lock (_updateLock)
            {
                var providerState = ProviderManager.Instance.GlobalProvider?.State;
                foreach (var kvp in _listeningElements)
                {
                    UpdateElementVisibility(kvp.Key, providerState);
                }
            }
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                DeregisterElement(element);
            }
        }

        private static void RegisterElement(FrameworkElement element, ProviderState providerState)
        {
            element.Unloaded += OnElementUnloaded;
            _listeningElements.TryAdd(element, providerState);
        }

        private static void DeregisterElement(FrameworkElement element)
        {
            element.Unloaded -= OnElementUnloaded;
            _listeningElements.TryRemove(element, out ProviderState providerState);
        }

        private static void UpdateElementVisibility(FrameworkElement element, ProviderState? state)
        {
            var isVisibleWhen = GetIsVisibleWhen(element);

            element.Visibility = isVisibleWhen == state ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
