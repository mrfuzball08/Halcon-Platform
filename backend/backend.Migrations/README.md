# backend.Migrations

Isolated schema project for the Halcon backend database.

This project is intentionally separated from the runtime backend (`backend/`) so migrations can evolve independently and be automated in CI/CD.

## Important

- Runtime authentication is handled by **Supabase Auth** (`auth.users`).
- This project manages application tables expected by the backend (`public.profiles`, `public.orders`).
- `public.profiles.auth_user_id` references `auth.users(id)`.

## Connection variables

Use either:

- `MIGRATIONS_DB_CONNECTION`
- `SUPABASE_DB_CONNECTION`

Example PostgreSQL connection string:

`Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true`

## Commands

From this folder:

1. Restore tools (if needed):

`dotnet tool restore`

2. Add migration:

`dotnet tool run dotnet-ef migrations add InitialSchema`

3. Generate idempotent SQL script:

`dotnet tool run dotnet-ef migrations script --idempotent -o sql/0001_idempotent.sql`

4. Apply to database (optional from CLI):

`dotnet tool run dotnet-ef database update`

For GitHub Actions, you can generate the idempotent SQL and execute it with `psql` or a serverless worker.
