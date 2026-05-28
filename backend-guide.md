# Backend — Clean Architecture · Identity + Profil + Projets d'audit

> **Approche retenue** : `IApplicationDbContext` injecté directement dans les handlers
> (recommandation Medium + Jason Taylor template) — évite le boilerplate Repository
> tout en gardant la testabilité. Deux DbContext séparés : Identity ↔ Application.

---

## Structure des dossiers

```
src/
├── Domain/
│   ├── Entities/
│   │   ├── Project.cs
│   │   └── AuditedUrl.cs
│   ├── ValueObjects/
│   │   └── Address.cs
│   ├── Enums/
│   │   └── ProjectStatus.cs
│   └── Exceptions/
│       ├── NotFoundException.cs
│       ├── ForbiddenException.cs
│       └── ConflictException.cs
│
├── Application/
│   ├── Common/
│   │   ├── Interfaces/
│   │   │   ├── IApplicationDbContext.cs
│   │   │   ├── ICurrentUserService.cs
│   │   │   ├── IUserService.cs
│   │   │   └── IDateTimeProvider.cs
│   │   └── Behaviors/
│   │       ├── ValidationBehavior.cs
│   │       └── LoggingBehavior.cs
│   ├── Users/
│   │   ├── Commands/UpdateProfile/
│   │   │   ├── UpdateProfileCommand.cs
│   │   │   ├── UpdateProfileCommandHandler.cs
│   │   │   └── UpdateProfileCommandValidator.cs
│   │   └── Queries/GetProfile/
│   │       ├── GetProfileQuery.cs
│   │       ├── GetProfileQueryHandler.cs
│   │       └── UserProfileDto.cs
│   └── Projects/
│       ├── Commands/
│       │   ├── CreateProject/
│       │   │   ├── CreateProjectCommand.cs
│       │   │   ├── CreateProjectCommandHandler.cs
│       │   │   └── CreateProjectCommandValidator.cs
│       │   └── AddAuditedUrl/
│       │       ├── AddAuditedUrlCommand.cs
│       │       ├── AddAuditedUrlCommandHandler.cs
│       │       └── AddAuditedUrlCommandValidator.cs
│       └── Queries/GetMyProjects/
│           ├── GetMyProjectsQuery.cs
│           ├── GetMyProjectsQueryHandler.cs
│           └── ProjectDto.cs
│
├── Infrastructure/
│   ├── Identity/
│   │   ├── ApplicationUser.cs
│   │   ├── ApplicationRole.cs
│   │   ├── AppIdentityDbContext.cs
│   │   └── DesignTimeIdentityDbContextFactory.cs
│   ├── Persistence/
│   │   ├── ApplicationDbContext.cs
│   │   ├── DesignTimeApplicationDbContextFactory.cs
│   │   └── Configurations/
│   │       ├── ProjectConfiguration.cs
│   │       └── AuditedUrlConfiguration.cs
│   └── Services/
│       ├── CurrentUserService.cs
│       ├── DateTimeProvider.cs
│       └── UserService.cs
│
└── Api/
    ├── Controllers/
    │   ├── ProfileController.cs
    │   └── ProjectsController.cs
    ├── Constants/
    │   └── Roles.cs
    └── Program.cs (extraits DI)
```

---

## Domain

### Exceptions

```csharp
// src/Domain/Exceptions/NotFoundException.cs
namespace Domain.Exceptions;

public sealed class NotFoundException(string name, object key)
    : Exception($"Entité « {name} » avec la clé « {key} » introuvable.");
```

```csharp
// src/Domain/Exceptions/ForbiddenException.cs
namespace Domain.Exceptions;

public sealed class ForbiddenException()
    : Exception("Vous n'êtes pas autorisé à effectuer cette action.");
```

```csharp
// src/Domain/Exceptions/ConflictException.cs
namespace Domain.Exceptions;

public sealed class ConflictException(string message) : Exception(message);
```

---

### ValueObjects/Address.cs

```csharp
// src/Domain/ValueObjects/Address.cs
namespace Domain.ValueObjects;

/// <summary>
/// Objet valeur représentant une adresse physique et/ou entreprise.
/// Stocké comme entité possédée (owned entity) via EF Core.
/// </summary>
public sealed class Address
{
    public string? Street    { get; init; }
    public string? City      { get; init; }
    public string? Province  { get; init; }
    public string? PostalCode { get; init; }
    public string? Country   { get; init; } = "Canada";
    public string? Company   { get; init; }
}
```

---

### Enums/ProjectStatus.cs

```csharp
// src/Domain/Enums/ProjectStatus.cs
namespace Domain.Enums;

public enum ProjectStatus
{
    Active   = 1,
    Archived = 2,
    Draft    = 3,
}
```

---

### Entities/Project.cs

```csharp
// src/Domain/Entities/Project.cs
namespace Domain.Entities;

/// <summary>
/// Projet d'audit regroupant plusieurs URLs auditées.
/// L'OwnerId référence ApplicationUser.Id sans FK croisée entre contextes.
/// </summary>
public sealed class Project
{
    private readonly List<AuditedUrl> _auditedUrls = [];

    public Guid   Id          { get; private set; }
    public Guid   OwnerId     { get; private set; }   // clé vers Identity — pas de FK cross-context
    public string Name        { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProjectStatus Status    { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    public IReadOnlyList<AuditedUrl> AuditedUrls => _auditedUrls.AsReadOnly();

    // EF Core needs a parameterless constructor (private is fine)
    private Project() { }

    /// <summary>
    /// Méthode factory — seul point de création valide. Impossible de créer
    /// un Project dans un état invalide depuis l'extérieur du Domain.
    /// </summary>
    public static Project Create(
        Guid ownerId, string name, string? description, IDateTimeProvider clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Project
        {
            Id          = Guid.NewGuid(),
            OwnerId     = ownerId,
            Name        = name.Trim(),
            Description = description?.Trim(),
            Status      = ProjectStatus.Active,
            CreatedAt   = clock.UtcNow,
        };
    }

    public void Update(string name, string? description, IDateTimeProvider clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name        = name.Trim();
        Description = description?.Trim();
        UpdatedAt   = clock.UtcNow;
    }

    public AuditedUrl AddUrl(string url, string? title, string? notes, IDateTimeProvider clock)
    {
        var auditedUrl = AuditedUrl.Create(Id, url, title, notes, clock);
        _auditedUrls.Add(auditedUrl);
        UpdatedAt = clock.UtcNow;
        return auditedUrl;
    }

    public void Archive(IDateTimeProvider clock)
    {
        Status    = ProjectStatus.Archived;
        UpdatedAt = clock.UtcNow;
    }
}
```

---

### Entities/AuditedUrl.cs

```csharp
// src/Domain/Entities/AuditedUrl.cs
namespace Domain.Entities;

/// <summary>
/// URL ou application web auditée dans le cadre d'un projet.
/// </summary>
public sealed class AuditedUrl
{
    public Guid   Id        { get; private set; }
    public Guid   ProjectId { get; private set; }
    public string Url       { get; private set; } = string.Empty;
    public string? Title    { get; private set; }
    public string? Notes    { get; private set; }
    public int    LighthouseScore { get; private set; }  // 0–100
    public DateTime AuditedAt    { get; private set; }
    public DateTime? UpdatedAt   { get; private set; }

    private AuditedUrl() { }

    public static AuditedUrl Create(
        Guid projectId, string url, string? title, string? notes, IDateTimeProvider clock)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(url);

        return new AuditedUrl
        {
            Id        = Guid.NewGuid(),
            ProjectId = projectId,
            Url       = url.Trim(),
            Title     = title?.Trim(),
            Notes     = notes?.Trim(),
            LighthouseScore = 0,
            AuditedAt = clock.UtcNow,
        };
    }

    public void UpdateScore(int score, string? notes, IDateTimeProvider clock)
    {
        if (score is < 0 or > 100)
            throw new ArgumentOutOfRangeException(nameof(score), "Score doit être entre 0 et 100.");

        LighthouseScore = score;
        Notes           = notes?.Trim() ?? Notes;
        UpdatedAt       = clock.UtcNow;
    }
}
```

---

## Application — Interfaces communes

### IDateTimeProvider.cs

```csharp
// src/Application/Common/Interfaces/IDateTimeProvider.cs
namespace Application.Common.Interfaces;

/// <summary>
/// Abstraction de l'horloge système — permet de mocker le temps dans les tests.
/// </summary>
public interface IDateTimeProvider
{
    DateTime UtcNow { get; }
}
```

### ICurrentUserService.cs

```csharp
// src/Application/Common/Interfaces/ICurrentUserService.cs
namespace Application.Common.Interfaces;

/// <summary>
/// Expose l'identité de l'utilisateur connecté à partir du token JWT.
/// Implémenté dans Infrastructure via IHttpContextAccessor.
/// </summary>
public interface ICurrentUserService
{
    Guid   UserId   { get; }
    string UserName { get; }
    bool   IsAuthenticated { get; }
}
```

### IApplicationDbContext.cs

```csharp
// src/Application/Common/Interfaces/IApplicationDbContext.cs
namespace Application.Common.Interfaces;

/// <summary>
/// Interface du DbContext applicatif.
/// Injectée directement dans les handlers — pas de Repository générique.
/// DbSet expose les entités; SaveChangesAsync persiste l'unité de travail.
///
/// Pourquoi pas de Repository? EF Core DbSet<T> EST déjà un Repository,
/// et DbContext EST déjà une Unit of Work. Ajouter une couche par-dessus
/// est une abstraction qui fuit et qui ajoute du bruit sans valeur.
/// Référence : https://medium.com/startup-insider-edge/...
/// </summary>
public interface IApplicationDbContext
{
    DbSet<Project>    Projects    { get; }
    DbSet<AuditedUrl> AuditedUrls { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
```

### IUserService.cs

```csharp
// src/Application/Common/Interfaces/IUserService.cs
namespace Application.Common.Interfaces;

/// <summary>
/// Contrat d'accès aux données utilisateur Identity depuis la couche Application.
/// Implémenté dans Infrastructure via UserManager<ApplicationUser>.
///
/// Pourquoi une interface plutôt que d'injecter UserManager directement?
/// UserManager est une dépendance Infrastructure. Application ne doit jamais
/// référencer Microsoft.AspNetCore.Identity directement.
/// </summary>
public interface IUserService
{
    Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default);
    Task UpdateProfileAsync(Guid userId, UpdateProfileData data, CancellationToken ct = default);
}

public sealed record UserProfileDto(
    Guid     UserId,
    string   UserName,
    string   FirstName,
    string   LastName,
    string   Email,
    bool     IsEmailPublic,
    string?  Bio,
    string?  WebsiteUrl,
    string?  AvatarUrl,
    DateOnly? DateOfBirth,
    AddressDto? Address
);

public sealed record AddressDto(
    string? Street,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country,
    string? Company
);

public sealed record UpdateProfileData(
    string   FirstName,
    string   LastName,
    bool     IsEmailPublic,
    string?  Bio,
    string?  WebsiteUrl,
    string?  AvatarUrl,
    DateOnly? DateOfBirth,
    AddressDto? Address
);
```

---

## Application — Users

### UpdateProfile

```csharp
// src/Application/Users/Commands/UpdateProfile/UpdateProfileCommand.cs
namespace Application.Users.Commands.UpdateProfile;

public sealed record UpdateProfileCommand(
    string   FirstName,
    string   LastName,
    bool     IsEmailPublic,
    string?  Bio,
    string?  WebsiteUrl,
    string?  AvatarUrl,
    DateOnly? DateOfBirth,
    AddressDto? Address
) : ICommand<Unit>;
```

```csharp
// src/Application/Users/Commands/UpdateProfile/UpdateProfileCommandHandler.cs
namespace Application.Users.Commands.UpdateProfile;

internal sealed class UpdateProfileCommandHandler(
    ICurrentUserService currentUser,
    IUserService        userService)
    : ICommandHandler<UpdateProfileCommand, Unit>
{
    public async ValueTask<Unit> Handle(
        UpdateProfileCommand command, CancellationToken cancellationToken)
    {
        var data = new UpdateProfileData(
            command.FirstName,
            command.LastName,
            command.IsEmailPublic,
            command.Bio,
            command.WebsiteUrl,
            command.AvatarUrl,
            command.DateOfBirth,
            command.Address
        );

        await userService.UpdateProfileAsync(currentUser.UserId, data, cancellationToken);
        return Unit.Value;
    }
}
```

```csharp
// src/Application/Users/Commands/UpdateProfile/UpdateProfileCommandValidator.cs
namespace Application.Users.Commands.UpdateProfile;

public sealed class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Le prénom est requis.")
            .MaximumLength(100);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Le nom est requis.")
            .MaximumLength(100);

        RuleFor(x => x.Bio)
            .MaximumLength(500).When(x => x.Bio is not null);

        RuleFor(x => x.WebsiteUrl)
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out _))
            .When(x => !string.IsNullOrEmpty(x.WebsiteUrl))
            .WithMessage("L'URL du site web n'est pas valide.");

        RuleFor(x => x.DateOfBirth)
            .Must(d => d < DateOnly.FromDateTime(DateTime.UtcNow))
            .When(x => x.DateOfBirth is not null)
            .WithMessage("La date de naissance doit être dans le passé.");

        When(x => x.Address is not null, () =>
        {
            RuleFor(x => x.Address!.PostalCode)
                .Matches(@"^[A-Za-z]\d[A-Za-z][ -]?\d[A-Za-z]\d$")
                .When(x => !string.IsNullOrEmpty(x.Address!.PostalCode))
                .WithMessage("Le code postal doit être au format canadien (ex. K1A 0A6).");
        });
    }
}
```

---

### GetProfile

```csharp
// src/Application/Users/Queries/GetProfile/GetProfileQuery.cs
namespace Application.Users.Queries.GetProfile;

/// <summary>
/// Retourne le profil de l'utilisateur connecté.
/// UserId = null → l'utilisateur courant (via ICurrentUserService).
/// UserId non-null avec rôle Admin → profil d'un autre utilisateur.
/// </summary>
public sealed record GetProfileQuery(Guid? UserId = null) : IQuery<UserProfileDto>;
```

```csharp
// src/Application/Users/Queries/GetProfile/GetProfileQueryHandler.cs
namespace Application.Users.Queries.GetProfile;

internal sealed class GetProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserService        userService)
    : IQueryHandler<GetProfileQuery, UserProfileDto>
{
    public async ValueTask<UserProfileDto> Handle(
        GetProfileQuery query, CancellationToken cancellationToken)
    {
        var targetId = query.UserId ?? currentUser.UserId;

        var profile = await userService.GetProfileAsync(targetId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), targetId);

        return profile;
    }
}
```

---

## Application — Projects

### CreateProject

```csharp
// src/Application/Projects/Commands/CreateProject/CreateProjectCommand.cs
namespace Application.Projects.Commands.CreateProject;

public sealed record CreateProjectCommand(
    string  Name,
    string? Description
) : ICommand<Guid>;
```

```csharp
// src/Application/Projects/Commands/CreateProject/CreateProjectCommandHandler.cs
namespace Application.Projects.Commands.CreateProject;

internal sealed class CreateProjectCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService   currentUser,
    IDateTimeProvider     clock)
    : ICommandHandler<CreateProjectCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        CreateProjectCommand command, CancellationToken cancellationToken)
    {
        var project = Project.Create(
            ownerId:     currentUser.UserId,
            name:        command.Name,
            description: command.Description,
            clock:       clock);

        db.Projects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        return project.Id;
    }
}
```

```csharp
// src/Application/Projects/Commands/CreateProject/CreateProjectCommandValidator.cs
namespace Application.Projects.Commands.CreateProject;

public sealed class CreateProjectCommandValidator
    : AbstractValidator<CreateProjectCommand>
{
    public CreateProjectCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Le nom du projet est requis.")
            .MaximumLength(200);

        RuleFor(x => x.Description)
            .MaximumLength(1000).When(x => x.Description is not null);
    }
}
```

---

### AddAuditedUrl

```csharp
// src/Application/Projects/Commands/AddAuditedUrl/AddAuditedUrlCommand.cs
namespace Application.Projects.Commands.AddAuditedUrl;

public sealed record AddAuditedUrlCommand(
    Guid    ProjectId,
    string  Url,
    string? Title,
    string? Notes
) : ICommand<Guid>;
```

```csharp
// src/Application/Projects/Commands/AddAuditedUrl/AddAuditedUrlCommandHandler.cs
namespace Application.Projects.Commands.AddAuditedUrl;

internal sealed class AddAuditedUrlCommandHandler(
    IApplicationDbContext db,
    ICurrentUserService   currentUser,
    IDateTimeProvider     clock)
    : ICommandHandler<AddAuditedUrlCommand, Guid>
{
    public async ValueTask<Guid> Handle(
        AddAuditedUrlCommand command, CancellationToken cancellationToken)
    {
        // Charger le projet avec ses URLs pour validation d'invariant
        var project = await db.Projects
            .Include(p => p.AuditedUrls)
            .FirstOrDefaultAsync(p => p.Id == command.ProjectId, cancellationToken)
            ?? throw new NotFoundException(nameof(Project), command.ProjectId);

        // Vérification d'appartenance — WCAG de la sécurité : sécurisé par défaut
        if (project.OwnerId != currentUser.UserId)
            throw new ForbiddenException();

        var url = project.AddUrl(command.Url, command.Title, command.Notes, clock);
        await db.SaveChangesAsync(cancellationToken);

        return url.Id;
    }
}
```

```csharp
// src/Application/Projects/Commands/AddAuditedUrl/AddAuditedUrlCommandValidator.cs
namespace Application.Projects.Commands.AddAuditedUrl;

public sealed class AddAuditedUrlCommandValidator
    : AbstractValidator<AddAuditedUrlCommand>
{
    public AddAuditedUrlCommandValidator()
    {
        RuleFor(x => x.ProjectId)
            .NotEmpty();

        RuleFor(x => x.Url)
            .NotEmpty().WithMessage("L'URL est requise.")
            .Must(url => Uri.TryCreate(url, UriKind.Absolute, out var u)
                         && (u.Scheme == Uri.UriSchemeHttp || u.Scheme == Uri.UriSchemeHttps))
            .WithMessage("L'URL doit être une adresse HTTP ou HTTPS valide.");

        RuleFor(x => x.Title)
            .MaximumLength(200).When(x => x.Title is not null);

        RuleFor(x => x.Notes)
            .MaximumLength(2000).When(x => x.Notes is not null);
    }
}
```

---

### GetMyProjects

```csharp
// src/Application/Projects/Queries/GetMyProjects/GetMyProjectsQuery.cs
namespace Application.Projects.Queries.GetMyProjects;

public sealed record GetMyProjectsQuery(
    ProjectStatus? Status = null,
    int Page     = 1,
    int PageSize = 20
) : IQuery<PagedResult<ProjectDto>>;

public sealed record PagedResult<T>(
    IReadOnlyList<T> Items,
    int TotalCount,
    int Page,
    int PageSize,
    bool HasNextPage
);
```

```csharp
// src/Application/Projects/Queries/GetMyProjects/ProjectDto.cs
namespace Application.Projects.Queries.GetMyProjects;

public sealed record ProjectDto(
    Guid   Id,
    string Name,
    string? Description,
    string Status,
    int    UrlCount,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    IReadOnlyList<AuditedUrlDto> AuditedUrls
);

public sealed record AuditedUrlDto(
    Guid    Id,
    string  Url,
    string? Title,
    string? Notes,
    int     LighthouseScore,
    DateTime AuditedAt
);
```

```csharp
// src/Application/Projects/Queries/GetMyProjects/GetMyProjectsQueryHandler.cs
namespace Application.Projects.Queries.GetMyProjects;

internal sealed class GetMyProjectsQueryHandler(
    IApplicationDbContext db,
    ICurrentUserService   currentUser)
    : IQueryHandler<GetMyProjectsQuery, PagedResult<ProjectDto>>
{
    public async ValueTask<PagedResult<ProjectDto>> Handle(
        GetMyProjectsQuery query, CancellationToken cancellationToken)
    {
        // Requête de base — AsNoTracking car lecture seule
        var baseQuery = db.Projects
            .AsNoTracking()
            .Where(p => p.OwnerId == currentUser.UserId);

        // Filtre optionnel par statut
        if (query.Status is not null)
            baseQuery = baseQuery.Where(p => p.Status == query.Status);

        var totalCount = await baseQuery.CountAsync(cancellationToken);

        var projects = await baseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Include(p => p.AuditedUrls)
            .Select(p => new ProjectDto(
                p.Id,
                p.Name,
                p.Description,
                p.Status.ToString(),
                p.AuditedUrls.Count,
                p.CreatedAt,
                p.UpdatedAt,
                p.AuditedUrls
                    .OrderByDescending(u => u.AuditedAt)
                    .Select(u => new AuditedUrlDto(
                        u.Id, u.Url, u.Title, u.Notes,
                        u.LighthouseScore, u.AuditedAt))
                    .ToList()
            ))
            .ToListAsync(cancellationToken);

        return new PagedResult<ProjectDto>(
            projects,
            totalCount,
            query.Page,
            query.PageSize,
            (query.Page * query.PageSize) < totalCount
        );
    }
}
```

---

## Infrastructure — Identity

### ApplicationRole.cs

```csharp
// src/Infrastructure/Identity/ApplicationRole.cs
namespace Infrastructure.Identity;

/// <summary>
/// Rôle personnalisé — étend IdentityRole<Guid> pour des propriétés futures.
/// </summary>
public sealed class ApplicationRole : IdentityRole<Guid>
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
}
```

### ApplicationUser.cs

```csharp
// src/Infrastructure/Identity/ApplicationUser.cs
namespace Infrastructure.Identity;

/// <summary>
/// Utilisateur Identity étendu avec champs métier.
///
/// Bonnes pratiques Microsoft (.NET 10) :
/// - Hérite de IdentityUser<Guid> — clé primaire UUID au lieu de string
/// - Profile en owned entity (table non séparée par défaut,
///   configurable en table séparée via ToTable())
/// - Champs Register requis : FirstName, LastName (Username + Email via IdentityUser)
/// </summary>
public sealed class ApplicationUser : IdentityUser<Guid>
{
    // ── Champs Register (obligatoires dès la création) ──────────────────────
    public string FirstName { get; set; } = string.Empty;
    public string LastName  { get; set; } = string.Empty;

    // ── Profil étendu (optionnel — mis à jour après register) ───────────────
    public UserProfile Profile { get; set; } = new();

    // Propriété calculée non persistée
    public string FullName => $"{FirstName} {LastName}".Trim();
}

/// <summary>
/// Entité possédée — persiste dans la même table AspNetUsers.
/// Migrer vers .ToTable("UserProfiles") si la table devient trop large.
/// </summary>
public sealed class UserProfile
{
    public DateOnly? DateOfBirth  { get; set; }
    public bool      IsEmailPublic { get; set; } = false;  // privé par défaut ✅
    public string?   Bio          { get; set; }
    public string?   WebsiteUrl   { get; set; }
    public string?   AvatarUrl    { get; set; }

    // Owned entity imbriquée
    public Address?  Address { get; set; }
}
```

### AppIdentityDbContext.cs

```csharp
// src/Infrastructure/Identity/AppIdentityDbContext.cs
namespace Infrastructure.Identity;

/// <summary>
/// DbContext Identity — gère uniquement les tables AspNet*.
/// Séparé de ApplicationDbContext pour isoler les préoccupations :
/// - Identity : utilisateurs, rôles, claims, tokens
/// - Application : entités métier (Project, AuditedUrl)
///
/// Les deux contextes partagent la même base de données SQL Server
/// mais des migrations distinctes.
/// </summary>
public sealed class AppIdentityDbContext(DbContextOptions<AppIdentityDbContext> options)
    : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>(options)
{
    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Renommer les tables pour conventions maison (optionnel)
        builder.HasDefaultSchema("identity");

        builder.Entity<ApplicationUser>(b =>
        {
            b.Property(u => u.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            b.Property(u => u.LastName)
                .HasMaxLength(100)
                .IsRequired();

            // Owned entity — UserProfile persiste dans AspNetUsers
            b.OwnsOne(u => u.Profile, profile =>
            {
                profile.Property(p => p.Bio)
                    .HasMaxLength(500);

                profile.Property(p => p.WebsiteUrl)
                    .HasMaxLength(500);

                profile.Property(p => p.AvatarUrl)
                    .HasMaxLength(1000);

                // Owned entity imbriquée — Address dans même table
                profile.OwnsOne(p => p.Address, addr =>
                {
                    addr.Property(a => a.Street).HasMaxLength(200);
                    addr.Property(a => a.City).HasMaxLength(100);
                    addr.Property(a => a.Province).HasMaxLength(100);
                    addr.Property(a => a.PostalCode).HasMaxLength(10);
                    addr.Property(a => a.Country).HasMaxLength(100).HasDefaultValue("Canada");
                    addr.Property(a => a.Company).HasMaxLength(200);
                });
            });
        });
    }
}
```

### DesignTimeIdentityDbContextFactory.cs

```csharp
// src/Infrastructure/Identity/DesignTimeIdentityDbContextFactory.cs
namespace Infrastructure.Identity;

/// <summary>
/// Requis pour `dotnet ef migrations add` sur le projet Infrastructure.
/// Évite l'erreur "Unable to create DbContext instance at design time".
/// Ne jamais mettre de vraie connection string ici — utiliser variable d'env.
/// </summary>
public sealed class DesignTimeIdentityDbContextFactory
    : IDesignTimeDbContextFactory<AppIdentityDbContext>
{
    public AppIdentityDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppIdentityDbContext>()
            .UseSqlServer(
                Environment.GetEnvironmentVariable("IDENTITY_CONNECTION_STRING")
                ?? "Server=(localdb)\\mssqllocaldb;Database=Portfolio_Identity_Dev;Trusted_Connection=True;",
                sql => sql.MigrationsHistoryTable("__IdentityMigrationsHistory", "identity"))
            .Options;

        return new AppIdentityDbContext(options);
    }
}
```

---

## Infrastructure — Persistence

### ApplicationDbContext.cs

```csharp
// src/Infrastructure/Persistence/ApplicationDbContext.cs
namespace Infrastructure.Persistence;

/// <summary>
/// DbContext applicatif — entités métier uniquement.
/// Implémente IApplicationDbContext pour injection dans les handlers.
///
/// Séparé de AppIdentityDbContext :
/// ✅ Migrations indépendantes
/// ✅ Pas de FK croisée entre Identity et Business
/// ✅ OwnerId = Guid référence la clé Identity sans contrainte EF
/// </summary>
public sealed class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : DbContext(options), IApplicationDbContext
{
    public DbSet<Project>    Projects    => Set<Project>();
    public DbSet<AuditedUrl> AuditedUrls => Set<AuditedUrl>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.HasDefaultSchema("app");

        // Appliquer toutes les IEntityTypeConfiguration<T> du même assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

### Configurations/ProjectConfiguration.cs

```csharp
// src/Infrastructure/Persistence/Configurations/ProjectConfiguration.cs
namespace Infrastructure.Persistence.Configurations;

public sealed class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("Projects", "app");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Status)
            .HasConversion<string>()  // stocke "Active" au lieu de 1
            .HasMaxLength(20)
            .IsRequired();

        // OwnerId : Guid référence Identity — pas de FK cross-context
        // L'intégrité est garantie par l'application, pas la DB
        builder.Property(p => p.OwnerId)
            .IsRequired();

        builder.HasIndex(p => p.OwnerId);  // index pour les requêtes filtrées par user
        builder.HasIndex(p => p.CreatedAt); // index pour le tri par date

        // Navigation privée (_auditedUrls)
        builder.HasMany(p => p.AuditedUrls)
            .WithOne()
            .HasForeignKey(u => u.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);

        // Optimistic concurrency (optionnel mais recommandé)
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
    }
}
```

### Configurations/AuditedUrlConfiguration.cs

```csharp
// src/Infrastructure/Persistence/Configurations/AuditedUrlConfiguration.cs
namespace Infrastructure.Persistence.Configurations;

public sealed class AuditedUrlConfiguration : IEntityTypeConfiguration<AuditedUrl>
{
    public void Configure(EntityTypeBuilder<AuditedUrl> builder)
    {
        builder.ToTable("AuditedUrls", "app");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Url)
            .HasMaxLength(2048)  // limite RFC 2616
            .IsRequired();

        builder.Property(u => u.Title)
            .HasMaxLength(200);

        builder.Property(u => u.Notes)
            .HasMaxLength(2000);

        builder.Property(u => u.LighthouseScore)
            .HasDefaultValue(0);

        builder.HasIndex(u => u.ProjectId);
        builder.HasIndex(u => u.AuditedAt);

        // Contrainte de score
        builder.ToTable(t => t.HasCheckConstraint(
            "CK_AuditedUrls_LighthouseScore",
            "[LighthouseScore] >= 0 AND [LighthouseScore] <= 100"));
    }
}
```

### DesignTimeApplicationDbContextFactory.cs

```csharp
// src/Infrastructure/Persistence/DesignTimeApplicationDbContextFactory.cs
namespace Infrastructure.Persistence;

public sealed class DesignTimeApplicationDbContextFactory
    : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(
                Environment.GetEnvironmentVariable("APP_CONNECTION_STRING")
                ?? "Server=(localdb)\\mssqllocaldb;Database=Portfolio_App_Dev;Trusted_Connection=True;",
                sql => sql.MigrationsHistoryTable("__AppMigrationsHistory", "app"))
            .Options;

        return new ApplicationDbContext(options);
    }
}
```

---

## Infrastructure — Services

### CurrentUserService.cs

```csharp
// src/Infrastructure/Services/CurrentUserService.cs
namespace Infrastructure.Services;

/// <summary>
/// Lit l'identité depuis le ClaimsPrincipal injecté par le middleware JWT.
/// Sub claim = UserId (Guid), Name claim = UserName.
/// </summary>
public sealed class CurrentUserService(IHttpContextAccessor httpContextAccessor)
    : ICurrentUserService
{
    private ClaimsPrincipal? Principal =>
        httpContextAccessor.HttpContext?.User;

    public bool IsAuthenticated =>
        Principal?.Identity?.IsAuthenticated ?? false;

    public Guid UserId =>
        Guid.TryParse(
            Principal?.FindFirstValue(ClaimTypes.NameIdentifier),
            out var id)
            ? id
            : throw new InvalidOperationException("Utilisateur non authentifié.");

    public string UserName =>
        Principal?.FindFirstValue(ClaimTypes.Name)
        ?? throw new InvalidOperationException("Claim Name introuvable.");
}
```

### DateTimeProvider.cs

```csharp
// src/Infrastructure/Services/DateTimeProvider.cs
namespace Infrastructure.Services;

public sealed class DateTimeProvider : IDateTimeProvider
{
    public DateTime UtcNow => DateTime.UtcNow;
}
```

### UserService.cs

```csharp
// src/Infrastructure/Services/UserService.cs
namespace Infrastructure.Services;

/// <summary>
/// Implémentation IUserService via UserManager<ApplicationUser>.
/// Seul endroit où Identity est manipulé directement.
/// La couche Application ne connaît pas ApplicationUser — seulement les DTOs.
/// </summary>
public sealed class UserService(UserManager<ApplicationUser> userManager)
    : IUserService
{
    public async Task<UserProfileDto?> GetProfileAsync(Guid userId, CancellationToken ct = default)
    {
        var user = await userManager.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId, ct);

        if (user is null) return null;

        return MapToDto(user);
    }

    public async Task UpdateProfileAsync(
        Guid userId, UpdateProfileData data, CancellationToken ct = default)
    {
        var user = await userManager.FindByIdAsync(userId.ToString())
            ?? throw new NotFoundException(nameof(ApplicationUser), userId);

        user.FirstName = data.FirstName.Trim();
        user.LastName  = data.LastName.Trim();

        user.Profile.IsEmailPublic = data.IsEmailPublic;
        user.Profile.Bio           = data.Bio?.Trim();
        user.Profile.WebsiteUrl    = data.WebsiteUrl?.Trim();
        user.Profile.AvatarUrl     = data.AvatarUrl?.Trim();
        user.Profile.DateOfBirth   = data.DateOfBirth;

        user.Profile.Address = data.Address is null ? null : new Address
        {
            Street     = data.Address.Street?.Trim(),
            City       = data.Address.City?.Trim(),
            Province   = data.Address.Province?.Trim(),
            PostalCode = data.Address.PostalCode?.Trim(),
            Country    = data.Address.Country?.Trim() ?? "Canada",
            Company    = data.Address.Company?.Trim(),
        };

        var result = await userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(", ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"Mise à jour du profil échouée : {errors}");
        }
    }

    private static UserProfileDto MapToDto(ApplicationUser user) => new(
        user.Id,
        user.UserName!,
        user.FirstName,
        user.LastName,
        user.Email!,
        user.Profile.IsEmailPublic,
        user.Profile.Bio,
        user.Profile.WebsiteUrl,
        user.Profile.AvatarUrl,
        user.Profile.DateOfBirth,
        user.Profile.Address is null ? null : new AddressDto(
            user.Profile.Address.Street,
            user.Profile.Address.City,
            user.Profile.Address.Province,
            user.Profile.Address.PostalCode,
            user.Profile.Address.Country,
            user.Profile.Address.Company)
    );
}
```

---

## API — Controllers

### Roles.cs

```csharp
// src/Api/Constants/Roles.cs
namespace Api.Constants;

/// <summary>
/// Constantes de rôle — jamais de magic strings dans les controllers.
/// </summary>
public static class Roles
{
    public const string Member     = nameof(Member);
    public const string Admin      = nameof(Admin);
    public const string SuperAdmin = nameof(SuperAdmin);

    // Combinaisons utiles
    public const string AdminOrAbove = $"{Admin},{SuperAdmin}";
    public const string All          = $"{Member},{Admin},{SuperAdmin}";
}
```

### ProfileController.cs

```csharp
// src/Api/Controllers/ProfileController.cs
namespace Api.Controllers;

/// <summary>
/// Gestion du profil de l'utilisateur connecté.
/// Toutes les routes sont protégées par [Authorize] — secure by default.
/// </summary>
[ApiController]
[Route("api/v1/profile")]
[Authorize]
[Produces("application/json")]
public sealed class ProfileController(ISender sender) : ControllerBase
{
    /// <summary>Retourne le profil de l'utilisateur connecté.</summary>
    [HttpGet]
    [ProducesResponseType<UserProfileDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var result = await sender.Send(new GetProfileQuery(), ct);
        return Ok(result);
    }

    /// <summary>Met à jour le profil de l'utilisateur connecté.</summary>
    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> UpdateProfile(
        [FromBody] UpdateProfileRequest request,
        CancellationToken ct)
    {
        var command = new UpdateProfileCommand(
            request.FirstName,
            request.LastName,
            request.IsEmailPublic,
            request.Bio,
            request.WebsiteUrl,
            request.AvatarUrl,
            request.DateOfBirth,
            request.Address is null ? null : new AddressDto(
                request.Address.Street,
                request.Address.City,
                request.Address.Province,
                request.Address.PostalCode,
                request.Address.Country,
                request.Address.Company)
        );

        await sender.Send(command, ct);
        return NoContent();
    }
}

// ── Request models (API layer — pas dans Application) ─────────────────────

public sealed record UpdateProfileRequest(
    string   FirstName,
    string   LastName,
    bool     IsEmailPublic   = false,
    string?  Bio             = null,
    string?  WebsiteUrl      = null,
    string?  AvatarUrl       = null,
    DateOnly? DateOfBirth    = null,
    AddressRequest? Address  = null
);

public sealed record AddressRequest(
    string? Street,
    string? City,
    string? Province,
    string? PostalCode,
    string? Country,
    string? Company
);
```

### ProjectsController.cs

```csharp
// src/Api/Controllers/ProjectsController.cs
namespace Api.Controllers;

[ApiController]
[Route("api/v1/projects")]
[Authorize]
[Produces("application/json")]
public sealed class ProjectsController(ISender sender) : ControllerBase
{
    /// <summary>Retourne les projets d'audit de l'utilisateur connecté.</summary>
    [HttpGet]
    [ProducesResponseType<PagedResult<ProjectDto>>(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetMyProjects(
        [FromQuery] ProjectStatus? status,
        [FromQuery] int page     = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var result = await sender.Send(
            new GetMyProjectsQuery(status, page, pageSize), ct);
        return Ok(result);
    }

    /// <summary>Crée un nouveau projet d'audit.</summary>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateProject(
        [FromBody] CreateProjectRequest request,
        CancellationToken ct)
    {
        var projectId = await sender.Send(
            new CreateProjectCommand(request.Name, request.Description), ct);

        return CreatedAtAction(
            nameof(GetProject), new { id = projectId }, new { id = projectId });
    }

    /// <summary>Retourne un projet par ID (doit appartenir à l'utilisateur).</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType<ProjectDto>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetProject(Guid id, CancellationToken ct)
    {
        // La query filtre déjà par OwnerId — retourne null si pas le bon user
        var projects = await sender.Send(new GetMyProjectsQuery(), ct);
        var project  = projects.Items.FirstOrDefault(p => p.Id == id);

        return project is null ? NotFound() : Ok(project);
    }

    /// <summary>Ajoute une URL auditée à un projet.</summary>
    [HttpPost("{id:guid}/urls")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddAuditedUrl(
        Guid id,
        [FromBody] AddAuditedUrlRequest request,
        CancellationToken ct)
    {
        var urlId = await sender.Send(
            new AddAuditedUrlCommand(id, request.Url, request.Title, request.Notes), ct);

        return Created($"/api/v1/projects/{id}/urls/{urlId}", new { id = urlId });
    }
}

public sealed record CreateProjectRequest(string Name, string? Description = null);

public sealed record AddAuditedUrlRequest(
    string  Url,
    string? Title = null,
    string? Notes = null
);
```

---

## API — Program.cs (extraits DI)

```csharp
// src/Api/Program.cs — extraits pertinents

var builder = WebApplication.CreateBuilder(args);

// ── DbContexts ──────────────────────────────────────────────────────────────
// Même connection string — deux schemas séparés dans la même DB
var connectionString = builder.Configuration.GetConnectionString("Default")
    ?? throw new InvalidOperationException("Connection string 'Default' manquante.");

builder.Services.AddDbContext<AppIdentityDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("__IdentityMigrationsHistory", "identity")));

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString,
        sql => sql.MigrationsHistoryTable("__AppMigrationsHistory", "app")));

// Enregistrer l'interface — les handlers injectent IApplicationDbContext
builder.Services.AddScoped<IApplicationDbContext>(
    provider => provider.GetRequiredService<ApplicationDbContext>());

// ── Identity ─────────────────────────────────────────────────────────────────
builder.Services
    .AddIdentity<ApplicationUser, ApplicationRole>(options =>
    {
        // Permettre login par Email OU Username
        options.User.RequireUniqueEmail     = true;
        options.SignIn.RequireConfirmedEmail = false; // activer en prod

        options.Password.RequiredLength         = 8;
        options.Password.RequireDigit           = true;
        options.Password.RequireLowercase       = true;
        options.Password.RequireUppercase       = true;
        options.Password.RequireNonAlphanumeric = true;

        // Protection contre brute force
        options.Lockout.DefaultLockoutTimeSpan  = TimeSpan.FromMinutes(15);
        options.Lockout.MaxFailedAccessAttempts = 5;

        // SchemaVersion 2 requis pour .NET 10 (voir Microsoft docs)
        options.Stores.SchemaVersion    = IdentitySchemaVersions.Version2;
        options.Stores.MaxLengthForKeys = 256;
    })
    .AddEntityFrameworkStores<AppIdentityDbContext>()
    .AddDefaultTokenProviders();

// ── JWT Bearer ───────────────────────────────────────────────────────────────
builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme    = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"],
            ValidAudience            = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey         = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)),
            ClockSkew = TimeSpan.Zero,  // pas de délai de grâce — token expiré = refusé
        };
    });

// ── Services applicatifs ─────────────────────────────────────────────────────
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddSingleton<IDateTimeProvider, DateTimeProvider>();

// ── Mediator (source-generated) ──────────────────────────────────────────────
builder.Services.AddMediator(options =>
    options.ServiceLifetime = ServiceLifetime.Scoped);

// ── FluentValidation ────────────────────────────────────────────────────────
builder.Services.AddValidatorsFromAssembly(
    typeof(UpdateProfileCommandValidator).Assembly);

// ── Middleware d'exception global ────────────────────────────────────────────
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

// ── CORS ─────────────────────────────────────────────────────────────────────
builder.Services.AddCors(options =>
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins(
                builder.Configuration["AllowedOrigins"]!.Split(','))
            .AllowAnyHeader()
            .AllowAnyMethod()));

var app = builder.Build();

app.UseExceptionHandler();
app.UseCors("Frontend");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seeder de rôles au démarrage
await SeedRolesAsync(app);
app.Run();

static async Task SeedRolesAsync(WebApplication app)
{
    using var scope       = app.Services.CreateScope();
    var roleManager       = scope.ServiceProvider
                                .GetRequiredService<RoleManager<ApplicationRole>>();

    foreach (var role in new[] { Roles.Member, Roles.Admin, Roles.SuperAdmin })
    {
        if (!await roleManager.RoleExistsAsync(role))
            await roleManager.CreateAsync(new ApplicationRole(role));
    }
}
```

---

## GlobalExceptionHandler.cs

```csharp
// src/Api/GlobalExceptionHandler.cs
namespace Api;

/// <summary>
/// Mappe les exceptions domain vers RFC 9457 ProblemDetails.
/// Aucun try/catch dans les controllers — tout passe ici.
/// </summary>
internal sealed class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
    : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception   exception,
        CancellationToken cancellationToken)
    {
        var (statusCode, title) = exception switch
        {
            NotFoundException  => (StatusCodes.Status404NotFound,       "Ressource introuvable"),
            ForbiddenException => (StatusCodes.Status403Forbidden,       "Accès refusé"),
            ConflictException  => (StatusCodes.Status409Conflict,        "Conflit de données"),
            ValidationException v => (StatusCodes.Status422UnprocessableEntity, "Données invalides"),
            _                  => (StatusCodes.Status500InternalServerError, "Erreur interne"),
        };

        if (statusCode == 500)
            logger.LogError(exception, "Erreur non gérée");

        var problem = new ProblemDetails
        {
            Status = statusCode,
            Title  = title,
            Detail = exception is ValidationException ve
                ? string.Join("; ", ve.Errors.Select(e => e.ErrorMessage))
                : exception.Message,
        };

        httpContext.Response.StatusCode = statusCode;
        await httpContext.Response.WriteAsJsonAsync(problem, cancellationToken);
        return true;
    }
}
```

---

## Commandes de migration (deux contextes)

```bash
# Migrations Identity (AppIdentityDbContext)
dotnet ef migrations add InitIdentity \
    --project src/Infrastructure \
    --startup-project src/Api \
    --context AppIdentityDbContext \
    --output-dir Identity/Migrations

# Migrations Application (ApplicationDbContext)
dotnet ef migrations add InitApplication \
    --project src/Infrastructure \
    --startup-project src/Api \
    --context ApplicationDbContext \
    --output-dir Persistence/Migrations

# Appliquer les deux
dotnet ef database update --context AppIdentityDbContext \
    --project src/Infrastructure --startup-project src/Api

dotnet ef database update --context ApplicationDbContext \
    --project src/Infrastructure --startup-project src/Api
```

---

## Login avec Email OU Username (pour référence)

```csharp
// Dans AuthController (déjà existant) — point clé du login flexible
public async Task<IActionResult> Login([FromBody] LoginRequest request)
{
    // Cherche par Email d'abord, puis par Username
    var user = await userManager.FindByEmailAsync(request.EmailOrUsername)
            ?? await userManager.FindByNameAsync(request.EmailOrUsername);

    if (user is null || !await userManager.CheckPasswordAsync(user, request.Password))
        return Unauthorized(new { message = "Identifiants invalides." });

    // Vérification du lockout
    if (await userManager.IsLockedOutAsync(user))
        return Unauthorized(new { message = "Compte verrouillé. Réessayez dans 15 minutes." });

    var roles = await userManager.GetRolesAsync(user);

    var claims = new List<Claim>
    {
        new(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new(ClaimTypes.Name,           user.UserName!),
        new(ClaimTypes.Email,          user.Email!),
        new("firstName",               user.FirstName),
        new("lastName",                user.LastName),
    };

    claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

    // Générer le JWT...
}

public sealed record LoginRequest(string EmailOrUsername, string Password);
```
La bonne solution
csharp// src/Domain/Constants/Roles.cs
namespace Domain.Constants;

/// <summary>
/// Constantes de rôles métier.
/// Placé dans Domain — couche la plus intérieure — accessible depuis
/// Application, Infrastructure et Api sans violer les règles de dépendance.
/// </summary>
public static class Roles
{
    public const string Member     = nameof(Member);
    public const string Admin      = nameof(Admin);
    public const string SuperAdmin = nameof(SuperAdmin);

    // Combinaisons pour [Authorize(Roles = ...)]
    public const string AdminOrAbove = $"{Admin},{SuperAdmin}";
    public const string All          = $"{Member},{Admin},{SuperAdmin}";
}

Exemple concret de l'impact
Voici pourquoi ça compte en pratique — le handler GetProfileQueryHandler qui permet à un Admin de voir le profil d'un autre utilisateur :
csharp// src/Application/Users/Queries/GetProfile/GetProfileQueryHandler.cs
// ✅ Avec Domain.Constants.Roles — fonctionne
// ❌ Avec Api.Constants.Roles — ne compile pas (référence circulaire)

internal sealed class GetProfileQueryHandler(
    ICurrentUserService currentUser,
    IUserService        userService)
    : IQueryHandler<GetProfileQuery, UserProfileDto>
{
    public async ValueTask<UserProfileDto> Handle(
        GetProfileQuery query, CancellationToken cancellationToken)
    {
        var targetId = query.UserId ?? currentUser.UserId;

        // Un utilisateur ne peut voir que son propre profil...
        if (targetId != currentUser.UserId)
        {
            // ...sauf s'il est Admin ou SuperAdmin
            // ✅ Possible seulement si Roles est dans Domain
            var isPrivileged = currentUser.Roles.Any(r =>
                r is Roles.Admin or Roles.SuperAdmin);  // Domain.Constants.Roles

            if (!isPrivileged)
                throw new ForbiddenException();
        }

        var profile = await userService.GetProfileAsync(targetId, cancellationToken)
            ?? throw new NotFoundException(nameof(ApplicationUser), targetId);

        return profile;
    }
}
Et le controller lui aussi peut l'utiliser sans problème :
csharp// src/Api/Controllers/ProfileController.cs
using Domain.Constants; // ✅ Api référence Domain — toujours permis

[Authorize(Roles = Roles.AdminOrAbove)]
[HttpGet("{userId:guid}")]
public async Task<IActionResult> GetUserProfile(Guid userId, CancellationToken ct)
{
    var result = await sender.Send(new GetProfileQuery(userId), ct);
    return Ok(result);
}

Mise à jour du CLAUDE.md
La ligne dans ton CLAUDE.md dit :

"Role name constants live in a single Roles static class."

Elle devrait préciser l'emplacement :

"Role name constants live in Domain/Constants/Roles.cs — the innermost layer ensures all other layers (Application, Infrastructure, Api) can reference them without violating dependency rules."

Et même logique pour les autres constantes transversales comme les claim types ou les noms de policies — elles vont toutes dans Domain/Constants/.

