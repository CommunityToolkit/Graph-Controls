// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using Microsoft.Graph;
using Microsoft.Toolkit.Uwp;
using Windows.System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace CommunityToolkit.Graph.Uwp.Controls
{
    /// <summary>
    /// Specialized <see cref="ContentPresenter"/> to fetch and display data from the Microsoft Graph.
    /// </summary>
    public class GraphPresenter : ContentPresenter
    {
        /// <summary>
        /// Identifies the <see cref="RequestBuilder"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="RequestBuilder"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty RequestBuilderProperty =
            DependencyProperty.Register(nameof(RequestBuilder), typeof(IBaseRequestBuilder), typeof(GraphPresenter), new PropertyMetadata(null));

        /// <summary>
        /// Initializes a new instance of the <see cref="GraphPresenter"/> class.
        /// </summary>
        public GraphPresenter()
        {
            this.Loaded += this.GraphPresenter_Loaded;
        }

        /// <summary>
        /// Gets or sets a <see cref="IBaseRequestBuilder"/> to be used to make a request to the graph. The results will be automatically populated to the <see cref="ContentPresenter.Content"/> property. Use a <see cref="ContentPresenter.ContentTemplate"/> to change the presentation of the data.
        /// </summary>
        public IBaseRequestBuilder RequestBuilder
        {
            get { return (IBaseRequestBuilder)this.GetValue(RequestBuilderProperty); }
            set { this.SetValue(RequestBuilderProperty, value); }
        }

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
        /// Gets or sets a string to indicate a sorting order for the <see cref="RequestBuilder"/>. This is a helper to add this specific request option to the <see cref="QueryOptions"/>.
        /// </summary>
        public string OrderBy { get; set; }

        private async void GraphPresenter_Loaded(object sender, RoutedEventArgs e)
        {
            var dispatcherQueue = DispatcherQueue.GetForCurrentThread();

            // Note: some interfaces from the Graph SDK don't implement IBaseRequestBuilder properly, see https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/722
            if (this.RequestBuilder != null)
            {
                var request = new BaseRequest(this.RequestBuilder.RequestUrl, this.RequestBuilder.Client) // TODO: Do we need separate Options here?
                {
                    Method = HttpMethods.GET,
                    QueryOptions = this.QueryOptions?.Select(option => (Microsoft.Graph.QueryOption)option)?.ToList() ?? new List<Microsoft.Graph.QueryOption>(),
                };

                // Handle Special QueryOptions
                if (!string.IsNullOrWhiteSpace(this.OrderBy))
                {
                    request.QueryOptions.Add(new Microsoft.Graph.QueryOption("$orderby", this.OrderBy));
                }

                try
                {
                    var responseObj = await request.SendAsync<object>(null, CancellationToken.None).ConfigureAwait(false);

                    if (responseObj is JsonElement responseElement)
                    {
                        //// TODO: Deal with paging?

                        var value = responseElement.GetProperty("value");
                        object data = null;

                        if (this.IsCollection)
                        {
                            data = value.EnumerateArray().ToList().Select(elem => System.Text.Json.JsonSerializer.Deserialize(elem.GetRawText(), this.ResponseType));
                        }
                        else
                        {
                            data = System.Text.Json.JsonSerializer.Deserialize(value.GetRawText(), this.ResponseType);
                        }

                        _ = dispatcherQueue.EnqueueAsync(() => this.Content = data);
                    }
                }
                catch
                {
                    // TODO: We should figure out what we want to do for Loading/Error states here.
                }
            }
        }
    }
}