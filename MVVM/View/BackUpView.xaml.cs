using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using WotlkCPKTools.Core;
using WotlkCPKTools.MVVM.Model;

namespace WotlkCPKTools.MVVM.View
{
    public partial class BackUpView : UserControl
    {
        public ICommand SetBackupDateToNowCommand { get; }


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
                double otherColumnsWidth = DateColumn.ActualWidth + 40 + 30 + 45; // Date + Fav + Open + size
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
        

        
    }
}
