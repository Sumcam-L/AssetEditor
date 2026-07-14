# Asset Browser Filter Display Optimization Design

## Goal

Improve Asset Browser search and filter response by avoiding redundant filtering, publishing the first visible results earlier, and appending later results without repeatedly rebuilding the entire WPF list.

## Current Behavior

Every `FilterChanged` event immediately enqueues a full filter request. The worker drains queued requests before starting, but a request already executing cannot be cancelled. A stale request can therefore finish, clear the visible list, and start rebuilding pages before the newest request runs.

## Design

- Debounce every filter change for 150 milliseconds.
- Restart the debounce delay whenever another filter change arrives.
- After 150 milliseconds without another change, enqueue only the newest filter set.
- Assign a monotonically increasing generation to each filter change.
- Carry the generation with the queued request.
- Before publishing filtered IDs, replacing `FilteredEntities`, building a batch, or appending a batch, verify that the request generation is still current.
- Stale work may finish native filtering already in progress, but it must not modify visible results or start further page loads.
- Filter and sort the complete `EntityID` result before creating view models so final ordering remains unchanged.
- Build and publish an initial batch of 30 `InstanceEntityViewModel` objects so the first visible screen appears quickly.
- Build subsequent batches in groups of 100.
- Replace the current full `NotifyCollectionChangedAction.Reset` after every batch with range-add notifications that describe only the newly appended items.
- Marshal collection replacement and range additions to the WPF dispatcher. Background workers must not mutate the list currently bound to the UI.
- Check the generation before and after each batch build and again on the dispatcher before publishing it.
- Dispose view models built for a generation that becomes stale before publication.
- Dispose view models from the previous result after the latest generation replaces the visible collection, except objects still retained by the new result.
- Queue Perforce status work only after the initial batch is visible. Later batches continue using the existing backlog.
- Stop and dispose the debounce timer during `AssetBrowserViewModel.Dispose()` so no callback can enqueue work after shutdown.

## Scope

Production changes are limited to the Asset Browser view-model/collection boundary. A small range-notifying collection may be added under `Firaxis.AssetBrowser.ViewModels` if the existing collection types cannot publish multi-item additions correctly.

The following behavior remains unchanged:

- Filter semantics and sort order.
- Subsequent internal batch size of 100 assets.
- Thumbnail loading and caching.
- Perforce status semantics.
- Selection and auto-selection behavior.
- Final asset count and ordering.

## Verification

- Rapidly type several characters: only one filter request is published after input pauses for approximately 150 milliseconds.
- Start a slow filter, then change the text: the stale result never clears or replaces the latest list.
- Single filter changes still refresh after the debounce delay.
- The first 30 results appear before the remaining result set has been materialized.
- Each later batch generates an add notification rather than a reset notification.
- The final displayed count and ordering match the unoptimized filtering result.
- Rapid filter changes do not publish stale pages and do not leak stale view models.
- Perforce status updates begin after the initial batch is visible and still reach all published entities.
- Closing Asset Browser with a pending debounce callback causes no exception or later update.
- Build `Firaxis.AssetBrowser` in Release x64.
