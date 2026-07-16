# AST Idle Page Prewarm Design

## Problem

Current post-first-frame runtime evidence contains seven complete AST opens:

- Total AST open time: 1073-1564 ms, median 1253 ms.
- File load time: median 0 ms.
- `AssetEditorControl` creation: 419-497 ms, median 469 ms.
- `AssetEditorControl.Bind`: 243-352 ms, median 296 ms.
- Document-host show: median 220 ms.

File I/O is not the limiting factor. Before the document can be registered and painted, `AssetEditorControl.Bind()` synchronously binds Properties, Attachments, Cook Params, Geometries, Animations, Particles, Behaviors, and Splines. Most of these document pages are not initially active.

For ordinary Unit ASTs, Geometry, Cook Params, and Attachments account for roughly 77 percent of bind time. Geometry is normally the initial page and must remain ready for the first frame. Cook Params and Attachments commonly perform work before the user can see them.

## Goals

- Reduce the time from AST open command to the first complete editor frame.
- Keep Properties and the actual initial inner page fully bound before the first frame.
- Move binding of initially hidden AST pages out of the synchronous open path.
- Prewarm hidden pages automatically after the first frame, one page per UI idle opportunity.
- If the user selects an unbound page before its idle prewarm runs, bind it synchronously before displaying its content.
- Preserve current AST page controls, DockContent instances, tab ordering, capability rules, property editing behavior, and layout behavior.

## Non-Goals

- Do not lazy-create or pool page controls in this change.
- Do not move WinForms, DockPanel, PropertyView, DomNode, or CivTech/native work to a worker thread.
- Do not defer native adapter initialization or `AssetAdapter.Update()`.
- Do not change saved inner-layout behavior.
- Do not restore aggressive property-control reuse that previously regressed Class dropdown and nested property editors.
- Do not retain inactive document windows, use delayed hiding, freeze redraw, show placeholders, or display loading UI.
- Do not apply this behavior to non-AST editors in this change.

## Selected Approach

Keep the existing `AssetEditorControl` constructor and its eight inner controls. Change only when the optional page controls receive their editing contexts.

The AST bind sequence becomes:

1. Resolve a stable capability snapshot for the current AST class.
2. Bind Properties synchronously.
3. Determine the page that must be active for the first frame using the existing fallback order.
4. Bind that initial page synchronously.
5. Record the remaining supported pages as pending, without calling their existing `Bind` methods.
6. Complete outer document registration and render the real AST first frame.
7. Schedule one pending page bind per UI idle opportunity.
8. Stop scheduling when all pages are bound, the control closes, or the bind generation becomes stale.

All existing page controls and DockContent shells remain present, so tab identity, icons, pane state, theme integration, and fallback selection continue to use the current structure.

## Page Categories

### Always Synchronous

- Properties: visible alongside the document page and required for normal editing.
- The initial active document page.

The initial page remains selected using the current preference order:

1. Geometries
2. Cook Params
3. Attachments
4. Animations
5. Particles
6. Behaviors
7. Splines
8. Properties

Only a supported and visible page may be selected.

### Pending Prewarm

Supported pages not selected as the initial page are placed in this prewarm order:

1. Cook Params
2. Attachments
3. Animations
4. Behaviors
5. Particles
6. Splines

Geometries is also pending only for an unusual AST where it is supported but another page is explicitly required as the initial page. Under current behavior, supported Geometry remains the normal initial page.

Unsupported pages remain hidden and are not queued.

## Binding State

Each optional page has one state for the current AST bind generation:

- `Unavailable`: the capability is absent; the DockContent remains hidden.
- `Pending`: supported but its existing page `Bind` method has not run.
- `Binding`: binding is in progress; reentrant visibility or idle notifications do not bind it again.
- `Bound`: page binding completed successfully.

State belongs to the current `AssetEditorControl` and current context generation. It does not pool controls or share state between documents.

A generation increments whenever `AssetEditorControl.Bind()` accepts a new context or the control starts unregistering/disposal. Idle callbacks capture the generation and exit when it no longer matches.

## Idle Prewarm

Prewarm runs on the WinForms UI thread through `Application.Idle`, following the project's existing UI-idle scheduling pattern.

Rules:

- At most one page is bound per idle callback.
- After one page completes, unsubscribe from the current idle callback and schedule the next opportunity rather than draining the whole queue in one turn.
- Do not run while the control is disposed, unregistering, lacks a context, or has a stale generation.
- Do not run if no pending page remains.
- Binding exceptions follow the existing synchronous bind error behavior; they are logged with the page and generation and must not leave the page in `Binding`.
- A page bound by user selection is removed from pending work before its bind begins.

The queue is local to `AssetEditorControl`; it must not use the close-cleanup queue because initialization errors and user-visible state have different handling requirements.

## User Selection

The existing inner DockPanel active-content path becomes the authoritative first-use boundary.

Before an unbound page becomes usable:

1. Resolve its page binding state.
2. If `Pending`, synchronously invoke the page's existing bind method.
3. Mark it `Bound` only after successful completion.
4. Remove it from idle prewarm.
5. Continue existing active-content, focus, property, and command routing.

This prevents a blank first page frame. Reentrant notifications while state is `Binding` do not start a second bind.

## Capability Snapshot

`AssetContext` currently performs repeated class database lookups through `HasGeometries`, `HasAnimations`, `HasParticleEffects`, `HasBehaviors`, and `HasSplines`. The bind operation will resolve those values once into an immutable per-generation snapshot, including Cook Params availability.

The snapshot controls page visibility, initial-page selection, and pending prewarm membership. A class or context reload starts a new generation and creates a new snapshot rather than mutating the active snapshot.

This is a supporting low-risk optimization and, more importantly, keeps all page-state decisions internally consistent during one bind.

## Reload, Class Change, and Close

### Reload or Rebind

- Increment the generation.
- Unsubscribe any pending idle callback.
- Clear the old pending queue.
- Recalculate capabilities.
- For a replacement context, rebind Properties and the new initial page synchronously.
- For `Reloaded` on the same context, do not rebind Properties because the event can occur while the Class Name dropdown is active.
- Preserve the current active optional page when it remains supported and bind that page synchronously; otherwise bind the normal fallback page.
- Rebuild pending prewarm for the new generation.

Existing page controls remain document-owned. Other supported optional pages enter the new pending generation and receive the current model state when their existing `Bind` method runs. Do not introduce a generic `Bind(null)` loop because optional page controls have different null contracts.

### Class Capability Change

- Recalculate page availability from the new class.
- Hide newly unsupported pages through `HideInnerDocument()`.
- A newly supported page enters `Pending` unless it becomes the required active page.
- If the current active page becomes unavailable, synchronously bind and activate the existing fallback page.

### Close and Dispose

- Mark the control unregistering before host removal through the existing unregister callback.
- Increment the generation and unsubscribe `Application.Idle`.
- Clear pending page work.
- Never bind a page after unregister or disposal begins.

## Error Handling

- Page binding uses the existing page-specific `Bind` methods; this change does not suppress their exceptions.
- On failure, restore the page to `Pending` only if the generation is still current and the document remains usable; otherwise discard the work.
- Log page name, trigger (`initial`, `idle`, `user`, `rebind`), generation, and elapsed time.
- User-triggered binding failure leaves the page unselected when possible and preserves the previous active page.
- Idle binding failure stops automatic prewarm for that page to avoid an infinite idle retry loop; later explicit user selection may retry once under the current generation.

## Diagnostics

Add AST-specific timing markers:

```text
AssetPageBind begin page=<name> trigger=<initial|idle|user|rebind> generation=<n>
AssetPageBind end page=<name> trigger=<...> generation=<n> elapsed=<ms>
AssetPageBind skip page=<name> reason=<bound|binding|unavailable|stale|unregistering>
AssetPagePrewarm scheduled generation=<n> pending=<count>
AssetPagePrewarm canceled generation=<n> reason=<rebind|unregister|dispose>
```

Retain existing `Bind timing` markers during rollout so current and new samples remain comparable.

## Testing

### Focused Repro

Extend or add an AST page-prewarm repro using real `AssetEditorControl` page controls and lightweight contexts where practical. Verify:

- Initial `Bind()` binds Properties and only the selected initial page.
- Supported hidden pages are pending after initial bind.
- One idle callback binds exactly one pending page.
- Repeated idle opportunities eventually bind all pending pages in the defined order.
- User selection binds a pending page immediately and removes it from idle work.
- Idle and user selection cannot bind the same page twice.
- Closing before idle execution prevents all pending binds.
- Rebind/reload invalidates old-generation work.
- No-Geometry AST synchronously binds the correct fallback page.
- Unsupported pages never bind.

### Existing Regressions

Keep these passing:

- `asset-editor-inner-active`
- `property-view-visibility`
- `control-host-document-virtualization`
- `dockpane-tab-switch`
- AST property editing matrix, especially Class dropdown and nested AttachmentPoint editors
- read-only AST transaction checks

### Manual Verification

- Open representative Unit AST and no-Geometry/Particle-heavy AST documents.
- Confirm the first frame contains Properties and the correct initial page.
- Immediately click each optional page before prewarm completes; no blank page appears.
- Leave a document idle and confirm later page switches are already warm.
- Open and immediately close an AST; no pending page bind runs after close.
- Reload and change Class where supported; page visibility and data remain correct.
- Switch between multiple AST documents while each has pending prewarm.

## Performance Acceptance

Compare at least ten same-session Unit AST opens before and after the change using median and P90.

Targets:

- Reduce median `InitializeContext bind` from approximately 296 ms to 100-150 ms.
- Reduce median complete AST open from approximately 1253 ms toward 1100 ms.
- Bind no more than the first-frame-required pages before `DocumentFirstFrame`.
- Perform no more than one hidden-page bind per UI idle opportunity.
- Introduce no regression in first optional-page display, property editing, document switching, close, reload, class change, or shutdown.

The targets are directional acceptance thresholds, not permission to weaken correctness. If measured savings are materially smaller, retain the change only if logs prove work moved out of the opening critical path without increasing user-visible stalls.
