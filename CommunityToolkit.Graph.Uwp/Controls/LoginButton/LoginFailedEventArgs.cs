// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// <see cref="EventArgs"/> for <see cref="LoginButton.LoginFailed"/> event.
    /// </summary>
    public class LoginFailedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the exception which occured during login.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets the inner exception which occured during login.
        /// </summary>
        public Exception InnerException
        {
            get
            {
                return Exception?.InnerException;
            }
        }

        /// <summary>
        /// Gets the error message of the inner error or error.
        /// </summary>
        public string Message
        {
            get
            {
                return Exception?.InnerException?.Message ?? Exception?.Message;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="LoginFailedEventArgs"/> class.
        /// </summary>
        /// <param name="exception">Exception encountered during login.</param>
        public LoginFailedEventArgs(Exception exception)
        {
            Exception = exception;
        }
    }
}