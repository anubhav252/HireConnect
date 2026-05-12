import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="auth-wrapper fade-in">
      <div class="auth-card glass">
        <h2 class="auth-title">Join HireConnect</h2>
        <p class="auth-subtitle">Create your professional account today</p>

        <form [formGroup]="registerForm" (ngSubmit)="onSubmit()" class="auth-form">
          <div class="form-group">
            <label>Full Name</label>
            <input type="text" formControlName="fullName" placeholder="John Doe" class="form-input">
          </div>

          <div class="form-group">
            <label>Email Address</label>
            <input type="email" formControlName="email" placeholder="name@company.com" class="form-input">
          </div>

          <div class="form-group">
            <label>Password</label>
            <input type="password" formControlName="password" placeholder="••••••••" class="form-input">
          </div>

          <div class="role-selector">
            <label class="role-option" [class.selected]="registerForm.get('role')?.value === 'Candidate'">
              <input type="radio" formControlName="role" value="Candidate">
              <span>Candidate</span>
            </label>
            <label class="role-option" [class.selected]="registerForm.get('role')?.value === 'Recruiter'">
              <input type="radio" formControlName="role" value="Recruiter">
              <span>Recruiter</span>
            </label>
          </div>

          <button type="submit" class="btn-primary w-full" [disabled]="registerForm.invalid || isLoading">
            {{ isLoading ? 'Creating Account...' : 'Sign Up' }}
          </button>
        </form>

        <p class="auth-footer mt-20">
          Already have an account? <a routerLink="/auth/login">Login here</a>
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
      margin-bottom: 15px;
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
    }
    .form-input:focus { border-color: #3b82f6; }
    
    .role-selector {
      display: flex;
      gap: 15px;
      margin: 25px 0;
    }
    .role-option {
      flex: 1;
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      padding: 15px;
      text-align: center;
      cursor: pointer;
      transition: all 0.3s;
    }
    .role-option input { display: none; }
    .role-option.selected {
      background: rgba(59, 130, 246, 0.1);
      border-color: #3b82f6;
      color: #3b82f6;
    }
    
    .w-full { width: 100%; }
    .mt-20 { margin-top: 20px; }
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
export class RegisterComponent {
  private fb = inject(FormBuilder);
  private authService = inject(AuthService);
  private router = inject(Router);

  isLoading = false;

  registerForm = this.fb.group({
    fullName: ['', [Validators.required]],
    email: ['', [Validators.required, Validators.email]],
    password: ['', [Validators.required, Validators.minLength(6)]],
    role: ['Candidate', [Validators.required]]
  });

  onSubmit() {
    if (this.registerForm.invalid) return;

    this.isLoading = true;
    const { fullName, email, password, role } = this.registerForm.value;

    this.authService.register({ 
      fullName: fullName!, 
      email: email!, 
      password: password!, 
      role: role! 
    }).subscribe({
      next: () => {
        this.router.navigate(['/']);
      },
      error: (err) => {
        this.isLoading = false;
        alert(err.error?.message || 'Registration failed');
      }
    });
  }
}
