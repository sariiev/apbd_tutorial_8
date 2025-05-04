using Microsoft.Data.SqlClient;
using Tutorial8.Models.DTOs;

namespace Tutorial8.Services;

public class TripsService : ITripsService
{
    private readonly string _connectionString = "Data Source=localhost, 1433; User=SA; Password=APBD_tutorial_8; Initial Catalog=apbd; Integrated Security=False;Connect Timeout=30;Encrypt=False;Trust Server Certificate=False";
    
    public async Task<List<TripDTO>> GetTrips()
    {
        var tripsDict = new Dictionary<int, TripDTO>();
        var countriesDict = new Dictionary<int, CountryDTO>();

        // retrieves all trips with corresponding countries
        string tripsQuery = "SELECT t.IdTrip, t.Name as TripName, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name as CountryName FROM Trip t JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON ct.IdCountry = c.IdCountry";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(tripsQuery, conn))
        {
            await conn.OpenAsync();

            using (var reader = await cmd.ExecuteReaderAsync())
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
                        
                        tripsDict.Add(tripId, new TripDTO()
                        {
                            Id = tripId,
                            Name = reader.GetString(tripNameOrdinal),
                            Description = reader.GetString(tripDescriptionOrdinal),
                            StartDateTime = reader.GetDateTime(tripDateFromOrdinal),
                            EndDateTime = reader.GetDateTime(tripDateToOrdinal),
                            MaxPeople = reader.GetInt32(tripMaxPeopleOrdinal)
                        });   
                    }
                    
                    tripsDict[tripId].Countries.Add(countriesDict[countryId]);
                }
            }
        }
        

        return tripsDict.Values.ToList();
    }

    public async Task<TripDTO?> GetTripById(int id)
    {
        // retrieves a trip and corresponding countries by trip id
        string tripQuery = "SELECT t.IdTrip, t.Name as TripName, t.Description, t.DateFrom, t.DateTo, t.MaxPeople, c.IdCountry, c.Name as CountryName FROM Trip t JOIN Country_Trip ct ON t.IdTrip = ct.IdTrip JOIN Country c ON ct.IdCountry = c.IdCountry WHERE t.IdTrip = @IdTrip";
        
        using (SqlConnection conn = new SqlConnection(_connectionString))
        using (SqlCommand cmd = new SqlCommand(tripQuery, conn))
        {
            cmd.Parameters.AddWithValue("@IdTrip", id);
            
            await conn.OpenAsync();
            
            using (var reader = await cmd.ExecuteReaderAsync())
            {
                var trip = (TripDTO)null;
                while (await reader.ReadAsync())
                {
                    if (trip == null)
                    {
                        int tripNameOrdinal = reader.GetOrdinal("TripName");
                        int tripDescriptionOrdinal = reader.GetOrdinal("Description");
                        int tripDateFromOrdinal = reader.GetOrdinal("DateFrom");
                        int tripDateToOrdinal = reader.GetOrdinal("DateTo");
                        int tripMaxPeopleOrdinal = reader.GetOrdinal("MaxPeople");
                        
                        trip = new TripDTO()
                        {
                            Id = id,
                            Name = reader.GetString(tripNameOrdinal),
                            Description = reader.GetString(tripDescriptionOrdinal),
                            StartDateTime = reader.GetDateTime(tripDateFromOrdinal),
                            EndDateTime = reader.GetDateTime(tripDateToOrdinal),
                            MaxPeople = reader.GetInt32(tripMaxPeopleOrdinal)
                        };
                    }
                    
                    int countryIdOrdinal = reader.GetOrdinal("IdCountry");
                    int countryNameOrdinal = reader.GetOrdinal("CountryName");
                    
                    trip.Countries.Add(new CountryDTO()
                    {
                        Id = reader.GetInt32(countryIdOrdinal),
                        Name = reader.GetString(countryNameOrdinal)
                    });
                }

                return trip;
            }
        }
    }
}