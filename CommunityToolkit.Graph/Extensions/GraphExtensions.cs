// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;

namespace CommunityToolkit.Graph.Extensions
{
    /// <summary>
    /// Extension methods to the Graph SDK used by the controls and helpers.
    /// </summary>
    public static partial class GraphExtensions
    {
        /// <summary>
        /// Simple method to convert a <see cref="User"/> to a <see cref="Person"/> with basic common properties like <see cref="Entity.Id"/>, <see cref="User.DisplayName"/>, <see cref="Person.ScoredEmailAddresses"/>, <see cref="User.GivenName"/>, and <see cref="User.Surname"/> intact.
        /// </summary>
        /// <param name="user"><see cref="User"/> instance to convert.</param>
        /// <returns>A new basic <see cref="Person"/> representation of that user.</returns>
        public static Person ToPerson(this User user)
        {
            return new Person()
            {
                // Primary Id
                Id = user.Id,
                UserPrincipalName = user.UserPrincipalName,

                // Standard User Info
                DisplayName = user.DisplayName,
                ScoredEmailAddresses = new ScoredEmailAddress[]
                        {
                            new ScoredEmailAddress()
                            {
                                Address = user.Mail ?? user.UserPrincipalName,
                            },
                        },
                GivenName = user.GivenName,
                Surname = user.Surname,

                // Company Information
                CompanyName = user.CompanyName,
                Department = user.Department,
                JobTitle = user.JobTitle,
                OfficeLocation = user.OfficeLocation,
            };
        }

        /// <summary>
        /// Extension to provider Searching on OData Requests.
        /// </summary>
        /// <typeparam name="T"><see cref="IBaseRequest"/> type.</typeparam>
        /// <param name="request">Request chain.</param>
        /// <param name="query">Query to add for searching in QueryOptions.</param>
        /// <returns>Same type.</returns>
        public static T Search<T>(this T request, string query)
            where T : IBaseRequest
        {
            // Need quotes around query for e-mail searches: https://docs.microsoft.com/en-us/graph/people-example#perform-a-fuzzy-search
            request.QueryOptions?.Add(new QueryOption("$search", '"' + query + '"'));

            return request;
        }
    }
}
