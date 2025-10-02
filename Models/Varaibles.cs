namespace ReisProduction.Windelay.Models;
public static partial class DelayExecutor
{
    /// <summary>
    /// SpinWait iterations per loop. Default is 200 / number of processors, clamped between 25 and 100.
    /// </summary>
    public static int SpinWaitIterations { get; set; } = Math.Clamp(200 / Environment.ProcessorCount, 25, 100);
    /// <summary>
    /// Spin ahead time in milliseconds for HybridDelay. Default is 200ms.
    /// </summary>
    public static int SpinAheadMilisecond { get; set; } = 200;
}