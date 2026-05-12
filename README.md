# LMS - Learning Management System

## Quick Start

Clone the repo and run from the root of the project (where `docker-compose.yml` lives):

```bash
docker compose up --build
```

First run takes a few minutes to build the images. Subsequent starts are faster:

```bash
docker compose up
```

To stop everything:

```bash
docker compose down
```

To stop and wipe all data (database, Grafana dashboards, Prometheus data):

```bash
docker compose down -v
```

---

## Local Development (without Docker)

**Backend - also starts database:**
```bash
cd LMS_API
dotnet run
```
API runs at http://localhost:8080

**Frontend:**
```bash
cd lms_frontend
npm install // if not installed already
npm run dev
```
Frontend runs at http://localhost:5173

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
