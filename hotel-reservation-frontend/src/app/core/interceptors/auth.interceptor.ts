import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { TokenStorage } from '../auth/token.storage';
import { AuthService } from '../auth/auth.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const tokenStorage = inject(TokenStorage);
  const authService = inject(AuthService);
  const token = tokenStorage.getAccessToken();

  let authReq = req;
  if (token && !req.url.includes('/auth/connexion') && !req.url.includes('/auth/inscription')) {
    authReq = req.clone({
      setHeaders: {
        Authorization: `Bearer ${token}`
      }
    });
  }

  return next(authReq).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && !req.url.includes('/auth/connexion') && !req.url.includes('/auth/rafraichir-token')) {
        const refreshToken = tokenStorage.getRefreshToken();
        if (refreshToken) {
          return authService.refreshToken().pipe(
            switchMap(res => {
              const retryReq = req.clone({
                setHeaders: {
                  Authorization: `Bearer ${res.jwt}`
                }
              });
              return next(retryReq);
            }),
            catchError(refreshErr => {
              authService.logout();
              return throwError(() => refreshErr);
            })
          );
        } else {
          authService.logout();
        }
      }
      return throwError(() => error);
    })
  );
};
