# Halcon Platform Backend

This folder contains the server-side components for Halcon logistics operations. The backend is split into independent projects so runtime code, tests, and schema evolution can evolve with clear boundaries.

## Components

| Component | Path | Purpose | .NET Version |
| --- | --- | --- | --- |
| Backend API | backend/backend | Main ASP.NET Core Web API for authentication, users, orders, public tracking, and image evidence flows | .NET 10 |
| Unit Tests | backend/backend.Tests | Isolated test project for backend business rules and service behavior | .NET 10 |
| Database Migrator | backend/backend.Migrations | Isolated schema and migration project for database structure lifecycle | .NET 8 |

## Backend API Overview

The Backend API is the main application entry point for business operations. It exposes secured and public endpoints and enforces role-based behaviors for order lifecycle transitions.

### Main capabilities

- Authentication flow backed by Supabase Auth and JWT validation.
- Role-aware authorization for admin, sales, warehouse, route, and purchasing scenarios.
- Order management with logical deletion and restore behavior.
- Public order tracking endpoint for customer visibility.
- Evidence image handling for loading and delivery stages.
- Global API error handling middleware with consistent API exceptions.

### Architectural shape

- Controllers handle HTTP contract and authorization.
- Services implement business rules and validation.
- Repositories encapsulate data access.
- Shared common utilities provide validation, mapping, and error handling.

## Unit Tests Project Overview

The Unit Tests project is intentionally separated from the runtime API to keep test dependencies out of production builds.

### Current test focus

- Order status transition rules and authorization behavior.
- Constraints such as required delivery photo before delivered status.
- Public tracking response behavior based on order status.
- Domain validation helpers for role and status values.

### Design goal

- Fast, deterministic tests centered on business rules.
- No production package contamination in the API project.

## Database Migrator Project Overview

The Database Migrator project is dedicated to schema evolution and migration artifacts. It is separated from the runtime API so schema changes can be reviewed, validated, and automated independently.

### Responsibilities

- Maintain schema entities and migration history.
- Represent backend-owned tables expected by the application domain.
- Support CI compile validation for migration code integrity.

## Why this separation exists

- Cleaner production dependency graph in the main API.
- Safer lifecycle management for schema changes.
- Independent CI validation per concern: application behavior, tests, and migrations.
