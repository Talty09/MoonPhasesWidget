namespace MoonPhasesWidget.Astronomy;

/// <summary>
/// The state of the moon at a moment in time. All times are UTC.
/// </summary>
/// <param name="TimeUtc">The moment this describes.</param>
/// <param name="IlluminatedFraction">Fraction of the disk that is lit, from 0 (new) to 1 (full).</param>
/// <param name="IsWaxing">True between new moon and full moon, when the lit part is growing.</param>
/// <param name="CyclePosition">Position in the lunation from 0 (new) through 0.5 (full) back toward 1.</param>
/// <param name="AgeDays">Days since the previous new moon.</param>
/// <param name="Phase">The named phase, using eight equal 45-degree sectors centered on the principal phases.</param>
public readonly record struct MoonPhaseInfo(
    DateTime TimeUtc,
    double IlluminatedFraction,
    bool IsWaxing,
    double CyclePosition,
    double AgeDays,
    MoonPhase Phase,
    DateTime PreviousNewMoonUtc,
    DateTime NextNewMoonUtc,
    DateTime NextFullMoonUtc);
