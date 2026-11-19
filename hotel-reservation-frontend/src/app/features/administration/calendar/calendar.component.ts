import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { ChambreService } from '../../../core/services/chambre.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { Hotel, Chambre, Reservation, StatutReservation, ReservationCalendrierDto } from '../../../core/models';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';
import { RoomTypePipe } from '../../../shared/pipes/room-type.pipe';
import { StatusBadgePipe } from '../../../shared/pipes/status-badge.pipe';

@Component({
  selector: 'app-calendar',
  standalone: true,
  imports: [CommonModule, FormsModule, LoadingSpinnerComponent, RoomTypePipe, StatusBadgePipe],
  template: `
    <div class="calendar-page">
      <div class="page-header flex-header">
        <div>
          <h1 class="page-title">Calendrier de Disponibilité & Planning</h1>
          <p class="page-subtitle">Occupation en temps réel calculée sur les réservations et chambres</p>
        </div>

        <div class="view-switcher">
          <button 
            class="view-btn" 
            [class.active]="viewMode() === 'day'" 
            (click)="viewMode.set('day')">
            Jour
          </button>
          <button 
            class="view-btn" 
            [class.active]="viewMode() === 'week'" 
            (click)="viewMode.set('week')">
            Semaine
          </button>
          <button 
            class="view-btn" 
            [class.active]="viewMode() === 'month'" 
            (click)="viewMode.set('month')">
            Mois
          </button>
        </div>
      </div>

      <!-- Barre de contrôle hôtel & dates -->
      <div class="card-luxury p-controls">
        <div class="controls-grid">
          <div class="ctrl-col">
            <label class="form-label">Établissement :</label>
            <select class="form-select" [ngModel]="selectedHotelId()" (ngModelChange)="onHotelChange($event)">
              @for (h of hotels(); track h.id) {
                <option [value]="h.id">{{ h.nom }} ({{ h.ville }})</option>
              }
            </select>
          </div>

          <div class="ctrl-col nav-dates-wrap">
            <label class="form-label">Période affichée :</label>
            <div class="date-navigator">
              <button class="btn btn-outline btn-sm" (click)="navigatePeriod(-1)">◀ Précédent</button>
              <span class="period-display">{{ periodLabel() }}</span>
              <button class="btn btn-outline btn-sm" (click)="navigatePeriod(1)">Suivant ▶</button>
              <button class="btn btn-ghost btn-sm" (click)="goToToday()">Aujourd'hui</button>
            </div>
          </div>
        </div>
      </div>

      @if (isLoading()) {
        <app-loading-spinner message="Calcul du planning d'occupation..."></app-loading-spinner>
      } @else {
        <!-- Matrice du Calendrier -->
        <div class="calendar-matrix-wrapper card-luxury">
          <table class="calendar-table">
            <thead>
              <tr>
                <th class="th-room-fixed">Chambres</th>
                @for (d of currentDays(); track d.dateStr) {
                  <th class="th-day-col" [class.today]="d.isToday">
                    <span class="d-name">{{ d.dayName }}</span>
                    <span class="d-num">{{ d.dayNum }}</span>
                  </th>
                }
              </tr>
            </thead>
            <tbody>
              @for (r of rooms(); track r.id) {
                <tr>
                  <td class="td-room-info">
                    <span class="r-number">Ch. {{ r.numero }}</span>
                    <small class="r-type">{{ r.type | roomType }}</small>
                  </td>

                  @for (d of currentDays(); track d.dateStr) {
                    @let booking = getBookingForRoomAndDate(r.id, d.dateStr);
                    <td class="td-cell" [class.is-today]="d.isToday">
                      @if (booking) {
                        <div 
                          class="booking-slot" 
                          [ngClass]="getSlotClass(booking.statut)"
                          (click)="openDetail(booking, r)"
                          title="Cliquer pour afficher les détails">
                          <span class="slot-text">#{{ booking.reservationId.substring(0, 6) }}</span>
                        </div>
                      } @else {
                        <div class="empty-slot" title="Libre">·</div>
                      }
                    </td>
                  }
                </tr>
              }
            </tbody>
          </table>
        </div>

        <!-- Légende -->
        <div class="calendar-legend">
          <div class="legend-item"><span class="legend-box confirmed"></span> Confirmée</div>
          <div class="legend-item"><span class="legend-box pending"></span> En Attente</div>
          <div class="legend-item"><span class="legend-box completed"></span> Terminée</div>
          <div class="legend-item"><span class="legend-box free"></span> Disponible</div>
        </div>
      }

      <!-- Modale Détail au Clic -->
      @if (selectedBookingDetail()) {
        <div class="modal-overlay" (click)="selectedBookingDetail.set(null)">
          <div class="modal-dialog" (click)="$event.stopPropagation()">
            <div class="card-header">
              <h3 class="modal-title">Détail de la Réservation</h3>
              <button class="btn-close" (click)="selectedBookingDetail.set(null)">✕</button>
            </div>
            <div class="card-body">
              <div class="detail-rows">
                <p><strong>Réf Réservation :</strong> #{{ selectedBookingDetail()!.booking.reservationId }}</p>
                <p><strong>Établissement :</strong> {{ selectedBookingDetail()!.booking.hotelNom }}</p>
                <p><strong>Chambre :</strong> n° {{ selectedBookingDetail()!.room.numero }} ({{ selectedBookingDetail()!.room.type | roomType }})</p>
                <p><strong>Date d'arrivée :</strong> {{ selectedBookingDetail()!.booking.dateDebut | date:'dd MMMM yyyy' }}</p>
                <p><strong>Date de départ :</strong> {{ selectedBookingDetail()!.booking.dateFin | date:'dd MMMM yyyy' }}</p>
                <p><strong>Invités :</strong> {{ selectedBookingDetail()!.booking.nbPersonnes }} personne(s)</p>
                <p>
                  <strong>Statut actuel :</strong> 
                  @let b = (selectedBookingDetail()!.booking.statut | statusBadge:'reservation');
                  <span class="badge" [ngClass]="b.cssClass">{{ b.label }}</span>
                </p>
              </div>
            </div>
            <div class="card-footer modal-actions">
              <button class="btn btn-primary btn-sm" (click)="selectedBookingDetail.set(null)">Fermer</button>
            </div>
          </div>
        </div>
      }
    </div>
  `,
  styles: [`
    .flex-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 1.5rem;
    }

    .view-switcher {
      display: flex;
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      overflow: hidden;
      background: var(--white);
    }

    .view-btn {
      padding: 0.5rem 1.25rem;
      border: none;
      background: none;
      font-size: 0.885rem;
      font-weight: 600;
      color: var(--text-secondary);
      cursor: pointer;
      transition: all var(--transition-fast);
    }

    .view-btn.active {
      background: var(--forest-green);
      color: var(--white);
    }

    .p-controls {
      padding: 1.25rem 1.5rem;
      margin-bottom: 1.5rem;
    }

    .controls-grid {
      display: flex;
      justify-content: space-between;
      align-items: center;
      gap: 2rem;
      flex-wrap: wrap;
    }

    .date-navigator {
      display: flex;
      align-items: center;
      gap: 0.75rem;
    }

    .period-display {
      font-weight: 700;
      color: var(--forest-green);
      font-size: 0.95rem;
      min-width: 170px;
      text-align: center;
    }

    /* Table Calendar */
    .calendar-matrix-wrapper {
      overflow-x: auto;
      margin-bottom: 1.5rem;
    }

    .calendar-table {
      width: 100%;
      border-collapse: collapse;
      font-size: 0.85rem;
      table-layout: fixed;
    }

    .th-room-fixed {
      width: 160px;
      background: var(--ivory-light);
      padding: 0.85rem 1rem;
      border-bottom: 2px solid var(--border-subtle);
      border-right: 1px solid var(--border-subtle);
      text-align: left;
    }

    .th-day-col {
      padding: 0.65rem 0.25rem;
      text-align: center;
      background: var(--ivory-light);
      border-bottom: 2px solid var(--border-subtle);
      border-right: 1px solid var(--border-subtle);
      min-width: 44px;
    }

    .th-day-col.today {
      background: #f7f1e2;
    }

    .d-name {
      display: block;
      font-size: 0.7rem;
      color: var(--text-muted);
      text-transform: uppercase;
    }

    .d-num {
      display: block;
      font-weight: 700;
      color: var(--forest-green);
    }

    .td-room-info {
      padding: 0.75rem 1rem;
      border-bottom: 1px solid var(--border-subtle);
      border-right: 1px solid var(--border-subtle);
      background: var(--white);
    }

    .r-number {
      font-weight: 700;
      color: var(--forest-green);
      display: block;
    }

    .r-type {
      font-size: 0.75rem;
      color: var(--text-muted);
      display: block;
    }

    .td-cell {
      border-bottom: 1px solid var(--border-subtle);
      border-right: 1px solid var(--border-subtle);
      padding: 4px;
      text-align: center;
      vertical-align: middle;
      background: var(--white);
    }

    .td-cell.is-today {
      background: rgba(214, 183, 122, 0.08);
    }

    .booking-slot {
      border-radius: var(--radius-xs);
      padding: 0.35rem 0.2rem;
      font-size: 0.7rem;
      font-weight: 700;
      color: var(--white);
      cursor: pointer;
      text-overflow: ellipsis;
      overflow: hidden;
      white-space: nowrap;
      transition: transform var(--transition-fast);
    }

    .booking-slot:hover {
      transform: scale(1.05);
      box-shadow: var(--shadow-sm);
    }

    .slot-confirmed { background-color: var(--color-success); }
    .slot-pending { background-color: var(--color-warning); }
    .slot-completed { background-color: var(--color-info); }
    .slot-cancelled { background-color: var(--color-error); }

    .empty-slot {
      color: #ddd;
      font-size: 1.2rem;
    }

    /* Légende */
    .calendar-legend {
      display: flex;
      gap: 1.5rem;
      font-size: 0.85rem;
      color: var(--text-secondary);
    }

    .legend-item {
      display: flex;
      align-items: center;
      gap: 0.4rem;
    }

    .legend-box {
      width: 14px;
      height: 14px;
      border-radius: 3px;
    }

    .legend-box.confirmed { background: var(--color-success); }
    .legend-box.pending { background: var(--color-warning); }
    .legend-box.completed { background: var(--color-info); }
    .legend-box.free { background: #eee; border: 1px solid #ccc; }

    .modal-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
    }

    .detail-rows p {
      font-size: 0.9rem;
      margin-bottom: 0.65rem;
      color: var(--text-secondary);
    }

    .modal-actions {
      display: flex;
      justify-content: flex-end;
    }
  `]
})
export class CalendarComponent implements OnInit {
  private hotelService = inject(HotelService);
  private chambreService = inject(ChambreService);
  private reservationService = inject(ReservationService);

  hotels = signal<Hotel[]>([]);
  selectedHotelId = signal<string | null>(null);
  rooms = signal<Chambre[]>([]);
  bookings = signal<ReservationCalendrierDto[]>([]);

  isLoading = signal<boolean>(true);
  viewMode = signal<'day' | 'week' | 'month'>('week');
  currentDate = signal<Date>(new Date());

  selectedBookingDetail = signal<{ booking: ReservationCalendrierDto; room: Chambre } | null>(null);

  currentDays = computed(() => {
    const mode = this.viewMode();
    const base = new Date(this.currentDate());
    const days: { dateStr: string; dayName: string; dayNum: number; isToday: boolean }[] = [];
    const todayStr = new Date().toISOString().substring(0, 10);

    let count = 7;
    let start = new Date(base);

    if (mode === 'day') {
      count = 1;
    } else if (mode === 'week') {
      count = 7;
      // début de semaine (lundi)
      const day = start.getDay();
      const diff = start.getDate() - day + (day === 0 ? -6 : 1);
      start.setDate(diff);
    } else if (mode === 'month') {
      count = 14; // afficher 14 jours par page de mois pour lisibilité
    }

    const dayNames = ['Dim', 'Lun', 'Mar', 'Mer', 'Jeu', 'Ven', 'Sam'];

    for (let i = 0; i < count; i++) {
      const d = new Date(start);
      d.setDate(start.getDate() + i);
      const iso = d.toISOString().substring(0, 10);
      days.push({
        dateStr: iso,
        dayName: dayNames[d.getDay()],
        dayNum: d.getDate(),
        isToday: iso === todayStr
      });
    }

    return days;
  });

  periodLabel = computed(() => {
    const days = this.currentDays();
    if (days.length === 0) return '';
    if (days.length === 1) return days[0].dateStr;
    return `${days[0].dateStr} au ${days[days.length - 1].dateStr}`;
  });

  ngOnInit(): void {
    this.hotelService.getHotels().subscribe({
      next: (hList) => {
        this.hotels.set(hList);
        if (hList.length > 0) {
          this.onHotelChange(hList[0].id);
        } else {
          this.isLoading.set(false);
        }
      },
      error: () => this.isLoading.set(false)
    });
  }

  onHotelChange(hotelId: string): void {
    this.selectedHotelId.set(hotelId);
    this.loadData(hotelId);
  }

  loadData(hotelId: string): void {
    this.isLoading.set(true);
    this.chambreService.getChambresByHotel(hotelId).subscribe(rooms => {
      this.rooms.set(rooms);

      const days = this.currentDays();
      if (days.length > 0) {
        const start = days[0].dateStr;
        const end = days[days.length - 1].dateStr;
        this.reservationService.getCalendrier(hotelId, start, end).subscribe(cal => {
          this.bookings.set(cal.reservations || []);
          this.isLoading.set(false);
        });
      } else {
        this.isLoading.set(false);
      }
    });
  }

  navigatePeriod(delta: number): void {
    const cur = new Date(this.currentDate());
    const mode = this.viewMode();
    const daysStep = mode === 'day' ? 1 : mode === 'week' ? 7 : 14;
    cur.setDate(cur.getDate() + delta * daysStep);
    this.currentDate.set(cur);

    const hid = this.selectedHotelId();
    if (hid) this.loadData(hid);
  }

  goToToday(): void {
    this.currentDate.set(new Date());
    const hid = this.selectedHotelId();
    if (hid) this.loadData(hid);
  }

  getBookingForRoomAndDate(roomId: string, dateStr: string): ReservationCalendrierDto | undefined {
    return this.bookings().find(b => {
      if (b.chambreId !== roomId) return false;
      const start = b.dateDebut.substring(0, 10);
      const end = b.dateFin.substring(0, 10);
      return dateStr >= start && dateStr < end;
    });
  }

  getSlotClass(statut: number): string {
    switch (statut) {
      case StatutReservation.Confirmee: return 'slot-confirmed';
      case StatutReservation.EnAttente: return 'slot-pending';
      case StatutReservation.Terminee: return 'slot-completed';
      case StatutReservation.Annulee: return 'slot-cancelled';
      default: return 'slot-confirmed';
    }
  }

  openDetail(booking: ReservationCalendrierDto, room: Chambre): void {
    this.selectedBookingDetail.set({ booking, room });
  }
}
