using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WotlkCPKTools.Core
{
    public class AddonStatusToIconConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2)
                return null;

            bool isComplete = values[0] is bool bComplete && bComplete;
            bool isUpdated = values[1] is bool bUpdated && bUpdated;

            if (!isComplete)
                return App.Current.Resources["IconCloudInsideFilled-Red"];
            if (isUpdated)
                return App.Current.Resources["IconCloudCheckFilled-Green"];
            return App.Current.Resources["IconCloudInsideFilled-Yellow"];
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
