namespace HotelReservation.Application.Services;

public interface ITokenService
{
    string GenererJwt(Guid utilisateurId, string email, string role);
    string GenererRefreshToken();
    string GenererTokenSecurise();
}
