// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Windows.UI.ApplicationSettings;

namespace CommunityToolkit.Authentication
{
    /// <summary>
    /// <see cref="WebAccountCommand"/> can be produced through this parameter.
    /// </summary>
    public class WebAccountCommandParameter
    {
        /// <summary>
        /// Gets the delegate that's invoked when the user selects an account and a specific
        /// action in the account settings pane.
        /// </summary>
        public WebAccountCommandInvokedHandler Invoked { get; }

        /// <summary>
        /// Gets the actions that the command performs on the web account in the accounts pane.
        /// </summary>
        public SupportedWebAccountActions Actions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="WebAccountCommandParameter"/> class.
        /// </summary>
        /// <param name="invoked">The delegate that's invoked when the user selects an account and a specific
        /// action in the account settings pane.</param>
        /// <param name="actions">The actions that the command performs on the web account in the accounts pane.</param>
        public WebAccountCommandParameter(WebAccountCommandInvokedHandler invoked, SupportedWebAccountActions actions)
        {
            Invoked = invoked;
            Actions = actions;
        }
    }
}
