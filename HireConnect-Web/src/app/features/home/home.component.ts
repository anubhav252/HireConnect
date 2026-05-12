import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterLink, Router } from '@angular/router';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-home',
  standalone: true,
  imports: [CommonModule, RouterLink, FormsModule],
  template: `
    <div class="home-container">
      <!-- Hero Section -->
      <section class="hero fade-in">
        <div class="hero-content">
          <h1 class="hero-title">Elevate Your Career with <span class="gradient-text">HireConnect</span></h1>
          <p class="hero-subtitle">The platform for connecting top talent with industry-leading recruiters.</p>
          
          <div class="search-bar glass">
            <input type="text" [(ngModel)]="searchTerm" (keyup.enter)="onSearch()" placeholder="Job title, skills, or company..." class="search-input">
            <input type="text" [(ngModel)]="location" (keyup.enter)="onSearch()" placeholder="Location..." class="search-input location">
            <button class="btn-primary search-btn" (click)="onSearch()">Search Jobs</button>
          </div>

          <div class="stats-row">
            <div class="stat-item">
              <span class="stat-value">10k+</span>
              <span class="stat-label">Active Jobs</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">5k+</span>
              <span class="stat-label">Companies</span>
            </div>
            <div class="stat-item">
              <span class="stat-value">50k+</span>
              <span class="stat-label">Candidates</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Featured Categories -->
      <section class="categories fade-in">
        <h2 class="section-title">Popular Categories</h2>
        <div class="category-grid">
          <div class="category-card glass glass-hover">
            <div class="icon">💻</div>
            <h3>Technology</h3>
            <p>2.5k Jobs</p>
          </div>
          <div class="category-card glass glass-hover">
            <div class="icon">🎨</div>
            <h3>Design</h3>
            <p>1.2k Jobs</p>
          </div>
          <div class="category-card glass glass-hover">
            <div class="icon">📊</div>
            <h3>Marketing</h3>
            <p>800 Jobs</p>
          </div>
          <div class="category-card glass glass-hover">
            <div class="icon">🏢</div>
            <h3>Management</h3>
            <p>1.5k Jobs</p>
          </div>
        </div>
      </section>
    </div>
  `,
  styles: [`
    .home-container {
      padding: 40px 20px;
      max-width: 1200px;
      margin: 0 auto;
    }
    .hero {
      text-align: center;
      padding: 120px 0;
      background-image: url('../../../assets/images/hero-bg.png');
      background-size: cover;
      background-position: center;
      border-radius: 24px;
      margin-bottom: 60px;
      position: relative;
    }
    .hero::before {
      content: '';
      position: absolute;
      top: 0; left: 0; right: 0; bottom: 0;
      background: rgba(15, 23, 42, 0.7);
      border-radius: 24px;
      z-index: 0;
    }
    .hero-content {
      position: relative;
      z-index: 1;
    }
    .hero-title {
      font-size: 4rem;
      font-weight: 800;
      margin-bottom: 20px;
      line-height: 1.1;
    }
    .gradient-text {
      background: linear-gradient(135deg, #3b82f6, #8b5cf6);
      -webkit-background-clip: text;
      -webkit-text-fill-color: transparent;
    }
    .hero-subtitle {
      font-size: 1.25rem;
      color: #94a3b8;
      max-width: 700px;
      margin: 0 auto 40px;
    }
    .search-bar {
      display: flex;
      padding: 10px;
      max-width: 800px;
      margin: 0 auto 60px;
      gap: 10px;
    }
    .search-input {
      flex: 1;
      background: transparent;
      border: none;
      color: white;
      padding: 15px;
      font-size: 1rem;
      outline: none;
    }
    .search-input.location {
      border-left: 1px solid rgba(255, 255, 255, 0.1);
    }
    .search-btn {
      min-width: 150px;
    }
    .stats-row {
      display: flex;
      justify-content: center;
      gap: 60px;
    }
    .stat-item {
      display: flex;
      flex-direction: column;
    }
    .stat-value {
      font-size: 2rem;
      font-weight: 700;
      color: #3b82f6;
    }
    .stat-label {
      color: #64748b;
      font-size: 0.9rem;
    }
    .section-title {
      font-size: 2.5rem;
      font-weight: 700;
      margin-bottom: 40px;
      text-align: center;
    }
    .category-grid {
      display: grid;
      grid-template-columns: repeat(auto-fit, minmax(250px, 1fr));
      gap: 20px;
    }
    .category-card {
      padding: 40px;
      text-align: center;
      cursor: pointer;
    }
    .category-card .icon {
      font-size: 3rem;
      margin-bottom: 20px;
    }
    .category-card h3 {
      font-size: 1.5rem;
      margin-bottom: 10px;
    }
    .category-card p {
      color: #3b82f6;
      font-weight: 600;
    }
  `]
})
export class HomeComponent {
  private router = inject(Router);
  searchTerm = signal('');
  location = signal('');

  onSearch() {
    this.router.navigate(['/jobs'], {
      queryParams: {
        title: this.searchTerm(),
        location: this.location()
      }
    });
  }
}
