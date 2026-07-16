# Startup Taskbar Handoff Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Preserve the Asset Editor main-window taskbar button after the startup splash closes without activating or cycling the main window.

**Architecture:** Keep `AssetEditorForm.CreateParams` as the permanent window-style policy. Replace the pre-close shell registration with one bounded post-close handoff that forces WinForms taskbar re-registration, reapplies top-level extended styles, sends a non-activating frame change, and explicitly registers the final handle with the shell.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, Win32 window styles, `ITaskbarList`, MSBuild

## Global Constraints

- Close the splash before any forced main-window taskbar registration.
- Defer taskbar re-registration with `BeginInvoke` and perform exactly one deferred fallback.
- Do not activate, focus, foreground, minimize, restore, move, resize, or reorder the main window.
- Do not introduce a timer or recurring taskbar repair loop.
- Keep COM shell failures non-fatal.
- Do not stage or commit the pending AST read-only source changes or AST implementation plan as part of this task.
- Commit implementation only after real Windows startup verification.

---

### Task 1: Make The Startup Handoff Repro Reject The Current Bug

**Files:**
- Modify: `docs/superpowers/repros/startup-taskbar-handoff/Program.cs:8-41`

**Interfaces:**
- Consumes: source text from `AssetEditor/AssetEditor/Program.cs`
- Produces: a process exit code and diagnostic proving the required post-splash handoff structure

- [ ] **Step 1: Replace the permissive assertions**

Replace the current checks after reading `source` with assertions that require these exact source markers:

```csharp
if (!source.Contains("splash.ShowInTaskbar = true"))
{
    Console.Error.WriteLine("FAIL: startup has no taskbar button while initialization is running.");
    return 1;
}

int shownIndex = source.IndexOf("assetEditorForm.Shown +=", StringComparison.Ordinal);
int closeIndex = source.IndexOf("splash.Close();", shownIndex, StringComparison.Ordinal);
int beginInvokeIndex = source.IndexOf("assetEditorForm.BeginInvoke", closeIndex, StringComparison.Ordinal);
int forcedRegistrationIndex = source.IndexOf("RefreshMainWindowTaskbarRegistration(assetEditorForm, forceReregister: true)", beginInvokeIndex, StringComparison.Ordinal);
int fallbackIndex = source.IndexOf("RefreshMainWindowTaskbarRegistration(assetEditorForm)", forcedRegistrationIndex, StringComparison.Ordinal);
int runIndex = source.IndexOf("Application.Run(assetEditorForm)", StringComparison.Ordinal);

if (shownIndex < 0 || closeIndex < shownIndex || beginInvokeIndex < closeIndex ||
    forcedRegistrationIndex < beginInvokeIndex || fallbackIndex < forcedRegistrationIndex ||
    runIndex < fallbackIndex)
{
    Console.Error.WriteLine("FAIL: splash does not hand off to a deferred forced taskbar registration with one fallback.");
    return 1;
}

string shownHandler = source.Substring(shownIndex, runIndex - shownIndex);
int directRegistrationIndex = shownHandler.IndexOf("RegisterMainWindowTaskbarButton(assetEditorForm)", StringComparison.Ordinal);
if (directRegistrationIndex >= 0 && directRegistrationIndex < shownHandler.IndexOf("splash.Close();", StringComparison.Ordinal))
{
    Console.Error.WriteLine("FAIL: main taskbar registration occurs before the splash closes.");
    return 1;
}

if (!source.Contains("forceReregister") ||
    !source.Contains("form.ShowInTaskbar = false") ||
    !source.Contains("form.ShowInTaskbar = true") ||
    !source.Contains("WS_EX_APPWINDOW") ||
    !source.Contains("WS_EX_TOOLWINDOW") ||
    !source.Contains("SWP_FRAMECHANGED") ||
    source.Contains("SetForegroundWindow(") ||
    source.Contains("BringToFront()") ||
    source.Contains("assetEditorForm.Activate()") ||
    source.Contains("assetEditorForm.Focus()"))
{
    Console.Error.WriteLine("FAIL: taskbar repair is missing required style refresh or introduces focus stealing.");
    return 1;
}

Console.WriteLine("PASS: splash closes before bounded, non-activating main taskbar re-registration.");
return 0;
```

- [ ] **Step 2: Run the repro and verify RED**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/startup-taskbar-handoff/StartupTaskbarHandoffRepro.csproj"
```

Expected: exit code 1 with `FAIL: splash does not hand off to a deferred forced taskbar registration with one fallback.`

---

### Task 2: Restore The Durable Post-Splash Handoff

**Files:**
- Modify: `AssetEditor/AssetEditor/Program.cs:35-83,507-519`

**Interfaces:**
- Consumes: WinForms `Form`, `ShowInTaskbar`, `BeginInvoke`, Win32 `GetWindowLong`, `SetWindowLong`, `SetWindowPos`, and existing `ITaskbarList`
- Produces: `RefreshMainWindowTaskbarRegistration(Form form, bool forceReregister = false)`

- [ ] **Step 1: Add constants and native APIs**

Inside `Program`, add:

```csharp
private const int GWL_EXSTYLE = -20;
private const int WS_EX_TOOLWINDOW = 0x00000080;
private const int WS_EX_APPWINDOW = 0x00040000;
private const uint SWP_NOSIZE = 0x0001;
private const uint SWP_NOMOVE = 0x0002;
private const uint SWP_NOZORDER = 0x0004;
private const uint SWP_NOACTIVATE = 0x0010;
private const uint SWP_FRAMECHANGED = 0x0020;

[DllImport("user32.dll")]
private static extern int GetWindowLong(IntPtr hWnd, int nIndex);

[DllImport("user32.dll")]
private static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

[DllImport("user32.dll", SetLastError = true)]
private static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, uint flags);
```

- [ ] **Step 2: Replace the single-purpose shell helper**

Rename `RegisterMainWindowTaskbarButton(Form form)` to:

```csharp
private static void RefreshMainWindowTaskbarRegistration(Form form, bool forceReregister = false)
```

At the beginning of the method, add:

```csharp
if (form == null || form.IsDisposed || form.Disposing)
{
	return;
}

if (forceReregister)
{
	form.ShowInTaskbar = false;
	form.ShowInTaskbar = true;
}

IntPtr handle = form.Handle;
int exStyle = GetWindowLong(handle, GWL_EXSTYLE);
SetWindowLong(handle, GWL_EXSTYLE, (exStyle | WS_EX_APPWINDOW) & ~WS_EX_TOOLWINDOW);
SetWindowPos(handle, IntPtr.Zero, 0, 0, 0, 0,
	SWP_NOMOVE | SWP_NOSIZE | SWP_NOZORDER | SWP_NOACTIVATE | SWP_FRAMECHANGED);
```

Keep the existing `ITaskbarList.HrInit()` and `AddTab(handle)` logic after the style refresh, changing `form.Handle` to the captured `handle`.

- [ ] **Step 3: Replace the Shown handoff sequence**

In the main form `Shown` handler, remove:

```csharp
RegisterMainWindowTaskbarButton(assetEditorForm);
splash.Close();
```

Replace it with:

```csharp
splash.Close();
assetEditorForm.BeginInvoke((Action)delegate
{
	if (assetEditorForm.IsDisposed || assetEditorForm.Disposing)
	{
		return;
	}
	RefreshMainWindowTaskbarRegistration(assetEditorForm, forceReregister: true);
	assetEditorForm.BeginInvoke((Action)delegate
	{
		RefreshMainWindowTaskbarRegistration(assetEditorForm);
	});
});
```

Keep the existing statistics dump after scheduling the handoff.

- [ ] **Step 4: Run the repro and verify GREEN**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/startup-taskbar-handoff/StartupTaskbarHandoffRepro.csproj"
```

Expected: exit code 0 with `PASS: splash closes before bounded, non-activating main taskbar re-registration.`

- [ ] **Step 5: Build Release x64**

Run:

```powershell
rtk dotnet msbuild "AssetEditor/AssetEditor.csproj" -t:Build -p:Configuration=Release -p:Platform=x64 -p:PostBuildEvent= -p:RunPostBuildEvent=Never
```

Expected: build succeeds with 0 errors. Existing repository warnings are acceptable.

- [ ] **Step 6: Inspect exact scope**

Run:

```powershell
rtk git diff --check
rtk git status --short
rtk git diff -- "AssetEditor/AssetEditor/Program.cs" "docs/superpowers/repros/startup-taskbar-handoff/Program.cs"
```

Expected: taskbar changes are limited to the two approved files and this plan. Existing AST changes remain present but untouched and unstaged.

- [ ] **Step 7: Deploy and hash-verify the executable**

Confirm no `AssetEditor` process is running. Copy:

```text
AssetEditor/bin/x64/Release/net462/AssetEditor.exe
```

to:

```text
E:\SteamLibrary\steamapps\common\Sid Meier's Civilization VI SDK\AssetModTools\AssetEditor\AssetEditor.exe
```

Compute SHA-256 for both files and require identical hashes after deployment.

- [ ] **Step 8: Verify real startup behavior**

1. Start Asset Editor normally and confirm taskbar presence transitions from splash to main window without a gap.
2. Close Asset Editor and confirm no orphan taskbar entry or process remains.
3. Cover Asset Editor with another application, start Asset Editor, and keep the other application foregrounded until the splash closes.
4. Confirm the main Asset Editor taskbar button exists without Asset Editor stealing foreground focus.
5. Repeat normal and covered startup at least three times each.

Expected: every startup preserves the main taskbar button after splash closure and does not activate Asset Editor over the covering application.

- [ ] **Step 9: Commit only after user verification**

Run:

```powershell
rtk git add -- "AssetEditor/AssetEditor/Program.cs" "docs/superpowers/repros/startup-taskbar-handoff/Program.cs" "docs/superpowers/plans/2026-07-15-startup-taskbar-handoff.md"
rtk git diff --cached --check
rtk git diff --cached --name-status
rtk git commit -m "fix: preserve taskbar button after splash"
```

Expected: exactly the two implementation/repro files and this plan are staged. The AST source files and AST plan remain unstaged.
