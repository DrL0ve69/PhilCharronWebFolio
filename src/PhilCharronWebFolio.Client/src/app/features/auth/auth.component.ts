import { Component, signal, computed, ChangeDetectionStrategy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ReactiveFormsModule, FormBuilder, Validators } from '@angular/forms';
import { AuthService } from '../../core/services/auth.service';
import { NotificationService } from '../../core/services/notification.service';
import { FormErrorComponent } from '../../shared/form-error/form-error.component';
import { inject } from '@angular/core';
import { Router } from '@angular/router';

@Component({
  selector: 'app-auth',
  imports: [CommonModule, ReactiveFormsModule, FormErrorComponent],
  changeDetection: ChangeDetectionStrategy.OnPush,
  templateUrl: 'auth.component.html',
  styleUrls: ['auth.component.scss']
})
export class AuthComponent {
  private fb = inject(FormBuilder).nonNullable; // ajout du nonNullable pour éviter les nulls dans les formulaires
  private authService = inject(AuthService);
  private router = inject(Router);
  private notificationService = inject(NotificationService);

  // Signals pour l'état de l'UI
  /*
  isLoginMode = signal(true);
  isLoading = signal(false);
  errorMessage = signal<string | null>(null);
  */
  isFlipped = signal(false);
  isLoading = signal(false);

  loginForm = this.fb.group({
    login: ['', Validators.required],
    password: ['', Validators.required],
  });

  registerForm = this.fb.group({
    firstName: ['', Validators.required],
    lastName: ['', Validators.required],
    userName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(8)]],
  });

  toggleFlip() {
    this.isFlipped.update(v => !v);
  }

  /*
  toggleMode() {
    this.isLoginMode.update(v => !v);
    this.errorMessage.set(null);
    this.authForm.reset();
  }
  */

  onLogin() {
    if (this.loginForm.invalid) return;
    this.isLoading.set(true);
    const { login, password } = this.loginForm.value;

    this.authService.login(login!, password!).subscribe({
      next: () => this.router.navigate(['/profile']),
      error: (err) => {
        this.isLoading.set(false);
        this.notificationService.show('Erreur lors de la connexion.', err.error?.message || 'Login failed');
      }
    });
  }

  onRegister() {
    if (this.registerForm.invalid) {
    this.notificationService.show('Veuillez corriger les erreurs du formulaire.', 'warning');
    return;
    }

    const data = this.registerForm.value;
    this.authService.register({
      firstName: data.firstName!,
      lastName: data.lastName!,
      userName: data.userName!,
      email: data.email!,
      password: data.password!,
    }).subscribe({
      next: () => {this.router.navigate(['/profile']); this.notificationService.show('Inscription réussie !');},
      error: (err) => this.notificationService.show('Erreur lors de l\'inscription.', err.error?.message || 'Registration failed')
    });
  }

  /*
  onSubmit() {
    if (this.authForm.invalid) return;

    this.isLoading.set(true);
    this.errorMessage.set(null);

    const formValues = this.authForm.getRawValue();

    const request$ = this.isLoginMode() 
      ? this.authService.login({ email: formValues.email, password: formValues.password })
      : this.authService.register(formValues);

    request$.subscribe({
      next: () => {
        this.isLoading.set(false);
        if (this.isLoginMode()) {
          this.router.navigate(['/']); // Redirection vers l'accueil
        } else {
          // Si inscription réussie, on bascule direct sur la connexion
          this.toggleMode();
          this.errorMessage.set('Inscription réussie ! Vous pouvez vous connecter.');
        }
      },
      error: (err) => {
        this.isLoading.set(false);
        this.errorMessage.set(err.error?.title || 'Une erreur est survenue.');
      }
    });
  }
    */
}
