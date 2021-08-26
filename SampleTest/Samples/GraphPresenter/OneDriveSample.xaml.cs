// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using System.Threading.Tasks;
using CommunityToolkit.Authentication;
using CommunityToolkit.Graph.Extensions;
using Microsoft.Graph;
using Microsoft.Toolkit;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;

namespace SampleTest.Samples.GraphPresenter
{
    public sealed partial class OneDriveSample : Page
    {
        public IBaseRequestBuilder RecentDriveItemsRequestBuilder { get; set; }

        public OneDriveSample()
        {
            InitializeComponent();

            ProviderManager.Instance.ProviderStateChanged += (s, e) => UpdateRequestBuilder();
            UpdateRequestBuilder();
        }

        private void UpdateRequestBuilder()
        {
            var provider = ProviderManager.Instance.GlobalProvider;
            switch (provider?.State)
            {
                case ProviderState.SignedIn:
                    RecentDriveItemsRequestBuilder = provider.GetClient().Me.Drive.Recent();
                    break;

                default:
                    RecentDriveItemsRequestBuilder = null;
                    break;
            }
        }
    }

    internal sealed class AsyncResult<TResult> : ObservableObject
    {
        private TaskNotifier<TResult> taskNotifier;

        public Task<TResult> Task
        {
            get => taskNotifier;
            private set
            {
                SetPropertyAndNotifyOnCompletion(ref taskNotifier, value, nameof(ResultOrDefault));
            }
        }

        public AsyncResult(Task<TResult> task)
        {
            Task = task;
        }

        public TResult ResultOrDefault => this.Task.GetResultOrDefault();
    }

    public class OneDriveThumbnailConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RemoteItem ri)
            {
                // drives/${file.remoteItem.parentReference.driveId}/items/${file.remoteItem.id}/thumbnails/0/medium
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider?.State == ProviderState.SignedIn)
                {
                    var graph = provider.GetClient();
                    return new AsyncResult<ThumbnailSet>(graph.Drives[ri.ParentReference.DriveId].Items[ri.Id].Thumbnails["0"].Request().GetAsync());
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

    public class FileSizeConverter : IValueConverter
    {
        private static readonly string[] Suffixes = { "B", "KB", "MB", "GB", "TB" };

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long size)
            {
                float number = size;

                var suffixIndex = 0;
                string Output() => $"{Math.Round(number)}{Suffixes[suffixIndex]}";

                do
                {
                    if (number < 1024f)
                    {
                        return Output();
                    }

                    number = number / 1024f;
                }
                while (++suffixIndex < Suffixes.Length - 1);

                return Output();
            }

            return null;

        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
