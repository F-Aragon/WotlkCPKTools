using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WotlkCPKTools.Core
{
    public class DownloadStatusToTextConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            DateTime? lastUpdate = values[0] as DateTime?;
            string status = values[1] as string;

            if (!string.IsNullOrEmpty(status))
                return status; // Shows "Downloading..." or "Extracting...", etc.

            return lastUpdate?.ToString("yyyy-MM-dd HH:mm") ?? "";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
