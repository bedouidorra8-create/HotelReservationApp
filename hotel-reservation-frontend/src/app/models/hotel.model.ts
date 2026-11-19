export interface Hotel {
  id: string;
  nom: string;
  ville: string;
  adresse: string;
  nbEtoiles: number;
  description: string;
  telephone: string;
  email: string;
  images: string[];
  actif: boolean;
  nbChambresDisponibles: number;
  chambres: Chambre[];
}

export interface Chambre {
  id: string;
  hotelId: string;
  numero: string;
  type: number; // 1: Simple, 2: Double, 3: Suite, 4: Familiale
  etage: number;
  prixMontant: number;
  prixDevise: string;
  statut: number; // 1: Libre, 2: Occupee, 3: Maintenance
}

export interface CreerHotelRequest {
  nom: string;
  ville: string;
  adresse: string;
  nbEtoiles: number;
  description: string;
  telephone: string;
  email: string;
  images?: string[];
}

export interface CreerChambreRequest {
  hotelId: string;
  numero: string;
  type: number;
  etage: number;
  prixMontant: number;
  prixDevise?: string;
}

export interface DisponibiliteChambre {
  chambreId: string;
  numero: string;
  type: number;
  etage: number;
  prixMontant: number;
  prixDevise: string;
  statutParJour: { [key: string]: string }; // Key: YYYY-MM-DD, Value: "Libre" | "Occupee" | "Maintenance"
}

export interface DisponibilitesHotelResponse {
  hotelId: string;
  hotelNom: string;
  dateDebut: string;
  dateFin: string;
  chambres: DisponibiliteChambre[];
}
