using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.ViewModel
{
    public class ComputerListViewModel : RequestCloseViewModel
    {
        #region Properties

        private string _textBoxContents;
        public string TextBoxContents
        {
            get => _textBoxContents;
            set
            {
                _textBoxContents = value;
                OnPropertyChanged("TextBoxContents");
            }
        }

        private ICommand _doneCommand;
        public ICommand DoneCommand
        {
            get
            {
                if (_doneCommand == null)
                {
                    DoneCommand = new DelegateCommand(param => DoneExecute(this, null), param => DoneCanExecute());
                }
                return _doneCommand;
            }
            set
            {
                _doneCommand = value;
                OnPropertyChanged("DoneCommand");
            }
        }

        #endregion

        #region Dependencies

        private readonly IEventPublisher _eventPublisher;

        #endregion

        #region Constructor

        public ComputerListViewModel(IEventPublisher publisher, List<string> devList)
        {
            _eventPublisher = publisher;
            var sb = new StringBuilder();

            if (devList.Count > 0)
            {
                foreach (var dev in devList)
                {
                    sb.AppendLine(dev);
                }
            }

            TextBoxContents = sb.ToString();
        }

        #endregion

        #region Commands

        public void DoneExecute(object sender, EventArgs e)
        {
            var devList = new List<string>(TextBoxContents.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            

            _eventPublisher.Publish(new DeviceListUpdateEvent { DeviceList = devList });

            OnRequestClose(EventArgs.Empty);
        }

        private bool DoneCanExecute()
        {
            return true;
        }

        #endregion
    }
}