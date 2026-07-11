# Asset Editor Batched Inner Layout Design

## Goal

Reduce AST document opening time by eliminating repeated inner `DockPanel` activation and layout work while `AssetEditorControl` creates its fixed set of editor pages.

Current timing evidence shows that file loading takes only 0-20 ms, while `AssetEditorControl` creation takes 614-754 ms. During construction, each page is shown immediately, producing an activation chain such as `Cook Params -> Geometries -> Attachments -> Animations -> Particles -> Behaviors -> Splines` and a full set of visibility and layout callbacks for every step.

## Scope

This optimization is limited to initial inner-page construction in `AssetEditorControl`.

It must not change:

- Generic document virtualization in `ControlHostService`.
- Asset Previewer activation or binding behavior.
- AST context binding semantics.
- Reload behavior.
- Theme-change reconstruction behavior unless covered by the same safe batching primitive.
- Saved inner layout behavior.
- User-driven inner tab switching.

## Design

### Batched construction

`AssetEditorControl` will create all inner controls and their `DockContent` wrappers while the inner `DockPanel` is in an initialization batch. Adding a page during this batch registers its metadata, control, icon, and events without immediately showing it.

After every fixed page has been registered, the control will materialize their default dock states as one operation:

- `Properties` uses `DockTop`.
- AST category pages use `Document` initially.

The operation will suppress intermediate layout where supported, then resume layout once. It will not suppress window redraw through `WM_SETREDRAW` or introduce delayed hiding.

### Binding and final activation

`Bind()` remains responsible for determining which category pages are valid for the AST context. Existing bind methods continue to show valid pages, hide invalid pages, and bind their data.

Only one final inner document is activated after binding. Geometry remains preferred when available. Existing fallback selection remains responsible for selecting another valid page when geometry is unavailable.

No page is lazily created: all controls still exist before binding and retain the current lifetime and disposal contracts.

### Saved layout

The optimization does not newly enable or reinterpret saved class layouts. Existing layout-state behavior remains unchanged. Batching only removes transient activation states that are not user-visible or persisted.

### Theme changes and reset

Initial implementation will optimize constructor-time page creation only. Theme reconstruction and explicit layout reset retain their current behavior to avoid broadening the lifecycle change. They may use the batching mechanism later only if runtime evidence identifies them as a meaningful hotspot.

## Failure Handling

The batch must always restore normal layout processing, including if an inner page fails to initialize. Construction exceptions continue to propagate; the optimization must not leave an otherwise usable control in a permanently suspended layout state.

## Verification

Automated regression coverage will verify:

- Inner pages are registered without being shown one by one during construction.
- Exactly one preferred document page is active after normal binding.
- Geometry is preferred when available.
- A valid fallback page is selected when geometry is unavailable.
- Existing saved-layout and active-inner-page regressions continue to pass.
- Generic document-host and preview-activation regressions continue to pass.

Runtime verification will compare fresh `paint_timing.log` data against the current baseline:

- `InitializeContext create`: 614-754 ms.
- Complete AST open: 1431-2048 ms.
- Constructor trace currently contains repeated page activation and visibility transitions.

Success requires removal of the constructor-time page activation chain and a measurable reduction in median control creation time without blank UI, preview rebound, or tab-switch regressions.
