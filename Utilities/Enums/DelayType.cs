namespace ReisProduction.Windelay.Utilities.Enums;
public enum DelayType : ushort
{
    HybridDelay = 0x01,
    HighResSpin = 0x02,
    SpinDelay = 0x03,
    WaitableTimer = 0x04,
    HighResSleep = 0x05,
    SleepDelay = 0x06,
    TaskDelay = 0x07,
    TaskDelayWait = 0x08,
    EventWaitHandle = 0x09,
    TimersTimerDelay = 0x0A,
    TimerDelay = 0x0B,
    FormsTimerDelay = 0x0C
}