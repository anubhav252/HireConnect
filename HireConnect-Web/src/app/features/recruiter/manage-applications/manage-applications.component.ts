import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { Application } from '../../../core/models/application.models';
import { ApplicationService } from '../../../core/services/application/application.service';
import { InterviewService } from '../../../core/services/interview/interview.service';
import { JobService } from '../../../core/services/job/job.service';
import { ProfileService } from '../../../core/services/profile/profile.service';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-manage-applications',
  standalone: true,
  imports: [CommonModule, RouterLink],
  template: `
    <div class="manage-apps-wrapper fade-in">
      <div class="header">
        <a routerLink="/recruiter/dashboard" class="btn-text">← Back to Dashboard</a>
        <h2 class="title">Manage <span class="gradient-text">Applications</span></h2>
        <p class="subtitle">Review applicants for: <strong>{{ jobTitle() || 'Job #' + jobId }}</strong></p>
      </div>

      <div class="apps-list glass">
        @if (isLoading()) {
          <div class="loading">Loading applications...</div>
        } @else if (applications().length === 0) {
          <div class="empty">No applications yet for this job.</div>
        } @else {
          <table class="dashboard-table">
            <thead>
              <tr>
                <th>Candidate Name</th>
                <th>Applied Date</th>
                <th>Resume</th>
                <th>Status</th>
                <th>Actions</th>
              </tr>
            </thead>
            <tbody>
              @for (app of applications(); track app.applicationId) {
                <tr>
                  <td>
                    <div class="candidate-info">
                      <strong>{{ candidateNames()[app.candidateId] || 'Loading...' }}</strong>
                      <span class="id-hint">ID: {{ app.candidateId }}</span>
                    </div>
                  </td>
                  <td>{{ app.appliedAt | date }}</td>
                  <td><a [href]="app.resumeUrl" target="_blank" class="link-primary">View Resume</a></td>
                  <td>
                    <span class="status-badge" [class]="app.status.toLowerCase()">{{ app.status }}</span>
                  </td>
                  <td>
                    <div class="action-buttons">
                      @if (app.status === 'Applied') {
                        <button class="btn-icon success" (click)="updateStatus(app.applicationId, 'Shortlisted')" title="Shortlist" [disabled]="isProcessing(app.applicationId)">✓</button>
                        <button class="btn-icon delete" (click)="updateStatus(app.applicationId, 'Rejected')" title="Reject" [disabled]="isProcessing(app.applicationId)">✕</button>
                      } @else if (app.status === 'Shortlisted') {
                        <div class="schedule-form">
                          <input type="datetime-local" #interviewDate class="date-input">
                          <button class="btn-primary btn-sm" (click)="scheduleInterview(app, interviewDate.value)" 
                                  [disabled]="isProcessing(app.applicationId)"
                                  style="padding: 4px 10px; font-size: 0.8rem;">
                            {{ isProcessing(app.applicationId) ? 'Scheduling...' : 'Schedule' }}
                          </button>
                          <button class="btn-icon delete" (click)="updateStatus(app.applicationId, 'Rejected')" title="Reject" [disabled]="isProcessing(app.applicationId)">✕</button>
                        </div>
                      } @else if (app.status === 'Interviewing' || app.status === 'Scheduled') {
                        <div class="status-actions">
                          <span class="status-hint">Interview Sent</span>
                          <div class="interview-ops" style="display: flex; gap: 8px; margin-top: 5px;">
                            <button class="btn-text btn-sm delete" (click)="cancelInterview(app.applicationId)" style="font-size: 0.7rem; color: #ef4444;">Cancel Schedule</button>
                            <button class="btn-icon delete" (click)="updateStatus(app.applicationId, 'Rejected')" title="Reject Candidate" style="width: 28px; height: 28px; font-size: 0.7rem;">✕</button>
                          </div>
                        </div>
                      }
                    </div>
                  </td>
                </tr>
              }
            </tbody>
          </table>
        }
      </div>
    </div>
  `,
  styles: [`
    .manage-apps-wrapper { max-width: 1000px; margin: 40px auto; padding: 0 20px; }
    .header { margin-bottom: 30px; }
    .btn-text { color: #3b82f6; text-decoration: none; font-weight: 600; display: inline-block; margin-bottom: 20px; }
    .title { font-size: 2.5rem; font-weight: 700; }
    .gradient-text { background: linear-gradient(135deg, #06b6d4, #3b82f6); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .apps-list { overflow: hidden; padding: 10px; }
    .dashboard-table { width: 100%; border-collapse: collapse; text-align: left; }
    .dashboard-table th { padding: 15px 20px; font-size: 0.9rem; color: #94a3b8; border-bottom: 1px solid rgba(255,255,255,0.1); }
    .dashboard-table td { padding: 15px 20px; border-bottom: 1px solid rgba(255, 255, 255, 0.05); }
    
    .link-primary { color: #3b82f6; text-decoration: none; font-weight: 500; }
    .link-primary:hover { text-decoration: underline; }

    .status-badge { padding: 4px 10px; border-radius: 20px; font-size: 0.75rem; background: rgba(255, 255, 255, 0.1); }
    .status-badge.applied { background: rgba(59, 130, 246, 0.1); color: #3b82f6; }
    .status-badge.shortlisted { background: rgba(34, 197, 94, 0.1); color: #22c55e; }
    .status-badge.rejected { background: rgba(239, 68, 68, 0.1); color: #ef4444; }

    .action-buttons { display: flex; gap: 10px; }
    .btn-icon { background: rgba(255, 255, 255, 0.05); border: none; color: white; width: 35px; height: 35px; border-radius: 6px; cursor: pointer; display: flex; align-items: center; justify-content: center; transition: all 0.2s; }
    .btn-icon.success:hover { background: rgba(34, 197, 94, 0.2); color: #22c55e; }
    .btn-icon.delete:hover { background: rgba(239, 68, 68, 0.2); color: #ef4444; }

    .loading, .empty { padding: 40px; text-align: center; color: #94a3b8; }
    .candidate-info { display: flex; flex-direction: column; }
    .id-hint { font-size: 0.7rem; color: #64748b; }
    .status-hint { font-size: 0.8rem; color: #22c55e; font-weight: 600; }
    button:disabled { opacity: 0.5; cursor: not-allowed; }
    .schedule-form { display: flex; gap: 5px; align-items: center; }
    .date-input { background: rgba(255,255,255,0.05); border: 1px solid rgba(255,255,255,0.1); color: white; padding: 4px; border-radius: 4px; font-size: 0.8rem; }
    .status-actions { display: flex; flex-direction: column; align-items: center; }
  `]
})
export class ManageApplicationsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private appService = inject(ApplicationService);
  private jobService = inject(JobService);
  private profileService = inject(ProfileService);
  private interviewService = inject(InterviewService);
  private authService = inject(AuthService);

  jobId: number = 0;
  jobTitle = signal<string>('');
  applications = signal<Application[]>([]);
  candidateNames = signal<Record<number, string>>({});
  processingIds = signal<Set<number>>(new Set());
  isLoading = signal(true);

  isProcessing(id: number) {
    return this.processingIds().has(id);
  }

  ngOnInit() {
    this.jobId = +(this.route.snapshot.paramMap.get('id') || 0);
    if (this.jobId) {
      this.fetchJobDetails();
      this.fetchApplications();
    }
  }

  fetchJobDetails() {
    this.jobService.getJobById(this.jobId).subscribe(job => {
      this.jobTitle.set(job.title);
    });
  }

  fetchApplications() {
    this.appService.getApplicationsByJob(this.jobId).subscribe({
      next: (data: any) => {
        this.applications.set(data);
        this.isLoading.set(false);
        this.fetchCandidateNames(data);
      },
      error: () => this.isLoading.set(false)
    });
  }

  fetchCandidateNames(apps: Application[]) {
    apps.forEach(app => {
      this.profileService.getCandidateProfile(app.candidateId).subscribe(profile => {
        this.candidateNames.update(names => ({ ...names, [app.candidateId]: profile.fullName }));
      });
    });
  }

  updateStatus(appId: number, status: string) {
    if (this.isProcessing(appId)) return;
    
    // Check if status is already the same
    const currentApp = this.applications().find(a => a.applicationId === appId);
    if (currentApp?.status === status) return;

    this.processingIds.update(set => new Set(set).add(appId));

    this.appService.updateStatus(appId, status).subscribe({
      next: () => {
        const updated = this.applications().map(a => 
          a.applicationId === appId ? { ...a, status } : a
        );
        this.applications.set(updated);
        this.processingIds.update(set => {
          const newSet = new Set(set);
          newSet.delete(appId);
          return newSet;
        });
      },
      error: () => {
        this.processingIds.update(set => {
          const newSet = new Set(set);
          newSet.delete(appId);
          return newSet;
        });
      }
    });
  }

  scheduleInterview(app: Application, dateValue: string) {
    if (!dateValue) {
      alert('Please select a date and time for the interview.');
      return;
    }

    const scheduledAt = new Date(dateValue);
    const recruiterId = this.authService.currentUser()?.userId || 0;
    
    if (this.isProcessing(app.applicationId)) return;
    this.processingIds.update(set => new Set(set).add(app.applicationId));

    this.interviewService.scheduleInterview({
      jobId: this.jobId,
      applicationId: app.applicationId,
      candidateId: app.candidateId,
      recruiterId: recruiterId,
      scheduledAt: scheduledAt.toISOString(),
      meetLink: 'https://meet.google.com/new',
      mode: 'Online',
      notes: 'Initial HR Screening'
    }).subscribe({
      next: () => {
        alert('Interview scheduled successfully!');
        this.appService.updateStatus(app.applicationId, 'Interviewing').subscribe(() => {
          const updated = this.applications().map(a => 
            a.applicationId === app.applicationId ? { ...a, status: 'Interviewing' } : a
          );
          this.applications.set(updated);
          this.processingIds.update(set => {
            const newSet = new Set(set);
            newSet.delete(app.applicationId);
            return newSet;
          });
        });
      },
      error: () => {
        alert('Failed to schedule interview.');
        this.processingIds.update(set => {
          const newSet = new Set(set);
          newSet.delete(app.applicationId);
          return newSet;
        });
      }
    });
  }

  cancelInterview(appId: number) {
    if (confirm('Are you sure you want to cancel this interview?')) {
      this.processingIds.update(set => new Set(set).add(appId));
      
      this.interviewService.getInterviewsByApplication(appId).subscribe({
        next: (interviews) => {
          const activeInterview = interviews.find(i => i.status !== 'Cancelled');
          if (activeInterview) {
            this.interviewService.cancelInterview(activeInterview.interviewId).subscribe({
              next: () => {
                // Manually update the signal immediately for instant UI feedback
                this.applications.update(apps => 
                  apps.map(a => a.applicationId === appId ? { ...a, status: 'Shortlisted' } : a)
                );
                
                // Still call backend to sync
                this.appService.updateStatus(appId, 'Shortlisted').subscribe();
                
                this.processingIds.update(set => {
                  const newSet = new Set(set);
                  newSet.delete(appId);
                  return newSet;
                });
                alert('Interview cancelled and moved back to shortlist.');
              },
              error: () => {
                alert('Failed to cancel interview.');
                this.processingIds.update(set => {
                  const newSet = new Set(set);
                  newSet.delete(appId);
                  return newSet;
                });
              }
            });
          } else {
            // If no active interview found, just revert status
            this.updateStatus(appId, 'Shortlisted');
            this.processingIds.update(set => {
              const newSet = new Set(set);
              newSet.delete(appId);
              return newSet;
            });
          }
        },
        error: (err) => {
          console.error('Interview lookup failed', err);
          if (confirm('Could not connect to Interview service. Would you like to force-revert the status to Shortlisted anyway?')) {
             this.applications.update(apps => 
               apps.map(a => a.applicationId === appId ? { ...a, status: 'Shortlisted' } : a)
             );
             this.appService.updateStatus(appId, 'Shortlisted').subscribe();
          }
          this.processingIds.update(set => {
            const newSet = new Set(set);
            newSet.delete(appId);
            return newSet;
          });
        }
      });
    }
  }
}
