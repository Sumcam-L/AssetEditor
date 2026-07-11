# Document Tab Deactivation Paint Tracing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Trace why the previously active document tab remains highlighted for 50-150 ms after the new tab highlights.

**Architecture:** Extend the existing generation-gated `DocumentSwitchTrace` with a tab-strip-specific record helper, then instrument `FiraxisDockPaneStrip` at active-content observation, refresh, native paint dispatch, and managed paint boundaries. The diagnostics observe state only and do not alter activation or rendering.

**Tech Stack:** C# 7.3, .NET Framework 4.6.2, WinForms, WeifenLuo DockPanel Suite, existing console repro projects.

## Global Constraints

- Emit tab-strip diagnostics only while `DocumentSwitchTrace.IsActive` is true.
- Do not change document activation, docking layout, tab ordering, visibility, or paint scheduling.
- Diagnostic state must never control rendering.
- Do not introduce timers, delayed callbacks, redraw freezes, snapshot overlays, or synchronous full-strip refreshes.
- Use one focused repro, the existing native trace repro, the document virtualization repro, and the production build.

---

### Task 1: Specify The Tab-Strip Trace Contract

**Files:**
- Create: `docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj`
- Create: `docs/superpowers/repros/document-tab-paint-trace/Program.cs`
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentSwitchTrace.cs`

**Interfaces:**
- Consumes: `DocumentSwitchTrace.Begin(string)`, `DocumentSwitchTrace.End(long)`, `DocumentSwitchTrace.IsActive`, and `PaintTimingLog`.
- Produces: `DocumentSwitchTrace.TraceTabStrip(string phase, string context)` for gated, exception-safe tab-strip records.

- [ ] **Step 1: Create a failing console repro project**

Create `DocumentTabPaintTraceRepro.csproj` using the same target framework, references, and source-link pattern as `docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj`, adding a project reference to `Firaxis.Theme/Firaxis.Theme/Firaxis.Theme.csproj` if required to instantiate the themed strip through a dock pane.

Create `Program.cs` with these assertions:

```csharp
PaintTimingLog.Clear();
DocumentSwitchTrace.TraceTabStrip("refresh", "old=first,new=second,oldRect={X=0,Y=0,Width=80,Height=24},newRect={X=80,Y=0,Width=80,Height=24}");
PaintTimingLog.Flush();
AssertLogDoesNotContain("TabStripTrace");

long generation = DocumentSwitchTrace.Begin("tab-strip-repro");
DocumentSwitchTrace.TraceTabStrip("refresh", "old=first,new=second,oldRect={X=0,Y=0,Width=80,Height=24},newRect={X=80,Y=0,Width=80,Height=24}");
DocumentSwitchTrace.TraceTabStrip("paint-begin", "clip={X=80,Y=0,Width=80,Height=24}");
DocumentSwitchTrace.TraceTabStrip("paint-end", "active=second");
DocumentSwitchTrace.End(generation);
PaintTimingLog.Flush();
AssertLogContains("TabStripTrace");
AssertLogContains("phase=refresh");
AssertLogContains("old=first,new=second");
AssertLogContains("phase=paint-begin");
AssertLogContains("phase=paint-end");
```

Use local `AssertLogContains` and `AssertLogDoesNotContain` methods that read `paint_timing.log`, print one precise `FAIL:` line, and return exit code `1`; print a single `PASS:` line and return `0` when all assertions pass.

- [ ] **Step 2: Run the repro and verify the contract is absent**

Run:

```powershell
rtk test dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
```

Expected: build failure because `DocumentSwitchTrace.TraceTabStrip` does not exist.

- [ ] **Step 3: Add the minimal gated trace helper**

Add this internal method to `DocumentSwitchTrace`:

```csharp
internal static void TraceTabStrip(string phase, string context)
{
    if (!IsActive)
        return;

    try
    {
        PaintTimingLog.Write(
            "TabStripTrace generation={0}, phase={1}, {2}",
            Volatile.Read(ref s_activeGeneration),
            phase ?? "null",
            context ?? string.Empty);
    }
    catch
    {
    }
}
```

- [ ] **Step 4: Run the focused repro**

Run:

```powershell
rtk test dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
```

Expected: `PASS: document tab paint tracing is generation-gated and records refresh and paint boundaries.`

- [ ] **Step 5: Commit the trace contract**

```powershell
rtk git add "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentSwitchTrace.cs" "docs/superpowers/repros/document-tab-paint-trace"
rtk git commit -m "test: specify document tab paint tracing"
```

### Task 2: Instrument The Firaxis Document Tab Strip

**Files:**
- Modify: `Firaxis.Theme/Firaxis.Theme/FiraxisDockPaneStrip.cs:15-40,602-618`
- Modify: `docs/superpowers/repros/document-tab-paint-trace/Program.cs`

**Interfaces:**
- Consumes: `DocumentSwitchTrace.TraceTabStrip(string phase, string context)` from Task 1 and existing `DockPane.ActiveContent`, `Tabs`, `GetTabRectangle(int)`, and `PaintEventArgs.ClipRectangle`.
- Produces: trace phases `active-change`, `refresh`, `native-before`, `native-after`, `paint-begin`, and `paint-end` with old/new identities and rectangles.

- [ ] **Step 1: Extend the repro with strip-level behavior assertions**

Construct a themed `DockPanel`, create one document pane containing two lightweight `DockContent` instances named `first` and `second`, obtain `pane.TabStripControl`, and activate tracing. Switch `pane.ActiveContent` from `first` to `second`, call `RefreshChanges()` through the pane behavior that normally follows activation, and paint the strip into a bitmap after creating its handle.

Assert the log contains:

```text
phase=active-change
old=first
new=second
oldRect=
newRect=
phase=refresh
phase=paint-begin
clip=
phase=paint-end
```

Also assert after tracing that `pane.ActiveContent == second`; this proves diagnostic state did not change activation.

- [ ] **Step 2: Run the focused repro and verify instrumentation is absent**

Run:

```powershell
rtk test dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
```

Expected: `FAIL:` because no `FiraxisDockPaneStrip` phase records exist.

- [ ] **Step 3: Track observed active content without controlling rendering**

Add one field:

```csharp
private IDockContent m_tracedActiveContent;
```

Add helpers that are called only when tracing is active:

```csharp
private void TraceActiveContentChange()
{
    if (!DocumentSwitchTrace.IsActive)
        return;

    IDockContent current = DockPane.ActiveContent;
    if (ReferenceEquals(m_tracedActiveContent, current))
        return;

    IDockContent previous = m_tracedActiveContent;
    m_tracedActiveContent = current;
    DocumentSwitchTrace.TraceTabStrip("active-change", BuildTraceContext(previous, current, null));
}
```

Implement `BuildTraceContext(IDockContent previous, IDockContent current, Rectangle? clip)` to safely format strip bounds, clip, identities, and rectangles. Resolve a rectangle by finding the content in `Tabs`; return `Rectangle.Empty` if missing. Resolve identity from `DockHandler.Form.Name` and `DockHandler.TabText`, catching disposal races and returning `unavailable`.

- [ ] **Step 4: Trace refresh and managed paint boundaries**

At the beginning of `OnRefreshChanges`, call `TraceActiveContentChange()` and record `phase=refresh` with the current old/new context before the existing button, height, and `Invalidate()` calls.

Update `OnPaint` without changing its rendering statements:

```csharp
protected override void OnPaint(PaintEventArgs e)
{
    TraceActiveContentChange();
    DocumentSwitchTrace.TraceTabStrip("paint-begin", BuildCurrentTraceContext(e.ClipRectangle));
    try
    {
        base.OnPaint(e);
        CalculateTabs();
        if (Appearance == DockPane.AppearanceStyle.Document && DockPane.ActiveContent != null && EnsureDocumentTabVisible(DockPane.ActiveContent, repaint: false))
            CalculateTabs();
        DrawTabStrip(e.Graphics);
    }
    finally
    {
        DocumentSwitchTrace.TraceTabStrip("paint-end", BuildCurrentTraceContext(e.ClipRectangle));
    }
}
```

Preserve the file's existing qualification/style where needed.

- [ ] **Step 5: Trace native paint dispatch without owning the handle**

Override `WndProc` and emit `native-before`/`native-after` only for `WM_PAINT` and `WM_ERASEBKGND`, reusing the selected message constants or names from `DocumentSwitchTrace`. Call `base.WndProc(ref m)` exactly once in a `try/finally`; diagnostics must not modify `m.Result`.

- [ ] **Step 6: Run the focused repro**

Run:

```powershell
rtk test dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
```

Expected: `PASS: document tab paint tracing records activation, refresh, native dispatch, and managed paint without changing activation.`

- [ ] **Step 7: Commit the strip instrumentation**

```powershell
rtk git add "Firaxis.Theme/Firaxis.Theme/FiraxisDockPaneStrip.cs" "docs/superpowers/repros/document-tab-paint-trace/Program.cs"
rtk git commit -m "feat: trace document tab paint timing"
```

### Task 3: Verify Integration And Collect Runtime Evidence

**Files:**
- Inspect: `paint_timing.log` in the deployed Asset Editor directory
- Modify only if a diagnostic defect is found: files from Tasks 1-2

**Interfaces:**
- Consumes: all trace phases from Task 2.
- Produces: one evidence-backed root-cause classification from the design spec; no visual fix is included in this plan.

- [ ] **Step 1: Run focused and integration regressions**

Run:

```powershell
rtk test dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
rtk test dotnet run --project "docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj"
rtk test dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
```

Expected: all three print `PASS:` and exit `0`.

- [ ] **Step 2: Build and deploy the production editor**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: `38 projects, 0 errors`. If deployment copy steps report locked DLLs, stop only `AssetEditor.exe` and rerun the same build.

- [ ] **Step 3: Reproduce one switch in each direction**

Open two already-loaded documents, clear `paint_timing.log`, and switch A to B and B to A once. Do not open or close documents during this capture.

- [ ] **Step 4: Classify the root cause from timestamps and clips**

For each direction, compare `active-change`, `refresh`, `native-before WM_PAINT`, `paint-begin`, and `paint-end`:

- First clip excludes `oldRect`: incomplete invalid region.
- Clip covers both but old identity remains current: state-ordering defect.
- Refresh is prompt but native paint dispatch is 50-150 ms late: message-processing delay.
- Paint begins promptly but ends 50-150 ms later: expensive strip paint.

Record exact timestamps and the selected classification in the implementation report. If the two directions disagree, report both and do not implement a speculative fix.

- [ ] **Step 5: Commit any diagnostic corrections only**

If runtime capture required correcting the diagnostics, rerun Steps 1-2 and commit only those corrections:

```powershell
rtk git add "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentSwitchTrace.cs" "Firaxis.Theme/Firaxis.Theme/FiraxisDockPaneStrip.cs" "docs/superpowers/repros/document-tab-paint-trace"
rtk git commit -m "fix: harden document tab paint tracing"
```

If no correction was required, do not create an empty commit.
