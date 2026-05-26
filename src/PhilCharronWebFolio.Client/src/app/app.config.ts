// ajout du interceptor pour injecter le token dans les requêtes HTTP et du withFetch pour utiliser fetch au lieu de XMLHttpRequest

import { ApplicationConfig, provideBrowserGlobalErrorListeners } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideHttpClient, withInterceptors, withFetch } from '@angular/common/http';
import { ENVIRONMENT } from './core/tokens/environment.token';
import { environment } from '../environments/environment';
import { authInterceptor } from './core/auth/auth.interceptor';

import { routes } from './app.routes';
import { provideClientHydration, withEventReplay } from '@angular/platform-browser';

export const appConfig: ApplicationConfig = {
  providers: [
    provideBrowserGlobalErrorListeners(),
    provideRouter(routes),
    provideClientHydration(withEventReplay()),
    provideHttpClient(withInterceptors([authInterceptor]), withFetch()),
    { provide: ENVIRONMENT, useValue: environment }
  ]
};
