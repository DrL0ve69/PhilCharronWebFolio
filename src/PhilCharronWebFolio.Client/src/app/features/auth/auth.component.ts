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
  template: `
    <div class="auth-container">
      <div class="auth-card" [class.flipped]="isFlipped()">

        <!-- Login Face -->
        <div class="auth-face auth-face--front">
          <h2>Welcome Back</h2>
          <form [formGroup]="loginForm" (ngSubmit)="onLogin()">
            <div class="form-group">
              <label for="login">Email or Username</label>
              <input id="login" formControlName="login" type="text" placeholder="Enter your credentials">
            </div>
            <div class="form-group">
              <label for="password">Password</label>
              <input id="password" formControlName="password" type="password" placeholder="********">
            </div>
            <button type="submit" [disabled]="loginForm.invalid" class="btn-primary">Login</button>
          </form>
          <p class="auth-switch">
            Don't have an account?
            <a href="javascript:void(0)" (click)="toggleFlip()">Register here</a>
          </p>
        </div>

        <!-- Register Face -->
        <div class="auth-face auth-face--back">
          <h2>Create Account</h2>
          <form [formGroup]="registerForm" (ngSubmit)="onRegister()">
            <div class="form-row">
              <div class="form-group">
                <label for="firstName">First Name</label>
                <input id="firstName" formControlName="firstName" type="text">
              </div>
              <div class="form-group">
                <label for="lastName">Last Name</label>
                <input id="lastName" formControlName="lastName" type="text">
              </div>
            </div>
            <div class="form-group">
              <label for="userName">Username</label>
              <input id="userName" formControlName="userName" type="text">
            </div>
            <div class="form-group">
              <label for="email">Email</label>
              <input id="email" formControlName="email" type="email">
              <app-form-error [control]="registerForm.get('email')" />
            </div>
            <div class="form-group">
              <label for="regPassword">Password</label>
              <input id="regPassword" formControlName="password" type="password">
            </div>
            <button type="submit" [disabled]="registerForm.invalid" class="btn-primary">Register</button>
          </form>
          <p class="auth-switch">
            Already have an account?
            <a href="javascript:void(0)" (click)="toggleFlip()">Login here</a>
          </p>
        </div>

      </div>
    </div>
  `,
  styles: `
    .auth-container {
      display: flex;
      justify-content: center;
      align-items: center;
      min-height: 80vh;
      perspective: 1000px;
    }

    .auth-card {
      position: relative;
      width: 400px;
      height: 550px;
      transition: transform 0.6s cubic-bezier(0.4, 0, 0.2, 1);
      transform-style: preserve-3d;
    }

    .auth-card.flipped {
      transform: rotateY(180deg);
    }

    .auth-face {
      position: absolute;
      width: 100%;
      height: 100%;
      backface-visibility: hidden;
      background: var(--card-bg, #fff);
      padding: 2rem;
      border-radius: 1rem;
      box-shadow: 0 10px 25px rgba(0,0,0,0.1);
      display: flex;
      flex-direction: column;
      gap: 1.5rem;
    }

    .auth-face--back {
      transform: rotateY(180deg);
    }

    h2 {
      text-align: center;
      margin-bottom: 1rem;
      color: var(--primary-color, #333);
    }

    .form-group {
      display: flex;
      flex-direction: column;
      gap: 0.5rem;
    }

    .form-row {
      display: grid;
      grid-template-columns: 1fr 1fr;
      gap: 1rem;
    }

    label {
      font-size: 0.875rem;
      font-weight: 500;
    }

    input {
      padding: 0.75rem;
      border: 1px solid #ddd;
      border-radius: 0.5rem;
      font-size: 1rem;
    }

    .btn-primary {
      padding: 0.75rem;
      background: var(--primary-color, #007bff);
      color: white;
      border: none;
      border-radius: 0.5rem;
      cursor: pointer;
      font-weight: 600;
      transition: opacity 0.2s;
    }

    .btn-primary:disabled {
      opacity: 0.5;
      cursor: not-allowed;
    }

    .auth-switch {
      text-align: center;
      margin-top: 1rem;
      font-size: 0.875rem;
    }

    .auth-switch a {
      color: var(--primary-color, #007bff);
      text-decoration: none;
      font-weight: 600;
    }
  `
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
