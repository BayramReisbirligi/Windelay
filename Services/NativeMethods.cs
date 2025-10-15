using System.Runtime.InteropServices;
namespace ReisProduction.Windelay.Services;
internal static class NativeMethods
{
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool CloseHandle(nint hObject);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern nint CreateWaitableTimer(nint lpTimerAttributes, bool bManualReset, string? lpTimerName);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern bool SetWaitableTimer(nint hTimer, ref long pDueTime, int lPeriod, nint pfnCompletionRoutine, nint lpArgToCompletionRoutine, bool fResume);
    [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern uint WaitForSingleObject(nint hHandle, uint dwMilliseconds);
    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern uint timeBeginPeriod(uint uMilliseconds);
    [DllImport("winmm.dll", SetLastError = true, CharSet = CharSet.Unicode)]
    internal static extern uint timeEndPeriod(uint uMilliseconds);
}