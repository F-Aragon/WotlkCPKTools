using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;
using WotlkCPKTools.Services;

namespace WotlkCPKTools.MVVM.ViewModel
{
    public class AddCustomListViewModel : ObservableObject
    {
        private string _url;
        private string _selectedFilePath;

        private readonly AddonsViewModel _mainVm;
        private readonly Window? _window;
        public string Url
        {
            get => _url;
            set => SetProperty(ref _url, value);
        }

        public string SelectedFilePath
        {
            get => _selectedFilePath;
            set => SetProperty(ref _selectedFilePath, value);
        }


        private string _listName_pastedTab;
        public string ListName_pastedTab
        {
            get => _listName_pastedTab;
            set => SetProperty(ref _listName_pastedTab, value);
        }

        private string _url_pastedTab;
        public string Url_pastedTab
        {
            get => _url_pastedTab;
            set => SetProperty(ref _url_pastedTab, value);
        }

        private string _pastedContent_pastedTab;
        public string PastedContent_pastedTab
        {
            get => _pastedContent_pastedTab;
            set => SetProperty(ref _pastedContent_pastedTab, value);
        }

        public ICommand AddFromUrlCommand { get; }
        public ICommand AddFromPasteCommand { get; }
        public ICommand SelectFileCommand { get; }

        public AddCustomListViewModel(AddonsViewModel mainVm, Window? window = null)
        {
            _mainVm = mainVm;
            _window = window;

            AddFromUrlCommand = new RelayCommand(AddFromUrl);
            AddFromPasteCommand = new RelayCommand(AddFromPaste);
            SelectFileCommand = new RelayCommand(SelectFile);
        }

        private void AddFromUrl(object obj)
        {
            if (string.IsNullOrWhiteSpace(Url))
            {
                MessageBox.Show("Please enter a URL.");
                return;
            }

            if (!Uri.TryCreate(Url, UriKind.Absolute, out var uriResult))
            {
                MessageBox.Show("Invalid URL format.");
                return;
            }

            if (!Url.Contains("github.com") || !Url.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                MessageBox.Show("The URL must be a GitHub link to a .txt file.");
                return;
            }

            try
            {
                string fileName = GitHubService.GetName(Url);
                string filePath = Path.Combine(Pathing.CustomAddOnsLists, fileName);

                // Initial content with @URL
                string content = "@" + Url;
                File.WriteAllText(filePath, content);

                Debug.WriteLine($"Custom list created from URL: {filePath}");

                var customList = new CustomAddonList
                {
                    ListName = Path.GetFileNameWithoutExtension(fileName),
                    RepoFileUrl = Url
                };

                // Execute DownloadCustomList using the reference to AddonsViewModel
                _mainVm.DownloadCustomList.Execute(customList);
                _mainVm.CustomAddonLists.Add(customList);

                // Close window if reference is passed
                if (_window != null)
                {
                    _window.DialogResult = true;
                    _window.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating custom list: {ex.Message}");
            }
        }

        private void AddFromPaste(object obj)
        {
            if (string.IsNullOrWhiteSpace(PastedContent_pastedTab))
            {
                MessageBox.Show("Please paste the addons content.");
                return;
            }

            try
            {
                // Create folder if it doesn't exist
                if (!Directory.Exists(Pathing.CustomAddOnsLists))
                    Directory.CreateDirectory(Pathing.CustomAddOnsLists);

                // Determine the filename
                string finalName = string.IsNullOrWhiteSpace(ListName_pastedTab)
                    ? $"CustomList_{DateTime.Now:yyyyMMdd_HHmmss}.txt"
                    : ListName_pastedTab.Trim() + ".txt";

                string filePath = Path.Combine(Pathing.CustomAddOnsLists, finalName);

                // Prepare content: if valid URL exists, add it as the first line with @
                string contentToSave;
                string? repoUrl = null;
                if (!string.IsNullOrWhiteSpace(Url_pastedTab) && Uri.TryCreate(Url_pastedTab, UriKind.Absolute, out _))
                {
                    contentToSave = "@" + Url_pastedTab + Environment.NewLine + PastedContent_pastedTab;
                    repoUrl = Url_pastedTab;
                }
                else
                {
                    contentToSave = PastedContent_pastedTab;
                }

                // Save file
                File.WriteAllText(filePath, contentToSave);

                // Create CustomAddonList object
                var customList = new CustomAddonList
                {
                    ListName = Path.GetFileNameWithoutExtension(finalName),
                    RepoFileUrl = repoUrl
                };

                // Add to collection and execute download if URL exists
                _mainVm.CustomAddonLists.Add(customList);
                if (!string.IsNullOrEmpty(customList.RepoFileUrl))
                {
                    _mainVm.DownloadCustomList.Execute(customList);
                }

                // Close window if reference is passed
                if (_window != null)
                {
                    _window.DialogResult = true;
                    _window.Close();
                }

                Debug.WriteLine($"Custom list added from paste: {filePath}");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error adding custom list from paste: {ex.Message}");
            }
        }

        private void SelectFile(object obj)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Text Files (*.txt)|*.txt",
                Title = "Select a Custom Addon List"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    // Create folder if it doesn't exist
                    if (!Directory.Exists(Pathing.CustomAddOnsLists))
                        Directory.CreateDirectory(Pathing.CustomAddOnsLists);

                    // Destination filename
                    string fileName = Path.GetFileName(dialog.FileName);
                    string destinationPath = Path.Combine(Pathing.CustomAddOnsLists, fileName);

                    // Copy selected file
                    File.Copy(dialog.FileName, destinationPath, overwrite: true);

                    // Get the URL (if exists) using your method
                    string? repoUrl = CustomAddonList.GetRepoFileUrl(destinationPath);

                    // Create CustomAddonList object
                    var customList = new CustomAddonList
                    {
                        ListName = Path.GetFileNameWithoutExtension(fileName),
                        RepoFileUrl = repoUrl
                    };

                    // Execute DownloadCustomList using the reference to AddonsViewModel
                    _mainVm.CustomAddonLists.Add(customList);
                    if (!string.IsNullOrEmpty(customList.RepoFileUrl))
                    {
                        _mainVm.DownloadCustomList.Execute(customList);
                    }

                    // Close window if reference is passed
                    if (_window != null)
                    {
                        _window.DialogResult = true;
                        _window.Close();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error adding custom list: {ex.Message}");
                }
            }
        }

    }

}
