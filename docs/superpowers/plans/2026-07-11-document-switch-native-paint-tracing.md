# Document Switch Native Paint Tracing Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Capture the exact native window that erases or paints the document area during an intermittent tab-only white flash without changing document-switch behavior.

**Architecture:** Add a process-local `DocumentSwitchTrace` gate and a reusable `NativeWindow` observer in `Atf.Gui.WinForms`. `ControlHostService` opens a short trace generation on outer document changes; outer hosts, inner hosts, logical controls, and the active pane report only the selected native messages while that generation is active.

**Tech Stack:** C# 10, .NET Framework 4.6.2 WinForms, WeifenLuo DockPanel Suite, `NativeWindow`, buffered `PaintTimingLog`, console repros.

## Global Constraints

- Diagnostics only: do not alter attach, detach, activation, visibility, background color, redraw, erase, or preview behavior.
- Trace only `WM_ERASEBKGND`, `WM_PAINT`, `WM_SHOWWINDOW`, `WM_WINDOWPOSCHANGING`, and `WM_WINDOWPOSCHANGED`.
- Outside an active trace window, return before formatting diagnostic strings.
- Observers must not create, recreate, own, or keep target handles alive.
- Handle creation, destruction, and recreation must not leave stale observer HWNDs.
- Message decode and logging failures must never affect target message processing.
- The trace window closes automatically after deferred activation and paint flushing settle.
- Existing `PaintTimingLog` buffering remains unchanged.

---

## File Structure

- Create `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentSwitchTrace.cs`: trace generation/gate, message formatting, `WINDOWPOS` decoding, and reusable non-owning observer.
- Modify `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`: inner-host WndProc tracing and logical-control observer lifecycle.
- Modify `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`: outer-host tracing, active trace-window lifecycle, pane observer reuse/disposal.
- Create `docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj`: focused diagnostic lifecycle repro.
- Create `docs/superpowers/repros/document-switch-native-trace/Program.cs`: gate, selected-message, no-handle-creation, recreation, and disposal assertions.

### Task 1: Add A Failing Trace Contract Repro

**Files:**
- Create: `docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj`
- Create: `docs/superpowers/repros/document-switch-native-trace/Program.cs`

**Interfaces:**
- Consumes: `PaintTimingLog.Clear()`, `PaintTimingLog.Flush()`, WinForms controls and handles.
- Produces: compile-time requirements for `DocumentSwitchTrace.Begin(string)`, `DocumentSwitchTrace.End(long)`, `DocumentSwitchTrace.IsActive`, and `DocumentSwitchTrace.Observe(Control, string, Func<string>, Func<bool>)` returning `IDisposable`.

- [ ] **Step 1: Create the repro project**

Use `net462`, `WinExe`/WinForms-compatible settings matching existing repros, and project references to `Atf.Gui` and `Atf.Gui.WinForms`.

- [ ] **Step 2: Write the failing trace contract**

The repro must:

```csharp
PaintTimingLog.Clear();
using (var form = new Form())
using (var panel = new Panel())
{
    form.Controls.Add(panel);
    if (panel.IsHandleCreated)
        return Fail("observer setup precondition created the target handle");

    IDisposable observer = DocumentSwitchTrace.Observe(
        panel,
        "logical",
        () => "doc=Test",
        () => true);

    if (panel.IsHandleCreated)
        return Fail("observer creation created the target handle");

    form.Show();
    Application.DoEvents();
    PaintTimingLog.Flush();
    AssertNoNativeTraceEntries();

    long generation = DocumentSwitchTrace.Begin("repro");
    panel.Hide();
    panel.Show();
    panel.Invalidate();
    panel.Update();
    Application.DoEvents();
    DocumentSwitchTrace.End(generation);
    PaintTimingLog.Flush();
    AssertSelectedMessagesOnly();
    AssertTraceContainsRoleAndHandles("logical");

    IntPtr firstHandle = panel.Handle;
    panel.RecreateHandleForTest();
    Application.DoEvents();
    if (panel.Handle == firstHandle)
        return Fail("target handle was not recreated");

    generation = DocumentSwitchTrace.Begin("recreated");
    panel.Hide();
    panel.Show();
    Application.DoEvents();
    DocumentSwitchTrace.End(generation);
    AssertTraceUsesCurrentHandle(panel.Handle);

    observer.Dispose();
    generation = DocumentSwitchTrace.Begin("disposed");
    panel.Hide();
    panel.Show();
    Application.DoEvents();
    DocumentSwitchTrace.End(generation);
    AssertNoRoleEntriesAfterMarker("logical", "disposed");
}
```

Expose handle recreation only from a private test subclass:

```csharp
private sealed class RecreatingPanel : Panel
{
    public void RecreateHandleForTest() => RecreateHandle();
}
```

- [ ] **Step 3: Run the repro and verify red state**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj"
```

Expected: compile failure because `DocumentSwitchTrace` does not exist.

### Task 2: Implement The Trace Gate And Native Observer

**Files:**
- Create: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentSwitchTrace.cs`
- Modify: `Atf.Gui.WinForms/Atf.Gui.WinForms.csproj` only if the project does not use SDK default compile inclusion.

**Interfaces:**
- Produces:
  - `internal static long Begin(string reason)`
  - `internal static void End(long generation)`
  - `internal static bool IsActive { get; }`
  - `internal static void Trace(Control control, string role, string phase, ref Message message, Func<string> context, Func<bool> isActiveSurface)`
  - `internal static IDisposable Observe(Control control, string role, Func<string> context, Func<bool> isActiveSurface)`

- [ ] **Step 1: Implement generation-safe trace gating**

Use `Interlocked.Increment` for generations and `Volatile.Read/Write` for active generation. `End(generation)` clears only the same generation so a stale deferred callback cannot close a newer switch trace.

```csharp
private static long s_nextGeneration;
private static long s_activeGeneration;

internal static long Begin(string reason)
{
    long generation = Interlocked.Increment(ref s_nextGeneration);
    Volatile.Write(ref s_activeGeneration, generation);
    PaintTimingLog.Write("NativeSwitchTrace begin generation={0}, reason={1}", generation, reason);
    return generation;
}

internal static void End(long generation)
{
    if (Interlocked.CompareExchange(ref s_activeGeneration, 0, generation) == generation)
        PaintTimingLog.Write("NativeSwitchTrace end generation={0}", generation);
}
```

- [ ] **Step 2: Filter before formatting**

`Trace` must immediately return when no generation is active or the message is outside the five-message set. Only then evaluate context delegates, parent HWND, colors, bounds, and optional `WINDOWPOS`.

- [ ] **Step 3: Decode `WINDOWPOS` defensively**

Define a private sequential struct with `IntPtr hwnd`, `IntPtr hwndInsertAfter`, `int x`, `int y`, `int cx`, `int cy`, and `uint flags`. Decode only when `LParam != IntPtr.Zero`; catch marshal errors and emit `windowPos=unavailable`.

- [ ] **Step 4: Implement non-owning observer lifecycle**

The observer subscribes to target `HandleCreated`, `HandleDestroyed`, and `Disposed`. It calls `AssignHandle` only when the target already has a handle, calls `ReleaseHandle` on destruction/disposal, and unsubscribes all events in `Dispose`. `WndProc` logs before and after base processing for paint/erase/show messages and before only for window-position messages.

- [ ] **Step 5: Run the focused repro**

Run the Task 1 command. Expected: PASS for gate, filtering, no implicit handle creation, recreated handle, and disposal.

### Task 3: Instrument Document Surfaces And Trace Windows

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`

**Interfaces:**
- Consumes: `DocumentSwitchTrace` from Task 2.
- Produces: outer-host, inner-host, logical-control, and active-pane native traces scoped to each outer document switch.

- [ ] **Step 1: Trace inner host messages and logical control handles**

In `DocumentHostControl`, create one observer for `LogicalControl` without forcing its handle. Override `WndProc` to trace role `inner-host` before and after selected messages. Dispose the observer from `Dispose(bool)` before calling base disposal.

- [ ] **Step 2: Give outer hosts a context provider**

Extend `ControlHostDockContent` with delegates supplied by `ControlHostService` for current document identity and active-surface status. In its existing `WndProc`, call `DocumentSwitchTrace.Trace` as role `outer-host` before and after the selected messages while preserving all existing timing logs and base call order.

- [ ] **Step 3: Open a generation on active document change**

At the start of `dockPanel_ActiveContentChanged`, after confirming the content actually changed and is a document, call `DocumentSwitchTrace.Begin` with old/new host identities. Store the returned generation in `m_documentSwitchTraceGeneration`.

- [ ] **Step 4: Observe the active pane without creating its handle**

Maintain a dictionary keyed by `DockPane` containing observer disposables. Create/reuse an observer only when the pane already has a handle. The observer role is `dock-pane`; its active predicate compares the pane to the current active document pane.

- [ ] **Step 5: Close only the matching trace generation**

In `ActivateClientIfStillActive`, after attach, flush, and activation complete, post one `BeginInvoke` callback that calls `DocumentSwitchTrace.End(capturedGeneration)`. If activation is superseded, the generation check prevents a stale callback from ending the newer trace. Also end the active generation during service disposal.

- [ ] **Step 6: Dispose pane observers**

In `ControlHostService.Dispose(bool disposing)`, dispose every pane observer and clear the dictionary without disposing panes or controls.

- [ ] **Step 7: Run focused diagnostics and virtualization repros**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/document-switch-native-trace/DocumentSwitchNativeTraceRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
```

Expected: every repro prints PASS and exits 0.

### Task 4: Build, Deploy, And Capture The White Flash

**Files:**
- Verify only; runtime log is external.

**Interfaces:**
- Consumes: completed diagnostic implementation.
- Produces: native message evidence identifying the first erase/full-area paint in a reproduced white flash.

- [ ] **Step 1: Build production**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: 0 errors. Stop only a stale deployed `AssetEditor.exe` if deployment files are locked, then rerun.

- [ ] **Step 2: Reproduce with tab clicks only**

Open at least two AST documents, display both at least once, then click adjacent tabs in both directions until the white flash appears. Stop immediately after reproduction.

- [ ] **Step 3: Analyze the final trace generation**

Read the final `NativeSwitchTrace begin` to matching `end` block from:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\paint_timing.log
```

Identify:

- First surface that becomes visible at full document bounds.
- First `WM_ERASEBKGND` and its `BackColor`.
- First full-area `WM_PAINT` before logical-control visibility.
- Parent/child HWND transition ordering.
- Whether DockPane, outer host, inner host, or logical control owns the white-producing operation.

Do not implement a flicker fix until this trace supports one specific root-cause hypothesis.
