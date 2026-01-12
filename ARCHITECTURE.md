# Cloudify Architecture

## 1. Architectural Goals

### Why Cloudify exists
Cloudify exists to provide a local-first cloud platform that approximates core cloud workflows (resource groups, environments, managed services) without requiring access to public cloud infrastructure. It is designed to enable rapid development, integration testing, and demos on a single developer machine while keeping system behavior deterministic and portable.

### Long-term vision
* **Provider-agnostic control plane**: A domain model and application layer that are independent of orchestration providers, enabling Docker Compose, Kubernetes, and real cloud backends to coexist behind stable ports.
* **Extensible managed service catalog**: New resource types can be added without eroding domain invariants or leaking provider-specific concerns into the core.
* **Multi-environment orchestration**: Environments become first-class, isolated orchestration units with deterministic naming and state.
* **Evolution to multi-node control plane**: The architecture anticipates future control plane distribution without invalidating local-first defaults.

### Non-goals
* **Production-grade cloud replacement**: No HA, multi-region, or managed SLAs are targeted in the MVP.
* **Security-hardening in MVP**: Authentication, authorization, SSL/TLS, and secrets management are intentionally absent today.
* **General-purpose PaaS runtime**: Cloudify focuses on infrastructure primitives and a limited managed service catalog, not a full application runtime or deployment pipeline.

## 2. High-Level System Overview

### Major components
* **Domain layer (`Cloudify.Domain`)**: Immutable core entities, value objects, and invariants.
* **Application layer (`Cloudify.Application`)**: Use-case orchestration, validation, and port interfaces.
* **Infrastructure layer (`Cloudify.Infrastructure`)**: SQLite persistence, Docker Compose orchestration, template rendering, port allocation, process execution, and host profiling.
* **API layer (`Cloudify.Api`)**: REST controllers mapping application use cases to HTTP endpoints, including RFC 7807 error modeling.
* **UI layer (`Cloudify.Ui`)**: Blazor Server UI acting as an API client, with no business logic.

### Repository layout
* **`src/`**: Production projects (`Cloudify.*`).
* **`test/`**: Test projects (`Cloudify.*.Tests`).

### Control plane vs execution plane
* **Control plane**: The API + application + domain layers act as the control plane. They accept intent, validate invariants, persist state, and drive orchestration.
* **Execution plane**: Docker Compose (invoked via a process runner) acts as the execution plane. It materializes environment state from composed YAML manifests and manages container lifecycles.

### Single-node assumption (MVP)
The system assumes a single host machine with local disk and Docker Engine. It does not attempt to model multi-node scheduling, network segmentation across hosts, or distributed state. Host capacity data is collected best-effort for display purposes only.

## 3. Clean Architecture / Hexagonal Architecture

### Layer responsibilities
* **Domain**: Encapsulates invariants and modeling choices (e.g., resource identity, environment membership, capacity/storage profiles). No infrastructure or framework dependencies.
* **Application**: Implements use cases (create environment, add resource, start/stop/scale, logs, health). Coordinates ports (state store, orchestrator, port allocator). Returns structured success/error results.
* **Infrastructure**: Implements ports using EF Core + SQLite, Docker Compose orchestration, filesystem writes, process execution, and host inspection.
* **API/UI**: Adapters that translate HTTP or UI workflows into application requests and DTOs.

### Dependency direction
Dependencies flow inward:
* API/UI depend on Application.
* Application depends on Domain and on ports (interfaces).
* Infrastructure depends on Application ports and Domain for implementations.

### Ports & adapters
Ports define boundary contracts (e.g., `IOrchestrator`, `IStateStore`, `IPortAllocator`, `ITemplateRenderer`, `ISystemProfileProvider`). Adapters implement those contracts (Docker Compose orchestrator, SQLite state store, port allocator). This allows swapping execution providers and persistence backends without altering use-case code.

### Why this architecture was chosen
Cloudify is explicitly designed for future orchestration providers. Clean Architecture isolates provider-specific behavior and persistence from domain logic, enabling substitution without eroding core invariants or contaminating the API surface.

## 4. Domain Model Design

### ResourceGroup
A `ResourceGroup` is a logical container for environments and tags. It enforces non-empty naming and provides ownership boundaries for environments.

### Environment
An `Environment` belongs to a `ResourceGroup` and contains resources. It captures:
* **Name** (`EnvironmentName`) and **NetworkMode** (`Bridge`, `Host`, `None`).
* **BaseDomain** for future DNS/ingress integration.
* **Resources** with unique names inside the environment.

### Resource abstraction
`Resource` is an abstract base class for managed services and application services. Common properties include:
* Identity, name, and owning environment ID.
* `ResourceType` (Redis/Postgres/Mongo/Rabbit/AppService).
* `ResourceState` (Provisioning/Running/Stopped/Failed/Deleted).
* Optional `CapacityProfile` and `PortPolicy`.

Concrete resources (Redis/Postgres/Mongo/Rabbit/AppService) attach resource-specific fields such as storage profile, credential profile, or container image.

### CapacityProfile
`CapacityProfile` represents CPU, memory, and replica hints. It enforces positive values and minimum replica counts. In the current implementation, only `AppService` uses replicas > 1; other resource types are treated as singletons.

### StorageProfile
`StorageProfile` defines volume name, size (GB), mount path, and persistence. It is required for stateful resources (Redis/Postgres/Mongo/Rabbit) and omitted for stateless AppService workloads.

### Domain invariants and rules
* Resource group and environment names are required and validated at construction time.
* Environments enforce unique resource names.
* Resources must belong to their owning environment or resource group when added.
* Capacity/storage profiles enforce non-empty fields and sane ranges.
* Ports must be within the valid TCP range when explicitly declared.

## 5. Application Layer (Use Cases)

### Command/query responsibilities
Use cases are explicit handlers with DTO-driven inputs/outputs:
* **Commands**: create resource groups, create environments, add resources, start/stop/restart/scale.
* **Queries**: list resource groups/environments, environment overview, resource logs, resource health.

### Statelessness expectations
Handlers are stateless and operate against injected ports. All persistent state lives in the state store (SQLite today).

### Error handling philosophy
Use cases return structured `Result`/`Result<T>` DTOs containing success flags and error codes/messages. API controllers translate these into RFC 7807 `ProblemDetails` responses with stable error codes.

### Why orchestration is triggered here
The application layer is the orchestration boundary. It validates intent, writes state, and invokes the orchestrator. This ensures consistent ordering: state is persisted before the execution plane is asked to materialize changes.

## 6. Infrastructure Layer

### State persistence (SQLite)
* **Storage**: EF Core + SQLite, stored at `./data/cloudify.db`.
* **Schema**: Resource groups, environments, resources, capacity/storage/credential profiles, port policies, port allocations, and schema versioning.
* **Invariants**: Unique indexes on environment name per resource group and resource name per environment enforce correctness beyond domain checks.

### Orchestrator adapters
`DockerComposeOrchestrator` implements `IOrchestrator` by:
* Rendering deterministic Docker Compose YAML per environment.
* Writing compose files under a per-environment directory.
* Executing Docker Compose commands via `ProcessRunner`.

### Template rendering system
`DockerComposeTemplateRenderer` generates `docker-compose.yml` using:
* Deterministic service naming derived from resource type + short ID.
* Deterministic volume naming per environment and resource.
* Environment variables from credential profiles.
* Port mappings from stored allocations.
* Optional health checks and resource constraints.

### Port allocation strategy
`PortAllocator` assigns ports by:
* Respecting requested ports when provided, validating availability against the host.
* Defaulting to resource-type base ports (e.g., 6379 for Redis, 5432 for Postgres).
* Incrementally scanning for free ports, avoiding conflicts in the environment and on the host.

### Process execution model (Docker Compose)
`ProcessRunner` executes the Docker Compose CLI with timeouts and captures stdout/stderr. It supports cancellation and ensures processes are terminated on timeout or cancellation.

## 7. Orchestration Architecture

### Environment-to-compose-project mapping
Each environment is a separate Docker Compose project. The project name is `cloudify-{environmentId}` to guarantee uniqueness and isolation.

### Service naming conventions
Services are named `{resourceType}-{shortId}` where `shortId` is the first six characters of the resource GUID. This avoids collisions across environments while keeping names stable.

### Volume naming conventions
Persistent volumes are named `cloudify-{environmentId}-{shortId}-data` and are declared explicitly in the compose file.

### Network model
Compose uses default networking with explicit host port bindings (`localhost:host:container`). Environment `NetworkMode` exists in the domain model but is not yet mapped to Compose-level networking; network isolation is currently per Compose project.

### Failure modes and recovery expectations
* **Compose command failures** surface as process errors; resource state is inferred from Compose `ps` output.
* **Missing services** are treated as `Deleted` in status/health queries.
* **Health checks** rely on container-level checks (Postgres/Mongo/Rabbit/Redis); unhealthy containers map to failed health states in the API.

## 8. Capacity & Scaling Model

### Host system awareness
`HostSystemProfileProvider` reports CPU count, total memory, and disk availability using best-effort host inspection. These values are informational and not enforced.

### CapacityProfile abstraction
Capacity profiles are stored alongside resources and surfaced to the orchestrator for possible enforcement. In Compose, only memory/cpu limits are applied where supported; replicas are used for `AppService` scaling.

### Current Compose limitations
* Compose constraints are not enforced uniformly across OS/engine implementations.
* Scaling is implemented only for `AppService` resources.

### Why capacity is modeled even when not fully enforced
The model is designed to map directly to Kubernetes resource requests/limits and cloud sizing primitives. Defining it early prevents breaking changes later and keeps application APIs stable.

### Path to Kubernetes-native scaling
A Kubernetes provider can translate capacity profiles into `requests`/`limits` and map replica counts to deployments or stateful sets. The application layer remains unchanged.

## 9. Storage Architecture

### Logical vs physical storage
* **Logical**: `StorageProfile` captures required size, mount path, and persistence semantics.
* **Physical**: Docker named volumes provide persistence in the current Compose provider.

### Volume lifecycle
Volumes are created by Compose when the environment is deployed. They persist across container restarts and are tied to deterministic names for stability.

### Persistence guarantees
Persistence is best-effort and local to the host machine. There are no replication or backup guarantees in the MVP.

### Future storage drivers
Future providers can map `StorageProfile` to Kubernetes PVCs or cloud storage services, enforcing size/IOPS constraints currently modeled but not enforced.

## 10. API Architecture

### REST-first philosophy
The API is the authoritative control plane. UI and future integrations consume the same HTTP endpoints; no separate UI control plane exists.

### DTO boundaries
Application handlers accept and return DTOs, ensuring domain objects do not leak across API boundaries. DTO validation is performed at the use-case layer.

### Error modeling (problem+json)
Failed results are translated to RFC 7807 `ProblemDetails`, with stable error codes mapped to HTTP status codes and `Type` URIs in the `https://cloudify.api/errors/{code}` namespace.

### Versioning strategy (future)
The API is currently unversioned. A future versioning strategy should be URI or header-based, with DTO evolution handled through additive changes or versioned endpoints.

## 11. UI Architecture

### API-driven UI
The UI is a pure API consumer. It issues HTTP requests to the REST API and renders responses without business logic.

### Blazor Server rationale
Blazor Server provides rapid UI iteration, shared .NET tooling, and server-side rendering without client bundling overhead. It aligns with the local-first MVP constraints.

### Why UI contains no business logic
Business rules, orchestration decisions, and validation live in the application layer. The UI only presents data and initiates commands through the API.

### Future React migration strategy
If a React front-end is introduced, the API contract remains unchanged. The Blazor UI can be replaced by a new client without touching the application or domain layers.

## 12. Observability

### Logs
Resource logs are retrieved via the orchestrator, which proxies Docker Compose logs for a given service. Logs are returned as raw text to the API consumer.

### Health checks
Resource health is inferred from `docker compose ps --format json`, mapped to `ResourceState` and `HealthStatus`. This is container-level health only.

### Current limitations
* No centralized logging, metrics, or tracing.
* No correlation IDs across API and orchestration actions.
* Health is only as accurate as Compose reporting.

### Future metrics and tracing
The architecture expects a future metrics subsystem (e.g., OpenTelemetry) to be added at the API and orchestrator boundaries without changing domain or use-case logic.

## 13. Security Model (Current & Future)

### Why MVP has no auth/SSL
Cloudify targets local development and demo workflows. Authentication, authorization, and TLS are deferred to reduce complexity and maximize usability.

### Threat model assumptions
* Single-user or trusted network usage.
* Local Docker host access is assumed.
* No exposure to the public internet.

### Planned security layers
* API authentication and role-based authorization.
* TLS termination and secure defaults.
* Secret management for credentials and connection strings.
* Audit logging for resource changes.

## 14. Extensibility Strategy

### New resource types
Add a new `Resource` subtype in the domain, extend DTOs, and implement Compose rendering/port allocation rules in infrastructure. The application layer remains stable.

### New orchestrator providers
Implement `IOrchestrator` and `ITemplateRenderer` (or their equivalents) for Kubernetes or cloud APIs. Keep provider-specific logic out of the application and domain layers.

### Plugin-style evolution
Ports and adapters provide the seams for a plugin architecture. Provider-specific assemblies can be loaded dynamically in a future evolution without altering core modules.

### Backward compatibility considerations
Maintain stable DTOs and error codes. Any domain changes should be additive or feature-flagged to preserve existing API consumers.

## 15. Trade-offs & Design Decisions

### Why Docker Compose first
* Lowest barrier to entry for local-first usage.
* Deterministic deployments on a single host.
* Fast feedback loop for infrastructure modeling.

### Why SQLite
* Embedded, zero-configuration persistence for single-node usage.
* Works across OS environments without external dependencies.
* Sufficient for MVP state scale.

### Why local-first
Local-first reduces friction for developers and makes platform behaviors deterministic. It is explicitly not a production-ready cloud.

### Known compromises
* Limited enforcement of resource constraints.
* No multi-node scheduling.
* No security or secrets handling.
* Compose-specific health and scaling semantics.

## 16. Future Architecture Evolution

### Kubernetes provider
A Kubernetes adapter will translate Cloudify resources into deployments/stateful sets/services, mapping capacity and storage profiles to native constructs.

### Multi-node control plane
A distributed control plane would separate API from execution and introduce a real scheduler. The current ports-based design allows this evolution without rewriting domain logic.

### Remote agents
Remote agents can manage execution on separate nodes, with the control plane coordinating via gRPC or HTTP. Orchestrator ports would encapsulate the transport.

### Cloud provider integration
Long-term support for managed cloud services can be added by implementing provider-specific adapters while keeping the resource model intact.

## 17. Summary

### Architectural principles recap
* **Local-first, deterministic behavior** for development and demos.
* **Clean Architecture** to isolate domain logic from provider-specific implementations.
* **Explicit domain invariants** to preserve correctness as providers expand.
* **REST-first control plane** with API-driven UI.

### Expectations for contributors
Contributors should preserve layer boundaries, keep provider-specific logic in infrastructure adapters, and evolve the domain model in a backward-compatible manner.
