# AST Read-Only Transaction Design

## Goal

Restore the original compiled Asset Editor behavior for read-only AST documents: the document tab visibly indicates read-only state, and an attempted property edit is rejected before the property value changes on screen.

## Evidence

The source in the original-copy directory does not fully match its compiled Debug binaries. IL inspection of the original `Firaxis.AssetEditing.dll` shows that `BaseEntityEditor.Open()` assigns both:

```csharp
controlInfo.IsDirtyDocument = () => document.Dirty;
controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
```

The current source and compiled Release binary omit these assignments. Consequently, `FiraxisDockPaneStrip` receives the default `false` read-only delegate and does not draw the original read-only tab overlay.

Read-only AST mutation is currently rejected by `BaseEntityPropertyContext.OnEnded()`. This runs after the property setter and DOM notifications. `TransactionContexts.DoTransaction()` then catches the resulting `InvalidTransactionException` and cancels the transaction, undoing the value. The user therefore sees a provisional new value, the warning dialog, and then the old value.

The configured SDK `AssetsPath` currently contains 7,313 AST files, all writable according to Windows file attributes. `VersionControlDocuments.TransactionContext_Beginning()` skips its early project check when a file is already writable, so it does not provide the original pre-mutation rejection for these files.

## Desired Behavior

- An AST for which `BaseInstanceEntityDocument.IsReadOnly` is true displays the existing read-only tab overlay.
- Dirty AST documents continue to display the existing dirty-tab overlay.
- Starting a property mutation on a read-only AST immediately shows the existing `File not changed` warning.
- No property setter, DOM change notification, provisional repaint, dirty transition, or undo operation occurs before the warning.
- The attempted edit leaves the property value unchanged.
- Writable active-project AST documents retain their current editing, dirty tracking, save, and source-control behavior.

## Design

### Restore Document-State Delegates

In `BaseEntityEditor.Open()`, assign `ControlInfo.IsDirtyDocument` and `ControlInfo.IsReadOnlyDocument` immediately after the `BaseInstanceEntityDocument` has been initialized and before the control is registered.

The delegates remain dynamic rather than storing a one-time Boolean. This matches the original compiled binary and allows the tab renderer to query the current document state whenever it paints.

### Reject Read-Only Transactions Before Mutation

Override `BaseEntityPropertyContext.OnBeginning()`.

If `Doc?.IsReadOnly` is true:

1. Show the existing warning text and title.
2. Throw `InvalidTransactionException` before calling the base implementation.

If the document is writable, call `base.OnBeginning()` and continue normally.

The warning text remains:

```text
Can not modify assets that are not part of the active project "<project>"
```

The title remains `File not changed`.

`TransactionContexts.DoTransaction()` already catches `InvalidTransactionException`, calls `Cancel()`, and prevents the transaction delegate from running when `Begin()` throws. No shared property editor changes are required.

### Preserve End-of-Transaction Defense

Keep the existing `BaseEntityPropertyContext.OnEnded()` read-only check unchanged. It remains defense in depth for nested or externally started transactions that bypass the normal `DoTransaction()` beginning path.

## Scope

Files in scope:

- `Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs`
- `Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs`

Out of scope:

- Changing `ProjectMapService` path classification
- Changing which AST paths are read-only
- Modifying Windows file attributes under SDK `AssetsPath`
- Changing version-control checkout behavior
- Changing shared PropertyGrid controls or property descriptors
- Changing ArtDef or XLP read-only policy
- Replacing the existing tab overlay visual design

## Error Handling

- A null `Doc` does not trigger read-only rejection; the base transaction behavior continues.
- The current project name is interpolated only when a non-null read-only document is present.
- The exception uses the existing `InvalidTransactionException` flow so callers receive the same failure semantics as other rejected editor transactions.
- The end-of-transaction guard remains available if a mutation enters an already-active transaction and therefore does not call `Begin()` again.

## Verification

### Automated/Source Regression Checks

- Confirm `BaseEntityEditor.Open()` assigns dynamic dirty and read-only delegates before `RegisterControl()`.
- Confirm `BaseEntityPropertyContext.OnBeginning()` throws for a read-only document before `base.OnBeginning()` or a transaction delegate can execute.
- Confirm a writable document reaches `base.OnBeginning()`.
- Confirm `OnEnded()` retains its existing read-only guard.

Where practical, use a focused transaction test that records whether the mutation delegate ran. The read-only case must return false from `DoTransaction()`, leave the value unchanged, and record zero setter invocations. The writable case must execute once.

### Build Verification

- Build `Firaxis.AssetEditing/Firaxis.AssetEditing.csproj` in Release x64 with post-build deployment disabled.
- Require zero build errors; existing repository warnings are acceptable.

### Manual Asset Editor Matrix

1. Open an AST from the configured external SDK `AssetsPath`.
2. Confirm its tab displays the original gray/read-only overlay.
3. Attempt to edit a text property and a dropdown property.
4. Confirm the warning appears immediately and the displayed value never changes.
5. Confirm the document does not become dirty.
6. Open an active-project AST and confirm normal text, dropdown, AttachmentPoint, dirty-state, save, and reopen behavior.
