using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public class NewAssetKnobSet : IKnobSet
{
	private EventHandler _003Cbacking_store_003EKnobSetChanged;

	private EventHandler _003Cbacking_store_003EKnobSetCleared;

	private IInstanceEntity m_pmAsset;

	private IList<IKnob> m_knobs;

	private KnobManager m_pmKnobManager;

	private string m_pmPreviewAssetName;

	private KnobGroupChangedEventHandler m_changedHandler;

	private KnobGroupClearedEventHandler m_clearedHandler;

	public virtual IDictionary<string, IKnobSubgroup> KnobsBySubgroup => global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetKnobsBySubgroup(m_knobs);

	public virtual IEnumerable<IKnob> Knobs => m_knobs;

	public virtual string KnobSetName => m_pmAsset.Name;

	[SpecialName]
	public virtual event EventHandler KnobSetCleared
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobSetCleared = (EventHandler)Delegate.Combine(_003Cbacking_store_003EKnobSetCleared, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobSetCleared = (EventHandler)Delegate.Remove(_003Cbacking_store_003EKnobSetCleared, value);
		}
	}

	[SpecialName]
	public virtual event EventHandler KnobSetChanged
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobSetChanged = (EventHandler)Delegate.Combine(_003Cbacking_store_003EKnobSetChanged, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobSetChanged = (EventHandler)Delegate.Remove(_003Cbacking_store_003EKnobSetChanged, value);
		}
	}

	[SpecialName]
	protected virtual void raise_KnobSetChanged(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EKnobSetChanged?.Invoke(value0, value1);
	}

	[SpecialName]
	protected virtual void raise_KnobSetCleared(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EKnobSetCleared?.Invoke(value0, value1);
	}

	public NewAssetKnobSet(IInstanceEntity pmAsset, string pmKnobGroupName, KnobManager pmKnobManager)
	{
		m_pmAsset = pmAsset;
		m_pmKnobManager = pmKnobManager;
		m_changedHandler = HandleKnobGroupChangedEvent;
		m_clearedHandler = HandleKnobGroupClearedEvent;
		base._002Ector();
		m_knobs = pmKnobManager.GetKnobs(m_pmPreviewAssetName = pmKnobGroupName.Replace(newValue: m_pmPreviewAssetName = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EGetNewAssetName(pmAsset), oldValue: pmAsset.Name));
		pmKnobManager.KnobGroupChanged += m_changedHandler;
		pmKnobManager.KnobGroupCleared += m_clearedHandler;
	}

	private void _007ENewAssetKnobSet()
	{
		_0021NewAssetKnobSet();
		GC.SuppressFinalize(this);
	}

	private void _0021NewAssetKnobSet()
	{
		KnobManager pmKnobManager = m_pmKnobManager;
		if (pmKnobManager != null)
		{
			pmKnobManager.KnobGroupChanged -= m_changedHandler;
			m_pmKnobManager.KnobGroupCleared -= m_clearedHandler;
			m_pmKnobManager = null;
		}
		m_knobs = null;
	}

	public virtual IKnob FindKnobByName(string pmKnobName)
	{
		return global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002E_INTERNAL_002EFindKnobByName(pmKnobName, m_knobs);
	}

	private void HandleKnobGroupChangedEvent(string pmKnobGroupName)
	{
		if (pmKnobGroupName == m_pmPreviewAssetName)
		{
			raise_KnobSetCleared(this, EventArgs.Empty);
			m_knobs = m_pmKnobManager.GetKnobs(m_pmPreviewAssetName);
			raise_KnobSetChanged(this, EventArgs.Empty);
		}
	}

	private void HandleKnobGroupClearedEvent(string pmKnobGroupName)
	{
		if (!(pmKnobGroupName == m_pmPreviewAssetName))
		{
			return;
		}
		IList<IKnob> knobs = m_knobs;
		if (knobs != null && knobs.Count > 0)
		{
			raise_KnobSetCleared(this, EventArgs.Empty);
			if (m_knobs != null)
			{
				m_knobs = new List<IKnob>();
			}
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007ENewAssetKnobSet();
			return;
		}
		try
		{
			_0021NewAssetKnobSet();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~NewAssetKnobSet()
	{
		Dispose(A_0: false);
	}
}
