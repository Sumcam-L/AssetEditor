# Gray UI Coverage Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Apply the existing dark gray skin to all normal dynamically created editor, tool-window, dialog, and input UI without changing content colors.

**Architecture:** Fix the central `SkinService` gap instead of recoloring every form. Skin controls even before handle creation, subscribe each skinned control tree to future `ControlAdded` events, and apply the active skin to newly attached subtrees; use targeted local corrections only for normal controls that explicitly overwrite the skin afterward.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, ATF skin XML, WPF resource conversion.

## Global Constraints

- Keep `Firaxis.ATF.Resources.Dark.skn` as the color source.
- Do not blanket-recolor color pickers, texture/image previews, render or curve canvases, syntax highlighting, or warning/error colors.
- Do not add a test program.
- Verify with the production build and real UI inspection.
- Perform one focused review of dynamic event subscription and disposal only.

---

### Task 1: Skin Dynamically Added Controls

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/SkinService.cs:203-211, 314-377`

**Interfaces:**
- Consumes: existing `SkinService.ApplyActiveSkin(object)` and `s_activeSkin`
- Produces: automatic active-skin application for subtrees added after their parent was skinned

- [ ] **Step 1: Apply styles before handle creation**

In `ApplyNewPropertyValues`, remove the `control.IsHandleCreated` requirement. A control style must be applied whenever the control is not disposed:

```csharp
if (control == null || !control.IsDisposed)
{
	SkinStyle skinStyle = FindBestSkinStyle(type, s_activeSkin.Styles);
	// existing setter application
}
```

This fixes current explicit calls such as `SkinService.ApplyActiveSkin(this)` that occur before the control creates a handle.

- [ ] **Step 2: Track future children of every skinned control**

For every non-disposed `Control` visited by `ApplyNewPropertyValues`, idempotently subscribe to `ControlAdded`:

```csharp
control.ControlAdded -= Control_ControlAdded;
control.ControlAdded += Control_ControlAdded;
```

Add the static handler:

```csharp
private static void Control_ControlAdded(object sender, ControlEventArgs e)
{
	if (s_activeSkin == null || e.Control == null || e.Control.IsDisposed)
	{
		return;
	}
	ApplyActiveSkin(e.Control, null);
}
```

The subscription lives on the control itself and does not create a global strong reference. Existing weak skin tracking remains unchanged.

- [ ] **Step 3: Build and inspect the central change**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
rtk git diff --check
rtk git diff -- "Atf.Gui.WinForms/Sce.Atf.Applications/SkinService.cs"
```

Expected: 0 build errors and no unrelated skin or renderer changes.

---

### Task 2: Remove Confirmed Normal-UI White Overrides

**Files:**
- Modify if still white after Task 1: `Firaxis.ATF/Firaxis.ATF/CollapsibleControl.cs:170-218`
- Modify if still white after Task 1: `Firaxis.ATF/Firaxis.ATF/CollapsiblePropertyEditingListControl.cs:40-48`
- Modify if still white after Task 1: `Firaxis.AssetEditing/Firaxis.AssetEditing/CollapsiblePropertyEditingControl.cs:42-50`

**Interfaces:**
- Consumes: inherited colors applied by `SkinService`
- Produces: collapsible property surfaces that no longer reset themselves to `SystemColors.Window`

- [ ] **Step 1: Inspect representative normal UI after the central fix**

Open AST, material/geometry, behavior, XLP, ArtDef, and Game Art editors plus file-type selection, find/select, embedded collection, and browser dialogs. Record only controls that remain Windows white after Task 1.

- [ ] **Step 2: Remove only confirmed post-skin overrides**

For the three collapsible controls above, remove designer assignments of `SystemColors.Window` only when real inspection confirms they remain white. Let the active skin's base `Control` style supply `BackColor` and `ForeColor`.

Do not change explicit colors in color editing, preview, canvas, syntax, key-frame, warning, or error code.

- [ ] **Step 3: Verify gray coverage and semantic exclusions**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
rtk git diff --check
```

Verify normal text fields, trees, lists, grids, property collections, collapsible controls, and toolbars are gray. Confirm previews, color controls, canvases, syntax highlighting, and warning/error colors are unchanged. Switch to the light theme and back once to verify original-value tracking and dynamic subscriptions do not corrupt either theme.

- [ ] **Step 4: Perform one focused lifecycle review and commit**

Review only duplicate `ControlAdded` subscriptions, disposed controls, weak tracking, and theme switching. Then commit:

```powershell
rtk git add "Atf.Gui.WinForms/Sce.Atf.Applications/SkinService.cs" "Firaxis.ATF/Firaxis.ATF/CollapsibleControl.cs" "Firaxis.ATF/Firaxis.ATF/CollapsiblePropertyEditingListControl.cs" "Firaxis.AssetEditing/Firaxis.AssetEditing/CollapsiblePropertyEditingControl.cs"
rtk git commit -m "fix: apply gray skin to dynamic ui"
```

Stage only files actually changed.
