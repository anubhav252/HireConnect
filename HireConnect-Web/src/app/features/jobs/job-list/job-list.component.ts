import { Component, OnInit, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobService } from '../../../core/services/job/job.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { Job } from '../../../core/models/job.models';
import { RouterLink, ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-job-list',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="job-list-container fade-in">
      <div class="header">
        <h2 class="title">
          {{ authService.isRecruiter() ? 'My' : 'Explore' }} 
          <span class="gradient-text">{{ authService.isRecruiter() ? 'Postings' : 'Available Jobs' }}</span>
        </h2>
        <p class="subtitle">
          {{ authService.isRecruiter() ? 'Manage your active job listings and tracking.' : 'Find the perfect opportunity that matches your skillset.' }}
        </p>
      </div>

      <div class="filters-sidebar glass">
        <h3>Filter Results</h3>
        <!-- Simple filters for now -->
        <div class="filter-group">
          <label>Job Type</label>
          <div class="checkbox-list">
            <label><input type="checkbox" (change)="toggleType('Full-Time')"> Full-time</label>
            <label><input type="checkbox" (change)="toggleType('Contract')"> Contract</label>
            <label><input type="checkbox" (change)="toggleType('Remote')"> Remote</label>
            <label><input type="checkbox" (change)="toggleType('Part-Time')"> Part-Time</label>
          </div>
        </div>
      </div>

      <div class="job-grid">
        @if (isLoading()) {
          <div class="loading-state">
            <div class="spinner"></div>
            <p>Fetching jobs...</p>
          </div>
        } @else if (filteredJobs().length === 0) {
          <div class="empty-state glass">
            <p>No jobs found matching your criteria.</p>
          </div>
        } @else {
          @for (job of filteredJobs(); track job.jobId) {
            <div class="job-card glass glass-hover">
              <div class="job-card-header">
                <div class="company-logo">🏢</div>
                <div class="job-info">
                  <h3 class="job-title">{{ job.title }}</h3>
                  <p class="job-category">{{ job.category }}</p>
                </div>
                <span class="job-type">{{ job.type }}</span>
              </div>

              <div class="job-details">
                <div class="detail">
                  <span class="icon">📍</span> {{ job.location }}
                </div>
                <div class="detail">
                  <span class="icon">💰</span> {{ job.salaryMin | currency:'USD':'symbol':'1.0-0' }} - {{ job.salaryMax | currency:'USD':'symbol':'1.0-0' }}
                </div>
              </div>

              <div class="job-card-footer">
                <span class="posted-date">Posted {{ job.postedAt | date }}</span>
                <button class="btn-primary" [routerLink]="['/jobs', job.jobId]">View Details</button>
              </div>
            </div>
          }
        }
      </div>
    </div>
  `,
  styles: [`
    .job-list-container {
      display: grid;
      grid-template-columns: 300px 1fr;
      gap: 30px;
      max-width: 1200px;
      margin: 40px auto;
      padding: 0 20px;
    }
    .header {
      grid-column: 1 / -1;
      text-align: center;
      margin-bottom: 20px;
    }
    .title { font-size: 2.5rem; font-weight: 700; }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #06b6d4);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .filters-sidebar {
      padding: 25px;
      height: fit-content;
    }
    .filters-sidebar h3 { margin-bottom: 20px; font-size: 1.2rem; }
    .filter-group { margin-bottom: 20px; }
    .checkbox-list { display: flex; flex-direction: column; gap: 10px; margin-top: 10px; }
    .checkbox-list label { color: #94a3b8; cursor: pointer; display: flex; gap: 8px; }

    .job-grid {
      display: flex;
      flex-direction: column;
      gap: 20px;
    }
    .job-card {
      padding: 25px;
    }
    .job-card-header {
      display: flex;
      align-items: center;
      gap: 20px;
      margin-bottom: 20px;
    }
    .company-logo {
      width: 50px;
      height: 50px;
      background: rgba(59, 130, 246, 0.1);
      display: flex;
      align-items: center;
      justify-content: center;
      font-size: 1.5rem;
      border-radius: 10px;
    }
    .job-info { flex: 1; }
    .job-title { font-size: 1.3rem; font-weight: 700; margin-bottom: 5px; }
    .job-category { color: #3b82f6; font-size: 0.9rem; font-weight: 600; }
    .job-type {
      background: rgba(6, 182, 212, 0.1);
      color: #06b6d4;
      padding: 4px 12px;
      border-radius: 20px;
      font-size: 0.8rem;
    }
    .job-details {
      display: flex;
      gap: 30px;
      margin-bottom: 20px;
      color: #94a3b8;
    }
    .detail { display: flex; align-items: center; gap: 8px; }
    .job-card-footer {
      display: flex;
      justify-content: space-between;
      align-items: center;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
      padding-top: 20px;
    }
    .posted-date { font-size: 0.85rem; color: #64748b; }

    .loading-state, .empty-state {
      padding: 60px;
      text-align: center;
    }
    .spinner {
      width: 40px;
      height: 40px;
      border: 3px solid rgba(59, 130, 246, 0.3);
      border-top-color: #3b82f6;
      border-radius: 50%;
      animation: spin 1s linear infinite;
      margin: 0 auto 20px;
    }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class JobListComponent implements OnInit {
  private jobService = inject(JobService);
  private route = inject(ActivatedRoute);
  public authService = inject(AuthService);
  
  jobs = signal<Job[]>([]);
  isLoading = signal(true);
  selectedTypes = signal<Set<string>>(new Set());

  filteredJobs = computed(() => {
    const allJobs = this.jobs();
    const types = this.selectedTypes();
    const isRecruiter = this.authService.isRecruiter();
    const currentUserId = this.authService.currentUser()?.userId;
    
    let filtered = allJobs;

    // Filter by recruiter if logged in as one
    if (isRecruiter && currentUserId) {
      filtered = filtered.filter(job => job.postedBy === currentUserId);
    }

    // Apply job type filters
    if (types.size > 0) {
      const lowerTypes = new Set(Array.from(types).map(t => t.toLowerCase()));
      filtered = filtered.filter(job => lowerTypes.has(job.type.toLowerCase()));
    }
    
    return filtered;
  });

  ngOnInit() {
    this.route.queryParamMap.subscribe(params => {
      const title = params.get('title') || '';
      const location = params.get('location') || '';
      
      if (title || location) {
        this.fetchSearchedJobs(title, location);
      } else {
        this.fetchJobs();
      }
    });
  }

  fetchJobs() {
    this.isLoading.set(true);
    this.jobService.getAllJobs().subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.jobs.set([]);
      }
    });
  }

  fetchSearchedJobs(title: string, location: string) {
    this.isLoading.set(true);
    this.jobService.searchJobs(title, location).subscribe({
      next: (data) => {
        this.jobs.set(data);
        this.isLoading.set(false);
      },
      error: () => {
        this.isLoading.set(false);
        this.jobs.set([]);
      }
    });
  }

  toggleType(type: string) {
    const current = new Set(this.selectedTypes());
    if (current.has(type)) {
      current.delete(type);
    } else {
      current.add(type);
    }
    this.selectedTypes.set(current);
  }
}
