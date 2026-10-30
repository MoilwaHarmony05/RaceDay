using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public AuthController(RaceDayDbContext db) => _db = db;

    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Phone, DateTime? DateOfBirth);
    public record LoginRequest(string Email, string Password);

    [HttpPost("organiser/register")]
    public async Task<IActionResult> RegisterOrganiser(RegisterRequest request)
    {
        if (await _db.Organisers.AnyAsync(o => o.Email == request.Email))
            return Conflict(new { message = "Email already registered" });

        var organiser = new Organiser
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth
        };

        _db.Organisers.Add(organiser);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { organiser.OrganiserId, organiser.Email });
    }

    [HttpPost("participant/register")]
    public async Task<IActionResult> RegisterParticipant(RegisterRequest request)
    {
        if (await _db.Participants.AnyAsync(p => p.Email == request.Email))
            return Conflict(new { message = "Email already registered" });

        var participant = new Participant
        {
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FirstName = request.FirstName,
            LastName = request.LastName,
            Phone = request.Phone,
            DateOfBirth = request.DateOfBirth
        };

        _db.Participants.Add(participant);
        await _db.SaveChangesAsync();
        return Created(string.Empty, new { participant.ParticipantId, participant.Email });
    }

    [HttpPost("organiser/login")]
    public async Task<IActionResult> LoginOrganiser(LoginRequest request)
    {
        var organiser = await _db.Organisers.FirstOrDefaultAsync(o => o.Email == request.Email);
        if (organiser is null || !BCrypt.Net.BCrypt.Verify(request.Password, organiser.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        HttpContext.Session.SetString("Role", "Organiser");
        HttpContext.Session.SetInt32("OrganiserId", organiser.OrganiserId);
        return Ok(new { organiser.OrganiserId, organiser.Email, role = "Organiser" });
    }

    [HttpPost("participant/login")]
    public async Task<IActionResult> LoginParticipant(LoginRequest request)
    {
        var participant = await _db.Participants.FirstOrDefaultAsync(p => p.Email == request.Email);
        if (participant is null || !BCrypt.Net.BCrypt.Verify(request.Password, participant.PasswordHash))
            return Unauthorized(new { message = "Invalid email or password" });

        HttpContext.Session.SetString("Role", "Participant");
        HttpContext.Session.SetInt32("ParticipantId", participant.ParticipantId);
        return Ok(new { participant.ParticipantId, participant.Email, role = "Participant" });
    }
}