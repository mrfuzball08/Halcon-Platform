# Halcon Platform - Frontend

This is the Next.js frontend application for the **Halcon Platform**, designed for managing construction material distribution.

## Tech Stack & Specifications

*   **Framework**: Next.js 16 (App Router)
*   **Runtime & Package Manager**: Bun
*   **Language**: TypeScript
*   **Styling**: Tailwind CSS v4 (Custom configured for enterprise UI)
*   **Icons**: SVG components natively embedded (e.g., `<Logo />`)

### Design Aesthetic & Theme
The frontend completely abandons generic, bright layouts. It employs a **"ChatGPT-dark"** premium enterprise interface.
- **Color Palette**: Backgrounds ranging from pure dark `#171717` to elevated surface panels like `#212121`.
- **Accents**: Deep, muted teal (`#10a37f` for primary actions) alongside `#ef4444` (`red-500`) for destructive actions.
- **Micro-interactions**: Hover effects, glow states, transparent borders (`rgba(255,255,255,0.08)`), and crisp tracking for visual density.
- **Typography**: Strictly optimized system fonts (`Geist`, sans and mono) emphasizing numerical legibility for inventory and order numbers.

## Features & Implementation Details

### Role-Based Access Control (RBAC)
User authorization operates at both the component level and route level:
- `useAuth`: A custom hook that consumes JWT tokens and dynamically checks roles like `hasRole("ADMIN", "WAREHOUSE")`.
- UI Elements (like the "Edit Order" modal or "Advance Status" panel) are hidden or shown conditionally based on the user's role.

### Image & Evidence Handling
- Uses the `FormData` interface to seamlessly submit unstructured image blobs (for loading and delivery proof) directly to the `.NET` backend.
- Displays cross-domain `url` resources originating from the cloud storage bucket via `<img>` with specific Next.js optimizations turned off to allow rapid, unrestricted UI mounting.

### Centralized API Integration (`lib/api.ts`)
The entire application proxies HTTP requests through a heavily centralized `request()` helper within `lib/api.ts`:
- **Auth Token Injection**: Automatically retrieves `halcon_token` from `localStorage` and appends it to outgoing requests.
- **Response Handling**: Seamlessly unpacks `.NET` server errors and standardizes `StatusCode` data structures.
- **Mock Fallback System**: Implements a zero-migration toggle: turning on the `NEXT_PUBLIC_MOCK_AUTH=true` system variable totally bypasses live backend fetches by loading `lib/mock-data.ts`, enabling safe frontend UI prototyping.

## Available Scripts

Using **Bun**, execute the following essential commands inside the `/frontend` directory:

- `bun dev`: Starts the Next.js hot-reloaded development server locally on port 3000.
- `bun run build`: Compiles, minifies, and emits a standalone production image.
- `bun run lint`: Invokes ESLint and strict TypeScript type-checking logic.
