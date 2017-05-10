using System;

namespace DeviceMonitor.Infrastructure.Events
{
    public interface IEventPublisher
    {
        void Publish<TEvent>(TEvent sampleEvent);
        IObservable<TEvent> GetEvent<TEvent>();
    }
}