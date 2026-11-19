import { Component, Input, OnInit } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AvisService } from '../../services/avis.service';
import { Avis } from '../../models/avis.model';

@Component({
  selector: 'app-avis-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './avis-list.html',
  styleUrls: ['./avis-list.scss']
})
export class AvisListComponent implements OnInit {
  @Input() hotelId!: string;
  avisList: Avis[] = [];
  loading: boolean = true;

  constructor(private avisService: AvisService) {}

  ngOnInit(): void {
    if (this.hotelId) {
      this.chargerAvis();
    }
  }

  chargerAvis(): void {
    this.loading = true;
    this.avisService.getByHotel(this.hotelId).subscribe({
      next: (data) => {
        this.avisList = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
      }
    });
  }

  getStarsArray(note: number): number[] {
    return Array(note).fill(0);
  }
}
