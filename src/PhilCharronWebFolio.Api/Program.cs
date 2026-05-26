using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using PhilCharronWebFolio.Api.Middlewares;
using PhilCharronWebFolio.Application;
using PhilCharronWebFolio.Infrastructure;
using PhilCharronWebFolio.Infrastructure.Identity;
using PhilCharronWebFolio.Infrastructure.Persistence;
using PhilCharronWebFolio.Infrastructure.Persistence.Interceptors;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- 1. CLEAN ARCHITECTURE (Tout est délégué ici) ---
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

// --- 2. BASE DE DONNÉES & IDENTITY (Peut aussi être déplacé dans AddInfrastructure) ---
builder.Services.AddDbContext<ApplicationDbContext>((sp, opts) =>
    opts.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
        .AddInterceptors(sp.GetRequiredService<AuditableEntityInterceptor>()));

builder.Services.AddIdentityCore<AppUser>(opts => { opts.User.RequireUniqueEmail = true; })
    .AddRoles<IdentityRole<Guid>>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// --- 3. API (Auth, Controllers, CORS) ---
var jwtKey = builder.Configuration["Jwt:Key"]!;
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opts => opts.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ValidateIssuer = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidateAudience = true,
        ValidAudience = builder.Configuration["Jwt:Audience"]
    });

builder.Services.AddAuthorization(options =>
    {
        options.FallbackPolicy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()
            .Build();
    });

// ====== API Services ======
builder.Services.AddControllers();
builder.Services.AddOpenApi(); // Pour .NET 10

// Services spécifiques à l'API, se trouve peut-être déja ailleurs: à voir
//builder.Services.AddHttpContextAccessor();

builder.Services.AddCors(opts => opts.AddPolicy("Frontend", p =>
    p.WithOrigins(builder.Configuration["AllowedOrigins"]!.Split(',')) // Contient localhost ou l'URL de production du frontend
     .AllowAnyMethod()
     .AllowAnyHeader()
     .AllowCredentials()));

var app = builder.Build();

// ====== Middleware Pipeline ======
// Global exception handler (DOIT être en premier)
app.UseMiddleware<GlobalExceptionMiddleware>();

// ====== Auto-migrate on startup ====== + Initialisation seeder
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();

    var services = scope.ServiceProvider;
    await DatabaseInitializer.InitializeRolesAsync(services);
}

if (app.Environment.IsDevelopment()) 
{ 
    app.MapOpenApi(); // Swagger / Scalar 
}

app.UseCors("Frontend");
app.UseHttpsRedirection();

// L'ordre est très important ici !
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
