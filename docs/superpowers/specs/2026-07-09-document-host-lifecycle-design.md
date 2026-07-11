# Document Host Lifecycle Design

## Problem

Main document tab switching can flicker for more than `.ast` AssetEditor documents. The issue belongs to the common WinForms document hosting path, not only `AssetEditorControl`.

Previous fixes improved correctness and removed some overpainting, but did not remove the remaining switch flicker. Global DockPane visibility changes are not acceptable because they made AssetEditor and AssetPreviewer slower and could hang the UI.

## Goals

- Address document-tab flicker for all normal document clients, not only `.ast` files.
- Keep the current outer document tab UX.
- Keep inactive heavyweight document controls out of the visible WinForms window tree.
- Avoid global DockPane document visibility semantic changes.
- Avoid changing non-document tool windows, previewer panels, property grids, output windows, and dock windows.
- Preserve each document client's existing activate, deactivate, close, save, dirty, and read-only behavior.
- Keep the first implementation reversible and guarded by focused repros.

## Non-Goals

- Do not rework every editor's UI internals.
- Do not change document registration for controls where `ControlInfo.IsDocument` is not `true`.
- Do not use `WM_SETREDRAW` or whole-window redraw freezes.
- Do not retain inactive DockPane document forms globally.
- Do not pre-position hidden inactive documents into the visible DockPane content rectangle.
- Do not reintroduce snapshot overlays or the failed AssetEditor shell experiment.

## Selected Approach

Introduce a generic document-host virtualizer inside `ControlHostService` for registered controls whose `ControlInfo.IsDocument == true`.

Instead of placing the real document control directly inside the DockPane `DockContent`, `ControlHostService` places a lightweight `DocumentHostControl` inside the `DockContent`. The real document control remains registered as the logical control in `ControlInfo.Control`, but it is attached only to the active lightweight host.

This keeps DockPane switching focused on cheap host controls while the real document controls are attached and detached by a controlled lifecycle. Inactive real document controls are detached from the visible window tree, avoiding the retained-visible model that caused responsiveness problems.

## Components

### DocumentHostControl

A lightweight WinForms control created by `ControlHostService` for each registered document.

Responsibilities:

- Fill the outer `DockContent` document area.
- Hold a reference to the logical real document control.
- Keep `Tag` aligned with the real document control so existing document clients can identify the active document.
- Provide an attach surface for the active real control.
- Notify `ControlHostService` when it becomes visible or disposed.

It must not own save, close, document state, dirty state, layout state, or editor-specific state.

### Logical Control Mapping

`ControlHostService` must distinguish between:

- the logical control: the real document control registered by the document client;
- the hosted control: the lightweight `DocumentHostControl` placed inside `DockContent`.

Existing APIs that receive a real control should continue to work:

- `FindControlInfo(realControl)` returns the same `ControlInfo`.
- `Show(realControl)` shows the corresponding lightweight host.
- `Hide(realControl)` hides the corresponding lightweight host.
- `UnregisterControl(realControl)` unregisters the corresponding lightweight host and detaches the real control.
- command routing and context menu lookup still resolve to the logical document `ControlInfo`.

`ControlInfo.HostControl` should remain the outer DockContent host, as it is today, so tab group and active group calculations remain stable.

### Active Document Attachment

`ControlHostService` maintains at most one attached real document control per `DockPane` document tab group. This preserves tiled/split document layouts, where more than one document pane may be visible at the same time, while still detaching inactive tab siblings inside the same pane.

On document activation:

1. DockPane activates the lightweight `DocumentHostControl`.
2. `ControlHostService` resolves the logical real control for that host.
3. The previous real document control in the same `DockPane` tab group is removed from its lightweight host and made not visible.
4. The selected real document control is added to the selected host, docked fill, made visible, and brought forward.
5. Existing `IControlHostClient.Activate(realControl)` runs with the real logical control, not the lightweight host.

On deactivation:

- `IControlHostClient.Deactivate(realControl)` runs for the previous logical control.
- The real control can remain attached until the next document activation, or be detached immediately if testing shows immediate detach is stable. The initial design should prefer detaching during the next activation so close/activation ordering stays simpler.

On close/unregister:

- If the real control is attached, detach it first.
- Remove it from mappings.
- Restore its original dock setting.
- Dispose the lightweight host and DockContent through the existing unregister path.
- Do not dispose the real control except through existing owner/document cleanup behavior.

## Scope Control

Only `ControlInfo.IsDocument == true` controls use the virtual host path.

This includes common document clients such as:

- `BaseEntityEditor` subclasses, including assets, behaviors, materials, geometry, light rigs, and similar entity documents.
- `ArtDefEditor` documents.
- `XLPEditor` documents.
- `GameArtSpecificationEditor` documents.
- Other center document clients that already register `ControlInfo.IsDocument = true`.

Non-document dock windows continue using direct hosting:

- Asset Previewer panes.
- property editors.
- output/history/project/resource windows.
- timeline and other tool windows unless they explicitly register as documents.

## Activation Flow

1. User selects an outer document tab.
2. DockPane activates the lightweight host inside that tab.
3. `ControlHostService` maps the lightweight host back to the real registered control.
4. `ControlHostService` switches the attached real control to the active host.
5. `ControlHostService` calls existing client activation with the real control.
6. Existing document clients update `DocumentRegistry.ActiveDocument`, `ContextRegistry.ActiveContext`, and editor-specific state as they do today.

The implementation should log key timing points through `PaintTimingLog`, including host switch time, real-control detach time, real-control attach time, and client activation time.

## Close Flow

1. User closes a document tab.
2. DockContent close handling resolves the real logical control from the lightweight host.
3. Existing `IControlHostClient.Close(realControl)` runs.
4. If close succeeds, `UnregisterControl(realControl)` detaches the real control and removes mappings.
5. Existing document-client cleanup continues unchanged.

Closing an inactive document must not detach or blank the active document.

## Error Handling

- If activation maps to a disposed real control, log the condition and leave the host blank.
- If a lightweight host is disposed while it owns the active real control, detach the real control first.
- If activation arrives before the host handle exists, defer attach with `BeginInvoke` and re-check active host, disposal state, and mapping validity.
- Visibility notifications must not throw; failures should log and fail closed.

## Testing

Automated checks:

- Add a generic `ControlHostService` repro that registers two document controls and verifies DockContent children are lightweight hosts while `Activate`, `Deactivate`, and `Close` receive the real controls.
- Verify only the active real document control is attached to a visible host after switching.
- Verify closing an inactive document does not detach the active real control.
- Verify `Show(realControl)`, `Hide(realControl)`, and `UnregisterControl(realControl)` work through the logical mapping.
- Keep existing repros passing:
  - `docs/superpowers/repros/dockpane-tab-switch/`
  - `docs/superpowers/repros/control-host-container-paint/`
  - `docs/superpowers/repros/firaxis-dockpane-content-paint/`

Manual checks:

- Open multiple document types, including `.ast`, ArtDef, XLP, and at least one other entity document.
- Switch from earlier/front documents to later/back documents and check for flicker.
- Switch from later/back documents to earlier/front documents and check for regressions.
- Confirm editor-specific inner state is preserved where applicable.
- Confirm AssetPreviewer and non-document dock windows remain responsive.
- Close active and inactive documents of multiple types.

Build check:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

## Fallback

If generic document virtualization destabilizes document lifecycle, stop and narrow the feature behind an explicit opt-in flag on `ControlInfo` or a small interface implemented by affected document clients. Do not continue patching global DockPane visibility behavior.

## Implementation Result

This design was implemented in the common WinForms `ControlHostService` path.

Implemented behavior:

- `DocumentHostControl` is used for registered controls where `ControlInfo.IsDocument == true`.
- `ControlInfo.Control` remains the real logical document control.
- `ControlInfo.HostControl` remains the outer `DockContent`.
- Activation, deactivation, close, show, hide, unregister, active-item lookup, and context-menu lookup resolve through the real logical control.
- Real document controls attach only to the active lightweight host for their `DockPane` tab group.
- Tiled/split document layouts are preserved by tracking active real controls per `DockPane`.
- `Show()` re-attaches a hidden virtual document if it was detached while another sibling was active.
- Deferred activation attaches the real control before forcing a paint flush.
- Non-document controls remain directly hosted.

Final manual result: document switching flicker is basically resolved while editor responsiveness remains acceptable.

Final automated verification:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-before-show/ControlHostBeforeShowRepro.csproj"
```

Verified result:

- AssetEditor build: 38 projects, 0 errors.
- All listed repros passed.
