using System.Windows.Input;
using System.Windows.Media;
using WotlkCPKTools.Core;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    internal class AddAddonWindowModel : ObservableObject
    {
        private string _githubUrl;
        private string _statusMessage;
        private SolidColorBrush _statusColor;
        private readonly AddonService _addonService = new AddonService();
        private readonly GitHubService _gitHubService = new GitHubService();

        public string GitHubUrl
        {
            get => _githubUrl;
            set => SetProperty(ref _githubUrl, value);
        }
        public string StatusMessage
        {
            get => _statusMessage;
            set => SetProperty(ref _statusMessage, value);
        }

        public SolidColorBrush StatusColor
        {
            get => _statusColor;
            set => SetProperty(ref _statusColor, value);
        }

        public ICommand AddAddonCommand { get; }

        public AddAddonWindowModel()
        {
            AddAddonCommand = new RelayCommand(async (obj) => await ExecuteAddCommand(obj));
        }

        private async Task ExecuteAddCommand(object parameter)
        {
            if (string.IsNullOrWhiteSpace(GitHubUrl))
            {
                StatusMessage = "URL cannot be empty";
                StatusColor = Brushes.Red;
                return;
            }

            if (!_gitHubService.IsValidGitHubRepoUrl(GitHubUrl))
            {
                StatusMessage = "Invalid URL format";
                StatusColor = Brushes.Red;
                return;
            }

            if (await _addonService.AddonExistsAsync(GitHubUrl))
            {
                StatusMessage = "This addon is already added";
                StatusColor = Brushes.Orange;
                return;
            }

            var addon = await _addonService.CreateAddonInfoFromGitHubUrl(GitHubUrl);

            if (addon == null)
            {
                StatusMessage = "Could not find repo information";
                StatusColor = Brushes.Red;
                return;
            }

            bool added = await _addonService.AddAddonToLocalJsonAsync(addon);
            if (added)
            {
                StatusMessage = "Addon successfully added";
                StatusColor = Brushes.Green;
            }
            else
            {
                StatusMessage = "Failed to add addon";
                StatusColor = Brushes.Orange;
            }

        }
    }
}
