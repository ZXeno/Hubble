using System;
using System.Collections.ObjectModel;
using System.Threading;
using System.Threading.Tasks;
using DeviceMonitor.Model;
using Timer = System.Timers.Timer;

namespace DeviceMonitor.Infrastructure
{
    public static class StatusService
    {
        private static readonly StaTaskScheduler _staTaskScheduler = new StaTaskScheduler(5);

        public static event EventHandler TimedEvent;
        public static void OnTimedEventFired(object sender, EventArgs e)
        {
            TimedEvent?.Invoke(sender, e);
        }

        private static ObservableCollection<DeviceStatusModel> _deviceList;
        public static ObservableCollection<DeviceStatusModel> DeviceList
        {
            get { return _deviceList; }
            set { _deviceList = value; }
        }

        private static Timer _timer;

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
                catch (Exception e)
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
                    OnTimedEventFired(null, new TimedEventArgs { DateTime = DateTime.Now, Message = "Checking..." });

                    var temp = new ObservableCollection<DeviceStatusModel>();
                    lock (_deviceList)
                    {
                        Parallel.ForEach(_deviceList, (devStatus) =>
                        {
                            var s = devStatus;
                            if (s == null) { return; }
                            UpdateDeviceStatus(ref s);
                            temp.Add(s);
                        });
                    }

                    OnTimedEventFired(null, new TimedEventArgs { DateTime = DateTime.Now, Message = "Idle", DeviceStatusList = temp });

                }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);
        }
    }
}