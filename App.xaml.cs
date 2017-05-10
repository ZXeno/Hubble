using System;
using System.IO;
using System.Windows;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.ViewModel;

namespace DeviceMonitor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        public static string WorkingPath = Environment.CurrentDirectory;
        public static string UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Hubble";
        public static EventPublisher EventAggregator { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            EventAggregator = new EventPublisher();

            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            var mainWindowViewModel = new MainWindowViewModel();
            var window = new MainWindow()
            {
                Title = "Hubble",
                ResizeMode = ResizeMode.CanResize,
                DataContext = mainWindowViewModel,
                MinWidth = 500,
                MinHeight = 200
            };
            Application.Current.MainWindow = window;
            window.Show();
        }
    }
}
