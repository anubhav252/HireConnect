import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { AuthResponse, LoginRequest, RegisterRequest, UserCredential } from '../../models/auth.models';
import { tap } from 'rxjs';
import { Router } from '@angular/router';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  private http = inject(HttpClient);
  private router = inject(Router);
  private apiUrl = `${environment.apiUrl}/auth`;

  // Reactive state for the current user
  currentUser = signal<AuthResponse | null>(this.getStoredAuth());

  register(request: RegisterRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/register`, request).pipe(
      tap(res => this.setAuth(res))
    );
  }

  login(request: LoginRequest) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/login`, request).pipe(
      tap(res => this.setAuth(res))
    );
  }

  loginWithGoogle(idToken: string) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/google-login`, { idToken }).pipe(
      tap(res => this.setAuth(res))
    );
  }

  loginWithGithub(code: string) {
    return this.http.post<AuthResponse>(`${this.apiUrl}/github-login`, { code }).pipe(
      tap(res => this.setAuth(res))
    );
  }

  logout() {
    localStorage.removeItem('hc_auth');
    this.currentUser.set(null);
    this.router.navigate(['/auth/login']);
  }

  private setAuth(res: AuthResponse) {
    localStorage.setItem('hc_auth', JSON.stringify(res));
    this.currentUser.set(res);
  }

  private getStoredAuth(): AuthResponse | null {
    const stored = localStorage.getItem('hc_auth');
    return stored ? JSON.parse(stored) : null;
  }

  getToken(): string | null {
    return this.currentUser()?.token || null;
  }

  isAuthenticated(): boolean {
    return !!this.currentUser();
  }

  isRecruiter(): boolean {
    return this.currentUser()?.role === 'Recruiter';
  }

  isCandidate(): boolean {
    return this.currentUser()?.role === 'Candidate';
  }
}
