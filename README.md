# VLauncher - User Registration & AD Management System

A complete user onboarding system that integrates Google Sign-In (via Chrome Extension) with Windows Active Directory management.

## Overview

VLauncher allows users to register using their Google account through a Chrome Extension, and administrators can then link those accounts to Active Directory users, manage group memberships, and reset passwords.

```
┌─────────────────────┐     ┌─────────────────────┐     ┌─────────────────────┐
│  Chrome Extension   │     │    ASP.NET Core     │     │  Windows Active     │
│                     │────►│    Web Server       │────►│  Directory          │
│  Google Sign-In     │     │    + MySQL DB       │     │  (VMware/On-prem)   │
└─────────────────────┘     └─────────────────────┘     └─────────────────────┘
```

## Features

- **Chrome Extension**: Google OAuth sign-in for user registration
- **Admin Dashboard**: Manage pending and registered users
- **Active Directory Integration**: Create users, manage groups, reset passwords
- **Clean Architecture**: Domain, Application, Infrastructure, Web layers
- **MediatR Pattern**: CQRS with Commands and Queries
- **Entity Framework Core**: MySQL database with migrations

## Tech Stack

| Component | Technology |
|-----------|------------|
| Backend | ASP.NET Core 8, C# |
| Database | MySQL with EF Core |
| Authentication | Windows Active Directory |
| Frontend | Razor Pages, Tailwind CSS |
| Extension | TypeScript, Chrome APIs |
| Pattern | Clean Architecture, MediatR, CQRS |

## Project Structure

```
VLauncher/
├── src/
│   ├── VLauncher.Domain/           # Entities, Enums, Interfaces
│   ├── VLauncher.Application/      # Commands, Queries, DTOs, Handlers
│   ├── VLauncher.Infrastructure/   # EF Core, AD Service, Repositories
│   └── VLauncher.Web/              # Razor Pages, API Controllers
└── chrome-extension/               # Chrome Extension (TypeScript)
```

## Prerequisites

- .NET 8 SDK
- MySQL Server
- Windows Active Directory (can be on VMware)
- Node.js (for Chrome Extension)
- Google Chrome

## Setup

### 1. Clone the Repository

```bash
git clone https://github.com/your-username/vlauncher-onboarding.git
cd vlauncher-onboarding
```

### 2. Configure Application Settings

Copy the template and fill in your credentials:

```bash
cd VLauncher/src/VLauncher.Web
cp appsettings.template.json appsettings.json
```

Edit `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=vlauncher;User=YOUR_USER;Password=YOUR_PASSWORD;"
  },
  "ActiveDirectory": {
    "Server": "YOUR_AD_SERVER_IP",
    "Domain": "yourdomain.local",
    "AdminUsername": "Administrator",
    "AdminPassword": "YOUR_AD_PASSWORD",
    "AdminGroupName": "VLauncher-Admins",
    "SecurityGroupsOu": "OU=VLauncher,DC=yourdomain,DC=local",
    "UsersOu": "OU=VLauncher,DC=yourdomain,DC=local"
  }
}
```

### 3. Setup MySQL Database

Create the database:

```sql
CREATE DATABASE vlauncher;
```

The application will automatically run migrations on startup.

### 4. Setup Active Directory

In your AD server, create:

1. **Organizational Unit**: `VLauncher` (under your domain root)
2. **Security Group**: `VLauncher-Admins` (inside VLauncher OU)
3. **Admin User**: Add your admin account to `VLauncher-Admins` group

### 5. Run the Application

```bash
cd VLauncher/src/VLauncher.Web
dotnet build
dotnet run
```

The application will be available at `http://localhost:5000`

### 6. Setup Chrome Extension (Optional)

```bash
cd VLauncher/chrome-extension
npm install
npm run build
```

Load the extension in Chrome:
1. Go to `chrome://extensions`
2. Enable "Developer mode"
3. Click "Load unpacked"
4. Select the `dist` folder

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| POST | `/api/Extension/register` | Register pending user (Chrome Extension) |

## Admin Dashboard

Access the admin panel at `/Admin/Dashboard` (requires AD authentication)

**Features:**
- View all users (Pending/Registered tabs)
- Register pending users to AD accounts
- Manage user group memberships
- Reset user passwords
- Delete users

## Architecture

### Clean Architecture Layers

```
┌─────────────────────────────────────────────────────────────────┐
│                         Web Layer                               │
│              (Razor Pages, API Controllers)                     │
├─────────────────────────────────────────────────────────────────┤
│                     Application Layer                           │
│           (Commands, Queries, Handlers, DTOs)                   │
├─────────────────────────────────────────────────────────────────┤
│                    Infrastructure Layer                         │
│         (EF Core, Repositories, AD Service)                     │
├─────────────────────────────────────────────────────────────────┤
│                       Domain Layer                              │
│              (Entities, Interfaces, Enums)                      │
└─────────────────────────────────────────────────────────────────┘
```

### Key Patterns

- **MediatR**: Decouples request handling from controllers
- **CQRS**: Separates read (Queries) and write (Commands) operations
- **Repository Pattern**: Abstracts data access
- **Unit of Work**: Manages database transactions
- **Dependency Injection**: Loose coupling between components

## User Flow

```
1. User installs Chrome Extension
2. User clicks "Sign in with Google"
3. Extension sends Google email to API
4. User appears in Admin Dashboard as "Pending"
5. Admin selects AD account and security groups
6. Admin clicks "Register"
7. User is linked to AD account and added to groups
8. User status changes to "Registered"
```

## Security Notes

- `appsettings.json` is excluded from git (contains credentials)
- Use `appsettings.template.json` as reference
- AD admin credentials should have minimal required permissions
- Session cookies are HTTP-only and use SameSite protection

## Contributing

1. Fork the repository
2. Create a feature branch
3. Commit your changes
4. Push to the branch
5. Create a Pull Request

## License

This project is proprietary software. All rights reserved.

---

Built with Clean Architecture, MediatR, and ASP.NET Core 8
