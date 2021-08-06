// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="LoginButton"/> control is a button which can be used to sign the user in or show them profile details.
    /// </summary>
    [TemplatePart(Name = LoginButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = LogoutButtonPart, Type = typeof(ButtonBase))]
    public partial class LoginButton : Control
    {
        private const string LoginButtonPart = "PART_LoginButton";
        private const string LogoutButtonPart = "PART_LogoutButton";

        private Button _loginButton;
        private ButtonBase _logoutButton;

        private bool _isLoading;

        /// <summary>
        /// Gets or sets a value indicating whether the control is loading and has not established a sign-in state.
        /// </summary>
        protected bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                UpdateButtonEnablement();
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginButton"/> class.
        /// </summary>
        public LoginButton()
        {
            this.DefaultStyleKey = typeof(LoginButton);

            ProviderManager.Instance.ProviderUpdated += OnProviderUpdated;
            ProviderManager.Instance.ProviderStateChanged += OnProviderStateChanged;
        }

        /// <summary>
        /// Initiates logging in with the current <see cref="IProvider"/> registered in the <see cref="ProviderManager"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignInAsync()
        {
            if (IsLoading)
            {
                return;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null)
            {
                try
                {
                    IsLoading = true;

                    var cargs = new CancelEventArgs();
                    LoginInitiated?.Invoke(this, cargs);
                    if (cargs.Cancel)
                    {
                        throw new OperationCanceledException();
                    }

                    await provider.SignInAsync();

                    if (provider.State != ProviderState.SignedIn)
                    {
                        throw new Exception("Login did not complete.");
                    }
                }
                catch (Exception e)
                {
                    IsLoading = false;
                    LoginFailed?.Invoke(this, new LoginFailedEventArgs(e));
                }
            }
        }

        /// <summary>
        /// Log a signed-in user out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignOutAsync()
        {
            if (IsLoading)
            {
                return;
            }

            IsLoading = true;

            var cargs = new CancelEventArgs();
            LogoutInitiated?.Invoke(this, cargs);
            if (cargs.Cancel)
            {
                return;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null)
            {
                try
                {
                    await provider.SignOutAsync();

                    if (provider.State != ProviderState.SignedOut)
                    {
                        throw new Exception("Logout did not complete.");
                    }
                }
                finally
                {
                    // There is no LogoutFailed event, so we do nothing.
                    IsLoading = false;
                }
            }
        }

        /// <inheritdoc/>
        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_loginButton != null)
            {
                _loginButton.Click -= LoginButton_Click;
            }

            _loginButton = GetTemplateChild(LoginButtonPart) as Button;

            if (_loginButton != null)
            {
                _loginButton.Click += LoginButton_Click;
            }

            if (_logoutButton != null)
            {
                _logoutButton.Click -= LogoutButton_Click;
            }

            _logoutButton = GetTemplateChild(LogoutButtonPart) as ButtonBase;

            if (_logoutButton != null)
            {
                _logoutButton.Click += LogoutButton_Click;
            }

            LoadData();
        }

        /// <summary>
        /// Show the user details flyout.
        /// </summary>
        protected void ShowFlyout()
        {
            if (FlyoutBase.GetAttachedFlyout(_loginButton) is FlyoutBase flyout)
            {
                flyout.ShowAt(_loginButton);
            }
        }

        /// <summary>
        /// Hide the user details flyout.
        /// </summary>
        protected void HideFlyout()
        {
            if (FlyoutBase.GetAttachedFlyout(_loginButton) is FlyoutBase flyout)
            {
                flyout.Hide();
            }
        }

        /// <summary>
        /// Update the enablement state of the button in relation to the _isLoading property.
        /// </summary>
        protected void UpdateButtonEnablement()
        {
            if (_loginButton != null)
            {
                _loginButton.IsEnabled = !_isLoading;
            }
        }

        private void OnProviderUpdated(object sender, IProvider e)
        {
            if (e == null)
            {
                ClearUserDetails();
            }
        }

        private async void OnProviderStateChanged(object sender, ProviderStateChangedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            switch (provider.State)
            {
                case ProviderState.SignedIn:
                    IsLoading = true;
                    if (!await TrySetUserDetailsAsync())
                    {
                        // Failed to retrieve user details, force signout.
                        await SignOutAsync();
                        return;
                    }

                    IsLoading = false;
                    LoginCompleted?.Invoke(this, new EventArgs());
                    break;

                case ProviderState.SignedOut:
                    ClearUserDetails();
                    IsLoading = false;
                    LogoutCompleted?.Invoke(this, new EventArgs());
                    break;

                case ProviderState.Loading:
                    IsLoading = true;
                    break;
            }
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            switch (provider.State)
            {
                case ProviderState.SignedIn:
                    ShowFlyout();
                    break;

                case ProviderState.SignedOut:
                    await SignInAsync();
                    break;
            }
        }

        private async void LogoutButton_Click(object sender, RoutedEventArgs e)
        {
            HideFlyout();
            await SignOutAsync();
        }

        private async void LoadData()
        {
            if (IsLoading)
            {
                return;
            }

            var provider = ProviderManager.Instance.GlobalProvider;
            switch (provider?.State)
            {
                case ProviderState.Loading:
                    IsLoading = true;
                    break;

                case ProviderState.SignedIn:
                    if (UserDetails == null)
                    {
                        await SignInAsync();
                    }

                    IsLoading = false;
                    break;

                case ProviderState.SignedOut:
                    if (UserDetails != null)
                    {
                        await SignOutAsync();
                    }

                    IsLoading = false;
                    break;
            }
        }

        private async Task<bool> TrySetUserDetailsAsync()
        {
            User userDetails = null;

            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider != null)
            {
                try
                {
                    userDetails = await provider.GetClient().GetMeAsync();
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
            }

            UserDetails = userDetails;

            return UserDetails != null;
        }

        private void ClearUserDetails()
        {
            UserDetails = null;
        }
    }
}
