# Asset Browser Gray Theme Design

## Goal

Make the docked Asset Browser consistently use the active application skin instead of falling back to white WPF system colors. Keep support for dark, light, and custom skins rather than hard-coding dark colors.

## Current Behavior

The Asset Browser is a WPF `AssetBrowserView` hosted inside the WinForms docking system through `WpfContentHost` and `ElementHost`. Its XAML resolves most colors through dynamic `SystemColors` resources supplied by `WpfSkinService`.

The root view requests skin application through `SkinServiceHelper.ApplySkin` while XAML is still constructing the control. The root then declares its own resource dictionary. This ordering can leave the completed view without the converted skin resources, causing WPF to resolve `SystemColors.ControlBrushKey` to the Windows light default. The bottom details `DataGrid` also has no explicit theme-bound background or foreground, so its default presentation can remain white independently.

The current Asset Browser theme files and host code match the original copy. This is an inherited skin-application issue, not a regression from the recent filter-performance changes.

## Design

### Root Skin Lifetime

`AssetBrowserView` will apply the current `IWpfSkinService` after it receives `Loaded`, when XAML resource construction is complete. It will remove the skin dictionary on `Unloaded`.

This follows the existing lifecycle used by `StackFilterView` and `FilterSelectionView`. The root XAML attached-property request will be removed so the same element is not managed through both construction-time and loaded-time paths.

The existing attached skin application on the filtered entity `ListView` will remain. It gives the list a local resource scope for item-container templates and is not followed by a local resource-dictionary declaration that can replace the applied resources.

### Details Grid Colors

The bottom `selectedEntityDetails` `DataGrid` will explicitly reference dynamic system brushes for its background, foreground, row backgrounds, and alternating-row background. These resources are supplied by `WpfSkinService`, so dark, light, and custom skins remain supported.

No semantic colors will change. Selection highlighting, source-control states, thumbnails, buttons, and other status colors will retain their existing behavior.

## Scope

Files in scope:

- `Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views/AssetBrowserView.cs`
- `Firaxis.AssetBrowser/Firaxis.AssetBrowser.Views.AssetBrowserView.xaml`

Out of scope:

- Asset Browser filtering, batching, virtualization, and selection behavior
- Timeline Editor theming
- Global `SkinService`, `WpfSkinService`, and skin conversion behavior
- Hard-coded replacement of all Asset Browser semantic colors
- Redesign of WPF control templates

## Failure Handling

The view will continue to tolerate an unavailable `IWpfSkinService` by using a null-conditional service lookup, matching the existing filter views. In that case WPF retains its system-resource fallback rather than failing to open the Asset Browser.

Repeated load and unload cycles must not duplicate resource dictionaries. `WpfSkinService` already tracks skinned elements and removes the dictionary when requested.

## Verification

1. Build `Firaxis.AssetBrowser` in Release x64 configuration.
2. Open the application with the saved dark theme and confirm the docked Asset Browser root, list, toolbar area, filters, and details grid use gray backgrounds with readable text.
3. Hide/show or recreate the Asset Browser and confirm it remains skinned without duplicated resources or exceptions.
4. Switch between dark and light themes and confirm the Asset Browser updates through dynamic resources.
5. Confirm searching, filtering, incremental result publication, selection, details display, and drag operations behave as before.
6. Confirm the existing unrelated `TimelineEditorControl.cs` worktree change is not modified or included in this implementation.
