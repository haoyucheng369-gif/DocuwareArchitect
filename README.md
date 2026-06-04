# DocumentPlatformArchitect

This repository models an enterprise document platform integration architecture
with a REST-first backend, an optional .NET SDK wrapper, a platform-facing web
client, a third-party consumer application, and a Keycloak-backed identity
boundary.

The goal is not to reproduce vendor internals. The project focuses on the
main integration shape of a document platform: browser-facing platform
operations use REST endpoints directly, while external .NET integrations can
use a typed client library over the same REST API.

## Architecture Overview

```mermaid
flowchart TB
    subgraph Core["DocumentPlatformArchitect Core"]
        I[Keycloak Identity Service]
        R[Platform.RestApi]
        S[Platform.DotNetSdk]
    end

    subgraph FirstParty["Platform UI"]
        W[Platform.WebClient]
    end

    subgraph External["Third-party integration"]
        T[ThirdParty.Consumer]
    end

    W -->|HTTP / REST| R
    W -.->|OIDC cookie login - next step| I
    T -->|request user token| I
    T -->|SDK call| S
    S -->|HTTP / REST with bearer token| R
    R -->|validate JWT issuer, audience, roles| I
```

> Note: this diagram describes the current implementation. The web client calls
> the REST API directly; OIDC cookie login for the web client is the next step.
> The `.NET SDK` remains an optional wrapper for third-party .NET
> applications.

## Component Responsibilities

- **Keycloak Identity Service**: OAuth2/OpenID Connect identity boundary used by both the browser-facing web client path and the SDK-based integration path.
- **Platform.RestApi**: core REST platform exposing document resources and platform APIs. Document endpoints are protected with JWT bearer authentication issued by Keycloak.
- **Platform.DotNetSdk**: .NET SDK wrapper that encapsulates REST requests and exposes a developer-friendly client interface (`IPlatformClient`). The SDK accepts a host-provided token provider and attaches the returned bearer token to REST API requests. It does not own the OAuth client registration.
- **Platform.WebClient**: MVC platform application that calls `Platform.RestApi` directly, similar to a browser-hosted product UI using platform endpoints.
- **ThirdParty.Consumer**: external consumer app simulating a third-party integration that references the SDK DLL and calls the platform through client methods.

## Architecture Coverage

- A core REST API as the main platform boundary.
- A typed .NET client library over the REST API.
- A first-party web client that calls REST endpoints directly.
- A third-party .NET consumer that obtains a user token through its own OAuth client registration and calls the same REST platform through the SDK.
- Identity/token separation from platform operations.
- Dependency-injected HTTP clients and configuration-driven service endpoints.
- Docker Compose orchestration for the platform services.
- A document workflow slice: list documents, create documents, and consume them from another application.

## Design Principles

- **Separation of concerns**: backend service, SDK wrapper, platform UI, and third-party consumer are clearly separated.
- **REST-first integration**: the REST API is the core platform contract.
- **Optional SDK layer**: the `.NET SDK` provides a typed wrapper for .NET applications without replacing the REST API.
- **Product-style boundaries**: each project has a focused role and communicates through explicit contracts.
- **Pluggable identity concept**: identity is separated from platform operations through OAuth2/OpenID Connect and JWT bearer validation.

## Scope

This architecture model currently implements document operations only. Areas
such as metadata, tasks, roles, groups, annotations, workflow activities,
document validation, and collaboration are outside the current scope.

Authentication is represented as a separate boundary backed by Keycloak. The
current repository includes a realm import with clients for the web client, REST
API, and a third-party consumer application. REST API token validation is
enabled. The SDK delegates token resolution to the host application; the current
third-party consumer reads a bearer token from the incoming request and forwards
it through the SDK.
WebClient OIDC cookie login is the next integration step.

## Running the Architecture

### Build all projects

```powershell
dotnet build
```

### Run with Docker Compose

```powershell
.\start-docker-with-swagger.ps1 -Build
```

This script builds and starts all services, then opens:

- REST API Swagger: `http://localhost:5000/swagger`
- WebClient UI: `http://localhost:5001`
- ThirdParty Consumer Swagger: `http://localhost:5002/swagger`
- Keycloak Admin Console: `http://localhost:8080/admin/master/console/`

Keycloak local admin credentials for the Admin Console:

- Username: `admin`
- Password: `admin`

Imported realm:

- Realm: `document-platform`
- Web client: `platform-webclient`
- Third-party OAuth client: `thirdparty-consumer`
- REST API client: `platform-rest-api`
- Realm user: `architect.user` / `password` with `platform-user`
- Realm admin: `architect.admin` / `password` with `platform-user` and `platform-admin`

For local API verification, use the ThirdParty Consumer Swagger UI to request a
user token. Paste the returned bearer token into the Swagger Authorize dialog
before calling protected document endpoints.

Role-based verification endpoints are also available:

- `POST /api/token/user` returns a token for `architect.user` by default
- `POST /api/token/admin` returns a token for `architect.admin` by default
- `GET /api/documents` allows `platform-user` or `platform-admin`
- `GET /api/documents/confidential` allows `platform-admin` only

Expected result through ThirdParty Consumer Swagger:

- `architect.user` token -> `/api/documents` returns `200`
- `architect.user` token -> `/api/documents/confidential` returns `403`
- `architect.admin` token -> both document endpoints return `200`

### Run projects individually

```powershell
dotnet run --project Platform.RestApi\Platform.RestApi.csproj
dotnet run --project Platform.WebClient\Platform.WebClient.csproj
dotnet run --project ThirdParty.Consumer\ThirdParty.Consumer.csproj
```

## Key Endpoints

- REST API `GET /api/documents` - read documents
- REST API `POST /api/documents` - create a document
- REST API `GET /api/documents/confidential` - read admin-only confidential documents
- ThirdParty Consumer `POST /api/token/user` - get a user token for Swagger testing
- ThirdParty Consumer `POST /api/token/admin` - get an admin token for Swagger testing
- ThirdParty Consumer `GET /api/documents` - call the REST API through the SDK using the supplied bearer token
- ThirdParty Consumer `POST /api/documents` - create a document through the SDK using the supplied bearer token
- ThirdParty Consumer `GET /api/documents/from-sdk` - SDK-backed document query in the third-party consumer
- ThirdParty Consumer `GET /api/documents/confidential` - call the admin-only document endpoint through the SDK

## Why this architecture exists

This project is intended to show practical platform design boundaries:

- a backend service with a clear HTTP boundary
- a reusable SDK abstraction layer
- a platform-facing UI
- a separate third-party integration surface
- a dedicated OAuth2/OpenID Connect identity boundary

It is a scoped architecture implementation, not a complete document management
system.
