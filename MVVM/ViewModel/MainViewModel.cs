using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    /// <summary>
    /// Main application ViewModel
    /// Handles footer commands and switching views
    /// </summary>
    class MainViewModel : ObservableObject
    {
        private readonly AppConfigService _appConfigService;

        private string _realmlistContent;
        public string RealmlistContent
        {
            get => _realmlistContent;
            set { _realmlistContent = value; OnPropertyChanged(); }
        }

        private string _statusMessage;
        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public ICommand LaunchWoWCommand { get; }
        public ICommand OpenRealmlistFolderCommand { get; }

        public RelayCommand AddonsViewCommand { get; set; }
        public RelayCommand BackUpViewCommand { get; set; }
        public RelayCommand MoreViewCommand { get; set; }

        public AddonsViewModel AddonsVM { get; set; }
        public BackUpViewModel BackUpVM { get; set; }
        public MoreViewModel MoreVM { get; set; }

        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set { _currentView = value; OnPropertyChanged(); }
        }

        public MainViewModel()
        {
            AddonsVM = new AddonsViewModel();
            BackUpVM = new BackUpViewModel();
            MoreVM = new MoreViewModel();

            CurrentView = AddonsVM;

            AddonsViewCommand = new RelayCommand(o => CurrentView = AddonsVM);
            BackUpViewCommand = new RelayCommand(o => CurrentView = BackUpVM);
            MoreViewCommand = new RelayCommand(o => CurrentView = MoreVM);

            _appConfigService = AppConfigService.Instance;

            // Listen to changes in AppConfigService (paths updates)
            _appConfigService.PropertyChanged += OnAppConfigChanged;

            // Footer commands
            LaunchWoWCommand = new RelayCommand(_ => LaunchWoW());
            OpenRealmlistFolderCommand = new RelayCommand(_ => OpenRealmlistFolder());

            // Load realmlist content at startup
            LoadRealmlistContent();
        }

        private void OnAppConfigChanged(object? sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(AppConfigService.RealmlistFolderPath))
            {
                // Reload file when folder changes
                LoadRealmlistContent();
            }
        }

        private void LoadRealmlistContent()
        {
            try
            {
                var folder = _appConfigService.RealmlistFolderPath;
                var filePath = Path.Combine(folder, "realmlist.wtf");

                RealmlistContent = File.Exists(filePath)
                    ? File.ReadAllText(filePath)
                    : "Archivo realmlist.wtf no encontrado.";
            }
            catch (Exception ex)
            {
                RealmlistContent = $"Error leyendo realmlist: {ex.Message}";
            }
        }

        private void LaunchWoW()
        {
            try
            {
                var exePath = _appConfigService.LauncherExePath;
                if (File.Exists(exePath))
                    Process.Start(exePath);
                else
                    StatusMessage = "Launcher not found.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }

        private void OpenRealmlistFolder()
        {
            try
            {
                var folder = _appConfigService.RealmlistFolderPath;
                if (Directory.Exists(folder))
                    Process.Start("explorer.exe", folder);
                else
                    StatusMessage = "Realmlist folder not found.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
            }
        }
    }
}
