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

        // Implementación de INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        /*
        public static async Task UpdateCustomListAsync(string remoteUrl)
        {

            bool _isUpdated = await IsUpdated(remoteUrl);
            
            if (!_isUpdated)
            {
                var client = new HttpClient();
                var content = await client.GetStringAsync(Pathing.RecommendedListUrl);

                if (!Directory.Exists(Pathing.CustomAddOnsLists))
                    Directory.CreateDirectory(Pathing.CustomAddOnsLists);

                await File.WriteAllTextAsync(Pathing.RecommendedFile, content);
            }

            
        }*/

        //If a single char is different, it will return false
        public static async Task<bool> IsUpdated(string remoteUrl)
        {
            // get online file content
            var remoteLines = await GitHubService.GetRawLinesAsync(remoteUrl);

            // get local file content
            string customListName = GitHubService.GetName(remoteUrl) + ".txt";
            string customListFilePath = Path.Combine(Pathing.CustomAddOnsLists, customListName);

            if (!File.Exists(customListFilePath))
                return false;

            var localLines = File.ReadAllLines(customListFilePath);

            // Comparar línea por línea
            return remoteLines.SequenceEqual(localLines);
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

