using System;
using System.Windows;
using System.Windows.Controls.Primitives;

namespace DeviceMonitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private Popup popup;

        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnLocationChanged(EventArgs e)
        {
            if (popup == null) { return; }

            popup.HorizontalOffset += 1;
            popup.HorizontalOffset -= 1;
            base.OnLocationChanged(e);
        }

        private void Popup_OnOpened(object sender, EventArgs e)
        {
            popup = sender as Popup;
        }

        private void Popup_OnClosed(object sender, EventArgs e)
        {
            popup = null;
        }
    }
}
