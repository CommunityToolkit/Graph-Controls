// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Graph;
using Microsoft.System;
using Microsoft.Toolkit.Graph.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Control which allows user to search for a person or contact within Microsoft Graph. Built on top of <see cref="TokenizingTextBox"/>.
    /// </summary>
    public partial class PeoplePicker : TokenizingTextBox
    {
        private DispatcherQueueTimer _typeTimer = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PeoplePicker"/> class.
        /// </summary>
        public PeoplePicker()
        {
            this.DefaultStyleKey = typeof(PeoplePicker);

            SuggestedItemsSource = new ObservableCollection<Person>();

            TextChanged += TokenBox_TextChanged;
            TokenItemAdding += TokenBox_TokenItemTokenItemAdding;
        }

        private async void TokenBox_TokenItemTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            using (args.GetDeferral())
            {
                // Try and convert typed text to people
                var graph = ProviderManager.Instance.GlobalProvider.Graph;
                if (graph != null)
                {
                    args.Item = (await graph.FindPersonAsync(args.TokenText)).CurrentPage.FirstOrDefault();
                }

                // If we didn't find anyone, then don't add anyone.
                if (args.Item == null)
                {
                    args.Cancel = true;

                    // TODO: We should raise a 'person not found' type event or automatically display some feedback?
                }
            }
        }

        private void TokenBox_TextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (!args.CheckCurrent())
            {
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var text = sender.Text;
                var list = SuggestedItemsSource as IList;

                if (list != null)
                {
                    if (_typeTimer == null)
                    {
                        _typeTimer = DispatcherQueue.CreateTimer();
                    }

                    _typeTimer.Debounce(
                    async () =>
                    {
                        var graph = ProviderManager.Instance.GlobalProvider.Graph;
                        if (graph != null)
                        {
                            // If empty, will clear out
                            list.Clear();

                            if (!string.IsNullOrWhiteSpace(text))
                            {
                                foreach (var user in (await graph.FindUserAsync(text)).CurrentPage)
                                {
                                    // Exclude people in suggested list that we already have picked
                                    if (!Items.Any(person => (person as Person)?.Id == user.Id))
                                    {
                                        list.Add(user.ToPerson());
                                    }
                                }

                                // Grab ids of current suggestions
                                var ids = list.Cast<object>().Select(person => (person as Person).Id);

                                foreach (var contact in (await graph.FindPersonAsync(text)).CurrentPage)
                                {
                                    // Exclude people in suggested list that we already have picked
                                    // Or already suggested
                                    if (!Items.Any(person => (person as Person)?.Id == contact.Id) &&
                                        !ids.Any(id => id == contact.Id))
                                    {
                                        list.Add(contact);
                                    }
                                }
                            }
                        }

                        // TODO: If we don't have Graph connection and just list of Person should we auto-filter here?
                    }, TimeSpan.FromSeconds(0.3));
                }
            }
        }
    }
}
