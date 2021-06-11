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
        private const string PersonViewDefaultImageSourceResourceName = "PersonViewDefaultImageSource";

        /// <summary>
        /// <see cref="PersonQuery"/> value used to retrieve the signed-in user's info.
        /// </summary>
        public const string PersonQueryMe = "me";

        private string _photoId = null;

        private string _defaultImageSource = "ms-appx:///Microsoft.Toolkit.Graph.Controls/Assets/person.png";

        private BitmapImage _defaultImage;

        private static async void PersonDetailsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                if (pv.PersonDetails != null)
                {
                    if (pv?.PersonDetails?.GivenName?.Length > 0 && pv?.PersonDetails?.Surname?.Length > 0)
                    {
                        pv.Initials = string.Empty + pv.PersonDetails.GivenName[0] + pv.PersonDetails.Surname[0];
                    }
                    else if (pv?.PersonDetails?.DisplayName?.Length > 0)
                    {
                        // Grab first two initials in name
                        var initials = pv.PersonDetails.DisplayName.ToUpper().Split(' ').Select(i => i.First());
                        pv.Initials = string.Join(string.Empty, initials.Where(i => char.IsLetter(i)).Take(2));
                    }

                    if (pv?.UserPhoto?.UriSource?.AbsoluteUri == pv._defaultImageSource || pv?.PersonDetails?.Id != pv._photoId)
                    {
                        // Reload Image
                        pv.UserPhoto = pv._defaultImage;
                        await pv.LoadImageAsync(pv.PersonDetails);
                    }
                    else if (pv?.PersonDetails?.Id != pv._photoId)
                    {
                        pv.UserPhoto = pv._defaultImage;
                        pv._photoId = null;
                    }
                }
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
                pv.IsLargeImage = pv.PersonViewType == PersonViewType.Avatar;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonView"/> class.
        /// </summary>
        public PersonView()
        {
            this.DefaultStyleKey = typeof(PersonView);

            _defaultImage = new BitmapImage(new Uri(_defaultImageSource));

            ProviderManager.Instance.ProviderStateChanged += (sender, args) => LoadData();
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (Resources.TryGetValue(PersonViewDefaultImageSourceResourceName, out object value) && value is string uri)
            {
                _defaultImageSource = uri;
                _defaultImage = new BitmapImage(new Uri(_defaultImageSource)); // TODO: Couldn't load image from app package, only remote or in our assembly?
                UserPhoto = _defaultImage;
            }

            LoadData();
        }

        private async void LoadData()
        {
            var provider = ProviderManager.Instance.GlobalProvider;

            if (provider == null || provider.State != ProviderState.SignedIn)
            {
                // Set back to Default if not signed-in
                if (provider != null)
                {
                    UserPhoto = _defaultImage;
                }

                return;
            }

            if (PersonDetails != null && UserPhoto == null)
            {
                await LoadImageAsync(PersonDetails);
            }
            else if (!string.IsNullOrWhiteSpace(UserId) || PersonQuery?.ToLowerInvariant() == PersonQueryMe)
            {
                User user = null;
                if (!string.IsNullOrWhiteSpace(UserId))
                {
                    // TODO: Batch when API easier https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/29
                    try
                    {
                        user = await provider.GetClient().GetUserAsync(UserId);
                    }
                    catch
                    {
                    }

                    try
                    {
                        // TODO: Move to LoadImage based on previous call?
                        await DecodeStreamAsync(await provider.GetBetaClient().GetUserPhoto(UserId));
                        _photoId = UserId;
                    }
                    catch
                    {
                    }
                }
                else
                {
                    try
                    {
                        user = await provider.GetClient().GetMeAsync();
                    }
                    catch
                    {
                    }

                    try
                    {
                        await DecodeStreamAsync(await provider.GetBetaClient().GetMyPhotoAsync());
                        _photoId = user.Id;
                    }
                    catch
                    {
                    }
                }

                if (user != null)
                {
                    PersonDetails = user.ToPerson();
                }
            }
            else if (PersonDetails == null && !string.IsNullOrWhiteSpace(PersonQuery))
            {
                var people = await provider.GetClient().FindPersonAsync(PersonQuery);
                if (people != null && people.Count > 0)
                {
                    var person = people.FirstOrDefault();
                    PersonDetails = person;
                    await LoadImageAsync(person);
                }
            }
        }

        private async Task LoadImageAsync(Person person)
        {
            try
            {
                // TODO: Better guarding
                var graph = ProviderManager.Instance.GlobalProvider.GetBetaClient();

                if (!string.IsNullOrWhiteSpace(person.UserPrincipalName))
                {
                    await DecodeStreamAsync(await graph.GetUserPhoto(person.UserPrincipalName));
                    _photoId = person.Id; // TODO: Only set on success for photo?
                }
                else if (!string.IsNullOrWhiteSpace(person.ScoredEmailAddresses.First().Address))
                {
                    // TODO https://github.com/microsoftgraph/microsoft-graph-toolkit/blob/master/src/components/mgt-person/mgt-person.ts#L174
                }
            }
            catch
            {
                // If we can't load a photo, that's ok.
            }
        }

        private async Task DecodeStreamAsync(Stream photoStream)
        {
            if (photoStream != null)
            {
                using (var ras = photoStream.AsRandomAccessStream())
                {
                    var bitmap = new BitmapImage();
                    await bitmap.SetSourceAsync(ras);
                    UserPhoto = bitmap;
                }
            }
        }
    }
}
