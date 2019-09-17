// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Helpers;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media.Imaging;

namespace Microsoft.Toolkit.Graph.Controls
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

        private static readonly string[] RequiredScopes = new string[] { "user.readbasic.all" };

        private string _photoId = null;

        private static async void PersonDetailsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                if (pv.PersonDetails != null)
                {
                    if (pv.PersonDetails.GivenName?.Length > 0 && pv.PersonDetails.Surname?.Length > 0)
                    {
                        pv.Initials = string.Empty + pv.PersonDetails.GivenName[0] + pv.PersonDetails.Surname[0];
                    }
                    else if (pv.PersonDetails.DisplayName?.Length > 0)
                    {
                        // Grab first two initials in name
                        var initials = pv.PersonDetails.DisplayName.ToUpper().Split(' ').Select(i => i.First());
                        pv.Initials = string.Join(string.Empty, initials.Where(i => char.IsLetter(i)).Take(2));
                    }

                    if (pv.UserPhoto == null || pv.PersonDetails.Id != pv._photoId)
                    {
                        // Reload Image
                        pv.UserPhoto = null;
                        await pv.LoadImageAsync(pv.PersonDetails);
                    }
                    else if (pv.PersonDetails.Id != pv._photoId)
                    {
                        pv.UserPhoto = null;
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

        private static void ShowDisplayPropertiesChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PersonView pv)
            {
                pv.IsLargeImage = pv.ShowName && pv.ShowEmail;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PersonView"/> class.
        /// </summary>
        public PersonView()
        {
            this.DefaultStyleKey = typeof(PersonView);

            ProviderManager.Instance.ProviderUpdated += (sender, args) => LoadData();
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            LoadData();
        }

        private async void LoadData()
        {
            var provider = ProviderManager.Instance.GlobalProvider;

            if (provider == null || provider.State != ProviderState.SignedIn)
            {
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
                        user = await provider.Graph.Users[UserId].Request().GetAsync();
                    }
                    catch
                    {
                    }

                    try
                    {
                        // TODO: Move to LoadImage based on previous call?
                        await DecodeStreamAsync(await provider.Graph.Users[UserId].Photo.Content.Request().GetAsync());
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
                        user = await provider.Graph.Me.Request().GetAsync();
                    }
                    catch
                    {
                    }

                    try
                    {
                        await DecodeStreamAsync(await provider.Graph.Me.Photo.Content.Request().GetAsync());
                        _photoId = user.Id;
                    }
                    catch
                    {
                    }
                }

                if (user != null)
                {
                    PersonDetails = new Person()
                    {
                        Id = user.Id,
                        DisplayName = user.DisplayName,
                        ScoredEmailAddresses = new ScoredEmailAddress[]
                        {
                            new ScoredEmailAddress()
                            {
                                Address = user.Mail ?? user.UserPrincipalName
                            }
                        },
                        GivenName = user.GivenName,
                        Surname = user.Surname
                    };
                }
            }
            else if (PersonDetails == null && !string.IsNullOrWhiteSpace(PersonQuery))
            {
                var people = await provider.Graph.FindPersonAsync(PersonQuery);
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
                var graph = ProviderManager.Instance.GlobalProvider.Graph;

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
