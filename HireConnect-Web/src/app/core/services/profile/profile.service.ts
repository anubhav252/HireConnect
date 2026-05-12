import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { CandidateProfileResponse, RecruiterProfileResponse } from '../../models/profile.models';

@Injectable({
  providedIn: 'root'
})
export class ProfileService {
  private http = inject(HttpClient);
  private baseUrl = environment.apiUrl;

  getCandidateProfile(userId: number) {
    return this.http.get<CandidateProfileResponse>(`${this.baseUrl}/candidates/user/${userId}`);
  }

  getRecruiterProfile(userId: number) {
    return this.http.get<RecruiterProfileResponse>(`${this.baseUrl}/recruiters/user/${userId}`);
  }

  createCandidateProfile(profile: any) {
    return this.http.post<CandidateProfileResponse>(`${this.baseUrl}/candidates`, profile);
  }

  createRecruiterProfile(profile: any) {
    return this.http.post<RecruiterProfileResponse>(`${this.baseUrl}/recruiters`, profile);
  }

  updateCandidateProfile(profile: any) {
    return this.http.put<CandidateProfileResponse>(`${this.baseUrl}/candidates/me`, profile);
  }

  updateRecruiterProfile(profile: any) {
    return this.http.put<RecruiterProfileResponse>(`${this.baseUrl}/recruiters/me`, profile);
  }
}
