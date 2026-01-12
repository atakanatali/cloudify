# Contributing to Cloudify

## 1. Introduction
Welcome to Cloudify and thank you for your interest in contributing. We are building a local-first cloud platform designed around Clean Architecture / Hexagonal Architecture principles. This project is meant to mature into a long-lived cloud orchestration platform, and we are uncompromising about architectural integrity.

**Philosophy:** quality over speed, architecture over shortcuts. We prefer deliberate, well-reviewed, and predictable changes even when that means moving slower.

**Who this guide is for:** senior backend engineers, DevOps engineers, platform architects, and any contributor who values long-term maintainability and disciplined systems design.

## 2. Code of Conduct (Short Reference)
We expect professional, respectful, and constructive collaboration. Please read the full Code of Conduct in [CODE_OF_CONDUCT.md](CODE_OF_CONDUCT.md). It is required for all contributors.

## 3. Project Philosophy
Cloudify is built to last decades, not sprints. The following principles are foundational and non-negotiable:

- **Long-term thinking (10+ years):** optimize for stability, clarity, and evolution.
- **Deterministic systems:** avoid hidden side effects, non-repeatable behavior, or non-deterministic outputs.
- **Minimal dependencies:** introduce external libraries only when they provide clear, long-term value.
- **Explicit over implicit behavior:** make dependencies, configuration, and runtime behavior discoverable and predictable.
- **Local-first, cloud-ready mindset:** everything must run locally with production-grade characteristics, and also be deployable to real cloud environments without architectural rework.

## 4. Architecture Rules (NON-NEGOTIABLE)
Cloudify follows Clean Architecture / Hexagonal Architecture. If a change violates these boundaries, it will not be merged.

### 4.1 Clean Architecture Boundaries
- **Domain** is the core: immutable business rules and policies.
- **Application** orchestrates use cases and workflows.
- **Infrastructure** provides adapters for persistence, networks, cloud providers, and external systems.
- **UI/API** is a delivery mechanism and must remain thin.

### 4.2 Domain Rules (No Infrastructure Leakage)
- Domain must not reference Application, Infrastructure, UI, or framework-specific code.
- Domain must remain pure and deterministic, with no I/O or side effects.
- Any violation of domain purity is a hard stop for review.

### 4.3 Application Layer Responsibilities
- Implements use cases and coordinates domain logic.
- Depends only on Domain and abstractions (interfaces/ports).
- Must not depend on Infrastructure implementations or UI frameworks.

### 4.4 Infrastructure Adapters Rules
- Implements interfaces defined by Application or Domain.
- Contains all external I/O, persistence, API clients, and provider-specific code.
- Must not leak infrastructure concerns into Domain or Application.

### 4.5 UI Responsibilities and Limits
- UI/API should only handle input validation, mapping, and presentation.
- UI must not contain business rules, orchestration logic, or infrastructure behavior.

### 4.6 Forbidden Dependencies per Layer (Explicit)
- **Domain**: must not depend on Application, Infrastructure, UI, or external frameworks.
- **Application**: must not depend on Infrastructure or UI; depends only on Domain and abstractions.
- **Infrastructure**: must not depend on UI; must not introduce business rules.
- **UI**: must not depend on Infrastructure directly; should call Application services only.

## 5. How to Contribute
### 5.1 Fork & Branch Strategy
- Fork the repository.
- Create topic branches from `main`.
- Use clear branch names: `feature/<short-description>` or `fix/<short-description>`.

### 5.2 Commit Message Expectations
- Use concise, imperative commits.
- One logical change per commit.
- Prefer: `Add`, `Fix`, `Refactor`, `Document`, `Test`.
- Example: `Add orchestrator interface for provider adapters`.

### 5.3 Pull Request Scope Guidelines
- Keep PRs focused and reviewable.
- Avoid mixing refactors with feature changes.
- Large changes require an issue and design discussion first.

### 5.4 When to Open an Issue vs PR
- **Open an issue** for architectural changes, new resources/providers, or cross-cutting refactors.
- **Open a PR** for small, scoped improvements or well-defined fixes.

## 6. Coding Standards
### 6.1 General Principles
- Follow **SOLID** principles.
- Prefer composition over inheritance.
- Use immutability where reasonable to reduce side effects.

### 6.2 Async/Await Best Practices
- Avoid blocking calls (`.Result`, `.Wait()`).
- Use async all the way through.
- Do not create “fire and forget” tasks without explicit lifecycle management.

### 6.3 CancellationToken Usage
- Public async APIs must accept `CancellationToken`.
- Pass tokens through all async layers.
- Never ignore cancellation in infrastructure or long-running operations.

### 6.4 Exception Handling Rules
- Do not use exceptions for control flow.
- Catch exceptions only when you can add context or translate them into domain/application errors.
- Do not swallow exceptions.

## 7. Documentation Expectations
### 7.1 XML Documentation Requirements
- Public types and APIs should include XML documentation.
- If a method defines a non-obvious invariant or side effect, document it.

### 7.2 README vs ARCHITECTURE.md
- Update **README** for user-facing behavior or usage changes.
- Update **ARCHITECTURE.md** for structural, layered, or system-design changes.

### 7.3 Inline Comments vs Docs
- Inline comments are for “why”, not “what”.
- If code is complex enough to require heavy inline comments, consider extracting or documenting higher-level intent instead.

## 8. Testing Guidelines
### 8.1 Unit Tests vs Integration Tests
- Unit tests validate domain and application logic.
- Integration tests validate infrastructure adapters and system boundaries.

### 8.2 Domain Test Expectations
- Domain logic must be thoroughly covered by deterministic unit tests.
- Avoid time-dependent or randomized tests without controlled clocks/inputs.

### 8.3 Infrastructure Test Limitations
- Mock external systems unless explicitly running against a local, deterministic environment.
- Do not write tests that require live cloud resources in CI.

### 8.4 Deterministic Test Behavior Requirement
- Tests must be repeatable and deterministic.
- Flaky tests will be removed.

## 9. Adding New Resources or Providers
### 9.1 Adding a New Resource Type (e.g., Kafka, MySQL)
- Define the domain model and interfaces in **Domain** and **Application**.
- Add use cases in Application that describe resource lifecycle operations.
- Implement provider-specific adapters in Infrastructure.
- Provide unit tests for domain/application logic and integration tests for adapters.
- Open an issue and request a design review before implementation.

### 9.2 Adding a New Orchestrator Provider (e.g., Kubernetes)
- Add a new provider adapter implementing the orchestrator interfaces.
- Ensure provider behavior is deterministic and explicitly configurable.
- Document provider-specific assumptions and limitations.
- Require architectural review prior to merging.

### 9.3 Required Interfaces and Design Review
- All new resources/providers must be defined via interfaces owned by Application or Domain.
- Design reviews must confirm boundary integrity and long-term maintainability.

## 10. Performance & Scalability Considerations
- Avoid premature optimization, but do not ignore performance regressions.
- Think in terms of scale even for MVP code.
- Prefer clear, predictable performance characteristics over micro-optimizations.

## 11. Review Process
### 11.1 What Reviewers Will Look For
- Clean Architecture compliance.
- Deterministic behavior and minimal side effects.
- Clear separation of concerns.
- Minimal, justified dependencies.
- Sufficient tests and documentation.

### 11.2 Common Reasons PRs Are Rejected
- Layer boundary violations.
- Infrastructure code in Domain or Application.
- Non-deterministic behavior without explicit justification.
- Missing tests or documentation.
- Overly broad or unfocused scope.

### 11.3 Architecture Consistency over Feature Completeness
- A smaller, correct change is preferred over a complete but inconsistent feature.
- Reviewers will prioritize long-term architectural integrity.

## 12. Final Notes
We welcome thoughtful contributions that respect Cloudify’s architectural principles. If in doubt, open an issue and discuss before coding. Architectural discipline is a core value of this project, and it is not negotiable.
