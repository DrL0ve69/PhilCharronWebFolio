import { InjectionToken } from '@angular/core';

export interface Environment {
  apiUrl: string;
  production: boolean;
}

export const ENVIRONMENT = new InjectionToken<Environment>('environment');
