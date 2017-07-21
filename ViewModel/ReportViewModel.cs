using System;
using System.Windows;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.MVVM;
using Microsoft.Win32;

namespace DeviceMonitor.ViewModel
{
    public class ReportViewModel : RequestCloseViewModel
    {
        private readonly string _defaultPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\Hubble";

        private ICommand _requestCloseCommand;
        public ICommand RequestCloseCommand
        {
            get
            {
                if (_requestCloseCommand == null)
                {
                    RequestCloseCommand = new DelegateCommand(param => RequestCloseCommandExecute(this, null), param => RequestCloseCanExecute());
                }
                return _requestCloseCommand;
            }
            set
            {
                _requestCloseCommand = value;
                OnPropertyChanged("RequestCloseCommand");
            }
        }

        private ICommand _saveOnlineReportCommand;
        public ICommand SaveOnlineReportCommand
        {
            get
            {
                if (_saveOnlineReportCommand == null)
                {
                    SaveOnlineReportCommand = new DelegateCommand(param => SaveOnlineReportCommandExecute(this, null), param => SaveOnlineReportCommandCanExecute());
                }
                return _saveOnlineReportCommand;
            }
            set
            {
                _saveOnlineReportCommand = value;
                OnPropertyChanged("SaveOnlineReportCommand");
            }
        }

        private ICommand _saveOfflineReportCommand;
        public ICommand SaveOfflineReportCommand
        {
            get
            {
                if (_saveOfflineReportCommand == null)
                {
                    SaveOfflineReportCommand = new DelegateCommand(param => SaveOfflineReportCommandExecute(this, null), param => SaveOfflineReportCommandCanExecute());
                }
                return _saveOfflineReportCommand;
            }
            set
            {
                _saveOfflineReportCommand = value;
                OnPropertyChanged("SaveOfflineReportCommand");
            }
        }

        private ICommand _saveStaleRecordReportCommand;
        public ICommand SaveStaleRecordReportCommand
        {
            get
            {
                if (_saveStaleRecordReportCommand == null)
                {
                    SaveStaleRecordReportCommand = new DelegateCommand(param => SaveStaleRecordReportCommandExecute(this, null), param => SaveStaleRecordReportCommandCanExecute());
                }
                return _saveStaleRecordReportCommand;
            }
            set
            {
                _saveStaleRecordReportCommand = value;
                OnPropertyChanged("SaveStaleRecordReportCommand");
            }
        }

        private readonly IEventPublisher _eventPublisher;

        public ReportViewModel(IEventPublisher publisher)
        {
            _eventPublisher = publisher;
        }


        private void RequestCloseCommandExecute(object sender, EventArgs e)
        {
            OnRequestClose(EventArgs.Empty);
        }

        private void SaveOnlineReportCommandExecute(object sender, EventArgs e)
        {
            var filePath = GetSaveAsPathFromUser();

            if (string.IsNullOrWhiteSpace(filePath)) { return; }

            _eventPublisher.Publish(new ReportSaveRequestEvent { ReportType = SaveEventEnum.OnlineReport, SavePath = filePath });
        }

        private void SaveOfflineReportCommandExecute(object sender, EventArgs e)
        {
            var filePath = GetSaveAsPathFromUser();

            if (string.IsNullOrWhiteSpace(filePath)) { return; }

            _eventPublisher.Publish(new ReportSaveRequestEvent { ReportType = SaveEventEnum.OfflineReport, SavePath = filePath });
        }

        private void SaveStaleRecordReportCommandExecute(object sender, EventArgs e)
        {
            var filePath = GetSaveAsPathFromUser();

            if (string.IsNullOrWhiteSpace(filePath)) { return; }

            _eventPublisher.Publish(new ReportSaveRequestEvent{ ReportType = SaveEventEnum.StaleRecordReport, SavePath = filePath});
        }

        private bool SaveOnlineReportCommandCanExecute() => true;

        private bool SaveOfflineReportCommandCanExecute() => true;

        private bool SaveStaleRecordReportCommandCanExecute() => true;

        private bool RequestCloseCanExecute() => true;

        private string GetSaveAsPathFromUser()
        {
            var dialog = new SaveFileDialog()  { InitialDirectory = _defaultPath, AddExtension = true, DefaultExt = ".csv", FileName = "Report.csv", Filter = "Comma Separated Value|*.csv"};
            dialog.ShowDialog();

            return dialog.FileName;
        }
    }
}