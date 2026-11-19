using HotelReservation.Application.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HotelReservation.Infrastructure.Services;

/// <summary>
/// Implementation minimale du service email.
/// En production, remplacer par SendGrid, Mailjet, SMTP reel, etc.
/// </summary>
public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task EnvoyerVerificationEmailAsync(string destinataire, string nom, string token, CancellationToken ct = default)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:4200";
        var lien = $"{baseUrl}/auth/verifier-email?token={token}";

        // En developpement : afficher dans les logs
        _logger.LogInformation(
            "[EMAIL - Verification] A: {Destinataire} | Lien: {Lien}",
            destinataire, lien);

        // TODO: Remplacer par vrai envoi SMTP/SendGrid en production
        await Task.CompletedTask;
    }

    public async Task EnvoyerResetMotDePasseAsync(string destinataire, string nom, string token, CancellationToken ct = default)
    {
        var baseUrl = _configuration["App:BaseUrl"] ?? "http://localhost:4200";
        var lien = $"{baseUrl}/auth/reinitialiser-mot-de-passe?token={token}";

        _logger.LogInformation(
            "[EMAIL - Reset MDP] A: {Destinataire} | Lien: {Lien}",
            destinataire, lien);

        await Task.CompletedTask;
    }
}
