using EventX.Api.Entities;
using Microsoft.EntityFrameworkCore;

namespace EventX.Api.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<SocialProfile> SocialProfiles => Set<SocialProfile>();
    public DbSet<Post> Posts => Set<Post>();
    public DbSet<Story> Stories => Set<Story>();
    public DbSet<StoryView> StoryViews => Set<StoryView>();
    public DbSet<SocialFollow> SocialFollows => Set<SocialFollow>();
    public DbSet<Like> Likes => Set<Like>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<Highlight> Highlights => Set<Highlight>();
    public DbSet<InvitationTemplate> InvitationTemplates => Set<InvitationTemplate>();
    public DbSet<InvitationDraft> InvitationDrafts => Set<InvitationDraft>();
    public DbSet<Invitation> Invitations => Set<Invitation>();
    public DbSet<SupplierCategory> SupplierCategories => Set<SupplierCategory>();
    public DbSet<Supplier> Suppliers => Set<Supplier>();
    public DbSet<Quote> Quotes => Set<Quote>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("users");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Email).IsUnique();

            entity.Property(x => x.FullName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.PasswordHash)
                .HasMaxLength(512)
                .IsRequired();

            entity.Property(x => x.UserType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.HasOne(x => x.SocialProfile)
                .WithOne(x => x.User)
                .HasForeignKey<SocialProfile>(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Posts)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.OrganizedEvents)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Stories)
                .WithOne(x => x.Author)
                .HasForeignKey(x => x.AuthorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.StoryViews)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Likes)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Comments)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Highlights)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.InvitationTemplates)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.InvitationDrafts)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Invitations)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Quotes)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Orders)
                .WithOne(x => x.Organizer)
                .HasForeignKey(x => x.OrganizerId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Notifications)
                .WithOne(x => x.User)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.FollowingProfiles)
                .WithOne(x => x.FollowerUser)
                .HasForeignKey(x => x.FollowerUserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SocialProfile>(entity =>
        {
            entity.ToTable("social_profiles");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId).IsUnique();
            entity.HasIndex(x => x.UserName).IsUnique();

            entity.Property(x => x.UserName)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.DisplayName)
                .HasMaxLength(150)
                .IsRequired();

            entity.Property(x => x.Bio)
                .HasMaxLength(500);

            entity.Property(x => x.AvatarUrl)
                .HasMaxLength(500);

            entity.Property(x => x.City)
                .HasMaxLength(120);

            entity.Property(x => x.Category)
                .HasMaxLength(80);

            entity.Property(x => x.Instagram)
                .HasMaxLength(120);

            entity.Property(x => x.Site)
                .HasMaxLength(200);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasMany(x => x.Posts)
                .WithOne(x => x.AuthorProfile)
                .HasForeignKey(x => x.AuthorProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.Stories)
                .WithOne(x => x.AuthorProfile)
                .HasForeignKey(x => x.AuthorProfileId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.Highlights)
                .WithOne(x => x.Profile)
                .HasForeignKey(x => x.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Followers)
                .WithOne(x => x.FollowingProfile)
                .HasForeignKey(x => x.FollowingProfileId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<SocialFollow>(entity =>
        {
            entity.ToTable("social_follows");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.FollowerUserId, x.FollowingProfileId }).IsUnique();
            entity.HasIndex(x => x.FollowingProfileId);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<Post>(entity =>
        {
            entity.ToTable("posts");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.Caption)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.HasMany(x => x.Likes)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Comments)
                .WithOne(x => x.Post)
                .HasForeignKey(x => x.PostId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Event>(entity =>
        {
            entity.ToTable("events");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.StartDate);
            entity.HasIndex(x => x.Status);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Type)
                .HasMaxLength(80);

            entity.Property(x => x.Description)
                .HasMaxLength(3000);

            entity.Property(x => x.Location)
                .HasMaxLength(300);

            entity.Property(x => x.CoverImageUrl)
                .HasMaxLength(500);

            entity.Property(x => x.EstimatedCost)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasMany(x => x.InvitationTemplates)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.InvitationDrafts)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Invitations)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Quotes)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Orders)
                .WithOne(x => x.Event)
                .HasForeignKey(x => x.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Story>(entity =>
        {
            entity.ToTable("stories");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.ExpiresAt);
            entity.HasIndex(x => x.AuthorId);

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(500)
                .IsRequired();

            entity.Property(x => x.Caption)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.HasMany(x => x.Highlights)
                .WithOne(x => x.Story)
                .HasForeignKey(x => x.StoryId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(x => x.Views)
                .WithOne(x => x.Story)
                .HasForeignKey(x => x.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Like>(entity =>
        {
            entity.ToTable("likes");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.PostId, x.UserId }).IsUnique();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<Quote>(entity =>
        {
            entity.ToTable("quotes");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.ServiceName)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(2500)
                .IsRequired();

            entity.Property(x => x.EstimatedValue)
                .HasPrecision(18, 2);

            entity.Property(x => x.ResponseValue)
                .HasPrecision(18, 2);

            entity.Property(x => x.CounterProposalValue)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.ResponseMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.CounterProposalMessage)
                .HasMaxLength(1000);

            entity.Property(x => x.GeneratedOrderId)
                .HasMaxLength(40);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Quotes)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(x => x.Order)
                .WithOne(x => x.Quote)
                .HasForeignKey<Order>(x => x.QuoteId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Order>(entity =>
        {
            entity.ToTable("orders");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => x.OrderedAt);
            entity.HasIndex(x => x.QuoteId).IsUnique();

            entity.Property(x => x.Id)
                .HasMaxLength(40);

            entity.Property(x => x.ProductId)
                .HasMaxLength(80)
                .IsRequired();

            entity.Property(x => x.ProductName)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.TotalPrice)
                .HasPrecision(18, 2);

            entity.Property(x => x.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Notes)
                .HasMaxLength(1000);

            entity.Property(x => x.OrderedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasOne(x => x.Supplier)
                .WithMany(x => x.Orders)
                .HasForeignKey(x => x.SupplierId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("notifications");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.IsRead);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.Type);

            entity.Property(x => x.Title)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.Message)
                .HasMaxLength(1200)
                .IsRequired();

            entity.Property(x => x.Type)
                .HasMaxLength(30)
                .IsRequired();

            entity.Property(x => x.Link)
                .HasMaxLength(500);

            entity.Property(x => x.IsRead)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("comments");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.Content)
                .HasMaxLength(1000)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<Highlight>(entity =>
        {
            entity.ToTable("highlights");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.ProfileId, x.StoryId }).IsUnique();

            entity.Property(x => x.Title)
                .HasMaxLength(120);

            entity.Property(x => x.CoverUrl)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<StoryView>(entity =>
        {
            entity.ToTable("story_views");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => new { x.StoryId, x.UserId }).IsUnique();
            entity.HasIndex(x => x.ViewedAt);

            entity.Property(x => x.ViewedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<InvitationTemplate>(entity =>
        {
            entity.ToTable("invitation_templates");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.DefaultSystem);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.EventId);

            entity.Property(x => x.Name)
                .HasMaxLength(160)
                .IsRequired();

            entity.Property(x => x.Style)
                .HasMaxLength(80);

            entity.Property(x => x.BackgroundColor)
                .HasMaxLength(20);

            entity.Property(x => x.PrimaryColor)
                .HasMaxLength(20);

            entity.Property(x => x.TextColor)
                .HasMaxLength(20);

            entity.Property(x => x.Font)
                .HasMaxLength(80);

            entity.Property(x => x.Title)
                .HasMaxLength(200);

            entity.Property(x => x.Message)
                .HasMaxLength(1200);

            entity.Property(x => x.PreviewUrl)
                .HasMaxLength(500);

            entity.Property(x => x.DefaultSystem)
                .HasDefaultValue(false)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasMany(x => x.Drafts)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasMany(x => x.Invitations)
                .WithOne(x => x.Template)
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<InvitationDraft>(entity =>
        {
            entity.ToTable("invitation_drafts");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.UpdatedAt);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.LayoutJson)
                .HasMaxLength(32000)
                .IsRequired();

            entity.Property(x => x.PreviewHtml)
                .HasMaxLength(16000);

            entity.Property(x => x.PreviewUrl)
                .HasMaxLength(500);

            entity.Property(x => x.UpdatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasMany(x => x.Invitations)
                .WithOne(x => x.Draft)
                .HasForeignKey(x => x.DraftId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        modelBuilder.Entity<Invitation>(entity =>
        {
            entity.ToTable("invitations");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.OrganizerId);
            entity.HasIndex(x => x.EventId);
            entity.HasIndex(x => x.CreatedAt);

            entity.Property(x => x.Name)
                .HasMaxLength(200)
                .IsRequired();

            entity.Property(x => x.LayoutJson)
                .HasMaxLength(32000)
                .IsRequired();

            entity.Property(x => x.PreviewHtml)
                .HasMaxLength(16000);

            entity.Property(x => x.PreviewUrl)
                .HasMaxLength(500);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });

        modelBuilder.Entity<SupplierCategory>(entity =>
        {
            entity.ToTable("supplier_categories");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.IsActive);
            entity.HasIndex(x => x.SortOrder);

            entity.Property(x => x.Name)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(500);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            entity.HasMany(x => x.Suppliers)
                .WithOne(x => x.SupplierCategory)
                .HasForeignKey(x => x.SupplierCategoryId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Supplier>(entity =>
        {
            entity.ToTable("suppliers");

            entity.HasKey(x => x.Id);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.SupplierCategoryId);
            entity.HasIndex(x => x.City);
            entity.HasIndex(x => x.State);
            entity.HasIndex(x => x.Featured);
            entity.HasIndex(x => x.IsActive);

            entity.Property(x => x.Name)
                .HasMaxLength(180)
                .IsRequired();

            entity.Property(x => x.City)
                .HasMaxLength(120)
                .IsRequired();

            entity.Property(x => x.State)
                .HasMaxLength(2)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(2500)
                .IsRequired();

            entity.Property(x => x.ImageUrl)
                .HasMaxLength(500);

            entity.Property(x => x.PriceMin)
                .HasPrecision(18, 2);

            entity.Property(x => x.PriceMax)
                .HasPrecision(18, 2);

            entity.Property(x => x.Rating)
                .HasPrecision(5, 2)
                .HasDefaultValue(0);

            entity.Property(x => x.AcceptanceRate)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.ResponseRate)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.CancellationRate)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.PunctualityScore)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.RecentPerformanceScore)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.PopularityScore)
                .HasPrecision(6, 4)
                .HasDefaultValue(0);

            entity.Property(x => x.BadgesJson)
                .HasMaxLength(2000)
                .HasDefaultValue("[]")
                .IsRequired();

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true)
                .IsRequired();

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();
        });
    }
}
