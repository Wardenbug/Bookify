using Asp.Versioning;
using Bookify.Application.Apartments;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Bookify.Api.Controllers.Apartments;

[Route("api/apartments")]
[ApiVersion(ApiVersions.V1)]
[ApiController]
public class ApartmentsController : ControllerBase
{
    private readonly ISender _sender;

    public ApartmentsController(ISender sender)
    {
        _sender = sender;
    }

    [HttpGet]
    public async Task<IActionResult> SearchApartments(
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        var query = new SearchApartmentsQuery(startDate, endDate);

        var result = await _sender.Send(query, cancellationToken);

        return Ok(result.Value);
    }
}
