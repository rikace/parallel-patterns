namespace CSharp.Parallelx.EventEx
{
    using System;
    using System.Collections.Generic;
    using System.Reactive.Subjects;
    
    public sealed class MapperSubject<Tin, Tout> : ISubject<Tin, Tout>
    {
        readonly Func<Tin, Tout> mapper;
        public MapperSubject(Func<Tin, Tout> mapper)
        {
            this.mapper = mapper;
        }

        public void OnCompleted()
        {
            foreach (var o in observers.ToArray())
            {
                o.OnCompleted();
                observers.Remove(o);
            }
        }

        public void OnError(Exception error)
        {
            foreach (var o in observers.ToArray())
            {
                o.OnError(error);
                observers.Remove(o);
            }
        }

        public void OnNext(Tin value)
        {
            Tout newValue = default(Tout);
            try
            {
                //mapping statement
                newValue = mapper(value);
            }
            catch (Exception ex)
            {
                //if mapping crashed
                OnError(ex);
                return;
            }

            //if mapping succeeded 
            foreach (var o in observers)
                o.OnNext(newValue);
        }

        //all registered observers
        private readonly List<IObserver<Tout>> observers = new List<IObserver<Tout>>();
        public IDisposable Subscribe(IObserver<Tout> observer)
        {
            observers.Add(observer);
            return new ObserverHandler<Tout>(observer, OnObserverLifecycleEnd);
        }

        private void OnObserverLifecycleEnd(IObserver<Tout> o)
        {
            o.OnCompleted();
            observers.Remove(o);
        }

        //this class simply informs the subject that a dispose
        //has been invoked against the observer causing its removal
        //from the observer collection of the subject
        private class ObserverHandler<T> : IDisposable
        {
            private IObserver<T> observer;
            Action<IObserver<T>> onObserverLifecycleEnd;
            public ObserverHandler(IObserver<T> observer, Action<IObserver<T>> onObserverLifecycleEnd)
            {
                this.observer = observer;
                this.onObserverLifecycleEnd = onObserverLifecycleEnd;
            }

            public void Dispose()
            {
                onObserverLifecycleEnd(observer);
            }
        }
    }
}