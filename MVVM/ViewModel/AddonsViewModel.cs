using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
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

        public AddonsViewModel()
        {
            _addonService = new AddonService();
            _gridManagerService = new GridManagerService(_addonService, new GitHubService(), new FastAddAddonsService());

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
            // TODO: If later you want to also append it to FastAdd file, add that logic.
            RemoveInstalledCommand = new RelayCommand(o =>
            {
                if (o is AddonItem item)
                {
                    _gridManagerService.RemoveInstalled(item);
                    _ = ReloadBothAsync();
                }
            });

            // Add from Fast Add to Installed
            // TODO: Needs an Add/Install method in AddonService; for now show a message.
            AddFromFastAddCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    try
                    {
                        // If you have a method like InstallAddonFromGitHubAsync, call it here.
                        // await _addonService.InstallAddonFromGitHubAsync(item.GitHubLink);
                        MessageBox.Show("Add from Fast Add is not implemented yet."); // placeholder
                        await Task.CompletedTask;
                    }
                    finally
                    {
                        _ = ReloadBothAsync();
                    }
                }
            });

            // Delete from Fast Add list (file write not implemented in FastAddAddonsService yet)
            DeleteFastAddCommand = new RelayCommand(o =>
            {
                if (o is AddonItem item)
                {
                    // UI-only removal for now; reloading from disk will bring it back until you implement save.
                    var toRemove = FastAddAddons.FirstOrDefault(x => x.GitHubLink == item.GitHubLink);
                    if (toRemove != null)
                        FastAddAddons.Remove(toRemove);

                    MessageBox.Show("Delete from Fast Add is UI-only for now. Implement file save to persist.");
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
                        // 1. Eliminar del Fast Add JSON
                        await _gridManagerService._fastAddAddonsService.RemoveFastAddAddonAsync(item.GitHubLink);

                        // 2. Recargar UI
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
                        // 1. Agregar a Fast Add JSON
                        await _gridManagerService._fastAddAddonsService.AddFastAddAddonAsync(item.Name, item.GitHubLink);
                        
                        // 2. Eliminar del Installed (archivos y JSON)
                        await _addonService.RemoveAddonWithButton(item.Name);

                        // 3. Recargar las colecciones para UI 
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
                        // 1. Quitar de Fast Add
                        var fastAddList = _gridManagerService._fastAddAddonsService.LoadFastAddAddonsLocal();
                        fastAddList.RemoveAll(f => f.GitHubUrl == item.GitHubLink);
                        await _gridManagerService._fastAddAddonsService.SaveFastAddAddonsLocalAsync(fastAddList);

                        // 2. Agregar a Installed con SHA ficticio
                        var installed = _addonService.LoadAddonsFromLocal();
                        installed.Add(new AddonInfo
                        {
                            Name = item.Name,
                            GitHubUrl = item.GitHubLink,
                            OldSha = "000000000", // SHA ficticio
                            IsUpdated = false
                        });
                        await _addonService.SaveAddonsListToJson(installed);

                        // 3. Recargar colecciones
                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error moving addon: {ex.Message}");
                    }
                }
            });

            // Delete from Fast Add
            DeleteFastAddCommand = new RelayCommand(async o =>
            {
                if (o is AddonItem item)
                {
                    try
                    {
                        // 1. Quitar del JSON
                        var fastAddList = _gridManagerService._fastAddAddonsService.LoadFastAddAddonsLocal();
                        fastAddList.RemoveAll(f => f.GitHubUrl == item.GitHubLink);
                        await _gridManagerService._fastAddAddonsService.SaveFastAddAddonsLocalAsync(fastAddList);

                        // 2. Recargar UI
                        await ReloadBothAsync();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Error deleting addon: {ex.Message}");
                    }
                }
            });


            // Initial load
            _ = InitialLoadAsync();
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

                // Rebuild fast add filtered by installed
                ReplaceCollection(FastAddAddons, _gridManagerService.BuildFastAddItemsFiltered(InstalledAddons));

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
                await _addonService.SaveAddonsListToJson(completedList);

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

        
    }
}
