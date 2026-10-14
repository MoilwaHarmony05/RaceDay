using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public class EventsControllerTests
{
    private static RaceDayDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new RaceDayDbContext(options);
    }

    private static void SignInAsOrganiser(ControllerBase controller, int organiserId)
    {
        var http = new DefaultHttpContext();
        http.Session = new TestSession();
        http.Session.SetString("Role", "Organiser");
        http.Session.SetInt32("OrganiserId", organiserId);
        controller.ControllerContext = new ControllerContext { HttpContext = http };
    }

    [Fact]
    public async Task GetAll_ReturnsEvents()
    {
        await using var db = CreateDb();
        db.Events.Add(new Event
        {
            OrganiserId = 1,
            Name = "Soweto Marathon",
            Location = "Soweto",
            EventDate = DateTime.UtcNow,
            RegistrationDeadline = DateTime.UtcNow,
            EventType = "Run"
        });
        await db.SaveChangesAsync();

        var controller = new EventsController(db);
        var result = await controller.GetAll();

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.NotNull(ok.Value);
    }

    [Fact]
    public async Task Create_ReturnsCreated_WhenOrganiserIsLoggedIn()
    {
        await using var db = CreateDb();
        var controller = new EventsController(db);
        SignInAsOrganiser(controller, 1);

        var request = new EventsController.CreateEventRequest(
            "Soweto Marathon", "Annual race", "Soweto",
            DateTime.UtcNow.AddDays(30), DateTime.UtcNow.AddDays(10),
            42.2m, "Run", "Published");

        var result = await controller.Create(request);

        Assert.IsType<CreatedAtActionResult>(result);
        Assert.Equal(1, await db.Events.CountAsync());
    }

    [Fact]
    public async Task Create_ReturnsForbid_WhenNotOrganiser()
    {
        await using var db = CreateDb();
        var controller = new EventsController(db);

        var http = new DefaultHttpContext();
        http.Session = new TestSession();
        controller.ControllerContext = new ControllerContext { HttpContext = http };

        var request = new EventsController.CreateEventRequest(
            "Soweto Marathon", null, "Soweto",
            DateTime.UtcNow, DateTime.UtcNow, 10, "Run", null);

        var result = await controller.Create(request);

        Assert.IsType<ForbidResult>(result);
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