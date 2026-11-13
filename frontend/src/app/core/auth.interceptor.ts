import { HttpErrorResponse, HttpInterceptorFn } from '@angular/common/http';
import { inject } from '@angular/core';
import { catchError, finalize, shareReplay, switchMap, throwError } from 'rxjs';
import { AuthService } from './auth.service';

let refreshInFlight: ReturnType<AuthService['refresh']> | null = null;

export const authInterceptor: HttpInterceptorFn = (request, next) => {
  const auth = inject(AuthService);
  const token = auth.accessToken;
  const authenticatedRequest = token
    ? request.clone({ setHeaders: { Authorization: `Bearer ${token}` } })
    : request;
  const isAuthFlow = /\/auth\/(login|refresh-token)|\/users$/.test(request.url);

  return next(authenticatedRequest).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthFlow || !auth.session()?.refreshToken) {
        return throwError(() => error);
      }

      refreshInFlight ??= auth.refresh().pipe(
        shareReplay({ bufferSize: 1, refCount: false }),
        finalize(() => (refreshInFlight = null)),
      );

      return refreshInFlight.pipe(
        switchMap((tokens) => next(request.clone({ setHeaders: { Authorization: `Bearer ${tokens.accessToken}` } }))),
        catchError((refreshError: unknown) => {
          auth.clearSession();
          return throwError(() => refreshError);
        }),
      );
    }),
  );
};
