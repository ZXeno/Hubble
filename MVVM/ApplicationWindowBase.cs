using System;
using System.Windows;

namespace DeviceMonitor.MVVM
{
    public class ApplicationWindowBase : Window
    {


        public ApplicationWindowBase()
        {
            DataContextChanged += OnDataContextChanged;
        }

        /// <summary>
        /// Always opens a new Dialog as the top-most window.
        /// </summary>
        /// 
        public void ShowAsTopmostDialog()
        {
            Topmost = true;
            ShowDialog();
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs de)
        {
            var model = de.NewValue as IRequestCloseViewModel;
            if (model != null)
            {
                // if the new datacontext supports the IRequestCloseViewModel we can use
                // the event to be notified when the associated viewmodel wants to close
                // the window
                model.RequestClose += OnClose;
            }
        }

        private void OnClose(object sender, EventArgs de)
        {
            var model = DataContext as IRequestCloseViewModel;
            if (model != null)
            {
                model.RequestClose -= OnClose;
            }

            Close();
        }
    }
}