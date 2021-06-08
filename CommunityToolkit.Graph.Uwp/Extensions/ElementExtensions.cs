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

        private static readonly DependencyProperty _isVisibleWhenProperty =
            DependencyProperty.RegisterAttached("IsVisibleWhen", typeof(ProviderState), typeof(FrameworkElement), new PropertyMetadata(null, OnIsVisibleWhenPropertyChanged));

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
        public static void SetIsVisibleWhen(FrameworkElement element, ProviderState value)
        {
            element.SetValue(_isVisibleWhenProperty, value);
        }

        /// <summary>
        /// Gets a value indicating whether an element should update its visibility based on provider state changes.
        /// </summary>
        /// <param name="element">The target element.</param>
        /// <returns>A nullable bool value.</returns>
        public static ProviderState GetIsVisibleWhen(FrameworkElement element)
        {
            return (ProviderState)element.GetValue(_isVisibleWhenProperty);
        }

        private static void OnIsVisibleWhenPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
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
            var isVisibleWhen = GetIsVisibleWhen(element);

            element.Visibility = isVisibleWhen == providerState ? Visibility.Visible : Visibility.Collapsed;
        }
    }
}
