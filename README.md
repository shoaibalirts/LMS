# LMS - Learning Management System

## Compose files

| File | Use it for |
|---|---|
| `docker-compose-without-swarm.yml` | **Running locally with Docker.** Builds the API and frontend from source. |
| `docker-compose.yml` | **Docker Swarm deployments only.** Pulls prebuilt images from Docker Hub and runs multiple replicas. |

> ⚠️ Don't run plain `docker compose up` locally. It uses `docker-compose.yml`, which starts 3 API and 2 frontend replicas that all try to bind the same host ports (8080 / 80). Only one starts and the rest stay stuck in `Created`.

## Quick Start (Docker)

Prerequisite: Docker Desktop running, and ports **80, 1433, 3000, 3100, 8080, 9090, 9100** free (check with `docker ps` and stop anything using them).

Clone the repo and run from the project root:

```bash
docker compose -f docker-compose-without-swarm.yml up --build
```

The first run takes a few minutes to build the images. The API may restart once or twice while SQL Server initialises (~20s). Later starts are faster:

```bash
docker compose -f docker-compose-without-swarm.yml up
```

Add `-d` to run in the background. To stop everything:

```bash
docker compose -f docker-compose-without-swarm.yml down
```

To stop and wipe all data (database, Grafana dashboards, Prometheus data):

```bash
docker compose -f docker-compose-without-swarm.yml down -v
```

Once it's up: frontend at http://localhost, API at http://localhost:8080, API docs at http://localhost:8080/scalar.

---

## Local Development (without Docker)

Prerequisites: .NET 9 SDK, Node.js 20.19+ or 22.12+, and SQL Server LocalDB (Windows). In Development mode the API connects to `(localdb)\MSSQLLocalDB` and creates and seeds the `LMS` database on first start. If it can't connect, run `sqllocaldb start MSSQLLocalDB`.

**One-time setup:**
```bash
cd lms_frontend
npm install
```

**Start both** (Windows, from the project root; Ctrl+C stops both):
```powershell
.\start.ps1
```

**Or start them separately:**
```bash
# Backend
cd LMS_API
dotnet run --launch-profile http

# Frontend (second terminal)
cd lms_frontend
npm run dev
```

- API: http://localhost:8080 (docs at http://localhost:8080/scalar)
- Frontend: http://localhost:5173

> The frontend calls the API at `http://localhost:8080` (hard-coded in `lms_frontend/src/Services/api.js`), so the API must run on port 8080. Don't run this alongside the Docker setup, because both use port 8080.

---

## Docker Swarm Deployment

`docker-compose.yml` is a Swarm stack file. Push the images first (`shoaibali808/csharp-lms-webapi` and `shoaibali808/vue-lms-frontend`), then on the manager node:

```bash
docker swarm init            # once, if not already a swarm
docker stack deploy -c docker-compose.yml lms
docker stack rm lms          # to remove
```

---

## Services

| Service | URL | Description |
|---|---|---|
| Frontend | http://localhost | Vue.js app served by Nginx |
| API | http://localhost:8080 | ASP.NET Core 9 REST API |
| API Metrics | http://localhost:8080/metrics | Prometheus scrape endpoint |
| Prometheus | http://localhost:9090 | Metrics collection and storage |
| Grafana | http://localhost:3000 | Dashboards and visualization |
| Node Exporter | http://localhost:9100 | Host machine metrics |
| Loki | http://localhost:3100 | Log aggregation and storage |
| Promtail | — | Log collector (no UI) |

All the services and its ports can be found in docker-compose.yml.

### Service Details

**db** — Microsoft SQL Server 2022. Seeded automatically on first start in Development mode. Data persisted in the `sql_data` Docker volume.

**api** — ASP.NET Core Web API. Depends on `db`. Will restart automatically while SQL Server is initialising (takes ~20s on first run). Exposes a `/metrics` endpoint for Prometheus.

**web** — Vue 3 frontend built with Vite and served via Nginx. Depends on `api`.

**prometheus** — Scrapes metrics every 15 seconds from the API and node-exporter. Config lives in `prometheus.yml`.

**grafana** — Visualisation dashboards. Default login is `admin` / `admin`. Data persisted in the `grafana_data` Docker volume. Prometheus and Loki are auto-provisioned as data sources via `grafana/provisioning/datasources/datasources.yml`.

**node-exporter** — Exposes host machine metrics (CPU, memory, disk, network) to Prometheus.

**loki** — Stores and indexes logs shipped by Promtail. Queried from Grafana using LogQL.

**promtail** — Tails Docker container logs and ships them to Loki. Config lives in `promtail-config.yml`.

---

## Grafana Setup

Data sources (**Prometheus** and **Loki**) and dashboards are provisioned automatically on startup — no manual setup needed. After logging in (`admin` / `admin`), both data sources and the **LMS** dashboard folder will already be available.

Dashboards are loaded from `grafana/provisioning/dashboards/`. Any `.json` file placed there will be auto-imported on startup. To update a dashboard, export it as code from Grafana and overwrite the existing `.json` file, then restart the container.

### Create a dashboard (if starting from scratch)

Dashboards → New → New dashboard → Add visualization

---

## Grafana Dashboard Queries

### API Metrics

**Request rate (requests/second, by status code)**
```promql
sum(rate(http_requests_received_total[5m])) by (code)
```

**Error rate — 4xx and 5xx (%)**
```promql
sum(rate(http_requests_received_total{code=~"4..|5.."}[5m])) / sum(rate(http_requests_received_total[5m])) * 100
```

**Average response time (seconds)**
```promql
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])
```

### Host Metrics (Node Exporter)

**CPU usage (%)**
```promql
100 - (avg(rate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)
```

**Memory used (%)**
```promql
100 - ((node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes) * 100)
```

### Log Queries (Loki / LogQL)

**API logs (all)**
```logql
{container="csharp_webapi"} |= ``
```

**API logs (errors only)**
```logql
{container="csharp_webapi"} |= `error`
```

**Frontend logs (all)**
```logql
{container="vue_frontend"} |= ``
```

**Frontend logs (errors only)**
```logql
{container="vue_frontend"} |= `error`
```


## Grafana Alerts

Create them under **A graph card → More → New alert rule**. All alerts go in folder `LMS`, group `LMS`, evaluation interval `1m`, keep firing `none`.

### Avg Response Time
**Query:**
```promql
rate(http_request_duration_seconds_sum[5m]) / rate(http_request_duration_seconds_count[5m])
```
**Condition:** IS ABOVE `0.5` (500ms)
**Summary:** `High average response time on LMS API`
**Description:** `Average response time has exceeded 500ms for more than 5 minutes.`

---

### HTTP Error Rate
**Query:**
```promql
sum(rate(http_requests_received_total{code=~"4..|5.."}[5m])) / sum(rate(http_requests_received_total[5m])) * 100
```
**Condition:** IS ABOVE `5` (5%)
**Summary:** `High HTTP error rate on LMS API`
**Description:** `HTTP error rate (4xx/5xx) has exceeded 5% for more than 5 minutes.`

---

### CPU Usage
**Query:**
```promql
100 - (avg(rate(node_cpu_seconds_total{mode="idle"}[5m])) * 100)
```
**Condition:** IS ABOVE `80` (80%)
**Summary:** `High CPU usage on LMS host`
**Description:** `CPU usage has exceeded 80% for more than 5 minutes.`

---

### Memory Usage
**Query:**
```promql
100 - ((node_memory_MemAvailable_bytes / node_memory_MemTotal_bytes) * 100)
```
**Condition:** IS ABOVE `85` (85%)
**Summary:** `High memory usage on LMS host`
**Description:** `Memory usage has exceeded 85% for more than 5 minutes.`

---
