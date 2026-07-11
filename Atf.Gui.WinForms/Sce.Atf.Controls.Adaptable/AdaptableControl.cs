using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Controls.Adaptable;

public class AdaptableControl : Control
{
	private readonly List<IControlAdapter> m_adapters = new List<IControlAdapter>();

	private readonly Dictionary<Type, object> m_adapterCache = new Dictionary<Type, object>();

	private readonly Dictionary<Type, object> m_contextAdapterPreferred = new Dictionary<Type, object>();

	private object m_context;

	private bool m_autoResetCursor = true;

	public IEnumerable<IControlAdapter> ControlAdapters => m_adapters;

	public bool HasKeyboardFocus { get; set; }

	public bool IsImeChar { get; set; }

	public bool AutoResetCursor
	{
		get
		{
			return m_autoResetCursor;
		}
		set
		{
			m_autoResetCursor = value;
		}
	}

	public object Context
	{
		get
		{
			return m_context;
		}
		set
		{
			if (m_context != value)
			{
				OnContextChanging(EventArgs.Empty);
				this.ContextChanging.Raise(this, EventArgs.Empty);
				m_context = value;
				OnContextChanged(EventArgs.Empty);
				this.ContextChanged.Raise(this, EventArgs.Empty);
				Invalidate();
			}
		}
	}

	public event EventHandler Adapting;

	public event EventHandler Adapted;

	public event EventHandler Painting;

	public event EventHandler ContextChanging;

	public event EventHandler ContextChanged;

	public AdaptableControl()
	{
		base.DoubleBuffered = true;
	}

	public void Adapt(params IControlAdapter[] adapters)
	{
		OnAdapting(EventArgs.Empty);
		this.Adapting.Raise(this, EventArgs.Empty);
		Context = null;
		foreach (IControlAdapter adapter in m_adapters)
		{
			adapter.Unbind(this);
			Dispose(adapter);
		}
		m_adapters.Clear();
		m_adapterCache.Clear();
		m_contextAdapterPreferred.Clear();
		m_adapters.AddRange(adapters);
		foreach (IControlAdapter adapter2 in m_adapters)
		{
			adapter2.Bind(this);
		}
		for (int num = m_adapters.Count - 1; num >= 0; num--)
		{
			m_adapters[num].BindReverse(this);
		}
		OnAdapted(EventArgs.Empty);
		this.Adapted.Raise(this, EventArgs.Empty);
		Invalidate();
	}

	protected virtual void OnAdapting(EventArgs e)
	{
	}

	protected virtual void OnAdapted(EventArgs e)
	{
	}

	protected override void Dispose(bool disposing)
	{
		foreach (IControlAdapter adapter in m_adapters)
		{
			Dispose(adapter);
		}
		base.Dispose(disposing);
	}

	private static void Dispose(IControlAdapter adapter)
	{
		if (adapter is IDisposable disposable)
		{
			disposable.Dispose();
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.Capture = false;
		if (AutoResetCursor)
		{
			Cursor = Cursors.Default;
		}
		Focus();
		base.OnMouseDown(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (AutoResetCursor)
		{
			Cursor = Cursors.Default;
		}
		base.OnMouseMove(e);
	}

	protected override void WndProc(ref Message m)
	{
		if (m.Msg == 15)
		{
			OnPainting(EventArgs.Empty);
			this.Painting.Raise(this, EventArgs.Empty);
		}
		if (m.Msg == 646)
		{
			IsImeChar = true;
		}
		else
		{
			IsImeChar = false;
		}
		base.WndProc(ref m);
	}

	protected virtual void OnPainting(EventArgs e)
	{
	}

	public T As<T>() where T : class
	{
		return As(typeof(T)) as T;
	}

	public T Cast<T>() where T : class
	{
		T val = As<T>();
		if (val == null)
		{
			throw new AdaptationException(typeof(T).Name + " adapter required");
		}
		return val;
	}

	public bool Is<T>() where T : class
	{
		return As<T>() != null;
	}

	private object As(Type type)
	{
		if (m_adapterCache.TryGetValue(type, out var value))
		{
			return value;
		}
		for (int num = m_adapters.Count - 1; num >= 0; num--)
		{
			value = m_adapters[num];
			if (type.IsAssignableFrom(value.GetType()))
			{
				m_adapterCache.Add(type, value);
				return value;
			}
		}
		if (type.IsAssignableFrom(GetType()))
		{
			m_adapterCache.Add(type, this);
			return this;
		}
		return null;
	}

	public object Cast(Type type)
	{
		object obj = As(type);
		if (obj == null)
		{
			throw new AdaptationException(type.Name + " adapter required");
		}
		return obj;
	}

	public bool Is(Type type)
	{
		return As(type) != null;
	}

	public IEnumerable<T> AsAll<T>() where T : class
	{
		foreach (IControlAdapter adapter in m_adapters)
		{
			T t = adapter.As<T>();
			if (t != null)
			{
				yield return t;
			}
		}
	}

	public T ContextAs<T>() where T : class
	{
		if (m_contextAdapterPreferred.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		return m_context.As<T>();
	}

	public T ContextCast<T>() where T : class
	{
		if (m_contextAdapterPreferred.TryGetValue(typeof(T), out var value))
		{
			return value as T;
		}
		return m_context.Cast<T>();
	}

	public bool ContextIs<T>() where T : class
	{
		return m_context.Is<T>();
	}

	public void RegisterContextAdapter(Type type, object adapter)
	{
		m_contextAdapterPreferred.Add(type, adapter);
	}

	protected virtual void OnContextChanging(EventArgs e)
	{
	}

	protected virtual void OnContextChanged(EventArgs e)
	{
	}
}
