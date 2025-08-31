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

                // Copy files
                CopyDirectoryWithProgress(Pathing.WTF, backupFolderPath, progress);

                // Create CPKToolsInfo.txt
                string infoFilePath = Path.Combine(backupFolderPath, "CPKToolsInfo.txt");
                bool isFavorite = false; // Default
                using (var writer = new StreamWriter(infoFilePath))
                {
                    writer.WriteLine($"@{DateTime.Now:yyyy-MM-dd HH:mm:ss}"); // Date
                    writer.WriteLine($"#{backupTitle}");                      // Title
                    writer.WriteLine($"!Favorite:{isFavorite}");
                    writer.WriteLine(backupComment);                          // Comment (multiline supported)
                                        ;
                }

                return backupFolderPath;
            });
        }


        /// <summary>
        /// Recursively copies a directory and reports progress.
        /// </summary>
        private void CopyDirectoryWithProgress(string sourceDir, string destinationDir, IProgress<string>? progress)
        {
            Directory.CreateDirectory(destinationDir);
            var files = Directory.GetFiles(sourceDir, "*", SearchOption.AllDirectories);

            // Calculate total size
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

                double percent = (double)copiedBytes / totalBytes * 100;
                double copiedMb = copiedBytes / 1024.0 / 1024.0;
                double totalMb = totalBytes / 1024.0 / 1024.0;

                progress?.Report($"{copiedMb:F2}/{totalMb:F2} MB ({percent:F1}%)");
            }

            progress?.Report($"Done! - Size: {totalBytes / 1024.0 / 1024.0:F2} MB");
        }



    }
}
