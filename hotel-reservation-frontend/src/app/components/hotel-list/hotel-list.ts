import { Component, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HotelService } from '../../services/hotel.service';
import { Hotel } from '../../models/hotel.model';

@Component({
  selector: 'app-hotel-list',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterModule],
  templateUrl: './hotel-list.html',
  styleUrls: ['./hotel-list.scss']
})
export class HotelListComponent implements OnInit {
  hotels: Hotel[] = [];
  filteredHotels: Hotel[] = [];
  searchTerm: string = '';
  loading: boolean = true;
  errorMessage: string = '';

  constructor(private hotelService: HotelService) {}

  ngOnInit(): void {
    this.chargerHotels();
  }

  chargerHotels(): void {
    this.loading = true;
    this.hotelService.getAll().subscribe({
      next: (data) => {
        this.hotels = data;
        this.filteredHotels = data;
        this.loading = false;
      },
      error: (err) => {
        this.errorMessage = 'Impossible de charger la liste des hôtels.';
        this.loading = false;
      }
    });
  }

  filtrer(): void {
    const term = this.searchTerm.toLowerCase().trim();
    if (!term) {
      this.filteredHotels = this.hotels;
    } else {
      this.filteredHotels = this.hotels.filter(h =>
        h.nom.toLowerCase().includes(term) || h.ville.toLowerCase().includes(term)
      );
    }
  }

  getEtoilesArray(count: number): number[] {
    return Array(count).fill(0);
  }
}
