import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-wrapper fade-in">
      <div class="auth-card glass">
        <h2 class="auth-title">Welcome Back</h2>
        <p class="auth-subtitle">Login to access your dashboard</p>

        <form [formGroup]="loginForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="name@company.com" class="form-input">
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••" class="form-input">
          </div>

          <button type="submit" class="btn-primary w-full" [disabled]="loginForm.invalid || isLoading">
            {{ isLoading ? 'Logging in...' : 'Sign In' }}
          </button>
        </form>

        <div class="divider">
          <span>OR CONTINUE WITH</span>
        </div>

        <div class="social-auth">
          <button class="btn-social glass glass-hover" (click)="loginWith('github')">
            <img src="https://github.githubassets.com/images/modules/logos_page/GitHub-Mark.png" alt="GitHub">
            GitHub
          </button>
          <button class="btn-social glass glass-hover" (click)="loginWith('google')">
            <img src="https://upload.wikimedia.org/wikipedia/commons/c/c1/Google_%22G%22_logo.svg" alt="Google">
            Google
          </button>
        </div>

        <p class="auth-footer">
          Don't have an account? <a routerLink="/auth/register">Create one</a>
        </p>
      </div>
    </div>
  `,
  styles: [`
    .auth-wrapper {
      display: flex;
      justify-content: center;
      align-items: center;
      padding: 60px 20px;
    }
    .auth-card {
      width: 100%;
      max-width: 450px;
      padding: 40px;
      text-align: center;
    }
    .auth-title {
      font-size: 2rem;
      font-weight: 700;
      margin-bottom: 10px;
    }
    .auth-subtitle {
      color: #94a3b8;
      margin-bottom: 30px;
    }
    .auth-form {
      text-align: left;
    }
    .form-group {
      margin-bottom: 20px;
    }
    .form-group label {
      display: block;
      margin-bottom: 8px;
      font-weight: 500;
      font-size: 0.9rem;
    }
    .form-input {
      width: 100%;
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      padding: 12px;
      color: white;
      outline: none;
      transition: border-color 0.3s;
    }
    .form-input:focus {
      border-color: #3b82f6;
    }
    .w-full { width: 100%; }
    .divider {
      margin: 30px 0;
      position: relative;
      border-bottom: 1px solid rgba(255, 255, 255, 0.1);
    }
    .divider span {
      position: absolute;
      top: 50%;
      left: 50%;
      transform: translate(-50%, -50%);
      background: #1e293b;
      padding: 0 15px;
      font-size: 0.75rem;
      color: #64748b;
    }
    .social-auth {
      display: flex;
      gap: 15px;
      margin-bottom: 30px;
    }
    .btn-social {
      flex: 1;
      display: flex;
      align-items: center;
      justify-content: center;
      gap: 10px;
      padding: 12px;
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      background: transparent;
      color: white;
      cursor: pointer;
      font-weight: 500;
    }
    .btn-social img {
      width: 20px;
      height: 20px;
      filter: invert(1);
    }
    .auth-footer {
      color: #94a3b8;
      font-size: 0.9rem;
    }
    .auth-footer a {
      color: #3b82f6;
      text-decoration: none;
      font-weight: 600;
    }
  `]
})
export class LoginComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = false;

  loginForm = this.fb.group({
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required]]
  });

  onSubmit() {
    if (this.loginForm.invalid) return;

    this.isLoading = true;
    const { email, password } = this.loginForm.value;

    this.authService.login({ email: email!, password: password! }).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading = false;
        alert(err.error?.message || 'Login failed');
      }
    });
  }

  loginWith(provider: 'github' | 'google') {
    // Redirect to the Gateway social auth endpoint
    window.location.href = `${environment.apiUrl}/auth/login/${provider}?role=Candidate`;
  }
}
