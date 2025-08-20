using static ReisProduction.Windelay.Services.Interop;
using ReisProduction.Windelay.Utilities;
using System.Diagnostics;
namespace ReisProduction.Windelay.Models;
public static class DelayExecutor
{
    public static int SpinWaitIterations { get; set; } = Math.Clamp(200 / Environment.ProcessorCount, 25, 100);
    public static int SpinAheadMilisecond { get; set; } = 0;
    public static async Task HybridDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        var sw = Stopwatch.StartNew();
        int delayTime = delay.DelayMilisecond - Math.Min(SpinAheadMilisecond, delay.DelayMilisecond);
        if (delayTime > 0) try { await Task.Delay(delayTime, delay.Token); } catch (TaskCanceledException) { return; }
        while (sw.Elapsed.TotalMilliseconds < delay.DelayMilisecond && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
    public static void SpinDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        var sw = Stopwatch.StartNew();
        while (sw.Elapsed.TotalMilliseconds < delay.DelayMilisecond && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
    public static void HighResSpin(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        long freq = Stopwatch.Frequency,
           target = Stopwatch.GetTimestamp() + delay.DelayMilisecond * freq / 1000;
        while (Stopwatch.GetTimestamp() < target && !delay.Token.IsCancellationRequested)
            Thread.SpinWait(SpinWaitIterations);
    }
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
    public static void HighResSleep(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        _ = timeBeginPeriod(1);
        Thread.Sleep(delay.DelayMilisecond);
        _ = timeEndPeriod(1);
    }
    public static void SleepDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        Thread.Sleep(delay.DelayMilisecond);
    }
    public static void TaskDelayWait(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        Task.Delay(delay.DelayMilisecond, delay.Token).Wait(delay.Token);
    }
    public static void EventWaitHandleDelay(DelayAction delaya)
    {
        if (delaya.DelayMilisecond <= 0) return;
        using ManualResetEventSlim ev = new(false);
        ev.Wait(delaya.DelayMilisecond, delaya.Token);
    }
    public static async Task TaskDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        await Task.Delay(delay.DelayMilisecond, delay.Token);
    }
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
    public static async Task TimerDelay(DelayAction delay)
    {
        if (delay.DelayMilisecond <= 0) return;
        TaskCompletionSource tcs = new();
        using Timer timer = new(_ => tcs.TrySetResult(), null, delay.DelayMilisecond, Timeout.Infinite);
        using (delay.Token.Register(() => tcs.TrySetCanceled()))
            await tcs.Task;
    }
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