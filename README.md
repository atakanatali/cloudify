# Cloudify

## Quick Links

- [Architecture](ARCHITECTURE.md)

## 1. Project Overview

Cloudify is a **local-first cloud platform** that delivers an AWS/Azure/GCP-like experience on a developer’s own machine. It uses Docker Compose as the initial orchestration backend and exposes a clean API plus a Blazor Server UI for managing local infrastructure resources.

Cloudify solves the problem of **developing, testing, and demoing cloud-like infrastructure** without requiring a real cloud account, credentials, or a complex multi-node setup. It is intended for local development, team demos, and fast iteration on infrastructure patterns before committing to production-grade cloud services.

**Cloudify is not a replacement for real cloud providers.** It does not offer global availability, managed HA, elastic multi-region scaling, or production-grade security controls. It is a local, single-node platform designed to approximate cloud workflows while remaining simple and deterministic.

## 2. Key Concepts

- **Resource Group**: A logical container for related environments and resources. Mirrors the grouping semantics of major cloud providers.
- **Environment**: An isolated deployment target (e.g., `prod`, `test`, `dev`). Each environment has its own orchestration boundary.
- **Resource**: A provisionable infrastructure component (e.g., Redis, PostgreSQL, MongoDB, RabbitMQ, App Service).
- **Capacity Profile**: A logical model for CPU/memory/replica hints. Today it maps to Docker Compose constraints where possible.
- **Storage Profile**: A logical definition of persistence (persistent vs non-persistent) and storage allocation hints.
- **Orchestrator Provider**: The pluggable backend that executes resource lifecycle operations. Docker Compose is the current provider; Kubernetes is planned.

## 3. Features (MVP)

What works today:

- Resource Groups and Environments
- Provisioning resources via API/UI:
  - Redis
  - PostgreSQL
  - MongoDB
  - RabbitMQ (with management UI)
  - App Service (single .NET application via Docker image)
- Lifecycle operations: deploy, start, stop, restart, scale
- Logs and basic health visibility
- Capacity and storage profiles (logical hints mapped to Compose)
- Local-first operation (no cloud credentials)

Explicit MVP limitations:

- **No SSL/TLS**
- **No authentication/authorization**
- **No secrets vault**

## 4. Architecture Overview

Cloudify is built with **Clean Architecture (Hexagonal / Ports & Adapters)** to keep the domain model independent from infrastructure choices:

- **Domain**: Core entities, invariants, and business rules.
- **Application**: Use cases, orchestration, and policies.
- **Infrastructure**: Providers (Docker Compose), persistence (SQLite), and external integrations.
- **API**: ASP.NET Core Web API exposing REST endpoints.
- **UI**: Blazor Server consuming the API.

This separation ensures that future orchestrators (e.g., Kubernetes) or storage backends can be added without polluting domain logic. It also keeps the platform maintainable as complexity grows.

## 5. Orchestration Model

Cloudify uses **one Docker Compose project per Environment**.

- **Project naming**: `cloudify-{resourceGroup}-{environment}`
- **Service naming**: `{resourceType}-{resourceName}` (e.g., `postgres-primary`)
- **Port allocation**: Deterministic per service, with environment isolation to avoid collisions
- **Volume strategy**:
  - Named volumes for persistent storage
  - Anonymous or tmp volumes for ephemeral services

The orchestrator provider is responsible for translating Cloudify resource definitions into Compose services and running lifecycle operations through Docker Compose v2.

## 6. Capacity & Scaling Philosophy

Today, scaling is implemented for **App Service replicas** via Docker Compose service scaling. Other resources are provisioned as single instances.

- **Compose limitations**: Hard limits on CPU/memory enforcement depend on the underlying Docker engine and host OS.
- **Future-ready modeling**: Capacity profiles are designed to map cleanly to Kubernetes resource requests/limits and cloud provider instance sizing.
- **Host awareness**: Cloudify assumes a single host with finite capacity and does not overcommit resources.

## 7. Storage Model

Cloudify uses **logical storage profiles** to describe persistence requirements:

- **Persistent volumes**: Durable state stored in named Docker volumes
- **Non-persistent volumes**: Ephemeral data for caches or stateless services

Current Compose limitations mean that storage quotas and IOPS cannot be enforced. The model is defined now so future providers (e.g., Kubernetes, managed cloud storage) can honor these constraints.

## 8. Getting Started (Local Development)

### Prerequisites

- Docker Engine + Docker Compose v2
- .NET 8 SDK

### Clone the repository

```bash
git clone <repo-url>
cd cloudify
```

### Run the platform

```bash
./scripts/run.sh
```

### First access to the UI

- UI: `http://localhost:5000`
- API (Swagger): `http://localhost:5000/swagger`

### Folder structure

- `data/` — SQLite database and local state
- `environments/` — Generated environment definitions and Compose files
- `scripts/` — Utility scripts for local development

## 9. Quickstart Tutorial

1. **Create a Resource Group**
   - Example: `rg-demo`
2. **Create Environments**
   - `prod` and `test`
3. **Add resources**
   - Redis, PostgreSQL, MongoDB, RabbitMQ
4. **Add App Service**
   - Provide a Docker image for the .NET service
5. **View logs & health**
   - Use UI or API to inspect service status
6. **Scale App Service**
   - Increase replicas via UI/API

## 10. API Overview

Cloudify is **REST-first**. The UI is a consumer of the API, not a separate control plane.

- Swagger is available at `/swagger`
- Typical endpoints include:
  - `/api/resource-groups`
  - `/api/environments`
  - `/api/resources`
  - `/api/app-services`
  - `/api/lifecycle/{start|stop|restart|scale}`

This list is intentionally high level; see Swagger for full request/response details.

## 11. UI Overview

The UI is inspired by AWS-style console navigation, optimized for operational clarity:

- Responsible for:
  - Viewing resource groups and environments
  - Initiating lifecycle operations
  - Viewing logs, status, and health
- Intentionally **not** responsible for:
  - Any direct infrastructure logic
  - Security enforcement
  - Long-running orchestration (delegated to API)

## 12. Design Principles

- **SOLID** and clear boundaries
- **Minimal dependencies** to keep the core portable
- **Deterministic behavior** for repeatable local environments
- **Infrastructure as code (conceptual)** with declarative intent
- **Local-first philosophy** to reduce friction in development

## 13. Roadmap (Explicit)

- Kubernetes provider
- App Service source build (buildpack or Dockerfile support)
- Secrets management and SSL
- Multi-node support
- Plugin system for new resource types/providers

## 14. Limitations (Honest MVP Section)

- Single-node only
- Docker Compose orchestration constraints
- No HA for databases or brokers
- No security layer (authn/authz, SSL, secrets)
- Not suitable for production workloads

## 15. Contribution Guidelines (High level)

- Follow established architecture boundaries (Domain/Application/Infrastructure/API/UI)
- Keep provider-specific logic in Infrastructure adapters
- Prefer deterministic, testable use cases in Application layer
- Propose new providers/resources via issues or design docs before large PRs

## 16. License

This project is licensed under the terms described in the [LICENSE](LICENSE) file.
