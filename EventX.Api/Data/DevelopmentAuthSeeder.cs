using EventX.Api.Entities;
using EventX.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Data;

public static class DevelopmentAuthSeeder
{
    private const string OrganizerName = "arthur";
    private const string OrganizerEmail = "arthur@gmail.com";
    private const string OrganizerPassword = "123456A-r";

    public static async Task SeedOrganizerAsync(
        AppDbContext dbContext,
        IPasswordHasherService passwordHasherService,
        CancellationToken cancellationToken = default)
    {
        var normalizedEmail = OrganizerEmail.Trim().ToLowerInvariant();

        var user = await dbContext.Users
            .FirstOrDefaultAsync(x => x.Email.ToLower() == normalizedEmail, cancellationToken);

        if (user is null)
        {
            user = new User
            {
                FullName = OrganizerName,
                Email = normalizedEmail,
                UserType = UserType.Organizer,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            dbContext.Users.Add(user);
        }
        else
        {
            user.FullName = OrganizerName;
            user.UserType = UserType.Organizer;
            user.IsActive = true;
        }

        user.PasswordHash = passwordHasherService.HashPassword(OrganizerPassword);
        await dbContext.SaveChangesAsync(cancellationToken);
        await SeedEventsAsync(dbContext, user.Id, cancellationToken);
    }

    private static async Task SeedEventsAsync(
        AppDbContext dbContext,
        Guid organizerId,
        CancellationToken cancellationToken)
    {
        var hasEvents = await dbContext.Events
            .AnyAsync(x => x.OrganizerId == organizerId, cancellationToken);

        if (hasEvents)
        {
            return;
        }

        var now = DateTime.UtcNow;
        var seededEvents = new[]
        {
            new Event
            {
                Name = "EventX Summit 2026",
                Type = "Corporativo",
                Description = "Encontro anual de organizadores e parceiros.",
                StartDate = now.AddDays(15).Date.AddHours(14),
                EndDate = now.AddDays(15).Date.AddHours(20),
                Location = "Sao Paulo - SP",
                CoverImageUrl = "https://images.unsplash.com/photo-1492684223066-81342ee5ff30",
                EstimatedAudience = 350,
                EstimatedCost = 25000m,
                Status = EventStatus.Published,
                OrganizerId = organizerId,
                CreatedAt = now
            },
            new Event
            {
                Name = "Workshop de Planejamento EventX",
                Type = "Workshop",
                Description = "Workshop pratico para planejamento de eventos.",
                StartDate = now.AddDays(30).Date.AddHours(9),
                EndDate = now.AddDays(30).Date.AddHours(13),
                Location = "Campinas - SP",
                CoverImageUrl = "https://images.unsplash.com/photo-1511578314322-379afb476865",
                EstimatedAudience = 80,
                EstimatedCost = 8000m,
                Status = EventStatus.Draft,
                OrganizerId = organizerId,
                CreatedAt = now
            }
        };

        dbContext.Events.AddRange(seededEvents);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
