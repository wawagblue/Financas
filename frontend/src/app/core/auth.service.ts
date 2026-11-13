import { HttpClient } from '@angular/common/http';
import { Injectable, inject, signal } from '@angular/core';
import { Observable, tap } from 'rxjs';
import { TokenResponse, Usuario } from './api.models';

const AUTH = '/gateway/auth/api/v1';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  readonly session = signal<TokenResponse | null>(null);
  readonly user = signal<Usuario | null>(null);

  get accessToken(): string | null {
    return this.session()?.accessToken ?? null;
  }

  register(nome: string, email: string, password: string): Observable<Usuario> {
    return this.http.post<Usuario>(`${AUTH}/users`, { nome, email, password });
  }

  login(email: string, password: string): Observable<TokenResponse> {
    return this.http.post<TokenResponse>(`${AUTH}/auth/login`, { email, password }).pipe(
      tap((tokens) => this.session.set(tokens)),
    );
  }

  loadProfile(): Observable<Usuario> {
    return this.http.get<Usuario>(`${AUTH}/users/me`).pipe(tap((profile) => this.user.set(profile)));
  }

  refresh(): Observable<TokenResponse> {
    const refreshToken = this.session()?.refreshToken;
    if (!refreshToken) throw new Error('Não há sessão para renovar.');

    return this.http.post<TokenResponse>(`${AUTH}/auth/refresh-token`, { refreshToken }).pipe(
      tap((tokens) => this.session.set(tokens)),
    );
  }

  logout(): Observable<void> {
    const refreshToken = this.session()?.refreshToken;
    if (!refreshToken) {
      this.clearSession();
      return new Observable<void>((subscriber) => subscriber.complete());
    }

    return this.http.post<void>(`${AUTH}/auth/logout`, { refreshToken }).pipe(
      tap(() => this.clearSession()),
    );
  }

  clearSession(): void {
    this.session.set(null);
    this.user.set(null);
  }
}
