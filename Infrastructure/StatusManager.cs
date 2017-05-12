using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.Model;
using Timer = System.Timers.Timer;

namespace DeviceMonitor.Infrastructure
{
    public class StatusManager
    {
        private readonly StaTaskScheduler _staTaskScheduler = new StaTaskScheduler(5);
        private readonly IFileAndFolderServices _fileAndFolder;
        private readonly INetworkServices _network;
        private readonly IWmiServices _wmi;
        private readonly IEventPublisher _eventPublisher;

        private ObservableCollection<DeviceStatusModel> _deviceList;
        public ObservableCollection<DeviceStatusModel> DeviceList
        {
            get { return _deviceList ?? (_deviceList = new ObservableCollection<DeviceStatusModel>()); }
            set { _deviceList = value; }
        }

        private Timer _timer;

        public StatusManager(IEventPublisher publisher, INetworkServices networkServices, IWmiServices wmiServices, IFileAndFolderServices fileAndFolder)
        {
            _network = networkServices;
            _wmi = wmiServices;
            _eventPublisher = publisher;
            _fileAndFolder = fileAndFolder;

            LoadDeviceList();

            _eventPublisher.GetEvent<DeviceListUpdateEvent>().Subscribe(HandleDeviceListChangeEvent);
            _eventPublisher.GetEvent<UpdateTagEvent>().Subscribe(HandleUpdateTagEvent);
        }

        private void LoadDeviceList()
        {
            var importedDeviceList = _fileAndFolder.LoadDeviceList().ToList();
            if (importedDeviceList.Count == 0) { return; }

            UpdateDeviceCollection(importedDeviceList);
        }

        public static DeviceStatusModel GetNewDeviceStatus(string device)
        {
            var devList = new List<string>(device.Split(new string[] { ",",";","-","_","/" }, StringSplitOptions.RemoveEmptyEntries));
            var model = new DeviceStatusModel();

            model.Device = devList[0];
            if (devList.Count > 1)
            {
                model.Tag = devList[1];
            }
            
            return model;
        }

        public void UpdateDeviceStatus(ref DeviceStatusModel statusModel)
        {
            var time = DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString();

            if (_network.VerifyDeviceConnectivity(statusModel.Device))
            {
                statusModel.LastSeen = time;
                statusModel.Online = true;
                try
                {
                    statusModel.LoggedOnUser = _wmi.QueryLoggedOnUser(statusModel.Device);
                }
                catch (Exception)
                {
                    statusModel.LoggedOnUser = "Unable to query";
                }
            }
            else
            {
                statusModel.Online = false;
            }
        }

        public void SetTimer(int timeInMinutes, ObservableCollection<DeviceStatusModel> devList)
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Dispose();
                _timer = null;
            }

            _deviceList = devList;
            _timer = new Timer(timeInMinutes * 60000);
            _timer.Elapsed += UpdateDeviceStatusList;
            _timer.AutoReset = true;
            _timer.Enabled = true;

            UpdateDeviceStatusList(null, EventArgs.Empty);
        }

        private void UpdateDeviceStatusList(object sender, EventArgs e)
        {
            var t = Task.Factory.StartNew(
                () =>
                {
                    _eventPublisher.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Checking..." });
                    
                    lock (DeviceList)
                    {
                        Parallel.ForEach(DeviceList, (devStatus) =>
                        {
                            UpdateDeviceStatus(ref devStatus);
                        });
                    }

                    _eventPublisher.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Idle", DeviceStatusList = DeviceList });

                }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);
        }

        private void UpdateDeviceCollection(List<string> modifiedDeviceList)
        {
            if (modifiedDeviceList == null) { return; }

            var devNameList = DeviceList.Select(model => model.Device).ToList();

            var updateList = modifiedDeviceList;

            var listToRemove = devNameList.Where(p => !updateList.Contains(p));
            var listToAdd = updateList.Where(x => !devNameList.Contains(x));
            
            foreach (var dev in listToRemove)
            {
                var ds = DeviceList.FirstOrDefault(x => x.Device == dev);
                DeviceList.Remove(ds);
            }

            foreach (var dev in listToAdd)
            {
                DeviceList.Add(GetNewDeviceStatus(dev));
            }
        }

        private void HandleUpdateTagEvent(UpdateTagEvent updateEvent)
        {
            if (updateEvent == null || updateEvent.OpenPopup || updateEvent.Device == "") { return; }

            var status = DeviceList.FirstOrDefault(x => x.GUID == updateEvent.StatusRecordGuid);
            if (status == null) { return; }

            status.Tag = updateEvent.NewTag;
        }

        private void HandleDeviceListChangeEvent(DeviceListUpdateEvent updateEvent)
        {
            if (updateEvent.DeviceListIsUpdated) { return; }

            UpdateDeviceCollection(updateEvent.DeviceList);
            updateEvent.DeviceListIsUpdated = true;
            _eventPublisher.Publish(updateEvent);
        }
    }
}