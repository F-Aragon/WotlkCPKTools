using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;
using WotlkCPKTools.MVVM.View;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    public class AddonsViewModel : ObservableObject
    {
        private readonly AddonService _addonService;
        private readonly GridManagerService _gridManagerService;

        public ObservableCollection<AddonGroup> AddonGroups { get; set; } = new();

        public ICommand OpenAddAddonWindowCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }

        public AddonsViewModel()
        {
            _addonService = new AddonService();
            _gridManagerService = new GridManagerService(_addonService, new GitHubService());

            // Open window to add a new addon
            OpenAddAddonWindowCommand = new RelayCommand(o =>
            {
                var window = new AddAddonWindow();
                window.ShowDialog();
                LoadAddons();
            });

            // Delete an addon
            DeleteCommand = new RelayCommand(o =>
            {
                _gridManagerService.DeleteAddon((AddonItem)o);
                LoadAddons();
            });

            // Update an addon
            UpdateCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem addonItem)
                {
                    var addonInfo = _addonService.LoadAddonsFromLocal()
                                                 .FirstOrDefault(a => a.Name == addonItem.Name);
                    if (addonInfo != null)
                    {
                        await _addonService.UpdateAddonAndSaveAsync(addonInfo);
                        LoadAddons();
                    }
                }
            });

            // Refresh all addons from GitHub
            RefreshCommand = new RelayCommand(async o =>
            {
                var completedList = await _addonService.CreateCompleteListAsync();
                LoadAddons();
            });

            // Load the initial list of addons
            LoadAddons();
        }

        private void LoadAddons()
        {
            AddonGroups.Clear();

            var groups = _gridManagerService.GetAddonGroups();
            foreach (var group in groups)
            {
                AddonGroups.Add(group);
            }
        }
    }
}
