using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Controllers;

[ApiController]
[Route("api")]
public class CategoriesController : ControllerBase
{
    private readonly RaceDayDbContext _db;

    public CategoriesController(RaceDayDbContext db)
    {
        _db = db;
    }

    public record CreateCategoryRequest(string Name, decimal DistanceKm, int? MaxParticipants, decimal EntryFee, TimeSpan? StartTime);
    public record UpdateCategoryRequest(string? Name, decimal? DistanceKm, int? MaxParticipants, decimal? EntryFee, TimeSpan? StartTime);

    [HttpGet("events/{eventId:int}/categories")]
    public async Task<IActionResult> GetByEvent(int eventId)
    {
        var exists = await _db.Events.AnyAsync(e => e.EventId == eventId);
        if (!exists) return NotFound();

        var categories = await _db.Categories
            .Where(c => c.EventId == eventId)
            .Select(c => new { c.CategoryId, c.Name, c.DistanceKm, c.MaxParticipants, c.EntryFee, c.StartTime })
            .ToListAsync();

        return Ok(categories);
    }

    [HttpPost("events/{eventId:int}/categories")]
    public async Task<IActionResult> Create(int eventId, CreateCategoryRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var ev = await _db.Events.FindAsync(eventId);
        if (ev is null) return NotFound();
        if (ev.OrganiserId != organiserId) return Forbid();

        var category = new Category
        {
            EventId = eventId,
            Name = request.Name,
            DistanceKm = request.DistanceKm,
            MaxParticipants = request.MaxParticipants,
            EntryFee = request.EntryFee,
            StartTime = request.StartTime
        };

        _db.Categories.Add(category);
        await _db.SaveChangesAsync();

        return Created(string.Empty, new { category.CategoryId, category.Name, category.DistanceKm, category.EntryFee });
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateCategoryRequest request)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var category = await _db.Categories.Include(c => c.Event).FirstOrDefaultAsync(c => c.CategoryId == id);
        if (category is null) return NotFound();
        if (category.Event.OrganiserId != organiserId) return Forbid();

        category.Name = request.Name ?? category.Name;
        category.DistanceKm = request.DistanceKm ?? category.DistanceKm;
        category.MaxParticipants = request.MaxParticipants ?? category.MaxParticipants;
        category.EntryFee = request.EntryFee ?? category.EntryFee;
        category.StartTime = request.StartTime ?? category.StartTime;

        await _db.SaveChangesAsync();
        return Ok(new { category.CategoryId, category.Name, category.EntryFee });
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return Forbid();

        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var category = await _db.Categories
            .Include(c => c.Event)
            .Include(c => c.Enrolments)
            .FirstOrDefaultAsync(c => c.CategoryId == id);

        if (category is null) return NotFound();
        if (category.Event.OrganiserId != organiserId) return Forbid();
        if (category.Enrolments.Count > 0) return Conflict(new { message = "Category has enrolments" });

        _db.Categories.Remove(category);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}