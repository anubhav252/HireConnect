import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Notification } from '../../models/notification.models';
import { interval, switchMap } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class NotificationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/notifications`;

  getNotifications(userId: number) {
    return this.http.get<Notification[]>(`${this.apiUrl}/user/${userId}`);
  }

  markAsRead(notificationId: number) {
    return this.http.put(`${this.apiUrl}/${notificationId}/read`, {});
  }

  // Polling every 30 seconds
  pollNotifications(userId: number) {
    return interval(30000).pipe(
      switchMap(() => this.getNotifications(userId))
    );
  }
}
