# Week 04 - HTTP, REST, and ASP.NET Core Web API Prep

## 1. HTTP and REST Notes

### HTTP Methods
- **GET**: Retrieves a representation of a resource without modifying server state.
- **POST**: Submits data to create a new subordinate resource or trigger processing.
- **PUT**: Replaces the entire target resource with the requested payload, or creates it if supported.
- **PATCH**: Applies partial modifications to an existing resource.
- **DELETE**: Removes the specified target resource.

**Definitions:**
- **Safe**: The method is read-only and guarantees no side effects or mutations on the server state (GET).
- **Idempotent**: Making multiple identical requests results in the exact same server state as making a single request (GET, PUT, DELETE).

| Method | Safe | Idempotent |
| :--- | :---: | :---: |
| GET | Yes | Yes |
| POST | No | No |
| PUT | No | Yes |
| PATCH | No | No (can be, but not guaranteed by spec) |
| DELETE | No | Yes |

---

### HTTP Status Codes (Compliance Tracker Examples)
- **200 OK**: Successfully retrieved client compliance profile (`GET /api/clients/101`).
- **201 Created**: Successfully registered a new audit log entry (`POST /api/audits`).
- **204 No Content**: Successfully deleted an outdated compliance rule with no response body needed (`DELETE /api/rules/5`).
- **400 Bad Request**: Malformed JSON or missing mandatory client tax ID in payload.
- **401 Unauthorized**: Request lacks valid JWT bearer token or authentication header.
- **403 Forbidden**: Authenticated auditor lacks administrator privileges to approve policy overrides.
- **404 Not Found**: Target entity does not exist (`GET /api/clients/999`).
- **405 Method Not Allowed**: Attempting `POST` on an endpoint configured exclusively for `GET`.
- **409 Conflict**: Attempting to register an entity with an existing duplicate commercial registry number.
- **415 Unsupported Media Type**: Sending XML payload when the endpoint strictly consumes `application/json`.
- **500 Internal Server Error**: Unhandled database timeout or unexpected server-side exception.

---

### REST Resource Naming Rules
1. **Rule 1: Use plural nouns instead of verbs.**
   - *Good*: `/api/clients`
   - *Bad*: `/api/getAllClients`
2. **Rule 2: Represent relationships hierarchically through sub-resources.**
   - *Good*: `/api/clients/101/audits`
   - *Bad*: `/api/getClientAudits?clientId=101`
3. **Rule 3: Use lowercase URI paths with hyphens (kebab-case).**
   - *Good*: `/api/compliance-policies`
   - *Bad*: `/api/Compliance_Policies`

---

## 2. Fake API Analysis (`jsonplaceholder.typicode.com`)

| Request | Status Code | Expected / Preferred | Analysis |
| :--- | :---: | :---: | :--- |
| `GET /posts/1` | 200 OK | 200 OK | Standard successful fetch. |
| `GET /posts/99999` | 404 Not Found | 404 Not Found | Accurately indicates non-existing record. |
| `GET /posts?userId=1` | 200 OK | 200 OK | Returns array of matching resources. |
| `POST /posts` | 201 Created | 201 Created | Standard successful resource creation. |
| `PUT /posts/1` | 200 OK | 200 OK | Successfully updated full representation. |
| `PATCH /posts/1` | 200 OK | 200 OK | Successfully applied delta update. |
| `DELETE /posts/1` | 200 OK | 204 No Content | **Worth arguing about**: JsonPlaceholder returns `200 OK` with an empty object `{}`. In a production REST API, `204 No Content` is the preferred standard because no representation body needs to travel back over the wire. |

---

## 3. ASP.NET Core Web API Template Analysis

- **`var builder = WebApplication.CreateBuilder(args);`**: Initializes the host, loads configuration (`appsettings.json`, environment variables), and prepares the Dependency Injection (DI) container.
- **`builder.Services.AddControllers();`**: Registers MVC controller services into the DI container without adding view engines or Razor Pages.
- **`builder.Services.AddEndpointsApiExplorer();` & `builder.Services.AddSwaggerGen();`**: Registers API endpoint metadata extraction and Swagger generator services.
- **`var app = builder.Build();`**: Compiles the service collection into the active request pipeline application.
- **`if (app.Environment.IsDevelopment()) { app.UseSwagger(); app.UseSwaggerUI(); }`**: Exposes OpenAPI documentation only during local development. In production, Swagger is disabled to prevent leaking internal endpoint schemas and data contracts.
- **`app.UseHttpsRedirection();`**: Enforces secure communication by redirecting HTTP traffic to HTTPS.
- **`app.UseAuthorization();`**: Adds the authorization middleware to the pipeline to validate endpoint access rules.
- **`app.MapControllers();`**: Inspects route attributes (`[Route]`, `[HttpGet]`, etc.) on controller classes and maps incoming URLs to controller actions.
- **`app.Run();`**: Starts Kestrel server and begins listening for incoming requests.

**Key Technical Questions:**
- **Why does Swagger only appear in some environments?** Security best practice. Exposing internal API schemas publicly in Production aids attackers in mapping attack vectors.
- **What does `[ApiController]` change?** It enables automatic 400 Bad Request responses on model validation failures, requires attribute routing, and infers parameter binding sources (`[FromBody]`, `[FromQuery]`) automatically.
- **Where does the port come from?** From `Properties/launchSettings.json` under `applicationUrl` (e.g., `https://localhost:7xxx;http://localhost:5xxx`), or overridden via `ASPNETCORE_URLS`.

---

## 4. Git Branching Practice

1. **Fast-Forward vs Merge Commit**:
   - Created feature branch: `git switch -c feature/notes`.
   - Committed changes and switched back: `git switch main`.
   - Merged: `git merge feature/notes`.
   - **Result**: Fast-forward merge occurred because `main` had no new commits while the feature branch diverged; Git simply advanced the `main` pointer forward.

2. **Intentional Merge Conflict Resolution**:
   - Edited line 1 of a test file on `main` and committed.
   - Switched to feature branch, edited the exact same line with differing text, and committed.
   - Ran `git merge main` from the feature branch.
   - Git flagged a `CONFLICT (content): Merge conflict in test.txt`.
   - Resolution steps:
     1. Opened the conflicted file showing standard conflict markers (`<<<<<<<`, `=======`, `>>>>>>>`).
     2. Selected the appropriate combined content manually and removed the conflict markers.
     3. Staged the resolved file: `git add test.txt`.
     4. Finalized the merge commit: `git commit -m "chore: resolve merge conflict between main and feature branch"`.

---

## 5. Self-Check Answers

1. **If I call DELETE twice on the same client, is it still idempotent if the second call returns 404?**
   - **Yes, it is still idempotent.** Idempotency measures the resulting state on the server, not the HTTP response code. After both calls, the client remains non-existent.
2. **What's the difference between 401 and 403?**
   - `401 Unauthorized` means authentication failure (identity unknown / missing token).
   - `403 Forbidden` means authorization failure (identity is verified, but lacks sufficient permissions).
3. **Why is a GET that changes data dangerous?**
   - Browsers, search engine crawlers, and proxies assume GET is safe and idempotent. They perform aggressive caching and link pre-fetching, which could trigger unintended state changes or deletions.
4. **What status and header should a successful create return?**
   - Status code: `201 Created`.
   - Header: `Location: /api/clients/{id}` pointing to the URL of the newly created entity.
5. **What happens if you run `git merge main` while you're on a feature branch?**
   - It brings all the latest commits from `main` into your current `feature branch`, creating a merge commit if histories diverged. It does not touch `main`.
6. **One question I couldn't answer:**
   - In ASP.NET Core 8, when designing high-throughput APIs, what are the exact performance trade-offs between using Minimal APIs versus standard Controllers with `[ApiController]`?

---

## Notes & Assumptions
- Assumed standard RESTful conventions throughout.
- Mocked examples based on our Fusion Compliance Tracker domain model.