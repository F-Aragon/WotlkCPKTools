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

        private BackupInfo _selectedBackup;
        public BackupInfo SelectedBackup
        {
            get => _selectedBackup;
            set
            {
                if (SetProperty(ref _selectedBackup, value))
                {
                    // Reset edit buffers
                    if (value != null)
                    {
                        EditTitle = value.Title;
                        EditComments = value.Comments;
                        EditDate = value.Date; 
                    }
                    else
                    {
                        EditTitle = string.Empty;
                        EditComments = string.Empty;
                        EditDate = default; 
                    }

                    // Force command re-evaluation
                    CommandManager.InvalidateRequerySuggested();
                }
            }
        }


        // Edit buffers
        private string _editTitle = string.Empty;
        public string EditTitle
        {
            get => _editTitle;
            set => SetProperty(ref _editTitle, value);
        }

        private string _editComments = string.Empty;
        public string EditComments
        {
            get => _editComments;
            set => SetProperty(ref _editComments, value);
        }

        private DateTime _editDate;
        public DateTime EditDate
        {
            get => _editDate;
            set => SetProperty(ref _editDate, value);
        }


        private string _backupTitle = string.Empty;
        public string BackupTitle
        {
            get => _backupTitle;
            set
            {
                if (SetProperty(ref _backupTitle, value))
                    CommandManager.InvalidateRequerySuggested();
            }
        }

        private string _backupComments = string.Empty;
        public string BackupComments
        {
            get => _backupComments;
            set => SetProperty(ref _backupComments, value);
        }

        private string _currentBackupPath = string.Empty;
        public string CurrentBackupPath
        {
            get => _currentBackupPath;
            set => SetProperty(ref _currentBackupPath, value);
        }

        private string _backupProgress = string.Empty;
        public string BackupProgress
        {
            get => _backupProgress;
            set => SetProperty(ref _backupProgress, value);
        }

        // Commands
        public ICommand BackupWtfCommand { get; }
        public ICommand OpenBackupsFolder { get; }
        public ICommand OpenBackupFolderCommand { get; }
        public ICommand UpdateBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand ActivateBackupCommand { get; }
        public ICommand SetBackupDateToNowCommand { get; }

        public BackUpViewModel()
        {
            _filesManagerService = new FilesManagerService();

            BackupWtfCommand = new RelayCommand(async _ => await BackupWtfAsync(),
                                                _ => !string.IsNullOrWhiteSpace(BackupTitle));

            OpenBackupsFolder = new RelayCommand(_ => OpenBackupsFolderExecute());

            var service = new BackupService();
            foreach (var backup in service.LoadBackups())
            {
                // Sub to IsFavorite changes to update the list
                backup.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(BackupInfo.IsFavorite))
                        UpdateFavorite(backup);
                };

                Backups.Add(backup);
            }

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

            UpdateBackupCommand = new RelayCommand(
                p => UpdateBackup(),
                p => SelectedBackup != null
            );

            DeleteBackupCommand = new RelayCommand(
                p => DeleteBackup(),
                p => SelectedBackup != null
            );

            ActivateBackupCommand = new RelayCommand(
                p => ActivateBackup(SelectedBackup),
                p => SelectedBackup != null
            );

            /*SetBackupDateToNowCommand = new RelayCommand(
                p => EditDate = DateTime.Now,
                p => SelectedBackup != null
            );*/
            // Button now
            SetBackupDateToNowCommand = new RelayCommand(
                _ => { if (SelectedBackup != null) EditDate = DateTime.Now; },
                _ => SelectedBackup != null
            );



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

                // Create a new BackupInfo for the freshly created backup
                var newBackup = new BackupInfo
                {
                    FolderPath = backupPath,
                    Title = BackupTitle,
                    Comments = BackupComments,
                    Date = DateTime.Now, // Or whatever the actual backup date is
                    IsFavorite = false,
                    SizeMB = GetFolderSizeInMB(backupPath)
                };

                // Add to the collection so UI updates
                Backups.Add(newBackup);

                // Reset inputs
                BackupTitle = string.Empty;
                BackupComments = string.Empty;

                CurrentBackupPath = backupPath;
            }
            catch (Exception ex)
            {
                BackupProgress = "Error";
                CurrentBackupPath = ex.Message;
            }
        }

        private double GetFolderSizeInMB(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long totalBytes = 0;
            var files = Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories);
            foreach (var file in files)
            {
                totalBytes += new FileInfo(file).Length;
            }

            return Math.Round(totalBytes / 1024.0 / 1024.0, 2);
        }

        private void UpdateFavorite(BackupInfo backup)
        {
            if (backup == null) return;

            string infoFile = Path.Combine(backup.FolderPath, "CPKToolsInfo.txt");
            try
            {
                var lines = File.ReadAllLines(infoFile);
                for (int i = 0; i < lines.Length; i++)
                {
                    if (lines[i].StartsWith("IsFavorite:"))
                    {
                        lines[i] = $"IsFavorite:{backup.IsFavorite}";
                        break;
                    }
                }
                File.WriteAllLines(infoFile, lines);
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update IsFavorite for '{backup.Title}': {ex.Message}");
            }
        }

        private void OpenBackupsFolderExecute()
        {
            if (!Directory.Exists(Pathing.BackupsFolder))
            {
                Directory.CreateDirectory(Pathing.BackupsFolder);
            }

            Process.Start(new ProcessStartInfo
            {
                FileName = Pathing.BackupsFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        /// <summary>
        /// Updates the selected backup with the values from edit buffers.
        /// </summary>
        private void UpdateBackup()
        {
            var backup = SelectedBackup;  // local copy

            if (backup == null) return;

            backup.Title = EditTitle;
            backup.Comments = EditComments;
            backup.Date = EditDate;

            string infoFile = Path.Combine(backup.FolderPath, "CPKToolsInfo.txt");
            try
            {
                using (var writer = new StreamWriter(infoFile, false))
                {
                    writer.WriteLine($"@{backup.Date:yyyy-MM-dd HH:mm}");
                    writer.WriteLine($"#{backup.Title}");
                    writer.WriteLine($"IsFavorite:{backup.IsFavorite}");
                    writer.WriteLine(backup.Comments);
                }

                // Refresh UI
                var index = Backups.IndexOf(backup);
                if (index >= 0) Backups[index] = backup;

                Debug.WriteLine($"Backup '{backup.Title}' updated successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update backup '{backup?.Title}': {ex.Message}");
            }
        }



        /// <summary>
        /// Deletes the selected backup folder from disk and removes it from the collection.
        /// </summary>
        private void DeleteBackup()
        {
            if (SelectedBackup == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the backup '{SelectedBackup.Title}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning
            );

            if (result != System.Windows.MessageBoxResult.Yes)
                return; // Cancelled by user

            try
            {
                if (Directory.Exists(SelectedBackup.FolderPath))
                    Directory.Delete(SelectedBackup.FolderPath, recursive: true);

                Backups.Remove(SelectedBackup);
                SelectedBackup = null;

                Debug.WriteLine($"Backup deleted successfully.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete backup: {ex.Message}");
            }
        }


        private void ActivateBackup(BackupInfo backup)
        {
            if (backup == null) return;
            Debug.WriteLine($"Backup '{backup.Title}' activated.");
        }
    }
}
