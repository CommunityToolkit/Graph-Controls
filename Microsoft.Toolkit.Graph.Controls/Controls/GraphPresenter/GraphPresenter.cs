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
        public object RequestBuilder
        {
            get { return (object)GetValue(RequestBuilderProperty); }
            set { SetValue(RequestBuilderProperty, value); }
        }

        /// <summary>
        /// Identifies the <see cref="RequestBuilder"/> dependency property.
        /// </summary>
        /// <returns>
        /// The identifier for the <see cref="RequestBuilder"/> dependency property.
        /// </returns>
        public static readonly DependencyProperty RequestBuilderProperty =
            DependencyProperty.Register(nameof(RequestBuilder), typeof(object), typeof(GraphPresenter), new PropertyMetadata(null, OnDataChanged));

        public Type ResponseType { get; set; }

        public bool IsCollection { get; set; }

        private static async void OnDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is GraphPresenter presenter)
            {
                // Use BaseRequestBuilder as some interfaces from the Graph SDK don't implement IBaseRequestBuilder properly, see https://github.com/microsoftgraph/msgraph-sdk-dotnet/issues/722
                if (e.NewValue is BaseRequestBuilder builder)
                {
                    var request = new BaseRequest(builder.RequestUrl, builder.Client, null);
                    request.Method = "GET";

                    var response = await request.SendAsync<object>(null, CancellationToken.None).ConfigureAwait(false) as JObject;

                    //// TODO: Deal with paging?

                    var values = response["value"];
                    object data = null;

                    if (presenter.IsCollection)
                    {
                        data = values.ToObject(Array.CreateInstance(presenter.ResponseType, 0).GetType());
                    }
                    else
                    {
                        data = values.ToObject(presenter.ResponseType);
                    }

                    await DispatcherHelper.ExecuteOnUIThreadAsync(() =>
                    {
                        presenter.Content = data;
                    });
                }
            }
        }
    }
}
