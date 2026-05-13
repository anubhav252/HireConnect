import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Interview, InterviewRequestDto } from '../../models/interview.models';

@Injectable({
  providedIn: 'root'
})
export class InterviewService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/interviews`;

  getInterviewsByRecruiter(recruiterId: number) {
    return this.http.get<Interview[]>(`${this.apiUrl}/recruiter/${recruiterId}`);
  }

  scheduleInterview(interview: InterviewRequestDto) {
    return this.http.post<Interview>(this.apiUrl, interview);
  }

  getInterviewsByApplication(applicationId: number) {
    return this.http.get<Interview[]>(`${this.apiUrl}/application/${applicationId}`);
  }

  cancelInterview(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }

  updateInterviewStatus(id: number, status: string) {
    return this.http.put<Interview>(`${this.apiUrl}/${id}/status`, { status });
  }

  confirmInterview(id: number) {
    return this.http.put(`${this.apiUrl}/${id}/confirm`, {});
  }
}
