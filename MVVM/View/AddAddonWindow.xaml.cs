using System.Windows;
using System.Windows.Input;

namespace WotlkCPKTools.MVVM.View
{
    public partial class AddAddonWindow : Window
    {
        public AddAddonWindow()
        {
            InitializeComponent();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                this.DragMove();
        }
    }
}
