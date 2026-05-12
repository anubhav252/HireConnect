import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ApplicationService } from '../../../core/services/application/application.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { InterviewService } from '../../../core/services/interview/interview.service';
import { JobService } from '../../../core/services/job/job.service';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-candidate-dashboard',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="dashboard-container fade-in">
      <div class="dashboard-header">
        <div>
          <h2 class="title">My <span class="gradient-text">Dashboard</span></h2>
          <p class="subtitle">Track your job applications and upcoming interviews.</p>
        </div>
        <button class="btn-outline" routerLink="/jobs">Browse More Jobs</button>
      </div>

      <!-- Quick Stats -->
      <div class="stats-grid">
        <div class="stat-card glass">
          <span class="label">Total Applied</span>
          <span class="value">{{ applications().length }}</span>
        </div>
        <div class="stat-card glass highlight">
          <span class="label">Shortlisted</span>
          <span class="value">{{ shortlistedCount() }}</span>
        </div>
        <div class="stat-card glass">
          <span class="label">Interviews</span>
          <span class="value">{{ interviews().length }}</span>
        </div>
      </div>

      <!-- Applications List -->
      <div class="content-section">
        <h3 class="section-title">Applied Jobs</h3>
        
        <div class="applications-grid">
          @if (isLoading()) {
            <div class="loading">Loading your applications...</div>
          } @else if (applications().length === 0) {
            <div class="empty-state glass">
              <p>You haven't applied to any jobs yet. <a routerLink="/jobs">Start searching!</a></p>
            </div>
          } @else {
            @for (app of applications(); track app.applicationId) {
              <div class="app-card glass glass-hover">
                <div class="app-info">
                  <h4>{{ jobTitles()[app.jobId] || 'Job #' + app.jobId }}</h4>
                  <p class="applied-date">Applied on {{ app.appliedAt | date }}</p>
                </div>
                
                <div class="status-box">
                  <span class="status-label">Status</span>
                  <span class="status-value" [class]="app.status.toLowerCase()">{{ app.status }}</span>
                </div>

                <div class="actions">
                  <button class="btn-text" (click)="viewDetails(app.jobId)">View Details</button>
                  <button class="btn-text delete" (click)="withdraw(app.applicationId)">Withdraw</button>
                </div>
              </div>
            }
          }
        </div>
      </div>

      <!-- Saved Jobs Section -->
      <div class="content-section">
        <h3 class="section-title">Saved Jobs</h3>
        <div class="applications-grid">
          @if (savedJobs().length === 0) {
            <div class="empty-state glass">
              <p>No saved jobs yet.</p>
            </div>
          } @else {
            @for (job of savedJobs(); track job.jobId) {
              <div class="app-card glass glass-hover">
                <div class="app-info">
                  <h4>{{ job.title }}</h4>
                  <p class="applied-date">{{ job.companyName || job.location }}</p>
                </div>
                <div class="actions">
                  <button class="btn-text" (click)="viewDetails(job.jobId)">View & Apply</button>
                  <button class="btn-text delete" (click)="removeSavedJob(job.jobId)">Remove</button>
                </div>
              </div>
            }
          }
        </div>
      </div>

      <div class="content-section" *ngIf="interviews().length > 0">
        <h3 class="section-title">Upcoming Interviews</h3>
        <div class="applications-grid">
          @for (interview of interviews(); track interview.interviewId) {
            <div class="app-card glass glass-hover" style="border-left: 3px solid #06b6d4;">
              <div class="app-info">
                <h4>{{ jobTitles()[interview.jobId] || 'Job #' + interview.jobId }}</h4>
                <p class="applied-date">Scheduled on {{ interview.scheduledAt | date:'medium' }}</p>
              </div>
              
              <div class="status-box">
                <span class="status-label">Status: {{ interview.status }}</span>
                @if (interview.status === 'Scheduled') {
                  <div class="actions">
                    <button class="btn-text success" (click)="confirmInterview(interview.interviewId)">Accept</button>
                    <button class="btn-text delete" (click)="cancelInterview(interview)">Decline</button>
                  </div>
                } @else if (interview.status === 'Confirmed') {
                  <a [href]="interview.meetLink" target="_blank" class="status-value applied" style="text-decoration: none;">Join Meeting</a>
                } @else {
                  <span class="status-hint">{{ interview.status }}</span>
                }
              </div>
            </div>
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
      background: linear-gradient(135deg, #06b6d4, #3b82f6);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .stats-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(200px, 1fr));
      gap: 20px;
      margin-bottom: 40px;
    }
    .stat-card {
      padding: 25px;
      display: flex;
      flex-direction: column;
      gap: 5px;
    }
    .stat-card.highlight { border-color: #3b82f6; }
    .stat-card .label { font-size: 0.9rem; color: #94a3b8; }
    .stat-card .value { font-size: 2rem; font-weight: 800; color: white; }

    .content-section { margin-top: 40px; }
    .section-title { font-size: 1.5rem; font-weight: 600; margin-bottom: 20px; }

    .applications-grid {
      display: flex;
      flex-direction: column;
      gap: 15px;
    }
    .app-card {
      padding: 20px 30px;
      display: flex;
      align-items: center;
      justify-content: space-between;
    }
    .app-info h4 { font-size: 1.2rem; margin-bottom: 5px; }
    .applied-date { font-size: 0.85rem; color: #64748b; }

    .status-box { text-align: center; }
    .status-label { display: block; font-size: 0.75rem; color: #94a3b8; margin-bottom: 5px; }
    .status-value {
      padding: 4px 15px;
      border-radius: 20px;
      font-size: 0.85rem;
      font-weight: 600;
      background: rgba(255, 255, 255, 0.05);
    }
    .status-value.applied { color: #3b82f6; background: rgba(59, 130, 246, 0.1); }
    .status-value.shortlisted { color: #22c55e; background: rgba(34, 197, 94, 0.1); }
    .status-value.rejected { color: #ef4444; background: rgba(239, 68, 68, 0.1); }

    .actions { display: flex; gap: 20px; }
    .btn-text {
      background: transparent;
      border: none;
      color: #3b82f6;
      font-weight: 600;
      cursor: pointer;
    }
    .btn-text.delete { color: #ef4444; }
    .btn-text.success { color: #22c55e; }
    .status-hint { font-size: 0.85rem; color: #94a3b8; font-style: italic; }
    .empty-state { padding: 60px; text-align: center; }
    .empty-state a { color: #3b82f6; text-decoration: none; font-weight: 600; }
  `]
})
export class CandidateDashboardComponent implements OnInit {
  private appService = inject(ApplicationService);
  private authService = inject(AuthService);
  private interviewService = inject(InterviewService);
  private jobService = inject(JobService);
  private router = inject(Router);

  applications = signal<any[]>([]);
  interviews = signal<any[]>([]);
  savedJobs = signal<any[]>([]);
  jobTitles = signal<Record<number, string>>({});
  isLoading = signal(true);
  shortlistedCount = signal(0);

  ngOnInit() {
    this.fetchData();
    this.loadSavedJobs();
  }

  fetchData() {
    const userId = this.authService.currentUser()?.userId;
    if (!userId) return;

    this.appService.getApplicationsByCandidate(userId).subscribe({
      next: (data) => {
        this.applications.set(data);
        this.shortlistedCount.set(data.filter(a => a.status === 'Shortlisted').length);
        this.isLoading.set(false);
        this.fetchJobTitles(data);
      },
      error: () => this.isLoading.set(false)
    });

    // Fetch interviews for each application
    this.appService.getApplicationsByCandidate(userId).subscribe({
      next: (apps) => {
        this.interviews.set([]); // Clear existing
        apps.forEach(app => {
          this.interviewService.getInterviewsByApplication(app.applicationId).subscribe({
            next: (ints) => {
              if (ints.length > 0) {
                // Only take the latest interview for this application
                const latest = ints[ints.length - 1];
                const enriched = { ...latest, jobId: app.jobId };
                this.interviews.update(current => [...current, enriched]);
              }
            }
          });
        });
      }
    });
  }

  fetchJobTitles(apps: any[]) {
    apps.forEach(app => {
      this.jobService.getJobById(app.jobId).subscribe({
        next: (job: any) => {
          this.jobTitles.update(titles => ({ ...titles, [app.jobId]: job.title }));
        }
      });
    });
  }

  loadSavedJobs() {
    const saved = localStorage.getItem('hc_saved_jobs');
    if (saved) {
      this.savedJobs.set(JSON.parse(saved));
    }
  }

  removeSavedJob(jobId: number) {
    const updated = this.savedJobs().filter(j => j.jobId !== jobId);
    this.savedJobs.set(updated);
    localStorage.setItem('hc_saved_jobs', JSON.stringify(updated));
  }

  viewDetails(jobId: number) {
    this.router.navigate(['/jobs', jobId]);
  }

  withdraw(appId: number) {
    if (confirm('Are you sure you want to withdraw this application?')) {
      this.appService.deleteApplication(appId).subscribe({
        next: () => {
          this.applications.update(apps => apps.filter(a => a.applicationId !== appId));
          alert('Application withdrawn successfully.');
        },
        error: () => alert('Failed to withdraw application.')
      });
    }
  }

  confirmInterview(id: number) {
    this.interviewService.confirmInterview(id).subscribe({
      next: () => {
        alert('Interview confirmed!');
        this.fetchData();
      },
      error: () => alert('Failed to confirm interview.')
    });
  }

  cancelInterview(interview: any) {
    if (confirm('Are you sure you want to decline this interview? This will also withdraw your application.')) {
      this.interviewService.cancelInterview(interview.interviewId).subscribe({
        next: () => {
          // Also update application status to 'Withdrawn'
          this.appService.updateStatus(interview.applicationId, 'Withdrawn').subscribe({
            next: () => {
              alert('Interview declined and application withdrawn.');
              this.fetchData();
            }
          });
        },
        error: () => alert('Failed to decline interview.')
      });
    }
  }
}
