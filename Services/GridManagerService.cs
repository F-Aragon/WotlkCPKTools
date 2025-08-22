using System.Collections.ObjectModel;
using System.Windows;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class GridManagerService
    {
        private readonly AddonService _addonService;
        private readonly GitHubService _gitHubService;
        private readonly FastAddAddonsService _fastAddAddonsService = new FastAddAddonsService();


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

            // Group Fast Add
            var fastAddAddons = _fastAddAddonsService.LoadFastAddAddonsLocal();

            var fastAddAddonsGroup = new AddonGroup
            {
                GroupName = "Fast Add",
                Addons = new ObservableCollection<AddonItem>(
                    fastAddAddons.Select(f => new AddonItem
                    {
                        Name = f.Name,
                        GitHubLink = f.GitHubUrl,
                        IsUpdated = false
                    })
                )
            };

            groups.Add(installedGroup);
            groups.Add(fastAddAddonsGroup);

            return groups;
        }

        // Overload of GetAddonGroups(); Creates addon groups from a given list (instead of always reading local). Mainly for InitialLoad
        public ObservableCollection<AddonGroup> GetAddonGroups(List<AddonInfo> addons)
        {
            var groups = new ObservableCollection<AddonGroup>();

            var installedGroup = new AddonGroup
            {
                GroupName = "Installed",
                Addons = new ObservableCollection<AddonItem>(
                    addons.Select(a => new AddonItem
                    {
                        Name = a.Name,
                        IsUpdated = a.IsUpdated,
                        GitHubLink = a.GitHubUrl,
                        LastUpdate = a.LastUpdateDate
                    })
                )
            };

            var fastAddAddons = _fastAddAddonsService.LoadFastAddAddonsLocal();

            var fastAddAddonsGroup = new AddonGroup
            {
                GroupName = "Fast Add",
                Addons = new ObservableCollection<AddonItem>(
                    fastAddAddons.Select(f => new AddonItem
                    {
                        Name = f.Name,
                        GitHubLink = f.GitHubUrl,
                        IsUpdated = false
                    })
                )
            };

            groups.Add(installedGroup);
            groups.Add(fastAddAddonsGroup);

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
