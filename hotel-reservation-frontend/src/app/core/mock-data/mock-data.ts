/**
 * DONNÉES DE DÉMONSTRATION (MOCK DATA)
 * -------------------------------------------------------------
 * ATTENTION : Ces données sont exclusivement réservées à la prévisualisation
 * visuelle et aux fallbacks d'ergonomie lorsque les endpoints backend
 * correspondants sont indisponibles ou en attente d'implémentation.
 * Elles ne doivent JAMAIS être confondues avec des données réelles de production.
 */

import {
  Hotel,
  TypeChambre,
  StatutChambre,
  StatutReservation,
  StatutPaiement,
  ModePaiement,
  StatutAvis,
  StatutUtilisateur,
  DashboardDto,
  StatistiquesDto,
  Avis,
  AuditLogDto,
  Paiement
} from '../models';

export const DEMO_HOTELS: Hotel[] = [
  {
    id: '11111111-1111-1111-1111-111111111111',
    nom: 'Le Grand Palais Royal',
    ville: 'Paris',
    adresse: '15 Avenue des Champs-Élysées, 75008 Paris',
    nbEtoiles: 5,
    description: 'Hôtel de prestige offrant un raffinement inégalé au cœur de la capitale. Spa luxueux, restaurant gastronomique étoilé et suites panoramiques sur la Tour Eiffel.',
    telephone: '+33 1 42 68 00 00',
    email: 'contact@grandpalaisroyal.com',
    images: [
      'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=1200&q=80',
      'https://images.unsplash.com/photo-1582719478250-c89cae4dc85b?auto=format&fit=crop&w=800&q=80'
    ],
    actif: true,
    nbChambresDisponibles: 12,
    prixMin: 320,
    chambres: [
      {
        id: 'c1111111-1111-1111-1111-111111111111',
        hotelId: '11111111-1111-1111-1111-111111111111',
        numero: '101',
        type: TypeChambre.Simple,
        etage: 1,
        prixMontant: 320,
        prixDevise: 'EUR',
        statut: StatutChambre.Libre,
        actif: true
      },
      {
        id: 'c2222222-2222-2222-2222-222222222222',
        hotelId: '11111111-1111-1111-1111-111111111111',
        numero: '202',
        type: TypeChambre.Double,
        etage: 2,
        prixMontant: 480,
        prixDevise: 'EUR',
        statut: StatutChambre.Libre,
        actif: true
      },
      {
        id: 'c3333333-3333-3333-3333-333333333333',
        hotelId: '11111111-1111-1111-1111-111111111111',
        numero: '501',
        type: TypeChambre.Suite,
        etage: 5,
        prixMontant: 850,
        prixDevise: 'EUR',
        statut: StatutChambre.Occupee,
        actif: true
      }
    ]
  },
  {
    id: '22222222-2222-2222-2222-222222222222',
    nom: 'Hôtel Bellevue Riviera',
    ville: 'Nice',
    adresse: '42 Promenade des Anglais, 06000 Nice',
    nbEtoiles: 4,
    description: 'Une vue imprenable sur la baie des Anges, terrasse en front de mer et chambres élégantes au style méditerranéen.',
    telephone: '+33 4 93 00 11 22',
    email: 'reservation@bellevueriviera.fr',
    images: [
      'https://images.unsplash.com/photo-1520250497591-112f2f40a3f4?auto=format&fit=crop&w=1200&q=80',
      'https://images.unsplash.com/photo-1571003123894-1f0594d2b5d9?auto=format&fit=crop&w=800&q=80'
    ],
    actif: true,
    nbChambresDisponibles: 8,
    prixMin: 195,
    chambres: [
      {
        id: 'c4444444-4444-4444-4444-444444444444',
        hotelId: '22222222-2222-2222-2222-222222222222',
        numero: '304',
        type: TypeChambre.Double,
        etage: 3,
        prixMontant: 220,
        prixDevise: 'EUR',
        statut: StatutChambre.Libre,
        actif: true
      },
      {
        id: 'c5555555-5555-5555-5555-555555555555',
        hotelId: '22222222-2222-2222-2222-222222222222',
        numero: '408',
        type: TypeChambre.Familiale,
        etage: 4,
        prixMontant: 360,
        prixDevise: 'EUR',
        statut: StatutChambre.Libre,
        actif: true
      }
    ]
  },
  {
    id: '33333333-3333-3333-3333-333333333333',
    nom: 'Château des Cimes',
    ville: 'Chamonix',
    adresse: '8 Route des Alpages, 74400 Chamonix',
    nbEtoiles: 5,
    description: 'Chalet d’exception alliant authenticité alpine et grand confort contemporain avec accès direct aux pistes et spa scandinave.',
    telephone: '+33 4 50 53 10 20',
    email: 'info@chateaudescimes.com',
    images: [
      'https://images.unsplash.com/photo-1542314831-068cd1dbfeeb?auto=format&fit=crop&w=1200&q=80'
    ],
    actif: true,
    nbChambresDisponibles: 5,
    prixMin: 280,
    chambres: [
      {
        id: 'c6666666-6666-6666-6666-666666666666',
        hotelId: '33333333-3333-3333-3333-333333333333',
        numero: '12',
        type: TypeChambre.Suite,
        etage: 2,
        prixMontant: 450,
        prixDevise: 'EUR',
        statut: StatutChambre.Libre,
        actif: true
      }
    ]
  }
];

export const DEMO_DASHBOARD: DashboardDto = {
  nombreTotalUtilisateurs: 32,
  nombreHotels: 14,
  nombreReservations: 68,
  reservationsDuJour: 4,
  chiffreAffaires: 54890.00,
  paiementsEnAttente: 3,
  dernieresReservations: [
    {
      id: 'r111-111',
      chambreId: 'c111',
      clientId: 'u111',
      dateArrivee: '2026-11-15T14:00:00Z',
      dateDepart: '2026-11-19T11:00:00Z',
      nbNuits: 4,
      nbPersonnes: 2,
      statut: StatutReservation.Confirmee,
      dateCreation: '2026-11-12T09:30:00Z',
      totalMontant: 1680,
      totalDevise: 'TND',
      hotelNom: 'Dar Saïd — Demeure & Belvédère (Sidi Bou Saïd)',
      chambreNumero: '102'
    },
    {
      id: 'r222-222',
      chambreId: 'c222',
      clientId: 'u222',
      dateArrivee: '2026-11-20T15:00:00Z',
      dateDepart: '2026-11-24T11:00:00Z',
      nbNuits: 4,
      nbPersonnes: 2,
      statut: StatutReservation.Confirmee,
      dateCreation: '2026-11-14T14:20:00Z',
      totalMontant: 1840,
      totalDevise: 'TND',
      hotelNom: 'Anantara Sahara Tozeur Resort & Oasis',
      chambreNumero: '101'
    },
    {
      id: 'r333-333',
      chambreId: 'c333',
      clientId: 'u333',
      dateArrivee: '2026-11-25T14:00:00Z',
      dateDepart: '2026-11-28T11:00:00Z',
      nbNuits: 3,
      nbPersonnes: 2,
      statut: StatutReservation.EnAttente,
      dateCreation: '2026-11-16T11:10:00Z',
      totalMontant: 1260,
      totalDevise: 'TND',
      hotelNom: 'La Badira — Demeure Méditerranéenne (Hammamet)',
      chambreNumero: '101'
    }
  ],
  graphiqueReservationsParMois: {
    'Déc': 4,
    'Janv': 5,
    'Févr': 5,
    'Mars': 6,
    'Avr': 7,
    'Mai': 8,
    'Juin': 10,
    'Juil': 14,
    'Août': 18,
    'Sept': 12,
    'Oct': 11,
    'Nov': 15
  },
  graphiqueRevenusParMois: {
    'Déc': 3200,
    'Janv': 4100,
    'Févr': 4450,
    'Mars': 5800,
    'Avr': 6900,
    'Mai': 8200,
    'Juin': 11400,
    'Juil': 17800,
    'Août': 24500,
    'Sept': 13600,
    'Oct': 9800,
    'Nov': 14950
  }
};

export const DEMO_STATISTIQUES: StatistiquesDto = {
  reservationsParMois: [
    { annee: 2025, mois: 12, nombreReservations: 4 },
    { annee: 2026, mois: 1, nombreReservations: 5 },
    { annee: 2026, mois: 2, nombreReservations: 5 },
    { annee: 2026, mois: 3, nombreReservations: 6 },
    { annee: 2026, mois: 4, nombreReservations: 7 },
    { annee: 2026, mois: 5, nombreReservations: 8 },
    { annee: 2026, mois: 6, nombreReservations: 10 },
    { annee: 2026, mois: 7, nombreReservations: 14 },
    { annee: 2026, mois: 8, nombreReservations: 18 },
    { annee: 2026, mois: 9, nombreReservations: 12 },
    { annee: 2026, mois: 10, nombreReservations: 11 },
    { annee: 2026, mois: 11, nombreReservations: 15 }
  ],
  chiffreAffairesMensuel: [
    { annee: 2025, mois: 12, montant: 3200 },
    { annee: 2026, mois: 1, montant: 4100 },
    { annee: 2026, mois: 2, montant: 4450 },
    { annee: 2026, mois: 3, montant: 5800 },
    { annee: 2026, mois: 4, montant: 6900 },
    { annee: 2026, mois: 5, montant: 8200 },
    { annee: 2026, mois: 6, montant: 11400 },
    { annee: 2026, mois: 7, montant: 17800 },
    { annee: 2026, mois: 8, montant: 24500 },
    { annee: 2026, mois: 9, montant: 13600 },
    { annee: 2026, mois: 10, montant: 9800 },
    { annee: 2026, mois: 11, montant: 14950 }
  ],
  hotelsLesPlusReserves: [
    { hotelId: 'a1111111-0000-0000-0000-000000000001', hotelNom: 'Dar Saïd (Sidi Bou Saïd)', nombreReservations: 18 },
    { hotelId: 'a2222222-0000-0000-0000-000000000002', hotelNom: 'Anantara Sahara Tozeur Resort', nombreReservations: 15 },
    { hotelId: 'a6666666-0000-0000-0000-000000000006', hotelNom: 'La Badira (Hammamet)', nombreReservations: 14 },
    { hotelId: 'a3333333-0000-0000-0000-000000000003', hotelNom: 'Dar Dhiafa (Djerba)', nombreReservations: 11 },
    { hotelId: 'baaaaaaa-0000-0000-0000-000000000010', hotelNom: 'Mövenpick Resort Sousse', nombreReservations: 9 }
  ],
  repartitionPaiementsParMode: [
    { mode: ModePaiement.Carte, modeNom: 'Carte Bancaire GPG', nombrePaiements: 48, montantTotal: 42100 },
    { mode: ModePaiement.Virement, modeNom: 'Virement BIAT', nombrePaiements: 14, montantTotal: 12450 },
    { mode: ModePaiement.Especes, modeNom: 'Espèces au comptoir', nombrePaiements: 8, montantTotal: 4340 }
  ],
  nouveauxUtilisateursParMois: [
    { annee: 2026, mois: 6, nombreUtilisateurs: 4 },
    { annee: 2026, mois: 7, nombreUtilisateurs: 6 },
    { annee: 2026, mois: 8, nombreUtilisateurs: 8 },
    { annee: 2026, mois: 9, nombreUtilisateurs: 5 },
    { annee: 2026, mois: 10, nombreUtilisateurs: 4 },
    { annee: 2026, mois: 11, nombreUtilisateurs: 5 }
  ],
  tauxOccupationHotels: [
    { hotelId: 'a1111111-0000-0000-0000-000000000001', hotelNom: 'Dar Saïd (Sidi Bou Saïd)', totalChambres: 4, chambresOccupees: 3, tauxOccupationPourcentage: 75.0 },
    { hotelId: 'a2222222-0000-0000-0000-000000000002', hotelNom: 'Anantara Sahara Tozeur', totalChambres: 4, chambresOccupees: 3, tauxOccupationPourcentage: 75.0 },
    { hotelId: 'a6666666-0000-0000-0000-000000000006', hotelNom: 'La Badira (Hammamet)', totalChambres: 3, chambresOccupees: 2, tauxOccupationPourcentage: 66.7 },
    { hotelId: 'baaaaaaa-0000-0000-0000-000000000010', hotelNom: 'Mövenpick Resort Sousse', totalChambres: 4, chambresOccupees: 2, tauxOccupationPourcentage: 50.0 }
  ],
  evolutionAvis: [
    { annee: 2026, mois: 6, nombreAvis: 4, noteMoyenne: 4.8 },
    { annee: 2026, mois: 7, nombreAvis: 6, noteMoyenne: 4.9 },
    { annee: 2026, mois: 8, nombreAvis: 8, noteMoyenne: 4.7 },
    { annee: 2026, mois: 9, nombreAvis: 5, noteMoyenne: 4.8 },
    { annee: 2026, mois: 10, nombreAvis: 4, noteMoyenne: 4.6 },
    { annee: 2026, mois: 11, nombreAvis: 6, noteMoyenne: 4.9 }
  ]
};

export const DEMO_AVIS: Avis[] = [
  {
    id: 'av1',
    clientId: 'u111',
    hotelId: '11111111-1111-1111-1111-111111111111',
    reservationId: 'r111',
    note: 5,
    commentaire: 'Séjour exceptionnel ! Le service était irréprochable et le spa offre un moment de détente absolue.',
    date: '2026-10-28T10:15:00Z',
    statut: StatutAvis.Valide,
    clientNom: 'Alexandre Dumas',
    hotelNom: 'Le Grand Palais Royal'
  },
  {
    id: 'av2',
    clientId: 'u222',
    hotelId: '11111111-1111-1111-1111-111111111111',
    reservationId: 'r222',
    note: 4,
    commentaire: 'Très bel établissement, petit déjeuner gastronomique et personnel aux petits soins.',
    date: '2026-11-02T16:40:00Z',
    statut: StatutAvis.EnAttente,
    clientNom: 'Éléonore Martin',
    hotelNom: 'Le Grand Palais Royal'
  }
];

export const DEMO_AUDIT_LOGS: AuditLogDto[] = [
  {
    id: 'log-01',
    utilisateurId: 'admin-1',
    action: 'Création Établissement',
    entite: 'Hotel',
    entiteId: '11111111-1111-1111-1111-111111111111',
    dateAction: '2026-11-04T08:12:30Z',
    details: 'Création de l’hôtel Le Grand Palais Royal à Paris.',
    adresseIp: '192.168.1.45'
  },
  {
    id: 'log-02',
    utilisateurId: 'agent-1',
    action: 'Confirmation Réservation',
    entite: 'Reservation',
    entiteId: 'r111-111',
    dateAction: '2026-11-05T10:04:12Z',
    details: 'Confirmation manuelle de la réservation avec encaissement acompte.',
    adresseIp: '192.168.1.52'
  },
  {
    id: 'log-03',
    utilisateurId: 'admin-1',
    action: 'Validation Avis',
    entite: 'Avis',
    entiteId: 'av1',
    dateAction: '2026-11-05T11:22:00Z',
    details: 'Modération approuvée pour le client Alexandre Dumas.',
    adresseIp: '192.168.1.45'
  }
];

export const DEMO_PAIEMENTS: Paiement[] = [
  {
    id: 'p111',
    reservationId: 'r111-111',
    montant: 1280.00,
    devise: 'EUR',
    date: '2026-11-05T09:35:00Z',
    modePaiement: ModePaiement.Carte,
    reference: 'PAY-2026-88741',
    statut: StatutPaiement.Paye,
    hotelNom: 'Le Grand Palais Royal'
  },
  {
    id: 'p222',
    reservationId: 'r222-222',
    montant: 585.00,
    devise: 'EUR',
    date: '2026-11-05T14:25:00Z',
    modePaiement: ModePaiement.Virement,
    reference: 'VIR-2026-44102',
    statut: StatutPaiement.EnAttente,
    hotelNom: 'Hôtel Bellevue Riviera'
  }
];
