// src/shared/components/form-error/form-error.component.ts
import { Component, Input, ChangeDetectionStrategy } from '@angular/core';
import { AbstractControl } from '@angular/forms';

@Component({
  selector: 'app-form-error',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    @if (control?.invalid && (control?.touched || control?.dirty)) {
      <span class="error-message" role="alert">
        {{ getErrorMessage() }}
      </span>
    }
  `,
  styles: [`.error-message { color: #dc3545; font-size: 0.8rem; margin-top: 0.25rem; display: block; }`]
})
export class FormErrorComponent {
  @Input({ required: true }) control: AbstractControl | null = null;

  getErrorMessage(): string {
    if (!this.control?.errors) return '';
    if (this.control.hasError('required')) return 'Ce champ est requis.';
    if (this.control.hasError('email')) return 'Adresse email invalide.';
    if (this.control.hasError('minlength')) return 'Trop court.';
    return 'Champ invalide.';
  }
}