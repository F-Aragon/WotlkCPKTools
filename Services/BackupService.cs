using System.IO;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.Services
{
    public class BackupService : ObservableObject
    {
        /// <summary>
        /// Load all backups from the backups folder.
        /// </summary>
        public List<BackupInfo> LoadBackups()
        {
            var backups = new List<BackupInfo>();

            if (!Directory.Exists(Pathing.BackupsFolder))
                return backups;

            foreach (var folder in Directory.GetDirectories(Pathing.BackupsFolder))
            {
                var info = LoadBackupInfo(folder);
                if (info != null)
                    backups.Add(info);
            }

            return backups;
        }

        /// <summary>
        /// Load information about a single backup folder.
        /// </summary>
        private BackupInfo LoadBackupInfo(string folderPath)
        {
            string infoFile = Path.Combine(folderPath, "CPKToolsInfo.txt");

            string title;
            string comments;
            DateTime date;
            bool isFavorite = false;

            if (File.Exists(infoFile))
            {
                var lines = File.ReadAllLines(infoFile);

                // Get all comment lines (ignore @, #, or IsFavorite lines)
                var commentsLines = lines.Where(l => !l.StartsWith("@") && !l.StartsWith("#") && !l.StartsWith("!Favorite:", StringComparison.OrdinalIgnoreCase))
                    .Select(l => l.Trim());

                // Get date and title
                var dateLine = lines.FirstOrDefault(l => l.StartsWith("@"));
                var titleLine = lines.FirstOrDefault(l => l.StartsWith("#"));

                title = titleLine != null ? titleLine.TrimStart('#').Trim() : "Title missing";
                comments = string.Join(Environment.NewLine, commentsLines);
                date = dateLine != null ? ParseDate(dateLine) : Directory.GetCreationTime(folderPath);

                // Read favorite flag
                var favoriteLine = lines.FirstOrDefault(l => l.StartsWith("!Favorite:", StringComparison.OrdinalIgnoreCase));
                if (favoriteLine != null && bool.TryParse(favoriteLine.Substring("!Favorite:".Length).Trim(), out var fav))
                    isFavorite = fav;
            }
            else
            {
                title = ".txt missing";
                comments = "";
                date = Directory.GetCreationTime(folderPath);
                isFavorite = false;
            }

            var backup = new BackupInfo
            {
                FolderPath = folderPath,
                Title = title,
                Comments = comments,
                Date = date,
                SizeMB = CalculateFolderSize(folderPath) / 1024.0 / 1024.0,
                IsFavorite = isFavorite
            };

            return backup;
        }

        /// <summary>
        /// Parse a line with @date and return DateTime.
        /// </summary>
        private DateTime ParseDate(string line)
        {
            if (line.StartsWith("@"))
            {
                if (DateTime.TryParse(line.Substring(1), out var date))
                    return date;
            }
            return DateTime.MinValue;
        }

        /// <summary>
        /// Calculate the total folder size in bytes.
        /// </summary>
        private long CalculateFolderSize(string folderPath)
        {
            long size = 0;
            foreach (var file in Directory.GetFiles(folderPath, "*", SearchOption.AllDirectories))
            {
                size += new FileInfo(file).Length;
            }
            return size;
        }
    }
}
