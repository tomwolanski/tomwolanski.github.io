---
title: "Slow and silent - an async stack overflow"
date: 2025-02-05
cover : "cover.png"
image : "cover.png"
useRelativeCover : true
draft: false
hideToc : false
tags:
  - async
summary: "An interesting behavior of recursive async functions. Synchronous stack overflow is well known, but what happens in async world?"

---

## Stack Overflow

Almost everybody knows what stack overflow is, un recoverable crash of the program caused by invoking too many nested methods.

Most of the time we do not need to think about the stack, especially in managed word, where it is almost impossible to corrupt the memory. We do not need to go deep into the stack and heap relation, let assume it is an [implementation detail](https://learn.microsoft.com/en-us/archive/blogs/ericlippert/the-stack-is-an-implementation-detail-part-one), that works for us, not against. 

Until we reach its limits. 

Recently I was looking thru active PRs and stumbled upon a piece of code that made me think. I would never write it, but it was there, waiting to be merged to the production code. 
It was a background service, which would periodically (every 30 minutes) execute some work. It was using [Cronos](https://github.com/HangfireIO/Cronos) to await certain time, when the work should be done. It looks almost like this:

```csharp
internal class PeriodicJobBackgroundWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunTimedJobWithCron(stoppingToken);
    }

    async Task RunTimedJobWithCron(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var nextOccurrence = _cronJobSchedule.GetNextOccurrence(now, _previousOccurrence);
        if (nextOccurrence.HasValue)
        {
            await Task.Delay(nextOccurrence.Value.Delay, cancellationToken);
            _previousOccurrence = nextOccurrence;
            await DoCoreWorkAsync(cancellationToken);
        }

        if (!cancellationToken.IsCancellationRequested)
            await RunTimedJobWithCron(cancellationToken);
    }
}
```

I immediately wrote a comment, that the code will crash immediately after the startup, but the author claimed it works well. I had to test it.
It did not crash the way I expected. I jumped into the rabbit hole.

We will be reviewing three scenarios of recursive method without a base case.
- synchronous functions
- asynchronous functions that return immediately
- asynchronous functions that perform real awaiting

## Synchronous methods - fast and ugly

First, lets consider the basic example we are all familiar with. A recursive, synchronous function with no base case. 

```csharp
internal static class Program
{
    static void Main(string[] args)
    {
        DoStuff();
    }

    public static void DoStuff()
    {
        DoStuff();
    }
}
```

There is no surprise here, the application crashed and was not able to recover:
```
Stack overflow.
Repeated 32130 times:
--------------------------------
   at Recursive.Program.DoStuff()
--------------------------------
   at Recursive.Program.Main(System.String[])
```

## Asynchronous methods 

Our original code was a long running service, that interacted with several remote systems like databases. Writing an IO code as an async code is now a standard, so let's check if there are any surprises.

### Immediate return - no difference here

Our simple example, changed into asynchronous functions may look like this:
```csharp
internal static class Program
{
    static async Task Main(string[] args)
    {
        await DoStuffAsync(default);
    }

    public static async Task DoStuffAsync(CancellationToken cancellationToken)
    {
        if (!cancellationToken.IsCancellationRequested)
        {
            await DoStuffAsync(cancellationToken);
        }       
    }
}
```

As previously, the application crashed due to overflowing the stack. We expected this, especially after the synchronous test.
```
Stack overflow.
Repeated 4191 times:
--------------------------------
   at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[[RecursiveAsync.Program+<DoStuffAsync>d__1, RecursiveAsync, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]](<DoStuffAsync>d__1 ByRef)
   at RecursiveAsync.Program.DoStuffAsync(System.Threading.CancellationToken)
   at RecursiveAsync.Program+<DoStuffAsync>d__1.MoveNext()
   at System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start[[RecursiveAsync.Program+<DoStuffAsync>d__1, RecursiveAsync, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]](<DoStuffAsync>d__1 ByRef)
--------------------------------
   at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[[RecursiveAsync.Program+<DoStuffAsync>d__1, RecursiveAsync, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]](<DoStuffAsync>d__1 ByRef)
   at RecursiveAsync.Program.DoStuffAsync(System.Threading.CancellationToken)
   at RecursiveAsync.Program+<Main>d__0.MoveNext()
   at System.Runtime.CompilerServices.AsyncMethodBuilderCore.Start[[RecursiveAsync.Program+<Main>d__0, RecursiveAsync, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]](<Main>d__0 ByRef)
   at System.Runtime.CompilerServices.AsyncTaskMethodBuilder.Start[[RecursiveAsync.Program+<Main>d__0, RecursiveAsync, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null]](<Main>d__0 ByRef)
   at RecursiveAsync.Program.Main(System.String[])
   at RecursiveAsync.Program.<Main>(System.String[])
```

One may notice that our method does not do any real work. In the real service, we'd expect, database to be called at some point.

### Real async call - wait, what?

As stated before, our dummy method does not perform any actual work. Because it does not await any "real" asynchronous function, it could end synchronously. Thanks to this "hot path" optimization this scenario has extremely low memory usage, since nothing is boxed.

As described in [Dissecting the async methods in C#](https://devblogs.microsoft.com/premier-developer/dissecting-the-async-methods-in-c/) the async state machine states on the stack. Foreshadowing?

Let's force our code to not allow for this optimization. This can be done by calling any IO-bound operation like reading a file or making a web request, or using any of the methods provided by `System.Threading.Tasks.Task`. 

To save time when executing our example, I will be using [`Task.Yield()`](https://learn.microsoft.com/en-us/dotnet/api/system.threading.tasks.task.yield?view=net-9.0) method to force asynchronous execution of `DoStuffAsync`.

```csharp
internal static class Program
{
    static async Task Main(string[] args)
    {
        await DoStuffAsync(default);
    }

    public static async Task DoStuffAsync(CancellationToken cancellationToken)
    {
        await Task.Yield(); // forces the method to run asynchronously, simulates an async IO call

        if (!cancellationToken.IsCancellationRequested)
        {
            await DoStuffAsync(cancellationToken);
        }       
    }
}
```

The application does not crash! It seems like we solved the problem of limited stack trace. C# does not allow for tail-call optimization like F# does, so it must me something different.

The application works fine, but it slowly consumes almost all available resources.

![Increased memory consumption](async_consumption.png)

Few seconds of execution consumed almost 6BG or memory. Did the GC misbehaved? We can easly enforce periodic collections, just to be extra sure.
```csharp
internal static class Program
{
    static async Task Main(string[] args)
    {
        using var _ = new Timer(_ =>
        {
            GC.Collect();
            GC.WaitForPendingFinalizers();

            Console.WriteLine($"used: {GC.GetTotalAllocatedBytes()} g0:{GC.CollectionCount(0)}, g1:{GC.CollectionCount(1)}, g2:{GC.CollectionCount(2)}");
        },
        default,
        TimeSpan.FromSeconds(3),
        TimeSpan.FromSeconds(3));

        await DoStuffAsync(CancellationToken.None);
    }

    public static async Task DoStuffAsync(CancellationToken cancellationToken)
    {
        await Task.Yield();

        if (!cancellationToken.IsCancellationRequested)
        {
            await DoStuffAsync(cancellationToken);
        }       
    }
}
```

For good measure, our code outputs number of garbage collection for each of the generations. I stopped the application when allocated memory reached almost 7GB. As we can see, the garbage collector was working extra hours:
```
used: 2136637120 g0:266, g1:265, g2:9
used: 3729335624 g0:458, g1:456, g2:10
used: 5148979816 g0:629, g1:626, g2:11
used: 6332684352 g0:772, g1:769, g2:12
used: 7505105352 g0:914, g1:910, g2:13
```

Memory profiler allows us to see what exactly happened:
![memory profiler showing difference of two snapshots](memory_profiler.png)

By forcing the `DoStuffAsync` to finished asynchronously, the async state machine could no longer remain on the stack, had to be boxed into `syncTaskMethodBuilder+AsyncStateMachineBox<...>` type and moved to the heap. Unlike to stack, the heap can grow to accommodate all our boxed instances. 
The boxed stack frames could not be garbage-collected, since the application expected that we will eventually reach a base-case and go back towards to `Main` function. This never happens, so all of them will remain unused on the heap.

The problem was not solved, it was just moved from stack to heap.

## Summary

While, I think this behavior is interesting, and could be useful for quick and dirty implementation of stack-heavy depth-first search algorithms, there are always different and cleaner options, better suited for production code.

The real code is way slower than our exaggerated example application. The `Task.Delay(...)` will slow things down, the code was expected to execute few times a day. At this rate, leaking few extra kilobytes would not cause immediate harm, but will cause memory issues in a longer run. 

The most dangerous aspect of this, that it could be undetected on lower environments, where deployments and restarts are performed often, and would crash on production environment, where the service is supposed to run for longer time uninterrupted.

While in other languages like F# (tail-call optimization) or JavaScript (async based on message pump) would work without any issue, this is not the case in C#. Maybe it is better to be safe and not count on frequent service reboots to clean up the memory and do things in safer, more classical way, with a loop:

```csharp
internal class PeriodicJobBackgroundWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        await RunTimedJobWithCron(stoppingToken);
    }

    async Task RunTimedJobWithCron(CancellationToken cancellationToken)
    {
        while(!cancellationToken.IsCancellationRequested)
        {
            var now = DateTime.UtcNow;
            var nextOccurrence = _cronJobSchedule.GetNextOccurrence(now, _previousOccurrence);

            await Task.Delay(nextOccurrence.Value.Delay, cancellationToken);
            _previousOccurrence = nextOccurrence;
            await DoCoreWorkAsync(cancellationToken);
        }
    }
}
```

Or, if we know the operation will be always performed in constant intervals (as in our example), we can utilize more recent addition to the BCL: `System.Threading.Tasks.PeriodicTimer`. However, we will lose the versatility of Cron expressions.
```csharp
internal class PeriodicJobBackgroundWorker : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        TimeSpan period = GetPeriodFromConfig();

        using var timer = new PeriodicTimer(period);

        while(await timer.WaitForNextTickAsync(cancellationToken))
        {
            await DoCoreWorkAsync(cancellationToken);
        }
    }
}





