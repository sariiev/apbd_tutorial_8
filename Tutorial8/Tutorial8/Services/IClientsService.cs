using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public interface IClientsService
{
    Task<int> CreateClient(ClientDTO clientDto);
    Task<List<ClientTripDto>> GetClientTrips(int id);

    Task<List<ClientTripDto>> RegisterClientForTrip(int id, int tripId);

    Task<List<ClientTripDto>> DeleteClientRegistration(int id, int tripId);
}