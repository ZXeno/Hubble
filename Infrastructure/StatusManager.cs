using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.Model;
using Timer = System.Timers.Timer;

namespace DeviceMonitor.Infrastructure
{
    public class StatusManager
    {
        private readonly StaTaskScheduler _staTaskScheduler = new StaTaskScheduler(12);
        private readonly IFileAndFolderServices _fileAndFolder;
        private readonly INetworkServices _network;
        private readonly IWmiServices _wmi;
        private readonly IEventPublisher _eventPublisher;

        private ObservableCollection<DeviceStatusModel> _deviceList;
        public ObservableCollection<DeviceStatusModel> DeviceList
        {
            get => _deviceList ?? (_deviceList = new ObservableCollection<DeviceStatusModel>());
            set => _deviceList = value;
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
            _eventPublisher.GetEvent<ReportSaveRequestEvent>().Subscribe(HandleReportSaveRequestEvent);
        }

        private void LoadDeviceList()
        {
            var importedDeviceList = _fileAndFolder.LoadDeviceList().ToList();
            if (importedDeviceList?.Count == 0) { return; }

            UpdateDeviceCollection(importedDeviceList);
        }

        private void HandleReportSaveRequestEvent(ReportSaveRequestEvent eventRequest)
        {
            var deviceList = new List<string>();

            switch (eventRequest.ReportType)
            {
                case SaveEventEnum.OnlineReport:
                    deviceList = DeviceList.Where(x => x.Online == true).Select(device => device.Device).ToList();
                    break;
                case SaveEventEnum.OfflineReport:
                    deviceList = DeviceList.Where(x => x.Online == false).Select(device => device.Device).ToList();
                    break;
                case SaveEventEnum.StaleRecordReport:
                    deviceList = DeviceList.Where(x => x.PotentiallyStaleRecords).Select(device => device.Device).ToList();
                    break;
            }

            _fileAndFolder.SaveReport(deviceList, eventRequest.SavePath);
        }

        public static DeviceStatusModel GetNewDeviceStatus(string device)
        {
            var devList = new List<string>(device.Split(new string[] { ",",";" }, StringSplitOptions.RemoveEmptyEntries));
            var model = new DeviceStatusModel
            {
                Device = new string(devList[0].ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray()) // Clear any whitespaces from the device name string
            };

            if (devList.Count > 1)
            {
                model.Tag = devList[1];
            }
            
            return model;
        }

        public void UpdateDeviceStatus(ref DeviceStatusModel statusModel)
        {
            var time = DateTime.Now.ToShortTimeString() + " " + DateTime.Now.ToShortDateString();

            try
            {

                var pingTest = _network.PingTest(statusModel.Device);

                if (_network.VerifyDeviceConnectivity(pingTest))
                {
                    statusModel.LastSeen = time;
                    statusModel.Online = true;
                    statusModel.IpAddress = pingTest?.Address?.ToString() ?? "UKNOWN_ERROR";
                    statusModel.PotentiallyStaleRecords = _network.CheckForMultipleRecords(statusModel.Device);
                    statusModel.LoggedOnUser = _wmi.QueryLoggedOnUser(statusModel.Device);
                }
                else
                {
                    statusModel.Online = false;
                    statusModel.IpAddress = _network.GetIpStatusMessage(pingTest.Status);
                }
            }
            catch (Exception ex)
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
                    var doWait = false;

                    try
                    {
                        lock (DeviceList)
                        {
                            Parallel.ForEach(DeviceList, (devStatus) =>
                            {
                                UpdateDeviceStatus(ref devStatus);
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        doWait = true;
                    }

                    if (doWait)
                    {
                        _eventPublisher.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Error! Waiting 15 seconds..." });
                        Thread.Sleep(15000);
                    }

                    _eventPublisher.Publish(new TimedEvent { DateTime = DateTime.Now, Message = "Idle", DeviceStatusList = DeviceList });

                }, CancellationToken.None, TaskCreationOptions.None, _staTaskScheduler);
        }

        private void UpdateDeviceCollection(List<string> modifiedDeviceList)
        {
            if (modifiedDeviceList == null) { return; }
            var devList = new List<string>();
            var devListWithTag = new List<Pair<string,string>>();

            foreach (var listval in modifiedDeviceList)
            {
                var retval = listval.Split(new string[] {",", ";"}, StringSplitOptions.RemoveEmptyEntries);
                devList.Add(retval[0]);

                var tagPair = new Pair<string, string> {Value1 = retval[0]};
                if (retval.Length > 1)
                {
                    tagPair.Value2 = retval[1];
                }

                devListWithTag.Add(tagPair);
            }

            var devNameList = DeviceList.Select(model => model.Device).ToList();

            var updateList = devList;

            var listToRemove = devNameList.Where(x => !updateList.Contains(x));
            var listToAdd = updateList.Where(x => !devNameList.Contains(x));
            var listToChange = updateList.Where(x => devNameList.Contains(x));
            
            foreach (var dev in listToRemove)
            {
                var ds = DeviceList.FirstOrDefault(x => x.Device == dev);
                DeviceList.Remove(ds);
            }

            foreach (var dev in listToAdd)
            {
                if (DeviceList.FirstOrDefault(x => x.Device == dev) != null) { continue; }

                var newDeviceStatus = GetNewDeviceStatus(dev);
                var tagdata = devListWithTag.FirstOrDefault(x => x.Value1 == dev);
                if (!string.IsNullOrEmpty(tagdata?.Value2))
                {
                    newDeviceStatus.Tag = tagdata.Value2;
                }

                DeviceList.Add(newDeviceStatus);
            }

            foreach (var dev in listToChange)
            {
                var tagUpdate = devListWithTag.FirstOrDefault(x => x.Value1 == dev)?.Value2 ?? "";
                var statusToUpdate = DeviceList.FirstOrDefault(x => x.Device == dev);

                if (statusToUpdate == null || statusToUpdate.Tag == tagUpdate) { continue; }

                statusToUpdate.Tag = tagUpdate;
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