using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace WotlkCPKTools.Services
{
    public class FilesManagerService
    {
        /// <summary>
        /// Asynchronously backs up the given folder to a timestamped backup folder, reporting progress.
        /// </summary>
        public async Task<string> BackupFolderAsync(string backupTitle, string backupComment, IProgress<string>? progress = null)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(Pathing.WTF))
                    throw new DirectoryNotFoundException($"Source folder not found: {Pathing.WTF}");

                if (!Directory.Exists(Pathing.BackupsFolder))
                    Directory.CreateDirectory(Pathing.BackupsFolder);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFolderPath = Path.Combine(Pathing.BackupsFolder, $"{timestamp}");

                // Copia todos los archivos con progreso
                CopyDirectoryWithProgress(Pathing.WTF, backupFolderPath, progress);

                // Crea CPKToolsInfo.txt
                string infoFilePath = Path.Combine(backupFolderPath, "CPKToolsInfo.txt");
                bool isFavorite = false; // Default
                using (var writer = new StreamWriter(infoFilePath))
                {
                    writer.WriteLine($"@{DateTime.Now:yyyy-MM-dd HH:mm:ss}"); // Date
                    writer.WriteLine($"#{backupTitle}");                      // Title
                    writer.WriteLine($"!Favorite:{isFavorite}");
                    writer.WriteLine(backupComment);                          // Comment
                }

                return backupFolderPath;
            });
        }

        /// <summary>
        /// Recursively copies a directory y reporta progreso.
        /// </summary>
        private void CopyDirectoryWithProgress(string sourceDir, string destinationDir, IProgress<string>? progress)
        {
            Directory.CreateDirectory(destinationDir);
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            long totalBytes = 0;
            foreach (var file in files)
                totalBytes += new FileInfo(file).Length;

            long copiedBytes = 0;

            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);
                string destFile = Path.Combine(destinationDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);

                File.Copy(file, destFile, overwrite: true);

                copiedBytes += new FileInfo(file).Length;

                // Report progress
                double percent = (double)copiedBytes / totalBytes * 100;
                double copiedMb = copiedBytes / 1024.0 / 1024.0;
                double totalMb = totalBytes / 1024.0 / 1024.0;

                progress?.Report($"{copiedMb:F2}/{totalMb:F2} MB ({percent:F1}%)");
            }

            progress?.Report($"Done!: {totalBytes / 1024.0 / 1024.0:F2} MB");
        }

        /// <summary>
        /// Restaura un backup: siempre borra lo que hay en WTF y luego copia el backup, reportando progreso.
        /// </summary>
        public async Task RestoreBackupAsync(string backupFolder, IProgress<string>? progress = null)
        {
            if (!Directory.Exists(backupFolder))
                throw new DirectoryNotFoundException($"Backup folder not found: {backupFolder}");

            // Borra la carpeta WTF actual si existe
            if (Directory.Exists(Pathing.WTF))
                Directory.Delete(Pathing.WTF, recursive: true);

            // Crear WTF vacío
            Directory.CreateDirectory(Pathing.WTF);

            // Copiar backup con progreso
            await Task.Run(() => CopyDirectoryWithProgress(backupFolder, Pathing.WTF, progress));
        }

        /// <summary>
        /// Checks if all local folders of an addon exist in the AddOns directory.
        /// Receives the GitHub URL of the addon.
        /// </summary>
        public bool CheckIfAddonIsComplete(string githubUrl)
        {
            try
            {
                AddonService addonService = new AddonService();
                var addons = addonService.LoadAddonsFromLocal();

                // Find the addon by GitHub URL
                var match = addons.FirstOrDefault(a =>
                    a.GitHubUrl.Equals(githubUrl, StringComparison.OrdinalIgnoreCase));

                if (match == null || match.LocalFolders == null || match.LocalFolders.Count == 0)
                {
                    Debug.WriteLine($"Addon not found or has no LocalFolders for URL: {githubUrl}");
                    return false;
                }

                // Check that all local folders exist
                foreach (var folder in match.LocalFolders)
                {
                    string fullPath = Path.Combine(Pathing.AddOns, folder);
                    if (!Directory.Exists(fullPath))
                    {
                        Debug.WriteLine($"Missing folder for addon {match.Name}: {fullPath}");
                        return false;
                    }
                }

                return true; // all folders exist
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error checking completeness for addon {githubUrl}: {ex.Message}");
                return false;
            }
        }


    }
}
