using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class FastAddAddonInfoService
    {
        private readonly HttpClient _httpClient;
        private const string FastAddJsonUrl = "https://raw.githubusercontent.com/FranciscoRAragon/WotlkCPKTools/master/Resources/Data/fastAddAddons.json";

        public FastAddAddonInfoService(HttpClient? httpClient = null)
        {
            _httpClient = httpClient ?? new HttpClient();
        }

        // --- Online ---
        public async Task<List<FastAddAddonInfo>> LoadFastAddAddonsOnlineAsync()
        {
            try
            {
                string json = await _httpClient.GetStringAsync(FastAddJsonUrl);
                return JsonSerializer.Deserialize<List<FastAddAddonInfo>>(json) ?? new List<FastAddAddonInfo>();
            }
            catch (Exception)
            {
                return new List<FastAddAddonInfo>();
            }
        }

        // ---  Local ---
        public async Task<List<FastAddAddonInfo>> LoadFastAddAddonsLocalAsync()
        {
            if (!File.Exists(Pathing.fastAddAddonsFile))
                return new List<FastAddAddonInfo>();

            try
            {
                string json = await File.ReadAllTextAsync(Pathing.fastAddAddonsFile);
                return JsonSerializer.Deserialize<List<FastAddAddonInfo>>(json) ?? new List<FastAddAddonInfo>();
            }
            catch (Exception)
            {
                return new List<FastAddAddonInfo>();
            }
        }

        public List<FastAddAddonInfo> LoadFastAddAddonsLocal()
        {
            if (!File.Exists(Pathing.fastAddAddonsFile))
                return new List<FastAddAddonInfo>();

            try
            {
                string json = File.ReadAllText(Pathing.fastAddAddonsFile);
                return JsonSerializer.Deserialize<List<FastAddAddonInfo>>(json) ?? new List<FastAddAddonInfo>();
            }
            catch
            {
                return new List<FastAddAddonInfo>();
            }
        }

        // In FastAddAddonsService
        public async Task AddFastAddAddonAsync(string name, string gitHubUrl)
        {
            var fastAddList = LoadFastAddAddonsLocal();
            if (fastAddList.Any(f => f.GitHubUrl == gitHubUrl))
                return; // If already exists, do nothing

            fastAddList.Add(new FastAddAddonInfo { Name = name, GitHubUrl = gitHubUrl });
            string json = JsonSerializer.Serialize(fastAddList, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Pathing.fastAddAddonsFile, json);
        }

        public async Task RemoveFastAddAddonAsync(string githubUrl)
        {
            var list = await LoadFastAddAddonsLocalAsync();
            var item = list.FirstOrDefault(a => a.GitHubUrl == githubUrl);
            if (item != null)
            {
                list.Remove(item);
                await SaveFastAddAddonsLocalAsync(list);
            }
        }

        public async Task SaveFastAddAddonsLocalAsync(List<FastAddAddonInfo> list)
        {
            string json = JsonSerializer.Serialize(list, new JsonSerializerOptions { WriteIndented = true });
            await File.WriteAllTextAsync(Pathing.fastAddAddonsFile, json);
        }

        public async Task UpdateFastAddonFromDataGridAsync(FastAddAddonInfo updatedAddon)
        {
            var fastAddList = LoadFastAddAddonsLocal();
            var existing = fastAddList.FirstOrDefault(f => f.GitHubUrl.Equals(updatedAddon.GitHubUrl, StringComparison.OrdinalIgnoreCase));

            if (existing != null)
            {
                existing.Name = updatedAddon.Name;
                await File.WriteAllTextAsync(Pathing.fastAddAddonsFile,
                    JsonSerializer.Serialize(fastAddList, new JsonSerializerOptions { WriteIndented = true }));
            }
        }

    }
}
