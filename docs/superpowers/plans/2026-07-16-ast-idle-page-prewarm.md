# AST Idle Page Prewarm Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Reduce AST first-frame latency by synchronously binding only Properties and the initial inner page, then binding one supported hidden page per UI idle opportunity.

**Architecture:** Add a small UI-agnostic coordinator that owns per-generation page binding state, order, duplicate prevention, failure state, and cancellation. `AssetEditorControl` keeps all existing controls and DockContent shells, computes one capability snapshot, delegates page binds to the coordinator, starts prewarm only after its first paint, and synchronously claims a pending page when the user activates it.

**Tech Stack:** C# 11, .NET Framework 4.6.2, WinForms, Sony ATF property editing, WeifenLuo DockPanel Suite, Firaxis AST/CivTech contexts.

## Global Constraints

- Apply this optimization only to `AssetEditorControl` AST documents; do not change other entity editors.
- Keep Properties and the actual initial inner page fully bound before the first AST frame.
- Keep all existing page controls, DockContent instances, names, icons, tab order, and saved-layout behavior.
- Do not lazy-create, share, pool, or reuse page controls or property-row controls.
- Do not move WinForms, DockPanel, PropertyView, DomNode, or CivTech/native work to a worker thread.
- Do not defer `AssetAdapter.Update()` or native adapter initialization.
- Do not display loading UI, placeholders, blank pages, or constructor-sized intermediate frames.
- Do not use `Application.DoEvents()` in production code, redraw freezing, retained inactive documents, delayed hiding, or hidden-document pre-positioning.
- Bind at most one pending page per idle callback; use a 150 ms UI timer before subscribing for the next idle opportunity.
- Preserve existing Class dropdown, nested AttachmentPoint editors, read-only transaction behavior, first-frame priority, and Previewer deferral.
- Do not modify or revert unrelated working-tree changes.
- Do not commit or stage unless the user explicitly requests it.

## File Map

- Create `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs`: UI-agnostic per-generation page binding state machine.
- Modify `Firaxis.AssetEditing/Properties/AssemblyInfo.cs`: expose the coordinator only to its focused repro.
- Create `docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj`: focused coordinator executable.
- Create `docs/superpowers/repros/asset-page-idle-prewarm/Program.cs`: RED/GREEN coverage for initial bind, idle order, user priority, failures, rebind, and cancellation.
- Modify `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs`: capability snapshot, page registrations, first-paint scheduling, idle bridge, user-first binding, reload, and cleanup.
- Modify `docs/superpowers/repros/asset-editor-inner-active/Program.cs`: structural and visible-page integration assertions for the AST control.

---

### Task 1: Build the Page Binding Coordinator With TDD

**Files:**
- Create: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs`
- Modify: `Firaxis.AssetEditing/Properties/AssemblyInfo.cs`
- Create: `docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj`
- Create: `docs/superpowers/repros/asset-page-idle-prewarm/Program.cs`

**Interfaces:**
- Produces: `AssetPageBindingCoordinator.BeginGeneration(IEnumerable<Page>, object initialKey)`, `BindForUser(object)`, `BindNextIdle()`, `Cancel()`, `Generation`, `PendingCount`, `HasPending`, `GetState(object)`.
- Produces: `AssetPageBindingCoordinator.Page(string name, object key, Action<string> bind)` and `AssetPageBindingState` values `Pending`, `Binding`, `Bound`, `Failed`.
- Consumes: no WinForms, AST, DockPanel, or native types.

- [ ] **Step 1: Add the friend assembly and focused repro project**

Append to `Firaxis.AssetEditing/Properties/AssemblyInfo.cs`:

```csharp
[assembly: InternalsVisibleTo("AssetPageIdlePrewarmRepro")]
```

Create `docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj`:

```xml
<?xml version="1.0" encoding="utf-8"?>
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <OutputType>Exe</OutputType>
    <TargetFramework>net462</TargetFramework>
    <PlatformTarget>x64</PlatformTarget>
    <LangVersion>11.0</LangVersion>
    <AssemblyName>AssetPageIdlePrewarmRepro</AssemblyName>
  </PropertyGroup>
  <ItemGroup>
    <ProjectReference Include="..\..\..\..\Firaxis.AssetEditing\Firaxis.AssetEditing.csproj" />
  </ItemGroup>
</Project>
```

- [ ] **Step 2: Write the failing coordinator behavior repro**

Create `Program.cs`:

```csharp
using System;
using System.Collections.Generic;
using Firaxis.AssetEditing;

internal static class Program
{
    private static int Main()
    {
        var calls = new List<string>();
        var coordinator = new AssetPageBindingCoordinator();
        object geometry = new object();
        object cook = new object();
        object attachments = new object();

        int firstGeneration = coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Geometries", geometry, trigger => calls.Add("Geometry:" + trigger)),
            new AssetPageBindingCoordinator.Page("Cook Params", cook, trigger => calls.Add("Cook:" + trigger)),
            new AssetPageBindingCoordinator.Page("Attachments", attachments, trigger => calls.Add("Attachments:" + trigger))
        }, geometry);

        AssertSequence(calls, "Geometry:initial");
        Assert(coordinator.PendingCount == 2, "initial generation did not leave two hidden pages pending");

        Assert(coordinator.BindNextIdle(), "first idle did not bind one page");
        AssertSequence(calls, "Geometry:initial", "Cook:idle");
        Assert(coordinator.PendingCount == 1, "one idle callback bound more than one page");

        Assert(coordinator.BindForUser(attachments), "user did not claim pending page");
        AssertSequence(calls, "Geometry:initial", "Cook:idle", "Attachments:user");
        Assert(!coordinator.BindNextIdle(), "idle rebound a user-bound page");
        Assert(!coordinator.BindForUser(attachments), "user rebound an already-bound page");

        object failed = new object();
        bool fail = true;
        coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Initial", geometry, _ => { }),
            new AssetPageBindingCoordinator.Page("Failed", failed, _ =>
            {
                if (fail) throw new InvalidOperationException("expected");
                calls.Add("Failed:user");
            })
        }, geometry);
        try
        {
            coordinator.BindNextIdle();
            return Fail("idle binding exception was swallowed");
        }
        catch (InvalidOperationException ex) when (ex.Message == "expected")
        {
        }
        Assert(coordinator.GetState(failed) == AssetPageBindingState.Failed, "failed idle page did not leave Binding state");
        fail = false;
        Assert(coordinator.BindForUser(failed), "user could not retry an idle-failed page");
        Assert(coordinator.GetState(failed) == AssetPageBindingState.Bound, "user retry did not bind failed page");

        int staleGeneration = coordinator.Generation;
        coordinator.BeginGeneration(new[]
        {
            new AssetPageBindingCoordinator.Page("Replacement", cook, _ => calls.Add("Replacement:initial"))
        }, cook);
        Assert(coordinator.Generation > staleGeneration && coordinator.Generation > firstGeneration,
            "rebind did not advance generation");
        Assert(coordinator.GetState(geometry) == null, "old-generation page state survived rebind");

        coordinator.Cancel();
        Assert(!coordinator.HasPending && coordinator.PendingCount == 0, "cancel left pending work");
        Assert(!coordinator.BindNextIdle(), "canceled coordinator executed idle work");

        Console.WriteLine("PASS: AST page binding is ordered, single-step, user-prioritized, retryable, and cancellable.");
        return 0;
    }

    private static void AssertSequence(IList<string> actual, params string[] expected)
    {
        Assert(actual.Count == expected.Length, "unexpected bind count");
        for (int i = 0; i < expected.Length; i++)
            Assert(actual[i] == expected[i], "unexpected bind order at " + i + ": " + actual[i]);
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static int Fail(string message)
    {
        Console.Error.WriteLine("FAIL: " + message);
        return 1;
    }
}
```

- [ ] **Step 3: Run the repro and verify RED**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj"
```

Expected: compiler failure `CS0246` because `AssetPageBindingCoordinator` and `AssetPageBindingState` do not exist.

- [ ] **Step 4: Implement the minimal coordinator**

Create `AssetPageBindingCoordinator.cs`:

```csharp
using System;
using System.Collections.Generic;
using System.Linq;

namespace Firaxis.AssetEditing;

internal enum AssetPageBindingState
{
    Pending,
    Binding,
    Bound,
    Failed
}

internal sealed class AssetPageBindingCoordinator
{
    internal sealed class Page
    {
        public Page(string name, object key, Action<string> bind)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Bind = bind ?? throw new ArgumentNullException(nameof(bind));
        }

        public string Name { get; }
        public object Key { get; }
        public Action<string> Bind { get; }
        public AssetPageBindingState State { get; set; }
    }

    private readonly Dictionary<object, Page> m_pages = new Dictionary<object, Page>();
    private readonly List<Page> m_orderedPages = new List<Page>();
    private readonly Queue<Page> m_pending = new Queue<Page>();

    public int Generation { get; private set; }
    public int PendingCount => m_orderedPages.Count(page => page.State == AssetPageBindingState.Pending);
    public bool HasPending => PendingCount != 0;

    public int BeginGeneration(IEnumerable<Page> pages, object initialKey)
    {
        CancelCore();
        Generation++;
        foreach (Page page in pages)
        {
            page.State = AssetPageBindingState.Pending;
            m_pages.Add(page.Key, page);
            m_orderedPages.Add(page);
        }

        if (initialKey != null && m_pages.TryGetValue(initialKey, out Page initial))
        {
            Bind(initial, "initial", allowFailedRetry: false);
        }

        foreach (Page page in m_orderedPages)
        {
            if (page.State == AssetPageBindingState.Pending)
                m_pending.Enqueue(page);
        }
        return Generation;
    }

    public bool BindForUser(object key)
    {
        return key != null && m_pages.TryGetValue(key, out Page page) &&
            Bind(page, "user", allowFailedRetry: true);
    }

    public bool BindNextIdle()
    {
        while (m_pending.Count != 0)
        {
            Page page = m_pending.Dequeue();
            if (page.State == AssetPageBindingState.Pending)
                return Bind(page, "idle", allowFailedRetry: false);
        }
        return false;
    }

    public AssetPageBindingState? GetState(object key)
    {
        return key != null && m_pages.TryGetValue(key, out Page page) ? page.State : null;
    }

    public void Cancel()
    {
        CancelCore();
        Generation++;
    }

    private bool Bind(Page page, string trigger, bool allowFailedRetry)
    {
        if (page.State == AssetPageBindingState.Bound || page.State == AssetPageBindingState.Binding ||
            (page.State == AssetPageBindingState.Failed && !allowFailedRetry))
            return false;

        page.State = AssetPageBindingState.Binding;
        try
        {
            page.Bind(trigger);
            page.State = AssetPageBindingState.Bound;
            return true;
        }
        catch
        {
            page.State = AssetPageBindingState.Failed;
            throw;
        }
    }

    private void CancelCore()
    {
        m_pending.Clear();
        m_pages.Clear();
        m_orderedPages.Clear();
    }
}
```

- [ ] **Step 5: Run the coordinator repro and verify GREEN**

Run the same command. Expected exit 0:

```text
PASS: AST page binding is ordered, single-step, user-prioritized, retryable, and cancellable.
```

- [ ] **Step 6: Build and inspect**

Run:

```powershell
rtk dotnet build "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj"
rtk git diff --check -- "Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs" "Firaxis.AssetEditing/Properties/AssemblyInfo.cs" "docs/superpowers/repros/asset-page-idle-prewarm"
```

Expected: 0 errors and no whitespace errors.

---

### Task 2: Integrate Initial-Page-Only Binding Into AssetEditorControl

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs:20-159,329-350,422-498,522-715`
- Modify: `docs/superpowers/repros/asset-editor-inner-active/Program.cs`
- Test: `docs/superpowers/repros/asset-page-idle-prewarm/Program.cs`

**Interfaces:**
- Consumes: `AssetPageBindingCoordinator` from Task 1.
- Produces: one stable `PageCapabilities` snapshot per bind, synchronous Properties + initial page binding, and pending hidden pages in the required order.
- Produces: `BindPendingPageForUser(Firaxis.ATF.DockContent)` used by inner active/visible events.

- [ ] **Step 1: Add a failing structural integration assertion**

In `asset-editor-inner-active/Program.cs`, add after construction-log checks:

```csharp
string sourcePath = Path.GetFullPath(Path.Combine(
    AppDomain.CurrentDomain.BaseDirectory, "..", "..", "..", "..", "..", "..", "..",
    "Firaxis.AssetEditing", "Firaxis.AssetEditing", "AssetEditorControl.cs"));
string source = File.ReadAllText(sourcePath);
if (source.IndexOf("AssetPageBindingCoordinator", StringComparison.Ordinal) < 0 ||
    source.IndexOf("BindNextIdle", StringComparison.Ordinal) < 0 ||
    source.IndexOf("BindPendingPageForUser", StringComparison.Ordinal) < 0)
{
    Console.Error.WriteLine("FAIL: AssetEditorControl does not use deferred page binding boundaries.");
    return 1;
}
```

Run `asset-editor-inner-active`; expected failure with that message.

- [ ] **Step 2: Add page capability and coordinator fields**

Inside `AssetEditorControl`, add:

```csharp
private readonly struct PageCapabilities
{
    public PageCapabilities(IAssetEditorContext context)
    {
        CookParameters = context?.HasCookParameters == true;
        Geometries = context?.HasGeometries == true;
        Animations = context?.HasAnimations == true;
        Particles = context?.HasParticleEffects == true;
        Behaviors = context?.HasBehaviors == true;
        Splines = context?.HasSplines == true;
    }

    public bool CookParameters { get; }
    public bool Geometries { get; }
    public bool Animations { get; }
    public bool Particles { get; }
    public bool Behaviors { get; }
    public bool Splines { get; }
}

private readonly AssetPageBindingCoordinator m_pageBindings = new AssetPageBindingCoordinator();
private PageCapabilities m_pageCapabilities;
private bool m_configuringPageBindings;
private bool m_firstPaintCompleted;
```

- [ ] **Step 3: Separate page visibility from page data binding**

Add:

```csharp
private void ApplyPageCapabilities(PageCapabilities capabilities)
{
    SetPageAvailable(m_cookParameterSetEditor, capabilities.CookParameters);
    SetPageAvailable(m_geometrySetEditor, capabilities.Geometries);
    SetPageAvailable(m_attachmentEditor, available: true);
    SetPageAvailable(m_animationSetEditor, capabilities.Animations);
    SetPageAvailable(m_particleEffectSetEditor, capabilities.Particles);
    SetPageAvailable(m_behaviorSetEditor, capabilities.Behaviors);
    SetPageAvailable(m_splineSetEditor, capabilities.Splines);
}

private void SetPageAvailable(Control control, bool available)
{
    if (available)
        ShowInnerDocument(control);
    else
        HideInnerDocument(control);
}

private Control GetInitialPage(PageCapabilities capabilities)
{
    if (capabilities.Geometries) return m_geometrySetEditor;
    if (capabilities.CookParameters) return m_cookParameterSetEditor;
    return m_attachmentEditor;
}
```

Attachments remains the first guaranteed document page when Geometry and Cook Params are unavailable, matching the existing fallback order.

- [ ] **Step 4: Register supported pages in prewarm order**

Add:

```csharp
private IEnumerable<AssetPageBindingCoordinator.Page> CreatePageBindings(PageCapabilities capabilities)
{
    if (capabilities.Geometries)
        yield return CreatePageBinding("Geometries", m_geometrySetEditor, BindGeometrySet);
    if (capabilities.CookParameters)
        yield return CreatePageBinding("Cook Params", m_cookParameterSetEditor, BindCookParameters);

    yield return CreatePageBinding("Attachments", m_attachmentEditor, BindAttachments);
    if (capabilities.Animations)
        yield return CreatePageBinding("Animations", m_animationSetEditor, BindAnimationSet);
    if (capabilities.Behaviors)
        yield return CreatePageBinding("Behaviors", m_behaviorSetEditor, BindBehaviorSet);
    if (capabilities.Particles)
        yield return CreatePageBinding("Particles", m_particleEffectSetEditor, BindParticleEffectSet);
    if (capabilities.Splines)
        yield return CreatePageBinding("Splines", m_splineSetEditor, BindSplineEditor);
}

private AssetPageBindingCoordinator.Page CreatePageBinding(string name, Control control, Action bind)
{
    return new AssetPageBindingCoordinator.Page(name, control, trigger => BindPage(name, trigger, bind));
}

private void BindPage(string name, string trigger, Action bind)
{
    var timer = Stopwatch.StartNew();
    int generation = m_pageBindings.Generation;
    PaintTimingLog.Write("AssetPageBind begin page={0} trigger={1} generation={2}", name, trigger, generation);
    try
    {
        bind();
    }
    finally
    {
        PaintTimingLog.Write("AssetPageBind end page={0} trigger={1} generation={2} elapsed={3}ms",
            name, trigger, generation, timer.ElapsedMilliseconds);
    }
}

private void BindAttachments()
{
    m_attachmentEditor.Bind(m_context?.AttachmentsContext);
}
```

The yielded order puts Geometry first only so it can be selected as the initial page. After `BeginGeneration` consumes Geometry, the remaining pending order is Cook Params, Attachments, Animations, Behaviors, Particles, Splines.

- [ ] **Step 5: Replace eager optional-page binds in `Bind()`**

After detaching the old context, cancel old prewarm before assigning the new context. Do not introduce a generic `Bind(null)` loop: `ModelInstanceStateEditor.Bind` does not accept null, while each existing page binder already owns its replacement-context subscription behavior. Then replace the eager optional binds with:

```csharp
m_context = (IAssetEditorContext)context;
m_firstPaintCompleted = IsHandleCreated && Visible;
m_propertyEditor.Bind(m_context?.EntityContext);

m_configuringPageBindings = true;
try
{
    m_pageCapabilities = new PageCapabilities(m_context);
    ApplyPageCapabilities(m_pageCapabilities);
    Control initialPage = GetInitialPage(m_pageCapabilities);
    m_pageBindings.BeginGeneration(CreatePageBindings(m_pageCapabilities), initialPage);
    if (m_dockContent.TryGetValue(initialPage, out Firaxis.ATF.DockContent initialContent) &&
        initialContent.DockHandler.Pane != null)
    {
        initialContent.Activate();
        m_lastActiveInnerContent = initialContent;
    }
}
finally
{
    m_configuringPageBindings = false;
}
```

Retain `ScheduleEnsureActiveInnerContent()`, context `Reloaded` subscription, exception logging, and the existing total Bind timing. Remove the seven eager optional bind calls and their old per-step timing from the initial path; the new `AssetPageBind` markers replace them.

For `context == null`, bind the Properties editor to null, skip `BeginGeneration`, apply an empty capability snapshot, hide capability-gated pages, and leave no pending work. Do not call `ModelInstanceStateEditor.Bind(null)`.

- [ ] **Step 6: Bind a user-selected pending page synchronously**

Add:

```csharp
private void BindPendingPageForUser(Firaxis.ATF.DockContent dockContent)
{
    if (m_configuringPageBindings || dockContent == null || dockContent.Controls.Count == 0 ||
        m_disposing || m_controlHostUnregistering || IsDisposed)
        return;

    m_pageBindings.BindForUser(dockContent.Controls[0]);
}
```

Call it at the beginning of both active visibility boundaries:

```csharp
private void InnerDockPanel_ActiveContentChanged(object sender, EventArgs e)
{
    Firaxis.ATF.DockContent active = m_dockPanel.ActiveContent as Firaxis.ATF.DockContent;
    BindPendingPageForUser(active);
    // existing body follows
}
```

And inside the active-document branch of `InnerDockContent_VisibleChanged`, call `BindPendingPageForUser(dockContent)` before assigning `m_lastActiveInnerContent`.

- [ ] **Step 7: Use the snapshot in fallback visibility checks**

Replace repeated capability getters in `RestoreDefaultDockStates()` with `m_pageCapabilities`. Do not change the existing preferred control order in `TryActivateFallbackInnerContent()`.

- [ ] **Step 8: Run focused coordinator and inner-active repros**

Run serially:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
```

Expected: both exit 0 and print `PASS`.

---

### Task 3: Add First-Paint Idle Scheduling and Lifecycle Cancellation

**Files:**
- Modify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs:69-81,161-175,291-327,706-755`
- Modify: `docs/superpowers/repros/asset-page-idle-prewarm/Program.cs`

**Interfaces:**
- Consumes: `AssetPageBindingCoordinator.HasPending`, `BindNextIdle()`, `Cancel()`, and `Generation`.
- Produces: `SchedulePagePrewarmAfterFirstPaint()`, `PagePrewarm_Idle`, `PagePrewarmTimer_Tick`, and `CancelPagePrewarm(string)`.

- [ ] **Step 1: Extend the focused repro with an isolated one-step scheduling scenario**

Add this scenario after the existing cancellation assertions so it does not consume the queue used by the order test:

```csharp
var oneStepCalls = new List<string>();
object oneStepInitial = new object();
var oneStep = new AssetPageBindingCoordinator();
oneStep.BeginGeneration(new[]
{
    new AssetPageBindingCoordinator.Page("Initial", oneStepInitial, _ => oneStepCalls.Add("Initial")),
    new AssetPageBindingCoordinator.Page("One", new object(), _ => oneStepCalls.Add("One")),
    new AssetPageBindingCoordinator.Page("Two", new object(), _ => oneStepCalls.Add("Two"))
}, oneStepInitial);
int beforeIdle = oneStepCalls.Count;
Assert(oneStep.BindNextIdle(), "one-step scheduler did not bind pending work");
Assert(oneStepCalls.Count == beforeIdle + 1, "one scheduler turn did not bind exactly one page");
Assert(oneStep.PendingCount == 1, "one scheduler turn drained all pending pages");
```

This is characterization coverage because Task 1 already established single-step behavior before the WinForms idle bridge exists. It fails if a later coordinator change drains all pending work in one call.

- [ ] **Step 2: Add idle bridge fields and helpers**

Add fields:

```csharp
private bool m_pagePrewarmIdleSubscribed;
private Timer m_pagePrewarmTimer;
```

Add methods:

```csharp
private void SchedulePagePrewarmAfterFirstPaint()
{
    if (!m_firstPaintCompleted || m_pagePrewarmIdleSubscribed || !m_pageBindings.HasPending ||
        m_disposing || m_controlHostUnregistering || IsDisposed)
        return;

    Application.Idle += PagePrewarm_Idle;
    m_pagePrewarmIdleSubscribed = true;
    PaintTimingLog.Write("AssetPagePrewarm scheduled generation={0} pending={1}",
        m_pageBindings.Generation, m_pageBindings.PendingCount);
}

private void PagePrewarm_Idle(object sender, EventArgs e)
{
    UnsubscribePagePrewarmIdle();
    if (m_disposing || m_controlHostUnregistering || IsDisposed)
        return;

    try
    {
        m_pageBindings.BindNextIdle();
    }
    catch (System.Exception ex)
    {
        PaintTimingLog.Write("AssetPagePrewarm failed generation={0} error={1}: {2}",
            m_pageBindings.Generation, ex.GetType().Name, ex.Message);
    }

    if (!m_pageBindings.HasPending)
        return;

    if (m_pagePrewarmTimer == null)
    {
        m_pagePrewarmTimer = new Timer { Interval = 150 };
        m_pagePrewarmTimer.Tick += PagePrewarmTimer_Tick;
    }
    m_pagePrewarmTimer.Stop();
    m_pagePrewarmTimer.Start();
}

private void PagePrewarmTimer_Tick(object sender, EventArgs e)
{
    m_pagePrewarmTimer.Stop();
    SchedulePagePrewarmAfterFirstPaint();
}

private void UnsubscribePagePrewarmIdle()
{
    if (!m_pagePrewarmIdleSubscribed) return;
    Application.Idle -= PagePrewarm_Idle;
    m_pagePrewarmIdleSubscribed = false;
}

private void CancelPagePrewarm(string reason)
{
    m_pagePrewarmTimer?.Stop();
    UnsubscribePagePrewarmIdle();
    m_pageBindings.Cancel();
    PaintTimingLog.Write("AssetPagePrewarm canceled generation={0} reason={1}",
        m_pageBindings.Generation, reason);
}
```

- [ ] **Step 3: Start prewarm only after the first real paint**

At the end of `OnPaint`, after `base.OnPaint(e)`:

```csharp
if (!m_firstPaintCompleted)
{
    m_firstPaintCompleted = true;
    SchedulePagePrewarmAfterFirstPaint();
}
```

At the end of successful `Bind()`, call `SchedulePagePrewarmAfterFirstPaint()`. It no-ops before the first paint. A rebind on an already visible, handle-created control sets `m_firstPaintCompleted` to true in Task 2, so its new generation does not wait for an unrelated future repaint.

- [ ] **Step 4: Cancel stale work at every lifecycle boundary**

Call `CancelPagePrewarm`:

```csharp
public void BeforeControlHostUnregister()
{
    m_controlHostUnregistering = true;
    CancelPagePrewarm("unregister");
    // existing log
}
```

At the start of every `Bind()` before replacing context:

```csharp
CancelPagePrewarm("rebind");
```

At the start of `Dispose(bool disposing)` after setting `m_disposing = true`:

```csharp
CancelPagePrewarm("dispose");
if (m_pagePrewarmTimer != null)
{
    m_pagePrewarmTimer.Tick -= PagePrewarmTimer_Tick;
    m_pagePrewarmTimer.Dispose();
    m_pagePrewarmTimer = null;
}
```

- [ ] **Step 5: Route reload through a new optional-page generation without rebinding Properties**

Extract the optional-page configuration from `Bind()` into:

```csharp
private void ConfigurePageBindings(bool preserveActivePage)
{
    CancelPagePrewarm("configure");
    m_pageCapabilities = new PageCapabilities(m_context);
    ApplyPageCapabilities(m_pageCapabilities);
    Control initialPage = preserveActivePage ? GetCurrentSupportedPage() : null;
    initialPage ??= GetInitialPage(m_pageCapabilities);
    m_pageBindings.BeginGeneration(CreatePageBindings(m_pageCapabilities), initialPage);
    ActivateInitialPage(initialPage);
}
```

`GetCurrentSupportedPage()` returns the control hosted by the current active inner DockContent only when that control is present in the new supported-page set; otherwise it returns null. `ActivateInitialPage()` contains the existing guarded pane activation logic from Task 2.

Implement both helpers explicitly:

```csharp
private Control GetCurrentSupportedPage()
{
    Firaxis.ATF.DockContent active = m_dockPanel.ActiveContent as Firaxis.ATF.DockContent;
    if (active == null || active.Controls.Count == 0)
        return null;

    Control control = active.Controls[0];
    return IsSupportedPage(control) ? control : null;
}

private bool IsSupportedPage(Control control)
{
    return control == m_attachmentEditor ||
        (control == m_geometrySetEditor && m_pageCapabilities.Geometries) ||
        (control == m_cookParameterSetEditor && m_pageCapabilities.CookParameters) ||
        (control == m_animationSetEditor && m_pageCapabilities.Animations) ||
        (control == m_particleEffectSetEditor && m_pageCapabilities.Particles) ||
        (control == m_behaviorSetEditor && m_pageCapabilities.Behaviors) ||
        (control == m_splineSetEditor && m_pageCapabilities.Splines);
}

private void ActivateInitialPage(Control initialPage)
{
    if (initialPage == null || !m_dockContent.TryGetValue(initialPage, out Firaxis.ATF.DockContent content))
        return;

    if (content.DockHandler.Pane != null)
    {
        if (content.DockHandler.Pane.ActiveContent != content)
            content.DockHandler.Pane.ActiveContent = content;
        content.Activate();
    }
    else
    {
        content.Show(m_dockPanel, DockState.Document);
    }
    m_lastActiveInnerContent = content;
}
```

Call `ConfigurePageBindings(preserveActivePage: false)` from the normal replacement-context `Bind()` after Properties binding. Replace `AssetContext_Reloaded()` with:

```csharp
private void AssetContext_Reloaded(object sender, EventArgs e)
{
    if (m_context == null || m_disposing || m_controlHostUnregistering || IsDisposed)
        return;

    ConfigurePageBindings(preserveActivePage: true);
    ScheduleEnsureActiveInnerContent();
    SchedulePagePrewarmAfterFirstPaint();
}
```

This invalidates old idle work and recalculates capabilities without rebinding `m_propertyEditor`, which can be actively hosting the Class Name dropdown that triggered the reload. The current active optional page remains synchronous when supported; other pages enter the new pending generation.

- [ ] **Step 6: Run lifecycle-focused repros**

Run serially:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/property-view-visibility/PropertyViewVisibilityRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
```

Expected: all five print `PASS` and exit 0.

- [ ] **Step 7: Build AssetEditing and inspect lifecycle subscriptions**

Run:

```powershell
rtk dotnet build "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj"
rtk git diff --check -- "Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs" "Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs" "docs/superpowers/repros/asset-page-idle-prewarm" "docs/superpowers/repros/asset-editor-inner-active/Program.cs"
```

Expected: 0 errors, no whitespace errors, and every `Application.Idle += PagePrewarm_Idle` has a matching unsubscribe in idle execution, rebind, unregister, and dispose paths.

---

### Task 4: Full Regression and Runtime Performance Verification

**Files:**
- Verify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs`
- Verify: `Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs`
- Verify: `docs/superpowers/repros/asset-page-idle-prewarm/Program.cs`
- Verify: `docs/superpowers/repros/asset-editor-inner-active/Program.cs`

**Interfaces:**
- Consumes: completed coordinator and AST integration.
- Produces: fresh automated/build evidence and same-session AST timing comparison.

- [ ] **Step 1: Run all directly affected repros serially**

Run:

```powershell
rtk dotnet run --project "docs/superpowers/repros/asset-page-idle-prewarm/AssetPageIdlePrewarmRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/asset-editor-inner-active/AssetEditorInnerActiveRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/property-view-visibility/PropertyViewVisibilityRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/control-host-document-virtualization/ControlHostDocumentVirtualizationRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/dockpane-tab-switch/DockPaneTabSwitchRepro.csproj"
rtk dotnet run --project "docs/superpowers/repros/previewer-deferred-document-work/PreviewerDeferredDocumentWorkRepro.csproj"
```

Expected: all six print `PASS` and exit 0.

- [ ] **Step 2: Verify read-only/property editing coverage boundaries**

Run the available property-view repro again after the full integration:

```powershell
rtk dotnet run --project "docs/superpowers/repros/property-view-visibility/PropertyViewVisibilityRepro.csproj"
```

Expected: `PASS`. The repository does not currently contain executable repro projects for AST read-only transactions, Class dropdown interaction, or nested AttachmentPoint editing; those remain required manual checks in Step 4 and must not be claimed as automated coverage.

- [ ] **Step 3: Build deployment dependencies serially**

Run:

```powershell
rtk dotnet build "Firaxis.AssetEditing/Firaxis.AssetEditing.csproj"
rtk dotnet build "AssetEditor/AssetEditor.csproj"
```

Expected: both exit 0 with 0 errors. Record pre-existing warnings separately.

- [ ] **Step 4: Verify representative AST behavior manually**

Open a Unit AST, `ActivatedClutter.ast`, and an AST without Geometry where available. Confirm:

- First frame contains Properties and the correct initial page.
- No loading, blank, or small intermediate page appears.
- Immediately selecting any optional page never shows empty content.
- Leaving the AST idle makes later page switches warm.
- Rapid open/open and open/close do not execute pending binds on stale controls.
- Reload and Class changes update page visibility and current data.
- Read-only Class dropdown and nested AttachmentPoint controls retain their current behavior.

- [ ] **Step 5: Capture and compare at least ten Unit AST opens**

Use a fresh `paint_timing.log`. For every open, record:

- `InitializeContext create`
- synchronous `InitializeContext bind`
- initial `AssetPageBind`
- each idle/user `AssetPageBind`
- `DocumentFirstFrame`
- complete `Opened document`

Calculate median and P90. Acceptance:

- Median synchronous bind is 100-150 ms, down from approximately 296 ms.
- Median complete open trends toward 1100 ms from approximately 1253 ms.
- Only Properties and the initial page bind before `DocumentFirstFrame`.
- No idle callback binds more than one hidden page.

If savings are materially smaller, inspect marker attribution before retaining further complexity; do not claim success from a single fastest sample.

- [ ] **Step 6: Final repository checks**

Run:

```powershell
rtk git diff --check
rtk git status --short
rtk git diff -- "Firaxis.AssetEditing/Firaxis.AssetEditing/AssetPageBindingCoordinator.cs" "Firaxis.AssetEditing/Firaxis.AssetEditing/AssetEditorControl.cs" "Firaxis.AssetEditing/Properties/AssemblyInfo.cs" "docs/superpowers/repros/asset-page-idle-prewarm" "docs/superpowers/repros/asset-editor-inner-active/Program.cs" "docs/superpowers/specs/2026-07-16-ast-idle-page-prewarm-design.md" "docs/superpowers/plans/2026-07-16-ast-idle-page-prewarm.md"
```

Expected: no whitespace errors; unrelated pre-existing changes remain untouched; no production `Application.DoEvents`, worker-thread UI/native work, page-control pooling, or loading UI.
