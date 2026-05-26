import { Component, ChangeDetectionStrategy, inject } from '@angular/core';
import { RouterLink, RouterLinkActive, Router } from '@angular/router';
import { ThemeService, Theme } from '../../../core/services/theme.service';
import { LanguageService } from '../../../core/services/language.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-nav',
  // 1. On retire CommonModule, il n'est plus nécessaire avec le nouveau control flow
  imports: [RouterLink, RouterLinkActive], 
  templateUrl: './nav.component.html',
  styleUrl: './nav.component.scss',
  // 2. On garde OnPush pour la performance
  changeDetection: ChangeDetectionStrategy.OnPush 
})
export class NavComponent {
  readonly router = inject(Router);
  readonly themeService = inject(ThemeService);
  readonly langService = inject(LanguageService);
  readonly authService = inject(AuthService);

  // 3. On expose directement les signaux existants
  readonly currentTheme = this.themeService.currentTheme;
  readonly currentLang = this.langService.currentLang;
  
  // Plus besoin de `isLoggedIn = signal(false)`, on utilise le computed() du service !
  readonly isAuthenticated = this.authService.isAuthenticated;

  // Plus de constructor ni de checkAuth() ! 
  // C'est l'AuthService qui gère le localStorage de manière sécurisée (SSR).

  setTheme(theme: Theme): void {
    this.themeService.setTheme(theme);
  }

  setLang(lang: 'en' | 'fr'): void {
    this.langService.setLanguage(lang);
  }

  logout(): void {
    // 4. On délègue la logique de déconnexion au service
    this.authService.logout(); 
    this.router.navigate(['/auth']);
  }
}
