// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using CommunityToolkit.Authentication;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp
{
    /// <summary>
    /// Extensions on FrameworkElement for responding to authentication changes from XAML.
    /// </summary>
    public class ElementExtensions : DependencyObject
    {
        private static readonly IList<FrameworkElement> _listeningElements = new List<FrameworkElement>();

        private static readonly DependencyProperty _isVisibleWhenSignedInProperty =
            DependencyProperty.RegisterAttached("IsVisibleWhenSignedIn", typeof(bool?), typeof(FrameworkElement), new PropertyMetadata(null, OnIsVisibleWhenSignedInPropertyChanged));

        static ElementExtensions()
        {
            _listeningElements = new List<FrameworkElement>();

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
        }

        /// <summary>
        /// Sets a value indicating whether an element should update its visibility based on provider state changes.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <param name="value">A nullable bool value.</param>
        public static void SetIsVisibleWhenSignedIn(FrameworkElement element, bool? value)
        {
            element.SetValue(_isVisibleWhenSignedInProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether an element should update its visibility based on provider state changes.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <returns>A nullable bool value.</returns>
        public static bool? GetIsVisibleWhenSignedIn(FrameworkElement element)
        {
            return (bool?)element.GetValue(_isVisibleWhenSignedInProperty);
        }

        private static void OnIsVisibleWhenSignedInPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (e.NewValue == null)
                {
                    DeregisterElement(element);
                }
                else
                {
                    RegisterElement(element);
                }

                var providerState = ProviderManager.Instance.GlobalProvider?.State;
                UpdateElementVisibility(element, providerState);
            }
        }

        private static void OnProviderUpdated(object sender, ProviderUpdatedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            var providerState = provider?.State;

            foreach (var element in _listeningElements)
            {
                UpdateElementVisibility(element, providerState);
            }
        }

        private static void OnElementUnloaded(object sender, RoutedEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                DeregisterElement(element);
            }
        }

        private static void RegisterElement(FrameworkElement element)
        {
            element.Unloaded += OnElementUnloaded;
            _listeningElements.Add(element);
        }

        private static void DeregisterElement(FrameworkElement element)
        {
            element.Unloaded -= OnElementUnloaded;
            _listeningElements.Remove(element);
        }

        private static void UpdateElementVisibility(FrameworkElement element, ProviderState? providerState)
        {
            var isVisibleWhenSignedIn = GetIsVisibleWhenSignedIn(element);

            switch (isVisibleWhenSignedIn)
            {
                case true:
                    // When signed in, show the element.
                    element.Visibility = providerState == ProviderState.SignedIn ? Visibility.Visible : Visibility.Collapsed;
                    break;

                case false:
                    // When signed in, hide the element.
                    element.Visibility = providerState == ProviderState.SignedIn ? Visibility.Collapsed : Visibility.Visible;
                    break;

                default:
                    // Show the default visibility state.
                    element.Visibility = default;
                    break;
            }
        }
    }
}
