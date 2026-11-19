// Enums du Domaine (alignés strictement avec le backend ASP.NET Core)
export enum Genre {
  Homme = 1,
  Femme = 2
}

export enum ModePaiement {
  Carte = 1,
  Especes = 2,
  Virement = 3,
  Cheque = 4
}

export enum StatutAvis {
  EnAttente = 1,
  Valide = 2,
  Rejete = 3
}

export enum StatutChambre {
  Libre = 1,
  Occupee = 2,
  Maintenance = 3
}

export enum StatutHotel {
  Actif = 1,
  Inactif = 2
}

export enum StatutPaiement {
  EnAttente = 1,
  Paye = 2,
  Rembourse = 3,
  Echoue = 4,
  Annule = 5
}

export enum StatutReservation {
  EnAttente = 1,
  Confirmee = 2,
  Annulee = 3,
  Terminee = 4
}

export enum StatutUtilisateur {
  Actif = 1,
  Desactive = 2
}

export enum TypeChambre {
  Simple = 1,
  Double = 2,
  Suite = 3,
  Familiale = 4
}

// Authentification & Utilisateur
export interface ConnexionRequest {
  email: string;
  motDePasse: string;
}

export interface ConnexionResponse {
  jwt: string;
  refreshToken: string;
  jwtExpiration: string;
  utilisateurId: string;
  nomComplet: string;
  email: string;
  role: 'Client' | 'Agent' | 'Administrateur';
}

export interface InscrireUtilisateurRequest {
  nom: string;
  prenom: string;
  email: string;
  motDePasse: string;
  telephone: string;
  dateNaissance: string;
  sexe?: Genre | null;
  adresse: string;
  role: 'Client' | 'Agent' | 'Administrateur';
}

export interface TokenResponse {
  jwt: string;
  refreshToken: string;
  jwtExpiration: string;
}

export interface ProfilUtilisateurDto {
  id: string;
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
  role: string;
  dateNaissance: string;
  sexe?: Genre | null;
  adresse: string;
  photo?: string;
}

export interface ModifierProfilRequest {
  nom: string;
  prenom: string;
  telephone: string;
  adresse: string;
  photo?: string;
  dateNaissance?: string | null;
  sexe?: Genre | null;
}

export interface ChangerMotDePasseRequest {
  ancienMotDePasse: string;
  nouveauMotDePasse: string;
}

export interface DemanderResetPasswordRequest {
  email: string;
}

export interface ReinitialiserMotDePasseRequest {
  token: string;
  nouveauMotDePasse: string;
}

// Hôtels & Chambres
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
  chambres?: Chambre[];
  prixMin?: number;
}

export interface HotelResultatRechercheDto {
  id: string;
  nom: string;
  ville: string;
  adresse: string;
  nbEtoiles: number;
  description: string;
  images: string[];
  prixLePlusBas: number;
  chambresDisponibles: number;
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

export interface ModifierHotelRequest {
  nom: string;
  ville: string;
  adresse: string;
  nbEtoiles: number;
  description: string;
  telephone: string;
  email: string;
  images?: string[];
}

export interface Chambre {
  id: string;
  hotelId: string;
  numero: string;
  type: TypeChambre;
  etage: number;
  prixMontant: number;
  prixDevise: string;
  statut: StatutChambre;
  actif?: boolean;
}

export interface CreerChambreRequest {
  hotelId?: string;
  numero: string;
  type: TypeChambre;
  etage: number;
  prixMontant: number;
  prixDevise?: string;
}

export interface ModifierChambreRequest {
  type: TypeChambre;
  etage: number;
  prixMontant: number;
  prixDevise?: string;
}

export interface DisponibiliteChambre {
  chambreId: string;
  numero: string;
  type: TypeChambre;
  etage: number;
  prixMontant: number;
  prixDevise: string;
  statutParJour: { [dateIso: string]: string };
}

export interface DisponibilitesHotelResponse {
  hotelId: string;
  hotelNom: string;
  dateDebut: string;
  dateFin: string;
  chambres: DisponibiliteChambre[];
}

// Réservations
export interface Reservation {
  id: string;
  chambreId: string;
  clientId: string;
  dateArrivee: string;
  dateDepart: string;
  nbNuits: number;
  nbPersonnes: number;
  statut: StatutReservation;
  dateCreation: string;
  prixUnitaireMontant?: number;
  prixUnitaireDevise?: string;
  totalMontant?: number;
  totalDevise?: string;
  hotelNom?: string;
  chambreNumero?: string;
}

export interface CreerReservationRequest {
  chambreId: string;
  clientId?: string;
  dateArrivee: string;
  dateDepart: string;
  nbPersonnes: number;
}

export interface ModifierReservationRequest {
  dateArrivee: string;
  dateDepart: string;
  nbPersonnes: number;
}

export interface ReservationCalendrierDto {
  reservationId: string;
  hotelId: string;
  hotelNom: string;
  chambreId: string;
  chambreNumero: string;
  clientId: string;
  dateDebut: string;
  dateFin: string;
  statut: StatutReservation;
  nbPersonnes: number;
}

export interface CalendrierReservationsResponse {
  hotelId: string;
  hotelNom: string;
  dateDebut: string;
  dateFin: string;
  reservations: ReservationCalendrierDto[];
}

// Paiements & Factures
export interface Paiement {
  id: string;
  reservationId: string;
  montant: number;
  devise: string;
  date: string;
  modePaiement: ModePaiement;
  reference: string;
  statut: StatutPaiement;
  enregistrePar?: string;
  hotelNom?: string;
}

export interface EnregistrerPaiementRequest {
  reservationId: string;
  montant: number;
  devise: string;
  modePaiement: ModePaiement;
  reference: string;
}

export interface FactureDto {
  reservationId: string;
  dateEmission: string;
  total: number;
  devise: string;
  paiements: Paiement[];
  client?: { id: string; nom: string; prenom: string; email: string };
  hotel?: { id: string; nom: string; adresse: string };
  chambre?: { id: string; numero: string };
}

// Avis & Favoris
export interface Avis {
  id: string;
  clientId: string;
  hotelId: string;
  reservationId: string;
  note: number;
  commentaire: string;
  date: string;
  statut: StatutAvis;
  clientNom?: string;
  hotelNom?: string;
}

export interface AjouterAvisRequest {
  clientId?: string;
  hotelId: string;
  reservationId: string;
  note: number;
  commentaire: string;
}

export interface ModifierAvisRequest {
  note: number;
  commentaire: string;
}

export interface Favori {
  id: string;
  clientId: string;
  hotelId: string;
  dateAjout: string;
  hotelNom?: string;
  hotelVille?: string;
  hotelEtoiles?: number;
  hotelImage?: string;
  prixMin?: number;
}

export interface AjouterFavoriRequest {
  hotelId: string;
}

// Administration, Utilisateurs & Rôles
export interface UtilisateurDto {
  id: string;
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
  role: string;
  statut: StatutUtilisateur;
  dateCreation: string;
  adresse?: string;
  dateNaissance?: string;
  sexe?: Genre | null;
}

export interface AjouterUtilisateurRequest {
  nom: string;
  prenom: string;
  email: string;
  motDePasse: string;
  telephone: string;
  adresse: string;
  dateNaissance: string;
  sexe?: Genre | null;
  role: 'Agent' | 'Administrateur';
}

export interface RoleDto {
  id: string;
  nom: string;
  permissions: string[];
}

// Dashboard & Statistiques
export interface DashboardDto {
  nombreTotalUtilisateurs?: number | null;
  nombreHotels?: number | null;
  nombreReservations?: number | null;
  reservationsDuJour: number;
  chiffreAffaires?: number | null;
  paiementsEnAttente: number;
  dernieresReservations: Reservation[];
  graphiqueReservationsParMois?: { [key: string]: number } | null;
  graphiqueRevenusParMois?: { [key: string]: number } | null;
}

export interface ReservationsParMoisDto {
  annee: number;
  mois: number;
  nombreReservations: number;
}

export interface ChiffreAffairesMensuelDto {
  annee: number;
  mois: number;
  montant: number;
}

export interface HotelPlusReserveDto {
  hotelId: string;
  hotelNom: string;
  nombreReservations: number;
}

export interface PaiementsParModeDto {
  mode: ModePaiement;
  modeNom: string;
  nombrePaiements: number;
  montantTotal: number;
}

export interface NouveauxUtilisateursParMoisDto {
  annee: number;
  mois: number;
  nombreUtilisateurs: number;
}

export interface TauxOccupationHotelDto {
  hotelId: string;
  hotelNom: string;
  totalChambres: number;
  chambresOccupees: number;
  tauxOccupationPourcentage: number;
}

export interface EvolutionAvisDto {
  annee: number;
  mois: number;
  nombreAvis: number;
  noteMoyenne: number;
}

export interface StatistiquesDto {
  reservationsParMois: ReservationsParMoisDto[];
  chiffreAffairesMensuel: ChiffreAffairesMensuelDto[];
  hotelsLesPlusReserves: HotelPlusReserveDto[];
  repartitionPaiementsParMode: PaiementsParModeDto[];
  nouveauxUtilisateursParMois: NouveauxUtilisateursParMoisDto[];
  tauxOccupationHotels: TauxOccupationHotelDto[];
  evolutionAvis: EvolutionAvisDto[];
  dateDebut?: string | null;
  dateFin?: string | null;
}

// Journal d'audit
export interface AuditLogDto {
  id: string;
  utilisateurId?: string;
  action: string;
  entite: string;
  entiteId?: string;
  dateAction: string;
  details?: string;
  adresseIp?: string;
}
