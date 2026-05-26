import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { AuthComponent } from './features/auth/auth.component';
import { PersonalProfileComponent } from './features/profile/personal-profile.component';

export const routes: Routes =
[
    { 
    path: '', 
    loadComponent: () => import('./features/home/home.component').then(m => m.HomeComponent) 
    },
    { path: 'auth', component: AuthComponent },
    { path: 'profile', component: PersonalProfileComponent },
    {
        path: '**',
        redirectTo: '',
    },
];
