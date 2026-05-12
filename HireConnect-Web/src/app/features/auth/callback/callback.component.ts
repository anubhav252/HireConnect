import { Component, OnInit, inject } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-callback',
  standalone: true,
  template: `
    <div class="callback-container fade-in">
      <div class="glass loading-card">
        <div class="spinner"></div>
        <h2>Authenticating with HireConnect...</h2>
        <p>Please wait while we secure your session.</p>
      </div>
    </div>
  `,
  styles: [`
    .callback-container {
      display: flex;
      justify-content: center;
      align-items: center;
      height: 70vh;
    }
    .loading-card {
      padding: 60px;
      text-align: center;
      max-width: 500px;
    }
    .spinner {
      width: 50px;
      height: 50px;
      border: 4px solid rgba(59, 130, 246, 0.1);
      border-top: 4px solid #3b82f6;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 25px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
    h2 { margin-bottom: 10px; font-weight: 700; }
    p { color: #94a3b8; }
  `]
})
export class CallbackComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private authService = inject(AuthService);

  ngOnInit() {
    this.route.queryParams.subscribe(params => {
      const token = params['token'];
      const userId = params['userId'];
      const email = params['email'];
      const role = params['role'];

      if (token && userId) {
        // Construct the AuthResponse and save it
        const authData = {
          token,
          userId: +userId,
          email,
          role
        };
        
        // Use the internal setAuth method via a temporary cast or a helper if needed
        // For simplicity, we'll manually set it since we have access to localStorage logic in AuthService
        localStorage.setItem('hc_auth', JSON.stringify(authData));
        this.authService.currentUser.set(authData);

        // Redirect to dashboard based on role
        if (role === 'Recruiter') {
          this.router.navigate(['/recruiter/dashboard']);
        } else {
          this.router.navigate(['/candidate/dashboard']);
        }
      } else {
        this.router.navigate(['/auth/login']);
      }
    });
  }
}
