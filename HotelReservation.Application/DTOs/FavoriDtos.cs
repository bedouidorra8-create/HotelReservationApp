namespace HotelReservation.Application.DTOs;

/// <summary>Favori enrichi des informations utiles à l'affichage de l'hôtel.</summary>
public record FavoriDto(Guid Id, Guid HotelId, string HotelNom, string HotelVille, DateTime DateAjout);

/// <summary>Informations nécessaires pour ajouter un hôtel aux favoris.</summary>
public record AjouterFavoriRequest(Guid HotelId);