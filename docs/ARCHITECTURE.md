# ARCHITECTURE.md

## Overview

This document describes the proposed architecture for the **Quotes Mini-App**.
The system consists of two main services running in Docker Compose:

* **api** — .NET public API that logs into `quotes.toscrape.com`, scrapes the quotes, and returns them as JSON to the frontend.
* **frontend** — Angular application that consumes the API and displays the list of quotes.

The API is responsible for all scraping and communication with `quotes.toscrape.com`, ensuring the frontend never connects directly to the site.

---

## ASCII Diagram (Components & Data Flow)

```
+-----------+          +-----------+
| Frontend  | <--HTTP--|   API     |
| (Angular) |          | (.NET)    |
+-----------+          +-----------+
      |                     |
      |-- user requests --->|
      |                     |--(login+scrape)--> https://quotes.toscrape.com
      |<-- rendered data ---|
```

Notes:

* All login and scraping logic is handled inside the API.
* The frontend only consumes processed data from the API.
* The API should implement caching to avoid excessive scraping.

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

---

## Environment variables

Example configuration in `.env` or the `environment` section of Docker Compose:

* `SCRAPER_USER` — Username for logging into `quotes.toscrape.com`.
* `SCRAPER_PASS` — Password for logging in.
* `CACHE_TTL_SECONDS` — Cache lifetime in seconds (default: `60`).
* `ASPNETCORE_ENVIRONMENT` — `Development|Production`.
* `FRONTEND_API_URL` — URL the frontend uses to reach the API (e.g., `/api` in dev or `http://localhost:5000/api`).

---

## Docker networking and compose

* Use a single Docker network (e.g., `appnet`) created by Compose.
* **api**:

  * Publish a port (e.g., `5000:80`) so the frontend can access it.
  * Environment variables are defined in `.env`.
* **frontend**:

  * Build production assets and serve via Nginx on port `4200`, or use `ng serve` in development.

---

## Caching strategy

### In-memory cache (recommended)

* **Pros:** Simple, no external dependencies, quick to implement (IMemoryCache in .NET).
* **Cons:** Cache is per API instance — not shared across multiple replicas.
* **Use case:** Ideal for this challenge.

Recommended cache key: `quotes:page:1`.

---

## Testing

* **Unit tests (API):**

  * Test HTML parsing logic.
  * Validate correct return of `QuoteDto[]`.
  * Test error handling when the website is unreachable.
  * Verify cache behavior.
* **Integration tests:**

  * Run `docker-compose up` and request `/api/quotes` to ensure it returns data.
  * Check that the frontend receives and renders the data correctly.

---

## Error handling and resilience

* **Timeouts:** Configure timeouts for requests to `quotes.toscrape.com`.
* **Scraping errors:** Return `502 Bad Gateway` with a clear error message.
* **Rate limiting:** Implement backoff and rely on caching to avoid overloading the site.

---

## Example flow

1. The user opens the frontend.
2. The frontend requests `/api/quotes?page=1`.
3. The API checks the cache:

   * If present, returns cached data.
   * If not, logs into `quotes.toscrape.com`, fetches HTML, parses quotes, stores in cache.
4. The frontend receives the JSON and renders it.

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

## Deployment and running

1. Clone the repository.
2. Configure `.env` based on `.env.example`.
3. Run:

```bash
docker-compose up --build
```

4. Access:

   * Frontend: `http://localhost:4200`
   * API: `http://localhost:5000/api/quotes`

---
