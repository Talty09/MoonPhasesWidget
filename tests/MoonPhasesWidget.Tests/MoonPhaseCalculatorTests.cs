using MoonPhasesWidget.Astronomy;

namespace MoonPhasesWidget.Tests;

public class MoonPhaseCalculatorTests
{
    private static readonly TimeSpan EventTolerance = TimeSpan.FromMinutes(3);

    private static DateTime Utc(int year, int month, int day, int hour, int minute) =>
        new(year, month, day, hour, minute, 0, DateTimeKind.Utc);

    [Fact]
    public void PhaseJde_MatchesMeeusExample49a()
    {
        // New moon of 1977 February, k = -283: JDE 2443192.65118.
        double jde = MoonPhaseCalculator.PhaseJde(-283, isFullMoon: false);

        Assert.Equal(2443192.65118, jde, 0.00001);
    }

    [Fact]
    public void IlluminatedFraction_MatchesMeeusExample48a()
    {
        // 1992 April 12, 0h: Meeus gives k = 0.6786 (0.68 from the low-precision formula).
        var info = MoonPhaseCalculator.Calculate(Utc(1992, 4, 12, 0, 0));

        Assert.Equal(0.679, info.IlluminatedFraction, 0.005);
        Assert.True(info.IsWaxing);
    }

    // Published new moon times (UTC).
    [Theory]
    [InlineData(2024, 1, 11, 11, 57)]
    [InlineData(2024, 4, 8, 18, 21)]
    [InlineData(2025, 1, 29, 12, 36)]
    public void NextNewMoon_MatchesPublishedTimes(int year, int month, int day, int hour, int minute)
    {
        DateTime expected = Utc(year, month, day, hour, minute);

        var info = MoonPhaseCalculator.Calculate(expected.AddDays(-3));

        Assert.Equal(expected, info.NextNewMoonUtc, EventTolerance);
    }

    // Published full moon times (UTC).
    [Theory]
    [InlineData(2024, 1, 25, 17, 54)]
    [InlineData(2024, 4, 23, 23, 49)]
    [InlineData(2025, 1, 13, 22, 27)]
    public void NextFullMoon_MatchesPublishedTimes(int year, int month, int day, int hour, int minute)
    {
        DateTime expected = Utc(year, month, day, hour, minute);

        var info = MoonPhaseCalculator.Calculate(expected.AddDays(-3));

        Assert.Equal(expected, info.NextFullMoonUtc, EventTolerance);
    }

    [Fact]
    public void AtNewMoon_IsDarkAndJustStarting()
    {
        DateTime newMoon = Utc(2024, 1, 11, 11, 57);

        var info = MoonPhaseCalculator.Calculate(newMoon.AddMinutes(10));

        Assert.Equal(MoonPhase.NewMoon, info.Phase);
        Assert.True(info.IsWaxing);
        Assert.True(info.IlluminatedFraction < 0.01);
        Assert.Equal(newMoon, info.PreviousNewMoonUtc, EventTolerance);
        Assert.InRange(info.AgeDays, 0, 0.05);
    }

    [Fact]
    public void AtFullMoon_IsFullyLit()
    {
        var info = MoonPhaseCalculator.Calculate(Utc(2024, 1, 25, 17, 54));

        Assert.Equal(MoonPhase.FullMoon, info.Phase);
        Assert.True(info.IlluminatedFraction > 0.99);
        Assert.InRange(info.CyclePosition, 0.48, 0.52);
    }

    [Theory]
    [InlineData(3, MoonPhase.WaxingCrescent, true)]   // ~Jan 14
    [InlineData(7.4, MoonPhase.FirstQuarter, true)]   // ~Jan 18
    [InlineData(11, MoonPhase.WaxingGibbous, true)]   // ~Jan 22
    [InlineData(18, MoonPhase.WaningGibbous, false)]  // ~Jan 29
    [InlineData(22.1, MoonPhase.LastQuarter, false)]  // ~Feb 2
    [InlineData(26, MoonPhase.WaningCrescent, false)] // ~Feb 6
    public void PhaseNames_FollowTheCycle(double daysAfterNewMoon, MoonPhase expected, bool waxing)
    {
        var info = MoonPhaseCalculator.Calculate(Utc(2024, 1, 11, 11, 57).AddDays(daysAfterNewMoon));

        Assert.Equal(expected, info.Phase);
        Assert.Equal(waxing, info.IsWaxing);
    }

    [Fact]
    public void LocalTime_IsConvertedToUtc()
    {
        DateTime utc = Utc(2025, 6, 1, 12, 0);

        var fromLocal = MoonPhaseCalculator.Calculate(utc.ToLocalTime());
        var fromUtc = MoonPhaseCalculator.Calculate(utc);

        Assert.Equal(fromUtc, fromLocal);
    }

    [Fact]
    public void EventsBracketTheRequestedTime_AcrossManyYears()
    {
        for (var time = Utc(1990, 1, 1, 0, 0); time < Utc(2060, 1, 1, 0, 0); time = time.AddHours(37))
        {
            var info = MoonPhaseCalculator.Calculate(time);

            Assert.True(info.PreviousNewMoonUtc <= time, $"previous new moon after {time:O}");
            Assert.True(info.NextNewMoonUtc > time, $"next new moon not after {time:O}");
            Assert.True(info.NextFullMoonUtc > time, $"next full moon not after {time:O}");
            Assert.InRange(info.AgeDays, 0, 30);
            Assert.InRange(info.IlluminatedFraction, 0, 1);
        }
    }
}
