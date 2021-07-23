// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="PersonView"/> control displays a user photo and can display their name and e-mail.
    /// </summary>
    public partial class PersonView : Control
    {
        /// <summary>
        /// <see cref="PersonQuery"/> value used to retrieve the signed-in user's info.
        /// </summary>
        public const string PersonQueryMe = "me";

        private const string PersonViewDefaultImageSourceResourceName = "PersonViewDefaultImageSource";
        private const string PackageDefaultImageSource = "ms-appx:///CommunityToolkit.Graph.Uwp/Assets/person.png";

        private static void PersonDetailsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                pv.UpdateVisual();
            }
        }

        private static void QueryPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView view)
            {
                view.PersonDetails = null;
                view.LoadData();
            }
        }

        private static void PersonViewTypePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                pv.IsLargeImage = pv.PersonViewType is PersonViewType.TwoLines or PersonViewType.ThreeLines;

                if (pv.IsLargeImage)
                {
                    pv._imageDecodePixelHeight = 48;
                    pv._imageDecodePixelWidth = 48;
                }
                else
                {
                    pv._imageDecodePixelHeight = 24;
                    pv._imageDecodePixelWidth = 24;
                }

                pv.UpdateImageSize();
            }
        }

        private static void PersonAvatarTypePropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                pv.UpdateVisual();
            }
        }

        private BitmapImage _defaultImage;
        private int _imageDecodePixelHeight;
        private int _imageDecodePixelWidth;
        private string _photoId = null;

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonView"/> class.
        /// </summary>
        public PersonView()
        {
            this.DefaultStyleKey = typeof(PersonView);
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Resources.TryGetValue(PersonViewDefaultImageSourceResourceName, out object value) && value is string uriString)
            {
                _defaultImage = new BitmapImage(new Uri(uriString));
            }
            else
            {
                _defaultImage = new BitmapImage(new Uri(PackageDefaultImageSource));
            }

            ProviderManager.Instance.ProviderStateChanged -= OnProviderStateChanged;
            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;

            VisualStateManager.GoToState(this, Enum.GetName(typeof(ProviderState), ProviderManager.Instance.State), true);
            LoadData();
        }

        private void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            VisualStateManager.GoToState(this, Enum.GetName(typeof(ProviderState), e.NewState), true);
            LoadData();
        }

        private async void LoadData()
        {
            var provider = ProviderManager.Instance.GlobalProvider;

            if (provider?.State == ProviderState.SignedIn)
            {
                await TryLoadPersonDetailsAsync();
                UpdateVisual();
            }
            else
            {
                LoadDefaultImage();
            }
        }

        private async void UpdateVisual()
        {
            if (PersonAvatarType is PersonAvatarType.Initials)
            {
                var initialsLoaded = TryLoadInitials();
                if (initialsLoaded)
                {
                    ClearUserPhoto();
                }
                else
                {
                    LoadDefaultImage();
                }
            }
            else if (PersonDetails != null)
            {
                if (PersonDetails.Id != _photoId)
                {
                    LoadDefaultImage();

                    var photoLoaded = await TryLoadUserPhotoAsync();
                    if (photoLoaded)
                    {
                        UpdateImageSize();
                    }
                    else
                    {
                        var initialsLoaded = TryLoadInitials();
                        if (initialsLoaded)
                        {
                            ClearUserPhoto();
                        }
                    }
                }
            }
            else
            {
                LoadDefaultImage();
            }
        }

        private void LoadDefaultImage()
        {
            if (UserPhoto != _defaultImage)
            {
                UserPhoto = _defaultImage;
                UpdateImageSize();

                _photoId = null;
                Initials = null;
            }
        }

        private void UpdateImageSize()
        {
            if (UserPhoto != null)
            {
                UserPhoto.DecodePixelHeight = _imageDecodePixelHeight;
                UserPhoto.DecodePixelWidth = _imageDecodePixelWidth;
            }
        }

        private void ClearUserPhoto()
        {
            UserPhoto = null;
            _photoId = null;
        }

        private async Task<bool> TryLoadPersonDetailsAsync()
        {
            if (PersonDetails != null)
            {
                return true;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State != ProviderState.SignedIn)
            {
                return false;
            }

            try
            {
                if (!string.IsNullOrWhiteSpace(UserId))
                {
                    var user = await provider.GetClient().GetUserAsync(UserId);
                    PersonDetails = user.ToPerson();
                }
                else if (PersonQuery?.ToLowerInvariant() == PersonQueryMe)
                {
                    var user = await provider.GetClient().GetMeAsync();
                    PersonDetails = user.ToPerson();
                }
                else if (!string.IsNullOrWhiteSpace(PersonQuery))
                {
                    var people = await provider.GetClient().FindPersonAsync(PersonQuery);
                    if (people != null && people.Count > 0)
                    {
                        var person = people.FirstOrDefault();
                        PersonDetails = person;
                    }
                }

                return PersonDetails != null;
            }
            catch
            {
                // TODO: Log exception
            }

            return false;
        }

        private async Task<bool> TryLoadUserPhotoAsync()
        {
            var person = PersonDetails;
            if (person == null)
            {
                return false;
            }

            if (PersonDetails.Id == _photoId && UserPhoto != null && UserPhoto != _defaultImage)
            {
                return true;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State != ProviderState.SignedIn)
            {
                return false;
            }

            Stream photoStream = null;

            // TODO: Better guarding
            try
            {
                var graph = ProviderManager.Instance.GlobalProvider?.GetBetaClient();
                if (PersonQuery?.ToLowerInvariant() == PersonQueryMe)
                {
                    photoStream = await graph.GetMyPhotoAsync();
                }
                else if (!string.IsNullOrWhiteSpace(person.UserPrincipalName))
                {
                    photoStream = await graph.GetUserPhoto(person.UserPrincipalName);
                }
                else if (!string.IsNullOrWhiteSpace(person.ScoredEmailAddresses.First().Address))
                {
                    // TODO https://github.com/microsoftgraph/microsoft-graph-toolkit/blob/master/src/components/mgt-person/mgt-person.ts#L174
                }
            }
            catch
            {
            }

            if (photoStream != null)
            {
                var decodeResults = await TryDecodeStreamAsync(photoStream);
                if (decodeResults.Success)
                {
                    UserPhoto = decodeResults.Image;
                    _photoId = person.Id;

                    return true;
                }
            }

            return false;
        }

        private bool TryLoadInitials()
        {
            if (!string.IsNullOrWhiteSpace(Initials))
            {
                return true;
            }

            if (PersonDetails == null)
            {
                Initials = null;
                return false;
            }

            string initials = null;

            if (PersonDetails?.GivenName?.Length > 0 && PersonDetails?.Surname?.Length > 0)
            {
                initials = string.Empty + PersonDetails.GivenName[0] + PersonDetails.Surname[0];
            }
            else if (PersonDetails?.DisplayName?.Length > 0)
            {
                // Grab first two initials in name
                var nameParts = PersonDetails.DisplayName.ToUpper().Split(' ').Select(i => i.First());
                initials = string.Join(string.Empty, nameParts.Where(i => char.IsLetter(i)).Take(2));
            }

            Initials = initials;

            return Initials != null;
        }

        private async Task<(bool Success, BitmapImage Image)> TryDecodeStreamAsync(Stream photoStream)
        {
            if (photoStream != null)
            {
                try
                {
                    using var ras = photoStream.AsRandomAccessStream();
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(ras);

                    return (true, bitmap);
                }
                catch
                {
                }
            }

            return (false, null);
        }
    }
}
