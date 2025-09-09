using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class GridManagerService
    {
        private readonly AddonService _addonService;
        private readonly GitHubService _gitHubService;
        public readonly FastAddAddonInfoService _fastAddAddonsService;

        public GridManagerService(AddonService addonService, GitHubService gitHubService, FastAddAddonInfoService? fastAddAddonsService = null)
        {
            _addonService = addonService;
            _gitHubService = gitHubService;
            _fastAddAddonsService = fastAddAddonsService ?? new FastAddAddonInfoService();
        }

        /// <summary>
        /// Builds the Installed addons list from local JSON.
        /// </summary>
        public ObservableCollection<AddonItem> BuildInstalledItems()
        {
            GitHubService _gitHubService = new GitHubService();

            var installed = new ObservableCollection<AddonItem>(
                _addonService.LoadAddonsFromLocal().Select(a =>
                {
                    var item = new AddonItem
                    {
                        Name = a.Name,
                        GitHubUrl = a.GitHubUrl,
                        LastUpdate = a.LastUpdateDate,
                        IsUpdated = a.IsUpdated
                    };

                    // Refresh the IsComplete property based on local folders
                    item.RefreshCompletionStatus();

                    return item;
                })
            );

            return installed;
        }


        /// <summary>
        /// Builds the Fast Add list from local JSON and filters out any item already installed (by GitHub link).
        /// </summary>
        public ObservableCollection<AddonItem> BuildFastAddItemsFiltered(ObservableCollection<AddonItem> installed)
        {
            var installedLinks = installed.Select(i => i.GitHubUrl).ToHashSet();

            var fastAddLocal = _fastAddAddonsService.LoadFastAddAddonsLocal();
            var fastAdd = new ObservableCollection<AddonItem>(
                fastAddLocal
                    .Where(f => !installedLinks.Contains(f.GitHubUrl))
                    .Select(f => new AddonItem
                    {
                        Name = f.Name,
                        GitHubUrl = f.GitHubUrl,
                        IsUpdated = false
                    })
            );

            return fastAdd;
        }

        /// <summary>
        /// Updates a single installed addon (delegates to AddonService).
        /// </summary>
        public async Task UpdateAddonAsync(AddonItem addonItem, ObservableCollection<AddonItem> installedItems)
        {
            var allAddons = _addonService.LoadAddonsFromLocal();
            var addonInfo = allAddons.FirstOrDefault(a => a.GitHubUrl.Equals(addonItem.GitHubUrl, System.StringComparison.OrdinalIgnoreCase));

            if (addonInfo == null)
            {
                MessageBox.Show($"Addon {addonItem.Name} not found in local JSON.");
                return;
            }
 
            await _addonService.UpdateAddonAndSaveAsync(addonInfo, installedItems);
        }

        /// <summary>
        /// Removes an installed addon by name (delegates to AddonService).
        /// </summary>
        public void RemoveInstalled(AddonItem addonItem)
        {
            _addonService.RemoveAddonWithButton(addonItem.GitHubUrl);
        }
    }
}
