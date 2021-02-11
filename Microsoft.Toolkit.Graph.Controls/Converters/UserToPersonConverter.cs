// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

using System;
using Microsoft.Graph;
using Microsoft.Toolkit.Graph.Extensions;
using Microsoft.UI.Xaml.Data;

namespace Microsoft.Toolkit.Graph.Converters
{
    /// <summary>
    /// Converts a <see cref="User"/> to a <see cref="Person"/>.
    /// </summary>
    public class UserToPersonConverter : IValueConverter
    {
        /// <inheritdoc/>
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is User user)
            {
                return user.ToPerson();
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
