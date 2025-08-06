using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using WotlkCPKTools.Core;

namespace WotlkCPKTools.MVVM.ViewModel
{
    internal class AddAddonWindowModel : ObservableObject
    {
        private string _gitHubUrl;

        public string GitHubUrl
        {
            get => _gitHubUrl;
            set
            {
                if (SetProperty(ref _gitHubUrl, value))
                {
                    OnPropertyChanged(nameof(CanAddAddon));
                    // Force re-evaluation of the button
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        public bool CanAddAddon => IsValidGitHubRepoUrl(GitHubUrl);

        public RelayCommand AddAddonCommand { get; }


        /*
        public AddAddonWindowModel()
        {
            AddAddonCommand = new RelayCommand(
                //test msgbox--- execute: o => MessageBox.Show($"URL: {GitHubUrl}", "GitHub URL"),
                canExecute: o => CanAddAddon
            );
        }
        */


        private bool IsValidGitHubRepoUrl(string url)
        {
            if (string.IsNullOrWhiteSpace(url)) return false;
            return Regex.IsMatch(url, @"^https:\/\/github\.com\/[\w\-]+\/[\w\-]+$");
        }
    }
}
