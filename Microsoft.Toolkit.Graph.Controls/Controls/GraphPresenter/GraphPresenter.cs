// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Microsoft.Toolkit.Uwp.Helpers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Microsoft.Toolkit.Graph.Controls
{
    /// <summary>
    /// Specialized <see cref="ContentPresenter"/> to fetch and display data from the Microsoft Graph.
    /// </summary>
    public class GraphPresenter : ContentPresenter
    {
        /// <summary>
        /// Gets or sets a <see cref="IBaseRequestBuilder"/> to be used to make a request to the graph. The results will be automatically populated to the <see cref="ContentPresenter.Content"/> property. Use a <see cref="ContentPresenter.ContentTemplate"/> to change the presentation of the data.
        /// </summary>
        public IBaseRequestBuilder RequestBuilder
        {
            get { return (IBaseRequestBuilder)GetValue(RequestBuilderProperty); }
            set { SetValue(RequestBuilderProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="RequestBuilder"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="RequestBuilder"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty RequestBuilderProperty =
            DependencyProperty.Register(nameof(RequestBuilder), typeof(IBaseRequestBuilder), typeof(GraphPresenter), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the <see cref="Type"/> of item returned by the <see cref="RequestBuilder"/>.
        /// Set to the base item type and use the <see cref="IsCollection"/> property to indicate if a collection is expected back.
        /// </summary>
        public Type ResponseType { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the returned data from the <see cref="RequestBuilder"/> is a collection.
        /// </summary>
        public bool IsCollection { get; set; }

        /// <summary>
        /// Gets or sets list of <see cref="QueryOption"/> representing <see cref="Microsoft.Graph.QueryOption"/> values to pass into the request built by <see cref="RequestBuilder"/>.
        /// </summary>
        public List<QueryOption> QueryOptions { get; set; } = new List<QueryOption>();

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPresenter"/> class.
        /// </summary>
        public GraphPresenter()
        {
            Loaded += GraphPresenter_Loaded;
        }

        private async void GraphPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            // Note: some interfaces from the Graph SDK don't implement IBaseRequestBuilder properly, see https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/722
            if (RequestBuilder != null)
            {
                var request = new BaseRequest(
                                        RequestBuilder.RequestUrl,
                                        RequestBuilder.Client); // TODO: Do we need separate Options here?
                request.Method = "GET";
                request.QueryOptions = QueryOptions?.Select(option => option.ToQueryOption())?.ToList();

                // TODO: Add Exception Handling
                // Note: CalendarView not supported https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/740
                var response = await request.SendAsync<object>(null, CancellationToken.None).ConfigureAwait(false) as JObject;

                //// TODO: Deal with paging?

                var values = response["value"];
                object data = null;

                if (IsCollection)
                {
                    data = values.ToObject(Array.CreateInstance(ResponseType, 0).GetType());
                }
                else
                {
                    data = values.ToObject(ResponseType);
                }

                await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                {
                    Content = data;
                });
            }
        }
    }
}
