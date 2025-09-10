using WotlkCPKTools.Core;

namespace WotlkCPKTools.MVVM.Model
{
    public class BackupInfo : ObservableObject
    {
        private string _title;
        public string Title { get => _title; set => SetProperty(ref _title, value); }

        private string _comments;
        public string Comments { get => _comments; set => SetProperty(ref _comments, value); }

        private DateTime _date;
        public DateTime Date { get => _date; set => SetProperty(ref _date, value); }

        private bool _isFavorite;
        public bool IsFavorite { get => _isFavorite; set => SetProperty(ref _isFavorite, value); }

        public string FolderPath { get; set; }

        private double _sizeMB;
        public double SizeMB { get => _sizeMB; set => SetProperty(ref _sizeMB, value); }
    }
}
