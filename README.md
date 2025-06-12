# URL Shortener - Design Summary

This document summarizes the architectural decisions, optimizations, and implementation strategies used in the URL Shortener Service, with a focus on the following key aspects:

- **Performance**
- **Scalability**
- **Reliability**

...


## ✅ How to Use the System

The system exposes a set of RESTful endpoints via HTTP. You can interact with the API using tools like **Postman**, **curl**, or via **Swagger UI** (`http://localhost:8080/swagger` when running in Docker).

### 🔗 1. Shorten a URL

**POST** `/shorten`

**Request Body**:
```json
{
  "url": "https://example.com/some/long/path"
}
```

**Response**:
```json
{
  "url": "http://localhost:8080/abc123",
  "code": "abc123",
  "status": "Success"
}
```

### 🔄 2. Redirect to Original URL

**GET** `/{code}`  
Redirects the client to the original URL (HTTP 301).

**Example**:
```
GET http://localhost:8080/abc123
→ 301 Redirect to https://example.com/some/long/path
```

### 📊 3. Get Click Stats

**GET** `/api/stats/{code}`

**Example**:
```
GET http://localhost:8080/api/stats/abc123
```

**Response**:
```json
{
  "stats": {
    "code": "abc123",
    "clicks": 4,
    "createdAt": "2024-06-09T15:23:00Z"
  },
  "status": "Success"
}
```

### ⚠️ Error Responses

If something goes wrong, the service returns structured error responses like:

```json
{
  "status": "DatabaseError",
  "message": "Database unavailable."
}
```

Other possible status values include:
- `NotFound`
- `UnexpectedError`
- `InvalidRequest`


## 📦 Final Notes
- All logic is covered with **unit tests**.
- Input validation is enforced for URL format and code length.
- API responses are standardized via DTOs and error response models.

This design ensures a high-performance, scalable, and reliable system ready for production use.

---

## 🐳 Docker & Deployment Instructions

### 🔹 Running the Full System with Docker

Use the provided `docker-compose.yml` to run the full environment including:

- ASP.NET Core API
- SQL Server database
- Redis distributed cache

### 🔸 Steps to Run:

1. **Build and start the containers**:
   ```bash
   docker compose up --build
   ```

2. **Access the API via Swagger**:
   - Navigate to: [http://localhost:8080/swagger](http://localhost:8080/swagger)

3. **Stop containers**:
   ```bash
   docker compose down
   ```

---

## 🛠️ Database & Table Creation

When the containerized API starts, the following happens:

- `AppDbContext` uses `db.Database.Migrate()` internally.
- If the SQL Server database or tables do not exist, EF Core **automatically creates them** using Migrations.
- No manual intervention is needed.

Ensure the connection string inside `appsettings.json` points to the **SQL Server container hostname**, for example:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=db;Database=OkToPost;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
}
```

This ensures the app connects to the `db` service defined in `docker-compose.yml`.

---

---

## 🚀 Running the System with Docker

Use the `docker-compose.yml` to spin up the entire system including:

- ASP.NET Core Web API (`app`)
- SQL Server database (`db`)
- Redis cache (`redis`)
- Unit tests (`tests` - optional)

### 🧪 1. Run Application + Database + Redis

```bash
docker compose up --build
```

- The `app` container will be built from `OkToPost/Dockerfile`.
- Database and Redis will also start automatically.
- Visit: [http://localhost:8080/swagger](http://localhost:8080/swagger)

### ✅ 2. Run Only the Tests (Optional)

```bash
docker compose run --rm tests
```

- This will run the tests defined in `OkToPost.Tests` and print results to the console.

### 🧹 3. Stop and Clean Up

```bash
docker compose down
```

- Stops all containers and removes the network and test container.

---

## 🗃️ Database & Migrations

- The application uses **Entity Framework Core** with `Migrations`.
- On startup, EF runs `db.Database.Migrate()` to automatically:
  - Create the database if it does not exist.
  - Apply all pending migrations.
- You don't need to manually create tables or seed data.

Make sure the connection string points to the **SQL Server container**:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=db;Database=OkToPost;User Id=sa;Password=YourStrong@Passw0rd;TrustServerCertificate=True"
}
```

---

## ✅ Performance
### 🔹 SQL Index Optimization
- `ShortCode` is defined as the **primary key** to allow fast lookups during redirects.
- A **unique non-clustered index** is applied to the `OriginalUrl` column to:
  - Prevent duplicate entries
  - Speed up searches when shortening an already existing URL

### 🔹 Background Database Calls (Click Count Increment)
- When a URL is resolved from cache, the click count is incremented **asynchronously in the background**.
- This avoids blocking the user request while ensuring statistics are updated.
- A scoped service is used within a `Task.Run` to safely access the database from a background thread.


- `IDistributedCache` (Redis) used for "hot" URL lookups.
- Avoids DB call for popular shortcodes.
- Background increment of click count using `Task.Run(...)` with `IServiceScopeFactory`.

---

## ✅ Scalability

- **Stateless** Web API with no in-memory dependency = horizontal scaling.
- **Redis** enables distributed caching between containers.
- **Docker** and `docker-compose` ensure consistent multi-container deployment.
- Database, cache, and web servers are fully **separated services**.

---

## ✅ Reliability

- Graceful handling of DB errors — returns 503 without crashing.
- Caching avoids DB dependency for frequent lookups.
- Tests run in isolation using Docker (`tests` service).
- Logs and typed responses (`ApiErrorResponse`) improve observability and monitoring.
