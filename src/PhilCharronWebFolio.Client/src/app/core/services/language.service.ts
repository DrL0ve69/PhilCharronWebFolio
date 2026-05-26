import { Injectable, signal } from '@angular/core';

export type Language = 'en' | 'fr';

@Injectable({
  providedIn: 'root',
})
export class LanguageService {
  currentLang = signal<Language>('fr');

  setLanguage(lang: Language) {
    this.currentLang.set(lang);
    // Integration with @angular/localize would happen here
    document.documentElement.lang = lang;
  }

  toggleLanguage() {
    const nextLang: Language = this.currentLang() === 'fr' ? 'en' : 'fr';
    this.setLanguage(nextLang);
  }
}
