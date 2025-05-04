using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Tutorial8.Exceptions;
using Tutorial8.Models.DTOs;
using Tutorial8.Services;

namespace Tutorial8.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ClientsController : Controller
{
    private readonly IClientsService _clientsService;

    private readonly Regex _nameRegex = new Regex("^[a-zA-Z]{2,32}$");
    private readonly Regex _emailRegex = new Regex("^\\w+@[\\w+.]+\\w+$");
    private readonly Regex _telephoneRegex = new Regex("^\\+\\d{10,12}$");
    private readonly Regex _peselRegex = new Regex("^\\d{11}$");

    public ClientsController(IClientsService clientsService)
    {
        _clientsService = clientsService;
    }

    /// <summary>
    /// Retrieves information about client's trips with a given client id
    /// </summary>
    /// <param name="id">
    /// Id of the client
    /// </param>
    /// <returns>
    /// Ok(200) with list of client's trips,
    /// NotFound(404) if client with a given id doesn't exist
    /// </returns>
    [HttpGet("{id}/trips")]
    public async Task<IActionResult> GetClientTrips(int id)
    {
        try
        {
            var trips = await _clientsService.GetClientTrips(id);
            return Ok(trips);
        }
        catch (ClientNotFoundException)
        {
            return NotFound(new {message = "Client not found"});
        }
    }

    /// <summary>
    /// Creates a new client with provided data
    /// </summary>
    /// <param name="clientDto">
    /// Data of the client to create
    /// </param>
    /// <returns>
    /// Created(201) with id of the created client
    /// BadRequest(400) if fields don't match the correct format
    /// </returns>
    [HttpPost]
    public async Task<IActionResult> CreateClient(ClientDTO clientDto)
    {
        if (!_nameRegex.IsMatch(clientDto.FirstName))
        {
            return BadRequest(new {message = "Invalid first name format"});
        }
        if (!_nameRegex.IsMatch(clientDto.LastName))
        {
            return BadRequest(new {message = "Invalid last name format"});
        }
        if (!_emailRegex.IsMatch(clientDto.Email))
        {
            return BadRequest(new {message = "Invalid email format"});
        }
        
        if (!_telephoneRegex.IsMatch(clientDto.Telephone))
        {
            return BadRequest(new {message = "Invalid telephone format"});
        }

        if (!_peselRegex.IsMatch(clientDto.Pesel))
        {
            return BadRequest(new {message = "Invalid pesel format"});
        }
        return new ObjectResult(await _clientsService.CreateClient(clientDto))
        {
            StatusCode = StatusCodes.Status201Created
        };
    }

    /// <summary>
    /// Registers a client for a trip
    /// </summary>
    /// <param name="id">
    /// Id of a client to register
    /// </param>
    /// <param name="tripId">
    /// Id of a trip to register for
    /// </param>
    /// <returns>
    /// Created(201) with updated list of client's trips,
    /// NotFound(404) if client with a given id doesn't exist,
    /// NotFound(404) if trip with a given id doesn't exist,
    /// Conflict(409) if max people is reached for provided trip
    /// Conflict(409) if provided client is already registered for provided trip
    /// </returns>
    [HttpPut("{id}/trips/{tripId}")]
    public async Task<IActionResult> RegisterClientForTrip(int id, int tripId)
    {
        try
        {
            var trips = await _clientsService.RegisterClientForTrip(id, tripId);
            return new ObjectResult(trips)
            {
                StatusCode = StatusCodes.Status201Created
            };
        }
        catch (ClientNotFoundException)
        {
            return NotFound(new {message = "Client not found"});
        }
        catch (TripNotFoundException)
        {
            return NotFound(new {message = "Trip not found"});
        }
        catch (MaxPeopleReachedException)
        {
            return Conflict(new {message = "No slots available for provided trip"});
        }
        catch (ClientAlreadyRegisteredForTripException)
        {
            return Conflict(new {message = "Client is already registered for provided trip"});
        }
    }

    /// <summary>
    /// Deletes client's registration for trip
    /// </summary>
    /// <param name="id">
    /// Id of a client to delete registration from
    /// </param>
    /// <param name="tripId">
    /// Id of a trip to delete registration for
    /// </param>
    /// <returns>
    /// Ok(200) with updated list of client's trips,
    /// NotFound(404) if client with a given id doesn't exist,
    /// NotFound(404) if trip with a given id doesn't exist,
    /// NotFound(404) if client is not registered for provided trip
    /// </returns>
    [HttpDelete("{id}/trips/{tripId}")]
    public async Task<IActionResult> DeleteClientRegistration(int id, int tripId)
    {
        try
        {
            var trips = await _clientsService.DeleteClientRegistration(id, tripId);
            return Ok(trips);
        }
        catch (ClientNotFoundException)
        {
            return NotFound(new {message = "Client not found"});
        }
        catch (TripNotFoundException)
        {
            return NotFound(new {message = "Trip not found"});
        }
        catch (ClientNotRegisteredForTripException)
        {
            return NotFound(new {message = "Client is not registered for provided trip"});
        }
    }
}