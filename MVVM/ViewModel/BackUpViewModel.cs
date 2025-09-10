using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
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

        private string _restoreProgress = string.Empty;
        public string RestoreProgress
        {
            get => _restoreProgress;
            set => SetProperty(ref _restoreProgress, value);
        }

        // Progres bar
        private double _backupProgressPercentage;
        public double BackupProgressPercentage
        {
            get => _backupProgressPercentage;
            set => SetProperty(ref _backupProgressPercentage, value);
        }
        // Progres bar
        private double _restoreProgressPercentage;
        public double RestoreProgressPercentage
        {
            get => _restoreProgressPercentage;
            set => SetProperty(ref _restoreProgressPercentage, value);
        }

        private bool _isRestoring;
        public bool IsRestoring
        {
            get => _isRestoring;
            set => SetProperty(ref _isRestoring, value);
        }

        // Commands
        public ICommand BackupWtfCommand { get; }
        public ICommand OpenBackupsFolder { get; }
        public ICommand OpenBackupFolderCommand { get; }
        public ICommand UpdateBackupCommand { get; }
        public ICommand DeleteBackupCommand { get; }
        public ICommand RestoreBackupCommand { get; }
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

            RestoreBackupCommand = new RelayCommand(
                async _ => await RestoreBackupAsync(SelectedBackup),
                _ => SelectedBackup != null
            );

            SetBackupDateToNowCommand = new RelayCommand(
                _ => { if (SelectedBackup != null) EditDate = DateTime.Now; },
                _ => SelectedBackup != null
            );
        }

        private async Task BackupWtfAsync()
        {
            try
            {
                CurrentBackupPath = string.Empty;

                // Progress %
                var progress = new Progress<string>(s =>
                {
                    BackupProgress = s;

                    var percentIndex = s.IndexOf('%');
                    if (percentIndex > 0)
                    {
                        var parts = s.Substring(0, percentIndex).Split('(');
                        if (parts.Length > 1 && double.TryParse(parts[1], out double pct))
                            BackupProgressPercentage = pct;
                    }
                });

                
                string backupPath = await _filesManagerService.BackupFolderAsync(
                    backupTitle: BackupTitle,
                    backupComment: BackupComments,
                    progress: progress
                );

                
                var newBackup = new BackupInfo
                {
                    FolderPath = backupPath,
                    Title = BackupTitle,
                    Comments = BackupComments,
                    Date = DateTime.Now,
                    IsFavorite = false,
                    SizeMB = GetFolderSizeInMB(backupPath)
                };

                Backups.Add(newBackup);

                // Inputs Reset
                BackupTitle = string.Empty;
                BackupComments = string.Empty;

                CurrentBackupPath = backupPath;
            }
            catch (Exception ex)
            {
                BackupProgress = "Error";
                CurrentBackupPath = ex.Message;
            }
            finally
            {
                BackupProgress = string.Empty;
            }
        }


        private double GetFolderSizeInMB(string folderPath)
        {
            if (!Directory.Exists(folderPath)) return 0;

            long totalBytes = 0;
            foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
                totalBytes += new FileInfo(file).Length;

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
                    if (lines[i].StartsWith("!Favorite:"))
                    {
                        lines[i] = $"!Favorite:{backup.IsFavorite}";
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
                Directory.CreateDirectory(Pathing.BackupsFolder);

            Process.Start(new ProcessStartInfo
            {
                FileName = Pathing.BackupsFolder,
                UseShellExecute = true,
                Verb = "open"
            });
        }

        private void UpdateBackup()
        {
            var backup = SelectedBackup;
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
                    writer.WriteLine($"!Favorite:{backup.IsFavorite}");
                    writer.WriteLine(backup.Comments);
                }

                var index = Backups.IndexOf(backup);
                if (index >= 0) Backups[index] = backup;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to update backup '{backup?.Title}': {ex.Message}");
            }
        }

        private void DeleteBackup()
        {
            if (SelectedBackup == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to delete the backup '{SelectedBackup.Title}'?",
                "Confirm Delete",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning
            );

            if (result != System.Windows.MessageBoxResult.Yes) return;

            try
            {
                if (Directory.Exists(SelectedBackup.FolderPath))
                    Directory.Delete(SelectedBackup.FolderPath, recursive: true);

                Backups.Remove(SelectedBackup);
                SelectedBackup = null;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Failed to delete backup: {ex.Message}");
            }
        }

        private async Task RestoreBackupAsync(BackupInfo backup)
        {
            if (backup == null) return;

            var result = System.Windows.MessageBox.Show(
                $"Are you sure you want to restore backup '{backup.Title}'?\nThis will overwrite your current WTF folder.",
                "Restore Backup",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Warning
            );

            if (result != System.Windows.MessageBoxResult.Yes)
                return;

            try
            {
                IsRestoring = true; // <-- Show ProgressBar

                var progress = new Progress<string>(s =>
                {
                    RestoreProgress = s;

                    // Extract percentage
                    var percentIndex = s.IndexOf('%');
                    if (percentIndex > 0)
                    {
                        var parts = s.Substring(0, percentIndex).Split('(');
                        if (parts.Length > 1 && double.TryParse(parts[1], out double pct))
                            RestoreProgressPercentage = pct;
                    }
                });

                // Delete current WTF
                if (Directory.Exists(Pathing.WTF))
                    Directory.Delete(Pathing.WTF, recursive: true);

                // Restore backup with progress bar
                await _filesManagerService.RestoreBackupAsync(backup.FolderPath, progress);

                System.Windows.MessageBox.Show($"Backup '{backup.Title}' restored successfully.", "Success", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Failed to restore backup: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
            finally
            {
                IsRestoring = false; // <-- Hide progress bar
                RestoreProgress = string.Empty;
                RestoreProgressPercentage = 0;
            }
        }



    }
}
