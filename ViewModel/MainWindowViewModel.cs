using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Model;
using DeviceMonitor.MVVM;
using DeviceMonitor.View;

namespace DeviceMonitor.ViewModel
{
    public class MainWindowViewModel : ViewModelBase
    {
        public static event EventHandler DeviceListChangeEvent;
        public static void OnDeviceListChangeEvent(object sender, EventArgs e)
        {
            DeviceListChangeEvent?.Invoke(sender, e);
        }
        
        public ObservableCollection<DeviceStatusModel> DeviceStatusCollection
        {
            get { return StatusService.DeviceList ?? new ObservableCollection<DeviceStatusModel>(); }
            set
            {
                StatusService.DeviceList = value;
                OnPropertyChanged("DeviceStatusCollection");
            }
        }

        private List<ComboxValuePair> _values = new List<ComboxValuePair>
        {
            new ComboxValuePair("1 Minute", 1),
            new ComboxValuePair("5 Minutes", 5),
            new ComboxValuePair("10 Minutes", 10),
            new ComboxValuePair("15 Minutes", 15),
            new ComboxValuePair("20 Minutes", 20),
            new ComboxValuePair("25 Minutes", 25),
            new ComboxValuePair("30 Minutes", 30),

        };
        public List<ComboxValuePair> Values => _values;

        private ComboxValuePair _selectedRateValue;
        public ComboxValuePair SelectedRateValue
        {
            get { return _selectedRateValue ?? (_selectedRateValue = _values[0]); }
            set
            {
                _selectedRateValue = value;
                OnPropertyChanged("SelectedRateValue");
                UpdateTimer();
            }
        }

        private DeviceStatusModel _selectedDeviceStatus;
        public DeviceStatusModel SelectedDeviceStatus
        {
            get { return _selectedDeviceStatus; }
            set
            {
                _selectedDeviceStatus = value;
                OnPropertyChanged("SelectedDeviceStatus");
            }
        }

        private string _checkStatus;
        public string CheckStatus
        {
            get { return _checkStatus; }
            set
            {
                _checkStatus = value;
                OnPropertyChanged("CheckStatus");
            }
        }

        private ICommand _editCommand;
        public ICommand EditCommand
        {
            get
            {
                if (_editCommand == null)
                {
                    EditCommand = new DelegateCommand(param => EditCommandExecute(this, null), param => EditCommandCanExecute());
                }
                return _editCommand;
            }
            set
            {
                _editCommand = value;
                OnPropertyChanged("EditCommand");
            }
        }

        private ICommand _removeItem;
        public ICommand RemoveItem
        {
            get
            {
                if (_removeItem == null)
                {
                    RemoveItem = new DelegateCommand(param => RemoveItemExecute(this, null), param => RemoveItemCanExecute());
                }
                return _removeItem;
            }
            set
            {
                _removeItem = value;
                OnPropertyChanged("RemoveItem");
            }
        }

        public MainWindowViewModel()
        {
            StatusService.DeviceList = new ObservableCollection<DeviceStatusModel>();

            _checkStatus = "Idle";

            StatusService.TimedEvent += UpdateCheckStatus;
            DeviceListChangeEvent += HandleDeviceListChangeEvent;

            StatusService.SetTimer(SelectedRateValue.IntValue);
        }

        private void UpdateTimer()
        {
            StatusService.SetTimer(SelectedRateValue.IntValue);
        }

        private void UpdateCheckStatus(object sender, EventArgs e)
        {
            var args = e as TimedEventArgs;
            if (args == null) return;

            if (args.DeviceStatusList != null)
            {
                DeviceStatusCollection = args.DeviceStatusList;
            }

            CheckStatus = args.Message;
        }

        private void EditCommandExecute(object sender, EventArgs e)
        {
            var tmp = new List<string>();
            foreach (var model in DeviceStatusCollection)
            {
                tmp.Add(model.Device);
            }

            WindowService.ShowDialog<ComputerListView>(new ComputerListViewModel(tmp));
        }

        private bool EditCommandCanExecute()
        {
            return true;
        }

        private void HandleDeviceListChangeEvent(object sender, EventArgs e)
        {
            var devNameList = new List<string>();
            foreach (var model in DeviceStatusCollection)
            {
                devNameList.Add(model.Device);
            }

            var devListEventArgs = e as DeviceListUpdateEventArgs;
            if (devListEventArgs?.DeviceList.Count == 0) { return; }
            var updateList = devListEventArgs.DeviceList;

            var listToRemove = devNameList.Where(p => !updateList.Contains(p));
            var listToAdd = updateList.Where(x => !devNameList.Contains(x));

            var tempStatusCollection = new ObservableCollection<DeviceStatusModel>(DeviceStatusCollection);
            foreach (var dev in listToRemove)
            {
                var ds = tempStatusCollection.FirstOrDefault(x => x.Device == dev);
                tempStatusCollection.Remove(ds);
            }

            foreach (var dev in listToAdd)
            {
                tempStatusCollection.Add(StatusService.GetNewDeviceStatus(dev));
            }

            DeviceStatusCollection = tempStatusCollection;
            UpdateTimer();
        }

        private bool RemoveItemCanExecute()
        {
            return true;
        }

        private void RemoveItemExecute(object sender, EventArgs e)
        {
            if (!DeviceStatusCollection.Contains(SelectedDeviceStatus)) { return; }

            DeviceStatusCollection.Remove(SelectedDeviceStatus);
            SelectedDeviceStatus = null;
            UpdateTimer();
        }
    }
}