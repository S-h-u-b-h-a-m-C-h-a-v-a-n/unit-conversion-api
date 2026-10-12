namespace UnitConversion.Api.Contracts;

public sealed record CategoryResponse(string Key, string Name, string BaseUnit, int UnitCount);

public sealed record UnitResponse(string Symbol, string Name, string Category, IReadOnlyList<string> Aliases);
