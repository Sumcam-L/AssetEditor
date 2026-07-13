# Document Close Idle Cleanup Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the replacement document responsive before destroying the old native preview window and old AssetEditor UI.

**Architecture:** Add one shared WinForms idle cleanup queue in `Firaxis.ATF`. Previewer and AssetContext synchronously detach old objects from active state, then enqueue one destruction action each; the queue executes at most one action per idle cycle, waits at least 150 ms before enabling the next idle cycle, and drains synchronously at application exit or before project preview cleanup.

**Tech Stack:** C# 7.3, .NET Framework 4.6.2, WinForms, CivTech `IAssetPreviewer`.

## Global Constraints

- Do not add a test or repro program.
- Keep document registry removal, editing-context removal, event unsubscription, and `Bind(null)` synchronous.
- Execute all WinForms and native Previewer destruction on the UI thread.
- Process at most one heavy cleanup action per idle callback.
- Wait at least 150 ms before enabling another cleanup callback.
- Drain pending cleanup synchronously during application exit and project changes.

---

### Task 1: Shared UI Idle Cleanup Queue

**Files:**
- Create: `Firaxis.ATF/Firaxis.ATF/UiIdleCleanupQueue.cs`

**Interfaces:**
- Produces: `UiIdleCleanupQueue.Enqueue(string name, Action cleanup)` and `UiIdleCleanupQueue.Drain()`.
- Consumes: `System.Windows.Forms.Application.Idle` and `Application.ApplicationExit`.

- [ ] **Step 1: Add a static FIFO cleanup queue**

Implement a UI-thread-only queue. On the first enqueue, subscribe to `Application.Idle` and `Application.ApplicationExit`. `Application_Idle` must dequeue exactly one item, execute it, record its duration with `PaintTimingLog`, unsubscribe from `Idle`, and re-subscribe with `BeginInvoke` only if more work remains. This prevents WinForms from invoking the handler repeatedly during one continuous idle period.

```csharp
public static class UiIdleCleanupQueue
{
	private sealed class CleanupItem
	{
		public string Name;
		public Action Cleanup;
	}

	private static readonly Queue<CleanupItem> s_items = new Queue<CleanupItem>();
	private static bool s_idleSubscribed;

	public static void Enqueue(string name, Action cleanup)
	{
		if (cleanup == null)
			throw new ArgumentNullException(nameof(cleanup));
		s_items.Enqueue(new CleanupItem { Name = name, Cleanup = cleanup });
		SubscribeIdle();
	}

	public static void Drain()
	{
		UnsubscribeIdle();
		while (s_items.Count > 0)
			Execute(s_items.Dequeue());
	}
}
```

- [ ] **Step 2: Add idle rescheduling and shutdown draining**

After one item runs, start a one-shot `System.Windows.Forms.Timer` with `Interval = 150`. Its tick stops the timer and calls `SubscribeIdle`. `Drain()` stops the timer before synchronously emptying the queue.

Log:

```text
UiIdleCleanupQueue: queued name=..., count=...
UiIdleCleanupQueue: begin name=..., remaining=...
UiIdleCleanupQueue: end name=..., duration=...ms
```

- [ ] **Step 3: Build the shared project**

Run: `rtk dotnet build "Firaxis.ATF/Firaxis.ATF.csproj"`

Expected: `0 errors`; existing warnings are allowed.

---

### Task 2: Queue Native Preview Window Closing

**Files:**
- Modify: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs:106-150,415-440`

**Interfaces:**
- Consumes: `UiIdleCleanupQueue.Enqueue(string, Action)` and `UiIdleCleanupQueue.Drain()`.
- Produces: old `IPreviewWindow` instances removed from managed state immediately and destroyed during later idle processing.

- [ ] **Step 1: Replace direct BeginInvoke close with idle queueing**

Keep `ShutdownDocumentPreviewing` and `PreviewWindows.Remove` synchronous. Replace `ClosePreviewWindowDeferred` with:

```csharp
private void QueuePreviewWindowClose(IPreviewWindow previewWindow)
{
	UiIdleCleanupQueue.Enqueue("PreviewWindow", () => AssetPreviewer.CloseWindow(previewWindow));
}
```

- [ ] **Step 2: Drain before project-change preview cleanup**

At the beginning of `StartProjectChange`, call `UiIdleCleanupQueue.Drain()` before iterating live `PreviewWindows`. This prevents queued windows from surviving across native Previewer project teardown.

- [ ] **Step 3: Remove superseded close timing markers**

Remove `native close queued` and `deferred native close` markers. Queue-level markers now provide consistent timing for both native window and editor cleanup.

- [ ] **Step 4: Build Previewer**

Run: `rtk dotnet build "Firaxis.AssetPreviewing/Firaxis.AssetPreviewing.csproj"`

Expected: `0 errors`; existing warnings are allowed.

---

### Task 3: Queue Old AssetEditorControl Disposal

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetContext.cs:120-129`
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs:733-755`

**Interfaces:**
- Consumes: `UiIdleCleanupQueue.Enqueue(string, Action)`.
- Produces: synchronous editor unbinding followed by deferred WinForms disposal.

- [ ] **Step 1: Detach GUI state synchronously**

Capture the old GUI, set `base.GUI = null`, and call `Bind(null)` before queueing. This ensures disposed document data is not reachable from the still-live control.

```csharp
EntityEditorControlBase gui = base.GUI;
base.GUI = null;
if (gui != null)
{
	gui.Bind(null);
	UiIdleCleanupQueue.Enqueue("AssetEditorControl", gui.Dispose);
}
```

- [ ] **Step 2: Keep disposal idempotent**

Retain the existing `AssetEditorControl.Dispose(bool)` guard and event unsubscriptions. Keep the active-inner-content-last ordering already present in the worktree.

- [ ] **Step 3: Build AssetEditing**

Run: `rtk dotnet build "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj"`

Expected: `0 errors`; existing warnings are allowed.

---

### Task 4: Final Build And Runtime Evidence

**Files:**
- Verify only; no new files.

**Interfaces:**
- Consumes: timing markers emitted by `UiIdleCleanupQueue` and existing document-switch diagnostics.
- Produces: build and runtime evidence that activation precedes cleanup.

- [ ] **Step 1: Build the final application**

Run: `rtk dotnet build "AssetEditor/AssetEditor.csproj"`

Expected: `0 errors`; existing warnings are allowed.

- [ ] **Step 2: Check the diff**

Run: `rtk git diff --check` and `rtk git diff --stat`.

Expected: no whitespace errors and no new test or repro files.

- [ ] **Step 3: Exercise close with a replacement document**

Close the active document while another same-module document remains open.

Expected log ordering:

```text
ActiveDocumentChanged total: ...
Previewer: get=..., activate=...
NativeSwitchTrace end ...
UiIdleCleanupQueue: begin name=PreviewWindow ...
UiIdleCleanupQueue: end name=PreviewWindow ...
UiIdleCleanupQueue: begin name=AssetEditorControl ...
```

Each idle pass must contain only one `begin`/`end` pair.

- [ ] **Step 4: Exercise last-document and project-change cleanup**

Close the last document and perform one project change.

Expected: all queued items receive matching `end` markers, no exceptions are logged, and Previewer project cleanup completes.
