using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
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
        

        // Collections bound to XAML
        public ObservableCollection<AddonItem> InstalledAddons { get; } = new();
        public ObservableCollection<AddonItem> FastAddAddons { get; } = new();
        public ObservableCollection<CustomAddonList> CustomAddonLists { get; set; } = new();

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

        // Commands for XAML
        public ICommand OpenAddAddonWindowCommand { get; }
        public ICommand UpdateCommand { get; }
        public ICommand RemoveInstalledCommand { get; }
        public ICommand AddFromFastAddCommand { get; }
        public ICommand DeleteFastAddCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand UpdateAllCommand { get; }
        public ICommand MoveToFastAddCommand { get; }
        public ICommand MoveFromCustomListFileCommand { get; }
        public ICommand DownloadRecommendedList { get; }
        public ICommand DownloadCustomList { get; }


        public AddonsViewModel()
        {
            _addonService = new AddonService();
            _gridManagerService = new GridManagerService(_addonService, new GitHubService(), new FastAddAddonInfoService());

            // Open window to add a new addon
            OpenAddAddonWindowCommand = new RelayCommand(_ =>
            {
                var window = new AddAddonWindow();
                window.ShowDialog();
                _ = ReloadBothAsync();
            });

            // Update a single installed addon
            UpdateCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    await _gridManagerService.UpdateAddonAsync(item);
                    _ = ReloadBothAsync();
                }
            });

            // Remove from Installed (current behavior: just remove files/entry)
            RemoveInstalledCommand = new RelayCommand(o =>
            {
                if (o is AddonItem item)
                {
                    _gridManagerService.RemoveInstalled(item);
                    _ = ReloadBothAsync();
                }
            });

            // Refresh both lists
            RefreshCommand = new RelayCommand(_ => _ = ReloadBothAsync());

            // Update all installed addons
            UpdateAllCommand = new RelayCommand(async _ =>
            {
                var allAddons = _addonService.LoadAddonsFromLocal();
                await _addonService.UpdateAllAddonsAndSaveAsync(allAddons);
                Debug.WriteLine("UpdateAllCommand - All addons have been checked and updated if necessary.");
                _ = ReloadBothAsync();
            });

            // Delete from Fast Add

            DeleteFastAddCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    try
                    {
                        // 1. Remove from Fast Add JSON
                        await _gridManagerService._fastAddAddonsService.RemoveFastAddAddonAsync(item.GitHubUrl);

                        // 2. Reload both collections for UI
                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting addon: {ex.Message}");
                    }
                }
            });

            // 
            MoveToFastAddCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    try
                    {
                        // 1. Add to Fast Add JSON
                        await _gridManagerService._fastAddAddonsService.AddFastAddAddonAsync(item.Name, item.GitHubUrl);
                        
                        // 2. Remove from Installed (Files and JSON)
                        await _addonService.RemoveAddonWithButton(item.GitHubUrl);

                        // 3. Reload both collections for UI
                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving addon to Fast Add: {ex.Message}");
                    }
                }
            });


            // Add from Fast Add → Installed
            AddFromFastAddCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    try
                    {
                        // 1. Remove from fast add Json
                        var fastAddList = _gridManagerService._fastAddAddonsService.LoadFastAddAddonsLocal();
                        fastAddList.RemoveAll(f => f.GitHubUrl == item.GitHubUrl);
                        await _gridManagerService._fastAddAddonsService.SaveFastAddAddonsLocalAsync(fastAddList);

                        // 2. Add to installed Json
                        var installed = _addonService.LoadAddonsFromLocal();
                        installed.Add(new AddonInfo
                        {
                            Name = item.Name,
                            GitHubUrl = item.GitHubUrl,
                            OldSha = "000000000", // SHA to get IsUpdated
                            IsUpdated = false
                        });
                        await _addonService.SaveAddonsListToJson(installed);

                        // 3. Reload both collections for UI
                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving addon: {ex.Message}");
                    }
                }
            });

            MoveFromCustomListFileCommand = new RelayCommand(async o =>
            {
                if (o is FastAddAddonInfo item)
                {
                    try
                    {
                        var installedAddons = _addonService.LoadAddonsFromLocal();
                        if (installedAddons.Any(a => a.GitHubUrl.Equals(item.GitHubUrl, StringComparison.OrdinalIgnoreCase)))
                        {
                            MessageBox.Show("This addon is already installed.");
                            return;
                        }

                        var newAddon = new AddonInfo
                        {
                            Name = item.Name,
                            GitHubUrl = item.GitHubUrl,
                            OldSha = "000000000",
                            IsUpdated = false
                        };

                        installedAddons.Add(newAddon);
                        await _addonService.SaveAddonsListToJson(installedAddons);


                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving addon from custom list: {ex.Message}");
                    }
                }
            });

            // Download reccomended list and save to CustomLists folder
            DownloadRecommendedList = new RelayCommand(async o =>
            {
                try
                {
                    IsLoading = true; // Show Spinner

                    using (var client = new HttpClient())
                    {
                        var content = await client.GetStringAsync(Pathing.RecommendedListUrl);

                        if (!Directory.Exists(Pathing.CustomAddOnsLists))
                            Directory.CreateDirectory(Pathing.CustomAddOnsLists);

                        await File.WriteAllTextAsync(Pathing.RecommendedFile, content);
                    }

                    // Reload Custom Lists
                    await LoadCustomListsAsync();

                    Debug.WriteLine("Recommended list downloaded successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading recommended list: {ex.Message}");
                }
                finally
                {
                    IsLoading = false;
                }
            });

            //Downloads a custom list and save to CustomLists folder
            DownloadCustomList = new RelayCommand(async o =>
            {
                try
                {

                    if (o is not CustomAddonList list)
                    {
                        MessageBox.Show("Invalid custom list.");
                        return;
                    }

                    string filePath = Path.Combine(Pathing.CustomAddOnsLists, list.ListName + ".txt");

                    string? repoUrl = CustomAddonList.GetRepoFileUrl(filePath);
                    // Button should be disable if no URL, just in case
                    if (string.IsNullOrEmpty(repoUrl))
                    {
                        MessageBox.Show("No file URL found in the custom list.");
                        return;
                    }

                    // Transforms to RAW URL
                    string rawUrl = GitHubService.ConvertToRawUrl(repoUrl);

                    using var client = new HttpClient();
                    string content = await client.GetStringAsync(rawUrl);

                    // Overwrite local file
                    await File.WriteAllTextAsync(filePath, content);

                    // Reload Custom Lists
                    await LoadCustomListsAsync();

                    Debug.WriteLine("Custom list downloaded successfully.");
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error downloading custom list: {ex.Message}");
                }

            });

            



            // Initial load
            _ = InitialLoadAsync();
            _ = LoadCustomListsAsync();
        }

        /// <summary>
        /// Rebuilds both Installed and Fast Add collections and updates the UI.
        /// </summary>
        private async Task ReloadBothAsync()
        {
            try
            {
                IsLoading = true;

                // Rebuild installed
                ReplaceCollection(InstalledAddons, _gridManagerService.BuildInstalledItems());

                // Rebuild fastadd filtered by installed
                ReplaceCollection(FastAddAddons, _gridManagerService.BuildFastAddItemsFiltered(InstalledAddons));

                OnPropertyChanged(nameof(InstalledAddons));
                OnPropertyChanged(nameof(FastAddAddons));

                await Task.CompletedTask;
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Initial load: refreshes from GitHub (status), saves local JSON, then populates both lists.
        /// </summary>
        private async Task InitialLoadAsync()
        {
            try
            {
                IsLoading = true;

                // Pull latest status and save to local JSON
                var completedList = await _addonService.CreateCompleteListAsync();
                //await _addonService.SaveAddonsListToJson(completedList); done in CreateCompleteListAsync()

                await ReloadBothAsync();
            }
            finally
            {
                IsLoading = false;
            }
        }

        /// <summary>
        /// Utility to replace contents of an ObservableCollection without breaking bindings.
        /// </summary>
        private static void ReplaceCollection<T>(ObservableCollection<T> target, ObservableCollection<T> source)
        {
            target.Clear();
            foreach (var item in source)
                target.Add(item);
        }

        /// <summary>
        /// Loads X amount of list equal to the .txt in CustomLists Folder
        /// </summary>
        private async Task LoadCustomListsAsync()
        {
            var lists = AddonService.LoadAllCustomAddonLists();
            CustomAddonLists.Clear();
            foreach (var list in lists)
                CustomAddonLists.Add(list);
        }



    }
}
