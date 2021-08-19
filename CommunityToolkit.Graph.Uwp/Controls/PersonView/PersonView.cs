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
                if (!await TryLoadPersonDetailsAsync())
                {
                    // TODO: Handle failure to load the PersonDetails.
                }

                UpdateVisual();
            }
            else
            {
                LoadDefaultImage();

                if (!string.IsNullOrWhiteSpace(UserId) || !string.IsNullOrWhiteSpace(PersonQuery))
                {
                    PersonDetails = null;
                }
            }
        }

        private async void UpdateVisual()
        {
            if (PersonDetails == null)
            {
                LoadDefaultImage();
                return;
            }

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
            else if (PersonDetails.Id != _photoId)
            {
                var photoLoaded = await TryLoadUserPhotoAsync();
                if (photoLoaded)
                {
                    UpdateImageSize();
                }
                else
                {
                    ClearUserPhoto();

                    var initialsLoaded = TryLoadInitials();
                    if (!initialsLoaded)
                    {
                        LoadDefaultImage();
                    }
                }
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
            // TODO: Better guarding.
            if (PersonDetails != null)
            {
                return true;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State != ProviderState.SignedIn)
            {
                PersonDetails = null;
                return false;
            }

            var graph = provider.GetClient();

            try
            {
                if (!string.IsNullOrWhiteSpace(UserId))
                {
                    var user = await graph.GetUserAsync(UserId);
                    PersonDetails = user.ToPerson();
                }
                else if (PersonQuery?.ToLowerInvariant() == PersonQueryMe)
                {
                    var user = await graph.GetMeAsync();
                    PersonDetails = user.ToPerson();
                }
                else if (!string.IsNullOrWhiteSpace(PersonQuery))
                {
                    var people = await graph.FindPersonAsync(PersonQuery);
                    if (people != null && people.Count > 0)
                    {
                        PersonDetails = people.FirstOrDefault();
                    }
                }
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Insufficient privileges.
                    // Incremental consent must not be supported by the current provider.
                    // TODO: Log or handle the lack of sufficient privileges.
                }
                else
                {
                    // Something unexpected happened.
                    throw;
                }
            }

            return PersonDetails != null;
        }

        private async Task<bool> TryLoadUserPhotoAsync()
        {
            // TODO: Better guarding.
            var person = PersonDetails;
            if (person == null)
            {
                return false;
            }

            if (person.Id == _photoId && UserPhoto != null && UserPhoto != _defaultImage)
            {
                return true;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider?.State != ProviderState.SignedIn)
            {
                return false;
            }

            Stream photoStream = null;

            try
            {
                var graph = ProviderManager.Instance.GlobalProvider?.GetBetaClient();
                photoStream = await graph.GetUserPhoto(person.Id);
            }
            catch (Microsoft.Graph.ServiceException e)
            {
                if (e.StatusCode == System.Net.HttpStatusCode.Forbidden)
                {
                    // Insufficient privileges.
                    // Incremental consent must not be supported by the current provider.
                    // TODO: Log or handle the lack of sufficient privileges.
                }
                else if (e.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    // This entity does not support profile photos.
                }
                else if (e.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    // Image not found.
                }
                else if ((int)e.StatusCode == 422)
                {
                    // IncorrectRecipientTypeDetected:
                    // Incorrect recipient type detected in request url. Retry with Groups as the object type.
                }
                else
                {
                    // Something unexpected happened.
                    throw;
                }
            }

            if (photoStream == null)
            {
                return false;
            }

            var decodeResults = await TryDecodeStreamAsync(photoStream);
            if (decodeResults.Success)
            {
                UserPhoto = decodeResults.Image;
                _photoId = person.Id;

                return true;
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