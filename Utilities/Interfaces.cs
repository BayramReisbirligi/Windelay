namespace ReisProduction.Windelay.Utilities;
public interface IDelayAction
{
    int DelayMilisecond { get; }
    CancellationToken Token { get; }
    DelayType DelayType { get; }
}