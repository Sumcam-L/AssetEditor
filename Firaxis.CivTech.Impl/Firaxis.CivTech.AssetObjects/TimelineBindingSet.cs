using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class TimelineBindingSet : ITimelineBindingSet
{
	private List<ITimelineBinding> m_pmBindings;

	private unsafe global::AssetObjects.TimelineBindingSet* m_pkBindingSet;

	public virtual IEnumerable<ITimelineBinding> Bindings => new List<ITimelineBinding>(m_pmBindings);

	private unsafe ITimelineBinding FindBinding(global::AssetObjects.TimelineBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			List<ITimelineBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					TimelineBinding timelineBinding = (TimelineBinding)enumerator.Current;
					if (timelineBinding.GetAssetObject() == pkBinding)
					{
						return timelineBinding;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	public unsafe virtual ITimelineBinding FindBinding(string slotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		ITimelineBinding result = FindBinding(global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002EFindBinding(m_pkBindingSet, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	private unsafe ITimelineBinding Bind(global::AssetObjects.TimelineBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			ITimelineBinding timelineBinding = FindBinding(pkBinding);
			if (timelineBinding == null)
			{
				timelineBinding = new TimelineBinding(pkBinding);
				m_pmBindings.Add(timelineBinding);
			}
			return timelineBinding;
		}
		return null;
	}

	public unsafe virtual ITimelineBinding Bind(string slotName, string animationName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(animationName).ToPointer();
		ITimelineBinding result = Bind(global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002EBind(m_pkBindingSet, ptr, ptr2));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
		return result;
	}

	private unsafe void Unbind(sbyte* szSlotName)
	{
		TimelineBinding timelineBinding = (TimelineBinding)FindBinding(global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002EFindBinding(m_pkBindingSet, szSlotName));
		if (timelineBinding != null)
		{
			timelineBinding.RemoveReferences();
			m_pmBindings.Remove(timelineBinding);
			global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002EUnbind(m_pkBindingSet, szSlotName);
			ResolveReferences();
		}
	}

	public unsafe virtual void Unbind(string slotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		Unbind(ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe TimelineBindingSet(global::AssetObjects.TimelineBindingSet* pkBindingSet)
	{
		m_pmBindings = new List<ITimelineBinding>();
		m_pkBindingSet = pkBindingSet;
		base._002Ector();
		ResolveReferences();
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0042: Expected I, but got I8
		List<ITimelineBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((TimelineBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		m_pkBindingSet = null;
	}

	internal unsafe void ClearBindings()
	{
		List<ITimelineBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((TimelineBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002EClearBindings(m_pkBindingSet);
	}

	private unsafe void ResolveReferences()
	{
		List<ITimelineBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((TimelineBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002Ebegin(m_pkBindingSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002Eend(m_pkBindingSet, &iterator2)))
		{
			do
			{
				m_pmBindings.Add(new TimelineBinding(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ATimelineBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ETimelineBindingSet_002Eend(m_pkBindingSet, &iterator2)));
		}
	}
}
