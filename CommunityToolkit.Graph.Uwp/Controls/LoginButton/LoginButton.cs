// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="LoginButton"/> control is a button which can be used to sign the user in or show them profile details.
    /// </summary>
    [TemplatePart(Name = LoginButtonPart, Type = typeof(Button))]
    [TemplatePart(Name = SignOutButtonPart, Type = typeof(ButtonBase))]
    public partial class LoginButton : Control
    {
        private const string LoginButtonPart = "PART_LoginButton";
        private const string SignOutButtonPart = "PART_SignOutButton";

        private Button _loginButton;
        private ButtonBase _signOutButton;

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

            ProviderManager.Instance.ProviderStateChanged += (sender, args) => LoadData();
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

            if (_signOutButton != null)
            {
                _signOutButton.Click -= LoginButton_Click;
            }

            _signOutButton = GetTemplateChild(SignOutButtonPart) as ButtonBase;

            if (_signOutButton != null)
            {
                _signOutButton.Click += SignOutButton_Click;
            }

            LoadData();
        }

        private async void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            if (this.UserDetails != null)
            {
                if (FlyoutBase.GetAttachedFlyout(_loginButton) is FlyoutBase flyout)
                {
                    flyout.ShowAt(_loginButton);
                }
            }
            else
            {
                var cargs = new CancelEventArgs();
                LoginInitiated?.Invoke(this, cargs);

                if (!cargs.Cancel)
                {
                    await SignInAsync();
                }
            }
        }

        private async void SignOutButton_Click(object sender, RoutedEventArgs e)
        {
            await SignOutAsync();
        }

        private async void LoadData()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            switch (provider?.State)
            {
                case ProviderState.Loading:
                    IsLoading = true;
                    break;

                case ProviderState.SignedIn:
                    try
                    {
                        IsLoading = true;

                        // https://github.com/microsoftgraph/microsoft-graph-toolkit/blob/master/src/components/mgt-login/mgt-login.ts#L139
                        // TODO: Batch with photo request later? https://github.com/microsoftgraph/msgraph-sdk-dotnet-core/issues/29
                        UserDetails = await provider.GetClient().GetMeAsync();
                    }
                    catch (Exception e)
                    {
                        LoginFailed?.Invoke(this, new LoginFailedEventArgs(e));
                    }
                    finally
                    {
                        IsLoading = false;
                    }

                    break;

                case ProviderState.SignedOut:
                default:
                    UserDetails = null; // What if this was user provided? Should we not hook into these events then?
                    IsLoading = false;
                    break;
            }
        }

        /// <summary>
        /// Initiates logging in with the current <see cref="IProvider"/> registered in the <see cref="ProviderManager"/>.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignInAsync()
        {
            if (UserDetails != null || IsLoading)
            {
                return;
            }

            var provider = ProviderManager.Instance.GlobalProvider;

            if (provider != null)
            {
                try
                {
                    IsLoading = true;
                    await provider.SignInAsync();

                    if (provider.State == ProviderState.SignedIn)
                    {
                        // TODO: include user details?
                        LoginCompleted?.Invoke(this, new EventArgs());

                        LoadData();
                    }
                    else
                    {
                        LoginFailed?.Invoke(this, new LoginFailedEventArgs(new TimeoutException("Login did not complete.")));
                    }
                }
                catch (Exception e)
                {
                    LoginFailed?.Invoke(this, new LoginFailedEventArgs(e));
                }
                finally
                {
                    IsLoading = false;
                }
            }
        }

        /// <summary>
        /// Log a signed-in user out.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public async Task SignOutAsync()
        {
            // Close Menu
            if (FlyoutBase.GetAttachedFlyout(_loginButton) is FlyoutBase flyout)
            {
                flyout.Hide();
            }

            if (IsLoading)
            {
                return;
            }

            var cargs = new CancelEventArgs();
            LogoutInitiated?.Invoke(this, cargs);

            if (cargs.Cancel)
            {
                return;
            }

            if (UserDetails != null)
            {
                UserDetails = null;
            }
            else
            {
                return; // No-op
            }

            var provider = ProviderManager.Instance.GlobalProvider;

            if (provider != null)
            {
                IsLoading = true;
                await provider.SignOutAsync();
                IsLoading = false;

                LogoutCompleted?.Invoke(this, new EventArgs());
            }
        }
    }
}
