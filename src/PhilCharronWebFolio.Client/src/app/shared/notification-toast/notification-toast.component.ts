// src/shared/components/notification-toast/notification-toast.component.ts
import { Component, inject, ChangeDetectionStrategy } from '@angular/core';
import { NotificationService } from '../../core/services/notification.service';

@Component({
  selector: 'app-notification-toast',
  standalone: true,
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <div class="toast-container" aria-live="polite">
      @for (n of service.currentNotifications(); track n.id) {
        <div [class]="'toast ' + n.type" (click)="service.remove(n.id)">
          {{ n.message }}
        </div>
      }
    </div>
  `,
  styles: [`
    .toast-container { position: fixed; top: 1rem; right: 1rem; z-index: 1000; }
    .toast { padding: 1rem; margin-bottom: 0.5rem; border-radius: 4px; color: white; cursor: pointer; }
    .success { background: #28a745; }
    .error { background: #dc3545; }
    .warning { background: #ffc107; color: #000; }
  `]
})
export class NotificationToastComponent {
  protected readonly service = inject(NotificationService);
}