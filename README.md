# Finance Tracker API

A personal finance tracking REST API for managing accounts and transactions.

## Tech Stack

- **Runtime**: .NET 9.0, ASP.NET Core Web API
- **Database**: SQLite (dev) / PostgreSQL (production)
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

To authenticate: call `POST /api/auth/register` to create an account, then `POST /api/auth/login` to get a token. Click **Authorize** in Swagger, paste the token, and all protected endpoints will include it automatically.

## Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__DefaultConnection` | Database connection string | `Data Source=/app/data/finance.db` |
| `JwtSettings__SecretKey` | JWT signing secret (min 32 chars) | `your-secret-key-32-chars-minimum` |
| `JwtSettings__Issuer` | JWT issuer | `FinanceTracker` |
| `JwtSettings__Audience` | JWT audience | `FinanceTrackerUsers` |
| `Cors__AllowedOrigins` | Comma-separated allowed origins | `http://localhost:3000` |
