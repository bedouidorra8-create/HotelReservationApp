import { Pipe, PipeTransform } from '@angular/core';
import { TypeChambre } from '../../core/models';

@Pipe({
  name: 'roomType',
  standalone: true
})
export class RoomTypePipe implements PipeTransform {
  transform(value: TypeChambre | number | undefined): string {
    switch (value) {
      case TypeChambre.Simple:
      case 1:
        return 'Chambre Simple';
      case TypeChambre.Double:
      case 2:
        return 'Chambre Double';
      case TypeChambre.Suite:
      case 3:
        return 'Suite Prestige';
      case TypeChambre.Familiale:
      case 4:
        return 'Chambre Familiale';
      default:
        return 'Chambre Standard';
    }
  }
}
