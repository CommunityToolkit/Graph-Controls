// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Linq;
using Microsoft.Graph;
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
    [TemplatePart(Name = PeoplePickerTokenizingTextBoxName, Type = typeof(TokenizingTextBox))]
    public partial class PeoplePicker : Control
    {
        private const string PeoplePickerTokenizingTextBoxName = "PART_TokenizingTextBox";

        private TokenizingTextBox _tokenBox;

        private DispatcherTimer _typeTimer = new DispatcherTimer();

        /// <summary>
        /// Initializes a new instance of the <see cref="PeoplePicker"/> class.
        /// </summary>
        public PeoplePicker()
        {
            this.DefaultStyleKey = typeof(PeoplePicker);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_tokenBox != null)
            {
                _tokenBox.TextChanged -= TokenBox_TextChanged;
                _tokenBox.TokenItemAdded -= TokenBox_TokenItemAdded;
                _tokenBox.TokenItemCreating -= TokenBox_TokenItemCreating;
                _tokenBox.TokenItemRemoved -= TokenBox_TokenItemRemoved;
            }

            _tokenBox = GetTemplateChild(PeoplePickerTokenizingTextBoxName) as TokenizingTextBox;

            if (_tokenBox != null)
            {
                _tokenBox.TextChanged += TokenBox_TextChanged;
                _tokenBox.TokenItemAdded += TokenBox_TokenItemAdded;
                _tokenBox.TokenItemCreating += TokenBox_TokenItemCreating;
                _tokenBox.TokenItemRemoved += TokenBox_TokenItemRemoved;
            }
        }

        private async void TokenBox_TokenItemCreating(TokenizingTextBox sender, TokenItemCreatingEventArgs args)
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

        private void TokenBox_TokenItemAdded(TokenizingTextBox sender, TokenizingTextBoxItem args)
        {
            if (args?.Content is Person person)
            {
                PickedPeople.Add(person);
            }
        }

        private void TokenBox_TokenItemRemoved(TokenizingTextBox sender, TokenItemRemovedEventArgs args)
        {
            PickedPeople.Remove(args.Item as Person);
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
                _typeTimer.Debounce(
                    async () =>
                {
                    var graph = ProviderManager.Instance.GlobalProvider.Graph;
                    if (graph != null)
                    {
                        // If empty, clear out
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            SuggestedPeople.Clear();
                        }
                        else
                        {
                            SuggestedPeople.Clear();

                            foreach (var contact in (await graph.FindPersonAsync(text)).CurrentPage)
                            {
                                if (!PickedPeople.Any(person => person.Id == contact.Id))
                                {
                                    SuggestedPeople.Add(contact);
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
