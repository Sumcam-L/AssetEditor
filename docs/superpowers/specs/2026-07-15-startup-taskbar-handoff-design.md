# Startup Taskbar Handoff Design

## Goal

Ensure the Asset Editor main window always has a Windows taskbar button after the startup splash closes, including when another application covers Asset Editor during startup, without stealing focus or manipulating the main window state.

## Current Failure

Startup creates two independent top-level WinForms windows:

1. The splash, with `ShowInTaskbar = true`.
2. The main `AssetEditorForm`, also with `ShowInTaskbar = true` and permanent `WS_EX_APPWINDOW`/non-`WS_EX_TOOLWINDOW` styles.

The current `Shown` handler explicitly adds the main window to the taskbar and then closes the splash:

```csharp
RegisterMainWindowTaskbarButton(assetEditorForm);
splash.Close();
```

The Windows shell can remove or fail to preserve the main button while processing the splash window's destruction. Because the main form's `ShowInTaskbar` property is already true, WinForms does not recreate the shell entry afterward. The main window remains visible but has no taskbar button until another shell/window transition causes it to reappear.

## Why The Regression Recurred

The repository debugging notes record a manually verified later solution: close the splash, then force the main form through a taskbar re-registration on a deferred UI message, with one fallback. That solution is absent from the checked-in `Program.cs` and reachable Git history.

The existing `startup-taskbar-handoff` repro checks only that an explicit `AddTab(main)` appears before `splash.Close()`. It therefore accepts the current failing sequence and reports `PASS`. The regression persisted because the successful behavior was documented but not enforced by source or a discriminating regression check.

## Design

### Permanent Window Identity

`AssetEditorForm.CreateParams` remains the single definition of the main window's permanent shell identity:

- Include `WS_EX_APPWINDOW`.
- Exclude `WS_EX_TOOLWINDOW`.

This protects every handle creation and recreation. The startup handoff must not replace or duplicate this permanent policy.

### Post-Splash Taskbar Re-Registration

`Program` will own one focused helper for startup taskbar registration. The helper will:

1. Optionally force WinForms to unregister and recreate the taskbar entry by setting `ShowInTaskbar` from `false` to `true`.
2. Reapply `WS_EX_APPWINDOW` and clear `WS_EX_TOOLWINDOW` on the current main form handle.
3. Notify Windows that the non-client/window style changed without moving, sizing, activating, or changing Z-order.
4. Explicitly add the final main form handle to `ITaskbarList` as a shell-level fallback.

The helper accepts a `forceReregister` flag. Normal calls can preserve the current `ShowInTaskbar` state; startup calls after splash closure use `forceReregister: true`.

### Handoff Sequence

The main form `Shown` handler will:

1. Normalize `WindowState` only if it was unexpectedly minimized.
2. Close the startup splash.
3. Use `BeginInvoke` so splash destruction is processed before taskbar re-registration.
4. Call the helper with `forceReregister: true`.
5. Queue one additional deferred helper call as a fallback after the first re-registration has reached the message queue.

The statistics dump remains part of startup, but it does not control handoff ordering.

No taskbar helper runs before the splash closes during the successful startup path.

## Focus And Window-State Constraints

The handoff must not call:

- `Activate()`
- `Focus()`
- `BringToFront()`
- `SetForegroundWindow()`
- `ShowWindow()` with minimize, restore, or activation flags

The style notification uses `SWP_NOMOVE`, `SWP_NOSIZE`, `SWP_NOZORDER`, `SWP_NOACTIVATE`, and `SWP_FRAMECHANGED`. Startup must not minimize or restore a normally displayed main window merely to create a taskbar button.

## Regression Check

The source-level `startup-taskbar-handoff` repro will reject the current implementation and require all of the following:

- The splash keeps a taskbar button during initialization.
- A helper with a `forceReregister` path toggles `ShowInTaskbar` through `false` then `true`.
- The helper reapplies the required extended window styles and notifies Windows of a frame/style change.
- `splash.Close()` occurs before the first forced registration call in the `Shown` handler.
- The forced registration is deferred with `BeginInvoke`.
- A second deferred registration call exists as a bounded fallback.
- No direct main taskbar registration occurs before `splash.Close()` on the successful startup path.
- No focus-stealing or window-state cycling APIs are introduced.

This repro intentionally validates source structure because the bug depends on ordering at the Windows shell boundary and the repository does not have an automated desktop-shell integration harness. Real Windows startup verification remains required.

## Scope

Files in scope:

- `AssetEditor/AssetEditor/Program.cs`
- `docs/superpowers/repros/startup-taskbar-handoff/Program.cs`

`AssetEditor/AssetEditor/AssetEditorForm.cs` is verified but does not need behavioral changes unless investigation during implementation shows its permanent `CreateParams` policy is missing.

Out of scope:

- Changing splash appearance or initialization duration
- Changing document startup restoration
- Introducing a timer or recurring taskbar repair loop
- Activating or foregrounding Asset Editor
- Changing docking, themes, or window layout restoration
- Modifying or committing the current AST read-only implementation as part of this task

## Failure Handling

- COM taskbar calls remain non-fatal; shell integration failure must not prevent Asset Editor startup.
- Helper calls return immediately if the main form is disposed or disposing.
- Deferred callbacks repeat the same disposal checks.
- The bounded fallback runs only once. No continuous retry remains after startup.

## Verification

1. Run the strengthened `startup-taskbar-handoff` repro against the current source before implementation and confirm it fails for the old pre-close registration sequence.
2. Run it after implementation and require `PASS`.
3. Build `AssetEditor/AssetEditor.csproj` in Release x64 with zero errors.
4. Deploy the Release executable and required changed assemblies with hash verification.
5. Start Asset Editor normally and confirm a continuous taskbar presence from splash to main window.
6. Start Asset Editor while another application covers it; after the splash closes, confirm the main taskbar button exists without bringing Asset Editor to the foreground.
7. Repeat startup several times to exercise shell timing.
8. Confirm closing the application leaves no orphan taskbar button or running process.
9. Confirm the existing uncommitted AST changes and plan remain isolated from this task's commit.
