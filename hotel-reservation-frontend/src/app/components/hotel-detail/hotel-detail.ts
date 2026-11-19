import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { HotelService } from '../../services/hotel.service';
import { Hotel } from '../../models/hotel.model';
import { ChambreCalendarComponent } from '../chambre-calendar/chambre-calendar';
import { ReservationFormComponent } from '../reservation-form/reservation-form';
import { AvisListComponent } from '../avis-list/avis-list';

@Component({
  selector: 'app-hotel-detail',
  standalone: true,
  imports: [
    CommonModule,
    RouterModule,
    ChambreCalendarComponent,
    ReservationFormComponent,
    AvisListComponent
  ],
  templateUrl: './hotel-detail.html',
  styleUrls: ['./hotel-detail.scss']
})
export class HotelDetailComponent implements OnInit {
  hotel: Hotel | null = null;
  hotelId: string = '';
  loading: boolean = true;
  errorMessage: string = '';

  // Selected reservation data from calendar click
  selectedChambreId: string | null = null;
  selectedDateArrivee: string = '';
  selectedDateDepart: string = '';
  showReservationModal: boolean = false;

  constructor(
    private route: ActivatedRoute,
    private hotelService: HotelService
  ) {}

  ngOnInit(): void {
    this.hotelId = this.route.snapshot.paramMap.get('id') || '';
    if (this.hotelId) {
      this.chargerHotel();
    }
  }

  chargerHotel(): void {
    this.loading = true;
    this.hotelService.getById(this.hotelId).subscribe({
      next: (data) => {
        this.hotel = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Erreur lors du chargement des détails de l\'hôtel.';
        this.loading = false;
      }
    });
  }

  onCellSelected(event: { chambreId: string; dateArrivee: string; dateDepart: string }): void {
    this.selectedChambreId = event.chambreId;
    this.selectedDateArrivee = event.dateArrivee;
    this.selectedDateDepart = event.dateDepart;
    this.showReservationModal = true;
  }

  fermerModal(): void {
    this.showReservationModal = false;
  }
}
