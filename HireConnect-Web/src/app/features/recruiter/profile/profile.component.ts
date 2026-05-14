import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../../core/services/profile/profile.service';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-recruiter-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="profile-container fade-in">
      <div class="header">
        <h2 class="title">Recruiter <span class="gradient-text">Profile</span></h2>
        <p class="subtitle">Update your company and personal details to attract top talent.</p>
      </div>

      <div class="profile-card glass">
        @if (isLoading) {
          <div class="loading">Loading profile...</div>
        } @else {
          <form [formGroup]="profileForm" (ngSubmit)="onSubmit()" class="profile-form">
            
            <h3 class="section-heading">Personal Information</h3>
            <div class="form-row">
              <div class="form-group">
                <label>Full Name</label>
                <input type="text" formControlName="fullName" class="form-input" placeholder="John Doe">
              </div>
              <div class="form-group">
                <label>Email Address</label>
                <input type="email" formControlName="email" class="form-input" placeholder="john@example.com" readonly>
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Mobile Number</label>
                <input type="text" formControlName="mobile" class="form-input" placeholder="+1 234 567 8900">
              </div>
              <div class="form-group">
                <label>Bio / Designation</label>
                <input type="text" formControlName="bio" class="form-input" placeholder="Senior Tech Recruiter">
              </div>
            </div>

            <h3 class="section-heading mt-4">Company Details</h3>
            <div class="form-row">
              <div class="form-group">
                <label>Company Name</label>
                <input type="text" formControlName="companyName" class="form-input" placeholder="TechCorp Inc.">
              </div>
              <div class="form-group">
                <label>Industry</label>
                <input type="text" formControlName="industry" class="form-input" placeholder="Information Technology">
              </div>
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Company Size</label>
                <select formControlName="companySize" class="form-input">
                  <option value="">Select Size</option>
                  <option value="1-10">1-10 Employees</option>
                  <option value="11-50">11-50 Employees</option>
                  <option value="51-200">51-200 Employees</option>
                  <option value="201-500">201-500 Employees</option>
                  <option value="500+">500+ Employees</option>
                </select>
              </div>
              <div class="form-group">
                <label>Company Website</label>
                <input type="url" formControlName="website" class="form-input" placeholder="https://techcorp.com">
              </div>
            </div>

            <div class="form-group">
              <label>Company Logo URL</label>
              <input type="url" formControlName="logoUrl" class="form-input" placeholder="https://...">
            </div>

            <div class="form-actions">
              <button type="submit" class="btn-primary large" [disabled]="profileForm.invalid || isSaving">
                {{ isSaving ? 'Saving...' : 'Save Profile' }}
              </button>
              <span *ngIf="saveSuccess" class="success-msg">Profile updated successfully!</span>
            </div>
          </form>
        }
      </div>
    </div>
  `,
  styles: [`
    .profile-container { max-width: 800px; margin: 40px auto; padding: 0 20px; }
    .header { margin-bottom: 30px; text-align: center; }
    .title { font-size: 2.5rem; font-weight: 700; }
    .gradient-text { background: linear-gradient(135deg, #06b6d4, #3b82f6); -webkit-background-clip: text; -webkit-text-fill-color: transparent; }
    .subtitle { color: #94a3b8; margin-top: 10px; }

    .profile-card { padding: 40px; }
    .section-heading { margin-bottom: 15px; font-size: 1.2rem; color: #f8fafc; border-bottom: 1px solid rgba(255,255,255,0.1); padding-bottom: 5px; }
    .mt-4 { margin-top: 30px; }
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; margin-bottom: 8px; font-weight: 500; font-size: 0.9rem; color: #cbd5e1; }
    .form-input { width: 100%; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 8px; padding: 12px; color: white; outline: none; transition: border-color 0.3s; }
    .form-input:focus { border-color: #3b82f6; }
    select.form-input { appearance: none; cursor: pointer; }
    select.form-input option { background-color: #1e293b; color: white; }
    
    .form-actions { display: flex; align-items: center; gap: 20px; margin-top: 20px; }
    .btn-primary.large { padding: 12px 30px; font-size: 1rem; }
    .success-msg { color: #22c55e; font-weight: 500; }
    .loading { text-align: center; padding: 40px; color: #94a3b8; }
  `]
})
export class RecruiterProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);

  profileForm = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    mobile: [''],
    bio: [''],
    companyName: [''],
    industry: [''],
    companySize: [''],
    website: [''],
    logoUrl: ['']
  });

  isLoading = true;
  isSaving = false;
  saveSuccess = false;
  profileExists = false;
  userId: number | null = null;

  ngOnInit() {
    this.userId = this.authService.currentUser()?.userId || null;
    const userEmail = this.authService.currentUser()?.email || '';
    
    // Pre-fill email
    this.profileForm.patchValue({ email: userEmail });

    if (this.userId) {
      this.profileService.getRecruiterProfile(this.userId).subscribe({
        next: (profile) => {
          if (profile) {
            this.profileExists = true;
            this.profileForm.patchValue({
              fullName: profile.fullName,
              email: profile.email,
              mobile: profile.mobile,
              bio: profile.bio,
              companyName: profile.companyName,
              industry: profile.industry,
              companySize: profile.companySize,
              website: profile.website,
              logoUrl: profile.logoUrl
            });
          }
          this.isLoading = false;
        },
        error: (err) => {
          console.log('Profile not found, recruiter can create one.');
          this.profileExists = false;
          this.isLoading = false;
        }
      });
    }
  }

  onSubmit() {
    if (this.profileForm.invalid || !this.userId) return;

    this.isSaving = true;
    this.saveSuccess = false;

    const formValue = this.profileForm.value;
    
    const profileData: any = {
      fullName: formValue.fullName,
      mobile: formValue.mobile || '',
      bio: formValue.bio || '',
      companyName: formValue.companyName || '',
      industry: formValue.industry || '',
      companySize: formValue.companySize || '',
      website: formValue.website || '',
      logoUrl: formValue.logoUrl || '',
      addresses: []
    };

    if (!this.profileExists) {
      profileData.userId = this.userId;
      profileData.email = formValue.email;
    }

    const request = this.profileExists 
      ? this.profileService.updateRecruiterProfile(profileData)
      : this.profileService.createRecruiterProfile(profileData);

    request.subscribe({
      next: () => {
        this.isSaving = false;
        this.saveSuccess = true;
        this.profileExists = true;
        setTimeout(() => this.saveSuccess = false, 3000);
      },
      error: (err) => {
        this.isSaving = false;
        console.error('Profile Update Error:', err);
        const errorMsg = err.error?.message || err.error || 'Failed to update profile';
        alert(errorMsg);
      }
    });
  }
}
