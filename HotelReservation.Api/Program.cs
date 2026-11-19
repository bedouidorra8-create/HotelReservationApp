using System.Text;
using HotelReservation.Application.Services;
using HotelReservation.Application.UseCases;
using HotelReservation.Domain.Repositories;
using HotelReservation.Domain.Services;
using HotelReservation.Infrastructure.Data;
using HotelReservation.Infrastructure.Repositories;
using HotelReservation.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using HotelReservation.Application.DomainEvents;
using FluentValidation;
using HotelReservation.Api.Middleware;

var builder = WebApplication.CreateBuilder(args);

// 1. Add Controllers & OpenAPI
builder.Services.AddControllers();
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(DomainEventNotification<>).Assembly));
builder.Services.AddValidatorsFromAssembly(typeof(DomainEventNotification<>).Assembly);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new() { Title = "HotelReservation API", Version = "v1" });
    // Support JWT dans Swagger UI
    c.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Entrez le token JWT : Bearer {votre_token}"
    });
    c.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// 2. Configure CORS for Angular frontend (http://localhost:4200)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngularFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

// 3. Configure MySQL Database via Pomelo
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Server=localhost;Database=HotelReservationDb;User=root;Password=CHANGE_ME;";

var serverVersion = new MySqlServerVersion(new Version(8, 0, 35));

builder.Services.AddDbContext<AppDbContext>(options =>
{
    options.UseMySql(connectionString, serverVersion);
});

// 4. Configure JWT Bearer Authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key manquant dans appsettings.json");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer           = true,
            ValidateAudience         = true,
            ValidateLifetime         = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer              = builder.Configuration["Jwt:Issuer"] ?? "HotelReservationApi",
            ValidAudience            = builder.Configuration["Jwt:Audience"] ?? "HotelReservationClient",
            IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ClockSkew                = TimeSpan.Zero // Pas de tolerance : 15min = 15min exactement
        };
    });

builder.Services.AddAuthorization();

// 5. Register Domain Services
builder.Services.AddScoped<ReservationDomainService>();

// 6. Register Repositories
builder.Services.AddScoped<IHotelRepository, HotelRepository>();
builder.Services.AddScoped<IReservationRepository, ReservationRepository>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IUtilisateurRepository, UtilisateurRepository>();
builder.Services.AddScoped<IPaiementRepository, PaiementRepository>();
builder.Services.AddScoped<IAvisRepository, AvisRepository>();
builder.Services.AddScoped<IRoleRepository, RoleRepository>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IAuditLogRepository, AuditLogRepository>();
    builder.Services.AddScoped<IFavoriRepository, FavoriRepository>();

// 7. Register Application Services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, HotelReservation.Api.Services.CurrentUserService>();
builder.Services.AddScoped<IDateTimeProvider, DateTimeProvider>();
builder.Services.AddScoped<IFacturePdfService, FacturePdfService>();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<ITableauDeBordService, TableauDeBordService>();
builder.Services.AddScoped<IStatistiqueService, StatistiqueService>();

// 8. Register Use Cases — existing
builder.Services.AddScoped<CreerReservationUseCase>();
builder.Services.AddScoped<ModifierReservationUseCase>();
builder.Services.AddScoped<ChangerStatutReservationUseCase>();
builder.Services.AddScoped<ConsulterListeReservationsUseCase>();
builder.Services.AddScoped<ConsulterMesReservationsUseCase>();
builder.Services.AddScoped<RechercherReservationUseCase>();
builder.Services.AddScoped<AnnulerReservationUseCase>();
builder.Services.AddScoped<ConfirmerReservationUseCase>();
builder.Services.AddScoped<ConsulterDisponibilitesUseCase>();
builder.Services.AddScoped<ConsulterCalendrierReservationsUseCase>();
builder.Services.AddScoped<ConsulterReservationUseCase>();
builder.Services.AddScoped<EnregistrerPaiementUseCase>();
builder.Services.AddScoped<AnnulerPaiementUseCase>();
builder.Services.AddScoped<ConsulterPaiementsUseCase>();
builder.Services.AddScoped<ConsulterTousPaiementsUseCase>();
builder.Services.AddScoped<GenererFactureUseCase>();
builder.Services.AddScoped<GenererFacturePdfUseCase>();
builder.Services.AddScoped<AjouterAvisUseCase>();
builder.Services.AddScoped<ModifierAvisUseCase>();
builder.Services.AddScoped<SupprimerAvisUseCase>();
builder.Services.AddScoped<ConsulterAvisHotelUseCase>();
builder.Services.AddScoped<ConsulterAvisClientUseCase>();
builder.Services.AddScoped<ConsulterTousAvisUseCase>();
builder.Services.AddScoped<ConsulterAvisParIdUseCase>();
builder.Services.AddScoped<CreerChambreUseCase>();
builder.Services.AddScoped<ConsulterChambresUseCase>();
builder.Services.AddScoped<ModifierChambreUseCase>();
builder.Services.AddScoped<SupprimerChambreUseCase>();
builder.Services.AddScoped<ChangerStatutChambreUseCase>();
builder.Services.AddScoped<ActiverDesactiverChambreUseCase>();
builder.Services.AddScoped<RechercherChambreUseCase>();
builder.Services.AddScoped<CreerHotelUseCase>();
builder.Services.AddScoped<SupprimerHotelUseCase>();
builder.Services.AddScoped<ModifierHotelUseCase>();
builder.Services.AddScoped<ActiverDesactiverHotelUseCase>();
builder.Services.AddScoped<ConsulterHotelsUseCase>();
builder.Services.AddScoped<CreerClientUseCase>();
builder.Services.AddScoped<ConsulterClientUseCase>();
builder.Services.AddScoped<ConsulterListeClientsUseCase>();
builder.Services.AddScoped<RechercherClientUseCase>();
builder.Services.AddScoped<ModifierClientUseCase>();
builder.Services.AddScoped<SupprimerClientUseCase>();

builder.Services.AddScoped<ConsulterListeUtilisateursUseCase>();
builder.Services.AddScoped<ConsulterUtilisateurUseCase>();
builder.Services.AddScoped<RechercherUtilisateurUseCase>();
builder.Services.AddScoped<ModifierAgentUseCase>();
builder.Services.AddScoped<SupprimerAgentUseCase>();
builder.Services.AddScoped<ModifierAdministrateurUseCase>();
builder.Services.AddScoped<SupprimerAdministrateurUseCase>();
builder.Services.AddScoped<AjouterUtilisateurUseCase>();
builder.Services.AddScoped<ActiverDesactiverCompteUseCase>();
builder.Services.AddScoped<GererRolesUseCase>();
builder.Services.AddScoped<ValiderAvisUseCase>();
builder.Services.AddScoped<RejeterAvisUseCase>();

// 9. Register Use Cases — new auth
builder.Services.AddScoped<InscrireUtilisateurUseCase>();
builder.Services.AddScoped<SeConnecterUseCase>();
builder.Services.AddScoped<RafraichirTokenUseCase>();
builder.Services.AddScoped<DemanderResetPasswordUseCase>();
builder.Services.AddScoped<ReinitialiserMotDePasseUseCase>();
builder.Services.AddScoped<ChangerMotDePasseUseCase>();
builder.Services.AddScoped<VerifierEmailUseCase>();
builder.Services.AddScoped<DeconnexionUseCase>();
builder.Services.AddScoped<ConsulterMonProfilUseCase>();
builder.Services.AddScoped<ModifierSonProfilUseCase>();

// 10. Register Use Cases — new search & analytics
builder.Services.AddScoped<RechercherHotelsUseCase>();
builder.Services.AddScoped<ConsulterStatistiquesUseCase>();
builder.Services.AddScoped<ConsulterDashboardUseCase>();
builder.Services.AddScoped<ConsulterAuditLogsUseCase>();
builder.Services.AddScoped<AjouterFavoriUseCase>();
builder.Services.AddScoped<SupprimerFavoriUseCase>();
builder.Services.AddScoped<ConsulterFavorisUseCase>();

var app = builder.Build();

// Configure HTTP pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
    await HotelReservation.Infrastructure.Data.DbSeeder.SeedAsync(app.Services);
}

app.UseCors("AllowAngularFrontend");
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseAuthentication(); // IMPORTANT: avant UseAuthorization
app.UseAuthorization();
app.MapControllers();

app.Run();

