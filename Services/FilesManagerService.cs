using System;
using System.IO;
using System.Threading.Tasks;

namespace WotlkCPKTools.Services
{
    public class FilesManagerService
    {
        private readonly string _backupBasePath;

        public FilesManagerService(string backupBasePath = @"C:\Users\f\Desktop\TestWOW\testData\BackUps\WTFBackUps")
        {
            _backupBasePath = backupBasePath;
        }

        /// <summary>
        /// Asynchronously backs up the given folder to a timestamped backup folder, reporting progress.
        /// </summary>
        public async Task<string> BackupFolderAsync(
            string sourceFolder = @"E:\Juegos\Warmane WoW\World of Warcraft 3.3.5a\WTF",
            string backupTypeSuffix = "-Full",
            IProgress<string>? progress = null)
        {
            return await Task.Run(() =>
            {
                if (!Directory.Exists(sourceFolder))
                    throw new DirectoryNotFoundException($"Source folder not found: {sourceFolder}");

                string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
                string backupFolderName = $"{timestamp}{backupTypeSuffix}";
                string backupFolderPath = Path.Combine(_backupBasePath, backupFolderName);

                if (!Directory.Exists(_backupBasePath))
                    Directory.CreateDirectory(_backupBasePath);

                CopyDirectoryWithProgress(sourceFolder, backupFolderPath, progress);

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
