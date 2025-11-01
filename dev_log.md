Over the last few updates we have been making an effort to reduce the number and size of memory allocations per frame. This is in order to reduce the amount of work the garbage collector does and smooth out the framerate. Our eventual goal is to be able to turn on Incremental GC in Unity to smooth out the framerate even more, but right now that adds significant overhead. (It’s much better than a year ago, but there’s still more work to do before we can turn it on.)

In this update, we managed to reduce allocations in late-game bases by 40-60% by reworking core systems such as EventSystem, GameScenePartitioner, and CellChangeMonitor. There’s no silver bullet, just a lot of things to optimize, and we have rebuilt our profiler tool to tackle this problem. This post details some of the event system changes and how they might affect your mods.

EventSystem

The most important change to mention here is that many events have changed the parameter type being passed, especially in the case of value types (int, bool, float etc.) If you are casting and using the object parameter, it might need to be updated to using Boxed. This is detailed in the second example below.

Secondly, Event Handlers are being migrated away from capturing context-related information in closure objects (in C# terms delegates/Actions,Funcs) to instead allow registration of separate context objects. Capturing of methods is being shifted to a static closure where possible. Not everything in our codebase has been converted yet.

Thirdly, subscriptions now return a handle which can be used for Unsubscribing to an event, rather than providing a closure to compare to the closure that was registered. This is recommended, as calling Unsubscribe with a handle is now significantly faster than it was before.

Where possible, old methods are being provided to supply backwards compatibility. However, they will be marked obsolete once the migration is complete. It is highly recommended that you turn on warnings as errors, so that these obsolete methods are highlighted in the console. Often we’ll include a description with the Obsolete attribute explaining what to use instead.

Consider the following example:

// Where before
class MyClass {
   int instanceSpecificData;

   void OnEvent(object obj) {
      ++instanceSpecificData;
   }
   void StartListening() {
      Subscribe((int)GameHashes.Something, OnEvent);
   }
   void StopListening() {
    Unsubscribe((int)GameHashes.Something, OnEvent);
   }
}

// Now preferable:
class MyClass {
   int instanceSpecificData;
   int onEventHandle = 0;
   static Action<object, object> OnEventDispatcher = (data, context) => ((MyClass)context).OnEvent(data);

   void OnEvent(object obj) {
      ++instanceSpecificData;
   }
   void StartListening() {
         onEventHandle = Subscribe((int)GameHashes.Something, OnEventDispatcher, this);
   }
   void StopListening() {
    Unsubscribe(ref onEventHandle);
   }
}

While verbose, this change removes at least two Action<object> objects going to garbage collection per instance of MyClass.

To reduce boxing related objects going to garbage collection, pooled boxing objects have been created in the form of the generic type Boxed<T>. Overrides of BoxingTrigger (a version of Trigger specific to value types) have been provided to make use of them. 

Here is an example of how to utilize this:

// Where Before:
void DoTheThing() {
    int whatThing = 42;
    Trigger((int)GameHashes.ThingHappened, whatThing);
}

void WhenTheThing(object data) {
     int whatThing = (int)data;
     // ...
}

// Now:
void DoTheThing() {
    int whatThing = 42;
    BoxingTrigger((int)GameHashes.ThingHappened, whatThing);
}

void WhenTheThing(object data) {
     int whatThing = Boxed<int>.Unbox(data);
     // ...
}

This avoids the wrapper used to pass the value-type (int) as a reference type (object) from going to the garbage collector.

Specializations of Trigger(int, T) where T : struct have been added and marked obsolete to help identify any unintended boxing of event data. 

These are big changes and likely to disrupt mods; please take the time to investigate and adjust as needed. Our efforts to keep reducing the number and size of memory allocations per frame are ongoing, but we'll keep everyone updated as things progress. 