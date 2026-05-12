import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class AnalyticsService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/analytics`;

  getRecruiterStats(recruiterId: number) {
    return this.http.get<any>(`${this.apiUrl}/recruiter/${recruiterId}`);
  }

  getPlatformStats() {
    return this.http.get<any>(`${this.apiUrl}/admin`);
  }
}
