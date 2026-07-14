# Asset Browser Filter Display Optimization Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Make Asset Browser filtering publish only the latest request, show the first 30 results quickly, and append later results without rebuilding the full WPF list.

**Architecture:** Keep filtering and view-model construction on background workers, but assign every filter change a generation and publish only the current generation. Use a WPF `DispatcherTimer` for 150ms debounce and marshal collection replacement/appends to the application dispatcher.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WPF, `DispatcherTimer`, existing `AssetBrowserViewModel` filter thread.

## Global Constraints

- Preserve filter semantics, final asset count, and sort order.
- Initial internal batch size is exactly 30; subsequent batch size is exactly 100.
- Do not change XAML, thumbnail generation, selection behavior, or Perforce status semantics.
- Do not mutate the WPF-bound list from a background thread.
- A stale generation must never replace, clear, or append visible results.
- Dispose stale unpublished view models and stop the debounce timer during shutdown.

---

### Task 1: Debounce and Generation Control

**Files:**
- Modify: `Firaxis.AssetBrowser/Firaxis.AssetBrowser.ViewModels/AssetBrowserViewModel.cs`

**Interfaces:**
- Produces: `AssetBrowserFilterRequest.Generation`, `_filterGeneration`, and a 150ms `_filterDebounceTimer`.
- Consumes: existing `FilterRequests`, `FilterThreadSignal`, and `FilterVM.GetFilterSet()`.

- [ ] Add a `long Generation` constructor argument/property to `AssetBrowserFilterRequest`.
- [ ] Add constants `InitialBatchSize = 30`, `BatchSize = 100`, and `FilterDebounceMilliseconds = 150`.
- [ ] Add `_filterGeneration`, `_filterDebounceTimer`, and `_pendingFilterSet` fields protected by the existing `locker`.
- [ ] Initialize a UI-thread `DispatcherTimer` in the constructor. Its tick stops the timer, snapshots `_pendingFilterSet` and `_filterGeneration`, and calls `EnqueueFilterRequest(filterSet, generation)`.
- [ ] Replace `HandleFilterChanged()` with generation increment, pending filter replacement, and timer restart via `ApplicationHelper.BeginInvokeIfNeeded`.
- [ ] Route the initial `FilterVM` request through the same debounce method after construction instead of immediately enqueueing from the property setter.
- [ ] Make `FilterThreadCall()` skip a dequeued request when `request.Generation != Interlocked.Read(ref _filterGeneration)` before and after native filtering/sorting.
- [ ] Stop the timer, clear pending state, and increment generation at the start of `Dispose()`.
- [ ] Build Release x64 and verify zero errors.
- [ ] Commit with `perf: debounce asset browser filters`.

### Task 2: Incremental First-Batch Publication

**Files:**
- Modify: `Firaxis.AssetBrowser/Firaxis.AssetBrowser.ViewModels/AssetBrowserViewModel.cs`

**Interfaces:**
- Consumes: Task 1 generation fields and current-generation checks.
- Produces: first batch of 30, subsequent batches of 100, dispatcher-only list mutation, and no per-batch `Reset`.

- [ ] Remove `currentPageIndex`, `_loadedEntities`, `PopulateEntities()`, `FetchRange()`, `LoadPage()`, `LoadPageWork()`, `FireCollectionReset()`, and the page-advancing body of `OnCollectionChanged()`.
- [ ] Add `IsCurrentGeneration(long generation)` using `Interlocked.Read` and `m_filterThreadRunning`.
- [ ] Change `BuildFilteredEntityCollection` to accept a generation and stop constructing when it becomes stale.
- [ ] Add `DisposeViewModels(IEnumerable<InstanceEntityViewModel>)` for unpublished stale batches and replaced results.
- [ ] Add `PublishInitialBatch(long generation, IList<InstanceEntityViewModel> batch)` that synchronously marshals to the dispatcher, rechecks generation, replaces `FilteredEntities`, raises `OnPropertyChanged("FilteredEntities")`, and queues Perforce status after the list is visible.
- [ ] Add `PublishAdditionalBatch(long generation, IList<InstanceEntityViewModel> batch)` that marshals once to the dispatcher, rechecks generation, appends each item to the existing list, and raises `CollectionChanged` with one single-item `Add` event per item. Do not raise `Reset`.
- [ ] In `FilterThreadCall`, split sorted IDs into one 30-item batch followed by 100-item batches. Build/publish sequentially and stop immediately when stale.
- [ ] When a new initial batch replaces the old visible collection, dispose the old view models after replacement.
- [ ] Ensure stale built batches are disposed if dispatcher publication rejects them.
- [ ] Build Release x64 and verify zero errors.
- [ ] Manually verify rapid typing, first-batch display, smooth append, correct final count/order, Perforce updates, and clean close.
- [ ] Commit with `perf: stream asset browser filter results`.

### Task 3: Final Verification and Deployment

**Files:**
- Verify: `Firaxis.AssetBrowser/Firaxis.AssetBrowser.ViewModels/AssetBrowserViewModel.cs`

- [ ] Run `rtk git diff --check`.
- [ ] Run `rtk dotnet msbuild "Firaxis.AssetBrowser/Firaxis.AssetBrowser.csproj" -t:Build -p:Configuration=Release -p:Platform=x64 -p:PostBuildEvent= -p:RunPostBuildEvent=Never` and confirm zero errors.
- [ ] Confirm no AssetEditor process is running, deploy `Firaxis.AssetBrowser.dll`, and verify source/deployment SHA-256 equality.
- [ ] In Asset Browser, rapidly enter at least five characters and confirm the list updates once after the pause.
- [ ] Change the filter again while results are still appending and confirm no stale result appears.
- [ ] Confirm the first screen appears before the full result count finishes appending.
- [ ] Confirm final count/order, thumbnails, selection, and Perforce columns match previous behavior.
- [ ] Close Asset Browser while a debounce is pending and confirm no exception.
