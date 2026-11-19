export interface Avis {
  id: string;
  clientId: string;
  hotelId: string;
  reservationId: string;
  note: number;
  commentaire: string;
  date: string;
  statut: number;
}

export interface AjouterAvisRequest {
  clientId: string;
  hotelId: string;
  reservationId: string;
  note: number;
  commentaire: string;
}
