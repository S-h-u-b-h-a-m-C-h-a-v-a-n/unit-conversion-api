using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using UnitConversion.Api.Configuration;
using UnitConversion.Api.Contracts;
using UnitConversion.Api.Mapping;
using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Api.Controllers;

/// <summary>Converts values between units of the same category.</summary>
[ApiController]
[Route(ApiConstants.RoutePrefix + "/conversions")]
[Produces("application/json")]
public sealed class ConversionsController(IUnitConverter converter, IOptions<ConversionOptions> options) : ControllerBase
{
    private readonly ConversionOptions _options = options.Value;

    /// <summary>Converts a single value.</summary>
    /// <remarks>Example: <c>GET /api/v1/conversions?value=100&amp;from=C&amp;to=F</c> returns 212.</remarks>
    [HttpGet]
    [ProducesResponseType<ConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status422UnprocessableEntity)]
    public ActionResult<ConversionResponse> Convert([FromQuery] ConversionQuery query)
    {
        var result = converter.Convert(query.Value!.Value, query.From!, query.To!);
        return result.ToResponse(query.Decimals ?? _options.DefaultDecimalPlaces);
    }

    /// <summary>Converts several values in one request.</summary>
    /// <remarks>Items are independent: an invalid item is reported in its own entry and does not fail the batch.</remarks>
    [HttpPost("batch")]
    [Consumes("application/json")]
    [ProducesResponseType<BatchConversionResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public ActionResult<BatchConversionResponse> ConvertBatch([FromBody] BatchConversionRequest request)
    {
        var items = request.Items!;
        if (items.Count > _options.MaxBatchSize)
        {
            ModelState.AddModelError(nameof(request.Items), $"A batch may contain at most {_options.MaxBatchSize} items.");
            return ValidationProblem(ModelState);
        }

        var decimals = request.Decimals ?? _options.DefaultDecimalPlaces;
        var responses = new List<BatchConversionItemResponse>(items.Count);
        for (var i = 0; i < items.Count; i++)
        {
            responses.Add(ConvertItem(i, items[i], decimals));
        }

        var succeeded = responses.Count(r => r.Succeeded);
        return new BatchConversionResponse(succeeded, responses.Count - succeeded, responses);
    }

    private BatchConversionItemResponse ConvertItem(int index, ConversionRequest item, int decimals)
    {
        try
        {
            var result = converter.Convert(item.Value!.Value, item.From!, item.To!);
            return new BatchConversionItemResponse(index, true, result.ToResponse(decimals), null);
        }
        catch (UnitConversionException ex)
        {
            return new BatchConversionItemResponse(index, false, null, ex.Message);
        }
    }
}
