using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class FastAddAddonsService
    {
        private readonly HttpClient _httpClient;
        private const string FastAddJsonUrl = "https://raw.githubusercontent.com/FranciscoRAragon/WotlkCPKTools/master/Resources/Data/fastAddAddons.json";

        public FastAddAddonsService(HttpClient? httpClient = null)
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

    }
}
