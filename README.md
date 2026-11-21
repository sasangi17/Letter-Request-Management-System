# Employee Letter Request Management System

An employee letter request management system developed to simplify the process of requesting, reviewing, approving, rejecting, and generating official employee letters.

The system provides separate interfaces for employees and administrators, with role-based access control and digitally signed PDF letter generation.

## Project Overview

The Employee Letter Request Management System allows employees to submit requests for official company letters and track their request status. Administrators can review submitted requests, approve or reject them, and generate approved letters with their authorized signatures.

The system also supports Soft Copy and Hard Copy requests, including print-status tracking for Hard Copy letters.

## Technologies Used

### Frontend
- Angular
- TypeScript
- HTML5
- CSS3
- Bootstrap / Custom CSS (where applicable)

### Backend
- ASP.NET Core Web API 8
- C#
- ADO.NET
- SQL Server
- JWT Authentication
- RESTful APIs

### PDF Generation
- QuestPDF

### Development Tools
- Visual Studio
- Visual Studio Code
- SQL Server Management Studio (SSMS)
- Git
- GitHub
- Swagger / OpenAPI

## Main Features

### Employee Module
- Secure employee login.
- Submit official letter requests.
- Select the required letter type.
- Enter the required information for letter generation.
- Choose between Soft Copy and Hard Copy.
- Preview requested letters.
- View submitted request details.
- Track request statuses.
- Download approved letters as PDF.
- View rejection information when applicable.

### Administrator Module
- Secure administrator login.
- Dashboard with request statistics.
- View total, pending, approved, and rejected requests.
- View recent employee requests.
- Monitor monthly request activity.
- Search employee requests.
- Filter requests by status.
- View individual request details.
- Upload an administrator signature.
- Approve or reject pending requests.
- Generate approved PDF letters with the approving administrator's signature.
- Download and print approved letters.
- Track Hard Copy printing status.

### Letter Approval and PDF Generation
- Generate letters using submitted employee information.
- Maintain approval details.
- Capture the approving administrator's signature.
- Include the approver's name and signature in the generated PDF.
- Store the signature snapshot associated with the approval.
- Support approved letter downloading and printing.

### Authentication and Authorization
- JWT-based authentication.
- Role-based access for Employees and Administrators.
- Support for accounts with multiple roles.
- Protected API endpoints.

## Request Workflow

1. Employee logs into the system.
2. Employee selects a letter type.
3. Employee submits a letter request.
4. The request is submitted with Pending status.
5. Administrator reviews the request.
6. Administrator approves or rejects the request.
7. For approved requests, the system generates a signed PDF.
8. Employee can access and download the approved letter.
9. Hard Copy requests can be printed and their print status tracked.

## Request Statuses

| Status | Description |
|---|---|
| Pending | Waiting for administrator review |
| Approved | Successfully approved by an administrator |
| Rejected | Request rejected by an administrator |
| Cancelled | Request cancelled, where applicable |

## Copy Types

| Copy Type | Description |
|---|---|
| Soft Copy | Approved letter available as a downloadable PDF |
| Hard Copy | Approved letter intended for physical printing |

## System Architecture

The application follows a layered architecture.

### Frontend
Angular communicates with the backend through HTTP requests using REST API endpoints.

### Backend
The ASP.NET Core Web API handles authentication, business logic, request processing, approval operations, and PDF generation.

### Database
SQL Server stores employee information, letter types, letter requests, approval records, and related information.

### Backend Layers

- **Controllers** – Handle incoming API requests.
- **Business** – Implement application business logic.
- **Repository** – Handle database operations.
- **Models** – Contain domain models and DTOs.
- **Services** – Handle supporting operations such as JWT authentication and PDF generation.
- **Data Access** – Provides SQL database connectivity and query execution.

## Project Structure

```text
Employee-Letter-Request-Management-System/
│
├── Frontend/
│   └── request_letter/
│       ├── src/
│       │   └── app/
│       │       ├── admin/
│       │       ├── employee/
│       │       ├── services/
│       │       └── shared/
│       ├── angular.json
│       └── package.json
│
├── Backend/
│   └── Letter_Request_Management_System/
│       ├── Controllers/
│       ├── Business/
│       ├── Repository/
│       ├── Models/
│       │   ├── Domain/
│       │   └── Dto/
│       ├── Services/
│       ├── DataAccess/
│       └── Program.cs
│
└── README.md
```

*Note: The folder names above illustrate the main application structure. Adjust them to match your actual repository.*

## Installation and Setup

### Prerequisites

Make sure the following software is installed:

- Node.js and npm
- Angular CLI
- .NET 8 SDK
- SQL Server
- SQL Server Management Studio
- Visual Studio or Visual Studio Code

### 1. Clone the Repository

```bash
git clone https://github.com/sasangi17/Letter-Request-Management-System.git
```

Navigate to the project directory:

```bash
cd Letter-Request-Management-System
```

### 2. Backend Setup

Open the backend project in Visual Studio.

1. Configure the SQL Server connection string.
2. Configure the JWT authentication settings.
3. Ensure the required database and tables are available.
4. Restore the NuGet packages.
5. Build and run the ASP.NET Core Web API.

Using the terminal:

```bash
dotnet restore
dotnet build
dotnet run
```

The backend API can be tested using Swagger when Swagger is enabled.

### 3. Frontend Setup

Navigate to the Angular project directory:

```bash
cd Frontend/request_letter
```

Install the required dependencies:

```bash
npm install
```

Run the Angular application:

```bash
ng serve
```

Open the application in your browser:

```text
http://localhost:4200
```

## API Overview

The backend provides REST API endpoints for authentication, employee letter requests, administrator operations, and PDF generation.

| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/Auth/login` | User authentication |
| GET | `/api/Admin/requests` | Retrieve employee requests |
| POST | `/api/Admin/requestsapprove?id={id}` | Approve a request |
| POST | `/api/Admin/requestsreject?id={id}` | Reject a request |
| POST | `/api/Admin/signature` | Upload administrator signature |
| GET | `/api/Admin/signature` | Retrieve saved administrator signature |
| GET | `/api/Admin/approvalsignature?id={id}` | Retrieve approval signature details |
| GET | `/api/Admin/approvedpdf?id={id}` | Download approved PDF |
| POST | `/api/Admin/markprinted?id={id}` | Mark a Hard Copy as printed |

*Endpoint availability and authorization depend on the implemented backend controllers.*

## Security Considerations

- Use JWT authentication for protected endpoints.
- Apply role-based authorization.
- Validate user input before processing requests.
- Validate uploaded signature image formats and file sizes.
- Store database credentials and JWT secrets securely.
- Do not commit passwords, private keys, or connection-string secrets to GitHub.

## Future Improvements

- Email notifications for request status changes.
- Advanced reporting and analytics.
- Additional letter templates.
- Enhanced audit history.
- Improved request tracking and notifications.

## Author

**Sasangi Samadara**

Computer Science Undergraduate

GitHub: [@sasangi17](https://github.com/sasangi17)

---

*Employee Letter Request Management System – Simplifying employee letter requests and approval management.*
