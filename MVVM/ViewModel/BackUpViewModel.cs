using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    public class BackUpViewModel : ObservableObject
    {
        private readonly FilesManagerService _filesManagerService;
        public ObservableCollection<BackupInfo> Backups { get; } = new();

        private string _backupTitle = string.Empty;
        public string BackupTitle
        {
            get => _backupTitle;
            set
            {
                if (SetProperty(ref _backupTitle, value))
                {
                    // Ask the command system to re-evaluate CanExecute
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }

        private string _backupComments = string.Empty;
        public string BackupComments
        {
            get => _backupComments;
            set => SetProperty(ref _backupComments, value);
        }
        //-- 
        private string _currentBackupPath = string.Empty;
        public string CurrentBackupPath
        {
            get => _currentBackupPath;
            set => SetProperty(ref _currentBackupPath, value);
        }
        //-- 

        private string _backupProgress = string.Empty;
        public string BackupProgress
        {
            get => _backupProgress;
            set => SetProperty(ref _backupProgress, value);
        }

        public ICommand BackupWtfCommand { get; }
        public ICommand OpenBackupsFolder { get; }
        public ICommand OpenBackupFolderCommand { get; }
        public BackUpViewModel()
        {
            _filesManagerService = new FilesManagerService();

            BackupWtfCommand = new RelayCommand(async _ => await BackupWtfAsync(),
                                                _ => !string.IsNullOrWhiteSpace(BackupTitle));


            OpenBackupsFolder = new RelayCommand(_ => OpenBackupsFolderExecute());


            // VIEW
            var service = new BackupService();
            foreach (var backup in service.LoadBackups())
                Backups.Add(backup);

            OpenBackupFolderCommand = new RelayCommand(p =>
            {
                if (p is string folder && Directory.Exists(folder))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = folder,
                        UseShellExecute = true
                    });
                }
            });



        }

        /// <summary>
        /// Performs a backup of the WTF folder, updating status and path properties.
        /// </summary>
        private async Task BackupWtfAsync()
        {
            try
            {
                CurrentBackupPath = string.Empty;

                var progress = new Progress<string>(s => BackupProgress = s);

                // Pass title and comment from bindings
                string backupPath = await _filesManagerService.BackupFolderAsync(
                    backupTitle: BackupTitle,
                    backupComment: BackupComments,
                    progress: progress
                );

                BackupTitle = string.Empty;
                BackupComments = string.Empty;

                CurrentBackupPath = backupPath; // not using it for now
            }
            catch (Exception ex)
            {
                BackupProgress = "Error";
                CurrentBackupPath = ex.Message;
            }
        }

        private void OpenBackupsFolderExecute()
        {
            if (!Directory.Exists(Pathing.BackupsFolder))
            {
                Directory.CreateDirectory(Pathing.BackupsFolder);
            }

            // Open folder in explorer
            Process.Start(new ProcessStartInfo
            {
                FileName = Pathing.BackupsFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }
    }
}
