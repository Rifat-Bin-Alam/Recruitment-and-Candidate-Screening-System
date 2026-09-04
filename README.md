# Recruitment and Candidate Screening System

A web-based recruitment management and candidate screening system built
with **ASP.NET Core MVC**, **C#**, **Entity Framework Core**,
**SQLite**, **ASP.NET Core Identity**, and **Stripe**.

The system is designed to manage the recruitment process from job
circular creation and candidate applications to payment processing,
AI-based CV matching, interview ranking, offers, and joining schedules.

## Project Overview

The system provides two primary roles:

-   **Candidate** --- manages a profile and CV, searches jobs, applies
    for positions, completes payments when required, and tracks
    applications.
-   **HR Manager** --- creates and manages job circulars, reviews
    applications, views candidate documents, and manages recruitment
    decisions.

### Recruitment Flow

``` text
Job Circular
     ↓
Candidate Profile & CV
     ↓
Job Application
     ↓
Payment (if required)
     ↓
AI CV-to-Job Matching
     ↓
Shortlisted / Manual Review / Rejected
     ↓
Interview
     ↓
Interview Ranking
     ↓
Offer
     ↓
Joining Schedule
```

## Key Features

### Candidate

-   Registration and login
-   Role-based access
-   Candidate dashboard
-   Profile management
-   Education, experience, and skills
-   Photo and signature upload
-   CV upload and current-CV management
-   Secure private file storage
-   Published job listing
-   Job search by title, department, or location
-   Job sorting by publication date and deadline
-   Job details and requirements
-   Duplicate application prevention
-   Free and paid applications
-   Stripe Checkout
-   Payment verification
-   Application reference generation
-   My Applications and status tracking

### HR Manager

-   HR authentication
-   Role-based authorization
-   Job circular creation and editing
-   Publish and close job circulars
-   Job requirement management
-   Application search and status filtering
-   Candidate application details
-   Candidate photo and signature viewing
-   Secure CV viewing
-   Payment information

### Planned

-   AI CV-to-job matching
-   Automatic candidate scoring
-   Shortlisted / Manual Review / Rejected decisions
-   HR dashboard and recruitment statistics
-   Interview management
-   Interview ranking
-   Offer management
-   Joining schedules
-   Recruitment funnel/reporting

## AI Matching Rules

The planned AI matching module compares job-relevant candidate
information with the requirements of a job.

  AI Score   Decision
  ---------- -------------------
  `80–100`   **SHORTLISTED**
  `60–79`    **MANUAL REVIEW**
  `<60`      **REJECTED**

The matching process must not use irrelevant personal information such
as:

-   Name
-   Gender
-   Photograph
-   Signature
-   Other non-job-related identifiers

The focus is on skills, education, experience, and other job-relevant
qualifications.

## Technology Stack

  Technology                Purpose
  ------------------------- ----------------------------------
  C#                        Programming language
  ASP.NET Core MVC          Web application framework
  .NET 10 LTS               Runtime/framework
  Razor Views               UI
  HTML / CSS / JavaScript   Frontend
  Bootstrap                 Responsive UI
  Entity Framework Core     ORM
  SQLite                    Database
  ASP.NET Core Identity     Authentication and authorization
  Stripe Checkout           Payment processing
  AI Matching               CV-to-job relevance analysis

## Project Structure

``` text
RecruitmentAndCandidateScreeningSystem/
│
├── Areas/
│   └── Identity/
│       └── Pages/
│           └── Account/
│               └── Login.cshtml.cs
│
├── Controllers/
│   ├── CandidateController.cs
│   ├── CandidateJobsController.cs
│   ├── CandidateProfilesController.cs
│   ├── HRApplicationsController.cs
│   ├── HomeController.cs
│   ├── JobCircularsController.cs
│   └── StripeWebhookController.cs
│
├── Data/
│   ├── ApplicationDbContext.cs
│   └── IdentitySeeder.cs
│
├── Models/
│   ├── AIMatchingResult.cs
│   ├── ApplicationUser.cs
│   ├── CandidateProfile.cs
│   ├── CV.cs
│   ├── InterviewRanking.cs
│   ├── JobApplication.cs
│   ├── JobCircular.cs
│   ├── JobRequirement.cs
│   ├── JoiningSchedule.cs
│   ├── Offer.cs
│   ├── Payment.cs
│   ├── RecruitmentSource.cs
│   ├── RoleNames.cs
│   └── Enums/
│
├── Services/
│   └── StripePaymentService.cs
│
├── ViewModels/
│
├── Views/
│   ├── Candidate/
│   ├── CandidateProfiles/
│   ├── HRApplications/
│   ├── JobCirculars/
│   └── Shared/
│
├── Migrations/
├── SecureUploads/
│   ├── Photos/
│   ├── Signatures/
│   └── CVs/
│
├── wwwroot/
├── appsettings.json
├── Program.cs
├── RecruitmentAndCandidateScreeningSystem.csproj
└── RecruitmentAndCandidateScreeningSystem.db
```

> `SecureUploads` contains private candidate files and should **not** be
> committed to GitHub.

## Database

The project uses SQLite with Entity Framework Core.

Database:

``` text
RecruitmentAndCandidateScreeningSystem.db
```

Connection string:

``` json
"DefaultConnection": "Data Source=RecruitmentAndCandidateScreeningSystem.db"
```

### Main Domain Entities

``` text
ApplicationUser
CandidateProfile
CV
JobCircular
JobRequirement
JobApplication
Payment
RecruitmentSource
AIMatchingResult
InterviewRanking
Offer
JoiningSchedule
```

## Authentication and Authorization

The system uses ASP.NET Core Identity with:

``` text
Candidate
HRManager
```

Candidate-only controllers use role authorization such as:

``` csharp
[Authorize(Roles = RoleNames.Candidate)]
```

HR-only controllers use:

``` csharp
[Authorize(Roles = RoleNames.HRManager)]
```

Candidate ownership checks are also used for candidate-specific records
and files.

## Stripe Payment

Paid applications use Stripe Checkout.

``` text
Candidate
   ↓
Apply for Paid Job
   ↓
Create Pending Payment
   ↓
Stripe Checkout
   ↓
Successful Payment
   ↓
Server Verification
   ↓
Payment = Paid
   ↓
Application Reference Available
```

The application does not store card numbers or CVV information.

### Stripe Configuration

Development secrets should be stored with ASP.NET Core User Secrets
rather than source code.

``` json
"Stripe": {
  "SecretKey": "",
  "Currency": "usd",
  "WebhookSecret": ""
}
```

**Never commit a real Stripe secret key to GitHub.**

## Secure File Uploads

Private candidate files are stored outside `wwwroot`:

``` text
SecureUploads/
├── Photos/
├── Signatures/
└── CVs/
```

Allowed images:

``` text
.jpg
.jpeg
.png
```

Allowed CV files:

``` text
.pdf
.doc
.docx
```

Maximum sizes:

``` text
Photo      → 2 MB
Signature  → 2 MB
CV         → 5 MB
```

Files receive generated GUID-based filenames. Private files are served
through authorized controller actions rather than public URLs.

## Application Status

``` text
Submitted
Shortlisted
ManualReview
Rejected
Interview
Offered
Accepted
```

Payment statuses:

``` text
Pending
Paid
Failed
```

Offer statuses:

``` text
Pending
Accepted
Rejected
```

## Getting Started

### Prerequisites

-   Visual Studio with .NET 10 support
-   .NET 10 SDK
-   Git
-   Stripe test account for payment testing

### Clone

``` bash
git clone https://github.com/YOUR_USERNAME/RecruitmentAndCandidateScreeningSystem.git
cd RecruitmentAndCandidateScreeningSystem
```

### Restore Packages

``` bash
dotnet restore
```

### Apply Migrations

Using Visual Studio Package Manager Console:

``` powershell
Update-Database
```

Or:

``` bash
dotnet ef database update
```

### Run

``` bash
dotnet run
```

Or run the project from Visual Studio with **F5**.

## Development HR Account

The development environment seeds:

``` text
Email:    hr@recruitment.local
Password: Hr@123456
```

> Replace development credentials before any real deployment.

## Development Workflow

``` text
Requirement
    ↓
Model / Database
    ↓
Controller
    ↓
ViewModel
    ↓
Razor View
    ↓
Validation
    ↓
Authorization
    ↓
Testing
    ↓
Build Verification
    ↓
Next Feature
```

Features should be built and tested incrementally.

## Security

The project follows these security practices:

-   ASP.NET Core Identity
-   Role-based authorization
-   Candidate ownership checks
-   Server-side validation
-   Anti-forgery protection
-   File type restrictions
-   File size limits
-   Generated secure filenames
-   Private file storage
-   Server-side Stripe verification
-   No card/CVV storage
-   Privacy-aware AI matching

## Current Development Status

### Completed

-   [x] ASP.NET Core MVC project setup
-   [x] .NET 10 configuration
-   [x] SQLite database
-   [x] Entity Framework Core
-   [x] ASP.NET Core Identity
-   [x] Candidate and HR Manager roles
-   [x] Role seeding
-   [x] Candidate registration/login
-   [x] Role-based login redirection
-   [x] Candidate dashboard
-   [x] Candidate profile
-   [x] Secure photo upload
-   [x] Secure signature upload
-   [x] Secure CV upload
-   [x] Current CV management
-   [x] Job circular creation
-   [x] Job circular editing
-   [x] Job publishing
-   [x] Job closing
-   [x] Job requirements
-   [x] Candidate job listing
-   [x] Job search
-   [x] Job sorting
-   [x] Job application
-   [x] Duplicate application prevention
-   [x] Free application flow
-   [x] Paid application flow
-   [x] Stripe Checkout
-   [x] Stripe payment verification
-   [x] Payment status tracking
-   [x] Application reference generation
-   [x] Candidate My Applications
-   [x] HR Applications page
-   [x] HR application search/filter
-   [x] HR application details
-   [x] Secure HR photo access
-   [x] Secure HR signature access
-   [x] Secure HR CV access

### Next

-   [ ] AI CV-to-job matching
-   [ ] AI matching score display
-   [ ] Automatic recruitment decision
-   [ ] HR dashboard
-   [ ] Interview management
-   [ ] Interview ranking
-   [ ] Offer management
-   [ ] Joining schedule
-   [ ] Recruitment funnel/reporting
-   [ ] UI/UX polish
-   [ ] End-to-end testing
-   [ ] Production security review

## Recruitment Funnel

``` text
                    ┌─────────────────┐
                    │  Job Circular   │
                    └────────┬────────┘
                             ↓
                    ┌─────────────────┐
                    │ Candidate Apply │
                    └────────┬────────┘
                             ↓
                    ┌─────────────────┐
                    │  Payment Check  │
                    └────────┬────────┘
                             ↓
                    ┌─────────────────┐
                    │   AI Matching   │
                    └────────┬────────┘
                             ↓
              ┌──────────────┼──────────────┐
              ↓              ↓              ↓
        Score ≥ 80      Score 60–79     Score < 60
        Shortlisted    Manual Review     Rejected
              │              │
              └───────┬──────┘
                      ↓
               ┌─────────────┐
               │  Interview  │
               └──────┬──────┘
                      ↓
               ┌─────────────┐
               │   Ranking   │
               └──────┬──────┘
                      ↓
               ┌─────────────┐
               │    Offer    │
               └──────┬──────┘
                      ↓
               ┌─────────────┐
               │   Joining   │
               └─────────────┘
```

## Academic Purpose

This project demonstrates:

-   Software engineering
-   ASP.NET Core MVC development
-   Database design
-   MVC architecture
-   Authentication and authorization
-   Secure file handling
-   Payment integration
-   AI-assisted recruitment
-   Recruitment workflow automation

## Future Improvements

Possible future enhancements:

-   Advanced NLP-based CV parsing
-   Explainable AI matching
-   HR analytics
-   Recruitment funnel visualization
-   Email notifications
-   Interview scheduling
-   Candidate notifications
-   Pagination
-   Audit logging
-   Cloud file storage
-   Automated testing
-   CI/CD pipeline

## License

This project is intended primarily for academic and educational
purposes.

Add an appropriate open-source license before publicly distributing the
repository.

## Author

**Recruitment and Candidate Screening System**

Built with ASP.NET Core MVC, C#, Entity Framework Core, SQLite, ASP.NET
Core Identity, Bootstrap, Stripe, and AI-assisted candidate matching.
