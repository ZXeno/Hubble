using System;
using System.Windows.Input;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.ViewModel
{
    public class AboutWindowViewModel : RequestCloseViewModel
    {
        private ICommand _requestCloseCommand;
        public ICommand RequestCloseCommand
        {
            get
            {
                if (_requestCloseCommand == null)
                {
                    RequestCloseCommand = new DelegateCommand(param => RequestCloseCommandExecute(this, null), param => RequestCloseCanExecute());
                }
                return _requestCloseCommand;
            }
            set
            {
                _requestCloseCommand = value;
                OnPropertyChanged("RequestCloseCommand");
            }
        }

        private void RequestCloseCommandExecute(object sender, EventArgs e)
        {
            OnRequestClose(EventArgs.Empty);
        }

        private bool RequestCloseCanExecute() => true;
    }
}