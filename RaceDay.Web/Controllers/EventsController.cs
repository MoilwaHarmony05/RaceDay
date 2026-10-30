using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using RaceDay.Web.Models;

namespace RaceDay.Web.Controllers;

public class EventsController : Controller
{
    private readonly IHttpClientFactory _http;

    public EventsController(IHttpClientFactory http) => _http = http;

    public async Task<IActionResult> Index()
    {
        var client = _http.CreateClient("RaceDayApi");
        var events = await client.GetFromJsonAsync<List<EventViewModel>>("api/events")
                     ?? new List<EventViewModel>();
        return View(events);
    }
}