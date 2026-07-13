# Document First-Frame Size Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ensure every newly opened document editor has full document-host bounds before its first visible paint.

**Architecture:** Keep the current fast `9b1cef0` document-switching lifecycle. Change only `DocumentHostControl.AttachLogicalControl()` so a hidden logical editor receives the host `DisplayRectangle` before it becomes visible.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, ATF, DockPanel Suite.

## Global Constraints

- Do not reintroduce pre-attachment or forced paint flushing in `ControlHostService`.
- Do not add a test program.
- Verify with the production build, real documents, and `paint_timing.log`.
- Preserve current tab-switch, close, and Previewer shutdown behavior.

---

### Task 1: Size the Logical Editor Before Showing It

**Files:**
- Modify: `Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs:27-67`

**Interfaces:**
- Consumes: `DocumentHostControl.DisplayRectangle`
- Produces: first visible `LogicalControl.Bounds` equal to the host display rectangle

- [ ] **Step 1: Establish hidden bounds before visibility**

After `Controls.Add(LogicalControl)` and before `LogicalControl.Visible = true`, set the child bounds when the host has a usable display size:

```csharp
Rectangle displayBounds = DisplayRectangle;
if (displayBounds.Width > 0 && displayBounds.Height > 0)
{
	LogicalControl.Bounds = displayBounds;
}
```

Add `using System.Drawing;`. Include `displayBounds` in the existing `DocumentHostAttach` timing log so real traces show the expected first size.

- [ ] **Step 2: Build and inspect the isolated change**

Run:

```powershell
rtk dotnet build "AssetEditor/AssetEditor.csproj"
rtk git diff --check
rtk git diff -- "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs"
```

Expected: 0 build errors, no whitespace errors, and no change to `ControlHostService`.

- [ ] **Step 3: Verify real first frames**

Open an AST, material or geometry, behavior, and XLP or ArtDef. Confirm:

- no editor first appears at `150x150` or a designer size;
- first visible content fills the document surface;
- forward/backward tab switching remains at the current fast baseline;
- close and application exit remain clean.

In `paint_timing.log`, the first logical `WM_SHOWWINDOW`, `PaintBackground`, or `Paint` bounds must match the `displayBounds` logged by `DocumentHostAttach`.

- [ ] **Step 4: Commit**

```powershell
rtk git add "Atf.Gui.WinForms/Sce.Atf.Applications/DocumentHostControl.cs"
rtk git commit -m "fix: size document editor before first paint"
```
