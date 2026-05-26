import { Component, input } from '@angular/core';
import { CommonModule } from '@angular/common';

export interface ProfileCardProps {
  userName: string;
  email: string;
  fullName: string;
}

@Component({
  selector: 'app-profile-card',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './profile-card.component.html',
  styleUrl: './profile-card.component.scss'
})
export class ProfileCardComponent {
  user = input.required<ProfileCardProps>();
}
