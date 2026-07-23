# Startup Speed Design

## Problem

Launching `AssetEditor.exe` takes roughly 8 seconds before the main window is usable. Historical profiler data (`UI架构说明.md`, measured on an older revision) breaks this down as:

- MEF lazy initialization (`GetExport<T>().Value` cascade): ~4051ms
- `CompositionContainer.InitializeAll()` (60+ services): ~1487ms
- Message loop / first main-window layout+paint: ~2034ms
- Resource load: ~233ms
- SplashScreen show: ~206ms
- `ComposeEnvironmentParts`: ~106ms

In addition, the splash screen is created and shown only **after** the type-list construction and `new TypeCatalog(...)` (Program.cs `MainImpl`, splash at ~line 366, catalog at ~line 363). The reflection-heavy type gathering and catalog building run with no visible window, producing a blank-window period at the very start of the launch. This is the most user-visible part of the startup: the process appears dead before the splash ever appears.

The data above predates recent changes; the current per-phase breakdown must be re-measured before targeting specific optimizations.

## Goals

- Eliminate the initial blank-window period so the user sees feedback (the splash screen) almost immediately after launch.
- Reduce the real time from launch to a usable main window where this can be done within the existing initialization order.
- Make optimization decisions from freshly measured data, not stale profiler numbers.

## Non-Goals

- Do not defer any service initialization past the point it currently runs.
- Do not change the MEF composition order or the relative order of existing initialization steps.
- Do not parallelize service construction or file I/O in this change (that was a separate, rejected option).
- Do not cache or otherwise optimize the `TypeCatalog` reflection in this change (separate, rejected option).
- Do not move native DLL loading (`CivTechContext.EnsureCreated`) off its current path.
- Do not alter document-open behavior (handled separately).

## Selected Approach

Three phases, data-driven. Phase 0 produces the measurements that justify and target Phase 2; Phase 1 is a standalone, low-risk perceived-speed win that does not depend on measurement.

### Phase 0 — Startup instrumentation

Add a `Stopwatch` at the top of `MainImpl` and write `PaintTimingLog.Write("Startup: <phase> elapsed={0}ms", ...)` markers at each significant step, so the current real breakdown is captured in `paint_timing.log`:

- after `new App()` + `ImportResourceDictionary`
- after type-list construction (`GetCoreTypes`/`GetToolAppTypes`/`GetAssetEditorTypes`/`GetAssetPreviewerTypes`/`GetDebuggingTypes` + `RegisterEnvironmentParts`)
- after `new TypeCatalog(list)` + `CompositionContainer` creation
- after `splash.Show()`
- after `ComposeEnvironmentParts`
- after `AssetEditorForm` creation + `Compose(batch)`
- after `IAssetCloudSettingService.Value`
- after `CivTechContext.EnsureCreated`
- after project selection block
- after `IProjectMapService.Value` + `IWorkspaceDependencyRegistryService.Value`
- after `ICivTechService.Value`
- after `compositionContainer.InitializeAll()`
- at `assetEditorForm.Shown`: `Startup: main window shown elapsed={0}ms` (total launch-to-visible time)

These markers are pure logging and do not change behavior. They remain in the code as ongoing startup diagnostics (consistent with the existing `PaintTimingLog` usage).

### Phase 1 — Show the splash screen earlier (core perceived win)

`Firaxis.ATF.SplashScreen` is a self-contained WinForms `Form`. Its constructor only builds WinForms controls and references compiled resources (`Firaxis.ATF.Properties.Resources.Splashscreenvert`); `CaptionImage` uses `ResourceUtil.GetIcon(Resources.AssetEditorIcon)`. It has **no dependency on MEF, the type catalog, or the type list**.

Move the existing splash creation/configuration/`Show()` block (Program.cs ~lines 366–371) to immediately **after** `new App()` + `ImportResourceDictionary` (~line 347) and **before** the type-list construction (~line 348). The splash then appears during type gathering, catalog construction, and the entire MEF cascade, removing the blank window.

What stays unchanged:
- `splash.HookOutputWriter(value2)` remains where the `ISplashScreenOutputWriter` export becomes available (~line 425).
- All later `splash.Message = ...` progress updates remain at their current points.
- `splash.Close()` in `assetEditorForm.Shown` is unaffected; the `splash` variable remains in `MainImpl` scope.

Safety argument: the statements between the new splash position and the old one (type list, `RegisterEnvironmentParts`, `EmbeddedCollectionEditor` images, `TypeCatalog`, `CurrentUICulture`, `Localizer`) neither depend on the splash nor are depended upon by the splash. The splash is shown after the WPF `Application` (`new App()`) exists, preserving WinForms/WPF interop ordering.

### Phase 2 — In-order optimization, driven by Phase 0 data

Each item below is evaluated only if Phase 0 data shows it is a real bottleneck; nothing is changed speculatively.

- **First-frame optimization.** Use the `Startup: main window shown` marker to locate the main-window first-frame cost (historically ~2s in the message loop). Investigate `AssetEditorForm` first layout/paint cost sources (main-form control creation, docking layout, theme/skin application) and optimize the measured bottleneck. This does not change any service's initialization timing.
- **In-order redundancy removal.** If Phase 0 shows a service constructor performing duplicate file I/O or duplicate computation (e.g., a config file read more than once), consolidate it within the existing order. No parallelization and no deferral.

## Constraints

- Preserve MEF composition correctness: no concurrent `GetExport` calls, no reordering of dependent services.
- Preserve native DLL load path (`CivTechContext.EnsureCreated`) and its assertion-configuration dependency on `IAssetCloudSettingService`.
- Preserve all existing fatal-error handling paths in `MainImpl` (ProjectConfigException, ResultCodeException, missing ToolHost DLL), including their `splash?.Close()` calls — the splash now exists earlier, so these paths still close it correctly.
- Keep the splash a WinForms form shown after the WPF `Application` is created.

## Error Handling

- The earlier splash creation must not throw on the early path; it only constructs controls and reads compiled resources, which are available at that point.
- Existing `try/catch` blocks around `Compose(batch)`, project selection, and project-map setup are unchanged; their `splash?.Close()` calls remain valid because `splash` is now assigned earlier.
- Phase 2 changes each carry their own focused error handling and are gated on measured data.

## Testing

Startup is integration-level; there is no unit test harness for it. Verification is:

- Build the main application with 0 errors.
- Run the application once and read the `Startup:` markers from `paint_timing.log`; confirm the splash `Show()` marker precedes the `TypeCatalog`/MEF markers (blank window removed) and record the real per-phase timings.
- Manual functional regression: services initialize correctly, the main window appears, and a document can be opened (reuse the existing document-open path to confirm no collateral regression).
- Compare `Startup: main window shown` before and after the change.

## Acceptance Criteria

- The splash screen is visible before MEF composition begins; the initial blank window is eliminated.
- The current startup per-phase breakdown is captured in `paint_timing.log` via `Startup:` markers.
- No service initialization is deferred and no initialization order is changed.
- The main window reaches a usable state no slower than before; Phase 1 improves perceived speed and Phase 2 improves measured time where data justifies it.
- Existing fatal-error handling, native DLL loading, and document-open behavior do not regress.
