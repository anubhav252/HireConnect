import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { JobService } from '../../../core/services/job/job.service';
import { AuthService } from '../../../core/services/auth/auth.service';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-post-job',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule, RouterLink],
  template: `
    <div class="post-job-wrapper fade-in">
      <div class="form-card glass">
        <div class="header">
          <h2 class="title">@if(isEditMode){Edit} @else {Post a} <span class="gradient-text">@if(isEditMode){Job Posting} @else {New Opportunity}</span></h2>
          <p class="subtitle">@if(isEditMode){Update your job details to attract more candidates.} @else {Reach out to thousands of candidates across the platform.}</p>
        </div>

        <form [formGroup]="jobForm" (ngSubmit)="onSubmit()" class="job-form">
          <div class="form-row">
            <div class="form-group">
              <label>Job Title</label>
              <input type="text" formControlName="title" placeholder="e.g. Senior Backend Engineer" class="form-input">
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Category</label>
              <select formControlName="category" class="form-input">
                <option value="Technology">Technology</option>
                <option value="Design">Design</option>
                <option value="Marketing">Marketing</option>
                <option value="Management">Management</option>
                <option value="Sales">Sales</option>
                <option value="Customer Service">Customer Service</option>
                <option value="Finance">Finance</option>
                <option value="Healthcare">Healthcare</option>
                <option value="Education">Education</option>
                <option value="Human Resources">Human Resources</option>
                <option value="Legal">Legal</option>
              </select>
            </div>
            <div class="form-group">
              <label>Job Type</label>
              <select formControlName="type" class="form-input">
                <option value="Full-time">Full-time</option>
                <option value="Part-time">Part-time</option>
                <option value="Contract">Contract</option>
                <option value="Remote">Remote</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Location</label>
              <input type="text" formControlName="location" placeholder="e.g. Remote / New York" class="form-input">
            </div>
            <div class="form-group">
              <label>Experience Required (Years)</label>
              <input type="number" formControlName="experienceRequired" class="form-input" placeholder="e.g. 2">
            </div>
          </div>

          <div class="form-row">
            <div class="form-group">
              <label>Min Salary ($)</label>
              <input type="number" formControlName="salaryMin" class="form-input">
            </div>
            <div class="form-group">
              <label>Max Salary ($)</label>
              <input type="number" formControlName="salaryMax" class="form-input">
            </div>
          </div>


          <div class="form-group">
            <label>Description</label>
            <textarea formControlName="description" rows="5" placeholder="Detailed job description..." class="form-input"></textarea>
          </div>

          <div class="form-group">
            <label>Required Skills (comma separated)</label>
            <input type="text" formControlName="skills" placeholder="e.g. C#, .NET, SQL, Docker" class="form-input">
          </div>

          <div class="actions">
            <button type="button" class="btn-outline" routerLink="/recruiter/dashboard">Cancel</button>
            <button type="submit" class="btn-primary" [disabled]="jobForm.invalid || isLoading">
              {{ isLoading ? (isEditMode ? 'Updating...' : 'Publishing...') : (isEditMode ? 'Update Job' : 'Publish Job') }}
            </button>
          </div>
        </form>
      </div>
    </div>
  `,
  styles: [`
    .post-job-wrapper {
      max-width: 900px;
      margin: 40px auto;
      padding: 0 20px;
    }
    .form-card { padding: 40px; }
    .header { text-align: center; margin-bottom: 40px; }
    .title { font-size: 2.5rem; font-weight: 700; }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #06b6d4);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .job-form { display: flex; flex-direction: column; gap: 20px; }
    .form-row { display: flex; gap: 20px; }
    .form-group { flex: 1; display: flex; flex-direction: column; gap: 8px; }
    .form-group label { font-size: 0.9rem; font-weight: 500; color: #94a3b8; }
    .form-input {
      background: rgba(255, 255, 255, 0.05);
      border: 1px solid rgba(255, 255, 255, 0.1);
      border-radius: 8px;
      padding: 12px;
      color: white;
      outline: none;
      font-size: 1rem;
    }
    .form-input:focus { border-color: #3b82f6; }
    textarea.form-input { resize: vertical; }

    /* Fix for dropdown visibility */
    select.form-input option {
      background-color: #1e293b;
      color: white;
    }
    
    .actions {
      display: flex;
      justify-content: flex-end;
      gap: 15px;
      margin-top: 20px;
      padding-top: 20px;
      border-top: 1px solid rgba(255, 255, 255, 0.05);
    }
  `]
})
export class PostJobComponent {
  private fb = inject(FormBuilder);
  private jobService = inject(JobService);
  private authService = inject(AuthService);
  private router = inject(Router);
  private route = inject(ActivatedRoute);

  isLoading = false;
  isEditMode = false;
  jobId: number | null = null;

  categories = ['Technology', 'Design', 'Marketing', 'Management', 'Sales', 'Customer Service', 'Finance', 'Healthcare', 'Education', 'Human Resources', 'Legal'];
  jobTypes = ['Full-time', 'Part-time', 'Contract', 'Remote'];

  jobForm = this.fb.group({
    title: ['', [Validators.required]],
    category: ['Technology', [Validators.required]],
    type: ['Full-time', [Validators.required]],
    location: ['', [Validators.required]],
    salaryMin: [0, [Validators.required]],
    salaryMax: [0, [Validators.required]],
    description: ['', [Validators.required]],
    skills: ['', [Validators.required]],
    experienceRequired: [0, [Validators.required, Validators.min(0)]]
  });

  ngOnInit() {
    this.jobId = Number(this.route.snapshot.paramMap.get('id'));
    if (this.jobId) {
      this.isEditMode = true;
      this.loadJobData(this.jobId);
    }
  }

  loadJobData(id: number) {
    this.jobService.getJobById(id).subscribe(job => {
      this.jobForm.patchValue({
        ...job,
        skills: Array.isArray(job.skills) ? job.skills.join(', ') : job.skills
      });
    });
  }

  onSubmit() {
    if (this.jobForm.invalid) return;

    this.isLoading = true;
    const userId = this.authService.currentUser()?.userId;
    if (!userId) return;

    const formValue = this.jobForm.value;
    const jobData: any = {
      ...formValue,
      skills: formValue.skills ? formValue.skills.split(',').map((s: string) => s.trim()) : [],
      experienceRequired: Number(formValue.experienceRequired),
      postedBy: userId,
      status: 'Active',
      postedAt: new Date().toISOString()
    };

    const request = this.isEditMode && this.jobId
      ? this.jobService.updateJob(this.jobId, jobData)
      : this.jobService.createJob(jobData);

    request.subscribe({
      next: () => {
        this.router.navigate(['/recruiter/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        alert(`Failed to ${this.isEditMode ? 'update' : 'post'} job. Please try again.`);
      }
    });
  }
}
