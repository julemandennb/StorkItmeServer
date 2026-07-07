# StorkItmeServer

Lightweight ASP.NET Core Web API for managing "StorkItme" items, user groups and authentication/authorization.

Tech stack
- .NET 10 (net10.0)
- ASP.NET Core Web API
- Entity Framework Core (Npgsql for PostgreSQL)
- ASP.NET Core Identity (with role-based policies)
- Swagger for API exploration

Prerequisites
- .NET 10 SDK installed
- PostgreSQL (or use your own connection string compatible with Npgsql)

Configuration
- The app expects a connection string named `database` (used by EF Core). Example (appsettings.json or environment variable):

  "database": "Host=localhost;Database=storkitme;Username=postgres;Password=yourpassword"

- Optional: seed first admin user using configuration section `FirstAdminUser`:

  "FirstAdminUser": {
	"email": "admin@example.com",
	"password": "P@ssw0rd!"
  }

Running locally
1. Restore and build:
   dotnet restore
   dotnet build

2. Run the API (from repository root):
   dotnet run --project StorkItmeServer

Notes
- In Development environment the app calls `ApplyMigrations()` at startup and will create/update the database and roles automatically.
- Swagger UI is available in Development at `/swagger`.
- CORS policy `AllowAllOrigins` is enabled by default; adjust for production.

Main API endpoints (high-level)
- Authentication & user endpoints
  - POST /register (requires Manager)
  - POST /login
  - POST /refresh
  - GET /confirmEmail
  - POST /resendConfirmationEmail
  - POST /forgotPassword
  - POST /resetPassword
  - GET /info (requires Read)
  - POST /logout

- StorkItme endpoints (route prefix: /storkitme)
  - GET /storkitme/Get?uuid=... | ?itemNumber=... | ?ean=... (Read)
  - GET /storkitme/GetAll (Read)
  - POST /storkitme/Create (Member)
  - PUT /storkitme/{uuid} (Member)
  - DELETE /storkitme/Delete?uuid=... (Manager)

- User group endpoints (route prefix: /usergroup)
  - GET /usergroup/GetAll (Read)
  - GET /usergroup/Get?uuid=... (Read)
  - POST /usergroup/Create (Manager)
  - PUT /usergroup/Updata (Manager)
  - PUT /usergroup/AddUser (Manager)
  - DELETE /usergroup/Delete?uuid=... (Manager)
  - DELETE /usergroup/RemoveUser (Manager)

Testing
- TestProject contains helpers (SetDataBaseUp) that create an in-memory EF Core DataContext and seed sample data for unit tests.

Contributing
- Follow existing coding style. Run the app and Swagger to exercise endpoints. Use migrations / EF Core tools if you modify the model.

License
- No license is specified in this repository.
