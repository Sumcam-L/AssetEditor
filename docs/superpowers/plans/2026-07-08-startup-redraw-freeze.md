# Startup Redraw Freeze Implementation Plan (Abandoned)

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Hide the visible startup cascade where WinForms, DockPanel, and hosted WPF controls paint one area at a time.

**Outcome:** Abandoned after manual testing. The main-form-level `WM_SETREDRAW` freeze caused the whole application to flash twice after startup, likely because redraw restoration forced a full-form repaint and the splash/taskbar transition caused a second compositor update.

**Architecture:** Keep the existing ATF/DockPanel/WinForms architecture intact. Add a narrow startup redraw freeze around the main form so existing layout work can complete while the splash remains visible, then restore redraw and repaint the full window once before closing the splash.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, ATF `MainForm`, DockPanel Suite, Win32 `WM_SETREDRAW`.

## Global Constraints

- Keep the change minimal and avoid restructuring ATF, DockPanel Suite, or editor controls.
- Preserve the previous splash/taskbar fix: splash remains in the taskbar until the main form is ready.
- Use `rtk` for shell verification commands.
- Verify with `rtk dotnet build AssetEditor\AssetEditor.csproj`.
- Manual visual verification is required because this is a first-paint behavior and the repository has no existing automated test project.

---

### Task 1: Startup Redraw Freeze

**Files:**
- Modify: `AssetEditor/AssetEditor/AssetEditorForm.cs`
- Modify: `AssetEditor/AssetEditor/Program.cs`

**Interfaces:**
- Produces: `AssetEditorForm.BeginStartupRedrawFreeze()` to request startup redraw suppression.
- Produces: `AssetEditorForm.EndStartupRedrawFreeze()` to restore redraw and repaint the form.
- Consumes: existing `Program.MainImpl` startup flow and `assetEditorForm.Shown` callback.

- [x] **Step 1: Add startup redraw freeze API to `AssetEditorForm`**

Add `WM_SETREDRAW` interop and two public methods. The freeze request must survive handle creation by applying in `OnHandleCreated` if the handle does not exist yet.

- [x] **Step 2: Request redraw freeze immediately after creating the main form**

Call `assetEditorForm.BeginStartupRedrawFreeze();` before the form is composed and before `InitializeAll()` can restore DockPanel/toolstrip layouts.

- [x] **Step 3: Restore redraw from the first `Shown` callback**

After taskbar registration is refreshed, call `assetEditorForm.EndStartupRedrawFreeze();` before closing the splash. This keeps the existing splash visible until the main form is ready to paint a complete frame.

- [x] **Step 4: Build verification**

Run: `rtk dotnet build AssetEditor\AssetEditor.csproj`

Expected: `0 errors`. Existing warnings are acceptable if unchanged.

- [x] **Step 5: Manual visual verification**

Launch AssetEditor normally. Result: rejected because the startup transition introduced two whole-window flashes. The code was removed; any follow-up should target narrower DockPanel/toolstrip layout boundaries instead of freezing the entire main form.
