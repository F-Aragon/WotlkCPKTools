using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.Json;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    internal class AddonService
    {
        private readonly string _filePath;
        

        public AddonService(string filePath = @"C:\Users\f\Desktop\TestWOW\testData\storedAddons.json")
        {
            _filePath = filePath;
        }
        
        // Load Addons from Local JSON file
        public List<StoredAddonInfo> LoadAddonsFromLocal()
        {
            if (!File.Exists(_filePath)) return new List<StoredAddonInfo>();

            try
            {
                string json = File.ReadAllText(_filePath);
                var list = JsonSerializer.Deserialize<List<StoredAddonInfo>>(json);
                return list ?? new List<StoredAddonInfo>();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error trying to read addons: {ex.Message}");
                return new List<StoredAddonInfo>();
            }
        }

        public List<AddonInfo> CreateAddonInfoListFromStored()
        {
            List<StoredAddonInfo> _storedList = LoadAddonsFromLocal();
            List<AddonInfo> _fullList = new List<AddonInfo>();

            foreach (StoredAddonInfo storedAddon in _storedList)
            {
                AddonInfo addonInfo = new AddonInfo
                {
                    Name = storedAddon.Name,
                    GitHubUrl = storedAddon.GitHubUrl,
                    LocalPath = storedAddon.LocalPath,
                    OldSha = storedAddon.Sha,
                    LastUpdated = storedAddon.LastUpdated
                };
                _fullList.Add(addonInfo);
            }

            return _fullList;
        }

        public async Task<List<AddonInfo>> CreateCompleteListAsync()
        {
            List<AddonInfo> completedList = CreateAddonInfoListFromStored();
            GitHubService gitHubService = new GitHubService();

            foreach (AddonInfo addon in completedList)
            {
                var commitInfo = await gitHubService.GetLatestCommitInfoAsync(addon.GitHubUrl);
                if (commitInfo != null)
                {
                    addon.NewSha = commitInfo.Sha;
                    addon.NewCommitDate = commitInfo.Date;
                }
            }

            return completedList;
        }

        //To save Addons or List
        public void SaveAddons(List<AddonInfo> addonInfos)
        {
            var storedAddons = addonInfos.Select(a => new StoredAddonInfo
            {
                Name = a.Name,
                GitHubUrl = a.GitHubUrl,
                LocalPath = a.LocalPath,
                Sha = a.NewSha,
                LastUpdated = a.NewCommitDate
            }).ToList();

            try
            {
                string json = JsonSerializer.Serialize(storedAddons, new JsonSerializerOptions { WriteIndented = true });
                File.WriteAllText(_filePath, json);
                Debug.WriteLine("Addons saved successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error when saving addons: {ex.Message}");
            }
        }

        //For one addon, calls SaveAddons with a list containing the new addon
        public async Task<bool> AddAddonToLocalJsonAsync(AddonInfo newFullAddon)
        {
            List<AddonInfo> fullAddonInfoList = await CreateCompleteListAsync();

            if (fullAddonInfoList.Any(a => a.GitHubUrl.Equals(newFullAddon.GitHubUrl, StringComparison.OrdinalIgnoreCase)))
                return false;

            fullAddonInfoList.Add(newFullAddon);
            SaveAddons(fullAddonInfoList);
            return true;
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
                LastUpdated = null,
                NewCommitDate = addonCommitInfo.Date
            };
            Debug.WriteLine($"Created AddonInfo: {addonInfo.Name} with SHA: {addonInfo.NewSha}");
            return addonInfo;
        }

        //Check if Addon exists in Local Data
        public async Task<bool> AddonExistsAsync(string gitHubUrl)
        {
            var addons = await CreateCompleteListAsync();
            return addons.Any(a => a.GitHubUrl.Equals(gitHubUrl, StringComparison.OrdinalIgnoreCase));
        }


        public async Task<bool> TryAddAddonFromUrl(string gitHubUrl)
        {
            AddonInfo? addon = await CreateAddonInfoFromGitHubUrl(gitHubUrl);
            if (addon == null)
                return false;

            return await AddAddonToLocalJsonAsync(addon); 
        }




        //Convert AddonInfo to StoredAddonInfo, not used for now
        public static StoredAddonInfo AddonInfoToStoredAddonInfo(AddonInfo addon)
        {
            var storedAddon = new StoredAddonInfo
            {
                Name = addon.Name,
                GitHubUrl = addon.GitHubUrl,
                LocalPath = addon.LocalPath,
                Sha = addon.NewSha,
                LastUpdated = addon.NewCommitDate
            };

            return storedAddon;
        }

    }
}


