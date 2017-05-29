using System;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.Model;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.ViewModel
{
    public class DeviceDetailsViewModel : ViewModelBase
    {
        private DeviceStatusModel _status;
        public DeviceStatusModel Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged("Status");
                OnPropertyChanged("MultiEntryStatusText"); // Makes sure that this value is updated when this is set.
            }
        }

        private ICommand _doneCommand;
        public ICommand DoneCommand
        {
            get
            {
                if (_doneCommand == null)
                {
                    DoneCommand = new DelegateCommand(param => DoneCommandExecute(this, null), param => DoneCommandCanExecute());
                }
                return _doneCommand;
            }
            set
            {
                _doneCommand = value;
                OnPropertyChanged("DoneCommand");
            }
        }

        public string MultiEntryStatusText
        {
            get
            {
                if (Status != null && Status.MultipleAddress)
                {
                    return "This device may have a stale DNS entry.";
                }

                return "";
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                OnPropertyChanged("IsOpen");
            }
        }

        private readonly IEventPublisher _publisher;

        public DeviceDetailsViewModel(IEventPublisher publisher)
        {
            _publisher = publisher;

            _publisher.GetEvent<DeviceDetailsOpenEvent>().Subscribe(OpenControl);
        }

        private void OpenControl(DeviceDetailsOpenEvent detailsEvent)
        {
            if (!detailsEvent.OpenDetails) return;
            Status = detailsEvent.Status;
            IsOpen = true;
        }

        private void DoneCommandExecute(object sender, EventArgs e)
        {
            IsOpen = false;
        }

        private bool DoneCommandCanExecute() => true;
    }
}