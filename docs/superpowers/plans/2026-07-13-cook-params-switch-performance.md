# Cook Params Switch Performance Implementation Plan

**Goal:** Make first and repeated Cook Params activation fast by prebuilding its property rows during binding and eliminating unchanged visibility-driven rebuild, skin, and full-layout work.

**Architecture:** Add an opt-in eager hidden-build setting to the shared WinForms property grid and enable it for every Cook Params grid. Keep the normal lazy behavior for unrelated hidden grids. Make visibility transitions reuse the rendered context, while context/filter/observable events retain the existing explicit refresh and rebuild paths. Apply `CommandControl` skin during binding and use visibility only as a fallback for controls that have never been skinned.

## Task 1: Add focused lifecycle regression coverage

- Add a small WinForms repro project under `docs/superpowers/repros/property-view-visibility`.
- Bind a test context while the grid is hidden with eager hidden-build enabled.
- Assert that property controls exist before first display and that hide/show does not rebuild them.
- Assert that assigning a structurally different context still rebuilds the property controls.
- Add source-level guards for the command and grid visibility handlers so recursive skin and full-layout calls cannot silently return.
- Run the repro before implementation and confirm that it fails.

## Task 2: Add opt-in hidden property construction

- Expose `BuildPropertiesWhenHidden` from `PropertyView` through `PropertyGrid`.
- Let `UpdateEditingContext()` build properties when the view is visible or that option is enabled.
- Keep the option disabled by default to preserve unrelated property-grid startup behavior.
- Enable the option on every Cook Params PropertyGrid construction site.

## Task 3: Remove repeated visibility work

- Remove unconditional `UpdateEditingContext()` and recursive skin application from `PropertyView.OnVisibleChanged`.
- Remove recursive skin application and `ApplyAllLayoutState()` from `GridView.OnVisibleChanged`.
- Track whether `CommandControl` has received its initial skin; apply it during `Bind()` after command controls are refreshed, and retain a one-time visibility fallback for unbound uses.
- Preserve global runtime skin changes through the existing `SkinService` registry and broadcast path.

## Task 4: Harden Cook Params event binding

- Change each repeated Cook Params `SelectedPropertyChanged` subscription to unsubscribe before subscribe where applicable.
- Do not alter context ownership, command registration, or editor-specific binding behavior.

## Task 5: Verify

- Run the new property visibility repro.
- Run existing AssetEditor inner-page, DockPane tab-switch, document virtualization, and container-paint repros.
- Build `Atf.Gui.WinForms`, `Firaxis.ATF`, `Firaxis.AssetEditing`, and the production `AssetEditor` project in dependency order.
- Inspect the final diff and confirm unrelated existing worktree changes were not overwritten.
