# AssetEditor Tab Flicker Debugging Notes

## Symptom

AssetEditor document tab switching has a direction-specific flicker:

- Switching from later/back documents to earlier/front documents is visually stable.
- Switching from earlier/front documents to later/back documents flickers in the main `AssetEditorControl` area.
- The flicker is not in the 3D previewer and not the whole main window.

The flicker is fast, but visible. It happens around the outer document host / WinForms docking layer, not the inner AssetEditor tab cache.

## Confirmed Good Fixes

These changes were useful and should be kept unless a later rewrite replaces the surrounding system:

- `AssetEditorControl` now preserves the last active inner document tab, so switching away/back from `Attachments` no longer resets to `Geometries`.
- `FiraxisDockPane.OnPaint()` excludes `ContentRectangle`, preventing the pane background from painting over child content.
- `ControlHostService.DockContent.OnPaintBackground()` excludes visible child bounds, preventing the document host background from painting over its full-size child.
- Heavy `ActivateClient` work was deferred out of the immediate tab switch path; `TabSwitch` timing dropped to around 1ms in logs.
- Generic document host virtualization now routes `ControlInfo.IsDocument == true` documents through lightweight `DocumentHostControl` wrappers, while keeping real document controls as logical controls for lifecycle calls.

## Final Working Direction

The flicker was not only an `.ast` / `AssetEditorControl` problem. It affected the common WinForms document hosting path used by normal editor documents.

The working fix is generic document host virtualization inside `ControlHostService`:

- For `ControlInfo.IsDocument == true`, the outer DockPane `DockContent` hosts a lightweight `DocumentHostControl` instead of the real editor control directly.
- The real editor control remains the logical control in `ControlInfo.Control`.
- Existing lifecycle calls still receive the real control: `Activate`, `Deactivate`, `Close`, `Show`, `Hide`, and `Unregister` resolve through the logical-control mapping.
- Active real controls are tracked per `DockPane`, not globally, so tiled/split document layouts can still display one real document per visible pane.
- Inactive tab siblings in the same pane are detached from the visible WinForms tree.
- Non-document dock windows, previewer panes, property grids, output windows, and similar tools continue using direct hosting.
- Deferred activation attaches the real control before forcing a paint flush, avoiding a forced paint of an empty lightweight host.
- `Show()` re-attaches the real control when a hidden virtual document is shown again after another sibling detached it.

Manual result after this change: the document switching flicker is basically resolved while preserving responsiveness.

Key files:

- `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs`
- `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- `docs/superpowers/repros/control-host-document-virtualization/`

## Important Negative Results

Do not repeat these approaches without a substantially different design:

### Do Not Use WM_SETREDRAW / Whole-Window Freeze

Earlier redraw-freeze style approaches caused broader whole-window flashing. They are too blunt for this issue.

### Do Not Pre-Position Hidden Inactive Documents

Moving a hidden inactive document into the visible content rectangle before showing it made both switching directions flash.

The guard repro is:

```text
docs/superpowers/repros/dockpane-tab-switch/
```

Expected result:

```text
PASS: hidden inactive document was not pre-moved into the visible content rectangle.
```

### Snapshot Overlay Did Not Help

A temporary screenshot overlay over the document content area was implemented and tested, but manual testing showed no visible improvement. It was removed.

Reason: the remaining flicker is not cleanly covered by a short-lived managed overlay, likely because the problematic native child-window visibility/focus work is happening at a lower z-order/message boundary.

### Retaining Inactive Document Forms Fixes Flicker But Is Unsafe

A DockPane-level model change was tested:

- Keep inactive document forms visible in the shared content rectangle.
- Switch by z-order / `BringToFront()` instead of `Visible=true/false`.

Manual result:

- Flicker disappeared.
- AssetPreviewer and editor responsiveness degraded.
- There was a probability of UI hang / freeze.

This strongly suggests that keeping heavyweight AssetEditor document forms visible lets inactive editors/preview-related controls keep participating in messages, layout, painting, focus, or other expensive work.

### Short Delayed Hide Also Caused Hangs

A bounded variant was tested:

- Keep outgoing document visible briefly during the switch.
- Hide it after a `BeginInvoke` turn or a short WinForms timer.

Manual result:

- One-turn delay restored responsiveness but flicker returned.
- A 120ms timer could still hang.

Conclusion: even short-lived retained visibility at the global DockPane layer is risky for AssetEditor.

## Previous Stable Tradeoff

Before document host virtualization, the stable state was:

- No retained inactive document visibility at the global DockPane layer.
- Existing DockPane `Visible=true/false` document semantics are restored.
- Flicker can still appear, but responsiveness and AssetPreviewer behavior are stable.

Relevant guard/build commands:

```powershell
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-container-paint/ControlHostContainerPaintRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/firaxis-dockpane-content-paint/FiraxisDockPaneContentPaintRepro.csproj"
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

## Current Recommendation

Keep the generic `ControlInfo.IsDocument == true` document host virtualization path. Do not return to global DockPane retained-visible behavior, delayed hide timers, hidden pre-positioning, or snapshot overlays.

If future issues appear, narrow the virtual-host behavior behind an explicit opt-in flag or interface rather than changing DockPane visibility semantics globally.

Fresh verification after the implemented fix:

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
- `control-host-document-virtualization`: PASS.
- `dockpane-tab-switch`: PASS.
- `control-host-container-paint`: PASS.
- `firaxis-dockpane-content-paint`: PASS.
- `control-host-before-show`: PASS.

## Runtime Evidence Source

AssetEditor clears the timing log at startup:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\paint_timing.log
```

Use this log after each manual repro run. It previously showed:

- `TabSwitch` itself became fast after deferred activation.
- The remaining visual issue lined up with native child form visibility/focus transitions.
- `activeInner=null` was mostly focus-derived noise; pane state still showed active inner content such as `Geometries`.

## 2026-07-10 Stabilization Notes

After generic document host virtualization fixed the main tab-switch flicker, several lifecycle regressions appeared around `.ast` documents and application startup. These were fixed without returning to global DockPane retained-visibility behavior.

### Theme And Tab Close Buttons

Symptom:

- After startup, document tabs could lose the Firaxis tab theme / per-tab close-button behavior.

Root cause:

- `ThemeService` defaulted to a `FiraxisTheme`, but the default theme was not pushed into `ControlHostService` during construction.
- Later assigning another `FiraxisTheme` instance could be treated as a no-op by same-type theme checks.

Fix:

- `Firaxis.ATF/Firaxis.ATF/ThemeService.cs` now applies the default `FiraxisTheme` to `ControlHostService` from the constructor.
- The tab-switch repro was updated to assert `Firaxis.Theme.FiraxisDockPaneStrip` and per-tab close support.

### Search Property Selector Flicker

Symptom:

- Search/property selector interaction still flickered after document virtualization.

Root cause:

- `FlushDocumentPaint()` was running for every deferred activation, even when no real logical document control had just been attached.

Fix:

- `ControlHostService.ActivateClientIfStillActive()` only flushes document paint when `DocumentHostControl.AttachLogicalControl()` reports that the real logical control was newly attached.

### `.ast` Open Crash

Symptom:

- Opening `.ast` could crash during `BindCookParameters()`.

Evidence:

```text
Bind step cook parameters
Bind exception System.NullReferenceException
at Firaxis.AssetEditing.AssetEditorControl.BindCookParameters()
```

Root cause:

- Some AST inner `DockContent` instances were forced to `DockState.Document` before they had a valid inner `DockPane`.

Fix:

- `AssetEditorControl.ShowInnerDocument(Control control)` now shows an inner `DockContent` on the inner dock panel when it has no pane, and only updates `DockState` once pane state is valid.
- `AssetEditorControl.HideInnerDocument(Control control)` avoids forcing unknown/no-pane content to `Hidden`.
- The cook parameters, geometry, animation, particle, behavior, and spline bind paths now use these helpers.
- Geometry activation is guarded by `DockHandler.Pane != null`.

### `BeginInvoke` Before Handle

Symptom:

- A stale crash log showed:

```text
System.InvalidOperationException: Cannot call Invoke or BeginInvoke on a control until the window handle has been created.
```

Fix:

- `AssetEditorControl.ScheduleEnsureActiveInnerContent()` now defers work until `OnHandleCreated()` if the control handle does not exist yet.
- It catches the small `InvalidOperationException` race and defers again rather than crashing.

### Saved AST Inner Layout And Default Panes

Symptom:

- Startup restore could show blank/tiny `.ast` UI.
- Manual `.ast` open could miss Name/Class/DSG/property panes.

Evidence:

- The AST control was constructed and bound, but the inner layout application could leave inner panes hidden or with invalid pane state.

Final state:

- Default AST inner panes are shown during construction even when saved layout exists.
- Later manual testing confirmed AST layout restoration works for pane ratios and active inner tabs such as `Attachments`.
- Do not skip AST saved layout wholesale unless a future repro shows it is again the root cause.

Relevant file:

- `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs`

### Startup Restore / First AST / Manual AST Open Blank UI

Symptoms:

- Startup restore of the last `.ast` could show a blank document until clicked.
- Opening the first `.ast` when no other AST was open could require clicking the tab before UI appeared.
- Opening a new `.ast` while another `.ast` was already open became stable after later fixes.

Root cause:

- `DockContent` could become visible while still hosting only the lightweight `DocumentHostControl`; the real `AssetEditorControl` was not always attached because the usual `DockPanel.ActiveContentChanged` path did not fire or did not point at the relevant visible host.
- Relying only on `DockPanel.ActiveContent` is not sufficient after restore, first document registration, or certain close/open timing windows.

Fixes in `ControlHostService`:

- `AttachVisibleDocumentHost(DockContent)` attaches and flushes a visible virtual document host directly.
- `ShowAndAttachDocumentHost(DockContent)` explicitly shows the newly registered document and attaches its real logical control after menu registration and `BringClientToFront()` finish.
- `ScheduleActivateCurrentDockContent()` now also scans visible document hosts and runs a second delayed scan to catch DockPanel state that settles one UI turn later.
- `AttachVisibleDocumentHosts()` scans visible document hosts instead of relying solely on `DockPanel.ActiveContent`.

Manual results:

- Opening the first `.ast` after all AST tabs are closed now displays immediately.
- Opening additional `.ast` documents is stable.
- Startup restore no longer needs a click to display the restored AST UI.

### Closing Current AST To Next AST Blank UI

Symptom:

- Closing the current `.ast` and automatically switching to the next `.ast` left the UI blank until clicked.

Root causes:

- Close handling scheduled reactivation too early, before DockPanel finished selecting the next document.
- `DockPanel.ActiveContent` could still be wrong or null after close, so active-content-only reactivation missed the visible next AST host.
- The old closing `AssetEditorControl` could still receive a late `Bind` after it was removed from the document host.

Fixes:

- Close handling now schedules the activation sync asynchronously after unregistering the old document.
- Visible document host scanning attaches the next visible host even when `ActiveContent` is not useful.
- `IControlHostUnregisteringClient` was added so real logical controls can be notified before host unregister.
- `AssetEditorControl` implements `IControlHostUnregisteringClient` and skips later `Bind` calls once it is being unregistered.

Manual result:

- Closing the current `.ast` now switches to the next `.ast` and displays immediately.

Key files:

- `Atf.Gui.WinForms/Sce.Atf.Applications/IControlHostPreShowClient.cs`
- `Atf.Gui.WinForms/Sce.Atf.Applications/ControlHostService.cs`
- `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs`

### Taskbar Icon Missing After Splash Handoff

Symptom:

- If another application covered AssetEditor while the splash screen ended, the main window existed but did not appear in the taskbar.
- Hiding the covering application revealed the main window and the taskbar icon reappeared.

Root cause:

- `AssetEditorForm` already forces `WS_EX_APPWINDOW` and removes `WS_EX_TOOLWINDOW`, and `Program.RefreshMainWindowTaskbarRegistration()` already refreshed those styles.
- The missing piece was taskbar button re-registration after the splash screen closed. If `ShowInTaskbar` was already `true`, the old helper did not force WinForms/Shell to recreate the taskbar entry.

Fix:

- `Program.RefreshMainWindowTaskbarRegistration(Form form, bool forceReregister = false)` can now force `ShowInTaskbar` through `false -> true` before resetting extended styles.
- The main form `Shown` handler closes the splash first, then runs the forced taskbar registration on the next UI message, and repeats once as a fallback.
- This avoids stealing focus while ensuring the Shell sees the final top-level main window.

Manual result:

- Starting AssetEditor while another app covers it now preserves/restores the main window taskbar icon after splash closes.

Relevant files:

- `AssetEditor/AssetEditor/Program.cs`
- `AssetEditor/AssetEditor/AssetEditorForm.cs`

## Latest Verification Commands

Use these after editing the document host or AST lifecycle code:

```powershell
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Recent verified results:

- AssetEditor build: 38 projects, 0 errors.
- `control-host-document-virtualization`: PASS.
- `dockpane-tab-switch`: PASS.
- `asset-editor-inner-active`: PASS.

Operational note:

- WinForms repros can leave `dotnet` processes alive and lock DLL copy targets in the SDK AssetEditor directory. If build copy steps fail with `MSB3073` because a DLL is in use, check for stale `AssetEditor`, `dotnet`, or `VBCSCompiler` processes before rebuilding.
