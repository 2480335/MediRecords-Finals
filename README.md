# MediRecords — Clinical EMR Platform

End-to-end clinical record-keeping system covering patient registration, appointments, encounter charting (SOAP notes, vitals, nursing notes), orders (lab, imaging, prescriptions), billing (charges, unbilled queue), reports, and document management.

The platform serves **five role personas** — Admin, FrontDesk, Physician, Nurse, LabTech — each with a tailored workspace and explicit backend authorization.

---

## Table of contents

1. [Tech stack](#tech-stack)
2. [High-level architecture](#high-level-architecture)
3. [Project layout](#project-layout)
4. [Authentication & authorization flow](#authentication--authorization-flow)
5. [Role-based routing & landing dashboards](#role-based-routing--landing-dashboards)
6. [End-to-end patient journey](#end-to-end-patient-journey)
7. [Appointment → Encounter flow](#appointment--encounter-flow)
8. [Physician workspace & "Chart actions"](#physician-workspace--chart-actions)
9. [Prescription create + auto-merge](#prescription-create--auto-merge)
10. [Care plan create + auto-merge](#care-plan-create--auto-merge)
11. [Billing & charges flow](#billing--charges-flow)
12. [Backend request lifecycle](#backend-request-lifecycle)
13. [Domain model (key entities)](#domain-model-key-entities)
14. [API surface summary](#api-surface-summary)
15. [Running locally](#running-locally)

---

## Tech stack

| Layer       | Stack                                                                      |
| ----------- | -------------------------------------------------------------------------- |
| Backend     | ASP.NET Core (.NET 10), EF Core, SQL Server, JWT auth, AutoMapper, Swagger |
| Frontend    | Angular 18 (standalone components, signals), Reactive Forms, Bootstrap 5   |
| Auth        | JWT bearer tokens with role claims                                         |
| Persistence | SQL Server (`MediRecordsTestDB`)                                           |

---

## High-level architecture

```mermaid
flowchart LR
  subgraph Browser["🌐 Angular SPA  (MediRecordsWeb)"]
    UI["Feature Components<br/>(standalone, signals)"]
    SVC["Core Services<br/>(HttpClient wrappers)"]
    INT["JWT Auth Interceptor"]
    GRD["Auth + Role Guards"]
    UI --> SVC
    SVC --> INT
    GRD -.gate.-> UI
  end

  subgraph API["🛠 ASP.NET Core API  (MediRecords)"]
    CTL["Controllers<br/>[Authorize(Roles=…)]"]
    APP["Service Layer<br/>(business rules)"]
    REPO["Repository Layer<br/>(EF Core)"]
    DTO["DTOs ↔ AutoMapper"]
    CTL --> APP
    APP --> REPO
    APP <--> DTO
  end

  subgraph Data["💾 Persistence"]
    DB[("SQL Server<br/>MediRecordsTestDB")]
  end

  Browser -- "HTTPS + JWT Bearer" --> API
  REPO --> DB
```

---

## Project layout

```
MediRecords-Backend-main/
├── MediRecords/                  # ASP.NET Core API
│   ├── Controllers/              # 24 controllers, one per resource
│   ├── Services/                 # Per-resource service folders (business rules)
│   ├── Repository/               # Per-resource repository folders (EF Core)
│   ├── Dto/                      # Request / Response DTOs
│   ├── MappingProfiles/          # AutoMapper profiles
│   ├── Migrations/               # EF Core migrations
│   ├── Utility/                  # Constant.cs, Token helpers, EmailHelper
│   └── Program.cs                # DI registration + JWT + CORS + pipeline
│
├── MediRecords.Domain/           # Pure POCOs (no EF dependencies leak)
│   ├── Entities/                 # DbContext + entity classes
│   └── Enums/                    # UserRoleEnums, AppointmentStatus, …
│
└── MediRecordsWeb/               # Angular SPA
    └── src/app/
        ├── core/
        │   ├── guards/           # authGuard, roleGuard
        │   ├── interceptors/     # auth.interceptor.ts (attaches Bearer token)
        │   ├── models/           # TS interfaces matching backend DTOs
        │   └── services/         # 25+ HttpClient services
        ├── features/             # One folder per route page
        │   ├── auth/             # login, update-password
        │   ├── workspace/        # Physician dashboard
        │   ├── patients/         # Patient registry
        │   ├── appointments/
        │   ├── soap-notes/ vitals/ nursing-notes/ …
        │   └── admin/            # Admin-only: users, billing, reports, codes
        └── shared/components/    # SearchableSelectComponent, skeleton, etc.
```

---

## Authentication & authorization flow

The interceptor attaches the JWT to every request, the auth guard blocks unauthenticated routes, and the role guard enforces role-specific routes (e.g. `/admin/*` is Admin-only).

```mermaid
sequenceDiagram
  autonumber
  participant U as User
  participant FE as Angular UI<br/>(login.component)
  participant Auth as AuthService<br/>+ TokenService
  participant API as AuthController<br/>POST /api/v1/Auth/login
  participant SVC as AuthService<br/>(server)
  participant DB as SQL Server

  U->>FE: Enter email + password
  FE->>API: POST /Auth/login (creds)
  API->>SVC: Verify credentials
  SVC->>DB: SELECT user by email
  DB-->>SVC: User row (incl. RoleId)
  SVC->>SVC: BCrypt.Verify(pwd)
  SVC->>SVC: Issue JWT (sub, role, exp)
  SVC-->>API: { token, role, user }
  API-->>FE: 200 OK + JWT
  FE->>Auth: persist token + decode claims
  Auth->>Auth: currentUser signal set
  Auth->>FE: router.navigate(dashboardRouteForRole)

  Note over FE,API: All subsequent calls
  FE->>API: GET /Patient/all<br/>(Authorization: Bearer <jwt>)
  API->>API: [Authorize(Roles="Physician,…")] middleware
  alt Token valid + role allowed
    API-->>FE: 200 + data
  else Token expired
    API-->>FE: 401 Unauthorized
    FE->>Auth: logout() → /login
  else Wrong role
    API-->>FE: 403 Forbidden
  end
```

### Five-role authorization matrix

```mermaid
flowchart TD
  Login{{"Sign in"}} --> R{Role from JWT}

  R -- Admin --> A["/admin<br/>Users · Procedure codes ·<br/>Billing · Reports"]
  R -- Physician --> P["/physician<br/>Workspace (encounters) ·<br/>SOAP · Vitals · Lab/Imaging orders ·<br/>Prescriptions · Care plans · Allergies ·<br/>Problems · Medical history"]
  R -- Nurse --> N["/nurse<br/>Vitals · Nursing notes ·<br/>Care plans · Immunizations"]
  R -- FrontDesk --> F["/frontdesk<br/>Patient registry · Appointments ·<br/>Documents"]
  R -- LabTech --> L["/labtech<br/>Lab orders queue ·<br/>Lab results · Imaging reports"]
```

---

## Role-based routing & landing dashboards

```mermaid
flowchart LR
  subgraph Routes["app.routes.ts"]
    direction TB
    Login["/login (public)"]
    UPwd["/update-password (public)"]
    Shell["AdminLayout<br/>(authGuard)"]
    Shell --> Workspace["/workspace · Physician"]
    Shell --> Patients["/patients · FrontDesk, Physician, Nurse"]
    Shell --> Appts["/appointments · Admin, FrontDesk, Physician, Nurse"]
    Shell --> SOAP["/soap-notes · Physician"]
    Shell --> Vitals["/vitals · Admin, Physician, Nurse"]
    Shell --> Rx["/prescriptions · Admin, Physician, FrontDesk, Nurse"]
    Shell --> Charges["/charges · Admin, Physician"]
    Shell --> AdminPages["/admin/* · Admin only<br/>(roleGuard)"]
  end
```

Routes are declared in [`src/app/app.routes.ts`](MediRecordsWeb/src/app/app.routes.ts). Each lazy-loaded component is wrapped by `authGuard` (rejects no-token) and optionally `roleGuard([…allowed roles])`.

---

## End-to-end patient journey

The platform is structured around this clinical lifecycle:

```mermaid
flowchart LR
  FD["1. FrontDesk<br/>registers patient"] --> APPT["2. FrontDesk<br/>books appointment"]
  APPT --> CHECKIN["3. FrontDesk checks in<br/>→ auto-creates Encounter"]
  CHECKIN --> WS["4. Physician opens<br/>workspace → chart"]
  WS --> CHART["5. Chart actions<br/>SOAP / Vitals / Rx / Orders /<br/>Care plan / Follow-up"]
  CHART --> CLOSE["6. Physician closes<br/>encounter"]
  CLOSE --> BILL["7. Admin/Physician<br/>review charges"]
  BILL --> REPORT["8. Admin sees KPIs,<br/>doc-completeness reports"]
```

---

## Appointment → Encounter flow

When FrontDesk flips an appointment status to `CheckedIn`, the server atomically creates an `Encounter` row in the same `SaveChangesAsync` call. The new encounter id flows back so the Physician (if the appointment's provider) can open the chart immediately.

```mermaid
sequenceDiagram
  autonumber
  participant FD as FrontDesk UI
  participant API as AppointmentsController
  participant SVC as AppointmentsService
  participant DB as SQL Server
  participant Phy as Physician UI

  FD->>API: POST /Appointments (patientId, providerId, dateTime)
  API->>SVC: BookAppointmentAsync
  SVC->>SVC: Validate patient + provider exist<br/>Soft ProviderSchedule check<br/>Slot conflict check (30-min default)
  SVC->>DB: INSERT Appointment (status=Booked)
  DB-->>SVC: appointmentId
  SVC-->>FD: 201 Created

  Note over FD,Phy: Patient arrives at clinic

  FD->>API: PUT /Appointments/{id}/CheckedIn
  API->>SVC: UpdateAppointmentAsync(status=CheckedIn)
  SVC->>SVC: EnsureEncounterForCheckInAsync<br/>(create Encounter row)
  SVC->>DB: SaveChangesAsync (Appointment + Encounter)
  DB-->>SVC: encounterId
  SVC-->>FD: 200 + { appointmentId, encounterId }

  alt Physician is the appointment's provider
    FD->>Phy: redirect /workspace?encounterId=<id>
    Phy->>Phy: auto-open encounter detail drawer
  end
```

---

## Physician workspace & "Chart actions"

The workspace lists the physician's encounters for a chosen day. Clicking a card opens a drawer with status controls **and a 9-button "Chart actions" grid** that deep-links into each encounter-scoped feature with `?encounterId=X&patientName=Y` query params. Target pages lock the encounter (banner instead of dropdown) so the physician can't accidentally chart against a different one.

```mermaid
flowchart TD
  WS["Physician workspace<br/>(today's encounters)"] --> CARD["Click encounter card"]
  CARD --> DRAWER["Detail drawer opens<br/>Status controls + Chart actions"]

  DRAWER -->|"?encounterId=…"| SOAP["SOAP Notes"]
  DRAWER -->|"?encounterId=…"| VIT["Vitals"]
  DRAWER -->|"?encounterId=…"| NN["Nursing Notes"]
  DRAWER -->|"?encounterId=…"| LO["Lab Orders"]
  DRAWER -->|"?encounterId=…"| IO["Imaging Orders"]
  DRAWER -->|"?encounterId=…"| RX["Prescriptions"]
  DRAWER -->|"?encounterId=…"| CH["Charges"]
  DRAWER -->|"?encounterId=…"| FU["Follow-ups"]
  DRAWER -->|"?encounterId=…&patientId=…"| DOC["Documents"]

  classDef target fill:#dbeafe,stroke:#1d4ed8,color:#0b1426
  class SOAP,VIT,NN,LO,IO,RX,CH,FU,DOC target

  SOAP --> LOCK["lockedEncounter banner<br/>+ pre-filled form"]
  VIT --> LOCK
  NN --> LOCK
  LO --> LOCK
  IO --> LOCK
  RX --> LOCK
  CH --> LOCK
  FU --> LOCK
  DOC --> LOCK
```

---

## Prescription create + auto-merge

A subtle but important rule: when a physician creates a new prescription for an encounter that **already has one**, the new items get merged into the existing prescription via the UPDATE endpoint instead of creating a duplicate.

```mermaid
flowchart TD
  Start(["Physician submits<br/>'Create prescription' form"]) --> Validate{"Form valid +<br/>≥1 item?"}
  Validate -- no --> ShowErr["Show errors / toast"]
  Validate -- yes --> Lookup["Look up existing prescriptions<br/>for selected encounterId"]
  Lookup --> Exists{"Match found?"}

  Exists -- no --> Create["POST /PrescriptionWithItems<br/>(create new)"]
  Create --> ToastNew["Toast: 'Prescription created'"]
  ToastNew --> Refresh["Refresh list"]

  Exists -- yes --> Merge["Merge existing items + new items<br/>Keep existing status<br/>(don't un-issue an Issued Rx)"]
  Merge --> Update["PUT /PrescriptionWithItems/{id}<br/>(replace with merged list)"]
  Update --> ToastMerge["Toast: 'Added N item(s)<br/>to prescription #X'"]
  ToastMerge --> Refresh

  Refresh --> End(["List + card show<br/>patient name (not encounter id)"])
```

---

## Care plan create + auto-merge

Same idea on the backend: when a new care plan POST comes in for a patient with an **existing Active** care plan, the service appends the new goals (case-insensitive dedupe) and instructions onto the existing row instead of inserting a duplicate.

```mermaid
flowchart TD
  Start(["POST /CarePlan<br/>{ patientId, goals, instructions, status }"]) --> V1{"Validate<br/>(patient exists, goals present)"}
  V1 -- fail --> Err400["400 Bad Request"]
  V1 -- ok --> Lookup["repo.GetActiveByPatientAsync(patientId)"]
  Lookup --> Has{"Active plan exists?<br/>AND incoming Status = Active?"}

  Has -- no --> Insert["INSERT new CarePlan row"]
  Insert --> Return201A["Return new plan"]

  Has -- yes --> MergeGoals["Append new goals<br/>(skip duplicates, case-insensitive)"]
  MergeGoals --> MergeInstr["Append new instructions<br/>on a new line if not already present"]
  MergeInstr --> UpdateRow["UPDATE existing CarePlan row"]
  UpdateRow --> Return201B["Return merged plan"]
```

---

## Billing & charges flow

Visit charges are attached to encounters by Physicians; Admins review the **Unbilled queue**, mark batches as billed, and export CSV/JSON.

```mermaid
sequenceDiagram
  autonumber
  participant Phy as Physician
  participant Admin as Admin
  participant API as BillingController
  participant SVC as BillingService

  Phy->>API: POST /Billing/visit-charges<br/>(encounterId, codeId, amount)
  API->>SVC: AddCharge
  SVC-->>Phy: 201 Created

  Admin->>API: GET /Billing/unbilled-encounters<br/>?fromDate=…&toDate=…&providerId=…&page=…
  API->>SVC: GetUnbilledEncounters (paged)
  SVC-->>Admin: PagedResponse&lt;UnbilledEncounter&gt;

  Admin->>API: PUT /Billing/mark-billed (chargeIds[])
  API->>SVC: MarkBilled
  SVC-->>Admin: { processedCount }

  Admin->>API: GET /Billing/export?format=csv
  API-->>Admin: CSV file download
```

---

## Backend request lifecycle

Every API call traverses the same pipeline. Cross-cutting concerns (auth, validation, error translation) live in the middleware/controller layer; pure business rules live in the service layer; persistence lives in the repository layer.

```mermaid
flowchart LR
  Req(["HTTP Request<br/>+ Bearer JWT"]) --> Cors["CORS<br/>(AllowFrontend)"]
  Cors --> Auth["UseAuthentication<br/>(decode JWT)"]
  Auth --> Authz["UseAuthorization<br/>[Authorize(Roles=…)]"]
  Authz --> Ctrl["Controller action"]
  Ctrl --> Model["ModelState validation<br/>(DTO attributes)"]
  Model --> Svc["Service method<br/>(business rules + custom validation)"]
  Svc --> Repo["Repository<br/>(EF Core)"]
  Repo --> Db[("SQL Server")]
  Db --> Repo
  Repo --> Svc
  Svc --> Map["AutoMapper<br/>entity → response DTO"]
  Map --> Ctrl
  Ctrl --> Resp(["HTTP Response<br/>200 / 400 / 401 / 403 / 404 / 409 / 500"])

  Svc -.throws.-> ErrCatch["try/catch in controller"]
  ErrCatch -.MediRecordsException.-> Resp
  ErrCatch -.ArgumentException.-> Resp
  ErrCatch -.InvalidOperationException.-> Resp
```

---

## Domain model (key entities)

```mermaid
erDiagram
  User ||--o{ Patient            : "PrimaryProvider"
  User ||--o{ Encounter          : "ProviderId"
  User ||--o{ Appointment        : "ProviderId"
  User ||--o{ ProviderSchedule   : "ProviderId"
  UserRole ||--o{ User           : "RoleId"

  Patient ||--o{ Appointment     : "PatientId"
  Patient ||--o{ Encounter       : "PatientId"
  Patient ||--o{ Allergy         : "PatientId"
  Patient ||--o{ ProblemList     : "PatientId"
  Patient ||--o{ MedicalHistory  : "PatientId"
  Patient ||--o{ CarePlan        : "PatientId"
  Patient ||--o{ Immunization    : "PatientId"
  Patient ||--o{ MedicationList  : "PatientId"
  Patient ||--o{ Document        : "PatientId"

  Appointment ||--o| Encounter   : "auto-created on CheckIn"

  Encounter ||--o{ SOAPNote      : "EncounterId"
  Encounter ||--o{ VitalSign     : "EncounterId"
  Encounter ||--o{ NursingNote   : "EncounterId"
  Encounter ||--o{ LabOrder      : "EncounterId"
  Encounter ||--o{ ImagingOrder  : "EncounterId"
  Encounter ||--o{ Prescription  : "EncounterId"
  Encounter ||--o{ VisitChargeRef : "EncounterId"
  Encounter ||--o{ FollowUp      : "EncounterId"
  Encounter ||--o{ Document      : "EncounterId"

  Prescription ||--o{ PrescriptionItem : "PrescriptionId"
  LabOrder ||--o{ LabResult           : "LabOrderId"
  ImagingOrder ||--o{ ImagingReport   : "ImagingOrderId"

  VisitChargeRef }o--|| ProcedureCode : "CodeId"
```

### Status enums at a glance

| Entity        | Enum                                            |
| ------------- | ----------------------------------------------- |
| Appointment   | `Booked, CheckedIn, Completed, Cancelled, NoShow` |
| Encounter     | `Open(1), Closed(2), Locked(3)`                 |
| Patient       | `Inactive(0), Active(1), Deceased(2)`           |
| Prescription  | `Draft, Issued`                                 |
| CarePlan      | `Active(false), Completed(true)`                |
| User          | `Active(true) / Inactive(false)`                |

---

## API surface summary

All endpoints are under `/api/v1/{Controller}`. Authorization is enforced per-action.

| Controller                  | Notable endpoints                                                                                | Roles                                |
| --------------------------- | ------------------------------------------------------------------------------------------------ | ------------------------------------ |
| **Auth**                    | `POST /login`, `POST /User/forgotpassword`                                                       | Public                               |
| **User**                    | `GET /GetAll`, `GET /providers`, `GET /roles`, `POST /register`, `PUT /update`, `POST /delete/{id}` | Admin / mixed                        |
| **Patient**                 | `GET /all`, `GET /{id}`, `POST /`, `PUT /{id}`                                                   | FrontDesk + clinical roles           |
| **Encounter**               | `GET /workspace?date=`, `GET /all`, `GET /{id}`, `PATCH /{id}/status`                            | Physician, Nurse, FrontDesk, Admin   |
| **Appointments**            | `GET /`, `POST /`, `PUT /{id}/{status}`                                                          | FrontDesk to book; all to read       |
| **SOAPNote**                | `POST /soap`                                                                                     | Physician                            |
| **VitalSign**               | `POST /capture`                                                                                  | Authenticated                        |
| **NursingNote**             | `POST /`, `PUT /{id}`                                                                            | Authenticated                        |
| **LabOrder / LabResult**    | `GET`, `POST`, status updates                                                                    | Physician + LabTech                  |
| **ImagingOrder / Report**   | `GET`, `POST`                                                                                    | Physician (orders), LabTech (reports) |
| **PrescriptionWithItems**   | `GET`, `POST`, `PUT /{id}`, `DELETE /{id}`                                                       | Physician (full), Admin (delete)     |
| **CarePlan**                | `GET`, `POST` *(auto-merges into Active)*                                                        | Physician (create), Nurse (read)     |
| **Billing**                 | `GET /unbilled-encounters`, `POST /visit-charges`, `PUT /visit-charges/{id}`, `PUT /mark-billed`, `GET /export` | Physician + Admin |
| **Reports**                 | `GET /clinic-kpis`, `GET /documentation-completeness`, `GET /no-show-cancellation`, `GET /provider-utilization` | Admin |
| **Document**                | `POST` (upload), `GET` (search/download), `PUT`, `DELETE`                                        | Admin, Physician, Nurse, FrontDesk   |
| **Allergy / Problem / MedicalHistory / Immunization** | per-patient CRUD                                                          | Physician (+ Nurse for some)         |
| **FollowUp**                | `GET`, `POST`                                                                                    | Physician                            |
| **ProcedureCode**           | `GET`, `POST`, `PUT`                                                                             | Admin                                |
| **ProviderProductivity**    | reporting endpoints                                                                              | Admin                                |

### Reusable lookup endpoints (used to power frontend dropdowns)

| Endpoint                       | Returns                                          | Roles                                |
| ------------------------------ | ------------------------------------------------ | ------------------------------------ |
| `GET /Patient/all`             | `PatientLookupDto[]` (id, name, MRN, DOB, …)     | FrontDesk, Physician, Nurse, Admin   |
| `GET /Encounter/all`           | `EncounterLookupDto[]` (id, patient, date, …)    | Physician, Nurse, FrontDesk, Admin   |
| `GET /User/providers`          | `ProviderLookupDto[]` (id, name) — Physicians only | Physician, FrontDesk, Admin       |

The frontend wraps these in the **`<app-searchable-select>`** component for type-ahead filtering across all 26 dropdown locations.

---

## Running locally

### Prerequisites

- .NET 10 SDK
- Node.js 20+ and npm
- SQL Server (LocalDB or full instance)

### Backend

```bash
cd MediRecords
# 1. Update appsettings.json → ConnectionStrings:DefaultConnection
# 2. Apply migrations
dotnet ef database update
# 3. Run the API (defaults to https://localhost:5157)
dotnet run
```

Swagger UI is available at `https://localhost:5157/swagger` in Development.

### Frontend

```bash
cd MediRecordsWeb
npm install
npm start          # → http://localhost:4200
```

The Angular `environments/environment.ts` points `apiBaseUrl` at the backend. CORS in [`Program.cs`](MediRecords/Program.cs) is preconfigured to allow `http://localhost:4200`.

### First login

After running migrations + seeding (or manually inserting roles `Admin / Physician / Nurse / LabTech / FrontDesk` into `UserRole` and at least one `User` row with a BCrypt-hashed password), navigate to `/login` and sign in. The frontend's role guard will redirect you to the appropriate dashboard.

---

## Notes on diagrams

All flowcharts in this README use **Mermaid**, which is rendered natively by:

- GitHub (in the README preview)
- GitLab
- VS Code (with the Markdown Preview Mermaid extension or built-in support)
- Most modern static site generators (Docusaurus, MkDocs, Jekyll with plugins)

To export any diagram as PNG/SVG: open the README in VS Code preview, right-click the rendered diagram, **or** paste the diagram source into [mermaid.live](https://mermaid.live) and download from there.
