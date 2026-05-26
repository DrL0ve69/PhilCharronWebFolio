import { Injectable, signal } from '@angular/core';

export type Theme = 'light' | 'dark' | 'arcade';

@Injectable({
  providedIn: 'root',
})
export class ThemeService {
  currentTheme = signal<Theme>('light');

  setTheme(theme: Theme) {
    this.currentTheme.set(theme);
    document.documentElement.setAttribute('data-theme', theme);
  }

  toggleTheme() {
    const nextTheme: Theme = this.currentTheme() === 'light' ? 'dark' : 'light';
    this.setTheme(nextTheme);
  }
}
