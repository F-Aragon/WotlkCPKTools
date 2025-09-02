using System.IO;
using System.Windows.Input;
using Microsoft.Win32;
using WotlkCPKTools.Core;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    /// <summary>
    /// ViewModel for the MoreView UserControl
    /// Handles browsing and updating Realmlist and Launcher paths
    /// </summary>
    public class MoreViewModel : ObservableObject
    {
        private readonly AppConfigService _appConfigService;

        /// <summary>
        /// Gets or sets the Realmlist folder path (read from AppConfig)
        /// </summary>
        public string RealmlistFolderPath
        {
            get => _appConfigService.RealmlistFolderPath;
            set
            {
                if (_appConfigService.RealmlistFolderPath != value)
                {
                    _appConfigService.UpdateConfig(realmlistFolder: value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the Launcher executable path (read from AppConfig)
        /// </summary>
        public string LauncherExePath
        {
            get => _appConfigService.LauncherExePath;
            set
            {
                if (_appConfigService.LauncherExePath != value)
                {
                    _appConfigService.UpdateConfig(launcherExe: value);
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Command to browse for a realmlist.wtf file
        /// </summary>
        public ICommand BrowseRealmlistFileCommand { get; }

        /// <summary>
        /// Command to browse for a launcher executable
        /// </summary>
        public ICommand BrowseLauncherExeCommand { get; }

        /// <summary>
        /// Constructor: initializes AppConfigService and commands
        /// </summary>
        public MoreViewModel()
        {
            _appConfigService = AppConfigService.Instance;

            BrowseRealmlistFileCommand = new RelayCommand(_ => BrowseRealmlistFile());
            BrowseLauncherExeCommand = new RelayCommand(_ => BrowseLauncherExe());
        }

        /// <summary>
        /// Opens a file dialog to select realmlist.wtf and updates the folder path
        /// </summary>
        private void BrowseRealmlistFile()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select realmlist.wtf",
                Filter = "WTF files (*.wtf)|*.wtf|All files (*.*)|*.*",
                InitialDirectory = Pathing.WoWFolder
            };

            if (dialog.ShowDialog() == true)
            {
                RealmlistFolderPath = Path.GetDirectoryName(dialog.FileName) ?? Pathing.RealmlistFolder;
            }
        }

        /// <summary>
        /// Opens a file dialog to select any launcher executable and updates the path
        /// </summary>
        private void BrowseLauncherExe()
        {
            var dialog = new OpenFileDialog
            {
                Title = "Select Launcher exe",
                Filter = "Executable files (*.exe)|*.exe|All files (*.*)|*.*",
                InitialDirectory = Pathing.WoWFolder
            };

            if (dialog.ShowDialog() == true)
            {
                LauncherExePath = dialog.FileName;
            }
        }
    }
}
