# Halcon Platform

Halcon Platform is a comprehensive, enterprise-grade web application designed for construction material distribution and order tracking. It provides specialized tools for various departments (Sales, Warehouse, Route/Drivers, Purchasing, and Administration) to seamlessly manage orders from creation to delivery.

## Project Architecture

The platform follows a modern, decoupled client-server architecture, completely containerized for consistency across development and production environments.

### System Overview

*   **Frontend**: Built with **Next.js (App Router)** and **React**. Serves as the primary user interface with a premium, sleek ChatGPT-dark enterprise aesthetic.
*   **Backend**: Built with **.NET** (ASP.NET Core Web API). Provides robust, secure, and scalable RESTful endpoints.
*   **Database & Storage**: Powered by **Supabase** (PostgreSQL). Handles structured data (users, orders), authentication (JWT), and unstructured blob storage (delivery photo evidence).
*   **Infrastructure**: Fully containerized using **Docker** and orchestrated via `docker-compose`.

### Directory Structure

```text
.
├── backend/            # .NET Core API and Infrastructure layer
│   └── backend/        # Primary .NET project directory
│       ├── Controllers/    # REST API Endpoints
│       ├── Repositories/   # Data access layer (Supabase clients)
│       ├── Services/       # Core business logic
│       └── DTOs/           # Data Transfer Objects
├── frontend/           # Next.js Application
│   ├── src/app/        # App Router pages and layouts
│   ├── src/components/ # Shared UI components
│   └── src/lib/        # API client, auth, and utilities
├── localdocs/          # Internal backend API documentation
└── docker-compose.yml  # Multi-container orchestration
```

## Running the Application

Ensure you have Docker and Docker Compose installed.

### 1. Environment Configuration

Copy the example environment files and populate them with the required keys (specifically for Supabase):

```bash
cp .env.example .env
```

Ensure `NEXT_PUBLIC_API_URL` is configured to point to the backend's `/api` route (e.g., `http://localhost:5001/api`).

### 2. Start Services

Run the following command in the base directory:

```bash
docker compose up --build
```

This spins up:
- The **.NET backend** on the configured `API_PORT` (default `5001`).
- The **Next.js frontend** on the configured `FRONTEND_PORT` (default `5000`).

Access the application UI by visiting `http://localhost:5000`. 

## Architecture Decisions

- **Role-Based Access Control (RBAC)**: Deeply integrated across both the frontend (UI hiding/showing logic) and backend (endpoint authorization via JWT attributes). Roles include `ADMIN`, `SALES`, `WAREHOUSE`, `ROUTE`, and `PURCHASING`.
- **JWT Authentication**: User credentials and tokens are processed by Supabase Auth logic underneath. The backend verifies incoming tokens dynamically, allowing stateless execution.
- **Evidence Handling**: The system requires visual proof of loading and delivery. These workflow images are securely transmitted as standard multipart form data and stored inside a public-read Supabase Storage bucket (`order-evidence`).
