export interface Reservation {
  id: string;
  chambreId: string;
  clientId: string;
  dateArrivee: string;
  dateDepart: string;
  nbNuits: number;
  nbPersonnes: number;
  statut: number; // 1: EnAttente, 2: Confirmee, 3: Annulee, 4: Terminee
  dateCreation: string;
}

export interface CreerReservationRequest {
  chambreId: string;
  clientId: string;
  dateArrivee: string;
  dateDepart: string;
  nbPersonnes: number;
}
