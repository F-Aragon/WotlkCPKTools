using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Windows;
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

        private bool _isLoading;
        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }


        public ICommand OpenAddAddonWindowCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UpdateAllCommand { get; }
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



            UpdateAllCommand = new RelayCommand(async o =>
            {
                var allAddons = _addonService.LoadAddonsFromLocal();
                await _addonService.UpdateAllAddonsAndSaveAsync(allAddons);

                Debug.WriteLine("UpdateAllCommand - All addons have been checked and updated if necessary.");

                LoadAddons();
            });


            // Execute initial load of addons
            
            _ = InitialLoadAddonsAsync();
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

        private async Task InitialLoadAddonsAsync()
        {
            try
            {
                IsLoading = true;
                AddonGroups.Clear();
                // Get complete list with GitHub status
                var completedList = await _addonService.CreateCompleteListAsync();
                // Save updated list to local JSON
                await _addonService.SaveAddonsListToJson(completedList);
                // Group addons for the UI
                var groups = _gridManagerService.GetAddonGroups(completedList);
                foreach (var group in groups)
                {
                    AddonGroups.Add(group);
                }
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
}

