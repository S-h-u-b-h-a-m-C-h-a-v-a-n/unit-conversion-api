# Unit Conversion API

An ASP.NET Core (.NET 10) Web API that converts numeric values between units of measurement:
metres to feet, Celsius to Fahrenheit, kilograms to pounds, and so on.

It supports **8 categories / 73 units** out of the box: length, mass, temperature, volume,
area, time, speed and data storage. The design aims to scale to hundreds of units without
code changes to the conversion logic.

It also ships a small **browser UI** (plain HTML/CSS/JavaScript, served by the API at `/`)
for trying conversions without any tools.

```http
GET /api/v1/conversions?value=100&from=C&to=F
```

```json
{
  "value": 100,
  "from": { "symbol": "°C", "name": "celsius" },
  "to":   { "symbol": "°F", "name": "fahrenheit" },
  "result": 212,
  "category": "temperature"
}
```

---

## Contents

- [Running locally](#running-locally)
- [Web frontend](#web-frontend)
- [API reference](#api-reference)
- [Project structure](#project-structure)
- [Design decisions and trade-offs](#design-decisions-and-trade-offs)
- [Adding a unit or category](#adding-a-unit-or-category)
- [Development workflow](#development-workflow)

---

## Running locally

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0) (`dotnet --version` should print `10.0.x`)
- Optional: Docker

### With the .NET CLI

```bash
git clone <this-repository-url>
cd <repository-folder>

dotnet restore
dotnet run --project src/UnitConversion.Api --launch-profile http
```

The API listens on **http://localhost:5080**. In Development:

| URL | What |
| --- | --- |
| http://localhost:5080 | **Web frontend**: the converter UI |
| http://localhost:5080/scalar | Interactive API documentation (try requests in the browser) |
| http://localhost:5080/openapi/v1.json | OpenAPI document |
| http://localhost:5080/health | Health check |

For HTTPS, use `--launch-profile https` (https://localhost:7080). If you have not trusted the
dev certificate yet, run `dotnet dev-certs https --trust` once first.

`src/UnitConversion.Api/UnitConversion.Api.http` has ready-made sample requests for
Visual Studio, Rider or the VS Code REST Client extension.

### With Visual Studio / Rider

Open `UnitConversion.slnx`, set **UnitConversion.Api** as the startup project and press F5.

### With Docker

```bash
docker build -t unit-conversion-api .
docker run --rm -p 8080:8080 -e ASPNETCORE_ENVIRONMENT=Development unit-conversion-api
```

Then open http://localhost:8080 (UI) or http://localhost:8080/scalar (API docs). API docs are off by default outside Development; you
can enable them with `-e Api__EnableDocs=true`.

### Running the tests

```bash
dotnet test
```

---

## Web frontend

Open **http://localhost:5080** after starting the API. The page lets you:

- pick a category, type a value and choose the from/to units; the result updates as you type;
- swap the units, choose the rounding and copy the result;
- see the value in **every unit of the category** at once (this uses the batch endpoint),
  and click a row to make it the target unit;
- reuse recent conversions (kept in the browser's local storage).

Error responses from the API (incompatible units, below absolute zero, ...) are shown inline.
It works on phones and follows the system's light/dark theme.

**Why plain HTML/JS served from `wwwroot`:** the assessment is about the API, so the UI is
deliberately small. Static files need no Node.js, npm or build step, start with the same
`dotnet run`, and share the API's origin, so no CORS configuration is needed. The UI uses only
the public API endpoints, so it could later be replaced by a separate React/Angular app
(with CORS enabled) without touching the backend.

Source: `src/UnitConversion.Api/wwwroot/` (`index.html`, `css/site.css`, `js/app.js`).

---

## API reference

All endpoints are versioned under `/api/v1`. Errors use the
[RFC 7807 Problem Details](https://www.rfc-editor.org/rfc/rfc7807) format.

### Convert a value

`GET /api/v1/conversions?value={number}&from={unit}&to={unit}[&decimals={0-28}]`

| Parameter | Required | Description |
| --- | --- | --- |
| `value` | yes | Number to convert (invariant culture, `.` as decimal separator). |
| `from`, `to` | yes | Unit **symbol**, **name** or **alias**: `°C`, `C`, `celsius` and `degC` all work. |
| `decimals` | no | Rounding of the result (default `10`, configurable). |

Units are resolved like this:

1. Exact, case-sensitive symbol match (`MB` = megabyte, `Mb` = megabit).
2. Otherwise, case-insensitive match on symbol, name or alias (`FT`, `feet`, `Foot`).
3. If a case-insensitive symbol is ambiguous (`mb`), the error suggests the exact symbols.

URL-encode special characters: `°C` → `%C2%B0C`, `m²` → `m%C2%B2`. Plain-ASCII aliases
such as `C`, `m2` or `ft2` avoid that.

### Convert many values

`POST /api/v1/conversions/batch`

```json
{
  "items": [
    { "value": 1, "from": "mi", "to": "km" },
    { "value": 1, "from": "kg", "to": "m" }
  ],
  "decimals": 3
}
```

Each item succeeds or fails on its own, so a bad item does not fail the whole batch:

```json
{
  "succeeded": 1,
  "failed": 1,
  "items": [
    { "index": 0, "succeeded": true,  "conversion": { "value": 1, "result": 1.609, "...": "..." }, "error": null },
    { "index": 1, "succeeded": false, "conversion": null, "error": "Cannot convert 'kg' (mass) to 'm' (length): units belong to different categories." }
  ]
}
```

### Discovery

| Endpoint | Description |
| --- | --- |
| `GET /api/v1/categories` | All categories with base unit and unit count |
| `GET /api/v1/categories/{key}` | One category |
| `GET /api/v1/categories/{key}/units` | Units in a category |
| `GET /api/v1/units[?category={key}]` | All units, optionally filtered |
| `GET /api/v1/units/{unit}` | Resolve a symbol/name/alias to a unit |

### Status codes

| Status | When |
| --- | --- |
| `200` | Success |
| `400` | Missing/malformed input, unknown unit, units from different categories, batch too large |
| `404` | Unknown category or unit in the URL path |
| `422` | Value is physically impossible (below absolute zero) or the result overflows |
| `500` | Unexpected error (no internal details are returned) |

### Supported units

| Category (`key`) | Base unit | Units |
| --- | --- | --- |
| Length (`length`) | m | nm, µm, mm, cm, m, km, in, ft, yd, mi, nmi |
| Mass (`mass`) | kg | mg, g, kg, t, oz, lb, st, ton (short), LT (long) |
| Temperature (`temperature`) | K | K, °C, °F, °R |
| Volume (`volume`) | L | mL, cm³, L, m³, tsp, tbsp, fl oz, cup, pt, qt, gal, imp gal |
| Area (`area`) | m² | mm², cm², m², ha, km², in², ft², yd², ac, mi² |
| Time (`time`) | s | ns, µs, ms, s, min, h, d, wk, yr |
| Speed (`speed`) | m/s | m/s, km/h, mph, ft/s, kn |
| Data storage (`data-storage`) | B | b, kb, Mb, Gb, B, kB, MB, GB, TB, KiB, MiB, GiB, TiB |

Call `GET /api/v1/units` for the live list including all aliases.

---

## Project structure

```
├── src/
│   ├── UnitConversion.Api/              HTTP layer: controllers, contracts (DTOs), error handling, configuration
│   │   └── wwwroot/                     Browser UI (index.html, css/, js/), served at "/"
│   ├── UnitConversion.Core/             Domain + application logic, no framework dependencies
│   │   ├── Domain/                      UnitCategory, UnitDefinition, ConversionResult
│   │   ├── Abstractions/                IUnitCatalog, IUnitConverter, IUnitDefinitionProvider
│   │   ├── Services/                    UnitCatalog (index + validation), UnitConverter
│   │   └── Exceptions/                  Typed, client-facing domain errors
│   └── UnitConversion.Infrastructure/   Unit data (one provider per category) + DI registration
├── tests/
│   ├── UnitConversion.UnitTests/        Converter and catalog tests, including a round trip over every unit pair
│   └── UnitConversion.IntegrationTests/ Full HTTP pipeline tests via WebApplicationFactory
├── .github/                             CI workflow and PR template
├── Directory.Build.props                Shared build settings (nullable, analyzers, warnings as errors)
├── Directory.Packages.props             Central NuGet version management
├── global.json                          Pinned SDK
├── .editorconfig / .gitattributes       Code style and line endings, enforced in CI
└── Dockerfile                           Multi-stage, non-root runtime image
```

Dependencies only point inward: `Api → Infrastructure → Core`. `Core` has no NuGet or
ASP.NET dependencies, so the conversion logic can be reused (CLI, worker, other host) and
tested in isolation.

---

## Design decisions and trade-offs

### 1. Convert through a base unit (hub and spoke)

Each category has one base unit (metre, kilogram, kelvin, ...). Each unit only stores how it
relates to that base, and a conversion is `from → base → to`.

- **Why:** with *N* units you maintain *N* definitions instead of *N²* pairwise factors.
  Adding a unit makes it convertible to every other unit in its category at once. This is
  what makes "hundreds of units" manageable.
- **Trade-off:** two arithmetic steps instead of one, which is negligible.

### 2. Affine conversions with rational factors

Every unit is described by `base = (value + offset) × numerator / denominator`.

- Linear units (feet, pounds) have `offset = 0`. Temperature scales need an offset, so the
  same model handles both without special cases for temperature.
- Factors are stored as fractions, so ratios such as °F's `5/9` or km/h's `1000/3600` stay
  exact instead of becoming truncated repeating decimals.

### 3. `decimal` instead of `double`

- **Why:** unit factors are defined in base 10 (`1 in = 0.0254 m` exactly). With `double`,
  `100 °C → °F` returns `211.99999999999994`; with `decimal` and rational factors it returns
  exactly `212`. Callers of a conversion API expect the textbook answer.
- **Trade-off:** `decimal` is slower than `double` and has a smaller range (about ±7.9 × 10²⁸).
  Values that overflow return a clear `422` instead of a wrong number. For scientific ranges
  (astronomical distances in nanometres), a future version could switch to `double` or
  `BigInteger` rationals behind the same `IUnitConverter` interface.
- Results are rounded to `decimals` places (default 10) with `MidpointRounding.AwayFromZero`,
  and trailing zeros are removed.

### 4. Data behind an interface (`IUnitDefinitionProvider`)

The assessment allows hardcoded data, so units live in C# classes, one file per category, in
`Infrastructure/Units`. The conversion logic only depends on the `IUnitDefinitionProvider`
abstraction.

- **Why:** to support hundreds of units, or units maintained by non-developers, you add a
  provider that reads from a database, JSON file or admin service. `Core` and `Api` do not
  change.
- **Why C# rather than JSON for now:** compile-time checking, no file I/O or parsing code,
  and changes are reviewed like code.

### 5. Catalog is validated and indexed once, at startup

`UnitCatalog` is an immutable singleton built from all providers. It:

- builds dictionary indexes, so lookups are O(1) whatever the number of units, and it is
  thread-safe without locks;
- **fails fast** on bad data: duplicate symbols, an alias shared by two units, a missing base
  unit, or a unit registered in the wrong category. A bad unit definition breaks the build's
  tests and app startup, not a customer request.

### 6. Case-sensitive symbols, forgiving names

Symbols are matched by exact case first because case carries meaning (`Mb` megabit vs `MB`
megabyte, `b` bit vs `B` byte). Names and aliases ignore case for convenience. Ambiguous
inputs return an error listing the candidates instead of guessing.

### 7. Physical validation in the domain

Categories can declare a minimum (`0 K` for temperature), so `-300 °C` is rejected with a
`422` and an explanation instead of returning a meaningless value.

### 8. Errors

- Domain errors are typed exceptions (`UnitNotFoundException`, `IncompatibleUnitsException`,
  ...) mapped centrally to Problem Details by one `IExceptionHandler`. Controllers stay free of
  error plumbing, and every error response has the same shape plus a `traceId`.
- Unexpected exceptions return a generic `500` with no stack trace.
- **Trade-off:** a `Result<T>` type would avoid exceptions for control flow. Exceptions were
  chosen for readability, since these errors are rare relative to successful calls.

### 9. API shape

- `GET` for single conversions: they are safe, idempotent and cacheable, and easy to try from a
  browser.
- `POST /batch` for bulk work, with per-item results so clients can convert a whole form or
  report in one round trip.
- URL-segment versioning (`/api/v1`) keeps it simple and visible. If several versions ever
  need to coexist, `Asp.Versioning.Mvc` can be added.
- Controllers rather than Minimal APIs: they are familiar to most .NET teams and keep the
  attribute-based OpenAPI metadata next to each action. Both are valid choices.

### 10. Team and maintainability setup

- **Central Package Management** (`Directory.Packages.props`): one place to bump versions.
- **`Directory.Build.props`**: nullable reference types, latest analyzers and
  `TreatWarningsAsErrors` for every project.
- **`.editorconfig` + `dotnet format --verify-no-changes` in CI** so style is not debated in
  reviews.
- **`global.json`** pins the SDK so local and CI builds match.
- **Options pattern with validation on start** (`Conversion` section in `appsettings.json`), so
  misconfiguration fails at startup.

### Out of scope (possible next steps)

- Persisted unit catalog (database) plus an admin API to manage units
- Authentication, rate limiting and response caching
- Compound units (e.g. `kg·m/s²`) and currency (needs live exchange rates)
- Localised unit names
- Browser end-to-end tests for the UI (e.g. Playwright)

---

## Adding a unit or category

**New unit in an existing category:** add one line to the provider in
`src/UnitConversion.Infrastructure/Units/`:

```csharp
Linear(Key, "fur", "furlong", 201.168m, "furlongs"),   // 1 furlong = 201.168 m
```

Use `Linear` for `1 unit = factor × base`, `Ratio` when the factor is a fraction, and
`Affine` when the scale has a different zero point (like temperatures).

**New category:**

1. Create `src/UnitConversion.Infrastructure/Units/PressureUnits.cs` implementing
   `IUnitDefinitionProvider` (copy an existing file as a template).
2. Add `new PressureUnits()` to `HardcodedUnitProviders.All` in `DependencyInjection.cs`.
3. Add a few known-value cases to `UnitConverterTests`. The round-trip test covers the new units
   automatically, and the catalog validates the data at startup.

---

## Development workflow

```bash
dotnet build                          # warnings are errors
dotnet test                           # unit + integration tests
dotnet format                         # fix style issues
dotnet format --verify-no-changes     # what CI runs
```

CI (`.github/workflows/ci.yml`) runs format check, build, tests with coverage, and a Docker build
on every pull request.
