using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.Api.Controllers;
using RaceDay.Api.Data;
using RaceDay.Api.Models;

namespace RaceDay.Api.Tests;

public class EnrolmentsControllerTests
{
    private static RaceDayDbContext CreateDb()
    {
        var options = new DbContextOptionsBuilder<RaceDayDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new RaceDayDbContext(options);
    }

    private static void SignInAsParticipant(ControllerBase controller, int participantId)
    {
        var http = new DefaultHttpContext();
        http.Session = new TestSession();
        http.Session.SetString("Role", "Participant");
        http.Session.SetInt32("ParticipantId", participantId);
        controller.ControllerContext = new ControllerContext { HttpContext = http };
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
    public async Task Create_RecordsEnrolment_WhenParticipant()
    {
        await using var db = CreateDb();
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "42km", DistanceKm = 42.2m, EntryFee = 350 });
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        SignInAsParticipant(controller, 5);

        var result = await controller.Create(new EnrolmentsController.CreateEnrolmentRequest(1));

        Assert.IsType<CreatedResult>(result);
        Assert.Equal(1, await db.Enrolments.CountAsync());
        Assert.Equal(5, (await db.Enrolments.FirstAsync()).ParticipantId);
    }

    [Fact]
    public async Task Create_ReturnsForbid_WhenOrganiserTriesToEnrol()
    {
        await using var db = CreateDb();
        db.Categories.Add(new Category { CategoryId = 1, EventId = 1, Name = "42km", DistanceKm = 42.2m, EntryFee = 350 });
        await db.SaveChangesAsync();

        var controller = new EnrolmentsController(db);
        SignInAsOrganiser(controller, 1);

        var result = await controller.Create(new EnrolmentsController.CreateEnrolmentRequest(1));

        Assert.IsType<ForbidResult>(result);
        Assert.Equal(0, await db.Enrolments.CountAsync());
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