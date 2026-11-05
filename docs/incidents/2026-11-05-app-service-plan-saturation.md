# GeoLocation App Service plan saturation - 5 November 2026

## Summary

GeoLocation was unavailable from approximately 14:14 to 14:20 UTC because its
single-worker shared B1 App Service plan was moved from worker
`LW1SDLWK000AHD` to `LW1SDLWK000AJK`, where total host CPU reached 99.7-100%.
Azure App Service diagnostics show that GeoLocation's API process was using
effectively no CPU during the saturation and that all hosted application
processes remained below the detector thresholds. The CPU was therefore
consumed by host/platform processes rather than GeoLocation, CraftPledge, or
abnormal request traffic.

The worker move followed sustained plan memory pressure: memory was normally
above 80%, reached 95% immediately before the move, and was not attributable to
the hosted application processes. GeoLocation had three overlapping monitoring
mechanisms enabled on both Linux apps: Application Insights SDK instrumentation,
App Service codeless instrumentation and profiler/diagnostic extensions. The
profiler scheduler also ran for both apps immediately before the incident.
Those extensions are the strongest identified contributor to non-application
memory and CPU pressure, but the retained Azure data does not expose the exact
host process or the platform reason for moving the plan.

**Root-cause confidence:** high that host/platform work on the replacement
worker caused the CPU saturation; medium that duplicate monitoring and profiler
extensions caused or materially contributed to the preceding memory pressure
and worker move.

## Impact

- Direct API and web application endpoints timed out.
- The public routing endpoint timed out.
- Sitewatch recorded concurrent failures from UK South, East US and West
  Europe.
- The separate degraded bucket around 21:00 UTC was caused by sequential
  Sitewatch deployments and was not a GeoLocation failure.

## Timeline (UTC)

| Time | Evidence |
|---|---|
| 02:00-02:15 | Scheduled production deployment completed successfully, about 12 hours before the incident. |
| 13:45-14:10 | Plan CPU was generally 13-49%; plan memory was generally 84-94%. |
| 14:10-14:14 | Both GeoLocation profiler schedulers refreshed. Memory reached 95%; host `LW1SDLWK000AHD` still averaged 26.2% CPU. |
| 14:14 | GeoLocation request latency increased and availability began failing. |
| 14:15 | App Service diagnostics show the plan moved from `LW1SDLWK000AHD` to `LW1SDLWK000AJK`. |
| 14:15-14:20 | Replacement host CPU averaged 99.7% in the 14:15 bucket and 55.4% in the 14:20 bucket. One-minute plan CPU was 100%, 100%, 99% and 100% from 14:17 through 14:20. |
| 14:19-14:20 | New application role instances appeared; the hosted applications handled cold-start and warm-up requests. |
| 14:21 | Plan CPU fell to 91%; application CPU peaked during recovery, after the host saturation had begun. |
| 14:22 | Plan CPU returned to 24% and availability recovered. |

## Confirmed facts

### Topology and capacity

- Production runs on one Linux B1 worker with 1.90 GB memory and one CPU.
- The plan hosts three apps: GeoLocation API, GeoLocation web and CraftPledge.
- There is no autoscale configuration and B1 cannot add a second worker through
  the workload configuration.
- Seven-day five-minute maximums show chronic pressure:
  - CPU: average 38.18%, p95 66%, p99 99%, 72 samples at or above 80%.
  - Memory: average 83.67%, p95 88%, p99 92%, 2,211 of 2,256 samples at or
    above 80%.

### CPU ownership

- App Service `LinuxCpuDrillDown` reports GeoLocation API CPU below 70% on all
  workers and records effectively zero API CPU during the 14:15 and 14:20
  saturation buckets.
- The detector reports all hosted app processes below 70% while total host CPU
  reached 99.7%.
- Application Insights process counters peaked at only 11.3% for the API, 7.3%
  for the web app and 4.8% for CraftPledge.
- Over the full incident hour, App Service attributed 68.8 CPU-seconds to the
  API, 73.8 to the web app and 21.9 to CraftPledge. These totals do not explain
  a single-core host running at 99-100% for several minutes.

### Traffic and dependencies

- There was no request surge before saturation. GeoLocation handled fewer than
  500 requests per app over the hour.
- The large 4xx minute buckets occurred after the worker move and are consistent
  with warm-up/probe traffic during recovery.
- No App Service 5xx responses were recorded.
- Captured dependencies were low volume. The only GeoLocation dependency
  failures were expected local-address health checks, table cache misses and
  cancellations while the host was unavailable.
- No retry storm, high-volume client, bot or expensive route was present in the
  retained request telemetry.

### Change and platform correlation

- No deployment, configuration, autoscale or administrative activity occurred
  between 13:45 and 14:45 UTC.
- No Azure Resource Health or Service Health event correlated with the incident.
- App Service recorded no application container recycle event.
- All apps changed worker/role instance together, proving the failure boundary
  was the shared plan rather than an individual app.

## Likely contributing factors

1. **Chronic memory saturation.** The plan ran above 80% memory for 98% of the
   seven-day baseline and reached 95% immediately before the worker move.
2. **Duplicate instrumentation.** Both GeoLocation apps combined SDK-based
   Application Insights with App Service codeless instrumentation. Microsoft
   supports choosing one instrumentation method, not both.
3. **Profiler overhead on a small shared worker.** Both apps ran the in-process
   Linux profiler while also configuring App Service profiler and diagnostic
   extensions. Their synchronized two-hour scheduler refresh occurred
   immediately before the incident.
4. **Shared single-worker blast radius.** A host-level issue affected all three
   applications at once, with no second worker available to continue serving
   traffic.

## Telemetry gaps

- App Service HTTP, console and platform diagnostic logs were not sent to Log
  Analytics. Historical user-agent, client IP, platform messages and detailed
  request records are therefore unavailable.
- The App Service detector exposes host CPU and application CPU but not the
  responsible host/system process.
- The platform did not emit an Activity Log or Resource Health record explaining
  why the plan moved workers.
- Application Insights sampling retained too few requests to independently
  reconstruct every warm-up and timeout request.

These gaps prevent naming the exact host process or proving the worker-move
trigger. The evidence is sufficient to exclude GeoLocation request execution,
CraftPledge request execution, dependencies, deployments and abnormal traffic
as the CPU owner.

## Remediation

- Removed App Service codeless instrumentation from both apps and retained the
  existing SDK instrumentation and custom telemetry.
- Removed the in-process profiler package and registration from both apps.
- Removed App Service profiler and diagnostic extension settings that are not
  the supported Linux SDK profiler path.
- Enabled App Service HTTP, application, console and platform logs plus metrics
  in the central Log Analytics workspace.
- Added production alerts for:
  - shared-plan five-minute average CPU at or above 80%;
  - shared-plan five-minute average memory at or above 90%;
  - per-app five-minute average response time above five seconds;
  - five or more per-app HTTP 5xx responses in five minutes.

No API, authorization, routing, cache, health-check or user-visible behavior was
changed. The App Service SKU was not increased.

## Validation and follow-up

The change is validated by solution build, non-integration tests, .NET format,
Terraform format and Terraform validation. After production deployment, compare
the following over at least 24 hours and again after seven days:

- plan memory average, p95 and time above 80%;
- plan CPU p95/p99 and time above 80%;
- worker changes and platform log events;
- API/web p50 and p95 response time;
- availability and HTTP 5xx rate.

If memory remains persistently above 80% after removing duplicate
instrumentation, move GeoLocation to a dedicated plan or revise the shared
hosting capacity in the platform repository. That is a follow-up isolation
decision, not a substitute for the root-cause remediation in this change.
