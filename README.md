# 🚀 HireConnect

**HireConnect** is a state-of-the-art,recruitment platform built with a robust **Microservices Architecture**. It connects top-tier talent with industry-leading recruiters through a seamless, event-driven ecosystem.

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
- **Database**: Designed the initial schemas for PostgreSQL and configured Entity Framework Core migrations.Used Neon database for hosting the database.

### **Day 2: Authentication Core & Identity**
- **Identity**: Implemented ASP.NET Core Identity for robust user management.
- **JWT**: Developed the security infrastructure for token generation, validation, and role-based authorization.
- **Auth API**: Completed the core registration and login flows in the `AuthService`.
- **google and github oauth integrated**: implemented google and github oauth for user login.

### **Day 3-4: Profile & Job Microservices**
- **Candidate Profiles**: Developed the initial `ProfileService` endpoints for managing candidate resumes and bios.
- **Job Management**: Implemented CRUD operations for job postings in the `JobService`.
- **API Gateway**: Initialized the **YARP Gateway** to provide a unified entry point for the frontend.

### **Day 5: Event-Driven Communication**
- **Messaging**: Set up **RabbitMQ** as the message broker.
- **MassTransit**: Integrated MassTransit to facilitate asynchronous communication between services.
- **Events**: Implemented the first set of domain events: `UserRegistered` and `JobPosted`.

### **Day 6: Search & Infrastructure Setup**
- **Elasticsearch**: Integrated Elasticsearch into the `JobService` to enable high-performance full-text search.

### **Day 7-9: Angular Frontend Foundation**
- **Angular 18**: Initialized the frontend project using standalone components and signals.
- **Design System**: Built a custom UI library with a premium **Glassmorphism** dark-mode aesthetic.
- **Security**: Developed authentication interceptors and route guards to protect sensitive pages.

### **Day 10-11: Role-Based Dashboards**
- **Candidate Dashboard**: Built features for tracking applications, saved jobs, and interview invites.
- **Recruiter Dashboard**: Developed a panel for managing job posts and reviewing incoming applications.
- **Real-time**: Integrated **WebSockets** for instant in-app notifications.

### **Day 12-13: Interview & Notification Logic**
- **Interview Service**: Developed logic for scheduling interviews and managing meeting links.
- **Notifications**: Built a centralized `NotificationService` to handle multi-channel alerts.
- **Mailing**: Integrated **Twilio SendGrid** for transactional email notifications.

### **Day 14-15: Cloud Deployment (Render Phase 1)**
- **CI/CD**: Configured manual and automated deployment pipelines for Render.


### **Day 16-17: Advanced Profile Features & Routing**
- **Gateway Fix**: Corrected complex routing rules in YARP to support deep-linking for profiles.
- **Recruiter Profile**: Fully implemented the Recruiter profile management system (Company details, logo, industry).
- **Navigation**: Finalized the UI header with dynamic profile links for authenticated users.

---

## 🚀 Getting Started

### Prerequisites
- .NET 8 SDK
- Node.js & Angular CLI


### Local Setup
1.  **Backend**: Start the Gateway project (Port 5000) and all individual microservices.
2.  **Frontend**: Navigate to `HireConnect-Web` and run `npm start`.

---

## 🌐 Live Deployment
The project is currently live on Render with each service and frontend fully functional.
Live Frontend: https://hireconnect-frontend-rc62.onrender.com
Live Gateway: https://hireconnect-gateway.onrender.com
Live Notification Service : https://hireconnect-notification.onrender.com
Live Job Service : https://hireconnect-job.onrender.com
Live Application Service : https://hireconnect-application.onrender.com
Live Profile Service : https://hireconnect-atoy.onrender.com
Live Auth Service : https://hireconnect-auth.onrender.com
Live Interview Service : https://hireconnect-interview.onrender.com
Live Analytics Service : https://hireconnect-analytics.onrender.com
