using ReisProduction.Windelay.Utilities.Enums;
namespace ReisProduction.Windelay.Utilities;
public record DelayAction(
    int DelayMilisecond,
    CancellationToken Token,
    DelayType DelayType = DelayType.HybridDelay
) : IDelayAction;