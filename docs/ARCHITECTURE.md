# ARCHITECTURE.md

## Overview

This document describes the proposed architecture for the **QuoteHunt** app. The system consists of three main services running in Docker Compose:

* **scraper** — internal scraping service responsible for logging into `quotes.toscrape.com` and extracting quotes.
* **api** — public .NET Web API that serves JSON to the frontend. **It does not connect directly to `quotes.toscrape.com`.**
* **frontend** — Angular application that consumes the API and renders the quotes list.

The key security/requirement constraint: **the API must never perform direct requests to `quotes.toscrape.com`**. Only the `scraper` service is allowed to access that domain.

---

## ASCII Diagram (Components & Data Flow)

```
+-----------+          +-----------+          +---------------------+
| Frontend  | <--HTTP--|   API     | <--HTTP--|     Scraper         |
| (Angular) |          | (.NET)    |          | (Python/FastAPI)    |
+-----------+          +-----------+          +---------------------+
      |                     |                        |
      |                     |                        |
      |-- user requests --->|                        |
      |                     |-- internal request --->|--(login+scrape)--> https://quotes.toscrape.com
      |                     |                        |                       (external)
      |<-- rendered data ---|<-- JSON (QuoteDto) ----|
```

Notes:

* All inter-service traffic occurs over the Docker Compose network (internal `appnet`).
* The `scraper` is the only service that initiates outbound traffic to `quotes.toscrape.com`.
* The `scraper` container should *not* publish its port to the host; it should only be `expose`d in compose so the API can reach it internally.

---

## HTTP Contracts (JSON DTOs)

### QuoteDto (response item)

```json
{
  "text": "The world as we have created it is a process of our thinking. It cannot be changed without changing our thinking.",
  "author": "Albert Einstein",
  "tags": ["change", "deep-thoughts", "thinking", "world"]
}
```

### API - `GET /api/quotes?page=1`

**Response:** `200 OK`

```json
[
  {
    "text": "...",
    "author": "...",
    "tags": ["tag1", "tag2"]
  },
  {
    "text": "...",
    "author": "...",
    "tags": ["tagA"]
  }
]
```

### Scraper internal endpoint - `GET /internal/quotes?page=1`

**Response:** `200 OK` (same payload shape as `QuoteDto[]`)

---

## Environment variables

Place these in `.env` or inject via Docker Compose environment section. Provide a `.env.example` in repo.

* `SCRAPER_URL` — Base address for the scraper service used by API (e.g. `http://scraper:8000`).
* `SCRAPER_USER` — Optional username to log into `quotes.toscrape.com`.
* `SCRAPER_PASS` — Optional password to log into `quotes.toscrape.com`.
* `CACHE_TTL_SECONDS` — Cache TTL for in-memory cache in the API (default: `60`).
* `USE_REDIS` — `true|false` to enable Redis cache integration (optional).
* `REDIS_URL` — Redis connection string (if `USE_REDIS=true`).
* `ASPNETCORE_ENVIRONMENT` — `Development|Production` for .NET API.
* `FRONTEND_API_URL` — URL the frontend uses to reach the API (e.g., `/api` in dev with proxy, or `http://localhost:5000/api`).

**Important:** Do **not** place `quotes.toscrape.com` or its credentials into the API code or configuration in a way that allows the API to call the site directly; the API's `SCRAPER_URL` must point to the internal scraper container.

---

## Docker networking and compose notes

* Use a single Docker network (e.g., `appnet`) created by Compose.
* `scraper` service:

  * `expose: - "8000"` (reachable by other containers on `appnet`) but **do not** publish a host port.
  * Container name: `scraper` (so API can use `http://scraper:8000`).
* `api` service:

  * publish a host port (e.g., `5000:80`) so the frontend (or host) can access it.
  * set `SCRAPER_URL` env to `http://scraper:8000` in Compose.
* `frontend` service:

  * build production assets and serve via Nginx on port `4200` (or use `ng serve` in development).
* Example `docker-compose.yml` must include `depends_on` to control startup order but still implement health checks and retry logic.

---

## How the "no direct connection from API to quotes.toscrape.com" requirement is met

1. **Network design:** The API's `HttpClient` base URL (`SCRAPER_URL`) points to the internal service hostname `http://scraper:8000`. No code path in the API contains `quotes.toscrape.com`.
2. **Container exposure:** The `scraper` container is *exposed internally only* (no host binding). The only service able to reach `quotes.toscrape.com` is the `scraper` container itself (it performs external outbound requests).
3. **Code inspection / CI check:** Add a CI lint step (`grep -R "quotes.toscrape.com" src/`) that fails the build if the API project contains direct references to the target domain.
4. **Optional egress control:** If stricter enforcement is required, block outbound access to `quotes.toscrape.com` in the API container using network policies or iptables as part of the container startup.

Together these measures ensure the API itself does not perform direct requests to the target domain; it only calls the internal scraper.

---

## Caching strategy (In-memory vs Redis)

### In-memory cache (default)

* **Pros:** Simple, zero external dependencies, quick to implement (IMemoryCache in .NET).
* **Cons:** Cache is per API instance — not shared across multiple API replicas.
* **Use case:** Recommended for the challenge and local deployments.

### Redis (optional)

* **Pros:** Shared cache across instances, persistence options, TTL management.
* **Cons:** Additional service to run and maintain (adds complexity to Docker Compose and CI).
* **Use case:** If you plan to scale the API horizontally or want to centralize the cache, enable Redis behind `USE_REDIS=true` and configure `REDIS_URL`.

**Recommendation for the challenge:** Implement in-memory caching with an abstraction (ICache) so replacing the implementation with Redis later is straightforward. Default TTL: `CACHE_TTL_SECONDS` (60s). Cache key example: `quotes:page:1`.

---

## Testing strategy

### Unit tests

* **API:** Unit test controllers and services with mocks for `IScraperClient`. Validate:

  * Successful return of `QuoteDto[]`.
  * Error handling when scraper returns non-200.
  * Cache behavior (first call hits scraper, second call returns cached value).
* **Scraper:** Unit-test parsing functions using saved HTML fixtures from `quotes.toscrape.com` pages to verify robust selectors.

### Integration / E2E tests

* **Local compose smoke test:** `ci/e2e.sh` script performs:

  1. `docker-compose up --build -d`
  2. wait for health checks
  3. `curl http://localhost:5000/api/quotes` and assert JSON contains at least 1 quote
  4. `curl http://localhost:4200` assert 200
* **Contract tests:** Verify that the API's response schema matches expectations (e.g., using JSON schema validation).

### CI checks

* Run `dotnet test` to validate backend unit tests.
* Run linter scripts to ensure no references to `quotes.toscrape.com` in API code.
* Optional: run the compose-based smoke test on a runner with Docker available.

---

## Error handling and resilience

* **Scraper timeouts:** Use reasonable timeouts and retries for the login/scrape flow.
* **API behavior on scraper failure:** Return `502 Bad Gateway` when the scraper fails; include helpful error messages for the frontend.
* **Rate limiting / backoff:** Implement exponential backoff and throttle scraping frequency; rely on caching to reduce calls.

---

## Example flow (request/response)

1. Frontend requests `/api/quotes?page=1`.
2. API checks cache for `quotes:page:1`.

   * If cached: return cached JSON.
   * If not cached: API calls internal scraper `GET http://scraper:8000/internal/quotes?page=1`.
3. Scraper logs into `https://quotes.toscrape.com/login`, requests `/page/1`, parses HTML, returns `200` with JSON array.
4. API stores result in cache and forwards JSON to frontend.

**Example API response:**

```json
[
  {
    "text": "The world as we have created it is a process of our thinking. It cannot be changed without changing our thinking.",
    "author": "Albert Einstein",
    "tags": ["change","deep-thoughts","thinking","world"]
  }
]
```

---

## Deployment and running (developer notes)

* Provide `docker-compose.yml` at repository root to spin up `scraper`, `api`, and `frontend`.
* Provide `.env.example` for required environment variables.
* Ensure `scraper` is not port-published in Compose (use `expose`) so it remains internal-only.
* Use healthchecks and `depends_on` to ensure services become healthy before accepting traffic.

---

## Appendix

* Suggested file layout:

```
/README.md
/docs/ARCHITECTURE.md
/scraper/Dockerfile
/scraper/app.py
/api/Dockerfile
/api/src/... (dotnet project)
/frontend/Dockerfile
/docker-compose.yml
.env.example
```

* Minimal CI checks example:

  * `grep -R "quotes.toscrape.com" api/src/ && exit 1`  (fail on match)
  * `dotnet test`
  * `./ci/e2e.sh` (optional)
