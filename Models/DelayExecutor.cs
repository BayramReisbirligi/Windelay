using static ReisProduction.Windelay.Services.Interop;
using ReisProduction.Windelay.Utilities.Enums;
using ReisProduction.Windelay.Utilities;
using System.Diagnostics;
namespace ReisProduction.Windelay.Models;
public static partial class DelayExecutor
{
    /// <summary>
    /// General method to handle various delay types.
    /// </summary>
    public static async Task HandleDelay(DelayAction delay)
    {
        switch (delay.DelayType)
        {
            case DelayType.HybridDelay: await HybridDelay(delay); break;
            case DelayType.HighResSpin: HighResSpin(delay); break;
            case DelayType.SpinDelay: SpinDelay(delay); break;
            case DelayType.WaitableTimer: WaitableTimer(delay); break;
            case DelayType.HighResSleep: HighResSleep(delay); break;
            case DelayType.SleepDelay: SleepDelay(delay); break;
            case DelayType.TaskDelay: await TaskDelay(delay); break;
            case DelayType.TaskDelayWait: TaskDelayWait(delay); break;
            case DelayType.EventWaitHandle: EventWaitHandle(delay); break;
            case DelayType.TimersTimerDelay: await TimersTimerDelay(delay); break;
            case DelayType.TimerDelay: await TimerDelay(delay); break;
            case DelayType.FormsTimerDelay: await FormsTimerDelay(delay); break;
        }
    }
    /// <summary>
    /// Hybrid delay method that combines Task.Delay and SpinWait for improved accuracy.
    /// Better for medium to long delays while maintaining accuracy.
    /// </summary>
    public static async Task HybridDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        var sw = Stopwatch.StartNew();
        int delayTime = delay.DelayMilisecond - Math.Min(SpinAheadMilisecond, delay.DelayMilisecond);
        if (delayTime > 0) try { await Task.Delay(delayTime, delay.Token); } catch (TaskCanceledException) { return; }
        while (sw.Elapsed.TotalMilliseconds < delay.DelayMilisecond && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
    /// <summary>
    /// High-resolution spin wait method using Stopwatch for timing.
    /// Blocks the thread but provides better accuracy for short delays.
    /// Do not use for long delays to avoid high CPU usage and block thread.
    /// </summary>
    public static void HighResSpin(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        long freq = Stopwatch.Frequency,
           target = Stopwatch.GetTimestamp() + delay.DelayMilisecond * freq / 1000;
        while (Stopwatch.GetTimestamp() < target && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
    /// <summary>
    /// System.Threading.SpinWait method for timing.
    /// Blocks the thread but is less accurate than HighResSpin.
    /// Do not use for long delays to avoid high CPU usage and block thread.
    /// </summary>
    public static void SpinDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalMilliseconds < delay.DelayMilisecond && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
    /// <summary>
    /// Waitable timer method using Windows API for timing.
    /// </summary>
    public static void WaitableTimer(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        nint handle = CreateWaitableTimer(nint.Zero, true, null);
        if (handle == nint.Zero) return;
        try
        {
            long due = -1 * delay.DelayMilisecond * 10000;
            SetWaitableTimer(handle, ref due, 0, nint.Zero, nint.Zero, false);
            _ = WaitForSingleObject(handle, (uint)delay.DelayMilisecond);
        }
        finally { CloseHandle(handle); }
    }
    /// <summary>
    /// High-resolution sleep method using timeBeginPeriod and timeEndPeriod for timing.
    /// </summary>
    public static void HighResSleep(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        _ = timeBeginPeriod(1);
        Thread.Sleep(delay.DelayMilisecond);
        _ = timeEndPeriod(1);
    }
    /// <summary>
    /// System.Threading.Sleep method using Thread.Sleep for timing.
    /// </summary>
    public static void SleepDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        Thread.Sleep(delay.DelayMilisecond);
    }
    /// <summary>
    /// System.Threading.Tasks.Task.Delay method for timing.
    /// </summary>
    public static async Task TaskDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        await Task.Delay(delay.DelayMilisecond, delay.Token);
    }
    /// <summary>
    /// System.Threading.Tasks.Task.Delay method with Wait for timing.
    /// </summary>
    public static void TaskDelayWait(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        Task.Delay(delay.DelayMilisecond, delay.Token).Wait(delay.Token);
    }
    /// <summary>
    /// EventWaitHandle method using ManualResetEventSlim for timing.
    /// </summary>
    public static void EventWaitHandle(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        using ManualResetEventSlim ev = new(false);
        ev.Wait(delay.DelayMilisecond, delay.Token);
    }
    /// <summary>
    /// Timers.Timer method for timing.
    /// </summary>
    public static async Task TimersTimerDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        TaskCompletionSource tcs = new();
        using System.Timers.Timer timer = new(delay.DelayMilisecond) { AutoReset = false };
        timer.Elapsed += (_, __) => tcs.TrySetResult();
        timer.Start();
        using (delay.Token.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }
    /// <summary>
    /// System.Threading.Timer class method for timing.
    /// </summary>
    public static async Task TimerDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        TaskCompletionSource tcs = new();
        using Timer timer = new(_ => tcs.TrySetResult(), null, delay.DelayMilisecond, Timeout.Infinite);
        using (delay.Token.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }
    /// <summary>
    /// Windows.Forms.Timer method for timing.
    /// </summary>
    public static async Task FormsTimerDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        TaskCompletionSource tcs = new();
        using System.Windows.Forms.Timer timer = new() { Interval = delay.DelayMilisecond };
        timer.Tick += (_, __) => { timer.Stop(); tcs.TrySetResult(); };
        timer.Start();
        using (delay.Token.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }
}