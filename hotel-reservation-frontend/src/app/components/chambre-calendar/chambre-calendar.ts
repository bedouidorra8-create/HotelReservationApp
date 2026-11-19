import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../../services/hotel.service';
import { DisponibilitesHotelResponse, DisponibiliteChambre } from '../../models/hotel.model';

@Component({
  selector: 'app-chambre-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './chambre-calendar.html',
  styleUrls: ['./chambre-calendar.scss']
})
export class ChambreCalendarComponent implements OnInit {
  @Input() hotelId!: string;
  @Output() cellSelected = new EventEmitter<{ chambreId: string; dateArrivee: string; dateDepart: string }>();

  dateDebut: string = new Date().toISOString().split('T')[0];
  dateFin: string = new Date(Date.now() + 7 * 24 * 60 * 60 * 1000).toISOString().split('T')[0];

  disponibilites: DisponibilitesHotelResponse | null = null;
  joursList: string[] = [];
  loading: boolean = false;
  errorMessage: string = '';

  constructor(private hotelService: HotelService) {}

  ngOnInit(): void {
    if (this.hotelId) {
      this.chargerDisponibilites();
    }
  }

  chargerDisponibilites(): void {
    if (!this.hotelId) return;
    this.loading = true;
    this.errorMessage = '';

    this.hotelService.getDisponibilites(this.hotelId, this.dateDebut, this.dateFin).subscribe({
      next: (res) => {
        this.disponibilites = res;
        this.extraireJours();
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Erreur lors du chargement des disponibilités.';
        this.loading = false;
      }
    });
  }

  private extraireJours(): void {
    if (!this.disponibilites || this.disponibilites.chambres.length === 0) {
      this.joursList = [];
      return;
    }
    const firstChambre = this.disponibilites.chambres[0];
    this.joursList = Object.keys(firstChambre.statutParJour).sort();
  }

  getStatutClass(statut: string): string {
    switch (statut) {
      case 'Libre': return 'cell-libre';
      case 'Occupee': return 'cell-occupee';
      case 'Maintenance': return 'cell-maintenance';
      default: return '';
    }
  }

  onCellClick(chambre: DisponibiliteChambre, jour: string, statut: string): void {
    if (statut !== 'Libre') return;

    // Default 1 night selection from selected day
    const dateArr = new Date(jour);
    const dateDep = new Date(dateArr.getTime() + 24 * 60 * 60 * 1000);

    const dateArriveeStr = dateArr.toISOString().split('T')[0];
    const dateDepartStr = dateDep.toISOString().split('T')[0];

    this.cellSelected.emit({
      chambreId: chambre.chambreId,
      dateArrivee: dateArriveeStr,
      dateDepart: dateDepartStr
    });
  }

  getTypeLabel(type: number): string {
    switch (type) {
      case 1: return 'Simple';
      case 2: return 'Double';
      case 3: return 'Suite';
      case 4: return 'Familiale';
      default: return 'Inconnu';
    }
  }
}
