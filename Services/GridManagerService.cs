using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class GridManagerService
    {
        private readonly AddonService _addonService;
        private readonly GitHubService _gitHubService;

        public GridManagerService(AddonService addonService, GitHubService gitHubService)
        {
            _addonService = addonService;
            _gitHubService = gitHubService;
        }

        public ObservableCollection<AddonGroup> GetAddonGroups()
        {
            var groups = new ObservableCollection<AddonGroup>();

            // Group Installed
            var installedGroup = new AddonGroup
            {
                GroupName = "Installed",
                Addons = new ObservableCollection<AddonItem>(
                    _addonService.LoadAddonsFromLocal().Select(a => new AddonItem
                    {
                        Name = a.Name,
                        //IconPath = a.IconPath,
                        IsUpdated = a.IsUpdated,
                        GitHubLink = a.GitHubUrl,
                        LastUpdate = a.LastUpdateDate,
                        //Folders = new ObservableCollection<AddonFolder>(a.Folders.Select(f => new AddonFolder { FolderName = f }))
                    })
                )
            };

            // Available (TO DO)
            var availableGroup = new AddonGroup
            {
                GroupName = "Available",
                Addons = new ObservableCollection<AddonItem>
                {
                    new AddonItem { Name = "Questie" },
                    new AddonItem { Name = "AtlasLoot" }
                }
            };

            groups.Add(installedGroup);
            groups.Add(availableGroup);

            return groups;
        }

        public async Task UpdateAddon(AddonItem addonItem)
        {
            // 1. Get LocalAddons
            var allAddons = _addonService.LoadAddonsFromLocal();

            // 2. Find AddonItem match in (AddonInfo)allAddons
            var addonInfo = allAddons.FirstOrDefault(a => a.Name.Equals(addonItem.Name, StringComparison.OrdinalIgnoreCase));

            if (addonInfo == null)
            {
                MessageBox.Show($"Addon {addonItem.Name} not in local json.");
                return;
            }

            // 3. Update this addon.
            await _addonService.UpdateAddonAndSaveAsync(addonInfo);
        }

        public void DeleteAddon(AddonItem addonItem)
        {
            string _addonName = addonItem.Name;
            _addonService.RemoveAddonWithButton(_addonName);
        }
    }

}
