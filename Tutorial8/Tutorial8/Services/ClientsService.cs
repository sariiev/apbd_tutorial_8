using System.Globalization;
using Microsoft.Data.SqlClient;
using Tutorial8.Exceptions;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class ClientsService : IClientsService
{
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=APBD_tutorial_8; Initial Catalog=apbd; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    
    public async Task<List<ClientTripDto>> GetClientTrips(int id)
    {
        var tripsDict = new Dictionary<int, ClientTripDto>();
        var countriesDict = new Dictionary<int, CountryDTO>();

        // used to check if client exists
        string clientExistsQuery = "SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient";
        // retrieves client's trips
        string clientTripsQuery = "SELECT t.IdTrip, t.Name as TripName, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name as CountryName, ct2.RegisteredAt, ct2.PaymentDate FROM Trip t JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON ct.IdCountry = c.IdCountry JOIN Client_Trip ct2 ON t.IdTrip = ct2.IdTrip WHERE ct2.IdClient = @IdClient";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            using (SqlCommand clientExistsCommand = new SqlCommand(clientExistsQuery, conn))
            {
                clientExistsCommand.Parameters.AddWithValue("@IdClient", id);

                int count = (int) await clientExistsCommand.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new ClientNotFoundException();
                }
            }
            
            using (SqlCommand clientTripsCommand = new SqlCommand(clientTripsQuery, conn))
            {
                clientTripsCommand.Parameters.AddWithValue("@IdClient", id);

                using (var reader = await clientTripsCommand.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        int countryIdOrdinal = reader.GetOrdinal("IdCountry");
                        int countryId = reader.GetInt32(countryIdOrdinal);
                            
                        if (!countriesDict.ContainsKey(countryId))
                        {
                            int countryNameOrdinal = reader.GetOrdinal("CountryName");
                            countriesDict.Add(countryId, new CountryDTO()
                            {
                                Id = countryId,
                                Name = reader.GetString(countryNameOrdinal)
                            });
                        }
                        
                        int tripIdOrdinal = reader.GetOrdinal("IdTrip");
                        int tripId = reader.GetInt32(tripIdOrdinal);

                        if (!tripsDict.ContainsKey(tripId))
                        {
                            int tripNameOrdinal = reader.GetOrdinal("TripName");
                            int tripDescriptionOrdinal = reader.GetOrdinal("Description");
                            int tripDateFromOrdinal = reader.GetOrdinal("DateFrom");
                            int tripDateToOrdinal = reader.GetOrdinal("DateTo");
                            int tripMaxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                            int registrationDateOrdinal = reader.GetOrdinal("RegisteredAt");
                            int paymentDateOrdinal = reader.GetOrdinal("PaymentDate");
                            
                            tripsDict.Add(tripId, new ClientTripDto()
                                {
                                    Trip = new TripDTO() {
                                        Id = tripId,
                                        Name = reader.GetString(tripNameOrdinal),
                                        Description = reader.GetString(tripDescriptionOrdinal),
                                        StartDateTime = reader.GetDateTime(tripDateFromOrdinal),
                                        EndDateTime = reader.GetDateTime(tripDateToOrdinal),
                                        MaxPeople = reader.GetInt32(tripMaxPeopleOrdinal)
                                    },
                                    RegistrationDate = DateTime.ParseExact(reader.GetInt32(registrationDateOrdinal).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture),
                                    PaymentDate = await reader.IsDBNullAsync(paymentDateOrdinal) ? null : DateTime.ParseExact(reader.GetInt32(paymentDateOrdinal).ToString(), "yyyyMMdd", CultureInfo.InvariantCulture)
                                }
                            );   
                        }
                        
                        tripsDict[tripId].Trip.Countries.Add(countriesDict[countryId]);
                    }
                }
            }
        }
        

        return tripsDict.Values.ToList();
    }

    public async Task<int> CreateClient(ClientDTO clientDto)
    {
        // inserts new client's data
        string insertClientQuery =
            "INSERT INTO Client (FirstName, LastName, Email, Telephone, Pesel) VALUES (@FirstName, @LastName, @Email, @Telephone, @Pesel); SELECT SCOPE_IDENTITY();";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand insertClientCommand = new SqlCommand(insertClientQuery, conn))
        {
            await conn.OpenAsync();

            insertClientCommand.Parameters.AddWithValue("@FirstName", clientDto.FirstName);
            insertClientCommand.Parameters.AddWithValue("@LastName", clientDto.LastName);
            insertClientCommand.Parameters.AddWithValue("@Email", clientDto.Email);
            insertClientCommand.Parameters.AddWithValue("@Telephone", clientDto.Telephone);
            insertClientCommand.Parameters.AddWithValue("@Pesel", clientDto.Pesel);
            
            return Convert.ToInt32(await insertClientCommand.ExecuteScalarAsync());
        }
    }

    public async Task<List<ClientTripDto>> RegisterClientForTrip(int id, int tripId)
    {
        // used to check if client exists
        string clientExistsQuery = "SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient";
        // used to check if trip exists
        string tripExistsQuery = "SELECT COUNT(*) FROM Trip WHERE IdTrip = @IdTrip";
        // used to check if new client can be registered for trip
        string maxPeopleNotReachedQuery =
            "SELECT CASE WHEN COUNT(*) < t.MaxPeople THEN 1 ELSE 0 END FROM Client_Trip ct LEFT JOIN Trip t ON ct.IdTrip = t.IdTrip WHERE t.IdTrip = @IdTrip GROUP BY t.MaxPeople;";
        // registers client for trip
        string insertClientTripQuery =
            "INSERT INTO Client_Trip (IdClient, IdTrip, RegisteredAt) VALUES (@IdClient, @IdTrip, @RegisteredAt)";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();
            
            using (SqlCommand clientExistsCommand = new SqlCommand(clientExistsQuery, conn))
            {
                clientExistsCommand.Parameters.AddWithValue("@IdClient", id);
                
                int count = (int) await clientExistsCommand.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new ClientNotFoundException();
                }
            }

            using (SqlCommand tripExistsCommand = new SqlCommand(tripExistsQuery, conn))
            {
                tripExistsCommand.Parameters.AddWithValue("@IdTrip", tripId);

                int count = (int) await tripExistsCommand.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new TripNotFoundException();
                }
            }

            using (SqlCommand maxPeopleNotReachedCommand = new SqlCommand(maxPeopleNotReachedQuery, conn))
            {
                maxPeopleNotReachedCommand.Parameters.AddWithValue("@IdTrip", tripId);
                
                bool maxPeopleNotReached = Convert.ToBoolean((int) await maxPeopleNotReachedCommand.ExecuteScalarAsync());
                if (!maxPeopleNotReached)
                {
                    throw new MaxPeopleReachedException();
                }
            }

            using (SqlCommand insertClientTripCommand = new SqlCommand(insertClientTripQuery, conn))
            {
                insertClientTripCommand.Parameters.AddWithValue("@IdClient", id);
                insertClientTripCommand.Parameters.AddWithValue("@IdTrip", tripId);
                insertClientTripCommand.Parameters.AddWithValue("@RegisteredAt", Int32.Parse(DateTime.Now.ToString("yyyyMMdd")));

                try
                {
                    await insertClientTripCommand.ExecuteNonQueryAsync();
                }
                catch (SqlException ex)
                {
                    if (ex.Number == 2627)
                    {
                        throw new ClientAlreadyRegisteredForTripException();
                    }
                    else
                    {
                        throw ex;
                    }
                } 
            }
        }

        return await GetClientTrips(id);
    }

    public async Task<List<ClientTripDto>> DeleteClientRegistration(int id, int tripId)
    {
        // used to check if client exists
        string clientExistsQuery = "SELECT COUNT(*) FROM Client WHERE IdClient = @IdClient";
        // used to check if trip exists
        string tripExistsQuery = "SELECT COUNT(*) FROM Trip WHERE IdTrip = @IdTrip";
        // deletes client's registration for trip 
        string deleteClientTripRegistrationQuery =
            "DELETE FROM Client_Trip WHERE IdClient = @IdClient AND IdTrip = @IdTrip";

        using (SqlConnection conn = new SqlConnection(_connectionString))
        {
            await conn.OpenAsync();

            using (SqlCommand clientExistsCommand = new SqlCommand(clientExistsQuery, conn))
            {
                clientExistsCommand.Parameters.AddWithValue("@IdClient", id);
                
                int count = (int) await clientExistsCommand.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new ClientNotFoundException();
                }
            }
            
            using (SqlCommand tripExistsCommand = new SqlCommand(tripExistsQuery, conn))
            {
                tripExistsCommand.Parameters.AddWithValue("@IdTrip", tripId);

                int count = (int) await tripExistsCommand.ExecuteScalarAsync();
                if (count == 0)
                {
                    throw new TripNotFoundException();
                }
            }
            
            using (SqlCommand deleteClientTripRegistrationCommand = new SqlCommand(deleteClientTripRegistrationQuery, conn))
            {
                deleteClientTripRegistrationCommand.Parameters.AddWithValue("@IdClient", id);
                deleteClientTripRegistrationCommand.Parameters.AddWithValue("@IdTrip", tripId);

                int rowsAffected = await deleteClientTripRegistrationCommand.ExecuteNonQueryAsync();
                if (rowsAffected == 0)
                {
                    throw new ClientNotRegisteredForTripException();
                }
            }
        }
        
        return await GetClientTrips(id);
    }
}