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

            // Asegurar que la carpeta temp exista
            Directory.CreateDirectory(Pathing.TempFolder);
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
                repoUrl = repoUrl.Substring(0, repoUrl.Length - 4);

            var uri = new Uri(repoUrl);
            var segments = uri.Segments;

            if (segments.Length >= 3)
            {
                string owner = segments[1].TrimEnd('/');
                string repo = segments[2].TrimEnd('/');
                return $"https://api.github.com/repos/{owner}/{repo}/commits";
            }
            else
            {
                throw new ArgumentException("Invalid GitHub repository URL format.");
            }
        }

        public static string GetName(string gitHubUrl)
        {
            if (gitHubUrl.EndsWith(".git"))
                gitHubUrl = gitHubUrl.Substring(0, gitHubUrl.Length - 4);

            return gitHubUrl.Split('/')[^1];
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
                    return branchElement.GetString();

                return null;
            }
            catch
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

        public async Task<string?> DownloadZipAsync(string repoUrl)
        {
            string repoName = GetName(repoUrl);
            string repoMainBranch = await GetDefaultBranchAsync(repoUrl);
            var (owner, repo) = GetOwnerAndRepo(repoUrl);

            string zipUrl = $"https://github.com/{owner}/{repo}/archive/refs/heads/{repoMainBranch}.zip";
            string zipFilePath = Path.Combine(Pathing.TempFolder, repoName + ".zip");

            try
            {
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

        public static async Task<List<string>> GetRawLinesAsync(string rawUrl)
        {
            using (var client = new HttpClient())
            {
                var content = await client.GetStringAsync(rawUrl);

                var lines = content.Split(new[] { '\r', '\n' });
                return new List<string>(lines);
            }
        }

        public static string ConvertToRawUrl(string githubUrl)
        {
            if (string.IsNullOrWhiteSpace(githubUrl))
                throw new ArgumentException("error: empty url");

            string rawUrl = githubUrl.Replace("github.com", "raw.githubusercontent.com")
                                     .Replace("/blob/", "/");

            return rawUrl;
        }



    }
}
