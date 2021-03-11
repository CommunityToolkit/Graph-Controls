// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.ComponentModel;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="LoginButton"/> control is a button which can be used to sign the user in or show them profile details.
    /// </summary>
    public partial class LoginButton
    {
        /// <summary>
        /// The user clicked the sign in button to start the login process - cancelable.
        /// </summary>
        public event CancelEventHandler LoginInitiated;

        /// <summary>
        /// The login process was successful and the user is now signed in.
        /// </summary>
        public event EventHandler LoginCompleted;

        /// <summary>
        /// The user canceled the login process or was unable to sign in.
        /// </summary>
        public event EventHandler<LoginFailedEventArgs> LoginFailed;

        /// <summary>
        /// The user started to logout - cancelable.
        /// </summary>
        public event CancelEventHandler LogoutInitiated;

        /// <summary>
        /// The user signed out.
        /// </summary>
        public event EventHandler LogoutCompleted;
    }
}
