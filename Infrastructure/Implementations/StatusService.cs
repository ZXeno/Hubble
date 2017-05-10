using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceMonitor.Model;
using Timer = System.Timers.Timer;

namespace DeviceMonitor.Infrastructure
{
    // TODO: FOR THE LOVE OF GOD DON'T LEAVE THIS AS A STATIC CLASS!!

    public static class StatusService
    {
        private static readonly StaTaskScheduler _staTaskScheduler = new StaTaskScheduler(5);

        private static ObservableCollection<DeviceStatusModel> _deviceList;
        public static ObservableCollection<DeviceStatusModel> DeviceList
        {
            get
            {
                if (_deviceList == null)
                {
                    _deviceList = new ObservableCollection<DeviceStatusModel>();
                }
                return _deviceList;
            }
            set { _deviceList = value; }
        }

        private static Timer _timer;

        static StatusService()
        {
            App.EventAggregator.GetEvent<DeviceListUpdateEvent>().Subscribe(HandleDeviceListChangeEvent);
        }

        public static DeviceStatusModel GetNewDeviceStatus(string device)
        {
            var model = new DeviceStatusModel
            {
                Device = device
            };

            return model;
        }

        public static void UpdateDeviceStatus(ref DeviceStatusModel statusModel)
        {
            var time = DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString();

            if (NetworkServices.VerifyDeviceConnectivity(statusModel.Device))
            {
                statusModel.LastSeen = time;
                statusModel.Online = true;
                try
                {
                    statusModel.LoggedOnUser = WmiServices.QueryLoggedOnUser(statusModel.Device);
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

        public static void SetTimer(int timeInMinutes, ObservableCollection<DeviceStatusModel> devList)
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

        private static void UpdateDeviceStatusList(object sender, EventArgs e)
        {
            var t = Task.Factory.StartNew(
                () =>
                {
                    App.EventAggregator.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Checking..." });
                    
                    lock (DeviceList)
                    {
                        Parallel.ForEach(DeviceList, (devStatus) =>
                        {
                            UpdateDeviceStatus(ref devStatus);
                        });
                    }
                    
                    App.EventAggregator.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Idle", DeviceStatusList = DeviceList });

                }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);
        }

        public static void UpdateDeviceCollection(List<string> modifiedDeviceList)
        {
            if (modifiedDeviceList == null || modifiedDeviceList.Count == 0) { return; }

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

        private static void HandleDeviceListChangeEvent(DeviceListUpdateEvent updateEvent)
        {
            if (updateEvent.DeviceListIsUpdated) { return; }

            UpdateDeviceCollection(updateEvent.DeviceList);
            updateEvent.DeviceListIsUpdated = true;
            App.EventAggregator.Publish(updateEvent);
        }
    }
}