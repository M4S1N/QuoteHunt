# QuoteHunt Project - Technical Documentation

## 1. Introduction

This project consists of two main services:

* **QuoteHuntWebSite (Frontend)**: Angular web application for users to search and view quotes.
* **QuoteHuntWebAPI**: ASP.NET Core Web API that exposes endpoints to query quotes. It includes:
  * **ScraperService**: runs in the background, scrapes quotes from the source website, and caches them in Redis.
  * Redis caching to minimize scraping load and improve API performance.

The goal is to provide a performant API and user interface for quotes retrieval, with automated background scraping.

---

## 2. Architecture Overview

```plaintext
+-------------------+     HTTP     +-------------'---+     Background/Redis   +------------------+
|   Frontend (UI)   | <----------> | QuoteHunt API   | <--------------------> | Quotes Source    |
| (Angular Website) |              | (.NET + Scraper)|                        | (External)       |
+-------------------+              +---------------'-+                        +------------------+
                                            |
                                            v
                                      +----------+
                                      |  Redis   |
                                      +----------+
````

* Frontend communicates with the WebAPI via HTTP.
* ScraperService runs in the background inside WebAPI, fetching quotes periodically.
* Redis caches the quotes with a configurable TTL (default 5 minutes).
* API queries Redis first before attempting a scrape.

---

## 3. Service Descriptions

### QuoteHuntWebSite (Frontend)

* Written in Angular.
* Provides a UI for searching, filtering, and viewing quotes.
* Communicates with the WebAPI via REST endpoints.
* Dockerized for deployment.
* Configuration via environment files (`src/environments/`).

### QuoteHuntWebAPI

* Written in ASP.NET Core.
* Integrates the **ScraperService**, which:

  * Runs in the background.
  * Scrapes quotes paginated and optionally filtered by tag.
  * Stores results in Redis.
* `QuoteService` provides API endpoints to retrieve quotes, checking Redis cache first.
* Controller exposes endpoints like `GET /api/Quote?page={page}&tag={tag}`.
* Configurable Redis caching and scraping interval.

---

## 4. Technical Details

### 4.1 Technologies

* Angular (Frontend)
* .NET 8 (Backend)
* ASP.NET Core Web API
* StackExchange.Redis client library
* Docker and Docker Compose for containerization
* Selenium (headless) for scraping

### 4.2 API Endpoints

| Method | Route       | Description                                              | Query Parameters             |
| ------ | ----------- | -------------------------------------------------------- | ---------------------------- |
| GET    | /api/Quotes | Retrieve quotes optionally filtered by tag and paginated | `page` (int), `tag` (string) |

### 4.3 Data Transfer Objects (DTOs)

* `QuoteDTO`:

```csharp
public class QuoteDTO
{
    public string Text { get; set; }
    public string Author { get; set; }
    public List<string> Tags { get; set; }
}
```

---

## 5. Configuration

### appsettings.json example (Backend)

```json
{
  "Redis": {
    "Host": "quotehunt.redis",
    "Port": 6379,
    "UseRedis": true,
    "CacheTTLSeconds": 300
  }
}
```

* `UseRedis` enables/disables Redis caching.
* `CacheTTLSeconds` defines cache expiration time in seconds.

### environment.ts example (Frontend)

```typescript
export const environment = {
  production: false,
  apiUrl: 'http://localhost:5208/api'
};
```

---

## 6. Local Development Setup

### Prerequisites

* .NET SDK 8
* Node.js & Angular CLI
* Docker and Docker Compose

### Running the Services with Docker Compose

To run the entire stack (Redis, WebAPI with Scraper, and Frontend) locally:

```bash
docker-compose up --build
```

This will start:

* **Redis** exposed on port `6379`.
* **QuoteHuntWebAPI** with integrated background scraper, connected to Redis, exposed on port `5208`.
* **QuoteHuntWebSite** (Angular) exposed on port `4200`.

### Accessing the Application

* **Frontend:** [http://localhost:4200](http://localhost:4200)
* **API (Swagger):** [http://localhost:5208/swagger](http://localhost:5208/swagger)

### Running Services Individually

* **Run Redis**:

```bash
docker-compose up --build redis
```

* **Run WebAPI with Scraper**:

```bash
docker-compose up --build webapi
```

* **Run Frontend**:

```bash
docker-compose up --build website
```

Ensure `appsettings.json` or environment variables point to Redis at `quotehunt.redis:6379`.

---

## 7. Deployment and Docker

Example `docker-compose.yml` snippet:

```yaml
version: '3.8'

services:
  redis:
    image: redis:latest
    container_name: quotehunt.redis
    networks:
      - internal_network

  webapi:
    build:
      context: ./QuoteHuntWebAPI
    container_name: quotehunt.webapi
    ports:
      - "5208:5208"
    networks:
      - internal_network
    depends_on:
      - redis

  website:
    build: ./QuoteHuntWebSite
    container_name: quotehunt.website
    ports:
      - "4200:80"
    depends_on:
      - webapi
    networks:
      - internal_network

networks:
  internal_network:
    driver: bridge
```

---

## 8. Security Considerations

* Validate all inputs (`page`, `tag`) to prevent injection attacks.
* If exposing Redis externally, secure it with authentication.
* Use HTTPS and proper certificates in production.
* Restrict network access to internal services where possible.
