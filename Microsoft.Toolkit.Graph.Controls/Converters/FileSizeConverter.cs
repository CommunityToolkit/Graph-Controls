// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Toolkit.Graph.Extensions;
using Windows.UI.Xaml.Data;

namespace Microsoft.Toolkit.Graph.Controls.Converters
{
    /// <summary>
    /// The <see cref="FileSizeConverter"/> takes a long value in and converts it to a human readible string using the <see cref="Microsoft.Toolkit.Graph.Extensions.GraphExtensions.ToFileSizeString(long)"/> method.
    /// </summary>
    public class FileSizeConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is long size)
            {
                return size.ToFileSizeString();
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
