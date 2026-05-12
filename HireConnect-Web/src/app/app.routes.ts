import { Routes } from '@angular/router';
import { HomeComponent } from './features/home/home.component';
import { authGuard, roleGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'jobs', children: [
    { path: '', loadComponent: () => import('./features/jobs/job-list/job-list.component').then(m => m.JobListComponent) },
    { path: ':id', loadComponent: () => import('./features/jobs/job-details/job-details.component').then(m => m.JobDetailsComponent) }
  ]},
  { path: 'auth', loadChildren: () => import('./features/auth/auth.routes').then(m => m.AUTH_ROUTES) },
  { path: 'candidate', canActivate: [authGuard, roleGuard], data: { role: 'Candidate' }, children: [
    { path: 'dashboard', loadComponent: () => import('./features/candidate/dashboard/dashboard.component').then(m => m.CandidateDashboardComponent) },
    { path: 'profile', loadComponent: () => import('./features/candidate/profile/profile.component').then(m => m.ProfileComponent) }
  ]},
  { path: 'recruiter', children: [
    { path: 'dashboard', canActivate: [authGuard, roleGuard], data: { role: 'Recruiter' }, loadComponent: () => import('./features/recruiter/dashboard/dashboard.component').then(m => m.RecruiterDashboardComponent) },
    { path: 'post-job', canActivate: [authGuard, roleGuard], data: { role: 'Recruiter' }, loadComponent: () => import('./features/recruiter/post-job/post-job.component').then(m => m.PostJobComponent) },
    { path: 'edit-job/:id', canActivate: [authGuard, roleGuard], data: { role: 'Recruiter' }, loadComponent: () => import('./features/recruiter/post-job/post-job.component').then(m => m.PostJobComponent) },
    { path: 'jobs/:id/applications', canActivate: [authGuard, roleGuard], data: { role: 'Recruiter' }, loadComponent: () => import('./features/recruiter/manage-applications/manage-applications.component').then(m => m.ManageApplicationsComponent) }
  ]},
  { path: '**', redirectTo: '' }
];
