import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, switchMap, throwError } from 'rxjs';
import { IdentityService } from '../services/identity.service';

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(IdentityService);
  const token = auth.getToken();

  if (token) req = req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });

  return next(req).pipe(
    catchError((error: HttpErrorResponse) => {
      if (error.status === 401 && token) {
        return auth.refresh().pipe(
          switchMap(() => {
            const newToken = auth.getToken();
            req = req.clone({ setHeaders: { Authorization: `Bearer ${newToken}` } });
            return next(req);
          }),
          catchError((refreshError) => {
            auth.logout();
            return throwError(() => refreshError);
          })
        );
      }
      return throwError(() => error);
    })
  );
};