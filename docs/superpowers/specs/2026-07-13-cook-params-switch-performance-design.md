# Cook Params Switch Performance Design

## Goal

Reduce the time required to show `Cook Params` in every document editor that uses the shared WinForms property grid. The optimization must cover both the first page activation and repeated activation caused by inner-tab switching or switching between documents whose active inner page is already `Cook Params`.

## Current Behavior

Cook parameter data and editing controls are constructed synchronously when an editor binds its document context. Showing the page then repeats substantial work:

- `CommandControl.VisibleChanged` recursively applies the active skin to the complete control tree.
- `PropertyView.OnVisibleChanged` unconditionally rebuilds its editing context and recursively applies the active skin.
- `GridView.OnVisibleChanged` applies the skin again and reapplies all column and sorting layout state.

Cook parameter contexts commonly contain many property controls, so these visibility callbacks are significantly more expensive than those of other inner pages. The same callbacks run when an outer document becomes visible with `Cook Params` already selected.

## Scope

The change applies to the shared property-grid and command-control lifecycle, covering `Cook Params` in Asset, Entity, Behavior, FireFX, and other editors that use those controls.

The change must preserve:

- Synchronous document binding and existing editor ownership.
- Property updates, insertions, removals, reloads, filtering, and selection behavior.
- Runtime skin and theme changes.
- Column width, sorting, hidden-column, selection, and scroll state where the property structure has not changed.
- Existing DockPanel activation and document-switch behavior.

The change will not introduce asynchronous binding, lazy page construction, background UI work, or Cook Params-specific caches in individual editors.

## Design

### Property context invalidation

`PropertyView` will distinguish a current rendered editing context from one whose property structure requires rebuilding. Context assignment and existing structural change paths remain responsible for creating or invalidating the rendered property controls.

`OnVisibleChanged` will no longer unconditionally call `UpdateEditingContext()`. It will rebuild only when a context change, filter change, reload, insertion, removal, or another existing structural event has marked the rendered context stale. A page that was fully built by `Bind()` can therefore become visible without recreating its property rows.

Value-only changes continue to use the existing editing-control refresh path rather than triggering a structural rebuild.

### Skin application

Property and command controls will receive the active skin when they are created or rebound. A normal visibility transition will not recursively reapply the same skin to an unchanged control tree.

Runtime skin changes remain authoritative through `SkinService`'s existing global application path. Removing visibility-driven skin application must not prevent newly created controls from receiving the current skin or existing controls from receiving a later skin change.

### Grid layout

`GridView` will apply complete column and sorting layout after a property structure rebuild or an explicit layout-state change. A normal hidden-to-visible transition with an unchanged structure will retain the already calculated layout.

If visibility changes require a size-dependent correction, that correction must be limited to normal WinForms layout and painting; it must not rerun sorting, best-fit calculation, or per-property skinning.

### Editor integration

Individual Cook Params editors retain their current `BindCookParameters()` implementations. They continue to bind `CommandControl` and `PropertyGrid` to the new document context. This makes each document's property grid ready before its first activation and avoids editor-specific cache lifetime or invalidation rules.

Repeated event subscription encountered while implementing or testing this path may be corrected by removing the handler before adding it, but broader editor refactoring is outside this work.

## Failure Handling

Invalidation state must be cleared only after a successful rebuild. If control creation or binding throws, the next valid refresh must still be able to rebuild the grid. Existing exceptions continue to propagate; the optimization must not hide binding failures or leave event subscriptions attached to an obsolete context.

## Verification

Automated regression coverage will verify:

- Binding a property context constructs the grid before first activation.
- Repeated hide/show cycles with an unchanged context do not rebuild property rows.
- Repeated hide/show cycles do not recursively reapply the skin to the same command/property control tree.
- Assigning a different context rebuilds or refreshes controls as required.
- Filter changes, reloads, property insertions, and property removals remain visible.
- Runtime skin changes still update existing controls, and controls created afterward receive the active skin.
- Column layout and property values remain correct after hide/show and document switching.

Existing focused document-host, DockPanel tab-switch, AssetEditor inner-page, and production build regressions will also run.

Runtime timing will compare three scenarios with representative high-property-count documents:

1. First activation of `Cook Params` after opening a document.
2. Repeated switching between `Cook Params` and another inner page.
3. Switching between two documents that both have `Cook Params` selected.

Success requires removal of repeated property-structure construction and recursive visibility-driven skinning from all three paths, with a measurable reduction in UI-thread switching time and no stale values, layout regression, or runtime skin regression.
