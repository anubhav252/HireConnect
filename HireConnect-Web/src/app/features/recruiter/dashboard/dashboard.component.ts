import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { JobService } from '../../../core/services/job/job.service';
import { AnalyticsService } from '../../../core/services/analytics/analytics.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { Router, RouterLink } from '@angular/router';
import { ApplicationService } from '../../../core/services/application/application.service';
import { forkJoin, map } from 'rxjs';

@Component({
  selector: 'app-recruiter-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-container fade-in">
      <div class="dashboard-header">
        <div>
          <h2 class="title">Recruiter <span class="gradient-text">Dashboard</span></h2>
          <p class="subtitle">Manage your job postings and track candidate pipeline.</p>
        </div>
        <button class="btn-primary" routerLink="/recruiter/post-job">+ Post New Job</button>
      </div>

      <!-- Analytics Cards -->
      <div class="analytics-grid">
        <div class="stat-card glass">
          <div class="stat-icon">📄</div>
          <div class="stat-info">
            <span class="stat-label">Active Jobs</span>
            <span class="stat-value">{{ activeJobsCount() }}</span>
          </div>
        </div>
        <div class="stat-card glass">
          <div class="stat-icon">👥</div>
          <div class="stat-info">
            <span class="stat-label">Total Applications</span>
            <span class="stat-value">{{ stats()?.totalApplications || 0 }}</span>
          </div>
        </div>
        <div class="stat-card glass">
          <div class="stat-icon">🎯</div>
          <div class="stat-info">
            <span class="stat-label">Shortlisted</span>
            <span class="stat-value">{{ stats()?.shortlistedCount || 0 }}</span>
          </div>
        </div>
        <div class="stat-card glass">
          <div class="stat-icon">⚡</div>
          <div class="stat-info">
            <span class="stat-label">Conversion Rate</span>
            <span class="stat-value">{{ (stats()?.viewToApplyRatio || 0) | percent }}</span>
          </div>
        </div>
      </div>

      <!-- Jobs Pipeline -->
      <div class="content-section">
        <h3 class="section-title">Your Job Postings</h3>
        
        <div class="jobs-list glass">
          @if (isLoading()) {
             <div class="loading">Loading your jobs...</div>
          } @else if (myJobs().length === 0) {
             <div class="empty">You haven't posted any jobs yet.</div>
          } @else {
            <table class="dashboard-table">
              <thead>
                <tr>
                  <th>Job Title</th>
                  <th>Category</th>
                  <th>Posted Date</th>
                  <th>Applications</th>
                  <th>Status</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                @for (job of myJobs(); track job.jobId) {
                  <tr>
                    <td><strong>{{ job.title }}</strong></td>
                    <td>{{ job.category }}</td>
                    <td>{{ job.postedAt | date }}</td>
                    <td><span class="app-count">{{ applicationCounts()[job.jobId] || 0 }}</span></td>
                    <td><span class="status-badge" [class.active]="job.status === 'Active'">{{ job.status }}</span></td>
                    <td>
                      <div class="action-buttons">
                        <button class="btn-icon" [routerLink]="['/recruiter/jobs', job.jobId, 'applications']" title="View Applications">👥</button>
                        <a class="btn-icon" [routerLink]="['/recruiter/edit-job', job.jobId]" title="Edit Job">✏️</a>
                        <button class="btn-icon delete" (click)="deleteJob(job.jobId)" title="Delete Job">🗑️</button>
                      </div>
                    </td>
                  </tr>
                }
              </tbody>
            </table>
          }
        </div>
      </div>
    </div>
  `,
  styles: [`
    .dashboard-container {
      max-width: 1200px;
      margin: 40px auto;
      padding: 0 20px;
    }
    .dashboard-header {
      display: flex;
      justify-content: space-between;
      align-items: center;
      margin-bottom: 40px;
    }
    .title { font-size: 2.5rem; font-weight: 700; }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #8b5cf6);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .analytics-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(240px, 1fr));
      gap: 20px;
      margin-bottom: 40px;
    }
    .stat-card {
      padding: 25px;
      display: flex;
      align-items: center;
      gap: 20px;
    }
    .stat-icon {
      font-size: 2rem;
      width: 60px;
      height: 60px;
      background: rgba(59, 130, 246, 0.1);
      display: flex;
      align-items: center;
      justify-content: center;
      border-radius: 12px;
    }
    .stat-info { display: flex; flex-direction: column; }
    .stat-label { font-size: 0.9rem; color: #94a3b8; }
    .stat-value { font-size: 1.8rem; font-weight: 700; color: white; }

    .content-section { margin-top: 40px; }
    .section-title { font-size: 1.5rem; font-weight: 600; margin-bottom: 20px; }
    
    .jobs-list { overflow: hidden; }
    .dashboard-table {
      width: 100%;
      border-collapse: collapse;
      text-align: left;
    }
    .dashboard-table th {
      padding: 15px 20px;
      background: rgba(255, 255, 255, 0.05);
      font-size: 0.9rem;
      color: #94a3b8;
    }
    .dashboard-table td {
      padding: 15px 20px;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
    }
    .status-badge {
      padding: 4px 10px;
      border-radius: 20px;
      font-size: 0.75rem;
      background: rgba(255, 255, 255, 0.1);
    }
    .status-badge.active {
      background: rgba(34, 197, 94, 0.1);
      color: #22c55e;
    }
    .app-count {
      background: #3b82f6;
      color: white;
      padding: 2px 8px;
      border-radius: 10px;
      font-size: 0.8rem;
    }
    .action-buttons { display: flex; gap: 10px; }
    .btn-icon {
      background: rgba(255, 255, 255, 0.05);
      border: none;
      color: white;
      width: 35px;
      height: 35px;
      border-radius: 6px;
      cursor: pointer;
      display: flex;
      align-items: center;
      justify-content: center;
      transition: all 0.2s;
    }
    .btn-icon:hover { background: rgba(59, 130, 246, 0.2); }
    .btn-icon.delete:hover { background: rgba(239, 68, 68, 0.2); }

    .loading, .empty { padding: 40px; text-align: center; color: #94a3b8; }
  `]
})
export class RecruiterDashboardComponent implements OnInit {
  private jobService = inject(JobService);
  private analyticsService = inject(AnalyticsService);
  private appService = inject(ApplicationService);
  private authService = inject(AuthService);
  private router = inject(Router);

  myJobs = signal<any[]>([]);
  stats = signal<any>(null);
  isLoading = signal(true);
  activeJobsCount = signal(0);
  applicationCounts = signal<Record<number, number>>({});

  ngOnInit() {
    this.fetchData();
  }

  fetchData() {
    const userId = this.authService.currentUser()?.userId;
    if (!userId) return;

    // Fetch Recruiter's Jobs
    this.jobService.getAllJobs().subscribe(jobs => {
      const filtered = jobs.filter(j => j.postedBy === userId);
      this.myJobs.set(filtered);
      this.activeJobsCount.set(filtered.filter(j => j.status === 'Active').length);
      this.isLoading.set(false);

      // Fetch application counts for each job
      filtered.forEach(job => {
        this.appService.getApplicationsByJob(job.jobId).subscribe(apps => {
          this.applicationCounts.update(counts => ({ ...counts, [job.jobId]: apps.length }));
        });
      });
    });

    // Fetch Stats
    this.analyticsService.getRecruiterStats(userId).subscribe(data => {
      this.stats.set(data);
    });
  }

  editJob(jobId: number) {
    this.router.navigate(['/recruiter/edit-job', jobId]);
  }

  deleteJob(jobId: number) {
    if (confirm('Are you sure you want to delete this job posting?')) {
      this.jobService.deleteJob(jobId).subscribe({
        next: () => {
          this.myJobs.update(jobs => jobs.filter(j => j.jobId !== jobId));
          this.activeJobsCount.update(count => count - 1);
        },
        error: () => alert('Failed to delete job.')
      });
    }
  }
}
