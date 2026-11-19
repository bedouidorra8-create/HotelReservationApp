import { Pipe, PipeTransform } from '@angular/core';
import { StatutReservation, StatutPaiement, StatutChambre, StatutAvis, StatutUtilisateur } from '../../core/models';

@Pipe({
  name: 'statusBadge',
  standalone: true
})
export class StatusBadgePipe implements PipeTransform {
  transform(value: any, type: 'reservation' | 'paiement' | 'chambre' | 'avis' | 'utilisateur'): { label: string; cssClass: string } {
    switch (type) {
      case 'reservation':
        switch (value) {
          case StatutReservation.EnAttente:
          case 1:
            return { label: 'En attente', cssClass: 'badge-warning' };
          case StatutReservation.Confirmee:
          case 2:
            return { label: 'Confirmée', cssClass: 'badge-success' };
          case StatutReservation.Annulee:
          case 3:
            return { label: 'Annulée', cssClass: 'badge-danger' };
          case StatutReservation.Terminee:
          case 4:
            return { label: 'Terminée', cssClass: 'badge-info' };
          default:
            return { label: 'Inconnu', cssClass: 'badge-info' };
        }

      case 'paiement':
        switch (value) {
          case StatutPaiement.EnAttente:
          case 1:
            return { label: 'En attente', cssClass: 'badge-warning' };
          case StatutPaiement.Paye:
          case 2:
            return { label: 'Payé', cssClass: 'badge-success' };
          case StatutPaiement.Rembourse:
          case 3:
            return { label: 'Remboursé', cssClass: 'badge-info' };
          case StatutPaiement.Echoue:
          case 4:
            return { label: 'Échoué', cssClass: 'badge-danger' };
          case StatutPaiement.Annule:
          case 5:
            return { label: 'Annulé', cssClass: 'badge-danger' };
          default:
            return { label: 'Inconnu', cssClass: 'badge-info' };
        }

      case 'chambre':
        switch (value) {
          case StatutChambre.Libre:
          case 1:
            return { label: 'Disponible', cssClass: 'badge-success' };
          case StatutChambre.Occupee:
          case 2:
            return { label: 'Occupée', cssClass: 'badge-warning' };
          case StatutChambre.Maintenance:
          case 3:
            return { label: 'Maintenance', cssClass: 'badge-danger' };
          default:
            return { label: 'Non défini', cssClass: 'badge-info' };
        }

      case 'avis':
        switch (value) {
          case StatutAvis.EnAttente:
          case 1:
            return { label: 'En modération', cssClass: 'badge-warning' };
          case StatutAvis.Valide:
          case 2:
            return { label: 'Validé', cssClass: 'badge-success' };
          case StatutAvis.Rejete:
          case 3:
            return { label: 'Rejeté', cssClass: 'badge-danger' };
          default:
            return { label: 'Inconnu', cssClass: 'badge-info' };
        }

      case 'utilisateur':
        switch (value) {
          case StatutUtilisateur.Actif:
          case 1:
            return { label: 'Actif', cssClass: 'badge-success' };
          case StatutUtilisateur.Desactive:
          case 2:
            return { label: 'Désactivé', cssClass: 'badge-danger' };
          default:
            return { label: 'Inconnu', cssClass: 'badge-info' };
        }

      default:
        return { label: 'Inconnu', cssClass: 'badge-info' };
    }
  }
}
