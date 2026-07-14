# Asset Browser Gray Theme Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make the docked Asset Browser consistently use the active application skin, including its bottom details grid, instead of falling back to white WPF defaults.

**Architecture:** Keep the existing WPF skin conversion and dynamic `SystemColors` resource system. Apply the root view's skin after XAML loading through the same `Loaded`/`Unloaded` lifecycle already used by its child filter views, and bind the details grid's visual surfaces to converted system brushes.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WPF XAML, WinForms `ElementHost`, MEF/context-provided `IWpfSkinService`, MSBuild

## Global Constraints

- Support dark, light, and custom skins; do not hard-code dark replacement colors.
- Preserve Asset Browser filtering, batching, virtualization, selection, drag, and semantic-color behavior.
- Modify only `AssetBrowserView.cs`, `AssetBrowserView.xaml`, and this implementation plan.
- Do not modify, stage, or commit the existing unrelated `TimelineEditorControl.cs` worktree change.
- Do not change global `SkinService`, `WpfSkinService`, or skin conversion behavior.

---

### Task 1: Apply the Active Skin After XAML Construction

**Files:**
- Modify: `Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs:1-44`
- Modify: `Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml:2-3,292`

**Interfaces:**
- Consumes: `Firaxis.MVVMBase.IWpfSkinService`, `Firaxis.Utility.Context.GetService<T>()`, `IWpfSkinService.ApplySkin(FrameworkElement)`, and `IWpfSkinService.RemoveSkin(FrameworkElement)`
- Produces: `AssetBrowserView.UserControl_Loaded(object, RoutedEventArgs)` and `AssetBrowserView.UserControl_Unloaded(object, RoutedEventArgs)` lifecycle handlers

- [ ] **Step 1: Record the failing source assertions**

Run this read-only PowerShell assertion before editing:

```powershell
rtk proxy pwsh -NoProfile -Command '$xaml = Get-Content -Raw "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml"; $code = Get-Content -Raw "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs"; if ($xaml -notmatch ''aba:SkinServiceHelper.ApplySkin="True"'' -or $code -match ''UserControl_Loaded'' -or $xaml -match ''Name="selectedEntityDetails"[^>]*RowBackground='') { throw "Expected pre-fix Asset Browser theme state was not found" }'
```

Expected: command succeeds, confirming that the root still uses construction-time skin application, has no loaded handler, and the details grid has no explicit row background.

- [ ] **Step 2: Replace construction-time root skin application with lifecycle handlers**

In `AssetBrowserView.xaml`, remove only the root attached property `aba:SkinServiceHelper.ApplySkin="True"`, retain the `aba` namespace because the filtered entity list still uses it, and add:

```xaml
Loaded="UserControl_Loaded" Unloaded="UserControl_Unloaded"
```

The root declaration must remain equivalent to:

```xaml
<UserControl x:Class="Firaxis.AssetBrowser.Views.AssetBrowserView" xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation" xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml" xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006" xmlns:d="http://schemas.microsoft.com/expression/blend/2008" xmlns:abvm="clr-namespace:Firaxis.AssetBrowser.ViewModels" xmlns:mvvma="clr-namespace:Firaxis.MVVMBase.Attached;assembly=Firaxis.MVVMBase" xmlns:mvvmc="clr-namespace:Firaxis.MVVMBase.Controls;assembly=Firaxis.MVVMBase" xmlns:aba="clr-namespace:Firaxis.AssetBrowser.Attached" Background="{DynamicResource {x:Static SystemColors.ControlBrushKey}}" Margin="5" Loaded="UserControl_Loaded" Unloaded="UserControl_Unloaded" xmlns:views="clr-namespace:Firaxis.AssetBrowser.Views">
```

In `AssetBrowserView.cs`, add these imports:

```csharp
using System.Windows;
using Firaxis.MVVMBase;
using Firaxis.Utility;
```

Add these methods to `AssetBrowserView`:

```csharp
private void UserControl_Loaded(object sender, RoutedEventArgs e)
{
	Context.GetService<IWpfSkinService>()?.ApplySkin(this);
}

private void UserControl_Unloaded(object sender, RoutedEventArgs e)
{
	Context.GetService<IWpfSkinService>()?.RemoveSkin(this);
}
```

- [ ] **Step 3: Bind the details grid to dynamic skin brushes**

Replace the opening `selectedEntityDetails` element with the following attributes while preserving its existing columns and bindings:

```xaml
<DataGrid Name="selectedEntityDetails" AutoGenerateColumns="False" HeadersVisibility="None" GridLinesVisibility="None" IsReadOnly="True" BorderThickness="1" MinHeight="75" Background="{DynamicResource {x:Static SystemColors.ControlBrushKey}}" Foreground="{DynamicResource {x:Static SystemColors.ControlTextBrushKey}}" RowBackground="{DynamicResource {x:Static SystemColors.ControlBrushKey}}" AlternatingRowBackground="{DynamicResource {x:Static SystemColors.ControlLightBrushKey}}" AlternationCount="2" mvvmc:AutoAdjustingStackPanel.ChildWidth="2*" mvvmc:AutoAdjustingStackPanel.ChildHeight="2*" ItemsSource="{Binding SelectedEntityInfo, Mode=OneWay}">
```

Do not add fixed hexadecimal background or foreground colors.

- [ ] **Step 4: Run post-edit source assertions**

Run:

```powershell
rtk proxy pwsh -NoProfile -Command '$xaml = Get-Content -Raw "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml"; $code = Get-Content -Raw "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs"; if ($xaml -notmatch ''Loaded="UserControl_Loaded"'' -or $xaml -notmatch ''Unloaded="UserControl_Unloaded"'' -or $code -notmatch ''GetService<IWpfSkinService>\(\)\?\.ApplySkin\(this\)'' -or $code -notmatch ''GetService<IWpfSkinService>\(\)\?\.RemoveSkin\(this\)'' -or $xaml -notmatch ''Name="selectedEntityDetails"[^>]*RowBackground="\{DynamicResource \{x:Static SystemColors.ControlBrushKey\}\}"'' -or $xaml -notmatch ''Name="selectedEntityDetails"[^>]*AlternatingRowBackground="\{DynamicResource \{x:Static SystemColors.ControlLightBrushKey\}\}"'') { throw "Asset Browser theme implementation is incomplete" }'
```

Expected: command succeeds with no output.

- [ ] **Step 5: Build the Asset Browser in Release x64**

Run:

```powershell
rtk dotnet msbuild "Firaxis.AssetBrowser/Firaxis.AssetBrowser.csproj" -t:Build -p:Configuration=Release -p:Platform=x64 -p:PostBuildEvent= -p:RunPostBuildEvent=Never
```

Expected: build succeeds with 0 errors. Existing repository warnings are acceptable.

- [ ] **Step 6: Check scope and whitespace**

Run:

```powershell
rtk git diff --check
rtk git status --short
rtk git diff -- "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs" "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml"
```

Expected: no whitespace errors; only the two Asset Browser files and the pre-existing `TimelineEditorControl.cs` change are modified, while this plan is tracked separately. Confirm the Timeline file diff is not part of the Asset Browser implementation.

- [ ] **Step 7: Verify in the running application**

Launch the normal Release x64 Asset Editor using the project's established deployment path, then verify:

1. With `PickDarkTheme`, the Asset Browser root, filter area, toolbar area, entity list, and details grid are gray with readable text.
2. Select an entity and confirm both details-grid columns remain readable and no white row background appears.
3. Hide/show or recreate Asset Browser and confirm the gray skin remains without exceptions.
4. Switch to the light theme and back to the dark theme; confirm dynamic colors update both times.
5. Search rapidly, change filters, select rows, and start a drag; confirm recent debounce, incremental publication, selection, and drag behavior remain intact.

Expected: all checks pass. If the root remains white, stop and inspect `AssetBrowserView.Resources.MergedDictionaries` after `Loaded` rather than adding hard-coded colors.

- [ ] **Step 8: Commit only the Asset Browser implementation and plan**

Run:

```powershell
rtk git add -- "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs" "Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml" "docs/superpowers/plans/2026-07-14-asset-browser-gray-theme.md"
rtk git diff --cached --check
rtk git diff --cached --name-status
rtk git commit -m "fix: apply gray theme to asset browser"
```

Expected: the staged file list contains exactly the two Asset Browser files and this plan. It must not contain `Firaxis.AssetEditing/Firaxis.AssetEditing/TimelineEditorControl.cs`.
