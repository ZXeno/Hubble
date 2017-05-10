using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.ViewModel
{
    public class ComputerListViewModel : RequestCloseViewModel
    {
        private string _textBoxContents;
        public string TextBoxContents
        {
            get
            {
                return _textBoxContents;
            }
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

        public ComputerListViewModel(List<string> devList)
        {
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

        public void DoneExecute(object sender, EventArgs e)
        {
            var devList = new List<string>(TextBoxContents.Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries));
            var resultList = new List<string>();

            foreach (var d in devList)
            {
                var t = d;

                t = new string(t.ToCharArray().Where(c => !char.IsWhiteSpace(c)).ToArray());

                resultList.Add(t);
            }

            App.EventAggregator.Publish(new DeviceListUpdateEvent { DeviceList = resultList });

            OnRequestClose(EventArgs.Empty);
        }

        private bool DoneCanExecute()
        {
            return true;
        }
    }
}