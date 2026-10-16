using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/participants")]
public class ParticipantsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public ParticipantsController(RaceDayDbContext db)
    {
        _db = db;
    }

    public record UpdateParticipantRequest(string? FirstName, string? LastName, string? Phone, DateTime? DateOfBirth);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        var id = HttpContext.Session.GetInt32("ParticipantId");
        var participant = await _db.Participants.FindAsync(id);
        if (participant is null) return Unauthorized();

        return Ok(new { participant.ParticipantId, participant.Email, participant.FirstName, participant.LastName, participant.Phone, participant.DateOfBirth });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateParticipantRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        var id = HttpContext.Session.GetInt32("ParticipantId");
        var participant = await _db.Participants.FindAsync(id);
        if (participant is null) return Unauthorized();

        participant.FirstName = request.FirstName ?? participant.FirstName;
        participant.LastName = request.LastName ?? participant.LastName;
        participant.Phone = request.Phone ?? participant.Phone;
        participant.DateOfBirth = request.DateOfBirth ?? participant.DateOfBirth;

        await _db.SaveChangesAsync();
        return Ok(new { participant.ParticipantId, participant.Email, participant.FirstName, participant.LastName, participant.Phone, participant.DateOfBirth });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var participant = await _db.Participants.FindAsync(id);
        if (participant is null) return NotFound();

        return Ok(new { participant.ParticipantId, participant.FirstName, participant.LastName });
    }
}