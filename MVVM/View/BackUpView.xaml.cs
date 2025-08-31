using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WotlkCPKTools.MVVM.View
{
    public partial class BackUpView : UserControl
    {
        public BackUpView()
        {
            InitializeComponent();

        }

        private void AdjustTitleColumnWidth()
        {
            if (BackupsListView.ActualWidth > 0)
            {
                double otherColumnsWidth = DateColumn.ActualWidth + 50 + 40; // Date + Fav + Open
                double newWidth = BackupsListView.ActualWidth - otherColumnsWidth - 5; // margen interno
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
