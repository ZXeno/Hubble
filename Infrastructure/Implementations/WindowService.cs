using System.Windows;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.Infrastructure
{
    public static class WindowService
    {
        // TODO: FOR THE LOVE OF GOD DON'T LEAVE THIS AS A STATIC CLASS!!

        public static void ShowWindow<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel
            };
            win.Show();
        }

        public static void ShowDialog<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel,
                Owner = Application.Current.MainWindow
            };
            win.ShowAsTopmostDialog();
        }
    }
}