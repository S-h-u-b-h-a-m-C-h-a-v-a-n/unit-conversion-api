namespace UnitConversion.Api;

internal static class ApiConstants
{
    public const string RoutePrefix = "api/v1";

    /// <summary><see cref="decimal"/> supports at most 28 decimal places.</summary>
    public const int MaxDecimalPlaces = 28;
}
