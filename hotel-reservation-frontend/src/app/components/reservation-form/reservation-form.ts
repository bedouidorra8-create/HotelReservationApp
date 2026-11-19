import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ReservationService } from '../../services/reservation.service';
import { CreerReservationRequest } from '../../models/reservation.model';

@Component({
  selector: 'app-reservation-form',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './reservation-form.html',
  styleUrls: ['./reservation-form.scss']
})
export class ReservationFormComponent implements OnInit {
  @Input() prefilledChambreId: string | null = null;
  @Input() prefilledDateArrivee: string = '';
  @Input() prefilledDateDepart: string = '';

  @Output() reservationCreated = new EventEmitter<void>();

  chambreId: string = '';
  clientId: string = '11111111-1111-1111-1111-111111111111'; // Demo Client GUID
  dateArrivee: string = '';
  dateDepart: string = '';
  nbPersonnes: number = 2;

  loading: boolean = false;
  errorMessage: string = '';
  successMessage: string = '';

  constructor(private reservationService: ReservationService) {}

  ngOnInit(): void {
    if (this.prefilledChambreId) this.chambreId = this.prefilledChambreId;
    if (this.prefilledDateArrivee) this.dateArrivee = this.prefilledDateArrivee;
    if (this.prefilledDateDepart) this.dateDepart = this.prefilledDateDepart;
  }

  soumettre(): void {
    this.errorMessage = '';
    this.successMessage = '';

    if (!this.chambreId || !this.dateArrivee || !this.dateDepart) {
      this.errorMessage = 'Veuillez remplir tous les champs obligatoires.';
      return;
    }

    const req: CreerReservationRequest = {
      chambreId: this.chambreId,
      clientId: this.clientId,
      dateArrivee: this.dateArrivee,
      dateDepart: this.dateDepart,
      nbPersonnes: this.nbPersonnes
    };

    this.loading = true;
    this.reservationService.create(req).subscribe({
      next: (res) => {
        this.loading = false;
        this.successMessage = 'Réservation effectuée avec succès !';
        setTimeout(() => {
          this.reservationCreated.emit();
        }, 1500);
      },
      error: (err) => {
        this.loading = false;
        this.errorMessage = err.error || 'Erreur lors de la réservation (chevauchement ou chambre indisponible).';
      }
    });
  }
}
