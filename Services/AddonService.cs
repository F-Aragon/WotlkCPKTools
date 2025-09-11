using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class AddonService
    {
        // Load Addons from Local JSON file
        public List<AddonInfo> LoadAddonsFromLocal()
        {
            if (!File.Exists(Pathing.storedAddonsFile)) return new List<AddonInfo>();

            try
            {
                string json = File.ReadAllText(Pathing.storedAddonsFile);
                var list = JsonSerializer.Deserialize<List<AddonInfo>>(json);

                foreach (var addon in list)
                {
                    addon.RefreshUpdateStatus();

                }

                return list ?? new List<AddonInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error trying to read addons: {ex.Message}");
                return new List<AddonInfo>();
            }
        }

        //Load from local, gets commit info from GitHub api, and refreshes the update status of each addon.
        public async Task<List<AddonInfo>> CreateCompleteListAsync()
        {
            List<AddonInfo> completedList = LoadAddonsFromLocal();
            GitHubService gitHubService = new GitHubService();

            foreach (AddonInfo addon in completedList)
            {
                var commitInfo = await gitHubService.GetLatestCommitInfoAsync(addon.GitHubUrl);
                if (commitInfo != null)
                {
                    if (commitInfo.Sha != addon.NewSha)
                    {
                        addon.OldSha = addon.NewSha;
                        addon.OldCommitDate = addon.NewCommitDate;

                        addon.NewSha = commitInfo.Sha;
                        addon.NewCommitDate = commitInfo.Date;
                    }
                }
                addon.RefreshUpdateStatus();
            }
            await SaveAddonsListToJson(completedList);
            return completedList;
        }

        public async Task<bool> DownloadAddonAsync(AddonInfo addonInfo, ObservableCollection<AddonItem> installedItems)
        {
            // Find the correct AddonItme in the InstalledAddons list
            var addonItem = installedItems.FirstOrDefault(a => a.GitHubUrl == addonInfo.GitHubUrl);
            if (addonItem == null) return false;

            GitHubService _gitHubService = new GitHubService();

            try
            {
                addonItem.DownloadStatus = "Downloading...";
                string? zipPath = await _gitHubService.DownloadZipAsync(addonInfo.GitHubUrl);

                if (zipPath == null)
                {
                    addonItem.DownloadStatus = "DownloadError";
                    return false;
                }

                addonItem.DownloadStatus = "Uziping...";
                string extractPath = ExtractZipToTempFolder(zipPath);

                addonItem.DownloadStatus = "Deleting Old...";
                RemoveAddonFolders(addonInfo);

                addonItem.DownloadStatus = "Copying...";
                List<string> copiedAddonFolders = CopyAddonToAddonsFolder(extractPath);
                addonInfo.LocalFolders = copiedAddonFolders;

                addonItem.DownloadStatus = "Done!";
                DeleteTemporaryFiles(zipPath, extractPath);

                return true;
            }
            catch (Exception ex)
            {
                addonItem.DownloadStatus = $"Error: {ex.Message}";
                return false;
            }
        }

        public async Task<AddonInfo>? CreateAddonInfoFromGitHubUrl(string gitHubUrl)
        {
            GitHubService gitHubService = new GitHubService();

            CommitInfo? addonCommitInfo = await gitHubService.GetLatestCommitInfoAsync(gitHubUrl);

            if (addonCommitInfo == null)
                return null;

            AddonInfo addonInfo = new AddonInfo
            {
                Name = GitHubService.GetName(gitHubUrl),
                GitHubUrl = gitHubUrl,
                LocalPath = null,
                NewSha = addonCommitInfo.Sha,
                OldSha = null,
                OldCommitDate = null,
                NewCommitDate = addonCommitInfo.Date,
                LocalFolders = null,
                IsUpdated = false
            };
            Debug.WriteLine($"Created AddonInfo: {addonInfo.Name} with SHA: {addonInfo.NewSha}");
            return addonInfo;
        }

        public async Task<bool> AddonExistsAsync(string gitHubUrl)
        {
            var addons = LoadAddonsFromLocal();
            return addons.Any(a => a.GitHubUrl.Equals(gitHubUrl, StringComparison.OrdinalIgnoreCase));
        }

        public async Task<bool> RemoveAddonWithButton(string gitHubUrl)
        {
            try
            {
                // Load all stored addons
                List<AddonInfo> storedAddons = LoadAddonsFromLocal();

                // Find the addon to remove
                var match = storedAddons.Find(a => a.GitHubUrl.Equals(gitHubUrl, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    // Get all folders from other addons (excluding the one being removed)
                    var otherFolders = storedAddons
                        .Where(a => a.GitHubUrl != gitHubUrl)
                        .SelectMany(a => a.LocalFolders ?? new List<string?>())
                        .Distinct()
                        .ToList();

                    // Iterate through the folders of the addon to remove
                    foreach (var folder in match.LocalFolders ?? new List<string?>())
                    {
                        if (string.IsNullOrEmpty(folder)) continue;

                        string fullPath = Path.Combine(Pathing.AddOns, folder);

                        if (!otherFolders.Contains(folder) && Directory.Exists(fullPath))
                        {
                            // Folder is not shared: safe to delete
                            Directory.Delete(fullPath, true);
                            Debug.WriteLine($"Deleted folder '{folder}' for addon '{match.Name}'.");
                        }
                        else
                        {
                            // Folder is shared with other addons
                            var sharedWith = storedAddons
                                .Where(a => a.GitHubUrl != gitHubUrl && (a.LocalFolders ?? new List<string?>()).Contains(folder))
                                .Select(a => a.Name);

                            Debug.WriteLine($"Skipping '{folder}' because it is shared with: {string.Join(", ", sharedWith)}");
                        }
                    }
                }

                // Remove the addon from the local list
                storedAddons.RemoveAll(a => a.GitHubUrl.Equals(gitHubUrl, StringComparison.OrdinalIgnoreCase));
                await SaveAddonsListToJson(storedAddons);

                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to delete addon: {ex.Message}");
                return false;
            }
        }


        private static string ExtractZipToTempFolder(string zipFilePath, string? baseTempPath = null)
        {
            baseTempPath ??= Pathing.TempFolder;
            string zipName = Path.GetFileNameWithoutExtension(zipFilePath);
            string tempFolder = Path.Combine(baseTempPath, zipName + "_temp");

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);

            ZipFile.ExtractToDirectory(zipFilePath, tempFolder);

            return tempFolder;
        }

        public static void RemoveAddonFolders(AddonInfo addon, string? addonsPath = null)
        {
            addonsPath ??= Pathing.AddOns;

            try
            {
                if (addon.LocalFolders == null || addon.LocalFolders.Count == 0)
                {
                    Debug.WriteLine($"No local folders to remove for addon: {addon.Name}");
                    return;
                }

                foreach (string? folderName in addon.LocalFolders)
                {
                    if (string.IsNullOrWhiteSpace(folderName))
                        continue;

                    string folderPath = Path.Combine(addonsPath, folderName);

                    if (Directory.Exists(folderPath))
                    {
                        Directory.Delete(folderPath, true);
                        Debug.WriteLine($"Deleted folder: {folderPath}");
                    }
                    else
                    {
                        Debug.WriteLine($"Folder not found: {folderPath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error removing addon folders for {addon.Name}: {ex.Message}");
            }
        }

        private List<string> CopyAddonToAddonsFolder(string extractedPath, string? addonsPath = null)
        {
            addonsPath ??= Pathing.AddOns;
            List<string> copiedFolders = new();

            string zipName = Path.GetFileName(extractedPath);
            string cleanZipName = zipName.Substring(0, zipName.Length - "_temp".Length);

            string[] subDirs = Directory.GetDirectories(extractedPath);
            if (subDirs.Length == 1 && (subDirs[0].EndsWith("-master") || subDirs[0].EndsWith("-main")))
            {
                string rootFolder = subDirs[0];
                var innerDirs = Directory.GetDirectories(rootFolder);

                if (innerDirs.Length == 0)
                {
                    string targetDir = Path.Combine(addonsPath, cleanZipName);
                    Directory.CreateDirectory(targetDir);
                    foreach (var file in Directory.GetFiles(rootFolder))
                    {
                        string destFile = Path.Combine(targetDir, Path.GetFileName(file));
                        File.Copy(file, destFile, overwrite: true);
                    }
                    copiedFolders.Add(cleanZipName);
                }
                else
                {
                    foreach (var dir in innerDirs)
                    {
                        string targetDir = Path.Combine(addonsPath, Path.GetFileName(dir));
                        CopyDirectory(dir, targetDir);
                        copiedFolders.Add(Path.GetFileName(dir));
                    }
                }
            }
            else
            {
                Debug.WriteLine($"Wrong Zip structure {extractedPath}.");
            }

            return copiedFolders;
        }

        private void CopyDirectory(string sourceDir, string destinationDir)
        {
            Directory.CreateDirectory(destinationDir);

            foreach (var file in Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories))
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);
                string destFile = Path.Combine(destinationDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(file, destFile, overwrite: true);
            }
        }

        private static void DeleteTemporaryFiles(string zipFilePath, string extractedFolderPath)
        {
            try
            {
                if (File.Exists(zipFilePath))
                    File.Delete(zipFilePath);

                if (Directory.Exists(extractedFolderPath))
                    Directory.Delete(extractedFolderPath, recursive: true);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error deleting temporary files: {ex.Message}");
            }
        }

        public static void RefreshAllUpdateStatus(List<AddonInfo> addons)
        {
            foreach (var addon in addons)
            {
                addon.RefreshUpdateStatus();
            }
        }

        private async Task<bool> UpdateAddonAsync(AddonInfo addon, ObservableCollection<AddonItem> installedItems, bool _forceUpdate = false)
        {
            if (addon.IsUpdated && !_forceUpdate)
            {
                Debug.WriteLine($"Addon {addon.Name} already updated.");
                return false;
            }

            GitHubService gitHubService = new GitHubService();
            var commitInfo = await gitHubService.GetLatestCommitInfoAsync(addon.GitHubUrl);
            if (commitInfo != null)
            {
                bool succesDownloading = await DownloadAddonAsync(addon, installedItems);
                if (succesDownloading)
                {
                    addon.NewSha = commitInfo.Sha;
                    addon.NewCommitDate = commitInfo.Date;
                    addon.OldSha = addon.NewSha;
                    addon.OldCommitDate = addon.NewCommitDate;
                    addon.LastUpdateDate = DateTime.Now;
                    addon.IsUpdated = true;
                    Debug.WriteLine($"Addon {addon.Name} updated. (UpdateAddonAsync) ");
                }
                else
                {
                    Debug.WriteLine($"ERROR: Addon {addon.Name} NOT updated. (UpdateAddonAsync) ");
                    return false;
                }
            }
            else
            {
                Debug.WriteLine($"No commit info found for {addon.Name}");
                return false;
            }

            addon.RefreshUpdateStatus();
            
            return true;
        }

        public async Task UpdateAddonAndSaveAsync(AddonInfo addon, ObservableCollection<AddonItem> installedItems, bool _forceUpdate = false)
        {
            bool changed = await UpdateAddonAsync(addon, installedItems, _forceUpdate);
            
            if (!changed) return;

            var allAddons = LoadAddonsFromLocal();
            var index = allAddons.FindIndex(a => a.GitHubUrl == addon.GitHubUrl);
            if (index != -1)
                allAddons[index] = addon;
            else
                allAddons.Add(addon);

            await SaveAddonsListToJson(allAddons);
        }

        public async Task UpdateAllAddonsAndSaveAsync(List<AddonInfo> addons, ObservableCollection<AddonItem> installedItems, bool _forceUpdate = false)
        {
            bool anyChanged = false;
            var allAddons = LoadAddonsFromLocal();

            foreach (var addon in addons)
            {
                Debug.WriteLine($"Updating addon: {addon.Name}...");
                bool changed = await UpdateAddonAsync(addon, installedItems, _forceUpdate);
                if (changed)
                {
                    var index = allAddons.FindIndex(a => a.GitHubUrl == addon.GitHubUrl);
                    if (index != -1)
                        allAddons[index] = addon;
                    else
                        allAddons.Add(addon);

                    anyChanged = true;
                }
            }
            if (anyChanged)
                await SaveAddonsListToJson(allAddons);
        }

        public async Task<bool> AddAddonToLocalJsonAsync(AddonInfo newFullAddon)
        {
            List<AddonInfo> fullAddonInfoList = LoadAddonsFromLocal();

            if (fullAddonInfoList.Any(a => a.GitHubUrl.Equals(newFullAddon.GitHubUrl, StringComparison.OrdinalIgnoreCase)))
                return false;

            fullAddonInfoList.Add(newFullAddon);
            await SaveAddonsListToJson(fullAddonInfoList);  
            return true;
        }
        
        public async Task SaveAddonsListToJson(List<AddonInfo> AddonInfoList, bool _download = false)
        {

            try
            {
                string json = JsonSerializer.Serialize(AddonInfoList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(Pathing.storedAddonsFile, json);
                Debug.WriteLine("Json saved successfully. (SaveAddonsListToJson)");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when saving addons: {ex.Message} (SaveAddonsListToJson)");
            }
        }






        //CustomList Methods
        /// <summary>
        /// Loads a single custom addon list from a file.
        /// Each addon line must be in the format: Name: GitHubURL
        /// </summary>
        public static CustomAddonList LoadCustomAddonList(string filePath)
        {
            var listName = Path.GetFileNameWithoutExtension(filePath);
            string[] lines;

            try
            {
                lines = File.ReadAllLines(filePath);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error reading customlist {filePath}: {ex.Message}");
                return new CustomAddonList { ListName = listName, Addons = new ObservableCollection<FastAddAddonInfo>() };
            }

            var addons = FastAddAddonInfoService.ParseAddonFileLines(lines);
            string? repoFileUrl = CustomAddonList.GetRepoFileUrl(filePath);

            return new CustomAddonList { ListName = listName, Addons = addons, RepoFileUrl = repoFileUrl };
        }

        /// <summary>
        /// Loads all custom addon lists from Pathing.CustomAddOnsLists directory.
        /// </summary>
        public static ObservableCollection<CustomAddonList> LoadAllCustomAddonLists()
        {
            var allLists = new ObservableCollection<CustomAddonList>();
            
            if (!Directory.Exists(Pathing.CustomAddOnsLists))
                return allLists;

            foreach (var file in Directory.GetFiles(Pathing.CustomAddOnsLists, "*.txt"))
            {
                var customList = LoadCustomAddonList(file); // Reuse method

                allLists.Add(customList);
            }

            return allLists;
        }

        // Info from datagrid
        public async Task UpdateAddonFromDataGridAsync(string githubUrl, string newName)
        {
            var addons = LoadAddonsFromLocal(); 
            var existing = addons.FirstOrDefault(a => a.GitHubUrl.Equals(githubUrl, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Name = newName; 
                await SaveAddonsListToJson(addons); 
            }
        }



    }
}
