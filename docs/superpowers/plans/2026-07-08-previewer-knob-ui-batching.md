# Previewer Knob UI Batching Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reduce slow surrounding UI refresh when switching between already-open document tabs while Asset Previewer content itself updates quickly.

**Architecture:** Keep document activation and preview rendering unchanged. Batch layout work inside the Previewer Knobs internal DockHostControl while subgroup tabs are removed and recreated, avoiding repeated intermediate layouts/repaints during `ActiveKnobSet` changes.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, DockPanel Suite, Firaxis previewer controls.

## Global Constraints

- Do not use `WM_SETREDRAW`; previous trials caused whole-window flicker and window-state regressions.
- Do not change document activation ordering or preview rendering behavior.
- Keep the change scoped to Previewer Knob side-panel UI batching.
- Use `rtk` for shell verification commands.
- Verify with `rtk dotnet build AssetEditor\AssetEditor.csproj`.
- Manual verification: switch among already-open tabs and observe whether Previewer side UI catches up faster without new flicker.

---

### Task 1: Batch Previewer Knob Subgroup Tab Rebuild

**Files:**
- Modify: `Firaxis.ATF/Firaxis.ATF/DockHostControl.cs`
- Modify: `Firaxis.AssetPreviewing/Firaxis.AssetPreviewing/KnobSetEditingControl.cs`

**Interfaces:**
- Produces: `DockHostControl.BeginContentUpdate()` to suspend outer and internal DockPanel layout.
- Produces: `DockHostControl.EndContentUpdate()` to resume layout and invalidate once.
- Consumes: `KnobSetEditingControl.ActiveKnobSet` rebuild path.

- [x] **Step 1: Add DockHostControl batch layout API**

Add a small nesting-safe update counter. On first begin, call `SuspendLayout()` and `m_dockPanel.SuspendLayout()`. On final end, call `m_dockPanel.ResumeLayout(performLayout: false)`, `ResumeLayout(performLayout: false)`, and `Invalidate()` once.

- [x] **Step 2: Use batch API during ActiveKnobSet rebuild**

Wrap `RemoveAllSubgroubTabs()` and `AddSubgroubTabs()` in `BeginContentUpdate()` / `EndContentUpdate()` inside the `ActiveKnobSet` setter.

- [ ] **Step 3: Build verification**

Run: `rtk dotnet build AssetEditor\AssetEditor.csproj`

Expected: `0 errors`. Existing warnings are acceptable if unchanged or unrelated.

- [ ] **Step 4: Manual verification**

With several already-open tabs, switch between documents that use Asset Previewer. Confirm preview content still updates immediately and the surrounding Previewer Knobs/UI side panel no longer visibly lags as much. Confirm no new whole-window flicker.
