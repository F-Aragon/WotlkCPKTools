using System;
using System.Collections.Generic;
using System.IO;

namespace WotlkCPKTools.Services
{
    /// <summary>
    /// Service to manage AppConfig.txt, storing paths for launcher and realmlist.
    /// Implements singleton so all ViewModels share the same instance.
    /// </summary>
    public class AppConfigService
    {
        private static AppConfigService? _instance;

        /// <summary>
        /// Singleton instance of AppConfigService
        /// </summary>
        public static AppConfigService Instance => _instance ??= new AppConfigService();

        private readonly string _configFilePath;

        public string LauncherExePath { get; private set; }
        public string RealmlistFolderPath { get; private set; }

        /// <summary>
        /// Private constructor to enforce singleton pattern
        /// </summary>
        private AppConfigService()
        {
            _configFilePath = Pathing.AppConfigFile;
            EnsureAppConfigExists();
            LoadConfig();
        }

        /// <summary>
        /// Ensures that AppConfig.txt exists, creating it with default paths if missing
        /// </summary>
        private void EnsureAppConfigExists()
        {
            if (!File.Exists(_configFilePath))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_configFilePath)!);

                // Default paths
                LauncherExePath = Path.Combine(Pathing.WoWFolder, "wow.exe");
                RealmlistFolderPath = Pathing.RealmlistFolder;

                File.WriteAllLines(_configFilePath, new[]
                {
                    $"launcherExe: {LauncherExePath}",
                    $"realmlistFolder: {RealmlistFolderPath}"
                });
            }
        }

        /// <summary>
        /// Loads AppConfig.txt and stores values into properties
        /// </summary>
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

        /// <summary>
        /// Saves the given launcher exe and realmlist folder paths to AppConfig.txt
        /// </summary>
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

        /// <summary>
        /// Updates one or both paths and saves immediately
        /// </summary>
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
