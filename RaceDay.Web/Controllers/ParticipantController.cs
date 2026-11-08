using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

public class ParticipantController : Controller
{
    private readonly IHttpClientFactory _http;
    public ParticipantController(IHttpClientFactory http) => _http = http;

    public record EnrolmentViewModel(int EnrolmentId, int CategoryId, string Status, DateTime EnrolmentDate);
    public record CategoryViewModel(int CategoryId, string Name, decimal DistanceKm, decimal EntryFee);

    public async Task<IActionResult> Events()
    {
        var client = _http.CreateClient("RaceDayApi");
        var events = await client.GetFromJsonAsync<List<EventViewModel>>("api/events")
                     ?? new List<EventViewModel>();
        return View(events);
    }

    public async Task<IActionResult> Details(int id)
    {
        var client = _http.CreateClient("RaceDayApi");
        var response = await client.GetAsync($"api/events/{id}");
        if (!response.IsSuccessStatusCode) return NotFound();
        var json = await response.Content.ReadAsStringAsync();
        ViewBag.Json = json;
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Enrol(int categoryId)
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _http.CreateClient("RaceDayApi");
        var participantId = HttpContext.Session.GetInt32("ParticipantId");
        var response = await client.PostAsJsonAsync("api/enrolments", new { categoryId, participantId });

        if (response.IsSuccessStatusCode)
            TempData["Message"] = "Enrolled successfully.";
        else
            TempData["Message"] = "Could not enrol. You may already be enrolled.";

        return RedirectToAction("MyEnrolments");
    }

    public async Task<IActionResult> MyEnrolments()
    {
        if (HttpContext.Session.GetString("Role") != "Participant")
            return RedirectToAction("Login", "Account");

        var client = _http.CreateClient("RaceDayApi");
        var list = await client.GetFromJsonAsync<List<EnrolmentViewModel>>("api/enrolments/me")
                   ?? new List<EnrolmentViewModel>();
        return View(list);
    }
}