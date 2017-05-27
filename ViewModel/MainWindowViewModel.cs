using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
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
            get => _selectedRateValue ?? (_selectedRateValue = _values[0]);
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
            get => _selectedDeviceStatus;
            set
            {
                _selectedDeviceStatus = value;
                OnPropertyChanged("SelectedDeviceStatus");
            }
        }

        private bool _runAtStartup;
        public bool RunAtStartup
        {
            get => _runAtStartup;
            set
            {
                _runAtStartup = value;
                OnPropertyChanged("RunAtStartup");
            }
        }

        private string _checkStatus;
        public string CheckStatus
        {
            get => _checkStatus;
            set
            {
                _checkStatus = value;
                OnPropertyChanged("CheckStatus");
            }
        }

        private EditTagControlViewModel _editTagViewModel;
        public EditTagControlViewModel EditTagViewModel
        {
            get => _editTagViewModel;
            set
            {
                _editTagViewModel = value;
                OnPropertyChanged("EditTagViewModel");
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

        private ICommand _editTag;
        public ICommand EditTag
        {
            get
            {
                if (_editTag == null)
                {
                    EditTag = new DelegateCommand(param => EditTagExecute(this, null), param => EditTagCanExecute());
                }
                return _editTag;
            }
            set
            {
                _editTag = value;
                OnPropertyChanged("EditTag");
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

        private ICommand _enableListViewCommand;
        public ICommand EnableListViewCommand
        {
            get
            {
                if (_enableListViewCommand == null)
                {
                    EnableListViewCommand = new DelegateCommand(param => EnableListViewCommandExecute(this, null), param => EnableListViewCommandCanExecute());
                }
                return _enableListViewCommand;
            }
            set
            {
                _enableListViewCommand = value;
                OnPropertyChanged("EnableListViewCommand");
            }
        }

        private ICommand _enableGridViewCommand;
        public ICommand EnableGridViewCommand
        {
            get
            {
                if (_enableGridViewCommand == null)
                {
                    EnableGridViewCommand = new DelegateCommand(param => EnableGridViewCommandExecute(this, null), param => EnableGridViewCommandCanExecute());
                }
                return _enableGridViewCommand;
            }
            set
            {
                _enableGridViewCommand = value;
                OnPropertyChanged("EnableGridViewCommand");
            }
        }

        private bool _listVisibility;
        public bool ListVisibility
        {
            get => _listVisibility;
            set
            {
                _listVisibility = value;
                OnPropertyChanged("ListVisibility");

                if (_listVisibility && GridVisibility)
                {
                    GridVisibility = false;
                }
            }
        }

        private bool _gridVisibility;
        public bool GridVisibility
        {
            get => _gridVisibility;
            set
            {
                _gridVisibility = value;
                OnPropertyChanged("GridVisibility");

                if (_gridVisibility && ListVisibility)
                {
                    ListVisibility = false;
                }
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

            EditTagViewModel = new EditTagControlViewModel(publisher);
            ListVisibility = true;
            GridVisibility = false;

            _eventPublisher.GetEvent<TimedEvent>().Subscribe(UpdateCheckStatus);
            _eventPublisher.GetEvent<DeviceListUpdateEvent>().Subscribe(HandleDeviceListChangeEvent);
            
            App.StatusManager.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);

            RunAtStartup = _registry.CheckForStartupRegistryKey();
        }

        #endregion

        #region Functions

        private void UpdateTimer()
        {
            App.StatusManager.SetTimer(SelectedRateValue.IntValue, DeviceStatusCollection);
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
                var stringtoadd = model.Device;
                if (!string.IsNullOrEmpty(model.Tag))
                {
                    stringtoadd += $",{model.Tag}";
                }

                tmp.Add(stringtoadd);
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
                _fileAndFolderServices.SaveDeviceList(DeviceStatusCollection.Select(model => $"{model.Device},{model.Tag}").ToList());
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

        private void EditTagExecute(object sender, EventArgs e)
        {
            _eventPublisher.Publish(new UpdateTagEvent {StatusRecordGuid = SelectedDeviceStatus.GUID, Device = SelectedDeviceStatus.Device, NewTag = SelectedDeviceStatus.Tag, OpenPopup = true});
        }

        private void EnableListViewCommandExecute(object sender, EventArgs e)
        {
            ListVisibility = true;
        }

        private void EnableGridViewCommandExecute(object sender, EventArgs e)
        {
            GridVisibility = true;
        }

        private bool RemoveItemCanExecute() => true;

        private bool SaveDeviceListCanExecute() => true;

        private bool EditCommandCanExecute() => true;

        private bool ToggleRunAtLogonCommandCanExecute() => true;

        private bool RefreshCommandCanExecute() => DeviceStatusCollection.Count > 0;

        private bool OpenAboutCommandCanExecute() => true;

        private bool EditTagCanExecute() => SelectedDeviceStatus != null;

        private bool EnableListViewCommandCanExecute() => true;

        private bool EnableGridViewCommandCanExecute() => true;

        #endregion
    }
}