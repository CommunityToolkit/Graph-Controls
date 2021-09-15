// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.ObjectModel;
using Microsoft.Graph;
using Windows.UI.Xaml.Controls;

namespace SampleTest.Samples
{
    public sealed partial class PeoplePickerSample : Page
    {
        ObservableCollection<Person> MyPeople { get; set; } = new();

        public PeoplePickerSample()
        {
            InitializeComponent();
        }
    }
}
