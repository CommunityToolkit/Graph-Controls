// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// User focused extension methods to the Graph SDK used by the controls and helpers.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Retrieve the curernt user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<User> GetMeAsync(this GraphServiceClient graph)
        {
            try
            {
                return await graph.Me.Request().GetAsync();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Retrieve a user by id.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">The is of the user to retrieve.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public static async Task<User> GetUserAsync(this GraphServiceClient graph, string userId)
        {
            try
            {
                return await graph.Users[userId].Request().GetAsync();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Shortcut to perform a user query.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="query">User to search for.</param>
        /// <returns><see cref="IGraphServiceUsersCollectionPage"/> collection of <see cref="User"/>.</returns>
        public static async Task<IGraphServiceUsersCollectionPage> FindUserAsync(this GraphServiceClient graph, string query)
        {
            try
            {
                return await graph
                    .Users
                    .Request()
                    .Filter($"startswith(displayName, '{query}') or startswith(givenName, '{query}') or startswith(surname, '{query}') or startswith(mail, '{query}') or startswith(userPrincipalName, '{query}')")
                    ////.WithScopes(new string[] { "user.readbasic.all" })
                    .GetAsync();
            }
            catch
            {
            }

            return new GraphServiceUsersCollectionPage();
        }

        /// <summary>
        /// Helper to get the photo of a particular user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <param name="userId">UserID.</param>
        /// <returns>Stream with user photo or null.</returns>
        public static async Task<Stream> GetUserPhoto(this GraphServiceClient graph, string userId)
        {
            try
            {
                return await graph
                    .Users[userId]
                    .Photo
                    .Content
                    .Request()
                    ////.WithScopes(new string[] { "user.readbasic.all" })
                    .GetAsync();
            }
            catch
            {
            }

            return null;
        }

        /// <summary>
        /// Get the photo of the current user.
        /// </summary>
        /// <param name="graph">Instance of the <see cref="GraphServiceClient"/>.</param>
        /// <returns>Stream with user photo or null.</returns>
        public static async Task<Stream> GetMyPhotoAsync(this GraphServiceClient graph)
        {
            try
            {
                return await graph
                    .Me
                    .Photo
                    .Content
                    .Request()
                    ////.WithScopes(new string[] { "user.read" })
                    .GetAsync();
            }
            catch
            {
            }

            return null;
        }
    }
}
