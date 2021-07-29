// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.UI;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.System;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.Graph.Uwp.Controls
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

            _typeTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        }

        private async void TokenBox_TokenItemTokenItemAdding(TokenizingTextBox sender, TokenItemAddingEventArgs args)
        {
            using (args.GetDeferral())
            {
                // Try and convert typed text to people
                var graph = ProviderManager.Instance.GlobalProvider.GetClient();
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
                _typeTimer.Debounce(
                async () =>
                {
                    var text = sender.Text;
                    await UpdateResultsAsync(text);

                    // TODO: If we don't have Graph connection and just list of Person should we auto-filter here?
                }, TimeSpan.FromSeconds(0.3));
            }
        }

        private async Task UpdateResultsAsync(string text)
        {
            var list = SuggestedItemsSource as IList;
            if (list == null)
            {
                return;
            }

            var graph = ProviderManager.Instance.GlobalProvider.GetClient();
            if (graph == null)
            {
                return;
            }

            // If empty, will clear out
            list.Clear();

            if (string.IsNullOrWhiteSpace(text))
            {
                return;
            }

            IGraphServiceUsersCollectionPage usersCollection = null;
            try
            {
                usersCollection = await graph.FindUserAsync(text);
            }
            catch
            {
                // No users found.
            }

            if (usersCollection != null)
            {
                foreach (var user in usersCollection.CurrentPage)
                {
                    // Exclude people in suggested list that we already have picked
                    if (!Items.Any(person => (person as Person)?.Id == user.Id))
                    {
                        list.Add(user.ToPerson());
                    }
                }
            }

            IUserPeopleCollectionPage peopleCollection = null;
            try
            {
                peopleCollection = await graph.FindPersonAsync(text);
            }
            catch
            {
                // No people found.
            }

            if (peopleCollection != null)
            {
                // Grab ids of current suggestions
                var ids = list.Cast<object>().Select(person => (person as Person).Id);

                foreach (var contact in peopleCollection.CurrentPage)
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
    }
}
