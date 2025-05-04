namespace Tutorial8.Models.DTOs;

public class TripDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DateTime StartDateTime { get; set; }
    public DateTime EndDateTime { get; set; }
    public int MaxPeople { get; set; }
    public List<CountryDTO> Countries { get; set; } = new List<CountryDTO>();

    public override bool Equals(object? obj)
    {
        if (obj is TripDTO tripDto) return Id == tripDto.Id;
        return false;
    }
}

public class CountryDTO
{
    public int Id { get; set; }
    public string Name { get; set; }
}