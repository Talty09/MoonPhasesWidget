namespace MoonPhasesWidget.Astronomy;

/// <summary>
/// Moon phase calculations from Jean Meeus, "Astronomical Algorithms" (2nd ed.):
/// chapter 49 for the times of new and full moon (accurate to about a minute) and
/// chapter 48's low-precision formula for the illuminated fraction.
/// </summary>
public static class MoonPhaseCalculator
{
    private const double SynodicMonth = 29.530588861;
    private const double FirstNewMoonOf2000Jde = 2451550.09766; // lunation k = 0

    /// <summary>
    /// Describes the moon at <paramref name="time"/>. Local times are converted to UTC;
    /// unspecified times are treated as UTC.
    /// </summary>
    public static MoonPhaseInfo Calculate(DateTime time)
    {
        DateTime utc = ToUtc(time);
        double jde = AstroTime.ToJulianEphemerisDay(utc);

        double k = LunationAtOrBefore(jde);
        double previousNew = PhaseJde(k, isFullMoon: false);
        double nextNew = PhaseJde(k + 1, isFullMoon: false);
        double nextFull = PhaseJde(k + 0.5, isFullMoon: true);
        if (nextFull <= jde)
        {
            nextFull = PhaseJde(k + 1.5, isFullMoon: true);
        }

        bool isWaxing = nextFull < nextNew;
        double phaseAngle = PhaseAngleDegrees(jde);
        double illuminated = (1 + Math.Cos(ToRadians(phaseAngle))) / 2;
        double elongation = 180 - phaseAngle;
        double cycleDegrees = isWaxing ? elongation : 360 - elongation;

        return new MoonPhaseInfo(
            utc,
            illuminated,
            isWaxing,
            cycleDegrees / 360,
            jde - previousNew,
            PhaseFromCycle(cycleDegrees),
            AstroTime.FromJulianEphemerisDay(previousNew),
            AstroTime.FromJulianEphemerisDay(nextNew),
            AstroTime.FromJulianEphemerisDay(nextFull));
    }

    private static DateTime ToUtc(DateTime time) => time.Kind switch
    {
        DateTimeKind.Local => time.ToUniversalTime(),
        DateTimeKind.Unspecified => DateTime.SpecifyKind(time, DateTimeKind.Utc),
        _ => time,
    };

    private static MoonPhase PhaseFromCycle(double cycleDegrees)
    {
        int sector = (int)Math.Floor((cycleDegrees + 22.5) / 45) % 8;
        return (MoonPhase)sector;
    }

    /// <summary>The integer lunation number k of the last new moon at or before <paramref name="jde"/>.</summary>
    private static double LunationAtOrBefore(double jde)
    {
        double k = Math.Floor((jde - FirstNewMoonOf2000Jde) / SynodicMonth);
        while (PhaseJde(k, isFullMoon: false) > jde)
        {
            k--;
        }

        while (PhaseJde(k + 1, isFullMoon: false) <= jde)
        {
            k++;
        }

        return k;
    }

    /// <summary>
    /// Phase angle in degrees (0 = full, 180 = new), Meeus eq. 48.4 with the
    /// mean arguments of chapter 47. Good to a few tenths of a percent of illumination.
    /// </summary>
    private static double PhaseAngleDegrees(double jde)
    {
        double t = (jde - 2451545.0) / 36525;
        double d = 297.8501921 + 445267.1114034 * t - 0.0018819 * t * t
            + t * t * t / 545868 - Math.Pow(t, 4) / 113065000;
        double m = 357.5291092 + 35999.0502909 * t - 0.0001536 * t * t + t * t * t / 24490000;
        double mp = 134.9633964 + 477198.8675055 * t + 0.0087414 * t * t
            + t * t * t / 69699 - Math.Pow(t, 4) / 14712000;

        double i = 180 - d
            - 6.289 * Sin(mp)
            + 2.100 * Sin(m)
            - 1.274 * Sin(2 * d - mp)
            - 0.658 * Sin(2 * d)
            - 0.214 * Sin(2 * mp)
            - 0.110 * Sin(d);

        i = Normalize360(i);
        return i > 180 ? 360 - i : i;
    }

    /// <summary>
    /// JDE of the true new moon (integer <paramref name="k"/>) or full moon
    /// (<paramref name="k"/> ending in .5), Meeus chapter 49.
    /// </summary>
    internal static double PhaseJde(double k, bool isFullMoon)
    {
        double t = k / 1236.85;
        double t2 = t * t;
        double t3 = t2 * t;
        double t4 = t3 * t;

        double jde = FirstNewMoonOf2000Jde + SynodicMonth * k
            + 0.00015437 * t2 - 0.000000150 * t3 + 0.00000000073 * t4;

        double e = 1 - 0.002516 * t - 0.0000074 * t2;
        double m = 2.5534 + 29.10535670 * k - 0.0000014 * t2 - 0.00000011 * t3;
        double mp = 201.5643 + 385.81693528 * k + 0.0107582 * t2 + 0.00001238 * t3 - 0.000000058 * t4;
        double f = 160.7108 + 390.67050284 * k - 0.0016118 * t2 - 0.00000227 * t3 + 0.000000011 * t4;
        double omega = 124.7746 - 1.56375588 * k + 0.0020672 * t2 + 0.00000215 * t3;

        // Only the first seven coefficients differ between new and full moon.
        double[] c = isFullMoon
            ? [-0.40614, 0.17302, 0.01614, 0.01043, 0.00734, -0.00515, 0.00209]
            : [-0.40720, 0.17241, 0.01608, 0.01039, 0.00739, -0.00514, 0.00208];

        jde += c[0] * Sin(mp)
            + c[1] * e * Sin(m)
            + c[2] * Sin(2 * mp)
            + c[3] * Sin(2 * f)
            + c[4] * e * Sin(mp - m)
            + c[5] * e * Sin(mp + m)
            + c[6] * e * e * Sin(2 * m)
            - 0.00111 * Sin(mp - 2 * f)
            - 0.00057 * Sin(mp + 2 * f)
            + 0.00056 * e * Sin(2 * mp + m)
            - 0.00042 * Sin(3 * mp)
            + 0.00042 * e * Sin(m + 2 * f)
            + 0.00038 * e * Sin(m - 2 * f)
            - 0.00024 * e * Sin(2 * mp - m)
            - 0.00017 * Sin(omega)
            - 0.00007 * Sin(mp + 2 * m)
            + 0.00004 * Sin(2 * mp - 2 * f)
            + 0.00004 * Sin(3 * m)
            + 0.00003 * Sin(mp + m - 2 * f)
            + 0.00003 * Sin(2 * mp + 2 * f)
            - 0.00003 * Sin(mp + m + 2 * f)
            + 0.00003 * Sin(mp - m + 2 * f)
            - 0.00002 * Sin(mp - m - 2 * f)
            - 0.00002 * Sin(3 * mp + m)
            + 0.00002 * Sin(4 * mp);

        // Additional corrections common to all phases.
        jde += 0.000325 * Sin(299.77 + 0.107408 * k - 0.009173 * t2)
            + 0.000165 * Sin(251.88 + 0.016321 * k)
            + 0.000164 * Sin(251.83 + 26.651886 * k)
            + 0.000126 * Sin(349.42 + 36.412478 * k)
            + 0.000110 * Sin(84.66 + 18.206239 * k)
            + 0.000062 * Sin(141.74 + 53.303771 * k)
            + 0.000060 * Sin(207.14 + 2.453732 * k)
            + 0.000056 * Sin(154.84 + 7.306860 * k)
            + 0.000047 * Sin(34.52 + 27.261239 * k)
            + 0.000042 * Sin(207.19 + 0.121824 * k)
            + 0.000040 * Sin(291.34 + 1.844379 * k)
            + 0.000037 * Sin(161.72 + 24.198154 * k)
            + 0.000035 * Sin(239.56 + 25.513099 * k)
            + 0.000023 * Sin(331.55 + 3.592518 * k);

        return jde;
    }

    private static double Sin(double degrees) => Math.Sin(ToRadians(degrees));

    private static double ToRadians(double degrees) => degrees * Math.PI / 180;

    private static double Normalize360(double degrees)
    {
        double result = degrees % 360;
        return result < 0 ? result + 360 : result;
    }
}
