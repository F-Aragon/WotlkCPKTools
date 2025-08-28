using System;
using System.ComponentModel;

namespace WotlkCPKTools.MVVM.Model
{
    public class AddonItem : INotifyPropertyChanged
    {
        public string Name { get; set; }
        public string IconPath { get; set; }
        public string GitHubUrl { get; set; }
        public bool IsUpdated { get; set; }
        public DateTime? LastUpdate { get; set; }

        private string _downloadStatus = "";
        public string DownloadStatus
        {
            get => _downloadStatus;
            set
            {
                if (_downloadStatus != value)
                {
                    _downloadStatus = value;
                    OnPropertyChanged(nameof(DownloadStatus));
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
