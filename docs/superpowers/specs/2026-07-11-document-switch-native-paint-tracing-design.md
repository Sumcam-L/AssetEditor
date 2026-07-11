# Document Switch Native Paint Tracing Design

## Goal

Identify which native WinForms/DockPanel window erases or paints white during intermittent tab-only document switches after both target documents have already created handles.

## Evidence

The captured runtime sequence rules out first-handle creation as the sole cause. During a reproduced switch between previously displayed AST documents:

- The target outer `DockContent` became visible first.
- The target `AssetEditorControl` became visible approximately 47 ms later.
- Logical control attach then completed in 12 ms.
- Existing managed `OnPaintBackground` logging did not record a white-producing paint during the gap.

This points to a native window message or a DockPanel-owned surface not covered by current managed paint logs.

## Scope

Add temporary, low-overhead diagnostics only. Do not alter:

- Document attach or detach ordering.
- DockPanel activation behavior.
- Visibility behavior.
- Background colors.
- Redraw or erase handling.
- Preview activation.

## Trace Window

`ControlHostService` will open a short trace window whenever the active outer document changes. The trace window will cover the immediate native visibility and paint sequence, then close automatically after deferred activation and paint flushing settle.

Outside an active trace window, diagnostic calls return immediately and do not format log strings.

## Traced Surfaces

Record messages for these surfaces:

- Outer document `ControlHostDockContent`.
- Inner `DocumentHostControl`.
- Logical document control, including `AssetEditorControl`, through a lightweight `NativeWindow` observer installed by `DocumentHostControl` while the logical control has a handle.
- Active document `DockPane` when it is a control with an available handle, using the same observer mechanism.

The observer must not subclass, recreate, or own the target handle. It only attaches with `AssignHandle` and releases it when the target handle is destroyed or the observer is disposed.

## Messages

Record only:

- `WM_ERASEBKGND` (`0x0014`).
- `WM_PAINT` (`0x000F`).
- `WM_SHOWWINDOW` (`0x0018`).
- `WM_WINDOWPOSCHANGING` (`0x0046`).
- `WM_WINDOWPOSCHANGED` (`0x0047`).

Each entry includes:

- Surface role and managed type.
- Message name.
- HWND and parent HWND.
- `Visible`, `IsHandleCreated`, and `BackColor`.
- Managed bounds and client rectangle.
- For window-position messages, decoded `WINDOWPOS` coordinates, size, and flags when available.
- Current outer document identity and whether the surface belongs to the active document.

Logging occurs before and after base message processing for paint/erase/show messages so the trace can identify which call changed state.

## Lifecycle

- Outer and inner host tracing is built into their existing lifecycle.
- Logical-control observers attach on `HandleCreated`, detach on `HandleDestroyed`, and are disposed with `DocumentHostControl`.
- Pane observers are reused per pane and disposed when `ControlHostService` is disposed.
- Diagnostics must never keep a document, pane, or HWND alive.
- Any inability to decode a message is logged as missing detail and must not affect message processing.

## Verification

Automated repros will verify:

- No native trace entries are emitted outside a trace window.
- The selected message set is emitted inside a trace window.
- Observer attachment does not create a target handle.
- Handle recreation detaches and reattaches without stale HWND ownership.
- Disposing a document host releases its observer.
- Existing virtualization, tab-switch, paint, and AST inner-layout repros remain green.

Runtime verification requires another tab-only reproduction. The resulting trace must identify the first full-document paint or erase during the visible-host/real-control gap and its surface role.
