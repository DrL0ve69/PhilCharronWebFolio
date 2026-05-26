// src/core/services/notification.service.ts
import { Injectable, signal } from '@angular/core';

export type NotificationType = 'success' | 'error' | 'warning';

export interface AppNotification {
  id: number;
  message: string;
  type: NotificationType;
}

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private notifications = signal<AppNotification[]>([]);
  readonly currentNotifications = this.notifications.asReadonly();

  show(message: string, type: NotificationType = 'success') {
    const id = Date.now();
    this.notifications.update(list => [...list, { id, message, type }]);
    
    // Auto-suppression après 5 secondes
    setTimeout(() => this.remove(id), 5000);
  }

  remove(id: number) {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }
}