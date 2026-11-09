using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

public class OrganiserController : Controller
{
    private readonly IHttpClientFactory _http;
    public OrganiserController(IHttpClientFactory http) => _http = http;

    public async Task<IActionResult> Dashboard()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _http.CreateClient("RaceDayApi");
        var organiserId = HttpContext.Session.GetInt32("OrganiserId");

        // We pass the organiserId to the API so it knows who we are!
        var response = await client.GetAsync($"api/events/organiser/me?organiserId={organiserId}");

        // If the API crashes or returns an error, we catch it gracefully instead of showing a 500 page
        if (!response.IsSuccessStatusCode)
        {
            TempData["Error"] = "Could not load events from the API. Please check if the API is running.";
            return View(new List<EventViewModel>());
        }

        var events = await response.Content.ReadFromJsonAsync<List<EventViewModel>>() ?? new List<EventViewModel>();
        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");
        return View(new CreateEventDto());
    }

    [HttpPost]
    
    public async Task<IActionResult> Create(CreateEventDto model)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        // If the browser missed something (like a required field)
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var client = _http.CreateClient("RaceDayApi");
        var organiserId = HttpContext.Session.GetInt32("OrganiserId");

        var response = await client.PostAsJsonAsync("api/events", new
        {
            name = model.Name,
            description = model.Description,
            location = model.Location,
            eventDate = model.EventDate,
            registrationDeadline = model.RegistrationDeadline,
            distance = model.Distance,
            eventType = model.EventType,
            status = "Published",
            organiserId
        });

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not create event. Ensure the API is running and you are authenticated.";
            return View(model);
        }

        return RedirectToAction("Dashboard");
    }

    [HttpPost]
    public async Task<IActionResult> UploadBanner(int id, IFormFile file)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        if (file is null || file.Length == 0)
        {
            TempData["Message"] = "No file selected.";
            return RedirectToAction("Dashboard");
        }

        var client = _http.CreateClient("RaceDayApi");
        using var content = new MultipartFormDataContent();
        using var stream = file.OpenReadStream();
        content.Add(new StreamContent(stream), "file", file.FileName);

        var response = await client.PostAsync($"api/events/{id}/banner", content);
        TempData["Message"] = response.IsSuccessStatusCode
            ? "Banner uploaded."
            : "Upload failed. Make sure the event belongs to you.";

        return RedirectToAction("Dashboard");
    }
}

// This class is the "bucket" that catches the form data perfectly
public class CreateEventDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Location { get; set; } = string.Empty;
    public DateTime EventDate { get; set; }
    public DateTime RegistrationDeadline { get; set; }
    public decimal Distance { get; set; }
    public string EventType { get; set; } = string.Empty;
}