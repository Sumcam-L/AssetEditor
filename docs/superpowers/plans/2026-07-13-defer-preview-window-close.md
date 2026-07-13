# Deferred Preview Window Close Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Remove native preview-window destruction from the synchronous document-close path.

**Architecture:** `DocumentRegistry_DocumentRemoved` will synchronously detach managed event handlers and remove the window from `PreviewWindows`, then enqueue only `IAssetPreviewer.CloseWindow`. The callback runs on the existing Previewer WinForms control after the current close and document activation messages complete.

**Tech Stack:** C# 7.3, .NET Framework 4.6.2, WinForms, CivTech `IAssetPreviewer`.

## Global Constraints

- Do not add a test or repro program.
- Do not defer managed event detachment or dictionary removal.
- Keep project-change cleanup synchronous.
- Do not change behavior for opening or activating preview windows.

---

### Task 1: Defer Native Preview Window Destruction

**Files:**
- Modify: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/PreviewerDocumentService.cs:106-130`

**Interfaces:**
- Consumes: `PreviewerControlHost.PreviewerControl.BeginInvoke(Delegate)` and `AssetPreviewer.CloseWindow(IPreviewWindow)`.
- Produces: a private `ClosePreviewWindowDeferred(IPreviewWindow previewWindow)` helper.

- [ ] **Step 1: Remove the window from managed state before queuing native destruction**

In `DocumentRegistry_DocumentRemoved`, retain `ShutdownDocumentPreviewing(...)`, then remove the document from `PreviewWindows` before scheduling the native close. This guarantees later document activation cannot retrieve the old window.

- [ ] **Step 2: Add the minimal deferred-close helper**

```csharp
private void ClosePreviewWindowDeferred(IPreviewWindow previewWindow)
{
	Control previewerControl = PreviewerControlHost.PreviewerControl;
	if (previewerControl != null && previewerControl.IsHandleCreated && !previewerControl.IsDisposed)
	{
		previewerControl.BeginInvoke((Action)(() => AssetPreviewer.CloseWindow(previewWindow)));
	}
	else
	{
		AssetPreviewer.CloseWindow(previewWindow);
	}
}
```

The synchronous fallback prevents leaking the native window if the UI handle is unavailable during shutdown.

- [ ] **Step 3: Preserve timing evidence**

Log queue time separately from callback execution time so `paint_timing.log` proves the synchronous `DocumentRemoved` subscriber no longer includes the previous `108-140ms` native close.

Expected markers:

```text
Previewer: native close queued
Previewer: deferred native close=...ms
```

- [ ] **Step 4: Build the final application**

Run: `rtk dotnet build "AssetEditor/AssetEditor.csproj"`

Expected: build succeeds with `0 errors`; existing warnings are allowed.

- [ ] **Step 5: Verify with an actual document close**

Close one document while another same-module document remains open, then inspect `paint_timing.log`.

Expected:

```text
DocumentRemoved subscriber 'PreviewerDocumentService'
```

must no longer include the native `CloseWindow` duration, while:

```text
Previewer: deferred native close=...ms
```

appears after the next document has been selected and displayed.

- [ ] **Step 6: Review the final diff**

Run: `rtk git diff --check` and `rtk git diff --stat`.

Expected: no whitespace errors and no new test/repro files.
