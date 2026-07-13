# Gray UI Coverage and Document First-Frame Design

## Goals

1. Apply the existing gray AssetEditor skin to all normal editor, tool-window, dialog, input, tree, list, grid, and toolbar UI that currently falls back to Windows' white defaults.
2. Prevent a newly opened document editor from painting at its constructor size before expanding to the full document host.

## Scope

The gray theme covers all normal UI reachable during editing, including dynamically created document controls and dialogs. Content whose purpose is to display real colors remains unchanged, including color pickers, texture/image previews, render canvases, color swatches, and semantic warning/error colors.

This work does not add a new theme system, redesign controls, or reintroduce document pre-attachment behavior that previously regressed tab switching.

## Theme Architecture

The existing `SkinService` remains the single source of WinForms skin application. It already skins the main form, newly created forms, and explicitly registered control trees, but it does not automatically handle every ordinary control added dynamically after the main form is loaded.

The implementation will:

- Apply the active skin when a control tree is dynamically attached to a skinned parent.
- Avoid repeatedly skinning the same tree during ordinary layout and parent changes.
- Add or correct base skin styles for controls that do not inherit usable colors from their parent, especially `ListView`, `TreeView`, `TextBoxBase`, `DataGridView`, `ListBox`, and related editing surfaces.
- Preserve ToolStrip renderer and item color propagation through the existing `SkinService` path.
- Remove explicit `SystemColors.Window` assignments from normal Firaxis controls where they override the active skin, including collapsible property controls.
- Keep local explicit colors only when they represent content rather than chrome.

Dialogs remain covered by the existing `WinFormsUtil.WindowCreated` hook. Dynamic document and tool controls are covered when attached to an already skinned hierarchy.

## Theme Exclusions

Do not override:

- Color picker gradients, swatches, and sampled colors.
- Texture, image, render, preview, and curve canvases whose background is meaningful content.
- Syntax highlighting, selection markers, warning/error labels, and message severity colors.
- Explicit transparent controls that intentionally inherit or expose a painted parent.

The implementation will use existing skin styles and targeted corrections, not a blanket recursive assignment that destroys semantic colors.

## Document First Frame

The current generic document host adds a hidden logical editor with `DockStyle.Fill`, then sets it visible before WinForms has updated its bounds. The first visible paint therefore uses the editor's constructor size, commonly `150x150` or a designer size. Dock layout later expands it to the full host, creating the small-content flash.

`DocumentHostControl.AttachLogicalControl()` will establish the logical control's bounds while it is hidden:

1. Hide the logical control.
2. Remove it from any previous parent.
3. Set `DockStyle.Fill` and add it to the document host.
4. Set its bounds to the host `DisplayRectangle` while still hidden.
5. Make it visible and bring it to front.

If the host has no usable display size yet, attachment retains the existing layout behavior and records the condition for diagnosis. The change remains inside `DocumentHostControl`; it does not alter DockPanel activation, document switching, Previewer activation, or close cleanup.

## Lifecycle and Error Handling

- Dynamic skin application runs only when an active skin exists and the control is not disposed.
- Skin tracking continues to use weak references so dynamic document controls are not retained after close.
- Reparenting a control does not lose its original property values or duplicate tracking entries.
- A control without a matching skin style retains its current behavior; no default gray is forced over unknown custom content controls.
- Document pre-sizing uses the current host `DisplayRectangle`, respecting padding and split-pane dimensions.
- Existing document close, fast switching, and Previewer shutdown behavior must remain unchanged.

## Verification

No new test program is required. Verification uses a production build and real UI operations.

Theme verification:

- Open representative AST, material/geometry, behavior, XLP, ArtDef, and Game Art editors.
- Open common dialogs such as file-type selection, find/select, embedded collection, and browser launchers.
- Inspect text fields, tree/list views, data grids, property collections, collapsible controls, and toolbars for white Windows-default backgrounds.
- Confirm previews, color controls, canvases, syntax highlighting, and warning colors are unchanged.

First-frame verification:

- Open AST, material/geometry, behavior, and another non-entity document.
- Confirm the new editor first appears at full document-host size with no small correct-content frame.
- Confirm ordinary forward/backward tab switching remains at the current fast baseline.
- Confirm document close and application exit still work without Previewer assertions.
- Use `paint_timing.log` to confirm the first logical-control visible/paint bounds match the host display rectangle rather than `150x150` or a designer size.

## Acceptance Criteria

- No normal reachable editor/tool/dialog surface uses a default white Windows background under the gray theme.
- Intentional content colors remain intact.
- Dynamically created document controls receive the active skin without requiring per-editor calls.
- Every newly opened document editor is host-sized before its first visible paint.
- No regression in tab switching speed, document closing, project switching, or Previewer shutdown.
