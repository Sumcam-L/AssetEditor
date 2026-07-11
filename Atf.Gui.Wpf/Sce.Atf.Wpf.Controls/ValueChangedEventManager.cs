using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;

namespace Sce.Atf.Wpf.Controls;

public class ValueChangedEventManager : WeakEventManager
{
	private class ValueChangedRecord
	{
		private PropertyDescriptor m_pd;

		private ValueChangedEventManager m_manager;

		private WeakReference m_source;

		private ListenerList m_listeners = new ListenerList();

		private ValueChangedEventArgs m_eventArgs;

		public bool IsEmpty => m_listeners.IsEmpty;

		public ValueChangedRecord(ValueChangedEventManager manager, object source, PropertyDescriptor pd)
		{
			m_manager = manager;
			m_source = new WeakReference(source);
			m_pd = pd;
			m_eventArgs = new ValueChangedEventArgs(pd);
			pd.AddValueChanged(source, OnValueChanged);
		}

		public void Add(IWeakEventListener listener)
		{
			ListenerList.PrepareForWriting(ref m_listeners);
			m_listeners.Add(listener);
		}

		public void Remove(IWeakEventListener listener)
		{
			ListenerList.PrepareForWriting(ref m_listeners);
			m_listeners.Remove(listener);
			if (m_listeners.IsEmpty)
			{
				StopListening();
			}
		}

		public bool Purge()
		{
			ListenerList.PrepareForWriting(ref m_listeners);
			return m_listeners.Purge();
		}

		public void StopListening()
		{
			if (m_source != null && m_source.IsAlive)
			{
				m_pd.RemoveValueChanged(m_source.Target, OnValueChanged);
			}
			m_source = null;
		}

		private void OnValueChanged(object sender, EventArgs e)
		{
			using (m_manager.ReadLock)
			{
				m_listeners.BeginUse();
			}
			try
			{
				m_manager.DeliverEventToList(sender, m_eventArgs, m_listeners);
			}
			finally
			{
				m_listeners.EndUse();
			}
		}
	}

	private static ValueChangedEventManager CurrentManager
	{
		get
		{
			Type typeFromHandle = typeof(ValueChangedEventManager);
			ValueChangedEventManager valueChangedEventManager = (ValueChangedEventManager)WeakEventManager.GetCurrentManager(typeFromHandle);
			if (valueChangedEventManager == null)
			{
				valueChangedEventManager = new ValueChangedEventManager();
				WeakEventManager.SetCurrentManager(typeFromHandle, valueChangedEventManager);
			}
			return valueChangedEventManager;
		}
	}

	public static void AddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateAddListener(source, listener, pd);
	}

	public static void RemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (listener == null)
		{
			throw new ArgumentNullException("listener");
		}
		CurrentManager.PrivateRemoveListener(source, listener, pd);
	}

	protected override void StartListening(object source)
	{
	}

	protected override void StopListening(object source)
	{
	}

	protected override bool Purge(object source, object data, bool purgeAll)
	{
		bool result = false;
		HybridDictionary hybridDictionary = (HybridDictionary)data;
		ICollection keys = hybridDictionary.Keys;
		PropertyDescriptor[] array = new PropertyDescriptor[keys.Count];
		keys.CopyTo(array, 0);
		for (int num = array.Length - 1; num >= 0; num--)
		{
			bool flag = purgeAll || source == null;
			ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[array[num]];
			if (!flag)
			{
				if (valueChangedRecord.Purge())
				{
					result = true;
				}
				flag = valueChangedRecord.IsEmpty;
			}
			if (flag)
			{
				valueChangedRecord.StopListening();
				if (!purgeAll)
				{
					hybridDictionary.Remove(array[num]);
				}
			}
		}
		if (hybridDictionary.Count == 0)
		{
			result = true;
			if (source != null)
			{
				Remove(source);
			}
		}
		return result;
	}

	private ValueChangedEventManager()
	{
	}

	private void PrivateAddListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				hybridDictionary = (HybridDictionary)(base[source] = new HybridDictionary());
			}
			ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[pd];
			if (valueChangedRecord == null)
			{
				valueChangedRecord = (ValueChangedRecord)(hybridDictionary[pd] = new ValueChangedRecord(this, source, pd));
			}
			valueChangedRecord.Add(listener);
			ScheduleCleanup();
		}
	}

	private void PrivateRemoveListener(object source, IWeakEventListener listener, PropertyDescriptor pd)
	{
		using (base.WriteLock)
		{
			HybridDictionary hybridDictionary = (HybridDictionary)base[source];
			if (hybridDictionary == null)
			{
				return;
			}
			ValueChangedRecord valueChangedRecord = (ValueChangedRecord)hybridDictionary[pd];
			if (valueChangedRecord != null)
			{
				valueChangedRecord.Remove(listener);
				if (valueChangedRecord.IsEmpty)
				{
					hybridDictionary.Remove(pd);
				}
			}
			if (hybridDictionary.Count == 0)
			{
				Remove(source);
			}
		}
	}
}
