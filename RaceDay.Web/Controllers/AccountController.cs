using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;

namespace RaceDay.Web.Controllers;

public class AccountController : Controller
{
    private readonly IHttpClientFactory _http;
    public AccountController(IHttpClientFactory http) => _http = http;

    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string Email, string Password, string FirstName, string LastName, string? Phone, DateTime? DateOfBirth);
    public record LoginResponse(int? OrganiserId, int? ParticipantId, string Email, string Role);

    [HttpGet]
    public IActionResult Login() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string email, string password, string role)
    {
        var client = _http.CreateClient("RaceDayApi");
        var endpoint = role == "Organiser" ? "api/auth/organiser/login" : "api/auth/participant/login";
        var response = await client.PostAsJsonAsync(endpoint, new LoginRequest(email, password));

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Invalid email or password.";
            return View();
        }

        var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
        HttpContext.Session.SetString("Role", result!.Role);
        HttpContext.Session.SetString("Email", result.Email);
        if (result.OrganiserId.HasValue)
            HttpContext.Session.SetInt32("OrganiserId", result.OrganiserId.Value);
        if (result.ParticipantId.HasValue)
            HttpContext.Session.SetInt32("ParticipantId", result.ParticipantId.Value);

        return RedirectToAction("Index", "Home");
    }

    [HttpGet]
    public IActionResult Register() => View();

    [HttpPost]
    public async Task<IActionResult> Register(string email, string password, string firstName, string lastName, string role)
    {
        var client = _http.CreateClient("RaceDayApi");
        var endpoint = role == "Organiser" ? "api/auth/organiser/register" : "api/auth/participant/register";
        var response = await client.PostAsJsonAsync(endpoint, new RegisterRequest(email, password, firstName, lastName, null, null));

        if (!response.IsSuccessStatusCode)
        {
            ViewBag.Error = "Registration failed. Email may already be in use.";
            return View();
        }

        return RedirectToAction("Login");
    }

    public IActionResult Logout()
    {
        HttpContext.Session.Clear();
        return RedirectToAction("Index", "Home");
    }
}