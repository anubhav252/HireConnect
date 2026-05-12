import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, RouterLinkActive } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, RouterLinkActive],
  template: `
    <nav class="glass navbar">
      <div class="container">
        <a routerLink="/" class="logo">
          <span class="gradient-text">HireConnect</span>
        </a>

        <ul class="nav-links">
          <li><a routerLink="/jobs" routerLinkActive="active" [routerLinkActiveOptions]="{exact: true}">Browse Jobs</a></li>
          
          @if (authService.isCandidate()) {
            <li><a routerLink="/candidate/dashboard" routerLinkActive="active">My Applications</a></li>
          }
          
          @if (authService.isRecruiter()) {
            <li><a routerLink="/recruiter/dashboard" routerLinkActive="active">Recruiter Panel</a></li>
          }
        </ul>

        <div class="auth-actions">
          @if (!authService.isAuthenticated()) {
            <button class="btn-outline" routerLink="/auth/login">Login</button>
            <button class="btn-primary" routerLink="/auth/register">Get Started</button>
          } @else {
            <div class="user-info">
              <span class="user-role">{{ authService.currentUser()?.role }}</span>
              <button class="btn-logout" (click)="authService.logout()">Logout</button>
            </div>
          }
        </div>
      </div>
    </nav>
  `,
  styles: [`
    .navbar {
      position: sticky;
      top: 20px;
      margin: 20px auto;
      max-width: 1200px;
      padding: 15px 30px;
      z-index: 1000;
      display: flex;
      align-items: center;
      justify-content: center;
    }
    .container {
      width: 100%;
      display: flex;
      justify-content: space-between;
      align-items: center;
    }
    .logo {
      font-size: 1.5rem;
      font-weight: 800;
      text-decoration: none;
    }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #06b6d4);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .nav-links {
      display: flex;
      list-style: none;
      gap: 30px;
    }
    .nav-links a {
      color: #f8fafc;
      text-decoration: none;
      font-weight: 500;
      transition: color 0.3s;
    }
    .nav-links a:hover, .nav-links a.active {
      color: #3b82f6;
    }
    .auth-actions {
      display: flex;
      gap: 15px;
      align-items: center;
    }
    .user-role {
      background: rgba(59, 130, 246, 0.1);
      color: #3b82f6;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 0.8rem;
      margin-right: 10px;
    }
    .btn-logout {
      background: transparent;
      border: 1px solid rgba(255, 0, 0, 0.2);
      color: #ef4444;
      padding: 6px 12px;
      border-radius: 6px;
      cursor: pointer;
    }
    .btn-logout:hover {
      background: rgba(239, 68, 68, 0.1);
    }
  `]
})
export class NavbarComponent {
  authService = inject(AuthService);
}
