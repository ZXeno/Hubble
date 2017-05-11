using System;
using System.IO;
using System.Windows;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.Infrastructure.IoC;
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
        public static IoCContainer IoC { get; private set; }
        public static StatusManager StatusManager { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            InitializeIoCContainer();

            StatusManager = new StatusManager(
                IoC.Resolve<IEventPublisher>(), 
                IoC.Resolve<INetworkServices>(),
                IoC.Resolve<IWmiServices>());

            if (!Directory.Exists(UserFolder))
            {
                Directory.CreateDirectory(UserFolder);
            }

            var mainWindowViewModel = new MainWindowViewModel(
                IoC.Resolve<IEventPublisher>(),
                IoC.Resolve<IRegistryServices>(),
                IoC.Resolve<IFileAndFolderServices>(),
                IoC.Resolve<IWindowService>());
            var window = new MainWindow()
            {
                Title = "Hubble",
                ResizeMode = ResizeMode.CanResize,
                DataContext = mainWindowViewModel,
                MinWidth = 500,
                MinHeight = 200
            };
            Application.Current.MainWindow = window; // Assign this window as the application's main window
            window.Show();
        }

        private void InitializeIoCContainer()
        {
            IoC = new IoCContainer();

            IoC.Register<IEventPublisher, EventPublisher>(LifeTimeOptions.ContainerControlledLifeTimeOption);
            IoC.Register<IFileAndFolderServices, FileAndFolderService>();
            IoC.Register<INetworkServices, NetworkServices>();
            IoC.Register<IRegistryServices, RegistryServices>();
            IoC.Register<IWindowService, WindowService>();
            IoC.Register<IWmiServices, WmiServices>();
        }
    }
}
