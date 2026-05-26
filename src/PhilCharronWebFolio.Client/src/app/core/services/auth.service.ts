import { Injectable, inject, signal, computed, PLATFORM_ID } from '@angular/core';
import { isPlatformBrowser } from '@angular/common';
import { HttpClient, HttpErrorResponse } from '@angular/common/http';
import { ENVIRONMENT } from '../tokens/environment.token';
import { AuthResponseDto, ProfileDto } from './auth.models';
import { catchError, Observable, tap, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root',
})
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly env = inject(ENVIRONMENT);
  private readonly platformId = inject(PLATFORM_ID); // 1. Injecter l'ID de la plateforme
  
  private readonly apiUrl = `${this.env.apiUrl}/api/v1/auth`;

  // 2. Initialiser le signal à null par défaut pour le serveur
  private tokenSignal = signal<string | null>(null);

  // Computed signals
  public isAuthenticated = computed(() => this.tokenSignal() !== null);
  public currentToken = computed(() => this.tokenSignal());

  constructor() {
    // 3. Récupérer le token uniquement si on est dans le navigateur de l'utilisateur
    if (isPlatformBrowser(this.platformId)) {
      const savedToken = localStorage.getItem('jwt_token');
      this.tokenSignal.set(savedToken);
    }
  }

  login(loginOrEmail: string, password: string): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/login`, { loginOrEmail, password }).pipe(
      // 4. Stocker le token en cas de succès
      tap(response => this.setToken(response.token)),
      catchError(this.handleError)
    );
  }

  register(data: RegisterRequest): Observable<AuthResponseDto> {
    return this.http.post<AuthResponseDto>(`${this.apiUrl}/register`, data).pipe(
      tap(response => this.setToken(response.token)),
      catchError(this.handleError)
    );
  }

  getProfile(): Observable<ProfileDto> {
    return this.http.get<ProfileDto>(`${this.apiUrl}/profile`).pipe(
      catchError(this.handleError)
    );
  }

  logout() {
    this.setToken(null);
  }

  private setToken(token: string | null) {
    this.tokenSignal.set(token);
    // 5. Sécuriser l'écriture dans le localStorage
    if (isPlatformBrowser(this.platformId)) {
      if (token) {
        localStorage.setItem('jwt_token', token);
      } else {
        localStorage.removeItem('jwt_token');
      }
    }
  }

  private handleError(error: unknown): Observable<never> {
  if (error instanceof HttpErrorResponse) {
    console.error(`Backend returned code ${error.status}, body was: `, error.error);
  } else {
    console.error('An error occurred:', error);
  }
  return throwError(() => new Error('Une erreur technique est survenue, veuillez réessayer plus tard.'));
  }
}

export interface RegisterRequest {
  firstName: string;
  lastName: string;
  userName: string;
  email: string;
  password: string;
}
