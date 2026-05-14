import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';
import { NotificationService } from '../../../core/services/notification/notification.service';
import { Notification } from '../../../core/models/notification.models';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <header class="app-header glass">
      <div class="logo" routerLink="/">
        <span class="gradient-text">HireConnect</span>
      </div>

      <nav class="nav-links">
        <a routerLink="/jobs" routerLinkActive="active">Find Jobs</a>
        
        @if (authService.isAuthenticated()) {
          @if (authService.isCandidate()) {
            <a routerLink="/candidate/dashboard" routerLinkActive="active">Dashboard</a>
            <a routerLink="/candidate/profile" routerLinkActive="active">Profile</a>
          } @else if (authService.isRecruiter()) {
            <a routerLink="/recruiter/dashboard" routerLinkActive="active">Dashboard</a>
            <a routerLink="/recruiter/post-job" routerLinkActive="active">Post Job</a>
            <a routerLink="/recruiter/profile" routerLinkActive="active">Profile</a>
          }
        }
      </nav>

      <div class="auth-actions">
        @if (!authService.isAuthenticated()) {
          <button class="btn-text" routerLink="/auth/login">Login</button>
          <button class="btn-primary" routerLink="/auth/register">Sign Up</button>
        } @else {
          <div class="user-actions">
            <div class="notification-wrapper" (click)="toggleNotifications()">
              <span class="icon">🔔</span>
              @if (unreadCount() > 0) {
                <span class="badge">{{ unreadCount() }}</span>
              }
              
              @if (showNotifications()) {
                <div class="notification-dropdown glass">
                  <h4>Notifications</h4>
                  @if (notifications().length === 0) {
                    <p class="empty">No new notifications</p>
                  } @else {
                    <div class="notif-list">
                      @for (notif of notifications(); track notif.notificationId) {
                        <div class="notif-item" [class.unread]="!notif.isRead" (click)="markAsRead(notif.notificationId); $event.stopPropagation()">
                          <p>{{ notif.message }}</p>
                          <span class="time">{{ notif.createdAt | date:'short' }}</span>
                        </div>
                      }
                    </div>
                  }
                </div>
              }
            </div>

            <div class="profile-menu">
              <span class="user-name">{{ authService.currentUser()?.email }}</span>
              <button class="btn-outline btn-sm" (click)="logout()">Logout</button>
            </div>
          </div>
        }
      </div>
    </header>
  `,
  styles: [`
    .app-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      padding: 15px 40px;
      position: sticky;
      top: 0;
      z-index: 100;
      border-radius: 0;
      border-left: none;
      border-right: none;
      border-top: none;
    }
    .logo {
      font-size: 1.5rem;
      font-weight: 800;
      cursor: pointer;
    }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #06b6d4);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .nav-links {
      display: flex;
      gap: 30px;
    }
    .nav-links a {
      color: var(--text-main);
      text-decoration: none;
      font-weight: 600;
      transition: color 0.3s ease;
    }
    .nav-links a:hover, .nav-links a.active {
      color: var(--primary);
    }
    .auth-actions {
      display: flex;
      align-items: center;
      gap: 15px;
    }
    .btn-text {
      background: none;
      border: none;
      color: var(--text-main);
      font-weight: 600;
      cursor: pointer;
    }
    .btn-text:hover { color: var(--primary); }
    .btn-sm { padding: 6px 12px; font-size: 0.85rem; }

    .user-actions {
      display: flex;
      align-items: center;
      gap: 20px;
    }

    .notification-wrapper {
      position: relative;
      cursor: pointer;
    }
    .notification-wrapper .icon { font-size: 1.2rem; }
    .badge {
      position: absolute;
      top: -5px;
      right: -10px;
      background: #ef4444;
      color: white;
      font-size: 0.7rem;
      font-weight: bold;
      padding: 2px 6px;
      border-radius: 10px;
    }
    .notification-dropdown {
      position: absolute;
      top: 150%;
      right: -50px;
      width: 300px;
      padding: 15px;
      border-radius: 12px;
      cursor: default;
    }
    .notification-dropdown h4 { margin-bottom: 10px; font-size: 1rem; border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 5px; }
    .notif-list { max-height: 300px; overflow-y: auto; }
    .notif-item {
      padding: 10px;
      border-radius: 8px;
      transition: background 0.2s;
      cursor: pointer;
    }
    .notif-item:hover { background: rgba(255,255,255,0.05); }
    .notif-item.unread { border-left: 3px solid var(--primary); background: rgba(59, 130, 246, 0.05); }
    .notif-item p { font-size: 0.85rem; margin-bottom: 3px; }
    .notif-item .time { font-size: 0.7rem; color: var(--text-muted); }
    .empty { font-size: 0.85rem; color: var(--text-muted); text-align: center; padding: 10px 0; }

    .profile-menu {
      display: flex;
      align-items: center;
      gap: 15px;
      border-left: 1px solid rgba(255,255,255,0.1);
      padding-left: 20px;
    }
    .user-name { font-weight: 600; font-size: 0.9rem; }
  `]
})
export class HeaderComponent implements OnInit {
  authService = inject(AuthService);
  private notifService = inject(NotificationService);
  private router = inject(Router);

  notifications = signal<Notification[]>([]);
  showNotifications = signal(false);

  get unreadCount() {
    return () => this.notifications().filter(n => !n.isRead).length;
  }

  ngOnInit() {
    if (this.authService.isAuthenticated()) {
      this.loadNotifications();
    }
  }

  loadNotifications() {
    const userId = this.authService.currentUser()?.userId;
    if (userId) {
      this.notifService.pollNotifications(userId).subscribe(data => {
        this.notifications.set(data);
      });
      // Initial load
      this.notifService.getNotifications(userId).subscribe(data => {
        this.notifications.set(data);
      });
    }
  }

  toggleNotifications() {
    this.showNotifications.set(!this.showNotifications());
  }

  markAsRead(id: number) {
    this.notifService.markAsRead(id).subscribe(() => {
      const updated = this.notifications().map(n => n.notificationId === id ? { ...n, isRead: true } : n);
      this.notifications.set(updated);
    });
  }

  logout() {
    this.authService.logout();
  }
}
