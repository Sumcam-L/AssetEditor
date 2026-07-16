# Document Open First-Frame Priority Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make every newly opened document paint its real editor before expensive document activation and Previewer initialization, without showing loading UI or moving WinForms work to a background thread.

**Architecture:** Keep the existing logical-document virtualization and registration-before-show behavior. Give `DocumentHostControl` an explicit pending-first-frame state, consume it with one bounded host refresh before calling the document client, and defer Previewer `DocumentAdded` initialization by one UI message so it no longer blocks `IDocumentClient.Open`.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, Sony ATF, WeifenLuo DockPanel Suite, MEF, Firaxis native Previewer APIs.

## Global Constraints

- Cover all controls registered with `ControlInfo.IsDocument == true`; do not add editor-type allowlists.
- Do not display a loading overlay, progress message, placeholder, blank document, skeleton, or temporary tab.
- Do not use `Application.DoEvents()` in production code.
- Do not use `WM_SETREDRAW`, whole-window freezes, snapshot overlays, retained inactive documents, delayed-hide timers, or hidden inactive-document pre-positioning.
- Do not construct WinForms controls or Firaxis native editor/preview objects on worker threads.
- Preserve registration-before-show because it protects current property editing behavior.
- Preserve `ControlInfo.Control` as the logical editor and `ControlInfo.HostControl` as the outer `DockContent`.
- Keep existing active-document generation checks and close/project-change cleanup behavior.
- Do not modify or revert unrelated working-tree changes in `Program.cs`, `AssetBrowserFileCommands.cs`, `BaseEntityEditor.cs`, `BaseEntityPropertyContext.cs`, or startup repro files.
- Do not commit unless the user explicitly requests a commit.

## File Map

- Modify `docs/superpowers/repros/control-host-document-virtualization/Program.cs`: reproduce visible registration where a pre-attached document reaches client activation before its first paint.
- Modify `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`: own pending-first-frame state independently from attachment state.
- Modify `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`: render one bounded first frame before document client activation and remove recursive paint-tree flushing.
- Create `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/DeferredPreviewDocumentQueue.cs`: isolate cancellable next-message dispatch for newly added preview documents.
- Modify `Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs`: expose the queue only to its focused repro assembly.
- Create `docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj`: build the focused queue repro.
- Create `docs/superpowers/repros/previewer-deferred-document-work/Program.cs`: verify deferred, duplicate, canceled, and cleared work.
- Modify `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs`: defer native Previewer creation, cancel removed documents, and preserve pending documents across project changes.

---

### Task 1: Reproduce Missing First Paint Before Activation

**Files:**
- Modify: `docs/superpowers/repros/control-host-document-virtualization/Program.cs:1-215`
- Test: `docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj`

**Interfaces:**
- Consumes: existing `ControlHostService.RegisterControl(Control, ControlInfo, IControlHostClient)`.
- Produces: a failing assertion proving a document registered while the form is visible has painted its root and nested child before `IControlHostClient.Activate` starts slow non-paint work.

- [ ] **Step 1: Add a paint-tracking control and activation probe**

Add `using System.Threading;`. Add these members below `NewDocumentPanel()` and extend `TestClient` with `Activating`:

```csharp
private sealed class PaintTrackingPanel : Panel
{
    public int PaintCount { get; private set; }

    protected override void OnPaint(PaintEventArgs e)
    {
        PaintCount++;
        base.OnPaint(e);
    }
}

private static PaintTrackingPanel NewPaintDocumentPanel(string name, out PaintTrackingPanel child)
{
    var root = new PaintTrackingPanel
    {
        Name = name,
        Tag = name,
        BackColor = Color.FromArgb(24, 48, 72)
    };
    child = new PaintTrackingPanel
    {
        Name = name + "Child",
        Dock = DockStyle.Fill,
        BackColor = Color.FromArgb(72, 48, 24)
    };
    root.Controls.Add(child);
    return root;
}
```

In `TestClient`:

```csharp
public Action<Control> Activating { get; set; }

public void Activate(Control control)
{
    Activating?.Invoke(control);
    LastActivated = control;
    ActivationHistory.Add(control);
}
```

- [ ] **Step 2: Add the visible-registration first-frame scenario**

After the existing `service.UnregisterControl(first); Application.DoEvents();` block and before the final PASS, add:

```csharp
PaintTrackingPanel baselineChild;
PaintTrackingPanel targetChild;
using (PaintTrackingPanel baseline = NewPaintDocumentPanel("visibleBaseline", out baselineChild))
using (PaintTrackingPanel target = NewPaintDocumentPanel("visibleTarget", out targetChild))
{
    service.RegisterControl(baseline, NewDocumentInfo("Visible Baseline"), client);
    Application.DoEvents();

    bool targetPaintedBeforeActivation = false;
    client.Activating = control =>
    {
        if (control == target)
        {
            targetPaintedBeforeActivation = target.PaintCount > 0 && targetChild.PaintCount > 0;
            Thread.Sleep(250);
        }
    };

    service.RegisterControl(target, NewDocumentInfo("Visible Target"), client);
    client.Activating = null;

    if (!targetPaintedBeforeActivation)
    {
        Console.Error.WriteLine("FAIL: visible registered document reached slow activation before its first complete paint.");
        return 1;
    }

    service.UnregisterControl(target);
    service.UnregisterControl(baseline);
    Application.DoEvents();
}
```

The deliberate sleep models synchronous `DocumentRegistry.DocumentAdded` work without introducing native Previewer dependencies into this repro.

- [ ] **Step 3: Run the repro and verify RED**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
```

Expected: exit code 1 with:

```text
FAIL: visible registered document reached slow activation before its first complete paint.
```

If it passes before production changes, inspect the event sequence and make the assertion observe the nested target child before activation; do not weaken the assertion or insert an event pump inside production code.

- [ ] **Step 4: Check the isolated test diff**

Run:

```powershell
rtk git diff --check -- "docs/superpowers/repros/control-host-document-virtualization/Program.cs"
rtk git diff -- "docs/superpowers/repros/control-host-document-virtualization/Program.cs"
```

Expected: no whitespace errors; only the focused repro changes above.

---

### Task 2: Render One Pending First Frame Before Client Activation

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs:8-85`
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs:560-660,719-742,1025-1300`
- Test: `docs/superpowers/repros/control-host-document-virtualization/Program.cs`

**Interfaces:**
- Consumes: `DocumentHostControl.AttachLogicalControl()` and the active virtual document host selected by `ControlHostService`.
- Produces: `DocumentHostControl.NeedsFirstFrame`, `DocumentHostControl.MarkFirstFrameRendered()`, and `ControlHostService.RenderPendingDocumentFirstFrame(DockContent, string)`.

- [ ] **Step 1: Add first-frame state independent of attachment**

In `DocumentHostControl`, add the field and members:

```csharp
private bool m_needsFirstFrame = true;

public bool NeedsFirstFrame => m_needsFirstFrame;

public void MarkFirstFrameRendered()
{
    m_needsFirstFrame = false;
}
```

In `AttachLogicalControl()`, set the state only after the logical control is successfully added and made visible:

```csharp
LogicalControl.Visible = true;
m_needsFirstFrame = true;
```

Do not clear the flag merely because the control is attached. Registration intentionally attaches a document while its outer host is still hidden.

- [ ] **Step 2: Replace recursive paint flushing with one bounded document-host refresh**

Delete `FlushDocumentPaint()`, `VisibleTreeUpdateStats`, and `UpdateVisibleTree()`. Add this method in `ControlHostService` in their place:

```csharp
private bool RenderPendingDocumentFirstFrame(DockContent dockContent, string reason)
{
    if (dockContent == null || dockContent.DockState != DockState.Document ||
        !dockContent.Visible || dockContent.Controls.Count == 0)
    {
        return false;
    }

    DocumentHostControl documentHost = GetDocumentHost(dockContent.Controls[0]);
    if (documentHost == null || !documentHost.NeedsFirstFrame ||
        !documentHost.HasAttachedLogicalControl || !documentHost.Visible)
    {
        return false;
    }

    Rectangle displayBounds = documentHost.DisplayRectangle;
    if (displayBounds.Width <= 0 || displayBounds.Height <= 0)
    {
        PaintTimingLog.Write("DocumentFirstFrame: defer reason={0}, bounds={1}", reason, displayBounds);
        return false;
    }

    Control logicalControl = documentHost.LogicalControl;
    logicalControl.Bounds = displayBounds;
    var sw = Stopwatch.StartNew();
    documentHost.Refresh();
    documentHost.MarkFirstFrameRendered();
    PaintTimingLog.Write(
        "DocumentFirstFrame: reason={0}, total={1}ms, control={2}, bounds={3}",
        reason, sw.ElapsedMilliseconds, logicalControl.GetType().Name, displayBounds);
    return true;
}
```

`Control.Refresh()` invalidates and redraws the host and its child controls once. This replaces repeated `Invalidate`/`Update` calls for every visible descendant, which runtime logs measured at 91-439 ms.

- [ ] **Step 3: Gate document client activation on completed first-frame state**

Replace the attach-based flush condition in `ActivateClientIfStillActive()` with:

```csharp
Control logicalControl = AttachDocumentHostIfNeeded(dockContent);
RenderPendingDocumentFirstFrame(dockContent, "active-content");
DocumentHostControl documentHost = GetDocumentHost(dockContent.Controls[0]);
if (documentHost == null || !documentHost.NeedsFirstFrame)
{
    ActivateClient(logicalControl);
}
```

Replace the corresponding block in `AttachVisibleDocumentHost()` with:

```csharp
Control logicalControl = AttachDocumentHostIfNeeded(dockContent);
RenderPendingDocumentFirstFrame(dockContent, "visible-host");
DocumentHostControl documentHost = GetDocumentHost(dockContent.Controls[0]);
if (documentHost == null || !documentHost.NeedsFirstFrame)
{
    ActivateClient(logicalControl);
}
```

This permits ordinary non-virtual controls to activate while ensuring virtual documents do not enter `DocumentRegistry` before their first complete document-host refresh.

- [ ] **Step 4: Remove the registration bypass and cover direct Show**

At the end of `RegisterControl()`, replace the unconditional final activation with:

```csharp
if (info.IsDocument != true)
{
    ActivateClient(control);
}
```

The document path already calls `ShowAndAttachDocumentHost()` and schedules two visible-host recovery scans. Those paths now activate only after first-frame state is consumed.

In `Show(Control control)`, replace:

```csharp
AttachDocumentHostIfNeeded(dockContent);
```

with:

```csharp
AttachVisibleDocumentHost(dockContent);
```

This covers an already-active document shown after `Hide()` without adding a separate lifecycle path.

- [ ] **Step 5: Run the focused repro and verify GREEN**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
```

Expected: exit code 0 and:

```text
PASS: document virtualization routes lifecycle to logical controls and attaches only the active real control.
```

The new visible-registration assertion must pass before the 250 ms activation sleep completes.

- [ ] **Step 6: Run host regression repros**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-before-show/ControlHostBeforeShowRepro.csproj"
```

Expected: all four print `PASS` and exit 0.

- [ ] **Step 7: Build the shared host and inspect the diff**

Run:

```powershell
rtk dotnet build "Atf.Gui.WinForms/Atf.Gui.WinForms.csproj"
rtk git diff --check -- "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs" "Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs"
rtk git diff -- "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs" "Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs"
```

Expected: 0 build errors, no whitespace errors, no `Application.DoEvents`, no recursive visible-tree update, and no changes to global DockPanel visibility behavior.

---

### Task 3: Defer and Cancel Previewer Document Creation

**Files:**
- Create: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/DeferredPreviewDocumentQueue.cs`
- Modify: `Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs:1-17`
- Create: `docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj`
- Create: `docs/superpowers/repros/previewer-deferred-document-work/Program.cs`
- Modify: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs:28-76,95-155,442-467`

**Interfaces:**
- Produces: `DeferredPreviewDocumentQueue<T>.Enqueue(Control, T, Action<T>)`, `Cancel(T)`, `PendingItems`, and `Clear()`.
- Consumes: the first-frame-before-activation ordering from Task 2; `DocumentAdded` now occurs only after the real document host has refreshed.

- [ ] **Step 1: Write the focused deferred-queue repro first**

Create `docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net462</TargetFramework>
    <UseWindowsForms>True</UseWindowsForms>
    <PlatformTarget>x64</PlatformTarget>
    <LangVersion>11.0</LangVersion>
    <AssemblyName>PreviewerDeferredDocumentWorkRepro</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Firaxis.AssetPreviewing\Firaxis.AssetPreviewing.csproj" />
  </ItemGroup>
</Project>
```

Create `Program.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetPreviewing;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        using (var form = new Form())
        {
            form.Show();
            Application.DoEvents();

            var queue = new DeferredPreviewDocumentQueue<string>();
            var executed = new List<string>();

            queue.Enqueue(form, "first", executed.Add);
            queue.Enqueue(form, "first", executed.Add);
            if (executed.Count != 0 || queue.PendingItems.Count() != 1)
                return Fail("work did not remain deferred or duplicate work was queued");

            if (!queue.Cancel("first"))
                return Fail("pending work could not be canceled");
            Application.DoEvents();
            if (executed.Count != 0)
                return Fail("canceled work executed");

            queue.Enqueue(form, "second", executed.Add);
            Application.DoEvents();
            if (!executed.SequenceEqual(new[] { "second" }))
                return Fail("deferred work did not execute exactly once");

            queue.Enqueue(form, "third", executed.Add);
            queue.Clear();
            Application.DoEvents();
            if (executed.Count != 1 || queue.PendingItems.Any())
                return Fail("cleared work executed or remained pending");
        }

        Console.WriteLine("PASS: preview document work is deferred, deduplicated, and cancellable.");
        return 0;
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }
}
```

- [ ] **Step 2: Expose the not-yet-existing queue to the repro and verify RED**

Add to `Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs`:

```csharp
[assembly: InternalsVisibleTo("PreviewerDeferredDocumentWorkRepro")]
```

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj"
```

Expected: build failure stating `DeferredPreviewDocumentQueue<>` could not be found.

- [ ] **Step 3: Implement the minimal cancellable queue**

Create `DeferredPreviewDocumentQueue.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

internal sealed class DeferredPreviewDocumentQueue<T> where T : class
{
    private readonly HashSet<T> m_pending = new HashSet<T>();

    public IEnumerable<T> PendingItems => m_pending.ToArray();

    public bool Enqueue(Control dispatcher, T item, Action<T> action)
    {
        if (dispatcher == null || dispatcher.IsDisposed || !dispatcher.IsHandleCreated ||
            item == null || action == null || !m_pending.Add(item))
        {
            return false;
        }

        dispatcher.BeginInvoke((Action)(() =>
        {
            if (m_pending.Remove(item))
            {
                action(item);
            }
        }));
        return true;
    }

    public bool Cancel(T item)
    {
        return item != null && m_pending.Remove(item);
    }

    public void Clear()
    {
        m_pending.Clear();
    }
}
```

- [ ] **Step 4: Run the queue repro and verify GREEN**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj"
```

Expected: exit code 0 and:

```text
PASS: preview document work is deferred, deduplicated, and cancellable.
```

- [ ] **Step 5: Route normal DocumentAdded work through the queue**

Add this field to `PreviewerDocumentService`:

```csharp
private readonly DeferredPreviewDocumentQueue<IPreviewableDocument> m_pendingPreviewDocuments =
    new DeferredPreviewDocumentQueue<IPreviewableDocument>();
```

Replace `DocumentRegistry_DocumentAdded()` with:

```csharp
private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
{
    IPreviewableDocument previewableDocument = e.Item.As<IPreviewableDocument>();
    if (previewableDocument == null)
    {
        return;
    }

    Control dispatcher = PreviewerControlHost?.PreviewerControl;
    if (m_pendingPreviewDocuments.Enqueue(dispatcher, previewableDocument, InitializeAddedDocumentPreviewing))
    {
        PaintTimingLog.Write("Previewer: deferred document add path={0}", previewableDocument.Uri?.LocalPath ?? "null");
        return;
    }

    InitializeAddedDocumentPreviewing(previewableDocument);
}

private void InitializeAddedDocumentPreviewing(IPreviewableDocument previewableDocument)
{
    if (m_shuttingDown || previewableDocument == null ||
        !DocumentRegistry.Documents.Contains(previewableDocument) ||
        PreviewWindows.ContainsKey(previewableDocument))
    {
        return;
    }

    var sw = Stopwatch.StartNew();
    IPreviewWindow previewWindow = AssetPreviewer.OpenWindow(
        PreviewerControlHost.PreviewerControl.Handle,
        PreviewerEntityLoadingService.InstanceSet);
    PreviewWindows[previewableDocument] = previewWindow;
    previewableDocument.PreviewWindow = previewWindow;
    InitializeDocumentPreviewing(previewableDocument, previewWindow);
    PaintTimingLog.Write("Previewer: deferred document add complete={0}ms", sw.ElapsedMilliseconds);
}
```

The synchronous fallback is retained only when the Previewer dispatcher has no usable handle, preserving startup/project-restore behavior. Normal interactive document opens use the deferred branch.

- [ ] **Step 6: Cancel removed or shutting-down pending documents**

At the start of the previewable-document block in `DocumentRegistry_DocumentRemoved()`, before checking `PreviewWindows`, add:

```csharp
if (m_pendingPreviewDocuments.Cancel(previewableDocument))
{
    previewableDocument.PreviewWindow = null;
    PaintTimingLog.Write("Previewer: canceled deferred document add");
    return;
}
```

In `Shutdown()`, after incrementing `m_previewActivateGeneration`, add:

```csharp
m_pendingPreviewDocuments.Clear();
```

This prevents the existing "Removing untracked preview window" assertion when a document closes before its deferred Previewer callback runs.

- [ ] **Step 7: Preserve pending previews through project changes**

In `StartProjectChange()`, before adding `PreviewWindows.Keys`, capture and cancel pending documents:

```csharp
ProjectChangeCache.AddRange(m_pendingPreviewDocuments.PendingItems);
m_pendingPreviewDocuments.Clear();
ProjectChangeCache.AddRange(PreviewWindows.Keys.Where(doc => !ProjectChangeCache.Contains(doc)));
```

Replace the existing unconditional `ProjectChangeCache.AddRange(PreviewWindows.Keys)` with this block. `FinishProjectChange()` continues synchronously restoring every cached preview document through its existing loop because project switching already has an explicit blocking cleanup/restore boundary.

- [ ] **Step 8: Run the queue and host repros together**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
```

Expected: both print `PASS` and exit 0.

- [ ] **Step 9: Build Previewer and inspect the focused diff**

Run:

```powershell
rtk dotnet build "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing.csproj"
rtk git diff --check -- "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/DeferredPreviewDocumentQueue.cs" "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs" "Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs" "docs/superpowers/repros/previewer-deferred-document-work"
rtk git diff -- "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/DeferredPreviewDocumentQueue.cs" "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs" "Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs" "docs/superpowers/repros/previewer-deferred-document-work"
```

Expected: 0 build errors, no whitespace errors, no native work moved to a worker thread, and all queued callbacks guarded by pending-membership or document-registry checks.

---

### Task 4: Full Regression and Runtime Timing Verification

**Files:**
- Verify: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`
- Verify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- Verify: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs`
- Verify: `docs/superpowers/repros/control-host-document-virtualization/Program.cs`
- Verify: `docs/superpowers/repros/previewer-deferred-document-work/Program.cs`

**Interfaces:**
- Consumes: pending-first-frame state and deferred Previewer queue from Tasks 2-3.
- Produces: fresh automated, build, and manual evidence that the new document frame precedes Previewer work for all representative document types.

- [ ] **Step 1: Run the complete focused repro suite**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-before-show/ControlHostBeforeShowRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/document-tab-paint-trace/DocumentTabPaintTraceRepro.csproj"
```

Expected: all nine repros print `PASS` and exit 0.

- [ ] **Step 2: Build touched dependencies in deployment order**

Run:

```powershell
rtk dotnet build "Atf.Gui.WinForms/Atf.Gui.WinForms.csproj"
rtk dotnet build "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing.csproj"
rtk dotnet build "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj"
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: every command exits 0 with 0 errors. Building dependencies directly ensures the SDK deployment directory does not retain stale host or Previewer DLLs.

- [ ] **Step 3: Verify real document opens and timing order**

Launch AssetEditor and open, in this order:

1. An AST as the first document.
2. A second AST while the first remains open.
3. An ArtDef.
4. An XLP.
5. A Game Art Specification.
6. Another available entity document type.

Use both the Assets command and ordinary file-open path across the set. For every open, confirm:

- The old document remains intact during unavoidable synchronous construction.
- No loading text, placeholder, blank document, or small constructor-sized editor appears.
- Once the new tab becomes active, the next complete central frame is the real new editor.
- The Previewer updates afterward without blanking the central document.
- The main document is usable while deferred Previewer work completes.

Expected log order in `paint_timing.log`:

```text
DocumentHostAttach
DocumentFirstFrame: reason=..., total=...ms
Activate: doc=...
Previewer: deferred document add path=...
Opened document ...
Previewer: deferred document add complete=...ms
```

`DocumentFirstFrame` must precede `Activate: doc` and deferred Previewer completion. Normal opens must no longer contain `FlushDocumentPaint` entries.

- [ ] **Step 4: Verify rapid and cleanup scenarios**

Manually verify:

- Open two documents rapidly; the latest selected document and Previewer remain active.
- Close a document immediately after opening; no untracked-preview assertion appears.
- Close the active document and confirm the replacement paints immediately.
- Close an inactive document and confirm the active document stays attached.
- Restore documents at startup.
- Change projects with documents open.
- Exit with multiple documents open.

Expected: no crash, assertion, stale Previewer, blank document, or delayed click requirement.

- [ ] **Step 5: Run final repository checks**

Run:

```powershell
rtk git diff --check
rtk git status --short
rtk git diff -- "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs" "Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs" "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/DeferredPreviewDocumentQueue.cs" "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs" "Firaxis.AssetPreviewing/Properties/AssemblyInfo.cs" "docs/superpowers/repros/control-host-document-virtualization/Program.cs" "docs/superpowers/repros/previewer-deferred-document-work" "docs/superpowers/specs/2026-07-15-document-open-first-frame-priority-design.md" "docs/superpowers/plans/2026-07-15-document-open-first-frame-priority.md"
```

Expected: no whitespace errors; unrelated pre-existing changes remain present and untouched; the focused diff contains only first-frame state, bounded refresh, deferred Previewer work, repros, and documentation.
