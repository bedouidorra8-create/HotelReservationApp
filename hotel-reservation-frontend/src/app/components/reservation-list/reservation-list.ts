import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReservationService } from '../../services/reservation.service';
import { Reservation } from '../../models/reservation.model';

@Component({
  selector: 'app-reservation-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './reservation-list.html',
  styleUrls: ['./reservation-list.scss']
})
export class ReservationListComponent implements OnInit {
  reservations: Reservation[] = [];
  loading: boolean = true;
  errorMessage: string = '';

  constructor(private reservationService: ReservationService) {}

  ngOnInit(): void {
    this.chargerReservations();
  }

  chargerReservations(): void {
    this.loading = true;
    this.reservationService.getAll().subscribe({
      next: (data) => {
        this.reservations = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Erreur lors du chargement des réservations.';
        this.loading = false;
      }
    });
  }

  confirmer(id: string): void {
    this.reservationService.confirmer(id).subscribe({
      next: () => this.chargerReservations()
    });
  }

  annuler(id: string): void {
    this.reservationService.annuler(id).subscribe({
      next: () => this.chargerReservations()
    });
  }

  getStatutLabel(statut: number): string {
    switch (statut) {
      case 1: return 'En attente';
      case 2: return 'Confirmée';
      case 3: return 'Annulée';
      case 4: return 'Terminée';
      default: return 'Inconnu';
    }
  }

  getStatutClass(statut: number): string {
    switch (statut) {
      case 1: return 'badge-attente';
      case 2: return 'badge-confirmee';
      case 3: return 'badge-annulee';
      case 4: return 'badge-terminee';
      default: return '';
    }
  }
}
