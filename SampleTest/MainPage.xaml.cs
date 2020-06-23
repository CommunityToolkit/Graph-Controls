// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Toolkit.Graph.Controls;
using Microsoft.Toolkit.Graph.Controls.Extensions;
using Microsoft.Toolkit.Graph.Extensions;
using Microsoft.Toolkit.Graph.Providers;
using System.IO;
using Windows.ApplicationModel.Contacts;
using Windows.Foundation;
using Windows.Storage.Streams;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace SampleTest
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
        }

        private IRandomAccessStream _stream;

        // Testing https://docs.microsoft.com/en-us/windows/uwp/design/controls-and-patterns/contact-card
        private async void PersonView_PointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            if (ContactManager.IsShowContactCardSupported() && sender is PersonView pv)
            {
                Rect selectionRect = GetElementRect((FrameworkElement)sender);

                var contact = pv.PersonDetails.ToWindowsContact();

                // Test hack (how to re-use image we have already?)
                _stream = (await ProviderManager.Instance.GlobalProvider.Graph.GetUserPhoto(pv.PersonDetails.UserPrincipalName)).AsRandomAccessStream();

                contact.Thumbnail = RandomAccessStreamReference.CreateFromStream(_stream);

                ContactManager.ShowContactCard(contact, selectionRect, Placement.Below);
            }
        }

        // Gets the rectangle of the element 
        public static Rect GetElementRect(FrameworkElement element)
        {
            // Passing "null" means set to root element. 
            GeneralTransform elementTransform = element.TransformToVisual(null);
            Rect rect = elementTransform.TransformBounds(new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            return rect;
        }
    }
}
