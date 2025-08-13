# QuoteHunt Project - Technical Documentation (Backend)

## 1. Introduction

This project (backend) consists of two main services:

* **QuoteHuntScraper**: A .NET service responsible for scraping quotes from the source website.
* **QuoteHuntWebAPI**: An ASP.NET Core Web API that exposes endpoints to query quotes, leveraging the Scraper service and caching results using Redis.

The goal is to provide a performant API for quotes retrieval with caching to minimize scraping load.

---

## 2. Architecture Overview

```plaintext
+----------------+     HTTP     +------------------+    HTTP    +----------------+
| QuoteHunt API  | <----------> | QuoteHuntScraper | <--------> | Quotes Source  |
+----------------+              +------------------+           +----------------+
         |
         | (cache Redis)
         v
     +----------+
     |  Redis   |
     +----------+
```

* The WebAPI calls ScraperClient via HTTP.
* Redis caches the results with configurable TTL (default 5 minutes).
* Cache check is performed before scraping.

---

## 3. Service Descriptions

### QuoteHuntScraper

* Written in C# (.NET 8).
* Exposes endpoints that perform scraping of quotes, paginated and filterable by tag.
* Uses HttpClient internally.
* Configurable scraper base URL.

### QuoteHuntWebAPI

* Written in ASP.NET Core.
* Contains `QuoteService` that integrates:

  * `ScraperClient` for getting quotes.
  * Redis cache with TTL.
* Controller exposes endpoints like `GET /api/Quote?page={page}&tag={tag}`.
* Configuration options for enabling/disabling Redis caching.

---

## 4. Technical Details

### 4.1 Technologies

* .NET 8
* ASP.NET Core Web API
* StackExchange.Redis client library
* Docker and Docker Compose for containerization

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

## 5. Configuration

### appsettings.json example

```json
{
  "ScraperSettings": {
    "ScraperUrl": "http://quotehunt.webscraper:5033"
  },
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

## 6. Local Development Setup

### Prerequisites

- .NET SDK 8
- Docker and Docker Compose

### Running the Services with Docker Compose

To run the entire stack (Redis, WebScraper, and WebAPI) locally using Docker Compose, execute the following command in the root folder containing the `docker-compose.yml`:

```bash
docker-compose up --build redis webscraper webapi
````

This will start:

* **Redis** on an internal network (not exposed to host).
* **QuoteHuntScraper** on an internal network (not exposed to host).
* **QuoteHuntWebAPI** service connected to Redis and exposed on port `5208` on your localhost.

### Accessing the API

Once running, access the API at:

```
http://localhost:5208/swagger
```

### Running Services Individually (Optional)

If you prefer to run services individually for development:

* **Run Redis**

```bash
docker-compose up --build redis
```

* **Run QuoteHuntScraper**

```bash
docker-compose up --build webscraper
```

* **Run QuoteHuntWebAPI**

```bash
docker-compose up --build webapi
```

Ensure the scraper connects to Redis at `quotehunt.redis:6379`.

Make sure `appsettings.json` or environment variables include:

```json
{
  "Redis": {
    "Host": "quotehunt.redis",
    "Port": 6379
  },
  "ScraperSettings": {
    "ScraperUrl": "http://quotehunt.webscraper:5033"
  }
}
```

## 7. Deployment and Docker

For deployment, use the provided `docker-compose.yml` that defines:

* Redis service (internal, with optional volume for persistence)
* QuoteHuntScraper service
* QuoteHuntWebAPI service

Example `docker-compose.yml` snippet:

```yaml
version: '3.8'

services:
  redis:
    image: redis:latest
    container_name: quotehunt.redis
    networks:
      - internal_network

  webscraper:
    build:
      context: ./QuoteHuntScraper
    container_name: quotehunt.webscraper
    networks:
      - internal_network
    depends_on:
      - redis

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

networks:
  internal_network:
    driver: bridge
```

Make sure to configure environment variables inside your containers or via Docker Compose as needed to point to Redis and scraper URLs.

---

## 8. Security Considerations

* Validate all inputs (`page`, `tag`) to prevent injection attacks.
* If exposing Redis externally, secure it with authentication.
* Use HTTPS and proper certificates in production.
* Restrict network access to internal services where possible.
