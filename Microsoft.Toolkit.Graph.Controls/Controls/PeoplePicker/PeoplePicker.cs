// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Toolkit.Graph.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Microsoft.Toolkit.Uwp.UI.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

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
                _tokenBox.QueryTextChanged -= TokenBox_QueryTextChanged;
            }

            _tokenBox = GetTemplateChild(PeoplePickerTokenizingTextBoxName) as TokenizingTextBox;

            if (_tokenBox != null)
            {
                _tokenBox.QueryTextChanged += TokenBox_QueryTextChanged;
            }
        }

        private string _previousQuery;

        private void TokenBox_QueryTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
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
                                SuggestedPeople.Add(contact);
                            }
                        }

                        _previousQuery = text;
                    }
                }, TimeSpan.FromSeconds(0.3));
            }
        }
    }
}
