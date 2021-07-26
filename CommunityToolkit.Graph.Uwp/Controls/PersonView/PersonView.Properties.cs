// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="PersonView"/> control displays a user photo and can display their name and e-mail.
    /// </summary>
    public partial class PersonView
    {
        /// <summary>
        /// Gets or sets details about this person retrieved from the graph or provided by the developer.
        /// </summary>
        public Person PersonDetails
        {
            get { return (Person)GetValue(PersonDetailsProperty); }
            set { SetValue(PersonDetailsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PersonDetails"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="PersonDetails"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty PersonDetailsProperty =
            DependencyProperty.Register(nameof(PersonDetails), typeof(Person), typeof(PersonView), new PropertyMetadata(null, PersonDetailsPropertyChanged));

        /// <summary>
        /// Gets or sets a string to automatically retrieve data on the specified query from the graph.  Use <see cref="PersonQueryMe"/> to retrieve info about the current user.  Otherwise, it's best to use an e-mail address as a query.
        /// </summary>
        public string PersonQuery
        {
            get { return (string)GetValue(PersonQueryProperty); }
            set { SetValue(PersonQueryProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="PersonQuery"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="PersonQuery"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty PersonQueryProperty =
            DependencyProperty.Register(nameof(PersonQuery), typeof(string), typeof(PersonView), new PropertyMetadata(null, QueryPropertyChanged));

        /// <summary>
        /// Gets or sets the UserId.
        /// </summary>
        public string UserId
        {
            get { return (string)GetValue(UserIdProperty); }
            set { SetValue(UserIdProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="UserId"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="UserId"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty UserIdProperty =
            DependencyProperty.Register(nameof(UserId), typeof(string), typeof(PersonView), new PropertyMetadata(null, QueryPropertyChanged));

        /// <summary>
        /// Gets or sets the photo of the user to be displayed.
        /// </summary>
        public BitmapImage UserPhoto
        {
            get { return (BitmapImage)GetValue(UserPhotoProperty); }
            set { SetValue(UserPhotoProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="UserPhoto"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="UserPhoto"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty UserPhotoProperty =
            DependencyProperty.Register(nameof(UserPhoto), typeof(BitmapImage), typeof(PersonView), new PropertyMetadata(null));

        /// <summary>
        /// Gets the initials of the person from the <see cref="PersonDetails"/>.
        /// </summary>
        public string Initials
        {
            get { return (string)GetValue(InitialsProperty); }
            internal set { SetValue(InitialsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="Initials"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="Initials"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty InitialsProperty =
            DependencyProperty.Register(nameof(Initials), typeof(string), typeof(PersonView), new PropertyMetadata(string.Empty));

        /// <summary>
        /// Gets a value indicating whether the image has expanded based on the PersonViewType.
        /// </summary>
        public bool IsLargeImage
        {
            get { return (bool)GetValue(IsLargeImageProperty); }
            internal set { SetValue(IsLargeImageProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="IsLargeImage"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="IsLargeImage"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty IsLargeImageProperty =
            DependencyProperty.Register(nameof(IsLargeImage), typeof(bool), typeof(PersonView), new PropertyMetadata(false));

        /// <summary>
        /// Gets or sets the type of details to display in the PersonView part of the template.
        /// </summary>
        public PersonViewType PersonViewType
        {
            get => (PersonViewType)GetValue(PersonViewTypeProperty);
            set => SetValue(PersonViewTypeProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="PersonViewType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PersonViewTypeProperty =
            DependencyProperty.Register(nameof(PersonViewType), typeof(PersonViewType), typeof(PersonView), new PropertyMetadata(PersonViewType.TwoLines, PersonViewTypePropertiesChanged));

        /// <summary>
        /// Gets or sets the type of visual to display in the image part of the template.
        /// </summary>
        public PersonAvatarType PersonAvatarType
        {
            get => (PersonAvatarType)GetValue(PersonAvatarTypeProperty);
            set => SetValue(PersonAvatarTypeProperty, value);
        }

        /// <summary>
        /// Identifies the <see cref="PersonAvatarType"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty PersonAvatarTypeProperty =
            DependencyProperty.Register(nameof(PersonAvatarType), typeof(PersonAvatarType), typeof(PersonView), new PropertyMetadata(PersonAvatarType.Photo, PersonAvatarTypePropertiesChanged));
    }
}
