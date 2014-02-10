using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Events
{
    public class DomainEvent
    {
        public string Message;
    }

    public class OtherDomainEvent : DomainEvent
    {
        public int Number;
    }

    public interface IEventListener
    {
        void Register(IEventAggregator eventAggregator);
    }

    public interface IEventListener<in T> : IEventListener where T : DomainEvent
    {
        void OnEvent(T domainEvent);
    }

    public interface IEventAggregator
    {
        void Raise<T>(T e) where T : DomainEvent;
        void Subscribe<T>(IEventListener<T> listener) where T : DomainEvent;
    }

    public class EventAggregator : IEventAggregator
    {
        public void Raise<T>(T domainEvent) where T : DomainEvent
        {
            if (domainEvent == null) throw new ArgumentNullException("event");

            foreach (var listener in EventListenerStore<T>.Listeners)
            {
                listener.OnEvent(domainEvent);
            }
        }

        public void Subscribe<T>(IEventListener<T> listener) where T : DomainEvent
        {
            if (listener == null) throw new ArgumentNullException("listener");

            EventListenerStore<T>.Listeners.Add(listener);
        }

        private static class EventListenerStore<T> where T : DomainEvent
        {
            private static readonly Lazy<ConcurrentBag<IEventListener<T>>> _listeners = new Lazy<ConcurrentBag<IEventListener<T>>>();

            public static ConcurrentBag<IEventListener<T>> Listeners
            {
                get { return _listeners.Value; }
            }
        }
    }

    public class SomeObject : IEventListener<DomainEvent>
    {
        IEventAggregator _eventAggregator; 

        public SomeObject(IEventAggregator aggregator)
        {
            _eventAggregator = aggregator;
            Register(_eventAggregator);
        }

        public void OnEvent(DomainEvent domainEvent)
        {
            Console.WriteLine(string.Format("SomeObject received: {0}", domainEvent.Message));
        }

        public void Register(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }
    }

    public class SomeOtherObject : IEventListener<OtherDomainEvent>
    {
        IEventAggregator _eventAggregator;

        public SomeOtherObject(IEventAggregator aggregator)
        {
            _eventAggregator = aggregator;
            Register(_eventAggregator);
        }

        public void OnEvent(OtherDomainEvent domainEvent)
        {
            Console.WriteLine(string.Format("SomeOtherObject received: {0}, {1}", domainEvent.Message, domainEvent.Number));
        }

        public void Register(IEventAggregator eventAggregator)
        {
            eventAggregator.Subscribe(this);
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            var aggregator = new EventAggregator();
            
            Console.WriteLine("Adding someObj subscriber");
            var someObj = new SomeObject(aggregator);

            aggregator.Raise(new DomainEvent { Message = "event1" });
            aggregator.Raise(new OtherDomainEvent { Message = "other event1", Number = 1 });
            
            Console.WriteLine("Adding someOtherObj subscriber");
            var someOtherObj = new SomeOtherObject(aggregator);

            aggregator.Raise(new DomainEvent { Message = "event2" });
            aggregator.Raise(new OtherDomainEvent { Message = "other event2", Number = 2 });

            Console.ReadLine();
        }
    }
}
