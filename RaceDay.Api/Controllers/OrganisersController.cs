using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/organisers")]
public class OrganisersController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public OrganisersController(RaceDayDbContext db)
    {
        _db = db;
    }

    public record UpdateOrganiserRequest(string? FirstName, string? LastName, string? Phone, DateTime? DateOfBirth);

    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var id = HttpContext.Session.GetInt32("OrganiserId");
        var organiser = await _db.Organisers.FindAsync(id);
        if (organiser is null) return Unauthorized();

        return Ok(new { organiser.OrganiserId, organiser.Email, organiser.FirstName, organiser.LastName, organiser.Phone, organiser.DateOfBirth });
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateMe(UpdateOrganiserRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var id = HttpContext.Session.GetInt32("OrganiserId");
        var organiser = await _db.Organisers.FindAsync(id);
        if (organiser is null) return Unauthorized();

        organiser.FirstName = request.FirstName ?? organiser.FirstName;
        organiser.LastName = request.LastName ?? organiser.LastName;
        organiser.Phone = request.Phone ?? organiser.Phone;
        organiser.DateOfBirth = request.DateOfBirth ?? organiser.DateOfBirth;

        await _db.SaveChangesAsync();
        return Ok(new { organiser.OrganiserId, organiser.Email, organiser.FirstName, organiser.LastName, organiser.Phone, organiser.DateOfBirth });
    }
}