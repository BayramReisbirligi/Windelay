namespace ReisProduction.Windelay.Utilities.Enums;
public enum DelayType : ushort
{
    HybridDelay = 0x01,
    SpinDelay = 0x02,
    HighResSpin = 0x03,
    WaitableTimer = 0x04,
    HighResSleep = 0x05,
    SleepDelay = 0x06,
    TaskDelayWait = 0x07,
    EventWaitHandle = 0x08,
    TaskDelay = 0x09,
    TimersTimerDelay = 0x0A,
    TimerDelay = 0x0B,
    FormsTimerDelay = 0x0C
}