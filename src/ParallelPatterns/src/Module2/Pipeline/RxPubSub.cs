using System;
using System.Collections.Generic;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace RxPublisherSubscriber
{
    public class RxPubSub<T> : IDisposable
    {
        private ISubject<T> subject; 
        private readonly List<IObserver<T>> observers = new List<IObserver<T>>();  
        private readonly List<IDisposable> observables = new List<IDisposable>();  

        public RxPubSub(ISubject<T> subject)
        {
            this.subject = subject; 
        }
        public RxPubSub() : this(new Subject<T>()) { }  

        public IDisposable Subscribe(IObserver<T> observer)
        {
            observers.Add(observer);
            observables.Add(this.subject.Subscribe(observer));
            return new ObserverHandler<T>(observer, observers); 
        }

        public IDisposable AddPublisher(IObservable<T> observable) =>
            observable.SubscribeOn(TaskPoolScheduler.Default).Subscribe(subject); 

        public IObservable<T> AsObservable() => subject.AsObservable(); 
        public void Dispose()
        {
            subject.OnCompleted();
            observers.ForEach(x => x.OnCompleted());
            observers.Clear();  
        }
    }

    class ObserverHandler<T> : IDisposable  
    {
        private readonly IObserver<T> observer;
        private readonly List<IObserver<T>> observers;

        public ObserverHandler(IObserver<T> observer, List<IObserver<T>> observers)
        {
            this.observer = observer;
            this.observers = observers;
        }

        public void Dispose()  
        {
            observer.OnCompleted();
            observers.Remove(observer);
        }
    }
}