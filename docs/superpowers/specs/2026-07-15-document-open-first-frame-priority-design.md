# Document Open First-Frame Priority Design

## Problem

Opening a document leaves the complete previous application frame visible for a short period. The main window appears frozen and the new document arrives only after synchronous editor construction, binding, registration, activation, and preview setup finish.

Runtime evidence for an AST open shows:

- `AssetEditorControl` construction took 428 ms.
- Editor binding took 251 ms.
- `RegisterControl` add/show took 203 ms.
- The outer document tab painted before document activation finished.
- The synchronous activation path then occupied the UI thread for about 249 ms.
- Deferred Previewer activation added about 135 ms afterward.

The previous pixels remain visible because the UI thread cannot dispatch the new document's first paint while these synchronous operations run. This is not primarily a file I/O problem or an incorrect background paint.

## Goals

- Cover every document type opened through the common document service, including AST, ArtDef, XLP, Game Art Specification, FireFX, and other entity editors.
- Display the real new document as soon as its editor is ready, without a loading label, placeholder, blank document surface, or intermediate small frame.
- Prioritize the new document's first complete frame over Previewer setup and other work that is not required to render or interact with the document.
- Reduce actual open time where current diagnostics identify avoidable repeated layout, activation, or paint work.
- Preserve the current document virtualization, tab-switch responsiveness, close cleanup, property editing, and Previewer correctness.

## Non-Goals

- Do not move WinForms control construction or native editor objects to a worker thread.
- Do not redesign `IDocumentClient.Open` as an asynchronous API in this change.
- Do not display a loading overlay, progress message, skeleton, or temporary tab.
- Do not call `Application.DoEvents()` to force responsiveness.
- Do not use `WM_SETREDRAW`, whole-window freezes, snapshot overlays, retained inactive document windows, delayed-hide timers, or hidden document pre-positioning.
- Do not change DockPanel document visibility semantics globally.

## Selected Approach

Keep document parsing, control construction, and binding synchronous for thread safety, but make the handoff from the old document to the completed new editor a first-frame-priority boundary.

The sequence becomes:

1. Keep the old document fully visible while the new document is loaded, constructed, and bound.
2. Register the completed logical editor in its lightweight `DocumentHostControl` without exposing an incomplete editor.
3. Activate and lay out the new document host.
4. Produce one bounded first frame for the new document surface.
5. Only after that frame, run Previewer creation/binding and other activation work that is not required for the main document UI.
6. Continue ordinary deferred callbacks with generation and disposal checks so stale work cannot overwrite a newer document.

This does not make the expensive construction phase asynchronous, but it removes avoidable work from the interval between the new tab becoming active and the real editor becoming visible. It also prevents the application from painting an active new tab while still retaining old document pixels for synchronous Previewer work.

## Common Document Handoff

`ControlHostService` remains responsible for common document virtualization. Registration-before-show remains supported because it protects property editing and first-frame sizing.

The service must distinguish three states that are currently conflated:

- The logical control is attached to its `DocumentHostControl`.
- The document host is active and has final visible bounds.
- The logical control has produced its first visible frame for this activation.

`AttachLogicalControl()` returning `false` only means the control was already attached. It must not imply that first-frame work is unnecessary. A newly registered document can be attached while hidden, then become active later without ever entering the existing `attachedLogicalControl == true` paint path.

For a newly opened document, `ControlHostService` will request a first frame when the active host is visible and correctly sized, regardless of whether attachment happened during registration or activation. The request is consumed once for that activation so ordinary focus changes and duplicate visible-host scans do not repeat it.

## First-Frame Paint Boundary

The first-frame operation must be narrower than the existing recursive `FlushDocumentPaint()` implementation, which has measured costs from 91 ms to 439 ms.

The implementation will:

- Ensure the outer `DockContent`, `DocumentHostControl`, and logical editor have final document bounds.
- Invalidate the active document surface once.
- Dispatch only the paint required for the active document's first complete frame.
- Avoid recursively calling `Update()` on every visible descendant unless a focused repro proves a specific child requires it.
- Record the begin/end time and whether the first-frame request came from registration, active-content change, or visible-host recovery.

The exact minimal paint call will be selected by a failing WinForms repro. The implementation must not assume that attachment itself guarantees a paint.

## Activation Ordering

`BaseEntityEditor.IControlHostClient.Activate()` currently updates `DocumentRegistry.ActiveDocument`, which can synchronously add the document and invoke Previewer initialization before the main document receives its first frame.

The ordering will be separated into:

- Required document activation: active document identity, active editing context, command routing, and state needed by the visible editor.
- Post-first-frame activation: Previewer window creation, native asset loading/binding, preview knobs, animation selection, and widgets when they are not needed to render the main editor.

The common host will expose a narrow post-first-frame scheduling point rather than adding editor-specific knowledge. Previewer services will use that point and retain their existing generation checks. If the active document changes, closes, or the target control is disposed before deferred work runs, the callback exits without side effects.

Other `DocumentAdded` and `ActiveDocumentChanged` subscribers remain synchronous unless timing logs show they materially delay the first frame and their behavior is safe to defer. Changes will be evidence-driven and made one subscriber at a time.

## Open-Time Reduction

First-frame ordering improves perceived response, but the measured construction and binding costs remain real. Existing timing markers will be used to reduce only demonstrated common hotspots.

The first pass will compare all representative document types and inspect:

- Editor control construction.
- Initial DockPanel content creation and repeated layout.
- Property control construction.
- Binding of tabs that are not initially visible.
- Repeated activation or layout of the same inner content.
- Registration and show work.
- Synchronous document-registry subscribers.

Safe optimizations may batch repeated layout, defer construction of initially hidden optional content, or reuse already-created state. They must not change persisted layout behavior, editing availability, or the initial active inner tab without a focused test.

## Error Handling

- If `IDocumentClient.Open` throws, the old document remains active and visible; no partially registered host remains.
- A first-frame callback rechecks active host identity, visibility, handle creation, bounds, and disposal state.
- A post-first-frame callback rechecks document generation and registry membership.
- Closing a document before deferred work executes cancels that work by identity/generation checks.
- Previewer failure is logged and must not prevent the main document from remaining visible and usable.
- Duplicate activation and visible-host recovery callbacks may request the same frame, but only the current activation generation consumes it.

## Testing

### Automated Repro

Extend the common document virtualization repro with a visible-form scenario:

1. Register and paint a first colored document.
2. Register a second colored document while the form is already visible.
3. Simulate a slow activation subscriber without pumping messages.
4. Record outer-tab activation, logical-control visibility, first logical paint, activation begin/end, and post-first-frame work.
5. Assert the second document's first frame occurs before simulated nonessential activation work.
6. Assert a logical control attached before show still receives one first-frame request after its host becomes active.
7. Assert duplicate visible-host scans do not repeat first-frame work.
8. Assert rapid open/open and open/close sequences discard stale callbacks.

Add a focused paint-boundary repro if the existing virtualization repro cannot observe paint dispatch without introducing `Application.DoEvents()` into production behavior.

### Existing Regressions

Keep these passing:

- `control-host-document-virtualization`
- `dockpane-tab-switch`
- `control-host-container-paint`
- `firaxis-dockpane-content-paint`
- `control-host-before-show`
- `asset-editor-inner-active`
- `document-switch-native-trace`
- `document-tab-paint-trace`

### Manual Verification

Open AST, ArtDef, XLP, Game Art Specification, and another entity document from both Assets and ordinary file-open paths. For each:

- The old document may remain visible during unavoidable synchronous construction.
- No loading UI or blank intermediate document appears.
- Once the new tab becomes active, the real new editor appears as the next complete document frame.
- Previewer updates after the main document without temporarily blanking the main editor.
- Rapidly opening two documents leaves the latest document and preview active.
- First open, additional open, startup restore, active close, inactive close, and application exit remain stable.

## Diagnostics

Retain and align these timing markers:

- `OpenFromAssets`
- `InitializeContext`
- editor construction and bind timing
- `RegisterControl`
- `DocumentHostAttach`
- first-frame request/begin/end
- `ActiveDocumentChanged` subscriber timing
- Previewer post-first-frame scheduling and execution
- first logical `WM_SHOWWINDOW` and `WM_PAINT`

Success is measured from open-command entry to the first complete real-document frame, not only to `IDocumentClient.Open` return.

## Acceptance Criteria

- All normal document types use the same first-frame-priority handoff.
- No loading indicator, placeholder, blank frame, or constructor-sized frame is shown.
- A document attached before host show still receives exactly one first-frame opportunity after activation.
- Nonessential Previewer work does not run before the main document's first complete frame.
- The measured interval from active new tab to first real-document frame is reduced to one paint cycle without a synchronous Previewer-sized stall.
- Existing document switching, property editing, saved layout, close cleanup, and Previewer behavior do not regress.
- Focused repros and the main AssetEditor build pass.
