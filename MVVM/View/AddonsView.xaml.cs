using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WotlkCPKTools.MVVM.Model;
using WotlkCPKTools.MVVM.ViewModel;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.View
{
    public partial class AddonsView : UserControl
    {
        public AddonsView()
        {
            InitializeComponent();

        }
        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var eventArg = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = sender
            };

            var parent = ((Control)sender).Parent as UIElement;
            parent?.RaiseEvent(eventArg);
            e.Handled = true;
        }

        private async void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction != DataGridEditAction.Commit) return;

            if (e.EditingElement is TextBox textBox)
                textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();

            var dataGrid = sender as DataGrid;
            if (dataGrid == null) return;

            // --- Installed Addons ---
            if (dataGrid.Name == "InstalledAddonsGrid")
            {
                if (e.Row.Item is AddonItem addonItem)
                {
                    // Buscar el AddonInfo correspondiente
                    AddonService _addonService = new AddonService();
                    var storedAddons = _addonService.LoadAddonsFromLocal();
                    var match = storedAddons.FirstOrDefault(a => a.GitHubUrl == addonItem.GitHubUrl);

                    if (match != null)
                    {
                        match.Name = addonItem.Name;
                        await _addonService.SaveAddonsListToJson(storedAddons);
                    }
                }
            }
            // --- Fast Add Addons ---
            else if (dataGrid.Name == "FastAddAddonsGrid")
            {
                if (e.Row.Item is AddonItem fastItem)
                {
                    FastAddAddonInfoService _fastAddService = new FastAddAddonInfoService();
                    FastAddAddonInfo fastInfo = new FastAddAddonInfo
                    {
                        Name = fastItem.Name,
                        GitHubUrl = fastItem.GitHubUrl
                    };
                    await _fastAddService.UpdateFastAddonFromDataGridAsync(fastInfo);
                }
            }
            
        }



    }
}
