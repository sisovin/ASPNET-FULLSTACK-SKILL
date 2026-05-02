# ASPNET-FULLSTACK-SKILL

![.NET](https://img.shields.io/badge/.NET-8_LTS-512BD4?logo=dotnet&logoColor=white)
[![CI](https://github.com/OWNER/REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/ci.yml)
[![License](https://img.shields.io/github/license/OWNER/REPO)](LICENSE)

> Replace `OWNER/REPO` in badge URLs with your GitHub repository slug after pushing this project.

Comprehensive skill pack for full-stack and backend engineering with ASP.NET technologies.

## Table Of Contents

- [Scope](#scope)
- [Repository Layout](#repository-layout)
- [Reference Map (A-H)](#reference-map-a-h)
- [Quick Start](#quick-start)
- [UI/UX Governance Extensions](#uiux-governance-extensions)
- [Hook Setup](#hook-setup)
- [Strict Framework Mode](#strict-framework-mode)
- [Development Workflow](#development-workflow)
- [Compatibility Notes](#compatibility-notes)
- [Contributing](#contributing)
- [License](#license)

This repository is structured for progressive usage:
- `SKILL.md` is the entry point.
- `references/` contains deep technical sections A-H.
- `scripts/` provides reusable templates and tooling helpers.
- `.github/` includes repo-level agent customizations.

## Scope

This skill pack covers:
- ASP.NET Core MVC (modern)
- ASP.NET MVC 5 (legacy)
- ASP.NET Core Web API
- Minimal APIs (.NET 6+)
- MySQL integration (EF Core and ADO.NET)
- Full-stack frontend integration patterns
- Cross-platform deployment and cloud delivery
- GitHub workflow and release management
- Security, performance, and production best practices

Official framework references:
- ASP.NET Core: https://github.com/dotnet/aspnetcore
- ASP.NET MVC 5 (AspNetWebStack): https://github.com/aspnet/AspNetWebStack

## Repository Layout

```text
aspnet-fullstack-skill/
|-- SKILL.md
|-- README.md
|-- references/
|   |-- A-architecture.md
|   |-- B-backend.md
|   |-- C-database.md
|   |-- D-frontend.md
|   |-- E-architecture-patterns.md
|   |-- F-cloud-deploy.md
|   |-- G-github.md
|   |-- H-best-practices.md
|   `-- I-UI-UX-Pro-Design.md
|-- scripts/
|   |-- Program.cs.template
|   |-- aspnet-cheatsheet.sh
|   |-- setup-git-hooks.ps1
|   `-- ui-precommit-check.ps1
|-- .githooks/
|   `-- pre-commit
`-- .github/
    |-- agents/
    |   `-- ui-audit-refactor.agent.md
    |-- instructions/
    |   `-- frontend-ui-review.instructions.md
    `-- skills/
        `-- ui-ux-pro-max/
```

## Reference Map (A-H)

- Section A: Architecture and framework internals, routing, DI, middleware, filters, versioning, Swagger
- Section B: Backend service implementation, REST, serialization, auth, CORS, OWASP
- Section C: MySQL data access with EF Core and ADO.NET, migrations, optimization
- Section D: Full-stack frontend integration, CSS and JS patterns, client-server communication
- Section E: Clean architecture, layered architecture, SOLID, optional DDD
- Section F: Cross-platform runtime, reverse proxy, Docker, cloud deployment, CI/CD
- Section G: GitHub workflow, branching strategy, PR quality, release process
- Section H: Coding standards, logging, exception handling, performance, security hardening

## Quick Start

### 1. Open the project in VS Code

Open this folder as the workspace root:
- `D:\laragon\www\aspnet-fullstack-skill`

### 2. Start from the skill entry point

Read:
- `SKILL.md`

Then load the matching deep references in `references/` based on the task domain.

### 3. Use included scripts

- `scripts/Program.cs.template`: canonical ASP.NET Core startup and pipeline wiring
- `scripts/aspnet-cheatsheet.sh`: command reference for dotnet, ef, docker, git

## UI/UX Governance Extensions

This repository includes team-level UI quality enforcement.

### Repo-level frontend instruction

File:
- `.github/instructions/frontend-ui-review.instructions.md`

Purpose:
- Enforce frontend review discipline for accessibility, contrast, focus states, responsiveness, and component consistency.

### Custom UI audit agent mode

File:
- `.github/agents/ui-audit-refactor.agent.md`

Purpose:
- Specialized workflow for UI audits and component refactors with constrained tooling.

### Pre-commit UI enforcement

Files:
- `.githooks/pre-commit`
- `scripts/ui-precommit-check.ps1`
- `scripts/setup-git-hooks.ps1`

Checks:
- Missing visible focus styles on interactive UI
- Potential low-contrast text/background hex pairs
- Strict framework-aware mode for React, Vue, and Tailwind focus-visible requirements

## Hook Setup

Run once per local clone:

```powershell
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\setup-git-hooks.ps1
```

Verify:

```powershell
git config --get core.hooksPath
```

Expected value:

```text
.githooks
```

## Strict Framework Mode

The checker supports selective strict enforcement by framework.

Examples:

```powershell
# Default strict mode (React, Vue, Tailwind)
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\ui-precommit-check.ps1 -Strict

# Strict mode for React and Vue only
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\ui-precommit-check.ps1 -Strict -StrictFrameworks React,Vue

# Strict mode for Tailwind only
pwsh -NoProfile -ExecutionPolicy Bypass -File .\scripts\ui-precommit-check.ps1 -Strict -StrictFrameworks Tailwind
```

## Development Workflow

1. Pick the section(s) from `references/` that match the requested task.
2. Draft implementation aligned with architecture and security constraints.
3. Apply UI review checklist for frontend-facing changes.
4. Run relevant tests and validation commands.
5. Submit via PR with concise change summary and risk notes.

## Recommended Quality Baseline

- Use async-first data and API operations where possible.
- Keep controllers thin; move business logic to services.
- Prefer explicit DTO boundaries for API contracts.
- Validate inputs server-side regardless of client checks.
- Standardize structured logging and error envelopes.
- Design for deployment parity between local, CI, and production.

## Compatibility Notes

- ASP.NET Core guidance assumes modern .NET (recommended: .NET 8 LTS).
- ASP.NET MVC 5 guidance applies to .NET Framework 4.x maintenance scenarios.
- MySQL patterns include both EF Core and lower-level ADO.NET for mixed stacks.

## Contributing

- Keep `SKILL.md` concise and route details to `references/`.
- Preserve section naming consistency (`A-...` through `H-...`).
- Prefer additive updates over broad rewrites in deep reference files.
- Keep examples runnable and production-safe.

## License

This project is licensed under the MIT License. See `LICENSE`.
