using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
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
        #region Properties


        public ObservableCollection<DeviceStatusModel> DeviceStatusCollection
        {
            get { return StatusService.DeviceList ?? new ObservableCollection<DeviceStatusModel>(); }
            set
            {
                StatusService.DeviceList = value;
                OnPropertyChanged("DeviceStatusCollection");
            }
        }

        private readonly List<ComboxValuePair> _values = new List<ComboxValuePair>
        {
            new ComboxValuePair("1 Minute", 1),
            new ComboxValuePair("5 Minutes", 5),
            new ComboxValuePair("10 Minutes", 10),
            new ComboxValuePair("15 Minutes", 15),
            new ComboxValuePair("20 Minutes", 20),
            new ComboxValuePair("25 Minutes", 25),
            new ComboxValuePair("30 Minutes", 30)

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

        private bool _runAtStartup;
        public bool RunAtStartup
        {
            get { return _runAtStartup; }
            set
            {
                _runAtStartup = value;
                OnPropertyChanged("RunAtStartup");
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

        private ICommand _saveDeviceListCommand;
        public ICommand SaveDeviceListCommand
        {
            get
            {
                if (_saveDeviceListCommand == null)
                {
                    SaveDeviceListCommand = new DelegateCommand(param => SaveDeviceListExecute(this, null), param => SaveDeviceListCanExecute());
                }
                return _saveDeviceListCommand;
            }
            set
            {
                _saveDeviceListCommand = value;
                OnPropertyChanged("SaveDeviceListCommand");
            }
        }

        private ICommand _toggleRunAtLoggonCommand;
        public ICommand ToggleRunAtLogonCommand
        {
            get
            {
                if (_toggleRunAtLoggonCommand == null)
                {
                    ToggleRunAtLogonCommand = new DelegateCommand(param => ToggleRunAtLogonCommandExecute(this, null), param => ToggleRunAtLogonCommandCanExecute());
                }
                return _toggleRunAtLoggonCommand;
            }
            set
            {
                _toggleRunAtLoggonCommand = value;
                OnPropertyChanged("ToggleRunAtLogonCommand");
            }
        }

        private ICommand _refreshCommand;
        public ICommand RefreshCommand
        {
            get
            {
                if (_refreshCommand == null)
                {
                    RefreshCommand = new DelegateCommand(param => RefreshCommandExecute(this, null), param => RefreshCommandCanExecute());
                }
                return _refreshCommand;
            }
            set
            {
                _refreshCommand = value;
                OnPropertyChanged("RefreshCommand");
            }
        }

        #endregion
        
        #region Constructor

        public MainWindowViewModel()
        {
            _checkStatus = "Idle";
            
            App.EventAggregator.GetEvent<TimedEvent>().Subscribe(UpdateCheckStatus);
            App.EventAggregator.GetEvent<DeviceListUpdateEvent>().Subscribe(HandleDeviceListChangeEvent);
            
            if (File.Exists($"{App.UserFolder}\\devicelist.txt"))
            {
                LoadDeviceList();
            }

            StatusService.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);

            RunAtStartup = RegistryServices.CheckForStartupRegistryKey();
        }

        #endregion

        #region Functions

        private void UpdateTimer()
        {
            StatusService.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);
        }

        private void LoadDeviceList()
        {
            FileAndFolderService.LoadDeviceList();
        }

        private void UpdateCheckStatus(TimedEvent timedEvent)
        {
            if (timedEvent == null) return;

            if (timedEvent.DeviceStatusList != null && DeviceStatusCollection != timedEvent.DeviceStatusList)
            {
                DeviceStatusCollection = timedEvent.DeviceStatusList;
            }
            
            CheckStatus = timedEvent.Message;
        }

        private void HandleDeviceListChangeEvent(DeviceListUpdateEvent updateEvent)
        {
            if(!updateEvent.DeviceListIsUpdated) { return; }
            UpdateTimer();
            RefreshCommand = new DelegateCommand(param => RefreshCommandExecute(this, null), param => RefreshCommandCanExecute());
        }

        #endregion

        #region Commands

        private void EditCommandExecute(object sender, EventArgs e)
        {
            var tmp = new List<string>();
            foreach (var model in DeviceStatusCollection)
            {
                tmp.Add(model.Device);
            }

            WindowService.ShowDialog<ComputerListView>(new ComputerListViewModel(tmp));
        }

        private void RemoveItemExecute(object sender, EventArgs e)
        {
            if (!DeviceStatusCollection.Contains(SelectedDeviceStatus)) { return; }

            DeviceStatusCollection.Remove(SelectedDeviceStatus);
            SelectedDeviceStatus = null;
            UpdateTimer();
        }

        private void SaveDeviceListExecute(object sender, EventArgs e)
        {
            lock (DeviceStatusCollection)
            {
                FileAndFolderService.SaveDeviceList(DeviceStatusCollection.Select(statusModel => statusModel.Device).ToList());
            }
        }

        private void ToggleRunAtLogonCommandExecute(object sender, EventArgs e)
        {
            RegistryServices.ToggleRunOnStartup();
            RunAtStartup = RegistryServices.CheckForStartupRegistryKey();
        }

        private void RefreshCommandExecute(object sender, EventArgs e)
        {
            UpdateTimer();
        }

        private bool RemoveItemCanExecute()
        {
            return true;
        }

        private bool SaveDeviceListCanExecute()
        {
            return true;
        }

        private bool EditCommandCanExecute()
        {
            return true;
        }
        
        private bool ToggleRunAtLogonCommandCanExecute()
        {
            return true;
        }

        private bool RefreshCommandCanExecute()
        { 
            return DeviceStatusCollection.Count > 0;
        }

        #endregion
    }
}