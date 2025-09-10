using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows;
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

        // INotifyPropertyChanged implementation
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));


        


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


        public static bool DeleteCustomList(CustomAddonList aList)
        {

            if (string.IsNullOrWhiteSpace(aList.ListName))
                return false;

            var result = MessageBox.Show(
                         $"Are you sure you want to delete the list \"{aList.ListName}\"?",
                         "Confirm Deletion",
                         MessageBoxButton.YesNo,MessageBoxImage.Warning);

            if (result != MessageBoxResult.Yes)
                return false;

            string filePath = Path.Combine(Pathing.CustomAddOnsLists, (aList.ListName+".txt"));

            if (File.Exists(filePath)) 
            {
                File.Delete(filePath);
                Debug.WriteLine($"List: {aList.ListName}.txt file deleted");
                return true;
            }
            else
            {
                Debug.WriteLine($"ERROR: List: {aList.ListName}.txt file not found");
                return false;
            }
                
        }
    }
}

