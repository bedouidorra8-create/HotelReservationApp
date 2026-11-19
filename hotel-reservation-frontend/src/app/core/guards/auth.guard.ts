import { inject } from '@angular/core';
import { CanActivateFn, Router } from '@angular/router';
import { AuthService } from '../auth/auth.service';

export const authGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (authService.isAuthenticated()) {
    return true;
  }

  router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
  return false;
};

export const roleGuard: CanActivateFn = (route, state) => {
  const authService = inject(AuthService);
  const router = inject(Router);

  if (!authService.isAuthenticated()) {
    router.navigate(['/auth/login'], { queryParams: { returnUrl: state.url } });
    return false;
  }

  const expectedRoles = route.data?.['roles'] as string[] | undefined;
  const currentRole = authService.userRole();

  if (!expectedRoles || (currentRole && expectedRoles.includes(currentRole))) {
    return true;
  }

  // Si non autorisé pour ce rôle, redirection vers son espace approprié
  if (currentRole === 'Client') {
    router.navigate(['/client/dashboard']);
  } else {
    router.navigate(['/admin/dashboard']);
  }
  return false;
};
