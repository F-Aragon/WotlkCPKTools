using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.Model
{
    public class CustomAddonList : INotifyPropertyChanged
    {
        public string ListName { get; set; }  // FileName
        public ObservableCollection<FastAddAddonInfo> Addons { get; set; }

        private string? _repoFileUrl;
        public string? RepoFileUrl
        {
            get => _repoFileUrl;
            set
            {
                if (_repoFileUrl != value)
                {
                    _repoFileUrl = value;
                    OnPropertyChanged(nameof(RepoFileUrl));
                }
            }
        }

        private bool _isUpToDate;
        public bool IsUpToDate
        {
            get => _isUpToDate;
            set
            {
                if (_isUpToDate != value)
                {
                    _isUpToDate = value;
                    OnPropertyChanged(nameof(IsUpToDate));
                }
            }
        }

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        /*public static async Task UpdateCustomListAsync(string remoteUrl)
        {
            string rawUrl = GitHubService.ConvertToRawUrl(remoteUrl); // convertir a raw
            string content = await new HttpClient().GetStringAsync(rawUrl);

            if (!Directory.Exists(Pathing.CustomAddOnsLists))
                Directory.CreateDirectory(Pathing.CustomAddOnsLists);

            string localFileName = GitHubService.GetName(remoteUrl); // devuelve customListOnlineTest.txt
            string localPath = Path.Combine(Pathing.CustomAddOnsLists, localFileName);

            await File.WriteAllTextAsync(localPath, content);
        }*/


        //If a single char is different, it will return false

        public async Task CheckIfUpdatedAsync()
        {
            if (string.IsNullOrEmpty(RepoFileUrl))
            {
                IsUpToDate = false;
                return;
            }

            IsUpToDate = await IsUpdated(RepoFileUrl);
        }

        public static async Task<bool> IsUpdated(string remoteUrl)
        {
            // Convert the GitHub URL to its RAW version
            var rawUrl = GitHubService.ConvertToRawUrl(remoteUrl);

            // Fetch the remote file content, ignoring empty lines and trimming spaces
            var remoteLines = (await GitHubService.GetRawLinesAsync(rawUrl))
                                .Where(l => !string.IsNullOrWhiteSpace(l))
                                .Select(l => l.Trim());

            // Determine the local file path
            string customListName = GitHubService.GetName(remoteUrl);
            string customListFilePath = Path.Combine(Pathing.CustomAddOnsLists, customListName);

            Debug.WriteLine($"Testing file: {customListFilePath}");

            // If the local file doesn't exist, consider it not updated
            if (!File.Exists(customListFilePath))
                return false;

            // Read the local file content, ignoring empty lines and trimming spaces
            var localLines = File.ReadAllLines(customListFilePath)
                                 .Where(l => !string.IsNullOrWhiteSpace(l))
                                 .Select(l => l.Trim());

            // Compare line by line
            bool areEqual = remoteLines.SequenceEqual(localLines);

            Debug.WriteLine($"Are files equal: {areEqual}");

            return areEqual;
        }

        public static string? GetRepoFileUrl(string filePath)
        {
            var localLines = File.ReadAllLines(filePath);

            foreach (var line in localLines)
            {
                if (line.StartsWith('@')) return line.Substring(1).Trim();
            }

            return null;
        }

    }
}

