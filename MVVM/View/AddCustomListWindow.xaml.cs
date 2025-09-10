using System.Windows;
using System.Windows.Input;

namespace WotlkCPKTools.MVVM.View
{
    public partial class AddCustomListWindow : Window
    {
        public AddCustomListWindow()
        {
            InitializeComponent();
        }

        // Cerrar la ventana
        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            this.Close();
        }

        // Permitir arrastrar la ventana haciendo click en cualquier lugar
        private void Window_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
            {
                this.DragMove();
            }
        }
    }
}
