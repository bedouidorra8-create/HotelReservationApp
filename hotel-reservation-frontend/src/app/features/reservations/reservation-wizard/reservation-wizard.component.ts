import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, ActivatedRoute, Router } from '@angular/router';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { HotelService } from '../../../core/services/hotel.service';
import { ChambreService } from '../../../core/services/chambre.service';
import { ReservationService } from '../../../core/services/reservation.service';
import { PaiementService } from '../../../core/services/paiement.service';
import { AuthService } from '../../../core/auth/auth.service';
import { NotificationService } from '../../../core/services/notification.service';
import { Hotel, Chambre, Reservation, ModePaiement } from '../../../core/models';
import { RoomTypePipe } from '../../../shared/pipes/room-type.pipe';
import { LoadingSpinnerComponent } from '../../../shared/components/loading-spinner.component';

@Component({
  selector: 'app-reservation-wizard',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, ReactiveFormsModule, RoomTypePipe, LoadingSpinnerComponent],
  template: `
    <div class="wizard-container">
      <div class="container py-wizard">
        <!-- En-tête Étapes -->
        <div class="wizard-header text-center">
          <span class="sub-badge">EXPÉRIENCE PRIVILÈGE</span>
          <h1 class="wizard-main-title">Réservation de votre Séjour</h1>
          <p class="wizard-desc">Personnalisez chaque détail de votre parenthèse d'exception</p>

          <!-- Stepper -->
          <div class="stepper-bar">
            <div class="step-step" [class.active]="currentStep() >= 1" [class.current]="currentStep() === 1">
              <span class="step-num">1</span>
              <span class="step-label">Établissement</span>
            </div>
            <div class="step-line" [class.filled]="currentStep() >= 2"></div>
            <div class="step-step" [class.active]="currentStep() >= 2" [class.current]="currentStep() === 2">
              <span class="step-num">2</span>
              <span class="step-label">Chambre</span>
            </div>
            <div class="step-line" [class.filled]="currentStep() >= 3"></div>
            <div class="step-step" [class.active]="currentStep() >= 3" [class.current]="currentStep() === 3">
              <span class="step-num">3</span>
              <span class="step-label">Dates & Invités</span>
            </div>
            <div class="step-line" [class.filled]="currentStep() >= 4"></div>
            <div class="step-step" [class.active]="currentStep() >= 4" [class.current]="currentStep() === 4">
              <span class="step-num">4</span>
              <span class="step-label">Récapitulatif</span>
            </div>
            <div class="step-line" [class.filled]="currentStep() >= 5"></div>
            <div class="step-step" [class.active]="currentStep() >= 5" [class.current]="currentStep() === 5">
              <span class="step-num">5</span>
              <span class="step-label">Confirmation</span>
            </div>
          </div>
        </div>

        @if (isLoading()) {
          <app-loading-spinner message="Chargement des données du séjour..."></app-loading-spinner>
        } @else {
          <div class="wizard-content">
            <!-- ÉTAPE 1 : CHOIX DE L'HÔTEL -->
            @if (currentStep() === 1) {
              <div class="step-panel card-luxury p-panel">
                <h2 class="panel-title">1. Choisissez votre Établissement</h2>
                <div class="hotels-selection-grid">
                  @for (h of allHotels(); track h.id) {
                    <div 
                      class="hotel-select-card" 
                      [class.selected]="selectedHotel()?.id === h.id"
                      (click)="onSelectHotel(h)">
                      <img [src]="h.images[0] || 'https://images.unsplash.com/photo-1566073771259-6a8506099945?auto=format&fit=crop&w=400&q=80'" [alt]="h.nom" class="h-img" />
                      <div class="h-info">
                        <span class="h-city">📍 {{ h.ville }}</span>
                        <h3 class="h-name">{{ h.nom }}</h3>
                        <p class="h-price">Dès {{ h.prixMin || 250 }} DT / nuit</p>
                      </div>
                    </div>
                  }
                </div>

                <div class="panel-actions">
                  <button class="btn btn-gold" [disabled]="!selectedHotel()" (click)="goToStep(2)">
                    Continuer vers le choix de la chambre →
                  </button>
                </div>
              </div>
            }

            <!-- ÉTAPE 2 : CHOIX DE LA CHAMBRE -->
            @if (currentStep() === 2) {
              <div class="step-panel card-luxury p-panel">
                <div class="panel-header-row">
                  <div>
                    <h2 class="panel-title">2. Choisissez votre Chambre</h2>
                    <p class="panel-sub">Hôtel sélectionné : <strong>{{ selectedHotel()?.nom }}</strong> ({{ selectedHotel()?.ville }})</p>
                  </div>
                  <button class="btn btn-ghost btn-sm" (click)="goToStep(1)">Modifier l'hôtel</button>
                </div>

                @if (hotelRooms().length === 0) {
                  <p class="no-rooms-msg">Aucune chambre disponible dans cet établissement pour le moment.</p>
                } @else {
                  <div class="rooms-selection-list">
                    @for (r of hotelRooms(); track r.id) {
                      <div 
                        class="room-select-item" 
                        [class.selected]="selectedRoom()?.id === r.id"
                        (click)="onSelectRoom(r)">
                        <div class="room-details">
                          <span class="room-numb">Chambre n° {{ r.numero }} • Étage {{ r.etage }}</span>
                          <h4 class="room-title">{{ r.type | roomType }}</h4>
                          <span class="room-feature">Insonorisation royale • Vue panoramique • Literie d'exception</span>
                        </div>
                        <div class="room-tariff">
                          <span class="rate">{{ r.prixMontant }} {{ r.prixDevise || 'DT' }}</span>
                          <span class="rate-sub">par nuit</span>
                        </div>
                      </div>
                    }
                  </div>
                }

                <div class="panel-actions">
                  <button class="btn btn-outline" (click)="goToStep(1)">← Précédent</button>
                  <button class="btn btn-gold" [disabled]="!selectedRoom()" (click)="goToStep(3)">
                    Définir les dates et invités →
                  </button>
                </div>
              </div>
            }

            <!-- ÉTAPE 3 : DATES & INVITÉS -->
            @if (currentStep() === 3) {
              <div class="step-panel card-luxury p-panel">
                <h2 class="panel-title">3. Période de Séjour & Voyageurs</h2>

                <form [formGroup]="datesForm" class="dates-form">
                  <div class="form-row">
                    <div class="form-group flex-1">
                      <label class="form-label">Date d'arrivée *</label>
                      <input type="date" formControlName="dateArrivee" class="form-control" (change)="calculateTotal()" />
                    </div>

                    <div class="form-group flex-1">
                      <label class="form-label">Date de départ *</label>
                      <input type="date" formControlName="dateDepart" class="form-control" (change)="calculateTotal()" />
                    </div>
                  </div>

                  <div class="form-group">
                    <label class="form-label">Nombre de personnes / Invités *</label>
                    <select formControlName="nbPersonnes" class="form-select">
                      <option [ngValue]="1">1 personne (Individuel)</option>
                      <option [ngValue]="2">2 personnes (Couple / Double)</option>
                      <option [ngValue]="3">3 personnes</option>
                      <option [ngValue]="4">4 personnes (Famille)</option>
                    </select>
                  </div>

                  @if (dateError()) {
                    <div class="form-error">
                      <span>⚠</span> {{ dateError() }}
                    </div>
                  }

                  @if (nbNuits() > 0) {
                    <div class="duration-highlight">
                      <span>Durée calculée : <strong>{{ nbNuits() }} nuitée(s)</strong></span>
                      <span>Montant prévisionnel : <strong>{{ calculatedTotal() }} DT</strong></span>
                    </div>
                  }
                </form>

                <div class="panel-actions">
                  <button class="btn btn-outline" (click)="goToStep(2)">← Précédent</button>
                  <button class="btn btn-gold" [disabled]="datesForm.invalid || !!dateError() || nbNuits() <= 0" (click)="goToStep(4)">
                    Vérifier le récapitulatif →
                  </button>
                </div>
              </div>
            }

            <!-- ÉTAPE 4 : RÉCAPITULATIF & SOUMISSION -->
            @if (currentStep() === 4) {
              <div class="step-panel card-luxury p-panel">
                <h2 class="panel-title">4. Récapitulatif de votre Réservation</h2>
                <p class="panel-sub">Vérifiez l'exactitude de vos informations avant de confirmer.</p>

                <div class="recap-grid">
                  <div class="recap-block">
                    <h3 class="recap-block-title">Détails de l'Établissement</h3>
                    <p class="recap-line"><strong>Hôtel :</strong> {{ selectedHotel()?.nom }}</p>
                    <p class="recap-line"><strong>Adresse :</strong> {{ selectedHotel()?.adresse }}, {{ selectedHotel()?.ville }}</p>
                    <p class="recap-line"><strong>Contact :</strong> {{ selectedHotel()?.telephone }}</p>
                  </div>

                  <div class="recap-block">
                    <h3 class="recap-block-title">Hébergement Choisi</h3>
                    <p class="recap-line"><strong>Chambre :</strong> n° {{ selectedRoom()?.numero }} (Étage {{ selectedRoom()?.etage }})</p>
                    <p class="recap-line"><strong>Type :</strong> {{ selectedRoom()?.type | roomType }}</p>
                    <p class="recap-line"><strong>Prix unitaire :</strong> {{ selectedRoom()?.prixMontant }} {{ selectedRoom()?.prixDevise || 'DT' }} / nuit</p>
                  </div>

                  <div class="recap-block">
                    <h3 class="recap-block-title">Séjour & Invités</h3>
                    <p class="recap-line"><strong>Arrivée :</strong> {{ datesForm.value.dateArrivee | date:'EEEE dd MMMM yyyy' }} (dès 15h)</p>
                    <p class="recap-line"><strong>Départ :</strong> {{ datesForm.value.dateDepart | date:'EEEE dd MMMM yyyy' }} (jusqu'à 11h)</p>
                    <p class="recap-line"><strong>Voyageurs :</strong> {{ datesForm.value.nbPersonnes }} personne(s)</p>
                  </div>

                  <div class="recap-block highlight-total">
                    <h3 class="recap-block-title">Calcul Financier</h3>
                    <div class="calc-row">
                      <span>Tarif par nuitée</span>
                      <span>{{ selectedRoom()?.prixMontant }} DT</span>
                    </div>
                    <div class="calc-row">
                      <span>Nombre de nuits</span>
                      <span>× {{ nbNuits() }}</span>
                    </div>
                    <div class="total-row">
                      <span>Montant Total Figé</span>
                      <span class="total-val">{{ calculatedTotal() }} DT</span>
                    </div>
                  </div>
                </div>

                <div class="panel-actions">
                  <button class="btn btn-outline" (click)="goToStep(3)">← Modifier les dates</button>
                  <button class="btn btn-gold btn-lg" [disabled]="isSubmitting()" (click)="submitReservation()">
                    @if (isSubmitting()) {
                      <span>Création de la réservation...</span>
                    } @else {
                      <span>Confirmer ma réservation ✓</span>
                    }
                  </button>
                </div>
              </div>
            }

            <!-- ÉTAPE 5 : CONFIRMATION & PAIEMENT -->
            @if (currentStep() === 5 && confirmedReservation()) {
              <div class="step-panel card-luxury p-panel text-center">
                <div class="success-icon">✓</div>
                <h2 class="panel-title">Réservation Confirmée avec Succès !</h2>
                <p class="success-sub">
                  Votre référence de réservation : <strong>#{{ confirmedReservation()?.id?.substring(0, 8) }}</strong>
                </p>

                <div class="payment-action-box card-luxury">
                  <h3 class="pay-title">Règlement du Séjour</h3>
                  <p class="pay-desc">
                    Montant total à régler : <strong>{{ calculatedTotal() }} DT</strong>
                  </p>

                  <div class="payment-methods-grid">
                    <div 
                      class="pay-method-card" 
                      [class.selected]="selectedPaymentMode() === 1"
                      (click)="selectedPaymentMode.set(1)">
                      <span class="pay-icon">💳</span>
                      <span class="pay-name">Carte Bancaire</span>
                    </div>
                    <div 
                      class="pay-method-card" 
                      [class.selected]="selectedPaymentMode() === 3"
                      (click)="selectedPaymentMode.set(3)">
                      <span class="pay-icon">🏦</span>
                      <span class="pay-name">Virement</span>
                    </div>
                    <div 
                      class="pay-method-card" 
                      [class.selected]="selectedPaymentMode() === 2"
                      (click)="selectedPaymentMode.set(2)">
                      <span class="pay-icon">💵</span>
                      <span class="pay-name">Au comptoir</span>
                    </div>
                  </div>

                  <!-- Détails selon le mode de paiement choisi -->
                  @if (selectedPaymentMode() === 1) {
                    <div class="payment-details-form">
                      <div class="form-row">
                        <div class="form-group flex-1">
                          <label class="form-label" style="font-size: 0.8rem; text-align: left; display: block;">Titulaire de la carte</label>
                          <input type="text" [(ngModel)]="cardHolder" class="form-control" placeholder="Nom complet sur la carte" />
                        </div>
                      </div>
                      <div class="form-row" style="margin-top: 0.5rem;">
                        <div class="form-group flex-1">
                          <label class="form-label" style="font-size: 0.8rem; text-align: left; display: block;">Numéro de carte</label>
                          <input type="text" [(ngModel)]="cardNumber" class="form-control" placeholder="•••• •••• •••• ••••" maxlength="19" />
                        </div>
                        <div class="form-group" style="width: 100px;">
                          <label class="form-label" style="font-size: 0.8rem; text-align: left; display: block;">Expiration</label>
                          <input type="text" [(ngModel)]="cardExp" class="form-control" placeholder="MM/AA" maxlength="5" />
                        </div>
                        <div class="form-group" style="width: 80px;">
                          <label class="form-label" style="font-size: 0.8rem; text-align: left; display: block;">CVV</label>
                          <input type="password" [(ngModel)]="cardCvc" class="form-control" placeholder="•••" maxlength="4" />
                        </div>
                      </div>
                      <p class="secure-badge">🔒 Paiement sécurisé crypté SSL / TLS 256-bit via GPG Tunisie</p>
                    </div>
                  } @else if (selectedPaymentMode() === 3) {
                    <div class="bank-transfer-details">
                      <div class="transfer-info-card">
                        <div class="transfer-row">
                          <span class="label">Bénéficiaire :</span>
                          <strong>HOTELI TUNISIE S.A.</strong>
                        </div>
                        <div class="transfer-row">
                          <span class="label">Établissement bancaire :</span>
                          <strong>BIAT (Banque Internationale Arabe de Tunisie)</strong>
                        </div>
                        <div class="transfer-row">
                          <span class="label">RIB Tunisie (20 chiffres) :</span>
                          <strong class="highlight-rib">08 000 0001234567890 44</strong>
                        </div>
                        <div class="transfer-row">
                          <span class="label">Code SWIFT / BIC :</span>
                          <strong>BIATTNTT</strong>
                        </div>
                        <div class="transfer-row">
                          <span class="label">Motif obligatoire :</span>
                          <strong style="color: var(--forest-green);">RES-{{ confirmedReservation()?.id?.substring(0, 8) }}</strong>
                        </div>
                      </div>
                      <p class="transfer-note">
                        ℹ️ Merci d'indiquer votre référence de séjour dans le libellé de l'ordre de virement.
                      </p>
                    </div>
                  } @else if (selectedPaymentMode() === 2) {
                    <div class="counter-details">
                      <div class="counter-box">
                        <div class="counter-icon">🏨</div>
                        <div class="counter-text" style="text-align: left;">
                          <strong style="color: var(--forest-green);">Règlement à la réception lors du check-in</strong>
                          <p>Vous réglerez le montant de votre séjour ({{ calculatedTotal() }} DT) en espèces ou par carte bancaire lors de votre remise des clés à l'hôtel.</p>
                        </div>
                      </div>
                    </div>
                  }

                  <button class="btn btn-gold" [disabled]="isPaying()" (click)="processPayment()">
                    @if (isPaying()) {
                      <span>Enregistrement du paiement...</span>
                    } @else {
                      <span>Régler le montant total ({{ calculatedTotal() }} DT)</span>
                    }
                  </button>
                </div>

                <div class="calendar-export-wrap" style="margin-bottom: 2rem;">
                  <button type="button" class="btn btn-outline-gold" (click)="downloadIcs()">
                    📅 Ajouter à mon agenda (.ics)
                  </button>
                </div>

                <div class="final-links">
                  <a routerLink="/client/reservations" class="btn btn-outline">Voir mes réservations</a>
                  <a routerLink="/hotels" class="btn btn-ghost">Retour aux établissements</a>
                </div>
              </div>
            }
          </div>
        }
      </div>
    </div>
  `,
  styles: [`
    .wizard-container {
      background: radial-gradient(circle at center, #fbf9f4 0%, var(--ivory) 100%);
      min-height: calc(100vh - 78px - 250px);
    }

    .py-wizard {
      padding: 3rem 1.5rem 5rem;
    }

    .text-center {
      text-align: center;
    }

    .sub-badge {
      font-size: 0.75rem;
      letter-spacing: 0.2em;
      color: var(--secondary-green);
      font-weight: 700;
      display: inline-block;
      margin-bottom: 0.35rem;
    }

    .wizard-main-title {
      font-family: var(--font-serif);
      font-size: 2.35rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
    }

    .wizard-desc {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin-bottom: 2.5rem;
    }

    /* Stepper */
    .stepper-bar {
      display: flex;
      align-items: center;
      justify-content: center;
      max-width: 820px;
      margin: 0 auto 3rem;
    }

    .step-step {
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.4rem;
      position: relative;
    }

    .step-num {
      width: 36px;
      height: 36px;
      border-radius: 50%;
      background: var(--ivory-dark);
      color: var(--text-muted);
      display: flex;
      align-items: center;
      justify-content: center;
      font-weight: 700;
      font-size: 0.9rem;
      border: 2px solid var(--border-subtle);
      transition: all var(--transition-fast);
    }

    .step-label {
      font-size: 0.785rem;
      color: var(--text-muted);
      font-weight: 500;
      white-space: nowrap;
    }

    .step-step.active .step-num {
      background: var(--forest-green);
      color: var(--white);
      border-color: var(--forest-green);
    }

    .step-step.current .step-num {
      background: var(--soft-gold);
      color: var(--forest-green-dark);
      border-color: var(--soft-gold);
      box-shadow: 0 0 0 4px rgba(214, 183, 122, 0.25);
    }

    .step-step.active .step-label {
      color: var(--forest-green);
      font-weight: 600;
    }

    .step-line {
      flex: 1;
      height: 2px;
      background: var(--border-subtle);
      margin: 0 0.5rem 1.25rem;
    }

    .step-line.filled {
      background: var(--forest-green);
    }

    /* Panneau d'étape */
    .step-panel {
      max-width: 860px;
      margin: 0 auto;
    }

    .p-panel {
      padding: 2.5rem;
    }

    .panel-title {
      font-family: var(--font-serif);
      font-size: 1.6rem;
      color: var(--forest-green);
      margin-bottom: 1.5rem;
    }

    .panel-sub {
      color: var(--text-secondary);
      font-size: 0.9rem;
      margin-top: -1rem;
      margin-bottom: 1.5rem;
    }

    .panel-header-row {
      display: flex;
      justify-content: space-between;
      align-items: flex-start;
      margin-bottom: 1.5rem;
    }

    /* Grille Hôtels */
    .hotels-selection-grid {
      display: grid;
      grid-template-columns: repeat(auto-fill, minmax(240px, 1fr));
      gap: 1.5rem;
      margin-bottom: 2rem;
    }

    .hotel-select-card {
      border: 2px solid var(--border-subtle);
      border-radius: var(--radius-md);
      overflow: hidden;
      cursor: pointer;
      background: var(--white);
      transition: all var(--transition-fast);
    }

    .hotel-select-card:hover {
      border-color: var(--secondary-green);
      transform: translateY(-2px);
    }

    .hotel-select-card.selected {
      border-color: var(--soft-gold);
      box-shadow: 0 0 0 3px rgba(214, 183, 122, 0.35);
    }

    .h-img {
      width: 100%;
      height: 140px;
      object-fit: cover;
    }

    .h-info {
      padding: 1rem;
    }

    .h-city {
      font-size: 0.75rem;
      color: var(--secondary-green);
      font-weight: 600;
    }

    .h-name {
      font-family: var(--font-serif);
      font-size: 1.05rem;
      color: var(--forest-green);
      margin: 0.2rem 0;
    }

    .h-price {
      font-size: 0.85rem;
      color: var(--text-muted);
      font-weight: 500;
    }

    /* Liste Chambres */
    .rooms-selection-list {
      display: flex;
      flex-direction: column;
      gap: 1rem;
      margin-bottom: 2rem;
    }

    .room-select-item {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 1.25rem 1.5rem;
      border: 2px solid var(--border-subtle);
      border-radius: var(--radius-md);
      cursor: pointer;
      transition: all var(--transition-fast);
      background: var(--white);
    }

    .room-select-item:hover {
      border-color: var(--secondary-green);
    }

    .room-select-item.selected {
      border-color: var(--soft-gold);
      background: var(--ivory-light);
      box-shadow: 0 0 0 3px rgba(214, 183, 122, 0.35);
    }

    .room-numb {
      font-size: 0.8rem;
      color: var(--text-muted);
      display: block;
    }

    .room-title {
      font-family: var(--font-serif);
      font-size: 1.15rem;
      color: var(--forest-green);
      margin: 0.2rem 0;
    }

    .room-feature {
      font-size: 0.8rem;
      color: var(--text-secondary);
    }

    .room-tariff {
      text-align: right;
    }

    .rate {
      font-size: 1.35rem;
      font-weight: 700;
      color: var(--forest-green);
      display: block;
    }

    .rate-sub {
      font-size: 0.75rem;
      color: var(--text-muted);
    }

    /* Formulaire Dates */
    .dates-form {
      max-width: 650px;
      margin-bottom: 2rem;
    }

    .form-row {
      display: flex;
      gap: 1.5rem;
    }

    .flex-1 {
      flex: 1;
    }

    .duration-highlight {
      background: var(--secondary-green-subtle);
      border: 1px solid rgba(49, 91, 73, 0.2);
      padding: 1rem 1.25rem;
      border-radius: var(--radius-sm);
      display: flex;
      justify-content: space-between;
      color: var(--forest-green);
      font-size: 0.95rem;
      margin-top: 1rem;
    }

    /* Récapitulatif */
    .recap-grid {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1.5rem;
      margin-bottom: 2.5rem;
    }

    .recap-block {
      background: var(--ivory-light);
      border: 1px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      padding: 1.25rem;
    }

    .recap-block-title {
      font-size: 0.95rem;
      color: var(--forest-green);
      margin-bottom: 0.75rem;
      border-bottom: 1px solid var(--border-subtle);
      padding-bottom: 0.4rem;
    }

    .recap-line {
      font-size: 0.885rem;
      color: var(--text-secondary);
      margin-bottom: 0.4rem;
    }

    .highlight-total {
      background: #f7f3e8;
      border-color: var(--soft-gold-border);
    }

    .calc-row {
      display: flex;
      justify-content: space-between;
      font-size: 0.885rem;
      color: var(--text-secondary);
      margin-bottom: 0.35rem;
    }

    .total-row {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-top: 0.75rem;
      padding-top: 0.75rem;
      border-top: 1px solid var(--soft-gold-border);
      font-size: 1.05rem;
      font-weight: 700;
      color: var(--forest-green);
    }

    .total-val {
      font-size: 1.4rem;
      color: var(--forest-green);
    }

    /* Actions */
    .panel-actions {
      display: flex;
      justify-content: flex-end;
      gap: 1rem;
      border-top: 1px solid var(--border-subtle);
      padding-top: 1.5rem;
    }

    /* Étape Confirmation */
    .success-icon {
      width: 70px;
      height: 70px;
      border-radius: 50%;
      background: var(--color-success-bg);
      color: var(--color-success);
      font-size: 2.5rem;
      display: flex;
      align-items: center;
      justify-content: center;
      margin: 0 auto 1.5rem;
      border: 2px solid var(--color-success);
    }

    .success-sub {
      color: var(--text-secondary);
      font-size: 1.05rem;
      margin-bottom: 2rem;
    }

    .payment-action-box {
      max-width: 550px;
      margin: 0 auto 2.5rem;
      padding: 2rem;
    }

    .pay-title {
      font-family: var(--font-serif);
      font-size: 1.25rem;
      color: var(--forest-green);
      margin-bottom: 0.35rem;
    }

    .pay-desc {
      color: var(--text-secondary);
      font-size: 0.95rem;
      margin-bottom: 1.5rem;
    }

    .payment-methods-grid {
      display: flex;
      justify-content: center;
      gap: 1rem;
      margin-bottom: 1.75rem;
    }

    .pay-method-card {
      border: 2px solid var(--border-subtle);
      border-radius: var(--radius-sm);
      padding: 0.75rem 1rem;
      cursor: pointer;
      display: flex;
      flex-direction: column;
      align-items: center;
      gap: 0.35rem;
      min-width: 110px;
      transition: all var(--transition-fast);
    }

    .pay-method-card.selected {
      border-color: var(--soft-gold);
      background: var(--ivory-light);
    }

    .pay-icon {
      font-size: 1.5rem;
    }

    .pay-name {
      font-size: 0.785rem;
      font-weight: 600;
      color: var(--text-primary);
    }

    .payment-details-form, .bank-transfer-details, .counter-details {
      text-align: left;
      margin: 1.25rem 0 1.5rem;
      padding: 1.25rem;
      background: var(--ivory-light);
      border-radius: var(--radius-sm);
      border: 1px solid var(--border-subtle);
    }

    .secure-badge {
      font-size: 0.785rem;
      color: var(--forest-green);
      margin-top: 0.75rem;
      margin-bottom: 0;
      font-weight: 600;
      text-align: center;
    }

    .transfer-row {
      display: flex;
      justify-content: space-between;
      padding: 0.35rem 0;
      border-bottom: 1px dashed var(--border-subtle);
      font-size: 0.85rem;
    }

    .transfer-row:last-child {
      border-bottom: none;
    }

    .highlight-rib {
      font-family: monospace;
      font-size: 0.95rem;
      color: var(--forest-green);
    }

    .transfer-note {
      font-size: 0.8rem;
      color: var(--text-muted);
      margin-top: 0.75rem;
      margin-bottom: 0;
      text-align: center;
    }

    .counter-box {
      display: flex;
      gap: 1rem;
      align-items: center;
    }

    .counter-icon {
      font-size: 2rem;
    }

    .counter-text p {
      font-size: 0.85rem;
      color: var(--text-secondary);
      margin-top: 0.25rem;
      margin-bottom: 0;
    }

    .final-links {
      display: flex;
      justify-content: center;
      gap: 1rem;
    }

    @media (max-width: 768px) {
      .recap-grid {
        grid-template-columns: 1fr;
      }
      .form-row {
        flex-direction: column;
      }
    }
  `]
})
export class ReservationWizardComponent implements OnInit {
  private hotelService = inject(HotelService);
  private chambreService = inject(ChambreService);
  private reservationService = inject(ReservationService);
  private paiementService = inject(PaiementService);
  private authService = inject(AuthService);
  private notification = inject(NotificationService);
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  currentStep = signal<number>(1);
  isLoading = signal<boolean>(true);
  isSubmitting = signal<boolean>(false);
  isPaying = signal<boolean>(false);

  allHotels = signal<Hotel[]>([]);
  selectedHotel = signal<Hotel | null>(null);
  hotelRooms = signal<Chambre[]>([]);
  selectedRoom = signal<Chambre | null>(null);

  datesForm: FormGroup = this.fb.group({
    dateArrivee: ['', [Validators.required]],
    dateDepart: ['', [Validators.required]],
    nbPersonnes: [2, [Validators.required, Validators.min(1)]]
  });

  dateError = signal<string | null>(null);
  nbNuits = signal<number>(0);
  calculatedTotal = signal<number>(0);

  confirmedReservation = signal<Reservation | null>(null);
  selectedPaymentMode = signal<ModePaiement>(ModePaiement.Carte);

  // Coordonnées bancaires pour le paiement simulé sécurisé
  cardHolder: string = '';
  cardNumber: string = '';
  cardExp: string = '';
  cardCvc: string = '';

  ngOnInit(): void {
    if (!this.authService.isAuthenticated()) {
      this.notification.warning('Veuillez vous connecter pour effectuer une réservation.');
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: '/reservations/wizard' } });
      return;
    }

    // Pré-remplissage des dates par défaut (demain -> +3 jours)
    const tomorrow = new Date();
    tomorrow.setDate(tomorrow.getDate() + 1);
    const inThreeDays = new Date();
    inThreeDays.setDate(inThreeDays.getDate() + 4);

    this.datesForm.patchValue({
      dateArrivee: tomorrow.toISOString().substring(0, 10),
      dateDepart: inThreeDays.toISOString().substring(0, 10),
      nbPersonnes: 2
    });

    this.hotelService.getHotels().subscribe({
      next: (hotels) => {
        this.allHotels.set(hotels);
        this.isLoading.set(false);

        // Si hotelId et chambreId passés en queryParams
        const hotelId = this.route.snapshot.queryParams['hotelId'];
        const chambreId = this.route.snapshot.queryParams['chambreId'];

        if (hotelId) {
          const h = hotels.find(x => x.id === hotelId);
          if (h) {
            this.onSelectHotel(h);
            if (chambreId) {
              setTimeout(() => {
                const room = this.hotelRooms().find(r => r.id === chambreId);
                if (room) {
                  this.onSelectRoom(room);
                  this.currentStep.set(3);
                }
              }, 300);
            }
          }
        }
      },
      error: () => this.isLoading.set(false)
    });

    this.calculateTotal();
  }

  onSelectHotel(hotel: Hotel): void {
    this.selectedHotel.set(hotel);
    this.chambreService.getChambresByHotel(hotel.id).subscribe({
      next: (rooms) => {
        this.hotelRooms.set(rooms);
      }
    });
  }

  onSelectRoom(room: Chambre): void {
    this.selectedRoom.set(room);
    this.calculateTotal();
  }

  calculateTotal(): void {
    const arr = this.datesForm.value.dateArrivee;
    const dep = this.datesForm.value.dateDepart;

    if (!arr || !dep) {
      this.dateError.set(null);
      this.nbNuits.set(0);
      this.calculatedTotal.set(0);
      return;
    }

    const dArr = new Date(arr);
    const dDep = new Date(dep);

    if (dDep <= dArr) {
      this.dateError.set("La date de départ doit être strictement postérieure à la date d'arrivée.");
      this.nbNuits.set(0);
      this.calculatedTotal.set(0);
      return;
    }

    this.dateError.set(null);
    const diffTime = Math.abs(dDep.getTime() - dArr.getTime());
    const nights = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
    this.nbNuits.set(nights);

    const room = this.selectedRoom();
    if (room) {
      this.calculatedTotal.set(room.prixMontant * nights);
    }
  }

  goToStep(step: number): void {
    if (step === 4) {
      this.calculateTotal();
    }
    this.currentStep.set(step);
    window.scrollTo({ top: 0, behavior: 'smooth' });
  }

  submitReservation(): void {
    if (this.isSubmitting()) return;
    const room = this.selectedRoom();
    const user = this.authService.currentUser();
    if (!room || !user?.utilisateurId) return;

    this.isSubmitting.set(true);

    const payload = {
      chambreId: room.id,
      clientId: user.utilisateurId,
      dateArrivee: new Date(this.datesForm.value.dateArrivee).toISOString(),
      dateDepart: new Date(this.datesForm.value.dateDepart).toISOString(),
      nbPersonnes: Number(this.datesForm.value.nbPersonnes)
    };

    this.reservationService.createReservation(payload).subscribe({
      next: (res) => {
        this.isSubmitting.set(false);
        this.confirmedReservation.set(res);
        this.currentStep.set(5);
        this.notification.success('Votre réservation a été créée avec succès !');
      },
      error: (err) => {
        this.isSubmitting.set(false);
        this.notification.error(err.error?.message || 'Impossible de créer la réservation. Veuillez vérifier la disponibilité de la chambre.');
      }
    });
  }

  downloadIcs(): void {
    const res = this.confirmedReservation();
    const hotel = this.selectedHotel();
    const room = this.selectedRoom();
    const dates = this.datesForm.value;
    if (!res || !dates.dateArrivee || !dates.dateDepart) return;

    const formatDate = (dateStr: string) => dateStr.replace(/-/g, '');
    const dtStart = formatDate(dates.dateArrivee);
    const dtEnd = formatDate(dates.dateDepart);
    const now = new Date().toISOString().replace(/[-:]/g, '').split('.')[0] + 'Z';
    const ref = res.id.substring(0, 8);
    const hotelName = hotel?.nom || 'Établissement';
    const location = `${hotel?.adresse || ''}, ${hotel?.ville || ''}, Tunisie`;

    const icsContent = [
      'BEGIN:VCALENDAR',
      'VERSION:2.0',
      'PRODID:-//HOTELI//Reservation//FR',
      'CALSCALE:GREGORIAN',
      'METHOD:PUBLISH',
      'BEGIN:VEVENT',
      `UID:hoteli-${res.id}@hoteli.tn`,
      `DTSTAMP:${now}`,
      `DTSTART;VALUE=DATE:${dtStart}`,
      `DTEND;VALUE=DATE:${dtEnd}`,
      `SUMMARY:Séjour HOTELI — ${hotelName}`,
      `DESCRIPTION:Réservation HOTELI #${ref}\\nÉtablissement: ${hotelName}\\nChambre: ${room?.numero || ''}\\nMontant: ${this.calculatedTotal()} DT`,
      `LOCATION:${location}`,
      'STATUS:CONFIRMED',
      'END:VEVENT',
      'END:VCALENDAR'
    ].join('\r\n');

    const blob = new Blob([icsContent], { type: 'text/calendar;charset=utf-8;' });
    const link = document.createElement('a');
    link.href = URL.createObjectURL(blob);
    link.setAttribute('download', `reservation-hoteli-${ref}.ics`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
    this.notification.success('Votre séjour a été exporté vers votre agenda (.ics) !');
  }

  processPayment(): void {
    if (this.isPaying()) return;
    const res = this.confirmedReservation();
    if (!res) return;

    this.isPaying.set(true);

    const payload = {
      reservationId: res.id,
      montant: this.calculatedTotal(),
      devise: 'TND',
      modePaiement: this.selectedPaymentMode(),
      reference: `PAY-${Date.now().toString().slice(-6)}`
    };

    this.paiementService.createPaiement(payload).subscribe({
      next: () => {
        this.isPaying.set(false);
        this.notification.success('Paiement enregistré avec succès ! Facture disponible.');
        this.router.navigate(['/client/payments']);
      },
      error: (err) => {
        this.isPaying.set(false);
        this.notification.error(err.error?.message || 'Erreur lors du traitement du paiement.');
      }
    });
  }
}
