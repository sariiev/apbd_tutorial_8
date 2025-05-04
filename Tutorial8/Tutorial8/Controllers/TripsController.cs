using Microsoft.AspNetCore.Mvc;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TripsController : ControllerBase
{
    private readonly ITripsService _tripsService;

    public TripsController(ITripsService tripsService)
    {
        _tripsService = tripsService;
    }

    /// <summary>
    /// Retrieves list of all trips
    /// </summary>
    /// <returns>
    /// Ok(200) with list of all trips
    /// </returns>
    [HttpGet]
    public async Task<IActionResult> GetTrips()
    {
        var trips = await _tripsService.GetTrips();
        return Ok(trips);
    }

    /// <summary>
    /// Retrieves information about a trip with a given trip id
    /// </summary>
    /// <param name="id">
    /// Id of the trip to retrieve
    /// </param>
    /// <returns>
    /// Ok(200) with the trip information
    /// NotFound(404) if trip with a given id doesn't exist
    /// </returns>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTrip(int id)
    {
        var trip = await _tripsService.GetTripById(id);
        if (trip == null)
        {
            return NotFound("Trip not found");
        }

        return Ok(trip);
    }
}