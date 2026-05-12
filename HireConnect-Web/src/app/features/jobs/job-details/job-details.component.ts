import { Component, OnInit, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { JobService } from '../../../core/services/job/job.service';
import { ApplicationService } from '../../../core/services/application/application.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ProfileService } from '../../../core/services/profile/profile.service';
import { Job } from '../../../core/models/job.models';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-job-details',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="job-details-wrapper fade-in" *ngIf="job() as j">
      <div class="back-nav">
        <a routerLink="/jobs" class="btn-text">← Back to Job Listings</a>
      </div>

      <div class="job-header-card glass">
        <div class="header-main">
          <div class="logo-box">🏢</div>
          <div class="title-box">
            <h1 class="job-title">{{ j.title }}</h1>
            <p class="company-name">Posted by <span class="highlight">{{ recruiterName() || 'Recruiter #' + j.postedBy }}</span></p>
          </div>
          <div class="badge-box">
            <span class="badge">{{ j.type }}</span>
            <span class="badge secondary">{{ j.category }}</span>
          </div>
        </div>

        <div class="header-stats">
          <div class="stat">
            <span class="icon">📍</span>
            <span>{{ j.location }}</span>
          </div>
          <div class="stat">
            <span class="icon">💰</span>
            <span>{{ j.salaryMin | currency }} - {{ j.salaryMax | currency }}</span>
          </div>
          <div class="stat">
            <span class="icon">📅</span>
            <span>Posted {{ j.postedAt | date }}</span>
          </div>
        </div>

        <div class="header-actions" *ngIf="!authService.isRecruiter()">
          <button class="btn-primary large" (click)="toggleApplyForm()" [disabled]="applied()">
            {{ applied() ? 'Applied ✓' : 'Apply for this Job' }}
          </button>
          <button class="btn-outline large" (click)="saveJob()">Save Job</button>
        </div>
      </div>

      <div class="apply-form-container glass fade-in" *ngIf="showApplyForm()">
        <h3>Submit Your Application</h3>
        <div class="form-group">
          <label>Resume Link (Google Drive, Dropbox, etc.)</label>
          <input type="url" [(ngModel)]="resumeUrl" placeholder="https://..." class="form-input">
        </div>
        <div class="form-group">
          <label>Cover Letter</label>
          <textarea [(ngModel)]="coverLetter" rows="4" placeholder="Why are you a good fit?" class="form-input"></textarea>
        </div>
        <div class="form-actions">
          <button class="btn-primary" (click)="submitApplication()" [disabled]="isSubmitting() || !resumeUrl">
            {{ isSubmitting() ? 'Submitting...' : 'Confirm Application' }}
          </button>
          <button class="btn-text" (click)="toggleApplyForm()">Cancel</button>
        </div>
      </div>

      <div class="job-body">
        <div class="description-section glass">
          <h3>Job Description</h3>
          <div class="text-content">
            {{ j.description }}
          </div>
        </div>

        <div class="sidebar">
          <div class="skills-card glass">
            <h3>Required Skills</h3>
            <div class="skills-list">
              <span class="skill-tag" *ngFor="let skill of j.skills">{{ skill }}</span>
            </div>
          </div>

          <div class="info-card glass">
            <h3>Job Overview</h3>
            <div class="info-item">
              <span class="label">Experience</span>
              <span class="value">{{ j.experienceRequired }}</span>
            </div>
            <div class="info-item">
              <span class="label">Job Type</span>
              <span class="value">{{ j.type }}</span>
            </div>
          </div>
        </div>
      </div>
    </div>

    <div class="loading-state" *ngIf="isLoading()">
      <div class="spinner"></div>
      <p>Loading job details...</p>
    </div>
  `,
  styles: [`
    .job-details-wrapper {
      max-width: 1100px;
      margin: 40px auto;
      padding: 0 20px;
    }
    .back-nav { margin-bottom: 25px; }
    .btn-text { color: #3b82f6; text-decoration: none; font-weight: 600; }
    
    .job-header-card { padding: 40px; margin-bottom: 30px; }
    .header-main { display: flex; align-items: center; gap: 25px; margin-bottom: 30px; }
    .logo-box { width: 80px; height: 80px; background: rgba(59, 130, 246, 0.1); border-radius: 16px; display: flex; align-items: center; justify-content: center; font-size: 2.5rem; }
    .title-box { flex: 1; }
    .job-title { font-size: 2.5rem; font-weight: 800; margin-bottom: 5px; }
    .company-name { color: #94a3b8; font-size: 1.1rem; }
    .badge-box { display: flex; gap: 10px; }
    .badge { padding: 6px 16px; border-radius: 20px; font-size: 0.85rem; font-weight: 600; background: rgba(59, 130, 246, 0.1); color: #3b82f6; }
    .badge.secondary { background: rgba(139, 92, 246, 0.1); color: #8b5cf6; }

    .header-stats { display: flex; gap: 40px; padding: 25px 0; border-top: 1px solid rgba(255, 255, 255, 0.05); border-bottom: 1px solid rgba(255, 255, 255, 0.05); margin-bottom: 30px; }
    .stat { display: flex; align-items: center; gap: 10px; color: #94a3b8; }
    .stat .icon { font-size: 1.2rem; }

    .header-actions { display: flex; gap: 20px; }
    .btn-primary.large, .btn-outline.large { padding: 15px 40px; font-size: 1.1rem; border-radius: 12px; }

    .job-body { display: grid; grid-template-columns: 1fr 350px; gap: 30px; }
    .description-section { padding: 40px; }
    .description-section h3 { margin-bottom: 25px; font-size: 1.5rem; }
    .text-content { color: #cbd5e1; line-height: 1.8; white-space: pre-line; }

    .sidebar { display: flex; flex-direction: column; gap: 20px; }
    .skills-card, .info-card { padding: 30px; }
    .skills-card h3, .info-card h3 { font-size: 1.2rem; margin-bottom: 20px; }
    .skills-list { display: flex; flex-wrap: wrap; gap: 10px; }
    .skill-tag { background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); padding: 6px 14px; border-radius: 8px; font-size: 0.85rem; }

    .info-item .value { font-weight: 600; }
    .highlight { color: #3b82f6; font-weight: 600; }

    .apply-form-container { padding: 30px; margin-bottom: 30px; border: 1px solid var(--primary); }
    .apply-form-container h3 { margin-bottom: 20px; font-size: 1.5rem; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; margin-bottom: 8px; font-weight: 500; }
    .form-input { width: 100%; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 8px; padding: 12px; color: white; outline: none; }
    .form-input:focus { border-color: #3b82f6; }
    .form-actions { display: flex; gap: 15px; align-items: center; }
    .btn-text { background: transparent; border: none; color: #94a3b8; cursor: pointer; font-weight: 600; }

    .loading-state { text-align: center; padding: 100px; }
    .spinner { width: 50px; height: 50px; border: 4px solid rgba(59, 130, 246, 0.1); border-top-color: #3b82f6; border-radius: 50%; animation: spin 1s linear infinite; margin: 0 auto 20px; }
    @keyframes spin { to { transform: rotate(360deg); } }
  `]
})
export class JobDetailsComponent implements OnInit {
  private route = inject(ActivatedRoute);
  private router = inject(Router);
  private jobService = inject(JobService);
  private appService = inject(ApplicationService);
  private profileService = inject(ProfileService);
  public authService = inject(AuthService);

  job = signal<Job | null>(null);
  recruiterName = signal<string>('');
  isLoading = signal(true);
  showApplyForm = signal(false);
  isSubmitting = signal(false);
  applied = signal(false);

  resumeUrl = '';
  coverLetter = '';

  ngOnInit() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.fetchJobDetails(+id);
    }
  }

  fetchJobDetails(id: number) {
    this.jobService.getJobById(id).subscribe({
      next: (data) => {
        this.job.set(data);
        this.isLoading.set(false);
        this.fetchRecruiterName(data.postedBy);
      },
      error: () => this.isLoading.set(false)
    });
  }

  fetchRecruiterName(userId: number) {
    this.profileService.getRecruiterProfile(userId).subscribe({
      next: (res: any) => {
        // Handle both camelCase and PascalCase
        const name = res.fullName || res.FullName || res.companyName || res.CompanyName;
        if (name) {
          this.recruiterName.set(name);
        } else {
          this.recruiterName.set(`Recruiter #${userId}`);
        }
      },
      error: (err) => {
        console.error('Failed to fetch recruiter profile:', err);
        this.recruiterName.set(`Recruiter #${userId}`);
      }
    });
  }

  toggleApplyForm() {
    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }
    
    if (!this.authService.isCandidate()) {
      alert('Only candidates can apply for jobs.');
      return;
    }

    this.showApplyForm.set(!this.showApplyForm());
  }

  saveJob() {
    const j = this.job();
    if (!j) return;

    if (!this.authService.isAuthenticated()) {
      this.router.navigate(['/auth/login'], { queryParams: { returnUrl: this.router.url } });
      return;
    }

    const savedJobsJson = localStorage.getItem('hc_saved_jobs');
    let savedJobs: any[] = savedJobsJson ? JSON.parse(savedJobsJson) : [];

    if (savedJobs.some(job => job.jobId === j.jobId)) {
      alert('Job is already saved!');
      return;
    }

    savedJobs.push({
      jobId: j.jobId,
      title: j.title,
      companyName: this.recruiterName() || 'Unknown Company',
      location: j.location
    });

    localStorage.setItem('hc_saved_jobs', JSON.stringify(savedJobs));
    alert('Job saved to your profile!');
  }

  submitApplication() {
    const userId = this.authService.currentUser()?.userId;
    const job = this.job();
    
    if (userId && job) {
      this.isSubmitting.set(true);
      this.appService.submitApplication({
        jobId: job.jobId,
        jobTitle: job.title,
        recruiterId: job.postedBy,
        candidateId: userId,
        candidateEmail: this.authService.currentUser()?.email || 'candidate@example.com',
        resumeUrl: this.resumeUrl,
        coverLetter: this.coverLetter
      }).subscribe({
        next: () => {
          this.isSubmitting.set(false);
          this.showApplyForm.set(false);
          this.applied.set(true);
          alert('Application submitted successfully!');
        },
        error: (err) => {
          this.isSubmitting.set(false);
          alert(err.error?.message || 'Failed to submit application.');
        }
      });
    }
  }
}
