namespace MoonPhasesWidget.Astronomy;

/// <summary>
/// Conversions between UTC, Julian Day (JD) and Julian Ephemeris Day (JDE, in Terrestrial Time).
/// </summary>
internal static class AstroTime
{
    private const double UnixEpochJulianDay = 2440587.5;
    private const double SecondsPerDay = 86400.0;

    public static double ToJulianDay(DateTime utc) =>
        UnixEpochJulianDay + (utc - DateTime.UnixEpoch).TotalDays;

    public static DateTime FromJulianDay(double jd) =>
        DateTime.UnixEpoch.AddDays(jd - UnixEpochJulianDay);

    public static double ToJulianEphemerisDay(DateTime utc) =>
        ToJulianDay(utc) + DeltaTSeconds(DecimalYear(utc)) / SecondsPerDay;

    public static DateTime FromJulianEphemerisDay(double jde)
    {
        double year = DecimalYear(FromJulianDay(jde));
        return FromJulianDay(jde - DeltaTSeconds(year) / SecondsPerDay);
    }

    /// <summary>
    /// ΔT = TT − UT in seconds, from the Espenak &amp; Meeus polynomials (NASA eclipse site).
    /// Accurate to a few seconds for recent decades; only a rough estimate far from the present.
    /// </summary>
    public static double DeltaTSeconds(double year)
    {
        if (year >= 1986 && year < 2005)
        {
            double t = year - 2000;
            return 63.86 + 0.3345 * t - 0.060374 * t * t + 0.0017275 * t * t * t
                + 0.000651814 * Math.Pow(t, 4) + 0.00002373599 * Math.Pow(t, 5);
        }

        if (year >= 2005 && year < 2050)
        {
            double t = year - 2000;
            return 62.92 + 0.32217 * t + 0.005589 * t * t;
        }

        double u = (year - 1820) / 100;
        double longTerm = -20 + 32 * u * u;
        return year >= 2050 && year < 2150 ? longTerm - 0.5628 * (2150 - year) : longTerm;
    }

    private static double DecimalYear(DateTime utc) => utc.Year + (utc.Month - 0.5) / 12;
}
