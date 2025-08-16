using System;
using System.Threading.Tasks;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    public class BackUpViewModel : ObservableObject
    {
        private readonly FilesManagerService _filesManagerService;

        private string _currentBackupPath = string.Empty;
        public string CurrentBackupPath
        {
            get => _currentBackupPath;
            set => SetProperty(ref _currentBackupPath, value);
        }

        private string _backupStatus = "Idle";
        public string BackupStatus
        {
            get => _backupStatus;
            set => SetProperty(ref _backupStatus, value);
        }

        public ICommand BackupWtfCommand { get; }

        public BackUpViewModel()
        {
            _filesManagerService = new FilesManagerService();
            BackupWtfCommand = new RelayCommand(async _ => await BackupWtfAsync());
        }

        /// <summary>
        /// Performs a backup of the WTF folder, updating status and path properties.
        /// </summary>
        private async Task BackupWtfAsync()
        {
            try
            {
                BackupStatus = "Starting...";
                CurrentBackupPath = string.Empty;

                var progress = new Progress<string>(s => BackupStatus = s);
                string backupPath = await _filesManagerService.BackupFolderAsync(progress: progress);

                CurrentBackupPath = backupPath;
                BackupStatus = "Done";
            }
            catch (Exception ex)
            {
                BackupStatus = "Error";
                CurrentBackupPath = ex.Message;
            }
        }
    }
}
