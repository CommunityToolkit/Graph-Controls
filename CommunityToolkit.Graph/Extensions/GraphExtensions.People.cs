// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.Threading.Tasks;
using Microsoft.Graph;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// People focused extension methods to the Graph SDK used by the controls and helpers.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Shortcut to perform a person query.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="query">User to search for.</param>
        /// <returns><see cref="IUserPeopleCollectionPage"/> collection of <see cref="Person"/>.</returns>
        public static async Task<IUserPeopleCollectionPage> FindPersonAsync(this GraphServiceClient graph, string query)
        {
            return await graph
                .Me
                .People
                .Request()
                .Search(query)
                .WithScopes(new string[] { "people.read" })
                .GetAsync();
        }
    }
}
