using Microsoft.AspNetCore.Mvc;
using UnitConversion.Api.Contracts;
using UnitConversion.Api.Mapping;
using UnitConversion.Core.Abstractions;
using UnitConversion.Core.Exceptions;

namespace UnitConversion.Api.Controllers;

/// <summary>Discovery endpoints for the supported conversion categories.</summary>
[ApiController]
[Route(ApiConstants.RoutePrefix + "/categories")]
[Produces("application/json")]
public sealed class CategoriesController(IUnitCatalog catalog) : ControllerBase
{
    /// <summary>Lists all supported categories.</summary>
    [HttpGet]
    [ProducesResponseType<IEnumerable<CategoryResponse>>(StatusCodes.Status200OK)]
    public IEnumerable<CategoryResponse> GetAll() =>
        catalog.Categories.Select(c => c.ToResponse(catalog.GetUnits(c.Key).Count));

    /// <summary>Gets a single category.</summary>
    [HttpGet("{key}")]
    [ProducesResponseType<CategoryResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public ActionResult<CategoryResponse> Get(string key) =>
        catalog.TryGetCategory(key, out var category)
            ? category.ToResponse(catalog.GetUnits(category.Key).Count)
            : throw new CategoryNotFoundException(key);

    /// <summary>Lists the units of a category.</summary>
    [HttpGet("{key}/units")]
    [ProducesResponseType<IEnumerable<UnitResponse>>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status404NotFound)]
    public IEnumerable<UnitResponse> GetUnits(string key) =>
        catalog.GetUnits(key).Select(u => u.ToResponse());
}
