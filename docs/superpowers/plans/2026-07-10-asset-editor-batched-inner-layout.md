# Asset Editor Batched Inner Layout Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reduce AST opening time by registering all fixed `AssetEditorControl` inner pages under one suspended-layout batch and omitting each page's transient explicit activation.

**Architecture:** Add an opt-in `DockContent.Show` overload that preserves the existing docking operation but can skip its final `Activate()` call. `AssetEditorControl` alone uses that overload inside one outer `DockPanel.SuspendLayout(allWindows: true)` scope, then relies on its existing post-bind geometry/fallback activation to choose the one final active document page.

**Tech Stack:** C# 10, .NET Framework 4.6.2 WinForms, WeifenLuo DockPanel Suite, custom console regression repros.

## Global Constraints

- Limit behavioral changes to constructor-time inner-page creation in `AssetEditorControl`.
- Do not change generic document virtualization, Asset Previewer binding, AST context binding, reload, saved-layout, or user tab-switch behavior.
- Do not lazily create pages.
- Do not use `WM_SETREDRAW`, delayed hiding, or retained-visible inactive pages.
- Geometry remains the preferred final inner page when available.
- Layout suspension must be released through `finally` if page construction throws.
- The workspace is not a Git repository, so commit steps are omitted.

---

## File Structure

- `WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking/DockContentHandler.cs`: expose an opt-in form of the existing show operation that skips only final activation.
- `WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking/DockContent.cs`: forward the opt-in show operation from `DockContent` to its handler.
- `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs`: batch fixed inner-page docking and request non-activating registration.
- `docs/superpowers/repros/asset-editor-inner-active/Program.cs`: assert constructor-time inner pages are not explicitly activated one by one while preserving active-page fallback behavior.

### Task 1: Add A Failing Constructor Activation Regression

**Files:**
- Modify: `docs/superpowers/repros/asset-editor-inner-active/Program.cs:14-25`

**Interfaces:**
- Consumes: `PaintTimingLog.Clear()`, `PaintTimingLog.Flush()`, and the existing `AssetEditorControl` constructor.
- Produces: a regression assertion that rejects constructor-time `DockTrace` entries matching inner page explicit activation.

- [ ] **Step 1: Add the failing constructor trace assertion**

Clear timing output immediately before constructing the first control, flush it immediately afterward, and reject explicit activation traces for the fixed inner document page names:

```csharp
PaintTimingLog.Clear();
using (AssetEditorControl control = new AssetEditorControl(string.Empty, new TestThemeService()))
{
    PaintTimingLog.Flush();
    string constructionLog = File.ReadAllText(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "paint_timing.log"));
    string[] transientPages = { "Cook Params", "Geometries", "Attachments", "Animations", "Particles", "Behaviors", "Splines" };
    foreach (string page in transientPages)
    {
        if (constructionLog.IndexOf("content=" + page + " Activate begin", StringComparison.Ordinal) >= 0)
        {
            Console.Error.WriteLine("FAIL: constructor explicitly activated inner page {0}.", page);
            return 1;
        }
    }

    // Existing host and active-content assertions remain here.
}
```

- [ ] **Step 2: Run the focused repro and verify red state**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
```

Expected: FAIL with `constructor explicitly activated inner page Cook Params` or another fixed page name.

### Task 2: Add Opt-In Non-Activating Dock Registration

**Files:**
- Modify: `WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking/DockContentHandler.cs:1017-1065`
- Modify: `WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking/DockContent.cs:404-407`

**Interfaces:**
- Consumes: existing `DockContentHandler.Show(DockPanel, DockState)` behavior.
- Produces: `DockContentHandler.Show(DockPanel dockPanel, DockState dockState, bool activate)` and `DockContent.Show(DockPanel dockPanel, DockState dockState, bool activate)`.

- [ ] **Step 1: Preserve the existing API as an activating wrapper**

Change the existing handler overload to delegate to the new overload:

```csharp
public void Show(DockPanel dockPanel, DockState dockState)
{
    Show(dockPanel, dockState, activate: true);
}

public void Show(DockPanel dockPanel, DockState dockState, bool activate)
{
    if (dockPanel == null)
    {
        throw new ArgumentNullException(Strings.DockContentHandler_Show_NullDockPanel);
    }
    if (dockState == DockState.Unknown || dockState == DockState.Hidden)
    {
        throw new ArgumentException(Strings.DockContentHandler_Show_InvalidDockState);
    }
    dockPanel.SuspendLayout(allWindows: true);
    DockPanel = dockPanel;
    if (dockState == DockState.Float)
    {
        if (FloatPane == null)
        {
            Pane = DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, DockState.Float, show: true);
        }
    }
    else if (PanelPane == null)
    {
        DockPane dockPane = null;
        foreach (DockPane pane in DockPanel.Panes)
        {
            if (pane.DockState == dockState)
            {
                if (dockPane == null || pane.IsActivated)
                {
                    dockPane = pane;
                }
                if (pane.IsActivated)
                {
                    break;
                }
            }
        }
        Pane = dockPane ?? DockPanel.Theme.Extender.DockPaneFactory.CreateDockPane(Content, dockState, show: true);
    }
    DockState = dockState;
    dockPanel.ResumeLayout(performLayout: true, allWindows: true);
    if (activate)
    {
        Activate();
    }
}
```

- [ ] **Step 2: Forward the opt-in overload from `DockContent`**

Keep the current two-argument API and add:

```csharp
public void Show(DockPanel dockPanel, DockState dockState, bool activate)
{
    DockHandler.Show(dockPanel, dockState, activate);
}
```

- [ ] **Step 3: Build the docking library and verify compatibility**

Run:

```powershell
rtk dotnet build "WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking/WeifenLuo.WinFormsUI.Docking.csproj"
```

Expected: build succeeds with 0 errors. Existing callers continue to use the activating two-argument overload.

### Task 3: Batch `AssetEditorControl` Inner Pages

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs:100-150`
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs:225-249`

**Interfaces:**
- Consumes: `DockContent.Show(DockPanel, DockState, bool activate)` from Task 2.
- Produces: constructor-time registration with no transient explicit page activation and one final layout pass.

- [ ] **Step 1: Add an activation option to the private registration helper**

Keep existing callers compatible and route initial registration through the opt-in overload:

```csharp
private void AddDockContext(Control key, string text, string iconName, DockState state, bool show, bool activate = true)
{
    // Existing metadata, wrapper, child-control, icon, and event setup stays unchanged.
    if (show)
    {
        m_dockContent[key].Show(m_dockPanel, state, activate);
    }
}
```

- [ ] **Step 2: Enclose fixed-page creation in one outer layout suspension**

After adding `m_dockPanel` to the editor, suspend all inner docking layout and release it in `finally`:

```csharp
m_dockPanel.SuspendLayout(allWindows: true);
try
{
    // Construct all eight existing controls exactly as today.
    // Register each with show: true, activate: false.
    AddDockContext(m_propertyEditor, "Properties", string.Empty, DockState.DockTop, show: true, activate: false);
    AddDockContext(m_cookParameterSetEditor, "Cook Params", Resources.CookParametersCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_geometrySetEditor, "Geometries", Resources.GeometryCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_attachmentEditor, "Attachments", Resources.AttachmentsCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_animationSetEditor, "Animations", Resources.AnimationsCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_particleEffectSetEditor, "Particles", Resources.ParticlesCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_behaviorSetEditor, "Behaviors", Resources.BehaviorCategoryIcon, DockState.Document, show: true, activate: false);
    AddDockContext(m_splineSetEditor, "Splines", Resources.SplineCategoryIcon, DockState.Document, show: true, activate: false);
}
finally
{
    m_dockPanel.ResumeLayout(performLayout: true, allWindows: true);
}
```

Do not move data binding into the constructor and do not remove the existing final geometry activation in `Bind()`.

- [ ] **Step 3: Run the focused repro and verify green state**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
```

Expected: PASS; no fixed inner page has an explicit constructor-time `Activate begin` trace, and all existing fallback/saved-layout assertions pass.

### Task 4: Run Cross-Subsystem Regressions And Build

**Files:**
- Verify only; no planned source edits.

**Interfaces:**
- Consumes: completed Tasks 1-3.
- Produces: evidence that batching did not regress document virtualization, preview activation, tab switching, paint flushing, taskbar handoff, or the production build.

- [ ] **Step 1: Run focused document and paint repros**

Run each command:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/paint-timing-log-clear/PaintTimingLogClearRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/startup-taskbar-handoff/StartupTaskbarHandoffRepro.csproj"
```

Expected: every repro prints `PASS` and exits with code 0. If a project filename differs, use the sole `.csproj` in that listed directory without altering test behavior.

- [ ] **Step 2: Build the production editor**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: 0 errors. Existing unrelated warnings may remain. If deployment copy reports a locked DLL, stop only the stale deployed `AssetEditor.exe` and rerun the same build.

- [ ] **Step 3: Perform runtime timing verification**

Launch the deployed editor, open at least three representative AST files after startup, and inspect:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\paint_timing.log
```

Expected:

- No constructor-time explicit activation chain across all fixed inner pages.
- One final valid inner document activation per opened AST.
- No blank document, old-preview rebound, or inner-tab restoration regression.
- Median `InitializeContext create` is measurably below the 614-754 ms baseline.
- Complete AST opening is measurably below the 1431-2048 ms baseline under comparable conditions.
