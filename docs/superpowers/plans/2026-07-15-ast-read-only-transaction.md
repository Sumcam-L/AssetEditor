# AST Read-Only Transaction Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Restore the original compiled AST read-only tab state and reject read-only property edits before any provisional value reaches the document or UI.

**Architecture:** Restore the dynamic dirty/read-only delegates that the original compiled `BaseEntityEditor.Open()` assigned to `ControlInfo`. Move the primary AST mutation rejection to `BaseEntityPropertyContext.OnBeginning()` so `TransactionContexts.DoTransaction()` stops before invoking the setter, while retaining the existing `OnEnded()` check as defense in depth.

**Tech Stack:** C# 11, .NET Framework 4.6.2, ATF DOM transactions, WinForms docking, MSBuild

## Global Constraints

- Do not change which AST paths are read-only.
- Do not modify `ProjectMapService`, SDK Assets file attributes, version-control checkout behavior, or shared PropertyGrid controls.
- Preserve the existing `File not changed` warning title and message.
- Preserve the existing `BaseEntityPropertyContext.OnEnded()` read-only guard.
- Writable active-project AST documents must retain normal editing, dirty tracking, save, and source-control behavior.
- Commit implementation only after the user verifies the deployed Release build in the real Asset Editor UI.

---

### Task 1: Restore AST Document-State Delegates

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs:285-305`

**Interfaces:**
- Consumes: `BaseInstanceEntityDocument.Dirty`, `BaseInstanceEntityDocument.IsReadOnly`, `ControlInfo.IsDirtyDocument`, and `ControlInfo.IsReadOnlyDocument`
- Produces: dynamic document-state delegates queried by `FiraxisDockPaneStrip` when painting AST tabs

- [ ] **Step 1: Run the failing source assertion**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command '$code = Get-Content -Raw "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs"; if ($code -match ''controlInfo\.IsDirtyDocument\s*=\s*\(\)\s*=>\s*document\.Dirty'' -or $code -match ''controlInfo\.IsReadOnlyDocument\s*=\s*\(\)\s*=>\s*document\.IsReadOnly'') { throw "Expected missing AST document-state delegates before the fix" }'
```

Expected: command succeeds, proving the current source lacks both delegates.

- [ ] **Step 2: Add the original compiled delegate assignments**

Immediately after:

```csharp
BaseInstanceEntityDocument document = InitializeDocument(domNode, uri, instanceEntity, instanceSet);
```

add:

```csharp
controlInfo.IsDirtyDocument = () => document.Dirty;
controlInfo.IsReadOnlyDocument = () => document.IsReadOnly;
```

Keep both assignments before `ControlHostService?.RegisterControl(...)`.

- [ ] **Step 3: Run the passing source assertion**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command '$code = Get-Content -Raw "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs"; $dirty = $code.IndexOf("controlInfo.IsDirtyDocument = () => document.Dirty", [StringComparison]::Ordinal); $readOnly = $code.IndexOf("controlInfo.IsReadOnlyDocument = () => document.IsReadOnly", [StringComparison]::Ordinal); $register = $code.IndexOf("ControlHostService?.RegisterControl", [StringComparison]::Ordinal); if ($dirty -lt 0 -or $readOnly -lt 0 -or $register -lt 0 -or $dirty -gt $register -or $readOnly -gt $register) { throw "AST document-state delegates are missing or assigned after registration" }'
```

Expected: command succeeds with no output.

---

### Task 2: Reject Read-Only AST Transactions Before Mutation

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs:140-160`

**Interfaces:**
- Consumes: `BaseInstanceEntityDocument.IsReadOnly`, `BaseInstanceEntityDocument.CivTechService.PrimaryProject.Name`, `HistoryContext.OnBeginning()`, `MessageBoxes.Show(...)`, and `InvalidTransactionException`
- Produces: `BaseEntityPropertyContext.OnBeginning()` pre-mutation read-only rejection

- [ ] **Step 1: Run the failing source assertion**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command '$code = Get-Content -Raw "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs"; if ($code -match ''protected override void OnBeginning\(\)'') { throw "Expected no pre-mutation AST read-only guard before the fix" }; if ($code -notmatch ''protected override void OnEnded\(\)'') { throw "Existing end-of-transaction guard is missing" }'
```

Expected: command succeeds, proving rejection currently occurs only at transaction end.

- [ ] **Step 2: Add the minimal pre-mutation guard**

Insert immediately before the existing `OnEnded()` override:

```csharp
protected override void OnBeginning()
{
	if (Doc != null && Doc.IsReadOnly)
	{
		string message = "Can not modify assets that are not part of the active project \"" + Doc.CivTechService.PrimaryProject.Name + "\"";
		MessageBoxes.Show(message, "File not changed", MessageBoxButton.OK, MessageBoxImage.Error);
		throw new InvalidTransactionException(message);
	}
	base.OnBeginning();
}
```

Do not alter the existing `OnEnded()` method.

- [ ] **Step 3: Run the passing source-order assertion**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command '$code = Get-Content -Raw "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs"; $match = [regex]::Match($code, ''protected override void OnBeginning\(\)\s*\{(?<body>.*?)\n\t\}'', [Text.RegularExpressions.RegexOptions]::Singleline); if (-not $match.Success) { throw "OnBeginning override not found" }; $body = $match.Groups["body"].Value; $guard = $body.IndexOf("Doc != null && Doc.IsReadOnly", [StringComparison]::Ordinal); $throw = $body.IndexOf("throw new InvalidTransactionException(message)", [StringComparison]::Ordinal); $baseCall = $body.IndexOf("base.OnBeginning()", [StringComparison]::Ordinal); if ($guard -lt 0 -or $throw -lt 0 -or $baseCall -lt 0 -or $throw -gt $baseCall) { throw "Read-only rejection does not occur before base transaction setup" }; if ($code -notmatch ''protected override void OnEnded\(\)'') { throw "Existing OnEnded defense was removed" }'
```

Expected: command succeeds with no output.

- [ ] **Step 4: Build the affected project in Release x64**

Run:

```powershell
rtk dotnet msbuild "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj" -t:Build -p:Configuration=Release -p:Platform=x64 -p:PostBuildEvent= -p:RunPostBuildEvent=Never
```

Expected: build succeeds with 0 errors. Existing repository warnings are acceptable.

- [ ] **Step 5: Inspect scope and whitespace**

Run:

```powershell
rtk git diff --check
rtk git status --short
rtk git diff -- "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs" "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs"
```

Expected: no whitespace errors; implementation changes are limited to the two approved source files, with this plan as the only additional uncommitted file.

- [ ] **Step 6: Deploy the verified DLL**

After confirming the SDK Asset Editor process is not locking the destination, copy:

```text
Firaxis.AssetEditing/bin/x64/Release/net462/Firaxis.AssetEditing.dll
```

to:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\Firaxis.AssetEditing.dll
```

Compute SHA-256 for both files and require matching hashes after deployment.

- [ ] **Step 7: Verify the real UI behavior**

In the normal SDK Asset Editor:

1. Open an AST under the configured external SDK `AssetsPath`.
2. Confirm its document tab shows the original gray/read-only overlay.
3. Attempt a text edit and confirm the warning appears before the displayed value changes.
4. Attempt a dropdown edit and confirm no selected value is provisionally committed.
5. Confirm the read-only AST never becomes dirty.
6. Open an AST from the active project, edit text and dropdown values, edit an AttachmentPoint, save, close, and reopen it.
7. Confirm the active-project AST still becomes dirty and persists edits normally.

Expected: all seven checks pass. If a read-only value still flashes, stop and capture transaction and `ItemChanged` ordering rather than changing shared PropertyGrid code.

- [ ] **Step 8: Commit only after user verification**

Run:

```powershell
rtk git add -- "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityEditor.cs" "Firaxis.AssetEditing/Firaxis.AssetEditing/BaseEntityPropertyContext.cs" "docs/superpowers/plans/2026-07-15-ast-read-only-transaction.md"
rtk git diff --cached --check
rtk git diff --cached --name-status
rtk git commit -m "fix: reject read-only ast edits before mutation"
```

Expected: exactly the two source files and this plan are staged. Push only when explicitly requested or when the user confirms the established upload workflow.
