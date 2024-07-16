namespace LloydWarningSystem.Net;

internal static class Qol
{
    public static string Pluralize(string opt1, string opt2, bool go)
        => go ? opt1 : opt2;

    public static string Pluralize(this string plural, bool go)
        => go ? plural : string.Empty;

    public static string Pluralize(this string plural, int num)
        => Pluralize(plural, num != 1);

    public static char Pluralize(this char plural, bool go)
        => go ? plural : '\0';

    public static char Pluralize(this char plural, int num)
        => Pluralize(plural, num != 1);
}
