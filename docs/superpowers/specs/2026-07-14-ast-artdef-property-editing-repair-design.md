# AST and ArtDef Property Editing Repair Design

## Summary

Repair three editor regressions without weakening project ownership protections:

- Keep the AST `Class Name` dropdown open long enough to select a value.
- Restore editing for the three fields in each AST AttachmentPoint.
- Allow a saved ArtDef that belongs to the active project to change its `Template`.

The repair must preserve the existing rule that documents outside the active project cannot be modified.

## Current Behavior

### AST Class Name

Clicking the `Class Name` dropdown briefly displays the list, then immediately closes it. The user cannot select a class.

### AST AttachmentPoint

Within each embedded AttachmentPoint property grid:

- `Attachment Point Name` does not create or show a text editing control.
- `Model Instance` does not show its dropdown button.
- `Bone Name` does not show its dropdown button.
- `Position`, `Rotation`, and `Scale` remain editable.

Other top-level AST text fields are also editable, so this is specific to the embedded property-grid path rather than a document-wide read-only state.

### ArtDef Template

`ArtDefEditor.Open()` marks the Template control read-only whenever the file already exists. `ArtDefEditor.Save()` then unconditionally marks it read-only after saving. As a result, a saved ArtDef cannot change its template even when the document belongs to the active project and is otherwise writable.

## Desired Behavior

### AST Class Name

- Clicking the dropdown opens the available class list.
- The list remains open while the user interacts with it.
- Selecting a class closes the list and commits the selected value through the existing property transaction.

### AST AttachmentPoint

- `Attachment Point Name` is a free-text field.
- `Model Instance` is constrained to the available model-instance list.
- `Bone Name` is constrained to the bones available for the selected model instance.
- `Position`, `Rotation`, and `Scale` retain their current behavior.
- Existing descriptors, converters, serialization, and model update logic remain unchanged unless investigation proves one of them is part of the root cause.

### ArtDef Template

- A saved or reopened ArtDef belonging to the active project can open the Template dropdown and select another template.
- Selection uses the existing template-change transaction and root-collection rebuild behavior.
- No confirmation dialog is shown before switching.
- Switching may remove data that only exists in the old template; this is accepted behavior for this task.

## Read-Only Safety Requirements

Project ownership protection is a hard requirement and must not be relaxed.

- `ArtDefDocument.IsReadOnly` remains the authority for whether an ArtDef is editable.
- ArtDefs from mod dependencies, outside the active project or its permitted writable scope, or with an unsupported newer version remain read-only.
- The Template control is enabled only when `ArtDefDocument.IsReadOnly` is false.
- `ArtDefContext` transaction guards remain unchanged as defense in depth. A modification attempted through any unintended UI path must still be rejected for a read-only document.
- AST property editing continues to honor both the property descriptor's `IsReadOnly` value and the document/context-level transaction guards.
- The shared property-grid repair must restore editor presentation and focus only; it must not bypass any read-only decision.

## Technical Design

### Shared WinForms Property Editing

The AST failures use the ATF WinForms `PropertyGridView` and `PropertyEditingControl` path. AttachmentPoints add another layer through `EmbeddedCollectionEditor.ItemControl`, which owns a nested `PropertyGridView`.

The implementation will first capture the failures with focused tests and instrumentation, then repair the smallest shared boundary shown to be responsible. The expected repair boundary is:

- Ensure a selected editable property in a nested `PropertyGridView` presents its reusable `PropertyEditingControl` over the value region with valid bounds and visibility.
- Ensure the editing control receives focus without the parent embedded collection clearing the nested selection.
- Ensure opening a dropdown does not cause parent selection synchronization or transient focus movement to immediately deactivate and close the dropdown form.
- Preserve the existing `PropertyEditingControl.Bind`, transaction, converter, and UI type editor paths.
- Preserve specialized numeric controls and all top-level property-grid behavior.

The repair belongs in the shared property editing layer rather than AST-specific controls because the missing text overlay, missing dropdown buttons, and immediately closing dropdown all concern editor presentation and focus. AST-specific descriptors already declare the required editability and editor types.

### ArtDef Template Enablement

Replace file-existence and post-save locking with document-read-only state:

- During `ArtDefEditor.Open()`, set `MainControl.TemplateReadOnly` from `document.IsReadOnly`, not from whether the path already exists.
- During `ArtDefEditor.Save()`, refresh `MainControl.TemplateReadOnly` from `document.IsReadOnly`, not an unconditional `true`.
- Keep `ArtDefSetControl` and `ArtDefSetAdapter` template-selection and mutation logic unchanged.
- Keep `ArtDefDocument.IsReadOnly` and `ArtDefContext` transaction validation unchanged.

## Data Flow

### AST Property Edit

1. The user clicks an editable property value.
2. `PropertyGridView` selects the property and binds its reusable `PropertyEditingControl` to the existing `PropertyEditorControlContext`.
3. For text, the editing control accepts input and commits through `PropertyEditorControlContext.SetValue()`.
4. For dropdown fields, the existing `UITypeEditor` opens through `IWindowsFormsEditorService` and returns the selected value.
5. The existing transaction context validates document writability and records the change.

### ArtDef Template Change

1. `ArtDefEditor` enables Template only when `document.IsReadOnly` is false.
2. The user chooses a template from `ArtDefSetControl`.
3. The existing adapter locates the selected template in the active project configuration.
4. The existing transaction applies that template and rebuilds the root collections.
5. Existing dirty tracking and save behavior persist the change.

## Error Handling

- Empty class, model-instance, or bone lists use the current editor behavior; this task does not invent placeholder values.
- Invalid or unavailable ArtDef templates continue to use the existing validation and error messages.
- Read-only ArtDef mutation attempts continue to be rejected by `ArtDefContext`, including its current user-facing message for assets outside the active project.
- No exceptions are swallowed or converted into silent write access.

## Verification

### Automated Regression Coverage

Add the smallest feasible tests around the shared property editing behavior:

- A normal editable string property enters text-edit mode.
- An editable string property in a nested `PropertyGridView` enters text-edit mode with a visible, non-empty editing area.
- A normal enum dropdown remains open until selection or explicit dismissal.
- A nested enum dropdown presents its button, remains open, and commits a selection.
- Read-only descriptors do not enter edit mode.
- Parent selection synchronization does not clear an active nested edit.

Add or adapt focused ArtDef tests where the existing test infrastructure permits:

- Existing writable ArtDef: Template is enabled.
- Newly created writable ArtDef: Template is enabled.
- Dependency or non-active-project ArtDef: Template is disabled.
- Saving a writable ArtDef does not disable Template.

Each regression test must be observed failing for the intended reason before production code changes.

### Manual AssetEditor Matrix

- Active-project AST: `Class Name` opens, stays open, and commits a selection.
- Active-project AST AttachmentPoint: name text editing works; model and bone dropdowns appear and commit; transforms still work.
- Read-only/non-project AST: none of these controls can commit changes.
- Active-project saved ArtDef: Template is enabled and can switch without confirmation.
- Active-project ArtDef after save: Template remains enabled.
- Dependency/non-active-project ArtDef: Template remains disabled and other edits remain blocked.
- Save and reopen changed AST and ArtDef documents to confirm values persist.

### Build Verification

- Compile the affected ATF WinForms and Firaxis AssetEditing projects.
- Run all relevant existing tests plus the new focused regression tests.
- If the running AssetEditor locks deployment DLLs, use a compile-only verification first and deploy only after the user confirms the running process can be closed.

## Out of Scope

- Changing AST or ArtDef serialization formats.
- Redesigning property grids or replacing ATF editors.
- Adding a template-change confirmation dialog.
- Changing which project paths count as writable.
- Changing theme colors or reverting dynamic gray-theme coverage unless root-cause evidence specifically requires a narrowly scoped adjustment.
- Refactoring unrelated document switching, preview, or cooking code.
