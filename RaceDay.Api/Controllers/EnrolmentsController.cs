using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/enrolments")]
public class EnrolmentsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EnrolmentsController(RaceDayDbContext db)
    {
        _db = db;
    }

    public record CreateEnrolmentRequest(int CategoryId);

    [HttpPost]
    public async Task<IActionResult> Create(CreateEnrolmentRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        var participantId = HttpContext.Session.GetInt32("ParticipantId");
        if (participantId is null) return Unauthorized();

        var category = await _db.Categories.FindAsync(request.CategoryId);
        if (category is null) return NotFound(new { message = "Category not found" });

        var already = await _db.Enrolments.AnyAsync(e =>
            e.ParticipantId == participantId && e.CategoryId == request.CategoryId);
        if (already) return Conflict(new { message = "Already enrolled" });

        var enrolment = new Enrolment
        {
            ParticipantId = participantId.Value,
            CategoryId = request.CategoryId,
            EnrolmentDate = DateTime.UtcNow,
            Status = "Confirmed"
        };

        _db.Enrolments.Add(enrolment);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new { enrolment.EnrolmentId, enrolment.CategoryId, enrolment.Status });
    }

    [HttpGet("me")]
    public async Task<IActionResult> MyEnrolments()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return Forbid();

        var participantId = HttpContext.Session.GetInt32("ParticipantId");
        var list = await _db.Enrolments
            .Where(e => e.ParticipantId == participantId)
            .Select(e => new { e.EnrolmentId, e.CategoryId, e.Status, e.EnrolmentDate })
            .ToListAsync();

        return Ok(list);
    }
}