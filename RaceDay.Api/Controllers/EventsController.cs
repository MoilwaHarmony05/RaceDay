using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api/events")]
public class EventsController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public EventsController(RaceDayDbContext db)
    {
        _db = db;
    }

    public record CreateEventRequest(
        string Name,
        string? Description,
        string Location,
        DateTime EventDate,
        DateTime RegistrationDeadline,
        decimal Distance,
        string EventType,
        string? Status);

    public record UpdateEventRequest(
        string? Name,
        string? Description,
        string? Location,
        DateTime? EventDate,
        DateTime? RegistrationDeadline,
        decimal? Distance,
        string? EventType,
        string? Status);

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var events = await _db.Events
            .Select(e => new { e.EventId, e.Name, e.Location, e.EventDate, e.EventType, e.Status })
            .ToListAsync();

        return Ok(events);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var ev = await _db.Events
            .Include(e => e.Categories)
            .FirstOrDefaultAsync(e => e.EventId == id);

        if (ev is null) return NotFound();

        return Ok(new
        {
            ev.EventId,
            ev.Name,
            ev.Description,
            ev.Location,
            ev.EventDate,
            ev.RegistrationDeadline,
            ev.Distance,
            ev.EventType,
            ev.Status,
            Categories = ev.Categories.Select(c => new { c.CategoryId, c.Name, c.DistanceKm, c.EntryFee })
        });
    }

    [HttpPost]
    public async Task<IActionResult> Create(CreateEventRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        if (organiserId is null) return Unauthorized();

        var ev = new Event
        {
            OrganiserId = organiserId.Value,
            Name = request.Name,
            Description = request.Description,
            Location = request.Location,
            EventDate = request.EventDate,
            RegistrationDeadline = request.RegistrationDeadline,
            Distance = request.Distance,
            EventType = request.EventType,
            Status = request.Status ?? "Draft"
        };

        _db.Events.Add(ev);
        await _db.SaveChangesAsync();

        return CreatedAtAction(nameof(GetById), new { id = ev.EventId }, new { ev.EventId, ev.Name, ev.Status });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateEventRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != organiserId) return Forbid();

        ev.Name = request.Name ?? ev.Name;
        ev.Description = request.Description ?? ev.Description;
        ev.Location = request.Location ?? ev.Location;
        ev.EventDate = request.EventDate ?? ev.EventDate;
        ev.RegistrationDeadline = request.RegistrationDeadline ?? ev.RegistrationDeadline;
        ev.Distance = request.Distance ?? ev.Distance;
        ev.EventType = request.EventType ?? ev.EventType;
        ev.Status = request.Status ?? ev.Status;

        await _db.SaveChangesAsync();
        return Ok(new { ev.EventId, ev.Name, ev.Status });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var ev = await _db.Events.FindAsync(id);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != organiserId) return Forbid();

        _db.Events.Remove(ev);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpGet("organiser/me")]
    public async Task<IActionResult> MyEvents()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var events = await _db.Events
            .Where(e => e.OrganiserId == organiserId)
            .Select(e => new { e.EventId, e.Name, e.EventDate, e.Status })
            .ToListAsync();

        return Ok(events);
    }
}