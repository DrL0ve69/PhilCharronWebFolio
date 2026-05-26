import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { ProfileCardComponent, ProfileCardProps } from '../../shared/components/profile-card/profile-card.component';
import { ProfileDto } from '../../core/services/auth.models';

@Component({
  selector: 'app-personal-profile',
  standalone: true,
  imports: [CommonModule, ProfileCardComponent],
  templateUrl: './personal-profile.component.html',
  styleUrl: './personal-profile.component.scss',
})
export class PersonalProfileComponent implements OnInit {
  readonly authService = inject(AuthService);

  profile = signal<ProfileDto | null>(null);
  isLoading = signal(true);
  error = signal<string | null>(null);

  ngOnInit() {
    this.authService.getProfile().subscribe({
      next: (data) => {
        this.profile.set(data);
        this.isLoading.set(false);
      },
      error: (err) => {
        this.error.set('Impossible de charger le profil.');
        this.isLoading.set(false);
      }
    });
  }

  get profileData(): ProfileCardProps | null {
    const p = this.profile();
    if (!p) return {userName: '', email: '', fullName: ''}; // Ou null selon ce que tu préfères
    return {
      userName: p.userName,
      email: p.email,
      fullName: p.fullName
    };
  }
}
