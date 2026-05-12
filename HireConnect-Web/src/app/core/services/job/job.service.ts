import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { Job, JobRequestDto } from '../../models/job.models';
import { map } from 'rxjs';

export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
}

@Injectable({
  providedIn: 'root'
})
export class JobService {
  private http = inject(HttpClient);
  private apiUrl = `${environment.apiUrl}/jobs`;

  getAllJobs() {
    return this.http.get<ApiResponse<Job[]>>(this.apiUrl).pipe(
      map(res => res.data)
    );
  }

  getJobById(id: number) {
    return this.http.get<ApiResponse<Job>>(`${this.apiUrl}/${id}`).pipe(
      map(res => res.data)
    );
  }

  searchJobs(title?: string, location?: string) {
    let url = `${this.apiUrl}/search?`;
    if (title) url += `keyword=${title}&`;
    if (location) url += `location=${location}`;
    
    return this.http.get<ApiResponse<Job[]>>(url).pipe(
      map(res => res.data)
    );
  }

  createJob(job: JobRequestDto) {
    return this.http.post<ApiResponse<Job>>(this.apiUrl, job).pipe(
      map(res => res.data)
    );
  }

  updateJob(id: number, job: Partial<Job>) {
    return this.http.put<ApiResponse<Job>>(`${this.apiUrl}/${id}`, job).pipe(
      map(res => res.data)
    );
  }

  deleteJob(id: number) {
    return this.http.delete(`${this.apiUrl}/${id}`);
  }
}
