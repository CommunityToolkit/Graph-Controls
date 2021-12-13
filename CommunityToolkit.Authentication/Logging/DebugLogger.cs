// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Diagnostics;

namespace CommunityToolkit.Authentication.Logging
{
    /// <summary>
    /// Logs messages using the <see cref="Debug"/> API.
    /// </summary>
    public class DebugLogger : ILogger
    {
        /// <summary>
        /// Singleton instance.
        /// </summary>
        public static readonly DebugLogger Instance = new DebugLogger();

        /// <inheritdoc />
        public void Log(string message)
        {
            Debug.WriteLine(message);
        }
    }
}
