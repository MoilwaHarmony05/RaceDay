using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;

namespace RaceDay.Api.Tests;

public class AuthControllerTests
{
    private static RaceDayDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    private static void AttachSession(ControllerBase controller)
    {
        var http = new DefaultHttpContext();
        http.Session = new TestSession();
        controller.ControllerContext = new ControllerContext { HttpContext = http };
    }

    [Fact]
    public async Task RegisterOrganiser_CreatesOrganiser()
    {
        await using var db = CreateDb();
        var controller = new AuthController(db);

        var request = new AuthController.RegisterRequest(
            "org@raceday.co.za", "Password123!", "Thabo", "Mokoena", "0821234567", new DateTime(1990, 5, 15));

        var result = await controller.RegisterOrganiser(request);

        Assert.IsType<CreatedResult>(result);
        Assert.Equal(1, await db.Organisers.CountAsync());
    }

    [Fact]
    public async Task LoginOrganiser_ReturnsOk_WhenPasswordMatches()
    {
        await using var db = CreateDb();
        var controller = new AuthController(db);
        AttachSession(controller);

        var hash = BCrypt.Net.BCrypt.HashPassword("Password123!");
        db.Organisers.Add(new Models.Organiser
        {
            Email = "org@raceday.co.za",
            PasswordHash = hash,
            FirstName = "Thabo",
            LastName = "Mokoena"
        });
        await db.SaveChangesAsync();

        var result = await controller.LoginOrganiser(
            new AuthController.LoginRequest("org@raceday.co.za", "Password123!"));

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task LoginOrganiser_ReturnsUnauthorized_WhenPasswordIsWrong()
    {
        await using var db = CreateDb();
        var controller = new AuthController(db);
        AttachSession(controller);

        db.Organisers.Add(new Models.Organiser
        {
            Email = "org@raceday.co.za",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Password123!"),
            FirstName = "Thabo",
            LastName = "Mokoena"
        });
        await db.SaveChangesAsync();

        var result = await controller.LoginOrganiser(
            new AuthController.LoginRequest("org@raceday.co.za", "WrongPassword"));

        Assert.IsType<UnauthorizedObjectResult>(result);
    }

    private sealed class TestSession : ISession
    {
        private readonly Dictionary<string, byte[]> _store = new();
        public IEnumerable<string> Keys => _store.Keys;
        public string Id => "test";
        public bool IsAvailable => true;
        public void Clear() => _store.Clear();
        public Task CommitAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public Task LoadAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public void Remove(string key) => _store.Remove(key);
        public void Set(string key, byte[] value) => _store[key] = value;
        public bool TryGetValue(string key, out byte[] value) => _store.TryGetValue(key, out value!);
    }
}