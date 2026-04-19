# Finance Tracker API

A personal finance tracking REST API for managing accounts and transactions.

## Tech Stack

- **Runtime**: .NET 9.0, ASP.NET Core Web API
- **Database**: SQLite (dev) / PostgreSQL / SQL Server (production)
- **ORM**: Entity Framework Core
- **Auth**: JWT Bearer tokens + BCrypt password hashing
- **Testing**: xUnit, integration tests with WebApplicationFactory
- **Containerization**: Docker, docker-compose

## Architecture

```
┌─────────────────────────────────────┐
│            API Layer                │
│  Controllers · Middleware · Auth    │
│         FinanceTracker.Api          │
└────────────────┬────────────────────┘
                 │
┌────────────────▼────────────────────┐
│         Application Layer           │
│       Services · DTOs · Interfaces  │
│                                     │
├─────────────────────────────────────┤
│           Domain Layer              │
│    Entities: Account, Transaction,  │
│    User · Repository Interfaces     │
│            FinanceTracker           │
├─────────────────────────────────────┤
│        Infrastructure Layer         │
│   EF Core · Repositories · DbContext│
└─────────────────────────────────────┘
```

> Application, Domain, and Infrastructure layers share the `FinanceTracker` project. `FinanceTracker.Api` is a separate project.

## Features

- Register and login with JWT authentication
- Create and manage accounts per user
- Add, view, and categorize transactions
- Transfer funds between accounts
- Claim-based authorization (users access only their own data)
- Rate limiting on auth endpoints
- Health check endpoint at `/health`

## Running Locally

```bash
# from repo root
dotnet run --project FinanceTracker.Api

# or enter the project first
cd FinanceTracker.Api
dotnet run
```

API runs at `http://localhost:5029`. Database migrations are applied automatically on startup.

Requires a JWT secret via user secrets:

```bash
dotnet user-secrets set "JwtSettings:SecretKey" "your-secret-key-min-32-chars"
```

## Running Tests

```bash
dotnet test
```

## Running with Docker

```bash
docker-compose up --build
```

API runs at `http://localhost:5029`. SQLite database is persisted in `./data/`. PostgreSQL is used in production.

## API Docs

Interactive Swagger UI available at `http://localhost:5029/swagger` when running locally.

To authenticate: 

1. Register a user by calling `POST /api/auth/register`  with request body:

```json
{
  "email": "user@example.com",
  "password": "Password123"
}
```

2. POST /api/auth/login with the same body to receive a JWT token.
3. In Swagger, click Authorize and paste the token. All protected endpoints will include it automatically.

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | SQLite connection string (fallback) | `Data Source=/app/data/finance.db` |
| `ConnectionStrings__PostgreSQL` | PostgreSQL connection string | `Host=...;Database=...;Username=...` |
| `ConnectionStrings__SqlServer` | SQL Server connection string | `Server=...;Database=...;User ID=...` |
| `JwtSettings__SecretKey` | JWT signing secret (min 32 chars) | `your-secret-key-32-chars-minimum` |
| `JwtSettings__Issuer` | JWT issuer | `FinanceTracker` |
| `JwtSettings__Audience` | JWT audience | `FinanceTrackerUsers` |
| `Cors__AllowedOrigins` | Comma-separated allowed origins | `http://localhost:3000` |
