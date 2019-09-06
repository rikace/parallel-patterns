using System;
using System.Collections.Generic;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace CSharp.Parallelx.EventEx
{  
    public interface ISubscriptionService
    {
        IDisposable Subscribe<TMessage>(Predicate<TMessage> canHandle, Action<TMessage> handle);
        void Publish<TMessage>(TMessage item);
    }

    public sealed class SubscriptionService : ISubscriptionService
    {
        readonly ISubject<object> _subject;
        readonly List<IDisposable> _publisherSubscriptions;

        public SubscriptionService()
        {
            _subject = new Subject<Object>();
            _publisherSubscriptions = new List<IDisposable>();
        }

        // Subscribes the given handler to the message bus. Only messages for which the given predicate resolves to true will be passed to the handler.
        public IDisposable Subscribe<TMessage>(Predicate<TMessage> canHandle, Action<TMessage> handle)
        {
            if (canHandle == null) throw new ArgumentNullException("canHandle");
            if (handle == null) throw new ArgumentNullException("handle");

            var resource =
                _subject.AsObservable().OfType<TMessage>().Where(msg => canHandle(msg)).Subscribe(handle);

            _publisherSubscriptions.Add(resource);
            return new Unsubsribe(resource, _publisherSubscriptions);
        }

        public void Publish<TMessage>(TMessage item)
        {
            _subject.OnNext(item);
        }

        private class Unsubsribe : IDisposable
        {
            readonly IDisposable _resource;
            readonly List<IDisposable> _subscriptions;
            public Unsubsribe(IDisposable resource, List<IDisposable> subscriptions)
            {
                this._resource = resource;
                this._subscriptions = subscriptions;
            }

            public void Dispose()
            {
                _subscriptions.Remove(_resource);
                _resource.Dispose();
            }
        }
        bool _disposed;

        public void Dispose()
        {
            if (!_disposed)
            {
                _publisherSubscriptions.ForEach(d => d.Dispose());
            }
            _disposed = true;
        }
    }

}