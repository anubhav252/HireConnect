# 🚀 HireConnect

**HireConnect** is a state-of-the-art, AI-ready recruitment platform built with a robust **Microservices Architecture**. It connects top-tier talent with industry-leading recruiters through a seamless, event-driven ecosystem.

---

## 🏗️ Architecture Overview

The system is designed for high scalability and resilience using a distributed microservices pattern:

- **Frontend**: Angular 18 (Premium Dark Theme, Standalone Components).
- **API Gateway**: YARP (Yet Another Reverse Proxy) for unified entry and routing.
- **Backend**: .NET 8 Microservices (Web API).
- **Interservice Communication**: MassTransit with RabbitMQ (Event-Driven).
- **Data Storage**: PostgreSQL (Relational), Elasticsearch (Search Engine), Redis (Caching).
- **Deployment**: Render (Cloud Platform).

---

## 🛠️ Microservices Ecosystem

1.  **Auth Service**: Handles JWT-based authentication and OAuth (Google/GitHub) integration.
2.  **Profile Service**: Manages detailed Candidate and Recruiter profiles.
3.  **Job Service**: Orchestrates job postings and search using Elasticsearch.
4.  **Application Service**: Tracks the lifecycle of job applications.
5.  **Interview Service**: Manages interview scheduling and video call integrations.
6.  **Notification Service**: Real-time alerts via WebSockets and Email.
7.  **Analytics Service**: Provides insights into hiring trends and platform usage.

---

## 📅 Development Progress (Detailed Timeline)

### **Day 1: Project Initialization & API Design**
- **Architecture**: Defined the microservices boundaries and event-driven communication patterns.
- **Scaffolding**: Created the .NET 8 solution and initial project structures for Auth, Profile, and Job services.
- **Database**: Designed the initial schemas for PostgreSQL and configured Entity Framework Core migrations.

### **Day 2: Authentication Core & Identity**
- **Identity**: Implemented ASP.NET Core Identity for robust user management.
- **JWT**: Developed the security infrastructure for token generation, validation, and role-based authorization.
- **Auth API**: Completed the core registration and login flows in the `AuthService`.

### **Day 3: Profile & Job Microservices**
- **Candidate Profiles**: Developed the initial `ProfileService` endpoints for managing candidate resumes and bios.
- **Job Management**: Implemented CRUD operations for job postings in the `JobService`.
- **API Gateway**: Initialized the **YARP Gateway** to provide a unified entry point for the frontend.

### **Day 4: Event-Driven Communication**
- **Messaging**: Set up **RabbitMQ** as the message broker.
- **MassTransit**: Integrated MassTransit to facilitate asynchronous communication between services.
- **Events**: Implemented the first set of domain events: `UserRegistered` and `JobPosted`.

### **Day 5: Search & Infrastructure Setup**
- **Elasticsearch**: Integrated Elasticsearch into the `JobService` to enable high-performance full-text search.
- **Caching**: Configured **Redis** for distributed caching of job listings and user sessions.
- **Docker**: Finalized the `docker-compose` environment for consistent local development.

### **Day 6: Angular Frontend Foundation**
- **Angular 18**: Initialized the frontend project using standalone components and signals.
- **Design System**: Built a custom UI library with a premium **Glassmorphism** dark-mode aesthetic.
- **Security**: Developed authentication interceptors and route guards to protect sensitive pages.

### **Day 7: Role-Based Dashboards**
- **Candidate Dashboard**: Built features for tracking applications, saved jobs, and interview invites.
- **Recruiter Dashboard**: Developed a panel for managing job posts and reviewing incoming applications.
- **Real-time**: Integrated **WebSockets** for instant in-app notifications.

### **Day 8: Interview & Notification Logic**
- **Interview Service**: Developed logic for scheduling interviews and managing meeting links.
- **Notifications**: Built a centralized `NotificationService` to handle multi-channel alerts.
- **Mailing**: Integrated **Twilio SendGrid** for transactional email notifications.

### **Day 9: OAuth 2.0 & External Integrations**
- **Social Login**: Added support for **Google** and **GitHub** authentication.
- **Cookie Security**: Optimized ASP.NET Core cookie policies for cross-site compatibility.
- **Stability**: Resolved "Correlation failed" errors in production OAuth flows.

### **Day 10: Cloud Deployment (Render Phase 1)**
- **CI/CD**: Configured manual and automated deployment pipelines for Render.
- **Cloud DB**: Migrated local PostgreSQL instances to **Neon Managed Databases**.
- **Networking**: Established internal service-to-service communication within the Render network.

### **Day 11: Production Stability & Search Optimization**
- **Search Tuning**: Optimized Elasticsearch indexing and search queries for the production environment.
- **Logging**: Implemented global error handling and centralized logging across all microservices.
- **Gateway Polish**: Fine-tuned YARP configuration for better load balancing and reliability.

### **Day 12: Advanced Profile Features & Routing**
- **Gateway Fix**: Corrected complex routing rules in YARP to support deep-linking for profiles.
- **Recruiter Profile**: Fully implemented the Recruiter profile management system (Company details, logo, industry).
- **Navigation**: Finalized the UI header with dynamic profile links for authenticated users.


---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js & Angular CLI
- Docker Desktop

### Local Setup
1.  **Infrastructure**: Run `docker-compose up -d` to start Postgres, RabbitMQ, and Redis.
2.  **Backend**: Start the Gateway project (Port 5000) and all individual microservices.
3.  **Frontend**: Navigate to `HireConnect-Web` and run `npm start`.

---

## 🌐 Live Deployment
The project is currently live on Render. Ensure your environment variables are configured in the Render Dashboard to match the `appsettings.Production.json` requirements.
