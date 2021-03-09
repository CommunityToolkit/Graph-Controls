// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Reflection;

namespace Microsoft.Toolkit.Graph.Providers.Extensions
{
    /// <summary>
    /// Helpers for Graph related HTTP Headers.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private const string SdkVersion = "SdkVersion";
        private const string LibraryName = "wct";
        private const string Bearer = "Bearer";
        private const string MockGraphToken = "{token:https://graph.microsoft.com/}";

        internal static void AddSdkVersion(this HttpRequestMessage request)
        {
            if (request == null || request.Headers == null)
            {
                return;
            }

            if (request.Headers.TryGetValues(SdkVersion, out IEnumerable<string> values))
            {
                var versions = new List<string>(values);
                versions.Insert(0, LibraryName + "/" + Assembly.GetExecutingAssembly().GetName().Version);
                request.Headers.Remove(SdkVersion);
                request.Headers.Add(SdkVersion, versions);
            }
        }

        internal static void AddMockProviderToken(this HttpRequestMessage request)
        {
            request
               .Headers
               .Authorization = new AuthenticationHeaderValue(Bearer, MockGraphToken);
        }
    }
}
