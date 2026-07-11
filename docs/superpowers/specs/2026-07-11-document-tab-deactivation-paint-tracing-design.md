# Document Tab Deactivation Paint Tracing Design

## Problem

When switching between any document types, the newly active tab highlights immediately, but the previously active tab retains its complete active appearance for approximately 50-150 ms before returning to its unselected appearance. The document content itself switches quickly.

`DockPane.ActiveContent` updates before `TabStripControl.RefreshChanges()` invalidates the strip, so a complete strip paint should render both states together. The observed double highlight indicates either that the first paint excludes the old tab rectangle or that an invalidated old-tab region is not painted until a later message.

## Goal

Collect enough native and managed paint evidence to distinguish invalid-region selection from message-processing delay without changing document switching behavior.

The eventual fix must make the old tab lose its active appearance by the next rendered frame while preserving current document-content responsiveness.

## Scope

Add gated diagnostics to `FiraxisDockPaneStrip` for document tabs only. Reuse the active `DocumentSwitchTrace` generation so normal application operation performs no high-volume logging outside an active switch trace.

Do not alter document activation, docking layout, tab ordering, visibility, or paint scheduling in this diagnostic change.

## Trace Data

For each traced tab switch, record:

- The previous and current `DockPane.ActiveContent` identities.
- Entry to `OnRefreshChanges`, including the strip client rectangle and old/new tab rectangles when available.
- Selected native paint messages, especially `WM_PAINT` and `WM_ERASEBKGND`, using the existing trace generation.
- Entry to `OnPaint`, including `PaintEventArgs.ClipRectangle`, strip bounds, current active content, and old/new tab rectangles.
- Completion of `OnPaint`, so delays inside painting can be separated from delays before paint dispatch.

Content identities should use stable form name and tab text fields already available through `DockHandler`. Missing or disposed content must be logged safely as `null` or unavailable rather than throwing.

## State

`FiraxisDockPaneStrip` may retain only the previous observed active content needed to describe one switch. This state is diagnostic and must not control rendering. It is updated when an active-content change is observed and cleared or replaced on the next traced switch.

No global timer, delayed callback, redraw freeze, or snapshot overlay is introduced.

## Evidence Interpretation

After one reproduced switch:

- If the first `OnPaint` clip intersects the new tab but not the old tab, the root cause is an incomplete invalid region. The implementation should invalidate the union of the old and new tab rectangles.
- If the first paint covers both rectangles but the old tab is still rendered active, investigate when the active-content state changes relative to painting.
- If both rectangles are invalidated promptly but `WM_PAINT` or `OnPaint` is dispatched 50-150 ms later, test a narrowly scoped immediate redraw of only the old/new tab union.
- If `OnPaint` itself takes 50-150 ms, profile `CalculateTabs`, `EnsureDocumentTabVisible`, and tab drawing before changing scheduling.

Only one hypothesis should be tested at a time after collecting evidence.

## Verification

Add a focused repro that verifies:

- Tab-strip diagnostics emit nothing without an active `DocumentSwitchTrace` generation.
- A traced active-content change includes old/new identities and tab rectangles.
- Paint records include clip rectangles and begin/end markers.
- Diagnostic state does not change tab activation or drawing decisions.

Run the existing native document-switch trace repro and document virtualization repro to guard integration behavior, then build `AssetEditor/AssetEditor.csproj`.

Manual evidence collection uses two already-open documents of any type. Switch in both directions and compare timestamps for active-content change, invalidation, native paint dispatch, and managed painting.

## Non-Goals

- Synchronously refreshing the entire tab strip as a speculative fix.
- Changing document virtualization or activation order.
- Optimizing document editor controls.
- Redesigning tab visuals.
- Reintroducing `WM_SETREDRAW`, global redraw freezes, retained hidden controls, or snapshot overlays.
