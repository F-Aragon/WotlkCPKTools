using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class FastAddAddonsService
    {
        public async Task<List<FastAddAddonInfo>> LoadFastAddAddonsAsync()
        {
            if (!File.Exists(Pathing.fastAddAddonsFile))
                return new List<FastAddAddonInfo>();

            string json = await File.ReadAllTextAsync(Pathing.fastAddAddonsFile);
            return JsonSerializer.Deserialize<List<FastAddAddonInfo>>(json) ?? new List<FastAddAddonInfo>();
        }
    }
}