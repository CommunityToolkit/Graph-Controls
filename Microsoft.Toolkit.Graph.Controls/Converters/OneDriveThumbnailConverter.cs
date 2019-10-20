using System;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Providers;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Graph.Controls.Converters
{
    /// <summary>
    /// Helper to return a <see cref="NotifyTaskCompletion{ThumbnailSet}"/>.
    /// </summary>
    public class OneDriveThumbnailConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is RemoteItem ri)
            {
                // drives/${file.remoteItem.parentReference.driveId}/items/${file.remoteItem.id}/thumbnails/0/medium
                var provider = ProviderManager.Instance.GlobalProvider;
                if (provider != null && provider.Graph != null)
                {
                    return new NotifyTaskCompletion<ThumbnailSet>(provider.Graph.Drives[ri.ParentReference.DriveId].Items[ri.Id].Thumbnails["0"].Request().GetAsync());
                }
            }

            return null;
        }

        /// <inheritdoc/>
        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }
}
