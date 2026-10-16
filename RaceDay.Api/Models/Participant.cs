namespace RaceDay.Api.Models;

public class Participant
{
    public int ParticipantId { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime? DateOfBirth { get; set; }

    public List<Enrolment> Enrolments { get; set; } = new();
}