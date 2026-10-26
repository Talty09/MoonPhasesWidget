namespace MoonPhasesWidget.Astronomy;

public enum MoonPhase
{
    NewMoon,
    WaxingCrescent,
    FirstQuarter,
    WaxingGibbous,
    FullMoon,
    WaningGibbous,
    LastQuarter,
    WaningCrescent,
}

public static class MoonPhaseExtensions
{
    public static string ToDisplayName(this MoonPhase phase) => phase switch
    {
        MoonPhase.NewMoon => "New Moon",
        MoonPhase.WaxingCrescent => "Waxing Crescent",
        MoonPhase.FirstQuarter => "First Quarter",
        MoonPhase.WaxingGibbous => "Waxing Gibbous",
        MoonPhase.FullMoon => "Full Moon",
        MoonPhase.WaningGibbous => "Waning Gibbous",
        MoonPhase.LastQuarter => "Last Quarter",
        MoonPhase.WaningCrescent => "Waning Crescent",
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, null),
    };
}
