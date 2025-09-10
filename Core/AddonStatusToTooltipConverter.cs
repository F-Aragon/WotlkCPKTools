using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace WotlkCPKTools.Core
{
    public class AddonStatusToTooltipConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            bool isComplete = values[0] is bool b1 && b1;
            bool isUpdated = values[1] is bool b2 && b2;

            if (!isComplete)
                return "Missin Folders. Click to Download.";
            else if (isUpdated)
                return "Addon is up to date.";
            else
                return "Addon outdated. Click to update.";
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}
