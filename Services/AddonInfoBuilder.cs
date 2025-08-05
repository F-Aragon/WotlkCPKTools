using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    class AddonInfoBuilder
    {
        //Things for testing, remove later

        string testApiUrl = "https://api.github.com/repos/NoM0Re/WeakAuras-WotLK/commits";
        string testRepoUrl = "https://github.com/NoM0Re/WeakAuras-WotLK";
        string localAddonsFolder = "C:\\Users\\f\\Desktop\\TestWOW\\Interface\\AddOns";

        //End of things for testing, remove later


        public static string GetName(string repoUrl)
        {
            string Name = repoUrl.Split("/").Last();

            return Name;
        }

        public static string GetUrl(string repoUrl)
        {
            return repoUrl;
        }

        public async Task <CommitInfo> GetShaAndCommitDateAsync(string testApiUrl)
        {
            HttpClient httpClient = new HttpClient();
            httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WotlkCPKTools");

            try
            {
                var httpClientResponse = await httpClient.GetAsync(testApiUrl);
                // Read the string from the response
                string jsonResponse = await httpClientResponse.Content.ReadAsStringAsync();
                var commits = JsonDocument.Parse(jsonResponse).RootElement;

                string sha = commits[0].GetProperty("sha").GetString();
                DateTime date = commits[0].GetProperty("commit").GetProperty("committer").GetProperty("date").GetDateTime();



                return new CommitInfo { Sha = sha, Date = date };
            }
            catch (Exception)
            {
                return null;
            }
            finally
            {
                httpClient.Dispose();
            }
        }

        //Get Local Data
        public async Task<StoredAddonInfo> GetLocalAddonInfoAsync(string repoUrl)
        {

            StoredAddonInfo storedAddonInfo = new StoredAddonInfo();

            //string _jsonFilePath = AppDomain.CurrentDomain.BaseDirectory+"\\storedAddons.json";
            string _jsonFilePath = "C:\\Users\\f\\Desktop\\TestWOW\\testData" + "\\storedAddons.json";
            string _jsonContent = await File.ReadAllTextAsync(_jsonFilePath);




            List<StoredAddonInfo> allStoredAddons = JsonSerializer.Deserialize<List<StoredAddonInfo>>(_jsonContent);

            storedAddonInfo = allStoredAddons.FirstOrDefault(a => a.GitHubUrl.Equals(repoUrl, StringComparison.OrdinalIgnoreCase));

            return storedAddonInfo;
        }
        
    }
    

}
