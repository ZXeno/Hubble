using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace DeviceMonitor.Model
{
    public class DeviceStatusModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private string _device;
        public string Device
        {
            get { return _device; }
            set
            {
                _device = value;
                OnPropertyChanged();
            }
        }

        private string _ipAddress;
        public string IpAddress
        {
            get { return _ipAddress; }
            set
            {
                _ipAddress = value;
                OnPropertyChanged();
            }
        }

        private string _lastSeen;
        public string LastSeen
        {
            get
            {
                return _lastSeen;
            }
            set
            {
                _lastSeen = value;
                OnPropertyChanged();
            }
        }

        private bool _online;
        public bool Online
        {
            get
            {
                return _online;
            }
            set
            {
                _online = value;
                OnPropertyChanged();
                OnPropertyChanged("OnlineString"); // This ensures the "OnlineString" property change notification changes when this does.
            }
        }

        private string _loggedOnUser;
        public string LoggedOnUser
        {
            get
            {
                return _loggedOnUser;
            }
            set
            {
                _loggedOnUser = value;
                OnPropertyChanged();
            }
        }

        private string _tag;
        public string Tag
        {
            get
            {
                return _tag;
            }
            set
            {
                _tag = value;
                OnPropertyChanged();
            }
        }

        public string OnlineString => !Online ? "Offline" : "Online";

        
    }
}