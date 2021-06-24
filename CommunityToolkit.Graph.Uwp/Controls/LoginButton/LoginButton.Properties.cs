// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Windows.UI.Xaml;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// The <see cref="LoginButton"/> control is a button which can be used to sign the user in or show them profile details.
    /// </summary>
    public partial class LoginButton
    {
        /// <summary>
        /// Gets or sets details about this person retrieved from the graph or provided by the developer.
        /// </summary>
        public User UserDetails
        {
            get { return (User)GetValue(UserDetailsProperty); }
            protected set { SetValue(UserDetailsProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="UserDetails"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="UserDetails"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty UserDetailsProperty =
            DependencyProperty.Register(nameof(UserDetails), typeof(User), typeof(LoginButton), new PropertyMetadata(null));
    }
}
