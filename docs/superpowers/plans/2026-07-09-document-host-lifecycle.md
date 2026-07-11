# Document Host Lifecycle Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Route all normal WinForms document controls through lightweight document hosts so tab switching moves cheap containers while real document controls attach only to the active host.

**Architecture:** `ControlHostService` remains the single integration point. For `ControlInfo.IsDocument == true`, it creates a lightweight `DocumentHostControl` inside each DockContent and keeps mappings between logical real controls and hosted lightweight controls. Existing public APIs still accept real controls and document clients still receive real controls in `Activate`, `Deactivate`, and `Close`; active real controls are tracked per `DockPane` so tiled/split document layouts can keep one visible real control per visible document pane.

**Tech Stack:** C# 11, WinForms, WeifenLuo DockPanel Suite, ATF `ControlHostService`, .NET Framework `net462` repro projects.

## Global Constraints

- Address document-tab flicker for all normal document clients, not only `.ast` files.
- Keep the current outer document tab UX.
- Keep inactive heavyweight document controls out of the visible WinForms window tree.
- Avoid global DockPane document visibility semantic changes.
- Avoid changing non-document tool windows, previewer panels, property grids, output windows, and dock windows.
- Preserve each document client's existing activate, deactivate, close, save, dirty, and read-only behavior.
- Only `ControlInfo.IsDocument == true` controls use the virtual host path.
- Do not use `WM_SETREDRAW` or whole-window redraw freezes.
- Do not retain inactive DockPane document forms globally.
- Do not pre-position hidden inactive documents into the visible DockPane content rectangle.
- Workspace is not a git repository; use verification checkpoints instead of commit steps.

---

## File Structure

- Create `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`: lightweight host that holds a logical document control and exposes attach/detach operations.
- Modify `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`: create document hosts, map logical controls to hosted controls, route activation/close/show/hide/unregister through logical controls.
- Create `docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj`: focused repro project.
- Create `docs/superpowers/repros/control-host-document-virtualization/Program.cs`: verifies logical control routing and active attachment behavior.
- Update existing repro expectations only if direct child assumptions now need to unwrap `DocumentHostControl`; do not weaken paint assertions.

---

### Task 1: Add DocumentHostControl Unit And Repro

**Files:**
- Create: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`
- Create: `docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj`
- Create: `docs/superpowers/repros/control-host-document-virtualization/Program.cs`

**Interfaces:**
- Produces: `internal sealed class DocumentHostControl : UserControl`
- Produces: `public Control LogicalControl { get; }`
- Produces: `public bool HasAttachedLogicalControl { get; }`
- Produces: `public void AttachLogicalControl()`
- Produces: `public void DetachLogicalControl()`

- [ ] **Step 1: Create a failing repro project**

Create `docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk.WindowsDesktop">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net462</TargetFramework>
    <UseWindowsForms>True</UseWindowsForms>
    <PlatformTarget>x64</PlatformTarget>
    <LangVersion>11.0</LangVersion>
  </PropertyGroup>
  <ItemGroup>
    <Reference Include="System.ComponentModel.Composition" />
    <ProjectReference Include="..\..\..\..\Atf.Gui.WinForms\Atf.Gui.WinForms.csproj" />
  </ItemGroup>
</Project>
```

Create `docs/superpowers/repros/control-host-document-virtualization/Program.cs`:

```csharp
using System;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;

internal static class Program
{
    [STAThread]
    private static int Main()
    {
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        using (Form form = new Form())
        using (Panel first = NewDocumentPanel("first"))
        using (Panel second = NewDocumentPanel("second"))
        {
            form.StartPosition = FormStartPosition.Manual;
            form.Bounds = new Rectangle(100, 100, 800, 600);

            ControlHostService service = new ControlHostService(form);
            ((IPartImportsSatisfiedNotification)service).OnImportsSatisfied();

            TestClient client = new TestClient();
            ControlInfo firstInfo = NewDocumentInfo("First");
            ControlInfo secondInfo = NewDocumentInfo("Second");

            service.RegisterControl(first, firstInfo, client);
            service.RegisterControl(second, secondInfo, client);
            form.Show();
            Application.DoEvents();

            if (firstInfo.Control != first || secondInfo.Control != second)
            {
                Console.Error.WriteLine("FAIL: ControlInfo.Control stopped tracking the logical real control.");
                return 1;
            }

            if (firstInfo.HostControl == null || secondInfo.HostControl == null)
            {
                Console.Error.WriteLine("FAIL: document controls did not receive DockContent host controls.");
                return 1;
            }

            Control firstHosted = firstInfo.HostControl.Controls.Cast<Control>().SingleOrDefault();
            Control secondHosted = secondInfo.HostControl.Controls.Cast<Control>().SingleOrDefault();
            if (firstHosted == first || secondHosted == second)
            {
                Console.Error.WriteLine("FAIL: real document controls were hosted directly instead of through lightweight hosts.");
                return 1;
            }

            service.Show(first);
            Application.DoEvents();
            if (client.LastActivated != first)
            {
                Console.Error.WriteLine("FAIL: Activate did not receive first logical real control.");
                return 1;
            }

            service.Show(second);
            Application.DoEvents();
            if (client.LastDeactivated != first || client.LastActivated != second)
            {
                Console.Error.WriteLine("FAIL: activation routing did not use logical real controls when switching.");
                return 1;
            }

            if (first.Parent != null && first.Visible)
            {
                Console.Error.WriteLine("FAIL: inactive first real control is still visible in a host.");
                return 1;
            }

            if (second.Parent == null || !second.Visible)
            {
                Console.Error.WriteLine("FAIL: active second real control is not attached and visible.");
                return 1;
            }

            service.UnregisterControl(first);
            Application.DoEvents();
            if (second.Parent == null || !second.Visible)
            {
                Console.Error.WriteLine("FAIL: unregistering inactive first document disturbed active second document.");
                return 1;
            }

            Console.WriteLine("PASS: document virtualization routes lifecycle to logical controls and attaches only the active real control.");
            return 0;
        }
    }

    private static Panel NewDocumentPanel(string name)
    {
        return new Panel
        {
            Name = name,
            Tag = name,
            BackColor = Color.FromArgb(20, 40, 60)
        };
    }

    private static ControlInfo NewDocumentInfo(string name)
    {
        return new ControlInfo(name, name, StandardControlGroup.Center)
        {
            IsDocument = true
        };
    }

    private sealed class TestClient : IControlHostClient
    {
        public Control LastActivated { get; private set; }

        public Control LastDeactivated { get; private set; }

        public void Activate(Control control)
        {
            LastActivated = control;
        }

        public void Deactivate(Control control)
        {
            LastDeactivated = control;
        }

        public bool Close(Control control)
        {
            return true;
        }
    }
}
```

- [ ] **Step 2: Run the repro to verify it fails**

Run: `rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"`

Expected: `FAIL: real document controls were hosted directly instead of through lightweight hosts.`

- [ ] **Step 3: Create the minimal DocumentHostControl implementation**

Create `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`:

```csharp
using System;
using System.Windows.Forms;

namespace Sce.Atf.Applications;

internal sealed class DocumentHostControl : UserControl
{
	public DocumentHostControl(Control logicalControl)
	{
		LogicalControl = logicalControl ?? throw new ArgumentNullException(nameof(logicalControl));
		Dock = DockStyle.Fill;
		Tag = logicalControl.Tag;
	}

	public Control LogicalControl { get; }

	public bool HasAttachedLogicalControl => LogicalControl.Parent == this;

	public void AttachLogicalControl()
	{
		if (LogicalControl.IsDisposed || HasAttachedLogicalControl)
		{
			return;
		}

		LogicalControl.Visible = false;
		if (LogicalControl.Parent != null)
		{
			LogicalControl.Parent.Controls.Remove(LogicalControl);
		}

		LogicalControl.Dock = DockStyle.Fill;
		Controls.Add(LogicalControl);
		LogicalControl.Visible = true;
		LogicalControl.BringToFront();
	}

	public void DetachLogicalControl()
	{
		if (!HasAttachedLogicalControl)
		{
			return;
		}

		LogicalControl.Visible = false;
		Controls.Remove(LogicalControl);
	}
}
```

- [ ] **Step 4: Build to verify the new file compiles**

Run: `rtk dotnet build "Atf.Gui.WinForms/Atf.Gui.WinForms.csproj"`

Expected: build succeeds. The virtualization repro may still fail until Task 2 wires it into `ControlHostService`.

---

### Task 2: Wire DocumentHostControl Into ControlHostService Registration And Lookup

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- Test: `docs/superpowers/repros/control-host-document-virtualization/Program.cs`

**Interfaces:**
- Consumes: `DocumentHostControl.LogicalControl`, `AttachLogicalControl()`, `DetachLogicalControl()`
- Produces: logical-control lookup helpers inside `ControlHostService`:
  - `private bool UseVirtualDocumentHost(ControlInfo info)`
  - `private Control GetHostedRegistrationControl(Control control, ControlInfo info)`
  - `private Control GetLogicalControl(Control control)`
  - `private DocumentHostControl GetDocumentHost(Control control)`

- [ ] **Step 1: Add helper methods**

Add helper methods near `FindControlInfo` / control lookup helpers:

```csharp
private static bool UseVirtualDocumentHost(ControlInfo info)
{
	return info != null && info.IsDocument.HasValue && info.IsDocument.Value;
}

private static Control GetHostedRegistrationControl(Control control, ControlInfo info)
{
	return UseVirtualDocumentHost(info) ? new DocumentHostControl(control) : control;
}

private static Control GetLogicalControl(Control control)
{
	return control is DocumentHostControl documentHost ? documentHost.LogicalControl : control;
}

private static DocumentHostControl GetDocumentHost(Control control)
{
	return control as DocumentHostControl;
}
```

- [ ] **Step 2: Change RegisterControl to add hosted control but preserve logical ControlInfo.Control**

In `RegisterControl(Control control, ControlInfo info, IControlHostClient client)`, replace the block that stores original dock, sets `control.Dock`, and adds `control` to `dockContent.Controls`.

Old block shape:

```csharp
info.OriginalDock = control.Dock;
control.Dock = DockStyle.Fill;
var tAdd = Stopwatch.StartNew();
dockContent.Controls.Add(control);
```

New block:

```csharp
Control hostedControl = GetHostedRegistrationControl(control, info);
info.OriginalDock = control.Dock;
hostedControl.Dock = DockStyle.Fill;
var tAdd = Stopwatch.StartNew();
dockContent.Controls.Add(hostedControl);
tAdd.Stop();
```

Keep `info.Control = control`, `info.HostControl = dockContent`, `m_dockContent.Add(info, dockContent)`, `dockContent.FormClosing += dockContent_FormClosing`, and the existing `IControlHostPreShowClient` call unchanged around this block.

- [ ] **Step 3: Update FindControlInfo behavior if needed**

Find the existing `FindControlInfo(Control control)` implementation. Ensure it resolves logical controls by comparing `pair.Key.Control` to `GetLogicalControl(control)`. The body should follow this shape:

```csharp
private ControlInfo FindControlInfo(Control control)
{
	Control logicalControl = GetLogicalControl(control);
	foreach (KeyValuePair<ControlInfo, DockContent> pair in m_dockContent)
	{
		if (pair.Key.Control == logicalControl)
		{
			return pair.Key;
		}
	}
	return null;
}
```

- [ ] **Step 4: Run the virtualization repro**

Run: `rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"`

Expected at this task boundary: it may fail on activation routing or attachment, but it must no longer fail with `real document controls were hosted directly`.

---

### Task 3: Route Activation, Deactivation, Close, Show, Hide, And Unregister Through Logical Controls

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- Test: `docs/superpowers/repros/control-host-document-virtualization/Program.cs`

**Interfaces:**
- Consumes: Task 2 logical-control helpers.
- Produces: active attach method `private Control AttachDocumentHostIfNeeded(DockContent dockContent)`.

- [ ] **Step 1: Add active document attach method**

Add this field near the existing private fields:

```csharp
private readonly Dictionary<DockPane, DocumentHostControl> m_activeDocumentHostsByPane = new Dictionary<DockPane, DocumentHostControl>();
```

Add this method near `ActivateClientIfStillActive`:

```csharp
private Control AttachDocumentHostIfNeeded(DockContent dockContent)
{
	Control hostedControl = dockContent.Controls.Count > 0 ? dockContent.Controls[0] : null;
	DocumentHostControl documentHost = GetDocumentHost(hostedControl);
	if (documentHost == null)
	{
		return hostedControl;
	}

	DockPane pane = dockContent.Pane;
	DocumentHostControl activeDocumentHost;
	if (pane != null && m_activeDocumentHostsByPane.TryGetValue(pane, out activeDocumentHost) && activeDocumentHost != documentHost)
	{
		activeDocumentHost.DetachLogicalControl();
	}

	documentHost.AttachLogicalControl();
	if (pane != null)
	{
		m_activeDocumentHostsByPane[pane] = documentHost;
	}
	return documentHost.LogicalControl;
}
```

- [ ] **Step 2: Update activation and deactivation in `dockPanel_ActiveContentChanged`**

Replace `DeactivateClient(m_activeDockContent.Controls[0]);` with:

```csharp
DeactivateClient(GetLogicalControl(m_activeDockContent.Controls[0]));
```

Replace direct activation calls:

```csharp
ActivateClient(dockContent.Controls[0]);
```

with:

```csharp
ActivateClient(AttachDocumentHostIfNeeded(dockContent));
```

Keep deferred activation for document dock states.

- [ ] **Step 3: Update deferred activation**

In `ActivateClientIfStillActive`, replace:

```csharp
ActivateClient(dockContent.Controls[0]);
```

with:

```csharp
ActivateClient(AttachDocumentHostIfNeeded(dockContent));
```

- [ ] **Step 4: Update active item and context menu lookup**

In `dockPanel_ActiveContentChanged`, replace:

```csharp
ControlInfo controlInfo = FindControlInfo(dockContent.Controls[0]);
```

with:

```csharp
ControlInfo controlInfo = FindControlInfo(GetLogicalControl(dockContent.Controls[0]));
```

In `dockPaneStrip_MouseUp`, replace:

```csharp
ControlInfo target = FindControlInfo(m_activeDockContent.Controls[0]);
```

with:

```csharp
ControlInfo target = FindControlInfo(GetLogicalControl(m_activeDockContent.Controls[0]));
```

- [ ] **Step 5: Update close handling**

In `dockContent_FormClosing`, replace:

```csharp
Control control = dockContent.Controls[0];
ControlInfo controlInfo = FindControlInfo(control);
if (controlInfo.Client.Close(control))
```

with:

```csharp
Control control = GetLogicalControl(dockContent.Controls[0]);
ControlInfo controlInfo = FindControlInfo(control);
if (controlInfo.Client.Close(control))
```

- [ ] **Step 6: Update unregister paths to detach virtual hosts**

In both `UnregisterControl(Control control)` and `UnregisterControl(ControlInfo info)`, before removing controls from `dockContent.Controls`, use:

```csharp
DocumentHostControl documentHost = dockContent.Controls.Count > 0 ? GetDocumentHost(dockContent.Controls[0]) : null;
if (documentHost != null)
{
	documentHost.DetachLogicalControl();
	DockPane pane = dockContent.Pane;
	DocumentHostControl activeDocumentHost;
	if (pane != null && m_activeDocumentHostsByPane.TryGetValue(pane, out activeDocumentHost) && activeDocumentHost == documentHost)
	{
		m_activeDocumentHostsByPane.Remove(pane);
	}
}
```

Then remove the actual hosted child:

```csharp
if (dockContent.Controls.Count > 0)
{
	dockContent.Controls.RemoveAt(0);
}
```

Keep restoring `controlInfo.Control.Dock = controlInfo.OriginalDock` or `info.Control.Dock = info.OriginalDock`.

- [ ] **Step 7: Update Show and Hide to find the host via logical controls**

Confirm `Show(Control control)` and `Hide(Control control)` call `FindControlInfo(control)` then `FindContent(controlInfo)` or equivalent, not `FindContent(control)` directly for virtualized documents. If `Hide(Control control)` currently calls `FindContent(control)`, change it to:

```csharp
ControlInfo controlInfo = FindControlInfo(control);
DockContent dockContent = controlInfo != null ? FindContent(controlInfo) : FindContent(control);
```

- [ ] **Step 8: Run the virtualization repro to verify it passes**

Run: `rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"`

Expected: `PASS: document virtualization routes lifecycle to logical controls and attaches only the active real control.`

---

### Task 4: Preserve Existing Repros And Adjust Direct-Child Assumptions

**Files:**
- Modify if needed: `docs/superpowers/repros/control-host-container-paint/Program.cs`
- Modify if needed: `docs/superpowers/repros/control-host-before-show/Program.cs`
- Test: existing repro projects

**Interfaces:**
- Consumes: virtual document host behavior from Tasks 2-3.
- Produces: repros that validate behavior without assuming the real document control is the direct DockContent child.

- [ ] **Step 1: Run existing repros**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-before-show/ControlHostBeforeShowRepro.csproj"
```

Expected:

```text
PASS: hidden inactive document was not pre-moved into the visible content rectangle.
PASS: document host leaves visible child bounds untouched during background paint.
PASS: FiraxisDockPane leaves child content area untouched during pane paint.
PASS: hosted control receives pre-show callback after parenting and before visibility.
```

- [ ] **Step 2: If container paint repro fails because the direct child is now a lightweight host, update its assertion**

In `control-host-container-paint/Program.cs`, replace:

```csharp
Rectangle childBounds = host.Controls[0].Bounds;
```

with:

```csharp
Control visibleChild = host.Controls[0];
Rectangle childBounds = visibleChild.Bounds;
```

Do not change the sentinel paint assertion.

- [ ] **Step 3: If before-show repro fails because document virtualization delays real-control parenting, update expected contract deliberately**

If `control-host-before-show` fails, change the repro to assert the real control received `BeforeControlHostShow()` before the DockContent became visible, but do not require direct parentage to the DockContent. Replace the parent assertion with:

```csharp
if (!control.BeforeShowCalled)
{
    Console.Error.WriteLine("FAIL: hosted control did not receive pre-show callback.");
    return 1;
}
```

Keep the visibility assertion:

```csharp
if (control.WasVisibleDuringBeforeShow)
{
    Console.Error.WriteLine("FAIL: pre-show callback ran after the control became visible.");
    return 1;
}
```

- [ ] **Step 4: Re-run all repros**

Run the same four commands from Step 1 plus:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
```

Expected: all five repros pass.

---

### Task 5: Build And Manual Validation Hooks

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- Test: `AssetEditor/AssetEditor.csproj`

**Interfaces:**
- Consumes: all previous tasks.
- Produces: timing evidence in `PaintTimingLog` for document virtualization switch cost.

- [ ] **Step 1: Add timing logs around attach and detach**

In `AttachDocumentHostIfNeeded`, wrap detach and attach with stopwatches:

```csharp
long tDetach = 0;
long tAttach = 0;
DockPane pane = dockContent.Pane;
DocumentHostControl activeDocumentHost;
if (pane != null && m_activeDocumentHostsByPane.TryGetValue(pane, out activeDocumentHost) && activeDocumentHost != documentHost)
{
	var swDetach = Stopwatch.StartNew();
	activeDocumentHost.DetachLogicalControl();
	tDetach = swDetach.ElapsedMilliseconds;
}

var swAttach = Stopwatch.StartNew();
documentHost.AttachLogicalControl();
tAttach = swAttach.ElapsedMilliseconds;
if (pane != null)
{
	m_activeDocumentHostsByPane[pane] = documentHost;
}
if (tDetach + tAttach > 0)
{
	PaintTimingLog.Write("DocumentHostSwitch: detach={0}ms, attach={1}ms", tDetach, tAttach);
}
```

- [ ] **Step 2: Build AssetEditor**

Run: `rtk dotnet build "AssetEditor/AssetEditor.csproj"`

Expected: `0 errors`. Existing warnings are acceptable if they match the current warning baseline.

- [ ] **Step 3: Run automated repro suite**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
```

Expected: all repros print `PASS`.

- [ ] **Step 4: Manual validation in AssetEditor**

Launch AssetEditor normally, then verify:

```text
Open multiple document types, including .ast, ArtDef, XLP, and at least one other entity document.
Switch earlier/front -> later/back documents and check for flicker.
Switch later/back -> earlier/front documents and check for regressions.
For asset documents, select Attachments, switch away, switch back, and confirm the inner tab is preserved.
Confirm AssetPreviewer and non-document dock windows remain responsive.
Close active and inactive documents of multiple types.
Review paint_timing.log for DocumentHostSwitch and TabSwitch timings.
```

Runtime log path:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\paint_timing.log
```

Expected: no hangs, no missing document UI, no active-document mismatch, and flicker removed or materially reduced.
