using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
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
            get { return App.StatusManager.DeviceList ?? new ObservableCollection<DeviceStatusModel>(); }
            set
            {
                App.StatusManager.DeviceList = value;
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

        private ICommand _openAboutCommand;
        public ICommand OpenAboutCommand
        {
            get
            {
                if (_openAboutCommand == null)
                {
                    OpenAboutCommand = new DelegateCommand(param => OpenAboutCommandExecute(this, null), param => OpenAboutCommandCanExecute());
                }
                return _openAboutCommand;
            }
            set
            {
                _openAboutCommand = value;
                OnPropertyChanged("OpenAboutCommand");
            }
        }

        #endregion

        #region Dependencies

        private IEventPublisher _eventPublisher;
        private IRegistryServices _registry;
        private IFileAndFolderServices _fileAndFolderServices;
        private IWindowService _windowService;

        #endregion

        #region Constructor

        public MainWindowViewModel(IEventPublisher publisher, IRegistryServices registry, IFileAndFolderServices fileAndFolder, IWindowService windowService)
        {
            _eventPublisher = publisher;
            _registry = registry;
            _fileAndFolderServices = fileAndFolder;
            _windowService = windowService;
            _checkStatus = "Idle";

            _eventPublisher.GetEvent<TimedEvent>().Subscribe(UpdateCheckStatus);
            _eventPublisher.GetEvent<DeviceListUpdateEvent>().Subscribe(HandleDeviceListChangeEvent);
            
            if (File.Exists($"{App.UserFolder}\\devicelist.txt"))
            {
                LoadDeviceList();
            }

            App.StatusManager.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);

            RunAtStartup = _registry.CheckForStartupRegistryKey();
        }

        #endregion

        #region Functions

        private void UpdateTimer()
        {
            App.StatusManager.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);
        }

        private void LoadDeviceList()
        {
            var list = _fileAndFolderServices.LoadDeviceList();
            _eventPublisher.Publish(new DeviceListUpdateEvent {DeviceList = list.ToList()});
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

            _windowService.ShowDialog<ComputerListView>(new ComputerListViewModel(_eventPublisher, tmp));
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
                _fileAndFolderServices.SaveDeviceList(DeviceStatusCollection.Select(statusModel => statusModel.Device).ToList());
            }
        }

        private void ToggleRunAtLogonCommandExecute(object sender, EventArgs e)
        {
            _registry.ToggleRunOnStartup();
            RunAtStartup = _registry.CheckForStartupRegistryKey();
        }

        private void RefreshCommandExecute(object sender, EventArgs e)
        {
            UpdateTimer();
        }

        private void OpenAboutCommandExecute(object sender, EventArgs e)
        {
            _windowService.ShowDialog<About>(new AboutWindowViewModel());
        }

        private bool RemoveItemCanExecute() => true;

        private bool SaveDeviceListCanExecute() => true;

        private bool EditCommandCanExecute() => true;

        private bool ToggleRunAtLogonCommandCanExecute() => true;

        private bool RefreshCommandCanExecute() => DeviceStatusCollection.Count > 0;

        private bool OpenAboutCommandCanExecute() => true;

        #endregion
    }
}