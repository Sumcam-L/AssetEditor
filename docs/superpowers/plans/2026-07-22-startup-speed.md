# Startup Speed Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Eliminate the initial blank-window period before the splash screen appears, and reduce launch-to-usable-main-window time, without deferring any service initialization or changing the MEF composition order.

**Architecture:** Keep the entire existing `MainImpl` initialization sequence intact. Add startup timing markers (Phase 0), move the self-contained WinForms splash screen earlier so it shows during type/catalog/MEF construction (Phase 1), then use the measured data to target first-frame and in-order redundancy optimizations (Phase 2).

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms + WPF interop, MEF (`System.ComponentModel.Composition`), Sony ATF, `PaintTimingLog` diagnostics.

## Global Constraints

- Do not defer any service initialization past its current point in `MainImpl`.
- Do not reorder MEF composition or the relative order of existing initialization steps.
- Do not parallelize service construction or file I/O.
- Do not cache/optimize the `TypeCatalog` reflection.
- Do not move native DLL loading (`CivTechContext.EnsureCreated`) off its current path.
- Preserve all existing fatal-error handling paths in `MainImpl` (their `splash?.Close()` calls must remain valid).
- Keep the splash a WinForms form shown after the WPF `Application` (`new App()`) exists.
- Do not change document-open behavior.
- Prefix shell commands with `rtk` per `AGENTS.md`.
- Do not commit unless the user explicitly requests a commit.

## File Map

- Modify `AssetEditor/AssetEditor/Program.cs`: add startup timing markers in `MainImpl`; move the splash creation/configuration/`Show()` block earlier.
- Reference (read-only) `Firaxis.ATF/Firaxis.ATF/SplashScreen.cs`: confirms the splash is self-contained (no MEF/catalog dependency).
- Reference (read-only) `docs/superpowers/specs/2026-07-22-startup-speed-design.md`: the approved design.
- Output: `paint_timing.log` in the SDK deployment directory receives the `Startup:` markers.

---

### Task 1: Add startup instrumentation (Phase 0)

**Files:**
- Modify: `AssetEditor/AssetEditor/Program.cs` (`MainImpl`, ~lines 332–566)

**Interfaces:**
- Consumes: existing `PaintTimingLog.Write(...)` and `PaintTimingLog.Clear()` (already called at `MainImpl` entry).
- Produces: `Startup: <phase> elapsed=Nms` markers in `paint_timing.log`.

- [ ] **Step 1: Add a startup stopwatch at the top of `MainImpl`**

Immediately after `PaintTimingLog.Clear();` (line 334), add:

```csharp
var startupTimer = Stopwatch.StartNew();
```

Ensure `using System.Diagnostics;` is present at the top of `Program.cs` (add if missing).

- [ ] **Step 2: Insert `Startup:` markers at each phase boundary**

Add a `PaintTimingLog.Write("Startup: <phase> elapsed={0}ms", startupTimer.ElapsedMilliseconds);` line at each of these points in `MainImpl` (after the named statement completes):

1. after `ImportResourceDictionary(...)` (line 347) — label `app+resources`
2. after `RegisterEnvironmentParts(args, list);` and the `EmbeddedCollectionEditor` image assignments (after line 362) — label `type-list`
3. after `using CompositionContainer compositionContainer = new CompositionContainer(new TypeCatalog(list));` (line 363) — label `type-catalog`
4. after `splash.Show();` (line 371) — label `splash-shown`
5. after `ComposeEnvironmentParts(args, compositionContainer)` returns true (after line 378) — label `compose-env`
6. after `compositionContainer.Compose(batch);` (line 395) — label `form-composed`
7. after `_ = export.Value;` for `IAssetCloudSettingService` (line 428) — label `asset-cloud`
8. after `Context.EnsureCreated<CivTechContext>(...)` (line 429) — label `civtech-context`
9. after the project-selection `try` block (after line 461) — label `project-selection`
10. after `_ = export6.Value;` for `IWorkspaceDependencyRegistryService` (line 491) — label `project-map+deps`
11. after `ICivTechService value4 = compositionContainer.GetExport<ICivTechService>().Value;` (line 512) — label `civtech-service`
12. after `compositionContainer.InitializeAll();` (line 527) — label `initialize-all`

- [ ] **Step 3: Add the main-window-shown marker**

In the `assetEditorForm.Shown += delegate { ... }` handler (line 546), as the first statement inside the delegate, add:

```csharp
PaintTimingLog.Write("Startup: main window shown elapsed={0}ms", startupTimer.ElapsedMilliseconds);
```

The `startupTimer` local is captured by the delegate closure.

- [ ] **Step 4: Build**

```bash
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: 0 errors.

---

### Task 2: Show the splash screen earlier (Phase 1)

**Files:**
- Modify: `AssetEditor/AssetEditor/Program.cs` (`MainImpl`)

**Interfaces:**
- Consumes: the self-contained `Firaxis.ATF.SplashScreen` (no MEF/catalog dependency — verified in SplashScreen.cs).
- Produces: splash visible before type/catalog/MEF construction; `Startup: splash-shown` precedes `Startup: type-catalog`.

- [ ] **Step 1: Move the splash block earlier**

Cut the existing splash creation/configuration/`Show()` block currently at ~lines 366–371:

```csharp
Firaxis.ATF.SplashScreen splash = new Firaxis.ATF.SplashScreen("Asset Editor");
splash.ShowInTaskbar = true;
splash.Message = "Constructing components...";
splash.CaptionImage = ResourceUtil.GetIcon(Resources.AssetEditorIcon);
splash.ShowOutputWindow();
splash.Show();
```

Paste it immediately after `ImportResourceDictionary(typeof(AssetBrowserView), "Shared.xaml");` (line 347) and before the `List<Type> list = new List<Type>();` line (line 348).

- [ ] **Step 2: Verify nothing between the new and old positions references `splash`**

Confirm that the statements now between the splash block and its old position (type-list construction, `RegisterEnvironmentParts`, `EmbeddedCollectionEditor` images, `new TypeCatalog(...)`, `CurrentUICulture`, `Localizer`) do not reference `splash`. They do not. The later `splash.HookOutputWriter(...)`, `splash.Message = ...`, and `splash?.Close()` references remain valid because `splash` is still in `MainImpl` scope and now assigned earlier.

- [ ] **Step 3: Confirm the `Startup: splash-shown` marker moved with the block**

The `PaintTimingLog.Write("Startup: splash-shown ...")` line added in Task 1 Step 2 (item 4) sits right after `splash.Show();`, so it moves with the block and now fires before `type-catalog`.

- [ ] **Step 4: Build**

```bash
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: 0 errors.

---

### Task 3: Deploy, measure, verify

**Files:**
- Verify: `paint_timing.log` in the SDK deployment directory.

- [ ] **Step 1: Deploy the rebuilt DLLs to the SDK directory**

Copy the rebuilt `AssetEditor.exe` and dependent DLLs from `AssetEditor/bin/x64/Debug/net462/` to the SDK deployment directory (`...\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\`), per the user's usual deployment step.

- [ ] **Step 2: Run and capture the startup markers**

Launch AssetEditor once, let it reach the main window, then read the `Startup:` markers from `paint_timing.log`:

```
rtk grep "Startup:" "<SDK deployment dir>\paint_timing.log"
```

- [ ] **Step 3: Verify the blank window is removed**

Confirm `Startup: splash-shown` elapsed time is small (well under `Startup: type-catalog`), i.e., the splash appears before MEF/catalog construction. Record the full per-phase breakdown.

- [ ] **Step 4: Manual functional regression**

Confirm: services initialize without error, the main window appears, the splash closes on show, and a document can be opened (reuse the existing document-open path). Confirm no fatal-error path is broken.

- [ ] **Step 5: Record before/after**

Record `Startup: main window shown` total. Compare against the pre-change baseline (if a baseline capture is desired, revert Task 2 temporarily or capture baseline after Task 1 before applying Task 2).

---

### Task 4: Phase 2 — data-driven in-order optimization (conditional)

**Files:**
- To be determined from Task 3 data.

This task is intentionally not fully specified up front: each optimization is justified and scoped only after Task 3 produces the real per-phase breakdown.

- [ ] **Step 1: Analyze the `Startup:` breakdown**

Identify the largest remaining phases. Expected candidates: first main-window frame (historically ~2s message loop) and any service constructor with redundant file I/O.

- [ ] **Step 2: First-frame optimization (if data supports)**

If `Startup: main window shown` minus `Startup: initialize-all` is large, investigate `AssetEditorForm` first layout/paint cost (main-form control creation, docking layout, skin/theme application). Add focused timing inside the main-form initialization, identify the bottleneck, and optimize within the existing order. Do not change service initialization timing.

- [ ] **Step 3: In-order redundancy removal (if data supports)**

If a service constructor performs duplicate file I/O or computation, consolidate it within the existing order. No parallelization, no deferral.

- [ ] **Step 4: Re-measure and verify**

After each Phase 2 change, rebuild, redeploy, re-run, and confirm the targeted phase decreased without regressing other phases or document-open behavior.

---

## Verification Summary

- Build: `rtk dotnet build "AssetEditor/AssetEditor.csproj"` → 0 errors after each task.
- Startup markers present in `paint_timing.log`; `splash-shown` precedes `type-catalog`.
- Manual regression: services initialize, main window appears, splash closes, a document opens, no fatal-error path broken.
- `Startup: main window shown` is no worse than baseline; Phase 1 improves perceived speed; Phase 2 improves measured time where data justifies.
