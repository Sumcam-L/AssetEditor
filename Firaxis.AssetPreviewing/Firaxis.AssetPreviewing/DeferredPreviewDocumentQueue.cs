using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;

namespace Firaxis.AssetPreviewing;

internal sealed class DeferredPreviewDocumentQueue<T> where T : class
{
	private readonly HashSet<T> m_pending = new HashSet<T>();

	public IEnumerable<T> PendingItems => m_pending.ToArray();

	public bool Enqueue(Control dispatcher, T item, Action<T> action)
	{
		if (dispatcher == null || dispatcher.IsDisposed || !dispatcher.IsHandleCreated ||
			item == null || action == null || !m_pending.Add(item))
		{
			return false;
		}

		dispatcher.BeginInvoke((Action)(() =>
		{
			if (m_pending.Remove(item))
			{
				action(item);
			}
		}));
		return true;
	}

	public bool Cancel(T item)
	{
		return item != null && m_pending.Remove(item);
	}

	public void Clear()
	{
		m_pending.Clear();
	}
}
