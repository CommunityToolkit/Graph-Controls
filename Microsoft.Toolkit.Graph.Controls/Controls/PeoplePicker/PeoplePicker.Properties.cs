// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp.UI.Controls;
using Windows.UI.Xaml;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Control which allows user to search for a person or contact within Microsoft Graph. Built on top of <see cref="TokenizingTextBox"/>.
    /// </summary>
    public partial class PeoplePicker
    {
        /////// <summary>
        /////// Gets the set of <see cref="Person"/> objects chosen by the user.
        /////// </summary>
        ////public ObservableCollection<Person> PickedPeople
        ////{
        ////    get { return (ObservableCollection<Person>)GetValue(SelectedPeopleProperty); }
        ////    internal set { SetValue(SelectedPeopleProperty, value); }
        ////}

        /////// <summary>
        /////// Identifies the <see cref="PickedPeople"/> dependency property.
        /////// </summary>
        /////// <returns>
        /////// The identifier for the <see cref="PickedPeople"/> dependency property.
        /////// </returns>
        ////public static readonly DependencyProperty SelectedPeopleProperty =
        ////    DependencyProperty.Register(nameof(PickedPeople), typeof(ObservableCollection<Person>), typeof(PeoplePicker), new PropertyMetadata(new ObservableCollection<Person>()));

        /// <summary>
        /// Gets or sets collection of people suggested by the graph from the user's query.
        /// </summary>
        public ObservableCollection<Person> SuggestedPeople
        {
            get { return (ObservableCollection<Person>)GetValue(SuggestedPeopleProperty); }
            set { SetValue(SuggestedPeopleProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="SuggestedPeople"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="SuggestedPeople"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty SuggestedPeopleProperty =
            DependencyProperty.Register(nameof(SuggestedPeople), typeof(ObservableCollection<Person>), typeof(PeoplePicker), new PropertyMetadata(new ObservableCollection<Person>()));
    }
}
