using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Linq;
using static System.Console;

namespace CommonHelpers
{
    public static class ObservableExt
   {
      public static IDisposable Trace<T>(this IObservable<T> source, string name)
         => source.Subscribe(
            onNext: val => WriteLine($"{name} -> {val}"),
            onError: ex => WriteLine($"{name} ERROR: {ex.Message}"),
            onCompleted: () => WriteLine($"{name} END"));
   }
}
