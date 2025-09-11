using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.MVVM.View
{
    public partial class BackUpView : UserControl
    {
        public ICommand SetBackupDateToNowCommand { get; }

        private GridViewColumnHeader? _lastHeaderClicked;
        private ListSortDirection _lastDirection = ListSortDirection.Ascending;

        public BackUpView()
        {
            SetBackupDateToNowCommand = new RelayCommand(param =>
            {
                if (param is BackupInfo backup)
                {
                    backup.Date = DateTime.Now;
                }
            });

            InitializeComponent();
        }

        private void AdjustTitleColumnWidth()
        {
            if (BackupsListView.ActualWidth > 0)
            {
                double otherColumnsWidth = DateColumn.ActualWidth + 40 + 30 + 45; // Date + Fav + Open + Size
                double newWidth = BackupsListView.ActualWidth - otherColumnsWidth - 5; // Internal margin
                if (newWidth > 0)
                    TitleColumn.Width = newWidth;
            }
        }

        private void BackupsListView_Loaded(object sender, RoutedEventArgs e)
        {
            AdjustTitleColumnWidth();
        }

        private void BackupsListView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            AdjustTitleColumnWidth();
        }

        // --- Sorting logic ---
        private void GridViewColumnHeader_Click(object sender, RoutedEventArgs e)
        {
            if (e.OriginalSource is GridViewColumnHeader header
                && header.Column != null)
            {
                string? sortBy = null;

                if (header.Content is string headerText)
                {
                    switch (headerText)
                    {
                        case "Date":
                            sortBy = "Date";
                            break;
                        case "Title":
                            sortBy = "Title";
                            break;
                        case "MB":
                            sortBy = "SizeMB";
                            break;
                        case "Fav":
                            sortBy = "IsFavorite"; 
                            break;
                        case "Open":
                            // Nothing
                            return;
                    }
                }

                if (!string.IsNullOrEmpty(sortBy))
                {
                    var view = CollectionViewSource.GetDefaultView(BackupsListView.ItemsSource);
                    if (view != null)
                    {
                        // invertir sentido si ya estaba ordenado por la misma prop
                        var current = view.SortDescriptions.FirstOrDefault();
                        ListSortDirection newDir =
                            (current.PropertyName == sortBy && current.Direction == ListSortDirection.Ascending)
                                ? ListSortDirection.Descending
                                : ListSortDirection.Ascending;

                        view.SortDescriptions.Clear();
                        view.SortDescriptions.Add(new SortDescription(sortBy, newDir));
                    }
                }
            }
        }
    }
}
