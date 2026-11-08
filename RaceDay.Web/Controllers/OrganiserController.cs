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
        var events = await client.GetFromJsonAsync<List<EventViewModel>>("api/events/organiser/me")
                     ?? new List<EventViewModel>();
        return View(events);
    }

    [HttpGet]
    public IActionResult Create()
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(string name, string description, string location, DateTime eventDate, DateTime registrationDeadline, decimal distance, string eventType)
    {
        if (HttpContext.Session.GetString("Role") != "Organiser")
            return RedirectToAction("Login", "Account");

        var client = _http.CreateClient("RaceDayApi");
        var organiserId = HttpContext.Session.GetInt32("OrganiserId");
        var response = await client.PostAsJsonAsync("api/events", new
        {
            name,
            description,
            location,
            eventDate,
            registrationDeadline,
            distance,
            eventType,
            status = "Published",
            organiserId
        });

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Could not create event. Make sure you are logged in on the API side too, or try again.";
            return View();
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