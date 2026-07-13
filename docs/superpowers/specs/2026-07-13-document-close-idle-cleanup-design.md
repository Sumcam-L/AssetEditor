# Document Close Idle Cleanup Design

## Goal

Minimize the time from closing the active document until the replacement document is visible and responsive. Resource cleanup may finish later on the UI thread.

## Evidence

The replacement document host appears in about 30 ms, but synchronous cleanup continues to occupy the UI thread:

- `DocumentRemoved`: 31-46 ms after deferring native preview-window close.
- Native `AssetPreviewer.CloseWindow`: 70-79 ms.
- Replacement Previewer activation: 74-106 ms.
- Old `AssetEditorControl` disposal: about 100 ms.

The remaining problem is not document selection. Heavy cleanup and replacement activation execute consecutively on the same UI thread before it can process user input.

## Design

### Synchronous Close Phase

The close path continues to perform all state-sensitive work synchronously:

- Unsubscribe the old document and preview-window event handlers.
- Remove the old preview window from managed lookup state.
- Unbind the old editor from its document.
- Remove the document and editing contexts from their registries.
- Select, attach, and activate the replacement document.

No disposed document remains addressable through active registries.

### Deferred Cleanup Phase

Native and WinForms destruction is queued for UI idle processing:

- Close the old native `IPreviewWindow`.
- Dispose the old `AssetEditorControl` and its inner DockContent controls.

The replacement document's deferred activation remains ahead of cleanup. Cleanup starts only after the replacement has completed its queued activation and the message queue reaches idle.

Each idle callback executes at most one heavy cleanup item. If more items remain, the next item waits for a later idle callback so native preview closing and editor disposal do not form one long uninterrupted pause.

### Ownership And Shutdown

The queue owns deferred objects immediately after they are detached from active state. A queued item is processed exactly once.

Project changes and application shutdown drain all pending cleanup synchronously before closing remaining live preview windows. If no valid WinForms synchronization control exists, cleanup falls back to synchronous execution.

## Scope

The first implementation covers:

- Native preview-window closing in `PreviewerDocumentService`.
- `AssetEditorControl` disposal initiated by `AssetContext.Dispose`.

It does not defer document model disposal, registry removal, editing-context removal, saves, or dirty-document confirmation.

## Diagnostics And Verification

Keep timing markers for queueing, replacement activation, idle cleanup start, and idle cleanup duration. Do not add a test or repro program.

Verify by:

- Building `AssetEditor/AssetEditor.csproj` with zero errors.
- Closing a document while another same-module document remains open.
- Confirming the replacement activation completes before idle cleanup begins.
- Confirming each idle callback processes one cleanup item.
- Closing the last document and changing projects to confirm pending resources are still released.
