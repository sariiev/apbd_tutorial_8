namespace Tutorial8.Models.DTOs;

public class ClientTripDto
{
    public TripDTO Trip { get; set; }
    public DateTime RegistrationDate { get; set; }
    public  DateTime? PaymentDate { get; set; }
}