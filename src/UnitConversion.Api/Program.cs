using Scalar.AspNetCore;
using UnitConversion.Api.Configuration;
using UnitConversion.Api.ErrorHandling;
using UnitConversion.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddOptions<ConversionOptions>()
    .Bind(builder.Configuration.GetSection(ConversionOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddUnitConversion();

builder.Services.AddControllers();
builder.Services.AddProblemDetails(options =>
    options.CustomizeProblemDetails = ctx =>
        ctx.ProblemDetails.Extensions["traceId"] = ctx.HttpContext.TraceIdentifier);
builder.Services.AddExceptionHandler<UnitConversionExceptionHandler>();
builder.Services.AddOpenApi();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Configuration.GetValue("Api:EnableDocs", app.Environment.IsDevelopment()))
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => options.WithTitle("Unit Conversion API"));
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

app.UseHttpsRedirection();

// Serves the browser UI from wwwroot (index.html at "/").
app.UseDefaultFiles();
app.UseStaticFiles();

app.MapControllers();
app.MapHealthChecks("/health");

await app.RunAsync();

/// <summary>Entry point; public so integration tests can host the app with WebApplicationFactory.</summary>
public partial class Program;
