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
        try
        {
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
        catch
        {
            CancelCore();
            throw;
        }
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
