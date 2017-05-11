using System.Windows;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.Infrastructure
{
    public class WindowService : IWindowService
    {
        public void ShowWindow<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
        {
            var win = new T
            {
                DataContext = viewModel
            };
            win.Show();
        }

        public void ShowDialog<T>(ViewModelBase viewModel) where T : ApplicationWindowBase, new()
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