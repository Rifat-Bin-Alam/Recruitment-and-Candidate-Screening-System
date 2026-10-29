# Recruitment and Candidate Screening System (RCSS)

A web-based recruitment platform built with ASP.NET Core MVC that streamlines hiring workflows, from job circular publication and candidate applications to CV screening and application management.

🌐 **Live Demo:** [Recruitment and Candidate Screening System](http://recruitment-and-candidate-screening-system.runasp.net/)

## Overview

The Recruitment and Candidate Screening System (RCSS) provides a centralized platform for HR managers and job candidates. It supports job posting, candidate profile management, application tracking, CV-to-job matching, and secure online payments.

## Features

### Candidate Portal
- Candidate registration, authentication, and profile management.
- Secure profile photo, signature, and CV uploads.
- Browse and search published job circulars.
- Apply for available positions and track application status.
- View relevant job requirements and CV matching results, where available.

### HR Management Portal
- Create, edit, publish, and manage job circulars.
- Review candidate applications and profiles.
- Track recruitment progress and application statuses.
- Access HR dashboards and reports.
- Use CV-to-job matching to support candidate screening.

### Security and Payments
- Role-based authentication and authorization using ASP.NET Core Identity.
- Server-side validation and anti-forgery protection.
- Stripe Checkout integration for supported payment workflows.
- Server-side payment verification.
- Responsive interfaces for desktop, tablet, and mobile devices.

## Technology Stack

| Category | Technologies |
|---|---|
| Backend | C#, ASP.NET Core MVC, .NET 10 |
| Frontend | Razor Views, HTML, CSS, JavaScript, Bootstrap |
| Database | SQLite, Entity Framework Core |
| Authentication | ASP.NET Core Identity |
| Payments | Stripe Checkout |
| Hosting | MonsterASP.NET |
| Version Control | Git, GitHub |

## CV-to-Job Matching

RCSS includes CV-to-job matching functionality to help compare candidate qualifications with job requirements.

The matching approach and scoring behavior depend on the implemented screening logic. Where rule-based matching is used, scores can be derived from relevant skills and job criteria without requiring an external AI inference API.

Matching results should support recruiter decision-making rather than replace human review.

## Demo Access

The application may provide separate demonstration accounts for HR managers and candidates.

For security, demo credentials should be provided through a controlled demo environment rather than published in the repository when they expose privileged functionality. Use dedicated demo accounts with restricted permissions and non-sensitive sample data.

## Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/)
- [Git](https://git-scm.com/)

### Installation

Clone the repository:

```bash
git clone https://github.com/Rifat-Bin-Alam/Recruitment-and-Candidate-Screening-System.git
cd Recruitment-and-Candidate-Screening-System
```

Restore dependencies:

```bash
dotnet restore
```

Configure your local settings, database, and Stripe test credentials as required by the project.

Run the application:

```bash
dotnet run
```

Open the local URL printed in the terminal.

### Configuration

Before running the application, verify that:

- The database connection and EF Core migrations are configured correctly.
- Required application settings are supplied securely.
- Stripe test keys are configured through user secrets or environment variables.
- Any required roles and demonstration accounts are seeded safely.

Never place production secrets or real passwords in source-controlled configuration files.

## Security Considerations

- Keep secrets and private configuration out of source control.
- Never commit production databases containing personal information.
- Store uploaded CVs, photographs, and signatures securely.
- Validate file types and sizes on the server.
- Enforce authorization on protected routes and operations.
- Verify payment events and transaction status on the server.
- Use HTTPS in deployed environments.
- Avoid exposing real candidate information through public demo accounts.

## Project Status

This project demonstrates the development of a recruitment management platform using ASP.NET Core MVC, Entity Framework Core, and ASP.NET Core Identity.

For the latest functionality and deployment status, visit the live demo and inspect the repository.

## Author

**Rifat Bin Alam**

GitHub: [Rifat-Bin-Alam](https://github.com/Rifat-Bin-Alam)

Portfolio: [rifat-bin-alam.vercel.app](https://rifat-bin-alam.vercel.app/)

Co-authored-by: Farhana Karim <farhanakarim3225@gmail.com>
