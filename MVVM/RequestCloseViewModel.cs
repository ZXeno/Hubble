using System;

namespace DeviceMonitor.MVVM
{
    public class RequestCloseViewModel : ViewModelBase, IRequestCloseViewModel
    {
        public event EventHandler RequestClose;
        protected void OnRequestClose(EventArgs e)
        {
            RequestClose?.Invoke(this, e);
        }

        protected override void OnDispose()
        {
            var listeners = RequestClose?.GetInvocationList();
            if (listeners?.Length > 0)
            {
                foreach (var listener in listeners)
                {
                    RequestClose -= (listener as EventHandler);
                }
            }

            RequestClose = null;

            base.OnDispose();
        }
    }
}