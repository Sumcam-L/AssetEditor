# Asset Browser Filter Debounce Design

## Goal

Improve Asset Browser search and filter response by avoiding redundant full-data filtering while the user is still changing filter input.

## Current Behavior

Every `FilterChanged` event immediately enqueues a full filter request. The worker drains queued requests before starting, but a request already executing cannot be cancelled. A stale request can therefore finish, clear the visible list, and start rebuilding pages before the newest request runs.

## Design

- Debounce every filter change for 150 milliseconds.
- Restart the debounce delay whenever another filter change arrives.
- After 150 milliseconds without another change, enqueue only the newest filter set.
- Assign a monotonically increasing generation to each filter change.
- Carry the generation with the queued request.
- Before publishing filtered IDs, clearing `FilteredEntities`, loading a page, or publishing a page reset, verify that the request generation is still current.
- Stale work may finish native filtering already in progress, but it must not modify visible results or start further page loads.
- Stop and dispose the debounce timer during `AssetBrowserViewModel.Dispose()` so no callback can enqueue work after shutdown.

## Scope

The change is limited to `Firaxis.AssetBrowser.ViewModels/AssetBrowserViewModel.cs`.

The following behavior remains unchanged:

- Filter semantics and sort order.
- Page size of 100 assets.
- Progressive page display.
- Thumbnail loading and caching.
- Perforce status updates.
- Selection and auto-selection behavior.

## Verification

- Rapidly type several characters: only one filter request is published after input pauses for approximately 150 milliseconds.
- Start a slow filter, then change the text: the stale result never clears or replaces the latest list.
- Single filter changes still refresh after the debounce delay.
- Closing Asset Browser with a pending debounce callback causes no exception or later update.
- Build `Firaxis.AssetBrowser` in Release x64.
