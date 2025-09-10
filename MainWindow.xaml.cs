using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Threading;

namespace WotlkCPKTools
{
    /// <summary>
    /// Main application window
    /// Handles window controls, drag, and animated random background
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly List<string> _backgroundImages = new()
        {
            "back1.jpeg",
            "back2.png",
            "back3.jpg",
            "back4.jpeg"
        };

        private readonly Random _random = new();
        private DispatcherTimer _backgroundTimer;
        private bool _usingFirstBrush = true;

        public MainWindow()
        {
            InitializeComponent();

            // Set first random background
            CrossfadeBackground(_backgroundImages[_random.Next(_backgroundImages.Count)]);

            // Start timer to change background every 10 seconds
            StartBackgroundTimer();
        }

        /// <summary>
        /// Initialize and start the background timer
        /// </summary>
        private void StartBackgroundTimer()
        {
            _backgroundTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromSeconds(45)
            };
            _backgroundTimer.Tick += (s, e) =>
            {
                string nextImage = _backgroundImages[_random.Next(_backgroundImages.Count)];
                CrossfadeBackground(nextImage);
            };
            _backgroundTimer.Start();
        }

        /// <summary>
        /// Crossfade between two ImageBrushes to smoothly change background
        /// </summary>
        private void CrossfadeBackground(string fileName)
        {
            try
            {
                var uri = new Uri($"pack://application:,,,/Resources/Images/Backgrounds/{fileName}", UriKind.Absolute);
                var newImage = new BitmapImage(uri);

                var brushIn = _usingFirstBrush ? BackgroundBrush2 : BackgroundBrush1;
                var brushOut = _usingFirstBrush ? BackgroundBrush1 : BackgroundBrush2;

                brushIn.ImageSource = newImage;

                var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(1000));
                var fadeOut = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(1000));

                brushIn.BeginAnimation(System.Windows.Media.ImageBrush.OpacityProperty, fadeIn);
                brushOut.BeginAnimation(System.Windows.Media.ImageBrush.OpacityProperty, fadeOut);

                _usingFirstBrush = !_usingFirstBrush;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to load background {fileName}: {ex.Message}");
            }
        }

        #region Window Buttons

        private void MinimizeWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

        private void RestoreWindow_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Normal;

        private void CloseWindow_Click(object sender, RoutedEventArgs e) => Close();

        #endregion

        /// <summary>
        /// Allow dragging the window from the border
        /// </summary>
        private void _MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
                DragMove();
        }
    }
}
