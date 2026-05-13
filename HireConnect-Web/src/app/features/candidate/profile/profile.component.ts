import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProfileService } from '../../../core/services/profile/profile.service';
import { AuthService } from '../../../core/services/auth/auth.service';

@Component({
  selector: 'app-profile',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
    <div class="profile-container fade-in">
      <div class="header">
        <h2 class="title">My <span class="gradient-text">Profile</span></h2>
        <p class="subtitle">Update your personal information and resume to stand out.</p>
      </div>

      <div class="profile-card glass">
        @if (isLoading) {
          <div class="loading">Loading profile...</div>
        } @else {
          <form [formGroup]="profileForm" (ngSubmit)="onSubmit()" class="profile-form">
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
                <label>Date of Birth</label>
                <input type="date" formControlName="dob" class="form-input">
              </div>
            </div>

            <div class="form-group">
              <label>Professional Bio</label>
              <textarea formControlName="bio" rows="4" class="form-input" placeholder="Briefly describe your career goals and achievements..."></textarea>
            </div>

            <div class="form-group">
              <label>Skills (Comma separated)</label>
              <input type="text" formControlName="skills" class="form-input" placeholder="Angular, C#, SQL...">
            </div>

            <div class="form-row">
              <div class="form-group">
                <label>Years of Experience</label>
                <input type="number" formControlName="experience" class="form-input" placeholder="5">
              </div>
              <div class="form-group">
                <label>Resume Link</label>
                <input type="url" formControlName="resumeUrl" class="form-input" placeholder="https://...">
              </div>
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
    .form-row { display: grid; grid-template-columns: 1fr 1fr; gap: 20px; }
    .form-group { margin-bottom: 20px; }
    .form-group label { display: block; margin-bottom: 8px; font-weight: 500; font-size: 0.9rem; }
    .form-input { width: 100%; background: rgba(255, 255, 255, 0.05); border: 1px solid rgba(255, 255, 255, 0.1); border-radius: 8px; padding: 12px; color: white; outline: none; transition: border-color 0.3s; }
    .form-input:focus { border-color: #3b82f6; }
    
    .form-actions { display: flex; align-items: center; gap: 20px; margin-top: 20px; }
    .btn-primary.large { padding: 12px 30px; font-size: 1rem; }
    .success-msg { color: #22c55e; font-weight: 500; }
    .loading { text-align: center; padding: 40px; color: #94a3b8; }
  `]
})
export class ProfileComponent implements OnInit {
  private fb = inject(FormBuilder);
  private profileService = inject(ProfileService);
  private authService = inject(AuthService);

  profileForm = this.fb.group({
    fullName: ['', Validators.required],
    email: ['', [Validators.required, Validators.email]],
    mobile: [''],
    dob: [''],
    bio: [''],
    skills: [''],
    experience: [0],
    resumeUrl: ['']
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
      this.profileService.getCandidateProfile(this.userId).subscribe({
        next: (profile) => {
          if (profile) {
            this.profileExists = true;
            this.profileForm.patchValue({
              fullName: profile.fullName,
              email: profile.email,
              mobile: profile.mobile,
              dob: profile.dob ? new Date(profile.dob).toISOString().split('T')[0] : '',
              bio: profile.bio,
              skills: profile.skills ? profile.skills.join(', ') : '',
              experience: profile.experience,
              resumeUrl: profile.resumeUrl
            });
          }
          this.isLoading = false;
        },
        error: (err) => {
          console.log('Profile not found, user can create one.');
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
    
    // Construct the data object carefully
    const profileData: any = {
      fullName: formValue.fullName,
      mobile: formValue.mobile || '',
      bio: formValue.bio || '',
      experience: formValue.experience || 0,
      resumeUrl: formValue.resumeUrl || '',
      skills: formValue.skills ? formValue.skills.split(',').map(s => s.trim()) : [],
      addresses: []
    };

    // Only include DOB if it's a valid string
    if (formValue.dob && formValue.dob !== '') {
      profileData.dob = formValue.dob;
    }

    if (!this.profileExists) {
      profileData.userId = this.userId;
      // For creation, ensure email is included if required by backend Create DTO
      profileData.email = formValue.email;
      // Default DOB for creation if not provided, to avoid backend validation issues
      if (!profileData.dob) profileData.dob = new Date().toISOString();
    }

    const request = this.profileExists 
      ? this.profileService.updateCandidateProfile(profileData)
      : this.profileService.createCandidateProfile(profileData);

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
