using System.ComponentModel;
using System.IO;

namespace WotlkCPKTools.Services
{
    /// <summary>
    /// Service to manage AppConfig.txt, storing paths for launcher and realmlist.
    /// Implements singleton so all ViewModels share the same instance.
    /// Now implements INotifyPropertyChanged to notify UI when paths change.
    /// </summary>
    public class AppConfigService : INotifyPropertyChanged
    {
        private static AppConfigService? _instance;
        public static AppConfigService Instance => _instance ??= new AppConfigService();

        private readonly string _configFilePath;

        private string _launcherExePath;
        public string LauncherExePath
        {
            get => _launcherExePath;
            private set
            {
                if (_launcherExePath != value)
                {
                    _launcherExePath = value;
                    OnPropertyChanged(nameof(LauncherExePath));
                }
            }
        }

        private string _realmlistFolderPath;
        public string RealmlistFolderPath
        {
            get => _realmlistFolderPath;
            private set
            {
                if (_realmlistFolderPath != value)
                {
                    _realmlistFolderPath = value;
                    OnPropertyChanged(nameof(RealmlistFolderPath));
                }
            }
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        private void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private AppConfigService()
        {
            _configFilePath = Pathing.AppConfigFile;
            EnsureAppConfigExists();
            LoadConfig();
        }

        private void EnsureAppConfigExists()
        {
            if (!File.Exists(_configFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);

                LauncherExePath = Path.Combine(Pathing.WoWFolder, "wow.exe");
                RealmlistFolderPath = Pathing.RealmlistFolder;

                File.WriteAllLines(_configFilePath, new[]
                {
                    $"launcherExe: {LauncherExePath}",
                    $"realmlistFolder: {RealmlistFolderPath}"
                });
            }
        }

        private void LoadConfig()
        {
            try
            {
                var lines = File.ReadAllLines(_configFilePath);
                var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

                foreach (var line in lines)
                {
                    if (string.IsNullOrWhiteSpace(line) || !line.Contains(":"))
                        continue;

                    var parts = line.Split(new[] { ':' }, 2);
                    if (parts.Length == 2)
                    {
                        var key = parts[0].Trim();
                        var value = parts[1].Trim();
                        dict[key] = value;
                    }
                }

                LauncherExePath = dict.ContainsKey("launcherExe") ? dict["launcherExe"] : string.Empty;
                RealmlistFolderPath = dict.ContainsKey("realmlistFolder") ? dict["realmlistFolder"] : string.Empty;
            }
            catch (Exception ex)
            {
                LauncherExePath = string.Empty;
                RealmlistFolderPath = string.Empty;
                Console.WriteLine($"Error reading AppConfig.txt: {ex.Message}");
            }
        }

        public void SaveConfig(string launcherExePath, string realmlistFolderPath)
        {
            LauncherExePath = launcherExePath ?? string.Empty;
            RealmlistFolderPath = realmlistFolderPath ?? string.Empty;

            Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);

            File.WriteAllLines(_configFilePath, new[]
            {
                $"launcherExe: {LauncherExePath}",
                $"realmlistFolder: {RealmlistFolderPath}"
            });
        }

        public void UpdateConfig(string? launcherExe = null, string? realmlistFolder = null)
        {
            if (!string.IsNullOrWhiteSpace(launcherExe))
                LauncherExePath = launcherExe;

            if (!string.IsNullOrWhiteSpace(realmlistFolder))
                RealmlistFolderPath = realmlistFolder;

            SaveConfig(LauncherExePath, RealmlistFolderPath);
        }
    }
}
