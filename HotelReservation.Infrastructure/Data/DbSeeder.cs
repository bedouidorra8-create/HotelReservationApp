using BCrypt.Net;
using HotelReservation.Domain.Entities;
using HotelReservation.Domain.Enums;
using HotelReservation.Domain.Services;
using HotelReservation.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Infrastructure.Data;

public static class DbSeeder
{
    public static async Task SeedAsync(IServiceProvider serviceProvider)
    {
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var logger = scope.ServiceProvider.GetService<ILogger<AppDbContext>>();

        // =========================================================================
        // BLOC 1 : RÔLES SYSTÈME
        // =========================================================================
        try
        {
            var rolesAttendus = new[]
            {
                new { Nom = "Administrateur", Desc = "Gestionnaire global de la plateforme HOTELI, utilisateurs, configurations et modération.", Perms = new[] { "TOUT_ACCES", "GERER_UTILISATEURS", "GERER_ROLES", "MODERER_AVIS", "CONSULTER_STATISTIQUES" } },
                new { Nom = "Agent", Desc = "Personnel hôtelier opérationnel : gestion des chambres, réservations, paiements et clients.", Perms = new[] { "GERER_CHAMBRES", "GERER_RESERVATIONS", "ENREGISTRER_PAIEMENTS", "CONSULTER_CLIENTS" } },
                new { Nom = "Client", Desc = "Voyageur : recherche d'hôtels, réservation, paiement sécurisé et avis vérifiés.", Perms = new[] { "RESERVER_CHAMBRE", "PAYER_RESERVATION", "DEPOSER_AVIS", "GERER_FAVORIS" } }
            };

            foreach (var r in rolesAttendus)
            {
                var roleExiste = await context.Roles.FirstOrDefaultAsync(role => role.Nom == r.Nom);
                if (roleExiste == null)
                {
                    var role = new Role(Guid.NewGuid(), r.Nom, r.Desc);
                    foreach (var perm in r.Perms)
                        role.AjouterPermission(perm);
                    await context.Roles.AddAsync(role);
                }
            }
            await context.SaveChangesAsync();
            logger?.LogInformation("[DbSeeder] Bloc 1 (Rôles) validé.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 1 (Rôles) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 2 : RÉACTIVATION DES HÔTELS EXISTANTS INACTIFS
        // =========================================================================
        try
        {
            var existingHotel1 = await context.Hotels.FirstOrDefaultAsync(h => h.Id == Guid.Parse("a1111111-0000-0000-0000-000000000001"));
            if (existingHotel1 != null && existingHotel1.Statut != StatutHotel.Actif)
            {
                existingHotel1.ActiverDesactiver(StatutHotel.Actif);
                logger?.LogInformation("[DbSeeder] Hôtel 'The Residence Tunis' réactivé.");
            }

            var existingHotel2 = await context.Hotels.FirstOrDefaultAsync(h => h.Id == Guid.Parse("a2222222-0000-0000-0000-000000000002"));
            if (existingHotel2 != null && existingHotel2.Statut != StatutHotel.Actif)
            {
                existingHotel2.ActiverDesactiver(StatutHotel.Actif);
                logger?.LogInformation("[DbSeeder] Hôtel 'Four Seasons Hotel Tunis' réactivé.");
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("[DbSeeder] Bloc 2 (Réactivation hôtels) validé.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 2 (Réactivation hôtels) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 3 : PERSONNEL (ADMINISTRATEURS & AGENTS)
        // =========================================================================
        try
        {
            // Admin 1 (existant)
            var adminId1 = Guid.Parse("11111111-1111-1111-1111-111111111111");
            if (!await context.Administrateurs.AnyAsync(a => a.Id == adminId1 || a.Email.Value == "admin@hotelapp.com"))
            {
                var admin1 = new Administrateur(
                    adminId1,
                    "Ben Salah",
                    "Mehdi",
                    new Email("admin@hotelapp.com"),
                    BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    "+216 71 000 001",
                    new DateTime(1985, 5, 15, 0, 0, 0, DateTimeKind.Utc),
                    Genre.Homme,
                    "Avenue Habib Bourguiba, Tunis",
                    "ADM-001",
                    "https://images.unsplash.com/photo-1534528741775-53994a69daeb"
                );
                admin1.ConfirmerEmailDirectement();
                admin1.DefinirDateCreation(DateTime.UtcNow.AddMonths(-12));
                await context.Administrateurs.AddAsync(admin1);
            }

            // Admin 2 (nouveau GUID unique)
            var adminId2 = Guid.Parse("11111111-0000-0000-0000-000000000002");
            if (!await context.Administrateurs.AnyAsync(a => a.Id == adminId2 || a.Email.Value == "directeur@hotelapp.com"))
            {
                var admin2 = new Administrateur(
                    adminId2,
                    "Khedhir",
                    "Slim",
                    new Email("directeur@hotelapp.com"),
                    BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                    "+216 73 100 002",
                    new DateTime(1978, 11, 4, 0, 0, 0, DateTimeKind.Utc),
                    Genre.Homme,
                    "Boulevard du 14 Janvier, Sousse",
                    "DIR-002",
                    "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d"
                );
                admin2.ConfirmerEmailDirectement();
                admin2.DefinirDateCreation(DateTime.UtcNow.AddMonths(-11));
                await context.Administrateurs.AddAsync(admin2);
            }

            // Agents régionaux avec GUIDs uniques garantis
            var agentsSpecs = new[]
            {
                new { Id = Guid.Parse("22222222-2222-2222-2222-222222222222"), Nom = "Trabelsi", Prenom = "Sonia", Email = "agent@hotelapp.com", Tel = "+216 71 000 002", Naissance = new DateTime(1992, 8, 20, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Les Berges du Lac 2, Tunis", Mat = "AGT-TUN-01", Photo = "https://images.unsplash.com/photo-1573496359142-b8d87734a5a2" },
                new { Id = Guid.Parse("22222222-0000-0000-0000-000000000002"), Nom = "Khemir", Prenom = "Tarek", Email = "agent.tozeur@hotelapp.com", Tel = "+216 76 450 110", Naissance = new DateTime(1989, 3, 14, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Palmeraie de Tozeur", Mat = "AGT-TOZ-02", Photo = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e" },
                new { Id = Guid.Parse("22222222-0000-0000-0000-000000000003"), Nom = "Ben Amor", Prenom = "Meriem", Email = "agent.djerba@hotelapp.com", Tel = "+216 75 650 330", Naissance = new DateTime(1994, 6, 25, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Houmt Souk, Djerba", Mat = "AGT-DJE-03", Photo = "https://images.unsplash.com/photo-1544005313-94ddf0286df2" },
                new { Id = Guid.Parse("22222222-0000-0000-0000-000000000004"), Nom = "Mahjoub", Prenom = "Anis", Email = "agent.sahel@hotelapp.com", Tel = "+216 72 280 440", Naissance = new DateTime(1991, 10, 5, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Yasmine Hammamet", Mat = "AGT-SAH-04", Photo = "https://images.unsplash.com/photo-1519085360753-af0119f7cbe7" }
            };

            foreach (var ag in agentsSpecs)
            {
                if (!await context.Agents.AnyAsync(a => a.Id == ag.Id || a.Email.Value == ag.Email))
                {
                    var agent = new Agent(
                        ag.Id,
                        ag.Nom,
                        ag.Prenom,
                        new Email(ag.Email),
                        BCrypt.Net.BCrypt.HashPassword("Agent123!"),
                        ag.Tel,
                        ag.Naissance,
                        ag.Sexe,
                        ag.Ville,
                        ag.Mat,
                        ag.Photo
                    );
                    agent.ConfirmerEmailDirectement();
                    agent.DefinirDateCreation(DateTime.UtcNow.AddMonths(-10));
                    await context.Agents.AddAsync(agent);
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("[DbSeeder] Bloc 3 (Personnel) validé.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 3 (Personnel) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 4 : CLIENTS VOYAGEURS (20 CLIENTS)
        // =========================================================================
        try
        {
            var clientsBaseSpecs = new[]
            {
                new { Id = Guid.Parse("33333333-3333-3333-3333-333333333333"), Nom = "Gharbi", Prenom = "Youssef", Email = "client@hotelapp.com", Tel = "+216 98 123 456", Naissance = new DateTime(1996, 3, 10, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Ennasr 2, Ariana", Photo = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d", MoisOffset = 11 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000002"), Nom = "Bouzid", Prenom = "Ines", Email = "ines.bouzid@gmail.com", Tel = "+216 22 345 678", Naissance = new DateTime(1993, 7, 18, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Sahloul, Sousse", Photo = "https://images.unsplash.com/photo-1534528741775-53994a69daeb", MoisOffset = 11 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000003"), Nom = "Mansouri", Prenom = "Karim", Email = "karim.mansouri@yahoo.fr", Tel = "+216 55 456 789", Naissance = new DateTime(1988, 12, 2, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "La Marsa, Tunis", Photo = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e", MoisOffset = 10 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000004"), Nom = "Chaabane", Prenom = "Leïla", Email = "leila.chaabane@outlook.com", Tel = "+216 97 567 890", Naissance = new DateTime(1995, 4, 29, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Route de Téniour, Sfax", Photo = "https://images.unsplash.com/photo-1544005313-94ddf0286df2", MoisOffset = 10 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000005"), Nom = "Jaziri", Prenom = "Mohamed Ali", Email = "m.jaziri@topnet.tn", Tel = "+216 29 678 901", Naissance = new DateTime(1982, 9, 15, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Corniche de Bizerte", Photo = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce", MoisOffset = 9 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000006"), Nom = "Laurent", Prenom = "Sophie", Email = "sophie.laurent@voyage.fr", Tel = "+33 6 12 34 56 78", Naissance = new DateTime(1990, 1, 22, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Paris, France", Photo = "https://images.unsplash.com/photo-1517841905240-472988babdf9", MoisOffset = 9 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000007"), Nom = "Ben Rhouma", Prenom = "Ahmed", Email = "ahmed.rhouma@gnet.tn", Tel = "+216 50 789 012", Naissance = new DateTime(1991, 5, 8, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Centre-ville, Nabeul", Photo = "https://images.unsplash.com/photo-1522075469751-3a6694fb2f61", MoisOffset = 8 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000008"), Nom = "Khalfallah", Prenom = "Nour", Email = "nour.khalfallah@gmail.com", Tel = "+216 94 890 123", Naissance = new DateTime(1998, 11, 30, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Marina de Monastir", Photo = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1", MoisOffset = 8 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000009"), Nom = "Dridi", Prenom = "Firas", Email = "firas.dridi@gmail.com", Tel = "+216 23 456 780", Naissance = new DateTime(1987, 6, 12, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Carthage Byrsa, Tunis", Photo = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d", MoisOffset = 7 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000010"), Nom = "Masmoudi", Prenom = "Salma", Email = "salma.masmoudi@yahoo.com", Tel = "+216 93 112 233", Naissance = new DateTime(1994, 2, 17, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Bab Diwan, Sfax", Photo = "https://images.unsplash.com/photo-1534528741775-53994a69daeb", MoisOffset = 7 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000011"), Nom = "Cherif", Prenom = "Wassim", Email = "wassim.cherif@outlook.com", Tel = "+216 52 334 455", Naissance = new DateTime(1990, 8, 9, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Kantaoui, Sousse", Photo = "https://images.unsplash.com/photo-1500648767791-00dcc994a43e", MoisOffset = 6 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000012"), Nom = "Belhadj", Prenom = "Amira", Email = "amira.belhadj@gmail.com", Tel = "+216 28 556 677", Naissance = new DateTime(1996, 10, 25, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "El Menzah 9, Tunis", Photo = "https://images.unsplash.com/photo-1544005313-94ddf0286df2", MoisOffset = 6 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000013"), Nom = "Guediche", Prenom = "Hedi", Email = "hedi.guediche@gmail.com", Tel = "+216 99 778 899", Naissance = new DateTime(1984, 4, 3, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Médina de Mahdia", Photo = "https://images.unsplash.com/photo-1492562080023-ab3db95bfbce", MoisOffset = 5 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000014"), Nom = "Müller", Prenom = "Claudia", Email = "claudia.muller@reise.de", Tel = "+49 151 2345678", Naissance = new DateTime(1986, 12, 14, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Munich, Allemagne", Photo = "https://images.unsplash.com/photo-1517841905240-472988babdf9", MoisOffset = 5 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000015"), Nom = "Zaier", Prenom = "Oussama", Email = "oussama.zaier@topnet.tn", Tel = "+216 26 889 900", Naissance = new DateTime(1992, 1, 19, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Oasis de Gabès", Photo = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d", MoisOffset = 4 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000016"), Nom = "Ayari", Prenom = "Rym", Email = "rym.ayari@gmail.com", Tel = "+216 95 001 122", Naissance = new DateTime(1997, 7, 8, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Bardo, Tunis", Photo = "https://images.unsplash.com/photo-1524504388940-b1c1722653e1", MoisOffset = 4 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000017"), Nom = "Rossi", Prenom = "Marco", Email = "marco.rossi@viaggi.it", Tel = "+39 340 1234567", Naissance = new DateTime(1989, 5, 27, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Milan, Italie", Photo = "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d", MoisOffset = 3 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000018"), Nom = "Triki", Prenom = "Sarra", Email = "sarra.triki@yahoo.fr", Tel = "+216 54 223 344", Naissance = new DateTime(1995, 9, 30, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "La Goulette, Tunis", Photo = "https://images.unsplash.com/photo-1534528741775-53994a69daeb", MoisOffset = 3 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000019"), Nom = "Othmane", Prenom = "Bilel", Email = "bilel.othmane@topnet.tn", Tel = "+216 96 112 244", Naissance = new DateTime(1988, 2, 28, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Homme, Ville = "Zone Touristique Djerba", Photo = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d", MoisOffset = 2 },
                new { Id = Guid.Parse("33333333-0000-0000-0000-000000000020"), Nom = "Gara", Prenom = "Kenza", Email = "kenza.gara@yahoo.com", Tel = "+216 27 334 466", Naissance = new DateTime(1994, 6, 18, 0, 0, 0, DateTimeKind.Utc), Sexe = Genre.Femme, Ville = "Centre-ville, Nabeul", Photo = "https://images.unsplash.com/photo-1534528741775-53994a69daeb", MoisOffset = 1 }
            };

            foreach (var c in clientsBaseSpecs)
            {
                if (!await context.Clients.AnyAsync(cl => cl.Id == c.Id || cl.Email.Value == c.Email))
                {
                    var newClient = new Client(
                        c.Id,
                        c.Nom,
                        c.Prenom,
                        new Email(c.Email),
                        BCrypt.Net.BCrypt.HashPassword("Client123!"),
                        c.Tel,
                        c.Naissance,
                        c.Sexe,
                        c.Ville,
                        c.Photo
                    );
                    newClient.ConfirmerEmailDirectement();
                    newClient.DefinirDateCreation(DateTime.UtcNow.AddMonths(-c.MoisOffset));
                    await context.Clients.AddAsync(newClient);
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("[DbSeeder] Bloc 4 (Clients) validé.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 4 (Clients) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 5 : 16 NOUVEAUX HÔTELS + CHAMBRES (TOTAL = 20 HÔTELS)
        // =========================================================================
        try
        {
            var nouveauxHotels = new List<(Hotel Hotel, List<(string Numero, TypeChambre Type, int Etage, Money Prix)> Chambres)>();

            // --- 10 NOUVEAUX HÔTELS TUNISIENS (TND) ---
            // 5. Sidi Bou Saïd
            var h5 = new Hotel(
                Guid.Parse("c1111111-0000-0000-0000-000000000005"),
                "Dar Saïd — Maison d'Hôtes & Belvédère",
                "Sidi Bou Saïd",
                "Rue Toumi, 2026 Sidi Bou Saïd, Golfe de Tunis",
                5,
                "Demeure arabo-andalouse du milieu du XIXe siècle perchée sur la colline mythique de Sidi Bou Saïd. Patios pavés de marbre, moucharabiehs d'azur, effluves de jasmin et terrasse suspendue face au bleu infini de la Méditerranée.",
                "+216 71 729 666",
                new Email("reservation@darsaid.com.tn"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff",
                    "https://images.unsplash.com/photo-1533105079780-92b9be482077",
                    "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b"
                }
            );
            nouveauxHotels.Add((h5, new()
            {
                ("101", TypeChambre.Simple, 1, new Money(320.00m, "TND")),
                ("102", TypeChambre.Double, 1, new Money(420.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(790.00m, "TND")),
                ("202", TypeChambre.Double, 2, new Money(460.00m, "TND"))
            }));

            // 6. Tozeur
            var h6 = new Hotel(
                Guid.Parse("c2222222-0000-0000-0000-000000000006"),
                "Anantara Sahara Tozeur Resort & Oasis",
                "Tozeur",
                "Route Mhrague, 2200 Tozeur",
                5,
                "Sanctuaire saharien majestueux conçu en briques artisanales ocres de Tozeur au bord des dunes dorées du Chott el-Jérid. Piscines privatives, palmeraie luxuriante, gastronomie berbère raffinée et nuits étoilées inoubliables sous le ciel du désert.",
                "+216 76 430 000",
                new Email("tozeur@anantara.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1509316975850-ff9c5deb0cd9",
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                    "https://images.unsplash.com/photo-1518684079-3c830dcef090"
                }
            );
            nouveauxHotels.Add((h6, new()
            {
                ("101", TypeChambre.Double, 1, new Money(580.00m, "TND")),
                ("102", TypeChambre.Suite, 1, new Money(990.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(1450.00m, "TND"))
            }));

            // 7. Djerba
            var h7 = new Hotel(
                Guid.Parse("c3333333-0000-0000-0000-000000000007"),
                "Dar Dhiafa — Menzel & Houch Insulaire",
                "Djerba",
                "Erriadh (Hara Sghira), 4146 Djerba",
                4,
                "Houch djerbien traditionnel réhabilité avec un art consommé au cœur du village artistique de Djerbahood. Voûtes chaulées à la chaux blanche, patios secrets ombragés de bougainvilliers éclatants, poteries de Guellala et quiétude insulaire absolue.",
                "+216 75 671 166",
                new Email("contact@dardhiafa.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1578683010236-d716f9a3f461",
                    "https://images.unsplash.com/photo-1590490360182-c33d57733427"
                }
            );
            nouveauxHotels.Add((h7, new()
            {
                ("101", TypeChambre.Simple, 1, new Money(190.00m, "TND")),
                ("102", TypeChambre.Double, 1, new Money(280.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(490.00m, "TND"))
            }));

            // 8. Mahdia
            var h8 = new Hotel(
                Guid.Parse("c4444444-0000-0000-0000-000000000008"),
                "Mahdia Palace Thalasso & Beach",
                "Mahdia",
                "Zone Touristique, 5111 Mahdia",
                5,
                "Écrin balnéaire bordé par une plage de sable fin immaculé et une mer turquoise cristalline. Architecture mauresque raffinée, marbre étincelant et centre de thalassothérapie d'eau de mer vivifiante inspiré des traditions de l'ancienne capitale fatimide.",
                "+216 73 683 777",
                new Email("palace.mahdia@topnet.tn"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                    "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4"
                }
            );
            nouveauxHotels.Add((h8, new()
            {
                ("101", TypeChambre.Double, 1, new Money(310.00m, "TND")),
                ("102", TypeChambre.Familiale, 1, new Money(480.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(690.00m, "TND"))
            }));

            // 9. Monastir
            var h9 = new Hotel(
                Guid.Parse("c5555555-0000-0000-0000-000000000009"),
                "Royal Thalassa Monastir",
                "Monastir",
                "Route Touristique Skanès, 5000 Monastir",
                5,
                "Sublime complexe posé au bord de l'eau, face à la forteresse historique du Ribat de Monastir. Jardins luxuriants d'oliviers et de palmiers, suites panoramiques avec terrasses privatives et spa marin d'envergure internationale.",
                "+216 73 520 520",
                new Email("contact@thalassa-monastir.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                    "https://images.unsplash.com/photo-1564501049412-61c2a3083791"
                }
            );
            nouveauxHotels.Add((h9, new()
            {
                ("101", TypeChambre.Double, 1, new Money(330.00m, "TND")),
                ("102", TypeChambre.Suite, 1, new Money(640.00m, "TND")),
                ("201", TypeChambre.Familiale, 2, new Money(510.00m, "TND"))
            }));

            // 10. Hammamet
            var h10 = new Hotel(
                Guid.Parse("c6666666-0000-0000-0000-000000000010"),
                "La Badira — Demeure Méditerranéenne",
                "Hammamet",
                "BP 437, Zone Touristique Mrezga, 8050 Hammamet",
                5,
                "Adresse d'exception réservée aux adultes, rendant hommage à la lumière et à la douceur de vivre d'Hammamet. Céramiques tunisiennes d'artiste, piscines à débordement suspendues face au large, senteurs de jasmin et haute gastronomie.",
                "+216 72 107 000",
                new Email("reservation@labadira.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1618773928121-c32242e63f39",
                    "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b"
                }
            );
            nouveauxHotels.Add((h10, new()
            {
                ("101", TypeChambre.Double, 1, new Money(420.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(790.00m, "TND")),
                ("202", TypeChambre.Suite, 2, new Money(880.00m, "TND"))
            }));

            // 11. Tunis Médina
            var h11 = new Hotel(
                Guid.Parse("c7777777-0000-0000-0000-000000000011"),
                "Palais Dar El Jeld Hotel & Spa",
                "Tunis Médina",
                "Rue Dar El Jeld, Médina de Tunis",
                5,
                "Palais patricien secret dissimulé au cœur des ruelles pavées de la Médina de Tunis (UNESCO). Zelliges ancestraux façonnés à la main, plafonds de bois sculpté doré, patio andalou embaumé de fleur d'oranger et conciergerie d'excellence.",
                "+216 70 016 190",
                new Email("palais@dareljeld.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                    "https://images.unsplash.com/photo-1534528741775-53994a69daeb"
                }
            );
            nouveauxHotels.Add((h11, new()
            {
                ("101", TypeChambre.Double, 1, new Money(450.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(850.00m, "TND")),
                ("202", TypeChambre.Suite, 2, new Money(990.00m, "TND"))
            }));

            // 12. Tabarka
            var h12 = new Hotel(
                Guid.Parse("c8888888-0000-0000-0000-000000000012"),
                "La Cigale Tabarka Thalasso & Golf",
                "Tabarka",
                "Zone Touristique El Morjane, 8110 Tabarka",
                5,
                "Joyau de la côte de corail niché entre les forêts séculaires de chênes-lièges de Kroumirie et les eaux émeraude de la Méditerranée. Parcours de golf 18 trous de championnat, thalasso haut de gamme et vue spectaculaire sur les Aiguilles de Tabarka.",
                "+216 78 671 200",
                new Email("info@lacigaletabarka.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                    "https://images.unsplash.com/photo-1571896349842-33c89424de2d"
                }
            );
            nouveauxHotels.Add((h12, new()
            {
                ("101", TypeChambre.Double, 1, new Money(390.00m, "TND")),
                ("102", TypeChambre.Suite, 1, new Money(720.00m, "TND")),
                ("201", TypeChambre.Familiale, 2, new Money(590.00m, "TND"))
            }));

            // 13. Kairouan
            var h13 = new Hotel(
                Guid.Parse("c9999999-0000-0000-0000-000000000013"),
                "La Kasbah Hôtel Kairouan",
                "Kairouan",
                "Cité El Oukba, 3100 Kairouan Médina",
                5,
                "Hôtel de prestige intégré dans les remparts historiques de la sainte Médina de Kairouan. Architecture hispano-mauresque féerique, patios ornés de zelliges ancestraux, colonnes sculptées et salons parés des plus somptueux tapis kairouanais.",
                "+216 77 237 301",
                new Email("contact@lakasbah-kairouan.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b",
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb"
                }
            );
            nouveauxHotels.Add((h13, new()
            {
                ("101", TypeChambre.Simple, 1, new Money(210.00m, "TND")),
                ("102", TypeChambre.Double, 1, new Money(290.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(480.00m, "TND"))
            }));

            // 14. Bizerte
            var h14 = new Hotel(
                Guid.Parse("caaaaaaa-0000-0000-0000-000000000014"),
                "Andalucia Beach Hotel & Suites",
                "Bizerte",
                "Zone Touristique Sidi Salem, 7000 Bizerte",
                4,
                "Hôtel les pieds dans l'eau au nord de la Tunisie, entre le Vieux Port phénicien et les falaises sauvages du Cap Blanc. Vaste piscine face aux vagues de la Méditerranée, chambres spacieuses et ambiance marine vivifiante.",
                "+216 72 443 444",
                new Email("reservation@andaluciabeach.tn"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                    "https://images.unsplash.com/photo-1578683010236-d716f9a3f461"
                }
            );
            nouveauxHotels.Add((h14, new()
            {
                ("101", TypeChambre.Simple, 1, new Money(180.00m, "TND")),
                ("102", TypeChambre.Double, 1, new Money(260.00m, "TND")),
                ("201", TypeChambre.Suite, 2, new Money(420.00m, "TND"))
            }));

            // --- 6 NOUVEAUX HÔTELS INTERNATIONAUX ---
            // 15. France (Paris) - EUR
            var h15 = new Hotel(
                Guid.Parse("cbbaaaaa-0000-0000-0000-000000000015"),
                "Hôtel de Crillon, Rosewood Hotel",
                "Paris",
                "10 Place de la Concorde, 75008 Paris, France",
                5,
                "Palace parisien mythique du XVIIIe siècle dominant la Place de la Concorde. Salons historiques d'apparat, suites de haute couture signées Karl Lagerfeld et cour d'honneur emblématique de l'art de vivre à la française.",
                "+33 1 44 71 15 00",
                new Email("crillon@rosewoodhotels.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb",
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945"
                }
            );
            nouveauxHotels.Add((h15, new()
            {
                ("101", TypeChambre.Double, 1, new Money(1200.00m, "EUR")),
                ("201", TypeChambre.Suite, 2, new Money(2800.00m, "EUR")),
                ("202", TypeChambre.Suite, 2, new Money(4500.00m, "EUR"))
            }));

            // 16. Maroc (Marrakech) - MAD
            var h16 = new Hotel(
                Guid.Parse("cbbbbbbb-0000-0000-0000-000000000016"),
                "La Mamounia Palace",
                "Marrakech",
                "Avenue Bab Jdid, 40040 Marrakech, Maroc",
                5,
                "Légende vivante de l'hospitalité marocaine nichée au cœur d'un parc séculaire de 8 hectares face aux remparts de Marrakech. Céramiques de Fès, zelliges étincelants, jardins d'orangers parfumés et gastronomie étoilée.",
                "+212 524 388 600",
                new Email("informations@mamounia.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1582719478250-c89cae4dc85b",
                    "https://images.unsplash.com/photo-1570077188670-e3a8d69ac5ff"
                }
            );
            nouveauxHotels.Add((h16, new()
            {
                ("101", TypeChambre.Double, 1, new Money(6500.00m, "MAD")),
                ("102", TypeChambre.Double, 1, new Money(7800.00m, "MAD")),
                ("201", TypeChambre.Suite, 2, new Money(14000.00m, "MAD"))
            }));

            // 17. Égypte (Le Caire) - EGP
            var h17 = new Hotel(
                Guid.Parse("cbbccccc-0000-0000-0000-000000000017"),
                "Marriott Mena House",
                "Le Caire",
                "6 Pyramids Road, Gizeh, Le Caire, Égypte",
                5,
                "Hôtel de légende fondé en 1869 offrant une vue directe et spectaculaire sur les Grandes Pyramides de Gizeh. Jardins royaux luxuriants, meubles orientaux sculptés à la main et atmosphère historique impériale.",
                "+20 2 3377 3222",
                new Email("menahouse@marriott.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1509316975850-ff9c5deb0cd9",
                    "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4"
                }
            );
            nouveauxHotels.Add((h17, new()
            {
                ("101", TypeChambre.Double, 1, new Money(12500.00m, "EGP")),
                ("102", TypeChambre.Double, 1, new Money(16000.00m, "EGP")),
                ("201", TypeChambre.Suite, 2, new Money(28000.00m, "EGP"))
            }));

            // 18. Turquie (Istanbul) - EUR
            var h18 = new Hotel(
                Guid.Parse("cbbddddd-0000-0000-0000-000000000018"),
                "Çırağan Palace Kempinski",
                "Istanbul",
                "Çırağan Caddesi 32, Beşiktaş, 34349 Istanbul, Turquie",
                5,
                "Ancien palais impérial ottoman du XIXe siècle majestueusement posé sur la rive européenne du détroit du Bosphore. Piscine à débordement chauffée face à l'Asie, marbre éclatant et service digne des sultans.",
                "+90 212 326 4646",
                new Email("reservation.ciraganpalace@kempinski.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1566073771259-6a8506099945",
                    "https://images.unsplash.com/photo-1618773928121-c32242e63f39"
                }
            );
            nouveauxHotels.Add((h18, new()
            {
                ("101", TypeChambre.Double, 1, new Money(750.00m, "EUR")),
                ("102", TypeChambre.Double, 1, new Money(950.00m, "EUR")),
                ("201", TypeChambre.Suite, 2, new Money(1900.00m, "EUR"))
            }));

            // 19. Espagne (Barcelone) - EUR
            var h19 = new Hotel(
                Guid.Parse("cbbeeeee-0000-0000-0000-000000000019"),
                "Hotel Arts Barcelona",
                "Barcelone",
                "Carrer de Marina 19-21, 08005 Barcelone, Espagne",
                5,
                "Tour iconique de verre et d'acier surplombant les plages de la Barceloneta et la Méditerranée. Restaurant deux étoiles Michelin, terrasses fleuries en bord de mer et suites panoramiques d'exception.",
                "+34 93 221 10 00",
                new Email("artsreservations@ritzcarlton.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1551882547-ff40c63fe5fa",
                    "https://images.unsplash.com/photo-1533105079780-92b9be482077"
                }
            );
            nouveauxHotels.Add((h19, new()
            {
                ("101", TypeChambre.Double, 1, new Money(520.00m, "EUR")),
                ("102", TypeChambre.Double, 1, new Money(680.00m, "EUR")),
                ("201", TypeChambre.Suite, 2, new Money(1350.00m, "EUR"))
            }));

            // 20. Italie (Venise) - EUR
            var h20 = new Hotel(
                Guid.Parse("cbbfffff-0000-0000-0000-000000000020"),
                "The Gritti Palace, a Luxury Collection Hotel",
                "Venise",
                "Campo Santa Maria Del Giglio 2467, 30124 Venise, Italie",
                5,
                "Palais aristocratique du XVe siècle ancienne résidence du doge Andrea Gritti dominant le Grand Canal de Venise. Boiseries précieuses, lustres en verre de Murano, terrasse historique face à la Basilique Santa Maria della Salute.",
                "+39 041 794611",
                new Email("grittipalace@luxurycollection.com"),
                new List<string>
                {
                    "https://images.unsplash.com/photo-1520250497591-112f2f40a3f4",
                    "https://images.unsplash.com/photo-1542314831-068cd1dbfeeb"
                }
            );
            nouveauxHotels.Add((h20, new()
            {
                ("101", TypeChambre.Double, 1, new Money(1100.00m, "EUR")),
                ("201", TypeChambre.Suite, 2, new Money(2400.00m, "EUR")),
                ("202", TypeChambre.Suite, 2, new Money(3600.00m, "EUR"))
            }));

            foreach (var item in nouveauxHotels)
            {
                if (!await context.Hotels.AnyAsync(h => h.Id == item.Hotel.Id || (h.Nom == item.Hotel.Nom && h.Ville == item.Hotel.Ville)))
                {
                    foreach (var ch in item.Chambres)
                    {
                        item.Hotel.AjouterChambre(ch.Numero, ch.Type, ch.Etage, ch.Prix, actif: true);
                    }
                    await context.Hotels.AddAsync(item.Hotel);
                }
            }

            await context.SaveChangesAsync();
            logger?.LogInformation("[DbSeeder] Bloc 5 (16 Hôtels & Chambres) validé.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 5 (Hôtels) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 6 : RÉSERVATIONS (20 AU TOTAL AVEC LES 2 EXISTANTES)
        // =========================================================================
        try
        {
            // Vérifier le nombre de réservations actuelles en base
            var nbReservationsActuelles = await context.Reservations.CountAsync();
            if (nbReservationsActuelles < 20)
            {
                var allClients = await context.Clients.OrderBy(c => c.Email.Value).ToListAsync();
                var allHotels = await context.Hotels.Include(h => h.Chambres).OrderBy(h => h.Nom).ToListAsync();
                var domainService = new ReservationDomainService();

                // Spécification de 18 nouvelles réservations strictes, sans chevauchement
                // Répartition globale souhaitée (20 total) : 8 Terminées, 6 Confirmées, 3 EnAttente, 3 Annulées
                // Comme 2 existantes sont déjà en base (1 Terminée + 1 Confirmée), on ajoute :
                // 7 Terminées, 5 Confirmées, 3 EnAttente, 3 Annulées (= 18 réservations)
                var reservationsSpecs = new[]
                {
                    // --- 7 TERMINÉES (séjours passés, chambres libérées) ---
                    new { HotelIdx = 4, ChIdx = 0, ClientIdx = 1, JoursArrivee = -90, Duree = 4, Statut = StatutReservation.Terminee, NbPers = 2 },
                    new { HotelIdx = 5, ChIdx = 0, ClientIdx = 2, JoursArrivee = -75, Duree = 3, Statut = StatutReservation.Terminee, NbPers = 2 },
                    new { HotelIdx = 6, ChIdx = 0, ClientIdx = 3, JoursArrivee = -60, Duree = 5, Statut = StatutReservation.Terminee, NbPers = 1 },
                    new { HotelIdx = 7, ChIdx = 0, ClientIdx = 4, JoursArrivee = -50, Duree = 4, Statut = StatutReservation.Terminee, NbPers = 3 },
                    new { HotelIdx = 8, ChIdx = 0, ClientIdx = 5, JoursArrivee = -40, Duree = 3, Statut = StatutReservation.Terminee, NbPers = 2 },
                    new { HotelIdx = 9, ChIdx = 0, ClientIdx = 6, JoursArrivee = -30, Duree = 2, Statut = StatutReservation.Terminee, NbPers = 2 },
                    new { HotelIdx = 10, ChIdx = 0, ClientIdx = 7, JoursArrivee = -20, Duree = 3, Statut = StatutReservation.Terminee, NbPers = 2 },

                    // --- 5 CONFIRMÉES (séjours actuels ou futurs) ---
                    new { HotelIdx = 11, ChIdx = 0, ClientIdx = 8, JoursArrivee = -1, Duree = 3, Statut = StatutReservation.Confirmee, NbPers = 2 },
                    new { HotelIdx = 12, ChIdx = 0, ClientIdx = 9, JoursArrivee = 5, Duree = 4, Statut = StatutReservation.Confirmee, NbPers = 2 },
                    new { HotelIdx = 13, ChIdx = 0, ClientIdx = 10, JoursArrivee = 12, Duree = 3, Statut = StatutReservation.Confirmee, NbPers = 2 },
                    new { HotelIdx = 14, ChIdx = 0, ClientIdx = 11, JoursArrivee = 20, Duree = 2, Statut = StatutReservation.Confirmee, NbPers = 2 },
                    new { HotelIdx = 15, ChIdx = 0, ClientIdx = 12, JoursArrivee = 30, Duree = 4, Statut = StatutReservation.Confirmee, NbPers = 2 },

                    // --- 3 EN ATTENTE (séjours à venir en attente de paiement) ---
                    new { HotelIdx = 16, ChIdx = 0, ClientIdx = 13, JoursArrivee = 15, Duree = 3, Statut = StatutReservation.EnAttente, NbPers = 2 },
                    new { HotelIdx = 17, ChIdx = 0, ClientIdx = 14, JoursArrivee = 25, Duree = 2, Statut = StatutReservation.EnAttente, NbPers = 2 },
                    new { HotelIdx = 18, ChIdx = 0, ClientIdx = 15, JoursArrivee = 35, Duree = 3, Statut = StatutReservation.EnAttente, NbPers = 1 },

                    // --- 3 ANNULÉES ---
                    new { HotelIdx = 19, ChIdx = 0, ClientIdx = 16, JoursArrivee = -15, Duree = 3, Statut = StatutReservation.Annulee, NbPers = 2 },
                    new { HotelIdx = 4, ChIdx = 1, ClientIdx = 17, JoursArrivee = 10, Duree = 2, Statut = StatutReservation.Annulee, NbPers = 2 },
                    new { HotelIdx = 5, ChIdx = 1, ClientIdx = 18, JoursArrivee = 18, Duree = 4, Statut = StatutReservation.Annulee, NbPers = 3 }
                };

                int newResIdx = 0;
                foreach (var spec in reservationsSpecs)
                {
                    newResIdx++;
                    var resId = Guid.Parse($"e0000000-0000-0000-0000-{(newResIdx):D12}");

                    if (await context.Reservations.AnyAsync(r => r.Id == resId))
                        continue;

                    var hotel = allHotels[spec.HotelIdx % allHotels.Count];
                    if (!hotel.Chambres.Any()) continue;

                    var chambre = hotel.Chambres.ElementAt(spec.ChIdx % hotel.Chambres.Count);
                    var client = allClients[spec.ClientIdx % allClients.Count];

                    var dateArrivee = DateTime.UtcNow.Date.AddDays(spec.JoursArrivee);
                    var dateDepart = dateArrivee.AddDays(spec.Duree);

                    var reservation = domainService.CreerReservation(
                        chambre.Id,
                        client.Id,
                        hotel.Id,
                        new DateRange(dateArrivee, dateDepart),
                        spec.NbPers,
                        Enumerable.Empty<Reservation>(),
                        chambre.Prix
                    );

                    reservation.DefinirDateCreation(dateArrivee.AddDays(-10));

                    if (spec.Statut == StatutReservation.Confirmee)
                    {
                        reservation.Confirmer();
                        if (dateArrivee <= DateTime.UtcNow.Date && dateDepart >= DateTime.UtcNow.Date)
                        {
                            hotel.ChangerStatutChambre(chambre.Id, StatutChambre.Occupee);
                        }
                    }
                    else if (spec.Statut == StatutReservation.Terminee)
                    {
                        reservation.Confirmer();
                        reservation.ChangerStatut(StatutReservation.Terminee);
                    }
                    else if (spec.Statut == StatutReservation.Annulee)
                    {
                        reservation.Annuler();
                    }

                    await context.Reservations.AddAsync(reservation);
                    await context.SaveChangesAsync();

                    // =========================================================================
                    // BLOC 7 : PAIEMENTS ASSOCIÉS
                    // =========================================================================
                    if (spec.Statut == StatutReservation.Terminee || spec.Statut == StatutReservation.Confirmee)
                    {
                        var pId = Guid.Parse($"d0000000-0000-0000-0000-{(newResIdx):D12}");
                        if (!await context.Paiements.AnyAsync(p => p.Id == pId || p.ReservationId == reservation.Id))
                        {
                            var modes = new[] { ModePaiement.Carte, ModePaiement.Virement, ModePaiement.Carte, ModePaiement.Especes };
                            var mode = modes[newResIdx % modes.Length];
                            var paiement = new Paiement(
                                pId,
                                reservation.Id,
                                reservation.Total,
                                mode,
                                $"PAY-HOTELI-{newResIdx:D4}-{reservation.Id.ToString()[..6].ToUpper()}",
                                Guid.Parse("22222222-2222-2222-2222-222222222222")
                            );
                            paiement.DefinirDate(dateArrivee.AddDays(-5));
                            await context.Paiements.AddAsync(paiement);
                        }
                    }
                    else if (spec.Statut == StatutReservation.Annulee && newResIdx == 16)
                    {
                        // 1 réservation annulée avec paiement remboursé/annulé
                        var pId = Guid.Parse($"d0000000-0000-0000-0000-{(newResIdx):D12}");
                        if (!await context.Paiements.AnyAsync(p => p.Id == pId || p.ReservationId == reservation.Id))
                        {
                            var paiement = new Paiement(
                                pId,
                                reservation.Id,
                                reservation.Total,
                                ModePaiement.Carte,
                                $"PAY-ANNULE-{newResIdx:D4}",
                                Guid.Parse("22222222-2222-2222-2222-222222222222")
                            );
                            paiement.Annuler();
                            await context.Paiements.AddAsync(paiement);
                        }
                    }

                    // =========================================================================
                    // BLOC 8 : AVIS ASSOCIÉS (Pour réservations terminées)
                    // =========================================================================
                    if (spec.Statut == StatutReservation.Terminee)
                    {
                        var aId = Guid.Parse($"f0000000-0000-0000-0000-{(newResIdx):D12}");
                        if (!await context.Avis.AnyAsync(a => a.Id == aId || a.ReservationId == reservation.Id))
                        {
                            var notes = new[] { 5, 4, 5, 5, 4, 5, 5 };
                            var commentaires = new[]
                            {
                                "Séjour absolument enchanteur. Le personnel est d'une prévenance exemplaire et le cadre d'un raffinement rare.",
                                "Excellente adresse, chambres spacieuses, propreté irréprochable et vue imprenable.",
                                "Une expérience hôtelière inoubliable, la haute gastronomie et le spa marin complètent à merveille le confort.",
                                "Prestations d'exception, calme absolu et architecture remarquable.",
                                "Très beau séjour, literie haut de gamme et service impeccable.",
                                "Un véritable havre de paix. Nous reviendrons sans la moindre hésitation.",
                                "Merveilleux souvenirs, hospitalité chaleureuse et atmosphère prestigieuse."
                            };

                            var avis = new Avis(
                                aId,
                                client.Id,
                                hotel.Id,
                                reservation.Id,
                                new NoteAvis(notes[newResIdx % notes.Length]),
                                commentaires[newResIdx % commentaires.Length]
                            );
                            avis.Valider();
                            await context.Avis.AddAsync(avis);
                        }
                    }

                    await context.SaveChangesAsync();
                }
            }

            logger?.LogInformation("[DbSeeder] Bloc 6, 7 & 8 (Réservations, Paiements, Avis) validés.");
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 6/7/8 (Réservations) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        // =========================================================================
        // BLOC 9 : FAVORIS CLIENTS
        // =========================================================================
        try
        {
            var clientType = await context.Clients.FirstOrDefaultAsync(c => c.Email.Value == "client@hotelapp.com");
            if (clientType != null)
            {
                var hotelsList = await context.Hotels.Take(5).ToListAsync();
                foreach (var h in hotelsList)
                {
                    if (!await context.Favoris.AnyAsync(f => f.UtilisateurId == clientType.Id && f.HotelId == h.Id))
                    {
                        await context.Favoris.AddAsync(new Favori(Guid.NewGuid(), clientType.Id, h.Id));
                    }
                }
                await context.SaveChangesAsync();
                logger?.LogInformation("[DbSeeder] Bloc 9 (Favoris) validé.");
            }
        }
        catch (Exception ex)
        {
            logger?.LogError(ex, "[DbSeeder] Erreur dans le Bloc 9 (Favoris) : {Message}. Inner: {Inner}", ex.Message, ex.InnerException?.Message);
            throw;
        }

        logger?.LogInformation("[DbSeeder] INITIALISATION COMPLÈTE DU CATALOGUE HOTELI TERMINÉE AVEC SUCCÈS !");
    }
}
