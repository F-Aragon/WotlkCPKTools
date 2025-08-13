using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Documents;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class AddonService
    {
        private readonly string _filePath;
        

        public AddonService(string filePath = @"C:\Users\f\Desktop\TestWOW\testData\storedAddons.json")
        {
            _filePath = filePath;
        }

        // Load Addons from Local JSON file
        public List<AddonInfo> LoadAddonsFromLocal()
        {
            if (!File.Exists(_filePath)) return new List<AddonInfo>();

            try
            {
                
                string json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<AddonInfo>>(json);

                foreach (var addon in list) {
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

        //Load from local, gets commit info from GitHub api, an refreshes the update status of each addon.
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
                        MessageBox.Show("New SHA: " + addon.NewSha + "\n" +
                                        "commit SHA: " + commitInfo.Sha);
                    }
  
                }
                addon.RefreshUpdateStatus();
            }
            await SaveAddonsListToJson(completedList);
            return completedList;
        }

        public async Task DownloadAddonsAsync(AddonInfo AddonInfo)
        {
            GitHubService _gitHubService = new GitHubService();
            //1. Download zip from GitHub
            string? zipPath = await _gitHubService.DownloadZipAsync(AddonInfo.GitHubUrl); //Using default path, add relative addon folder path later DownloadZipAsync(addon.GitHubUrl, relativePath)

            if (zipPath == null)
            {
                Debug.WriteLine($"Failed to download zip for addon: {AddonInfo.Name}");
                return; // Skip this addon if download failed
            }

            // 2. unZip
            string extractPath = ExtractZipToTempFolder(zipPath);

            // 3. Remove old folders in interface\AddOns
            RemoveAddonFolders(AddonInfo);

            // 4. Copy addon contents to AddOns folder, get a list of the folders that were copied
            List<string> copiedAddonFolders = CopyAddonToAddonsFolder(extractPath);

            // 5. Update AddonInfo with new folders.
            AddonInfo.LocalFolders = copiedAddonFolders;

            // 6. Erase temporary files
            DeleteTemporaryFiles(zipPath, extractPath);

        }

        //Create a new AddonInfo from GitHub URL
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
        
        //Check if Addon exists in Local Data
        public async Task<bool> AddonExistsAsync(string gitHubUrl)
        {
            var addons = LoadAddonsFromLocal(); 
            return addons.Any(a => a.GitHubUrl.Equals(gitHubUrl, StringComparison.OrdinalIgnoreCase));
        }

        // Remove Addon from Local Data
        public async Task<bool> RemoveAddonWithButton (string addonName)
        {
            try
            {
                List<AddonInfo> storedAddons = LoadAddonsFromLocal();
                var match = storedAddons.Find(o => o.Name.Equals(addonName, StringComparison.OrdinalIgnoreCase));
                if (match != null)
                {
                    RemoveAddonFolders(match);
                }

                storedAddons.RemoveAll(o => o.Name.Equals(addonName, StringComparison.OrdinalIgnoreCase));
                await SaveAddonsListToJson(storedAddons);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when trying to delete addon: {ex.Message}");
                return false;
            }

        }

        //Creates temporary folder and extracts the zip file to it
        private static string ExtractZipToTempFolder(string zipFilePath, string baseTempPath = @"C:\Users\f\Desktop\TestWOW\testData")
        {
            string zipName = Path.GetFileNameWithoutExtension(zipFilePath);
            string tempFolder = Path.Combine(baseTempPath, zipName + "_temp");

            if (Directory.Exists(tempFolder))
                Directory.Delete(tempFolder, true);

            Directory.CreateDirectory(tempFolder);

            ZipFile.ExtractToDirectory(zipFilePath, tempFolder);

            return tempFolder; 
        }

        //Removes addon folders from the WoW AddOns directory
        public static void RemoveAddonFolders(AddonInfo addon, string? addonsPath = @"C:\Users\f\Desktop\TestWOW\Interface\AddOns")
        {
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

        private List<string> CopyAddonToAddonsFolder(string extractedPath, string addonsPath = @"C:\Users\f\Desktop\TestWOW\Interface\AddOns")
        {
            List<string> copiedFolders = new();

            // Get zipName, remove "_temp"
            string zipName = Path.GetFileName(extractedPath);
            string cleanZipName = zipName.Substring(0, zipName.Length - "_temp".Length);

            // Get the subDirectories in the extracted path
            string[] subDirs = Directory.GetDirectories(extractedPath);
            if (subDirs.Length == 1 && (subDirs[0].EndsWith("-master") || subDirs[0].EndsWith("-main")))
            {
                string rootFolder = subDirs[0];
                //RootFolder is -master or -main, on the next line we get the inner directories
                var innerDirs = Directory.GetDirectories(rootFolder);

                if (innerDirs.Length == 0)
                {
                    // case 1: if there is only archives, creates a new folder with the zip name
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
                    // case 2: If there are inner directories, copy them to the addons folder
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
        // Aux method to copy directories recursively
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

        // Erase temporary files, zip and extracted folders
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

        //Check if all addons are updated, for single addons use AddonInfo.RefreshUpdateStatus()
        public static void RefreshAllUpdateStatus(List<AddonInfo> addons)
        {
            foreach (var addon in addons)
            {
                addon.RefreshUpdateStatus();
            }
        }

        //Update Addon files
        private async Task<bool> UpdateAddonAsync(AddonInfo addon, bool _forceUpdate = false)
        {
            
            
            if (addon.IsUpdated && !_forceUpdate)
            {
                Debug.WriteLine($"Addon {addon.Name} ya está actualizado.");
                return false;
            }
            // Download 
            await DownloadAddonsAsync(addon);

            addon.OldSha = addon.NewSha;
            addon.OldCommitDate = addon.NewCommitDate;
            addon.LastUpdateDate = DateTime.Now;
            addon.RefreshUpdateStatus();
            Debug.WriteLine($"Addon {addon.Name} actualizado correctamente.");
            return true;
        }

        //Update addon files with UpdateAddonAsync and write to Json
        public async Task UpdateAddonAndSaveAsync(AddonInfo addon, bool _forceUpdate = false)
        {
            bool changed = await UpdateAddonAsync(addon, _forceUpdate);
            if (!changed) return; // No changes, exit without saving

            var allAddons = LoadAddonsFromLocal();
            var index = allAddons.FindIndex(a => a.GitHubUrl == addon.GitHubUrl);
            if (index != -1)
                allAddons[index] = addon;
            else
                allAddons.Add(addon);

            await SaveAddonsListToJson(allAddons);
        }

        //Update and save Json of a list of addons if one changed
        public async Task UpdateAllAddonsAndSaveAsync(List<AddonInfo> addons, bool _forceUpdate = false)
        {
            bool anyChanged = false;
            var allAddons = LoadAddonsFromLocal();

            foreach (var addon in addons)
            {
                Debug.WriteLine($"Updating addon: {addon.Name}...");
                bool changed = await UpdateAddonAsync(addon, _forceUpdate);
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

        //For one addon, calls SaveAddons with a list containing the new addon
        public async Task<bool> AddAddonToLocalJsonAsync(AddonInfo newFullAddon)
        {
            List<AddonInfo> fullAddonInfoList = LoadAddonsFromLocal();

            if (fullAddonInfoList.Any(a => a.GitHubUrl.Equals(newFullAddon.GitHubUrl, StringComparison.OrdinalIgnoreCase)))
                return false;

            fullAddonInfoList.Add(newFullAddon);
            await SaveAddonsListToJson(fullAddonInfoList);
            return true;
        }
        
        //To save Addon or List
        public async Task SaveAddonsListToJson(List<AddonInfo> AddonInfoList, bool _download = false)
        {
            // 8. Save to json file
            try
            {
                string json = JsonSerializer.Serialize(AddonInfoList, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
                Debug.WriteLine("Addons saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when saving addons: {ex.Message}");
            }
        }
        

    }
}


