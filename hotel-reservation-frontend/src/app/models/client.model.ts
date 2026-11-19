export interface Client {
  id: string;
  nom: string;
  prenom: string;
  email: string;
  telephone: string;
  dateNaissance: string;
  sexe: string;
  adresse: string;
  photo: string;
  statut: number;
  dateCreation: string;
}

export interface CreerClientRequest {
  nom: string;
  prenom: string;
  email: string;
  motDePasse: string;
  telephone: string;
  dateNaissance: string;
  sexe: string;
  adresse: string;
  photo?: string;
}
