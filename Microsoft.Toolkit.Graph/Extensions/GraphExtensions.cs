// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace Microsoft.Toolkit.Graph.Helpers
{
    /// <summary>
    /// Extension methods to the Graph SDK used by the Microsoft.Toolkit.Graph.Controls.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Shortcut to perform a person query.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/></param>
        /// <param name="query">User to search for.</param>
        /// <returns><see cref="IUserPeopleCollectionPage"/> collection of <see cref="User"/>.</returns>
        public static async Task<IUserPeopleCollectionPage> FindPersonAsync(this GraphServiceClient graph, string query)
        {
            try
            {
                return await graph
                    .Me
                    .People
                    .Request()
                    .Search(query)
                    .WithScopes(new string[] { "people.read" })
                    .GetAsync();
            }
            catch
            {
            }

            return new UserPeopleCollectionPage();
        }

        /// <summary>
        /// Helper to get the photo of a particular user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/></param>
        /// <param name="userid">UserID</param>
        /// <returns>Stream with user photo or null.</returns>
        public static async Task<Stream> GetUserPhoto(this GraphServiceClient graph, string userid)
        {
            try
            {
                return await graph
                    .Users[userid]
                    .Photo
                    .Content
                    .Request()
                    .WithScopes(new string[] { "user.readbasic.all" })
                    .GetAsync();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Extension to provider Searching on OData Requests
        /// </summary>
        /// <typeparam name="T"><see cref="IBaseRequest"/> type</typeparam>
        /// <param name="request">Request chain.</param>
        /// <param name="query">Query to add for searching in QueryOptions</param>
        /// <returns>Same type</returns>
        public static T Search<T>(this T request, string query)
            where T : IBaseRequest
        {
            request.QueryOptions?.Add(new QueryOption("$search", query));

            return request;
        }
    }
}
