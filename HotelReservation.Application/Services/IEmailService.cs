namespace HotelReservation.Application.Services;

public interface IEmailService
{
    Task EnvoyerVerificationEmailAsync(string destinataire, string nom, string token, CancellationToken ct = default);
    Task EnvoyerResetMotDePasseAsync(string destinataire, string nom, string token, CancellationToken ct = default);
}
