using Microsoft.AspNetCore.Mvc;
using UnitConversion.Api.Contracts;
using UnitConversion.Api.Mapping;
using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Api.Controllers;

/// <summary>Discovery endpoints for individual units.</summary>
[ApiController]
[Route(ApiConstants.RoutePrefix + "/units")]
[Produces("application/json")]
public sealed class UnitsController(IUnitCatalog catalog) : ControllerBase
{
    /// <summary>Lists all units, optionally filtered by category.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<UnitResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IEnumerable<UnitResponse> GetAll([FromQuery] string? category = null) =>
        catalog.GetUnits(category).Select(u => u.ToResponse());

    /// <summary>Resolves a unit by symbol, name or alias.</summary>
    [HttpGet("{unit}")]
    [ProducesResponseType<UnitResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<UnitResponse> Get(string unit)
    {
        try
        {
            return catalog.GetUnit(unit).ToResponse();
        }
        catch (UnitNotFoundException ex)
        {
            return Problem(title: ex.Title, detail: ex.Message, statusCode: StatusCodes.Status404NotFound);
        }
    }
}
