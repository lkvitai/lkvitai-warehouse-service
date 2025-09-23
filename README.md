# Technical Specification (Draft v0.1)
**Project:** LKvitai.MES — Warehouse & Agnum Integration  
**Author:** Denis / ChatGPT (for LKvitai.MES)  
**Date:** 2025‑09‑20  
**Status:** Draft (v0.1) — ready for internal review

---

## 1) Project Overview
We are building a warehouse & stock management module (phase‑1) focused on **always‑correct physical balances**, **unlimited logical & physical warehouses**, **unit conversions**, and **flexible financial value adjustments** that **do not change physical stock**. The module will **export stock balances** to _Agnum_ on a configurable schedule and by a **chosen slice** (logical warehouses, physical warehouses, groups, or total).  
Initial UX is a **simple table‑based UI** (operator‑friendly), with a **plan/schematic view** of the warehouse and an **optional 3D visualization** (bonus) in a later iteration.

**Guiding principles:** configuration‑first, minimum custom code, change product types without recompilation, integrate with equipment, and document with C4/BPMN.

---

## 2) Objectives (What success looks like)
1. **Always‑correct physical stock** per SKU (no phantom items).
2. **Unlimited logical & physical warehouses** (reservations, scrap, non‑liquid, quarantine, production, etc.).
3. **Mixed hierarchies**: a product can belong to multiple categories and multiple logical warehouses at the same time.
4. **Unit conversions** (pallet ↔ bag ↔ pcs; meters ↔ m² ↔ units).
5. **Financial adjustments** (values) that affect accounting reports/exports but do not change physical quantities.
6. **Agnum integration** with flexible slicing (logical/physical/group/total) and scheduled export.
7. **Operator‑safe UX** (scanning, validation, undo).
8. **Warehouse plan** (schematic) and **optional 3D** visualization (wow/demo).

---

## 3) Scope
### In scope (phase‑1)
- Master data for **Items (SKU)**, **Warehouses** (logical & physical), **Locations** (zone → rack → bin).
- **Stock operations**: receive, transfer (logical↔logical, physical↔physical), adjust (scrap/return), quality status changes, conversions.
- **Inventory (cycle count / full)** with barcode/label scanning.
- **Unit conversion tables** and auto‑recalculation of balances.
- **Financial value adjustments** (manual; affect exports, not physical stock).
- **Agnum export** (CSV/XML/JSON — one will be primary, others optional) with configurable slicing & schedule.
- **Audit trail** for all movements and value adjustments.
- **Table‑based UI** + schematic warehouse plan (MVP).

### Out of scope (phase‑1)
- Hardware procurement & selection (scanners/printers/tablets).
- Hosting/infrastructure decisions (cloud vs on‑prem) — provide Docker artifacts but no final infra.
- Advanced forecasting & AI features.
- Deep 3D modeling (only basic prototype if time allows).

---

## 4) Constraints & Assumptions
- Tech stack aligns with LKvitai.MES: **.NET microservices in Docker**, **MS SQL** (NoSQL optional), **Bootstrap front‑end in Azure** (or on‑prem IIS), **Raspberry Pi** kiosks with serial printers (later), **Node‑RED** prototype for calculations.
- Integration target: **Agnum** (accounting) and **Avea** (warehouse legacy) later; for phase‑1 export only to Agnum.
- **Single developer** (Denis) — maximize reuse, keep architecture simple & transparent (Harvest + Linear/GitHub Projects for reporting).

---

## 5) Actors & Roles (warehouse‑focused)
- **FG Warehouse Operator** — finished goods receiving/transfer/pick, label scan, inventory.
- **RW Warehouse Operator** — raw/aux materials receiving/convert/issue to production, label scan.
- **Production Manager** — oversees logical flows (quarantine, WIP, reservations), approves corrections.
- **Accountant/CFO (Agnum)** — consumes exports, needs flexible slicing and financial adjustments.
- **COO** — wants reliable KPIs & accurate stock at a glance.
- **Quality Inspector** — sets/changes quality statuses (OK/Quarantine/Scrap).
- **System Administrator** — config, permissions, backup/restore, export schedules.
- **QA / Trainer** — tests flows; prepares training materials.

*(Full company actors exist but trimmed to warehouse context for phase‑1.)*

---

## 6) Functional Requirements
### 6.1 Physical & Logical Warehouses
- Unlimited number of **physical** warehouses with hierarchy: _warehouse → zone → rack → bin_.
- Unlimited number of **logical** warehouses (virtual pools/status buckets): _reserve, scrap, non‑liquid, production, quarantine_, etc.
- A SKU’s stock can be distributed across multiple physical locations and multiple logical pools concurrently.

### 6.2 Stock Operations
- **Receive**: add stock by location, batch/label, and quality status.
- **Transfer**: move stock between locations and/or logical pools; preserve batch identity.
- **Adjust**: controlled increase/decrease (e.g., found/missing, scrap); requires reason code.
- **Convert**: unit transformations via conversion tables; preserve total in base UoM.
- **Quality status change**: OK ↔ Quarantine ↔ Scrap — by permission, audited.
- **Inventory**: cycle/full counts with scanning; create/close sessions, variance reconciliation.

### 6.3 Unit Conversions
- Declarative **UoM conversion table** per SKU or category (e.g., 1 pallet = 30 bags; 1 bag = 25 pcs).
- Support for metric conversions (m ↔ m² ↔ pcs) with rounding strategies (up, down, nearest) per SKU.
- UI shows both **display UoM** and **base UoM**; balances consistent in base UoM.

### 6.4 Financial Adjustments (Values)
- Manual **value adjustments** for batches/locations/SKUs to tweak accounting **without** changing quantity.
- Separate **audit ledger** for value changes with reason code and user.
- Exports include **adjusted values**; physical stock reports remain unaffected.

### 6.5 Agnum Integration (Exports)
- Export modalities: **by logical warehouse**, **by physical warehouse**, **by group/category**, **aggregated total**.
- Export formats: **CSV (primary)**; XML/JSON optional.
- **Scheduling**: daily/weekly/monthly/quarterly; manual “Export Now” button.
- **Mapping config**: SKU code, warehouse codes, UoM, quantity (base), value (adjusted), batch/lot (optional), time.
- **Delivery**: file share/SFTP/folder drop; file naming with timestamp & slice code.
- **Validation**: dry‑run & per‑row error file.

### 6.6 Warehouse Plan & Visualization
- **Schematic plan**: zones/racks/bins with counts; quick locate by SKU or label.
- **Optional 3D prototype**: navigate warehouse, highlight bins with given SKU/batch/status.

### 6.7 UX & Safety
- **Table‑first UI**: fast grid, filter, search, keyboard shortcuts; big buttons; minimal dialogs.
- **Scan‑to‑action**: cursor‑less flows for scanners (enter submits).
- **Validation & Undo**: pre‑checks (conversions, permissions), soft‑undo window, hard audit trail.
- **Permissions (RBAC)**: role → operation rights; sensitive ops with 4‑eyes approval.

---

## 7) Non‑Functional Requirements
- **Availability:** 99.5%+ for internal ops; graceful degradation if export fails (retry queue).
- **Performance:** stock list ≤ 300 ms for common filters; movements creation ≤ 200 ms.
- **Auditability:** immutable movement & value‑adjustment logs.
- **Security:** per‑role permissions; signed export files (hash); PII‑free by design.
- **Deployability:** Docker compose/K8s‑ready; health checks; metrics (Prometheus); logs (Grafana).
- **Portability:** MS SQL primary; consider NoSQL for event log if needed.
- **Label printers & scanners:** serial/Ethernet compatible later; mock layer in MVP.

---

## 8) Data Model (Conceptual)
**Item (SKU)**: Id, Code, Name, CategoryIds[], BaseUoM, Active.  
**Warehouse (Physical)**: Id, Code, Name, ParentId (nullable), Type(WH/Zone/Rack/Bin).  
**Warehouse (Logical)**: Id, Code, Name, StatusType (Reserve/Scrap/…).
**StockBatch**: Id, ItemId, BatchCode (label), Quality(OK/Quarantine/Scrap), Mfg/Exp (optional).  
**StockBalance**: ItemId, PhysicalBinId, LogicalId, BatchId, QtyBase, Value (current).  
**Movement**: Id, Type(Receive/Transfer/Adjust/Convert/StatusChange/Inventory), From/To, QtyBase, UserId, Reason, Timestamp.  
**InventorySession**: Id, Scope (bins/items), Status(Open/Closed), StartedAt, ClosedAt.  
**InventoryCount**: SessionId, ItemId, BinId, BatchId, CountQtyBase, ScannerUserId, Variance.  
**ValueAdjustment**: Id, ItemId/BatchId/(BinId optional), DeltaValue, Reason, Timestamp, UserId.  
**UoMConversion**: ItemId/CategoryId, FromUoM, ToUoM, Ratio, RoundingMode.  
**ExportJob**: Id, SliceType, SliceKey, Format, Status, StartedAt, CompletedAt, FilePath, ErrorFile.

*Indexes*: by (ItemId, BinId, LogicalId), by BatchCode, by Movement.Timestamp; filtered indexes for fast open inventory sessions.

---

## 9) Services & API (High‑Level)
- **Warehouse Service**: manage physical/logical structures; plan view.  
  REST: `GET /warehouses`, `POST /warehouses`, …  
- **Stock Service**: movements, balances, inventory, conversions.  
  REST: `POST /movements`, `GET /balances`, `POST /inventory/sessions`, …  
- **Export (Agnum) Service**: slicing rules, schedule, generate files, delivery.  
  REST: `POST /exports/run`, `GET /exports/:id`, `PUT /exports/schedule`, …  
- **Label Service** (later): generate/print serial labels; parse scans.  
- **Auth/RBAC** (shared): roles, permissions, audit hooks.

**Events (MQTT/Bus)** _(optional in MVP)_:  
`stock.received`, `stock.transferred`, `stock.adjusted`, `stock.converted`, `stock.inventory.closed`, `export.completed`.

---

## 10) UI / Screens (MVP)
1. **Stock List (Balances)** — fast grid, filters: SKU, warehouse (phys/logical), quality, batch; totals in base UoM and display UoM; “Export” CTA.
2. **New Movement** — tabs: Receive, Transfer, Adjust, Convert, Status; scan label → prefill; live validation; undo toast.
3. **Inventory Sessions** — create scope (bins/items), print sheets/labels, scan & count UI, variance resolution, close session.
4. **Value Adjustments** — select scope (SKU/batch/bin), delta value, reason, preview effect on export.
5. **Agnum Export** — choose slice (logical/physical/group/total), format, schedule; run now; view history/files/errors.
6. **Warehouse Plan** — schematic tree & grid; locate SKU/batch; highlight bins; nav to movement.

---

## 11) Export to Agnum — Draft Mapping (to confirm)
**CSV columns (proposal):**
- `ExportAt` (UTC), `SliceType` (Logical/Physical/Group/Total), `SliceKey` (code),  
- `ItemCode`, `ItemName` (optional), `BaseUoM`,  
- `QtyBase`, `DisplayQty` (qty + display UoM),  
- `AdjValue` (current value including manual adjustments),  
- `BatchCode` (optional), `LocationCode` (physical bin), `Quality`.

**File naming:** `stock_{SliceType}_{SliceKey}_{yyyyMMdd_HHmmss}.csv`  
**Delivery:** local shared folder or SFTP.  
**Validation:** generate `_errors.csv` for rejected rows.  
**Scheduling:** CRON‑like config; manual override.

_Open questions for Agnum:_ required identifiers, supported slices, decimal/thousand separators, encoding (UTF‑8 BOM?).

---

## 12) Security & Audit
- **RBAC** per role & operation; special approval for Adjust/Value/Status.
- **Audit log** persists: who, what, when, before/after deltas, reason code.
- **Data retention** policy & export of audit for external review.

---

## 13) DevOps & Environments (draft)
- **Repos**: microservices per domain (warehouse, stock, export), shared front (Bootstrap).  
- **CI/CD**: GitHub Actions → Docker images → registry → deploy to dev/test/prod.  
- **Observability**: health endpoints, Prometheus metrics, Grafana dashboards; logs with correlation ids.  
- **Backups**: MS SQL backups; export file retention policy (e.g., 180 days).  
- **Infra**: portable — Azure App Service or on‑prem Docker (MAAS, Portainer, Traefik/Caddy).

---

## 14) Acceptance Criteria (Key)
- **Balances correctness:** For any sequence of Receive/Transfer/Adjust/Convert/Inventory, base UoM totals equal _initial + inputs − outputs_ within rounding rules.
- **Unit conversions:** Converting A→B→A yields original qty (modulo rounding); rounding policy visible and configurable.
- **Inventory:** Scanning a label pulls the exact batch; variance report lists per‑bin deltas; closing session posts movements with audit.
- **Value adjustments:** Change affects `AdjValue` in exports but not `QtyBase` nor movement history.
- **Exports:** Can produce CSV by any slice with correct totals; error file generated for invalid rows; scheduled job runs at configured time.
- **UX safety:** Forbidden actions blocked by RBAC; undo works for N seconds; all operations logged.

---

## 15) User Stories by Sprints (12 weeks / 6 sprints)
### Sprint 1 (Weeks 1–2) — Analysis & UX Prototype
- **As a system administrator**, I want to define logical & physical warehouses, so that I can model real storage structure.  
  **AC:** Create unlimited warehouses with attributes (logical/physical, codes).  
- **As a product manager**, I want to assign an item to multiple categories, so that classification is flexible.  
  **AC:** One SKU linked to multiple categories; API + UI prototype.  
- **As a business analyst**, I want to see a clickable prototype, so that stakeholders validate flows early.  
  **AC:** Table‑based mock with navigation; movement mock forms.

**Deliverable:** Validated prototype and confirmed export format options with Agnum.

### Sprint 2 (Weeks 3–4) — Core Stock CRUD
- **As a warehouse operator**, I want to register receiving items, so that balances increase correctly.  
  **AC:** `POST /movements` Receive; appears in balances.  
- **As a warehouse operator**, I want to transfer stock between warehouses, so that stock movement is tracked.  
  **AC:** Transfer updates From/To bins logically & physically.  
- **As a warehouse operator**, I want to scrap/deduct items, so that balances reflect reality.  
  **AC:** Adjust with reason code; audit record created.  
- **As a manager**, I want to view always‑correct stock balances, so that I trust the system.  
  **AC:** Stock list with filters and totals in base & display UoM.

**Deliverable:** MVP of movements & balances.

### Sprint 3 (Weeks 5–6) — Conversions & Inventory
- **As an operator**, I want to convert pallets ↔ bags ↔ pcs, so that operations are smooth.  
  **AC:** Conversions preserve totals; rounding rules applied.  
- **As a controller**, I want to perform inventory by scanning labels, so that discrepancies are resolved.  
  **AC:** Sessions, counts, variance, posting movements.  
- **As a manager**, I want low/empty stock alerts, so that I can react on time.  
  **AC:** Configurable thresholds; alert feed/log.

**Deliverable:** Inventory with scanning; conversion engine.

### Sprint 4 (Weeks 7–8) — Agnum Exports
- **As an accountant**, I want to export balances by warehouse/group/total, so that accounting is correct.  
  **AC:** CSV export per slice; tested with sample data.  
- **As an accountant**, I want to schedule exports, so that data flows automatically.  
  **AC:** Scheduler + history; error file for rejects.

**Deliverable:** Working export pipeline to Agnum (file drop).

### Sprint 5 (Weeks 9–10) — Value Adjustments & Plan
- **As a CFO**, I want manual value adjustments, so that reported P&L fits requirements.  
  **AC:** Value ledger; export reflects adjustments; audit trail.  
- **As a manager**, I want a warehouse plan (schematic), so that I can quickly locate items.  
  **AC:** Plan page; locate SKU/batch; navigate to bin.
- **As a manager**, I want optional 3D visualization, so that I can demo or explore layout.  
  **AC:** Prototype page (if time permits).

**Deliverable:** Value adjustments + plan view (+ optional 3D demo).

### Sprint 6 (Weeks 11–12) — Testing & Go‑Live
- **As a QA**, I want automated tests for stock operations, so that regressions are caught.  
  **AC:** Unit/integration tests for movements, conversions, exports.  
- **As a trainer**, I want user‑friendly guides, so that operators learn quickly.  
  **AC:** Quickstart & role‑based cheat sheets.  
- **As a COO**, I want the system deployed and stable, so that production can rely on it.  
  **AC:** Deploy to target env; monitoring dashboards; backup/restore tested.

**Deliverable:** Ready for production use; docs & training completed.

---

## 16) Risks & Open Questions
- **Agnum mapping details**: exact field list, identifiers, encoding, error‑handling expectations.  
- **Virtual warehouses in Agnum**: native support vs emulate via groups?  
- **Depth of 3D**: basic highlight vs detailed model.  
- **Mobile devices**: scanners/tablets at MVP or later?  
- **Audit expectations**: do value adjustments require dual approval?  
- **Deployment model**: Azure vs on‑prem vs hybrid; SFTP availability.

Mitigations: lock the export CSV schema early; keep 3D as optional; design audit with 4‑eyes toggle; abstract delivery (folder/SFTP).

---

## 17) Timeline & Milestones (3 months)
- **M1 (Weeks 1–2):** Analysis & Prototype — *approved prototype, export format confirmed.*
- **M2 (Weeks 3–4):** Core Stock — *receive/transfer/adjust, balances list.*
- **M3 (Weeks 5–6):** Conversions & Inventory — *conversion engine, inventory sessions.*
- **M4 (Weeks 7–8):** Exports — *CSV exports by slice; scheduler.*
- **M5 (Weeks 9–10):** Value & Plan — *value adjustments; plan view (+3D demo optional).*
- **M6 (Weeks 11–12):** Test & Go‑Live — *tests, docs, deployment.*

Definition of Done per sprint: features meet AC, tests added, docs updated, demo recorded.

---

## 18) Glossary
- **Physical warehouse**: real location structure (zone/rack/bin).  
- **Logical warehouse**: virtual pool/status (reserve, quarantine, scrap, WIP).  
- **Base UoM**: canonical unit for calculations (e.g., pcs or m²).  
- **Display UoM**: operator‑friendly view (bags, pallets, meters).  
- **Slice**: chosen export dimension (logical/physical/group/total).  
- **AdjValue**: adjusted financial value for export (does not change quantity).

---

## 19) Appendix A — Sample CSV (Proposal)
```
ExportAt,SliceType,SliceKey,ItemCode,BaseUoM,QtyBase,DisplayQty,AdjValue,BatchCode,LocationCode,Quality
2025-09-20T19:30:00Z,Physical,WH1_Z1_R1_B01,ROLLER-01,pcs,120,"4 pallets",2450.00,B20250905,WH1-Z1-R1-B01,OK
2025-09-20T19:30:00Z,Logical,RESERVE,ROLLER-01,pcs,35,"1 bag + 10 pcs",700.00,B20250905,WH1-Z1-R1-B02,OK
```
*To be validated with Agnum (fields, separators, encoding).*

---

**End of document.**
