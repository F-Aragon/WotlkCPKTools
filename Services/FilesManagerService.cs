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
        public async Task<string> BackupFolderAsync(string backupTypeSuffix = "-Full", IProgress<string>? progress = null)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(Pathing.WTF))
                    throw new DirectoryNotFoundException($"Source folder not found: {Pathing.WTF}");

                if (!Directory.Exists(Pathing.BackupsFolder))
                    Directory.CreateDirectory(Pathing.BackupsFolder);

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFolderPath = Path.Combine(Pathing.BackupsFolder, $"{timestamp}{backupTypeSuffix}");

                CopyDirectoryWithProgress(Pathing.WTF, backupFolderPath, progress);

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
            int total = files.Length;
            int count = 0;

            foreach (var file in files)
            {
                string relativePath = Path.GetRelativePath(sourceDir, file);
                string destFile = Path.Combine(destinationDir, relativePath);
                Directory.CreateDirectory(Path.GetDirectoryName(destFile)!);
                File.Copy(file, destFile, overwrite: true);

                count++;
                progress?.Report($"Copying {count}/{total} files...");
            }
        }
    }
}
