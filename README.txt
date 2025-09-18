# .NET 8 Blazor + ASP.NET Core API (KobraKai)

## What works
- **Auth**: Register + Login (JWT issued).  
- **Dashboard**: Displays current user from JWT claims (`id`, `email`, `role`, `tokenBalance`).  
- **Meals (UI)**: Member-only Create form + public listing (calls API).  
- **Orders (UI)**: Role-gated actions wired to API endpoints.

## Known limitations
- **Create Meal**: UI is complete and posts to `POST /api/meals`, but in this build the API route is not available, so you’ll see a friendly message instead of an error (HTTP 404 from API).
- If the API adds `/api/meals` later, the current UI works without changes.

## Run locally
### API
1. `cd API`
2. Set config values (appsettings.Development.json): `"Jwt:Key"`, `"Jwt:Issuer"`, `"Jwt:Audience"`, `"ConnectionStrings:Default"`.
3. Run: `dotnet run` → Swagger at `http://localhost:5017/swagger`.

### Web (Blazor Server)
1. `cd Web`
2. Ensure `Program.cs` sets the API base to `http://localhost:5017/`.
3. Run: `dotnet run` → `http://localhost:5167/`.

## Test flow
1. **Register** (UI or Swagger): member or beneficiary.
2. **Login** (UI).
3. **Dashboard** shows claims (id/email/role).
4. **Meals**:
   - As **member**, the Create form is enabled.  
   - Submit → you’ll see a message if the API route is unavailable in this build.

## Tech notes
- **Auth**: JWT stored in ProtectedLocalStorage; `ApiAuthHandler` adds `Authorization: Bearer` automatically.  
- **Role gating**: `CurrentUser` service loads `/api/auth/me` (if present) or falls back to claims; pages use `CU.IsMember` / `CU.IsBeneficiary` and server still enforces roles.  
- **API contract (target)**:
  - `POST /api/meals` (member) – create meal  
  - `GET /api/meals`, `GET /api/meals/{id}` – list/details  
  - `POST /api/orders` (beneficiary), `POST /api/orders/{id}/accept` (member)  
  - `GET /api/auth/me` – current user