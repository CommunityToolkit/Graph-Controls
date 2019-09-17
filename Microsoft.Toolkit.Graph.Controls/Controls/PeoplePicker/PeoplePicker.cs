using Microsoft.Graph;
using Microsoft.Toolkit.Graph;
using Microsoft.Toolkit.Graph.Helpers;
using Microsoft.Toolkit.Uwp.UI.Controls.Graph.Helpers;
using System;
using System.Collections.ObjectModel;
using TokenListBoxSample.Controls;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Templated Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234235

namespace Microsoft.Toolkit.Uwp.UI.Controls.Graph
{
    [TemplatePart(Name = PeoplePickerTokenListBoxName, Type = typeof(TokenListBox))]
    public partial class PeoplePicker : Control
    {
        private const string PeoplePickerTokenListBoxName = "PART_TokenListBox";

        private TokenListBox _tokenBox;

        private DispatcherTimer _typeTimer = new DispatcherTimer();

        public ObservableCollection<Person> SuggestedPeople
        {
            get { return (ObservableCollection<Person>)GetValue(SuggestedPeopleProperty); }
            set { SetValue(SuggestedPeopleProperty, value); }
        }

        // Using a DependencyProperty as the backing store for SuggestedPeople.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty SuggestedPeopleProperty =
            DependencyProperty.Register(nameof(SuggestedPeople), typeof(ObservableCollection<Person>), typeof(PeoplePicker), new PropertyMetadata(new ObservableCollection<Person>()));

        public PeoplePicker()
        {
            this.DefaultStyleKey = typeof(PeoplePicker);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_tokenBox != null)
            {
                _tokenBox.QueryTextChanged -= _tokenBox_QueryTextChanged;
            }

            _tokenBox = GetTemplateChild(PeoplePickerTokenListBoxName) as TokenListBox;

            if (_tokenBox != null)
            {
                _tokenBox.QueryTextChanged += _tokenBox_QueryTextChanged;
            }
        }

        private string _previousQuery;

        private void _tokenBox_QueryTextChanged(AutoSuggestBox sender, AutoSuggestBoxTextChangedEventArgs args)
        {
            if (!args.CheckCurrent())
            {
                return;
            }

            if (args.Reason == AutoSuggestionBoxTextChangeReason.UserInput)
            {
                var text = sender.Text;
                _typeTimer.Debounce(async () =>
                {
                    var graph = GlobalProvider.Instance.Graph;
                    if (graph != null)
                    {
                        // If empty, clear out
                        if (string.IsNullOrWhiteSpace(text))
                        {
                            SuggestedPeople.Clear();
                        }
                        // If we are typing, continue to reduce the set from current list
                        //else if (!string.IsNullOrWhiteSpace(_previousQuery) && text.StartsWith(_previousQuery))
                        //{
                        //    int count = SuggestedPeople.Count;
                        //    for (var i = count - 1; i >= 0; i--)
                        //    {
                        //        var contact = SuggestedPeople[i];
                        //        if (!contact.DisplayName.Contains(text, StringComparison.InvariantCultureIgnoreCase))
                        //        {
                        //            SuggestedPeople.Remove(contact);
                        //        }
                        //    }
                        //}
                        // Else, filter all contacts
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
