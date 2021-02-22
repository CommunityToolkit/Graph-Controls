// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    ///  A base class for building Graph powered controls.
    /// </summary>
    public static class ProviderStateManager
    {
        /// <summary>
        /// An list of common states that Graph based controls should support at a minimum.
        /// </summary>
        private enum LoginStates
        {
            /// <summary>
            /// The control is in a indeterminate state
            /// </summary>
            Indeterminate,

            /// <summary>
            /// The control has Graph context and can behave properly
            /// </summary>
            SignedIn,

            /// <summary>
            /// The control does not have Graph context and cannot load any data.
            /// </summary>
            SignedOut,

            /// <summary>
            /// There was an error loading the control.
            /// </summary>
            Error,
        }

        static ProviderStateManager()
        {
            ProviderManager.Instance.ProviderUpdated += async (s, a) => await UpdateAsync();
        }

        /// <summary>
        /// Update the data state of the control.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected async Task UpdateAsync()
        {
                GoToVisualState(LoginStates.Indeterminate);

                try
                {
                    var provider = ProviderManager.Instance.GlobalProvider;
                    if (provider == null)
                    {
                        await ClearDataAsync();
                    }
                    else
                    {
                        switch (provider.State)
                        {
                            case ProviderState.SignedIn:
                                await LoadDataAsync();
                                break;
                            case ProviderState.SignedOut:
                                await ClearDataAsync();
                                break;
                        }
                    }

                    UpdateVisualState();
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine(e);
                    GoToErrorState();
                }
            });
        }

        /// <summary>
        /// Load data from the Graph and apply the values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task LoadDataAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Clear any data state and reset default values.
        /// </summary>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        protected virtual Task ClearDataAsync()
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// Puts the control into an unrecoverable error state.
        /// </summary>
        /// <returns>A boolean indicating success.</returns>
        protected bool GoToErrorState(bool useTransitions = false)
        {
            return GoToVisualState(LoginStates.Error);
        }

        private void UpdateVisualState()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            if (provider == null)
            {
                GoToVisualState(LoginStates.SignedOut);
                return;
            }

            switch (provider.State)
            {
                case ProviderState.SignedOut:
                    GoToVisualState(LoginStates.SignedOut);
                    break;
                case ProviderState.SignedIn:
                    GoToVisualState(LoginStates.SignedIn);
                    break;
                case ProviderState.Loading:
                    GoToVisualState(LoginStates.Indeterminate);
                    break;
                default:
                    GoToVisualState(LoginStates.Error);
                    break;
            }
        }

        /// <summary>
        /// A helper method for setting the visual state of the control using a string value.
        /// </summary>
        /// <returns>A bool representing success or failure.</returns>
        protected bool GoToVisualState(string state, bool useTransitions = false)
        {
            return VisualStateManager.GoToState(this, state, useTransitions);
        }

        private bool GoToVisualState(LoginStates state, bool useTransitions = false)
        {
            return GoToVisualState(state.ToString(), useTransitions);
        }
    }
}