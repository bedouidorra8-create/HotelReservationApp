using HotelReservation.Domain.Entities;
using HotelReservation.Application.DomainEvents;
using HotelReservation.Domain.Common;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace HotelReservation.Infrastructure.Data;

public class AppDbContext : DbContext
{
    private readonly IMediator _mediator;

    public DbSet<Hotel> Hotels => Set<Hotel>();
    public DbSet<Chambre> Chambres => Set<Chambre>();
    public DbSet<Reservation> Reservations => Set<Reservation>();
    public DbSet<Utilisateur> Utilisateurs => Set<Utilisateur>();
    public DbSet<Client> Clients => Set<Client>();
    public DbSet<Personnel> Personnels => Set<Personnel>();
    public DbSet<Agent> Agents => Set<Agent>();
    public DbSet<Administrateur> Administrateurs => Set<Administrateur>();
    public DbSet<Paiement> Paiements => Set<Paiement>();
    public DbSet<Avis> Avis => Set<Avis>();
    public DbSet<Role> Roles => Set<Role>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<Favori> Favoris => Set<Favori>();

    public AppDbContext(DbContextOptions<AppDbContext> options, IMediator mediator) : base(options)
    {
        _mediator = mediator;
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // 1. Hotel Configuration
        modelBuilder.Entity<Hotel>(builder =>
        {
            builder.ToTable("Hotels");
            builder.HasKey(h => h.Id);

            builder.Property(h => h.Nom).HasMaxLength(150).IsRequired();
            builder.Property(h => h.Ville).HasMaxLength(100).IsRequired();
            builder.Property(h => h.Adresse).HasMaxLength(250).IsRequired();
            builder.Property(h => h.Telephone).HasMaxLength(30);
            builder.Property(h => h.Statut).HasConversion<int>().IsRequired();
            builder.Property(h => h.Images)
                .HasConversion(
                    images => JsonSerializer.Serialize(images, (JsonSerializerOptions?)null),
                    images => JsonSerializer.Deserialize<List<string>>(images, (JsonSerializerOptions?)null) ?? new())
                .HasColumnType("json");
            builder.Property(h => h.Images)
                .Metadata.SetValueComparer(new ValueComparer<List<string>>(
                    (left, right) => left != null && right != null && left.SequenceEqual(right),
                    value => value.Aggregate(0, (hash, item) => HashCode.Combine(hash, item.GetHashCode())),
                    value => value.ToList()));

            // Value Object: Email
            builder.OwnsOne(h => h.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .HasMaxLength(150)
                     .IsRequired();
            });

            // Navigation to Chambres with CASCADE delete
            builder.HasMany(h => h.Chambres)
                   .WithOne()
                   .HasForeignKey(c => c.HotelId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // 2. Chambre Configuration
        modelBuilder.Entity<Chambre>(builder =>
        {
            builder.ToTable("Chambres");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.Numero).HasMaxLength(20).IsRequired();
            builder.Property(c => c.Type).HasConversion<int>().IsRequired();
            builder.Property(c => c.Actif).HasDefaultValue(true).IsRequired();
            builder.Property(c => c.Statut).HasConversion<int>().IsRequired();

            // Value Object: Money
            builder.OwnsOne(c => c.Prix, prix =>
            {
                prix.Property(p => p.Amount)
                    .HasColumnName("PrixMontant")
                    .HasPrecision(18, 2)
                    .IsRequired();

                prix.Property(p => p.Currency)
                    .HasColumnName("PrixDevise")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            // SQL Constraint: UNIQUE (HotelId, Numero)
            builder.HasIndex(c => new { c.HotelId, c.Numero }).IsUnique();
        });

        // 3. Utilisateur Hierarchy Configuration (TPH)
        modelBuilder.Entity<Utilisateur>(builder =>
        {
            builder.ToTable("Utilisateurs");
            builder.HasKey(u => u.Id);

            builder.HasDiscriminator<string>("TypeUtilisateur")
                   .HasValue<Client>("Client")
                   .HasValue<Agent>("Agent")
                   .HasValue<Administrateur>("Administrateur");

            builder.Property(u => u.Nom).HasMaxLength(100).IsRequired();
            builder.Property(u => u.Prenom).HasMaxLength(100).IsRequired();
            builder.Property(u => u.MotDePasseHash).HasMaxLength(250).IsRequired();
            builder.Property(u => u.Telephone).HasMaxLength(30);
            builder.Property(u => u.Sexe).HasConversion<int?>().HasColumnName("Sexe");

            // Colonnes auth
            builder.Property(u => u.EmailVerifie).HasDefaultValue(false).IsRequired();
            builder.Property(u => u.TokenVerificationEmail).HasMaxLength(200);
            builder.Property(u => u.TokenVerificationEmailExpiration);
            builder.Property(u => u.TokenResetMotDePasse).HasMaxLength(200);
            builder.Property(u => u.TokenResetMotDePasseExpiration);

            // Value Object: Email with UNIQUE constraint
            builder.OwnsOne(u => u.Email, email =>
            {
                email.Property(e => e.Value)
                     .HasColumnName("Email")
                     .HasMaxLength(150)
                     .IsRequired();

                email.HasIndex(e => e.Value).IsUnique();
            });
        });

        // 8. RefreshToken Configuration
        modelBuilder.Entity<RefreshToken>(builder =>
        {
            builder.ToTable("RefreshTokens");
            builder.HasKey(rt => rt.Id);

            builder.Property(rt => rt.Token).HasMaxLength(500).IsRequired();
            builder.HasIndex(rt => rt.Token).IsUnique();

            builder.HasOne<Utilisateur>()
                   .WithMany()
                   .HasForeignKey(rt => rt.UtilisateurId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // 4. Reservation Configuration
        modelBuilder.Entity<Reservation>(builder =>
        {
            builder.ToTable("Reservations");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Statut).HasConversion<int>().IsRequired();

            builder.Property(r => r.DateArrivee).IsRequired();
            builder.Property(r => r.DateDepart).IsRequired();
            builder.Ignore(r => r.Periode);

            // Value Object: Money (PrixUnitaire)
            builder.OwnsOne(r => r.PrixUnitaire, prix =>
            {
                prix.Property(p => p.Amount)
                    .HasColumnName("PrixUnitaireMontant")
                    .HasPrecision(18, 2)
                    .IsRequired();

                prix.Property(p => p.Currency)
                    .HasColumnName("PrixUnitaireDevise")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            // Value Object: Money (Total)
            builder.OwnsOne(r => r.Total, total =>
            {
                total.Property(t => t.Amount)
                    .HasColumnName("TotalMontant")
                    .HasPrecision(18, 2)
                    .IsRequired();

                total.Property(t => t.Currency)
                    .HasColumnName("TotalDevise")
                    .HasMaxLength(10)
                    .IsRequired();
            });

            // SQL Constraint: ON DELETE RESTRICT from Chambre to Reservation
            builder.HasOne<Chambre>()
                   .WithMany()
                   .HasForeignKey(r => r.ChambreId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne<Client>()
                   .WithMany()
                   .HasForeignKey(r => r.ClientId)
                   .OnDelete(DeleteBehavior.Restrict);
        });

        // 5. Paiement Configuration
        modelBuilder.Entity<Paiement>(builder =>
        {
            builder.ToTable("Paiements");
            builder.HasKey(p => p.Id);

            builder.Property(p => p.ModePaiement).HasConversion<int>().IsRequired();
            builder.Property(p => p.Statut).HasConversion<int>().IsRequired();
            builder.Property(p => p.Reference).HasMaxLength(100);

            // Value Object: Money
            builder.OwnsOne(p => p.Montant, montant =>
            {
                montant.Property(m => m.Amount)
                       .HasColumnName("Montant")
                       .HasPrecision(18, 2)
                       .IsRequired();

                montant.Property(m => m.Currency)
                       .HasColumnName("Devise")
                       .HasMaxLength(10)
                       .IsRequired();
            });

            // SQL Constraint: ON DELETE RESTRICT from Reservation to Paiement
            builder.HasOne<Reservation>()
                   .WithMany()
                   .HasForeignKey(p => p.ReservationId)
                   .OnDelete(DeleteBehavior.Restrict);

            // SQL Constraint: ON DELETE SET NULL from Personnel to Paiement.EnregistrePar
            builder.HasOne<Personnel>()
                   .WithMany()
                   .HasForeignKey(p => p.EnregistrePar)
                   .OnDelete(DeleteBehavior.SetNull);
        });

        // 6. Avis Configuration
        modelBuilder.Entity<Avis>(builder =>
        {
            builder.ToTable("Avis", t =>
            {
                t.HasCheckConstraint("CK_Avis_Note", "Note >= 1 AND Note <= 5");
            });

            builder.HasKey(a => a.Id);
            builder.Property(a => a.Commentaire).HasMaxLength(1000);
            builder.Property(a => a.Statut).HasConversion<int>().IsRequired();

            // Value Object: NoteAvis
            builder.OwnsOne(a => a.Note, note =>
            {
                note.Property(n => n.Valeur)
                    .HasColumnName("Note")
                    .IsRequired();
            });

            // SQL Constraint: UNIQUE (ClientId, ReservationId)
            builder.HasIndex(a => new { a.ClientId, a.ReservationId }).IsUnique();

            // SQL Constraints: ON DELETE CASCADE (Client -> Avis, Hotel -> Avis)
            builder.HasOne<Client>()
                   .WithMany()
                   .HasForeignKey(a => a.ClientId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne<Hotel>()
                   .WithMany()
                   .HasForeignKey(a => a.HotelId)
                   .OnDelete(DeleteBehavior.Cascade);
        });

        // 7. Role Configuration
        modelBuilder.Entity<Role>(builder =>
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Nom).HasMaxLength(50).IsRequired();
            builder.HasIndex(r => r.Nom).IsUnique();

            builder.Property(r => r.Permissions)
                .HasField("_permissions")
                .UsePropertyAccessMode(PropertyAccessMode.Field)
                .HasConversion(
                    perms => JsonSerializer.Serialize(perms, (JsonSerializerOptions?)null),
                    json => JsonSerializer.Deserialize<List<string>>(json, (JsonSerializerOptions?)null) ?? new List<string>())
                .HasColumnType("json");

            builder.Property(r => r.Permissions)
                .Metadata.SetValueComparer(new ValueComparer<IReadOnlyCollection<string>>(
                    (c1, c2) => c1 != null && c2 != null && c1.SequenceEqual(c2),
                    c => c.Aggregate(0, (a, v) => HashCode.Combine(a, v.GetHashCode())),
                    c => c.ToList().AsReadOnly()));
        });

        modelBuilder.Entity<AuditLog>(builder =>
        {
            builder.ToTable("AuditLogs");
            builder.HasKey(log => log.Id);
            builder.Property(log => log.Action).HasMaxLength(100).IsRequired();
            builder.Property(log => log.Entite).HasMaxLength(100).IsRequired();
            builder.Property(log => log.AncienneValeur).HasColumnType("json");
            builder.Property(log => log.NouvelleValeur).HasColumnType("json");
            builder.HasIndex(log => log.DateHeure);
            builder.HasIndex(log => new { log.Entite, log.EntiteId });
        });

        modelBuilder.Entity<Favori>(builder =>
        {
            builder.ToTable("favoris");
            builder.HasKey(f => f.Id);
            builder.Property(f => f.Id).HasColumnType("char(36)");
            builder.Property(f => f.UtilisateurId).HasColumnType("char(36)").IsRequired();
            builder.Property(f => f.HotelId).HasColumnType("char(36)").IsRequired();
            builder.Property(f => f.DateAjout).HasColumnType("datetime(6)").IsRequired();
            builder.HasIndex(f => new { f.UtilisateurId, f.HotelId }).IsUnique();
            builder.HasOne<Utilisateur>().WithMany().HasForeignKey(f => f.UtilisateurId).OnDelete(DeleteBehavior.Cascade);
            builder.HasOne<Hotel>().WithMany().HasForeignKey(f => f.HotelId).OnDelete(DeleteBehavior.Cascade);
        });
    }

    /// <summary>
    /// Persiste les changements métier puis publie les événements de domaine.
    /// Les audits sont enregistrés explicitement par les use cases via IAuditLogService.
    /// </summary>
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var domainEvents = ChangeTracker.Entries<AggregateRoot<Guid>>()
            .SelectMany(entry => entry.Entity.DomainEvents)
            .ToList();

        var resultat = await base.SaveChangesAsync(cancellationToken);

        foreach (var aggregate in ChangeTracker.Entries<AggregateRoot<Guid>>().Select(entry => entry.Entity))
            aggregate.ClearDomainEvents();

        foreach (var domainEvent in domainEvents)
        {
            var notificationType = typeof(DomainEventNotification<>).MakeGenericType(domainEvent.GetType());
            var notification = (INotification)Activator.CreateInstance(notificationType, domainEvent)!;
            await _mediator.Publish(notification, cancellationToken);
        }

        return resultat;
    }
}
