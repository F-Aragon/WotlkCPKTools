using System;
using System.IO;
using System.Net.Http;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class GitHubService
    {
        private readonly HttpClient _httpClient;

        public GitHubService()
        {
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("WotlkCPKTools");
        }
        
        public async Task<CommitInfo?> GetLatestCommitInfoAsync(string repoUrl)
        {
            string repoApiUrl = ConvertRepoUrlToApiUrl(repoUrl);
            try
            {
                var response = await _httpClient.GetAsync(repoApiUrl);
                response.EnsureSuccessStatusCode();

                string jsonResponse = await response.Content.ReadAsStringAsync();
                var commits = JsonDocument.Parse(jsonResponse).RootElement;

                if (commits.GetArrayLength() == 0)
                    return null;

                var latestCommit = commits[0];
                string sha = latestCommit.GetProperty("sha").GetString();
                DateTime date = latestCommit.GetProperty("commit")
                                          .GetProperty("committer")
                                          .GetProperty("date")
                                          .GetDateTime();

                return new CommitInfo { Sha = sha, Date = date };
            }
            catch (Exception)
            {
                return null;
            }
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        public static string ConvertRepoUrlToApiUrl(string repoUrl)
        {
            if (repoUrl.EndsWith(".git", StringComparison.OrdinalIgnoreCase))
            {
                repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);
            }

            var uri = new Uri(repoUrl);
            var segments = uri.Segments;

            if (segments.Length >= 3)
            {
                string owner = segments[1].TrimEnd('/');
                string repo = segments[2].TrimEnd('/');

                return ($"https://api.github.com/repos/{owner}/{repo}/commits");
            }
            else
            {
                throw new ArgumentException("Invalid GitHub repository URL format.");
            }
        }

        public static string GetName(string gitHubUrl)
        {
            if (gitHubUrl.EndsWith(".git"))
            {
                gitHubUrl = gitHubUrl.Substring(0, gitHubUrl.Length - 4);
            }

            string name = gitHubUrl.Split('/')[^1]; // gitHubUrl.Split('/') returns an array of strings and [^1] gets the last element of the array
            return name; 
        }


        public bool IsValidGitHubRepoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;

            return Regex.IsMatch(url, @"^https:\/\/github\.com\/[\w\-]+\/[\w\-]+(\.git)?$");

        }


        public async Task<string?> GetDefaultBranchAsync(string repoUrl)
        {
            try
            {
                string repoApiBase = ConvertRepoUrlToApiUrl(repoUrl).Replace("/commits", "");
                var response = await _httpClient.GetAsync(repoApiBase);
                if (!response.IsSuccessStatusCode)
                    return null;

                string json = await response.Content.ReadAsStringAsync();
                using var doc = JsonDocument.Parse(json);
                if (doc.RootElement.TryGetProperty("default_branch", out var branchElement))
                {
                    return branchElement.GetString();
                }

                return null;
            }
            catch (Exception)
            {
                Console.WriteLine("Error fetching default branch: " + repoUrl);
                return null;
            }
        }



        public static (string owner, string repo) GetOwnerAndRepo(string repoUrl)
        {
            Uri uri = new Uri(repoUrl);
            var segments = uri.Segments.Select(s => s.Trim('/')).Where(s => s != "").ToArray();
  
            return (segments[0], segments[1]);
        }



        
        public async Task<string?> DownloadZipAsync(string repoUrl, string tempFolderPath = @"C:\Users\f\Desktop\TestWOW\CPKtemp") 
        {

            //https://github.com/ElvUI-WotLK/ElvUI

            string _repoName = GetName(repoUrl);
            string _repoMainBranch = await GetDefaultBranchAsync(repoUrl);
            var (_owner, _repo) = GetOwnerAndRepo(repoUrl);

            string zipUrl = $"https://github.com/{_owner}/{_repo}/archive/refs/heads/{_repoMainBranch}.zip";


            try
            {

                if (!Directory.Exists(tempFolderPath)) 
                {
                    Directory.CreateDirectory(tempFolderPath);
                }


                // Temp Zip file path
                string zipFilePath = Path.Combine(tempFolderPath, _repoName+".zip");

                using var response = await _httpClient.GetAsync(zipUrl);
                if (!response.IsSuccessStatusCode)
                    return null;

                await using var fs = new FileStream(zipFilePath, FileMode.Create, FileAccess.Write, FileShare.None);
                await response.Content.CopyToAsync(fs);

                return zipFilePath;
            }
            catch
            {
                return null;
            }
        }


    }
}
