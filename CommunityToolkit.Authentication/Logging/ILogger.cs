// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace CommunityToolkit.Authentication.Logging
{
    /// <summary>
    /// Defines a simple logger for handling messages during runtime.
    /// </summary>
    public interface ILogger
    {
        /// <summary>
        /// Log a message.
        /// </summary>
        /// <param name="message">The message to log.</param>
        void Log(string message);
    }
}
