using Microsoft.AspNetCore.Mvc;
using PlacementApi.Data;

namespace PlacementApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly PlacementDbContext _db;

    public HealthController(PlacementDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> Get(CancellationToken ct)
    {
        var databaseUp = await _db.Database.CanConnectAsync(ct);

        var body = new
        {
            status = databaseUp ? "Healthy" : "Degraded",
            database = databaseUp ? "Connected" : "Unreachable",
            time = DateTime.UtcNow
        };

        // 503 tells monitoring tools (and load balancers) that this instance should not receive traffic
        return databaseUp ? Ok(body) : StatusCode(StatusCodes.Status503ServiceUnavailable, body);
    }
}