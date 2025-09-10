using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Core
{
    public class IsInstalledConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length < 2) return false;
            if (values[0] is FastAddAddonInfo addon && values[1] is ObservableCollection<AddonItem> installed)
            {
                bool isInstalled = installed.Any(a => a.GitHubUrl.Equals(addon.GitHubUrl, StringComparison.OrdinalIgnoreCase));
                return isInstalled;
            }
            return false;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
