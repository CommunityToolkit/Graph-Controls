// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Graph;
using Microsoft.Graph.Auth;

namespace Microsoft.Toolkit.Graph.Extensions
{
    /// <summary>
    /// Extension methods to the Graph SDK used by the Microsoft.Toolkit.Graph.Controls.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Simple method to convert a <see cref="User"/> to a <see cref="Person"/> with basic common properties like <see cref="Entity.Id"/>, <see cref="User.DisplayName"/>, <see cref="Person.EmailAddresses"/>, <see cref="User.GivenName"/>, and <see cref="User.Surname"/> intact.
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
                EmailAddresses = new RankedEmailAddress[]
                        {
                            new RankedEmailAddress()
                            {
                                Address = user.Mail ?? user.UserPrincipalName
                            }
                        },
                GivenName = user.GivenName,
                Surname = user.Surname,

                // Company Information
                CompanyName = user.CompanyName,
                Department = user.Department,
                Title = user.JobTitle,
                OfficeLocation = user.OfficeLocation
            };
        }

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

        /// <summary>
        /// Translate numeric file size to string format.
        /// </summary>
        /// <param name="size">file size in bytes.</param>
        /// <returns>Returns file size string.</returns>
        public static string ToFileSizeString(this long size) // TODO: Move this to Microsoft.Toolkit
        {
            if (size < 1024)
            {
                return size.ToString("F0") + " bytes";
            }
            else if ((size >> 10) < 1024)
            {
                return (size / (float)1024).ToString("F1") + " KB";
            }
            else if ((size >> 20) < 1024)
            {
                return ((size >> 10) / (float)1024).ToString("F1") + " MB";
            }
            else if ((size >> 30) < 1024)
            {
                return ((size >> 20) / (float)1024).ToString("F1") + " GB";
            }
            else if ((size >> 40) < 1024)
            {
                return ((size >> 30) / (float)1024).ToString("F1") + " TB";
            }
            else if ((size >> 50) < 1024)
            {
                return ((size >> 40) / (float)1024).ToString("F1") + " PB";
            }
            else
            {
                return ((size >> 50) / (float)1024).ToString("F0") + " EB";
            }
        }

        /// <summary>
        /// Extension Helper to help convert timestamp results from the Graph to <see cref="DateTimeOffset"/>.
        /// </summary>
        /// <param name="dttz">Graph Timestamp</param>
        /// <returns>System Timestamp</returns>
            public static DateTimeOffset ToDateTimeOffset(this DateTimeTimeZone dttz)
        {
            // https://docs.microsoft.com/en-us/dotnet/standard/datetime/choosing-between-datetime
            var datetime = DateTime.Parse(dttz.DateTime);
            var timezone = TimeZoneInfo.FindSystemTimeZoneById(dttz.TimeZone);
            var dto = new DateTimeOffset(datetime, timezone.GetUtcOffset(datetime));

            return dto;
        }
    }
}
