﻿using System;
using System.Windows;
using System.Windows.Input;
using DeviceMonitor.Infrastructure;
using DeviceMonitor.Infrastructure.Events;
using DeviceMonitor.MVVM;

namespace DeviceMonitor.ViewModel
{
    public class EditTagControlViewModel : ViewModelBase
    {
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

        private Visibility _visibility;
        public Visibility Visibility{
            get => _visibility;
            set
            {
                _visibility = value;
                OnPropertyChanged("Visibility");
            }
        }

        private ICommand _doneCommand;
        public ICommand DoneCommand
        {
            get
            {
                if (_doneCommand == null)
                {
                    DoneCommand = new DelegateCommand(param => DoneCommandExecute(this, null), param => DoneCommandCanExecute());
                }
                return _doneCommand;
            }
            set
            {
                _doneCommand = value;
                OnPropertyChanged("DoneCommand");
            }
        }

        private bool _isOpen;
        public bool IsOpen
        {
            get => _isOpen;
            set
            {
                _isOpen = value;
                OnPropertyChanged("IsOpen");
            }
        }

        private string _currentDeviceName;
        private string _statusRecordGuid;

        private readonly IEventPublisher _publisher;

        public EditTagControlViewModel(IEventPublisher publisher)
        {
            _publisher = publisher;

            _publisher.GetEvent<UpdateTagEvent>().Subscribe(OpenControl);
        }

        private void OpenControl(UpdateTagEvent tagEvent)
        {
            if (!tagEvent.OpenPopup) return;

            TextBoxContents = tagEvent.NewTag;
            IsOpen = true;
            _currentDeviceName = tagEvent.Device;
            _statusRecordGuid = tagEvent.StatusRecordGuid;
        }

        private void DoneCommandExecute(object sender, EventArgs e)
        {
            IsOpen = false;
            _publisher.Publish(new UpdateTagEvent {StatusRecordGuid = _statusRecordGuid, Device = _currentDeviceName, NewTag = TextBoxContents, OpenPopup = false});
            TextBoxContents = "";
            _currentDeviceName = "";
        }

        private bool DoneCommandCanExecute() => true;
    }
}