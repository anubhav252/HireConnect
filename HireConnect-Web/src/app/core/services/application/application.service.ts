import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';

@Injectable({
  providedIn: 'root'
})
export class ApplicationService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/applications`;

  submitApplication(application: any) {
    return this.http.post(this.apiUrl, application);
  }

  getApplicationsByCandidate(candidateId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/candidate/${candidateId}`);
  }

  getApplicationsByJob(jobId: number) {
    return this.http.get<any[]>(`${this.apiUrl}/job/${jobId}`);
  }

  updateStatus(applicationId: number, status: string) {
    return this.http.put(`${this.apiUrl}/${applicationId}/status?status=${status}`, {}, { responseType: 'text' });
  }

  deleteApplication(applicationId: number) {
    return this.http.delete(`${this.apiUrl}/${applicationId}/withdraw`);
  }
}
