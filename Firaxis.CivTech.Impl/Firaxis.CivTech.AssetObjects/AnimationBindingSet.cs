using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationBindingSet : IAnimationBindingSet
{
	private List<IAnimationBinding> m_pmBindings;

	private unsafe global::AssetObjects.AnimationBindingSet* m_pkBindingSet;

	public virtual IEnumerable<IAnimationBinding> Bindings => new List<IAnimationBinding>(m_pmBindings);

	private unsafe IAnimationBinding FindBinding(global::AssetObjects.AnimationBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			List<IAnimationBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					AnimationBinding animationBinding = (AnimationBinding)enumerator.Current;
					if (animationBinding.GetAssetObject() == pkBinding)
					{
						return animationBinding;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	public unsafe virtual IAnimationBinding FindBinding(string slotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		IAnimationBinding result = FindBinding(global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002EFindBinding(m_pkBindingSet, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	private unsafe IAnimationBinding Bind(global::AssetObjects.AnimationBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			IAnimationBinding animationBinding = FindBinding(pkBinding);
			if (animationBinding == null)
			{
				animationBinding = new AnimationBinding(pkBinding);
				m_pmBindings.Add(animationBinding);
			}
			return animationBinding;
		}
		return null;
	}

	public unsafe virtual IAnimationBinding Bind(string slotName, string animationName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(animationName).ToPointer();
		IAnimationBinding result = Bind(global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002EBind(m_pkBindingSet, ptr, ptr2));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
		return result;
	}

	private unsafe void Unbind(sbyte* szSlotName)
	{
		AnimationBinding animationBinding = (AnimationBinding)FindBinding(global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002EFindBinding(m_pkBindingSet, szSlotName));
		if (animationBinding != null)
		{
			animationBinding.RemoveReferences();
			m_pmBindings.Remove(animationBinding);
			global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002EUnbind(m_pkBindingSet, szSlotName);
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

	public unsafe AnimationBindingSet(global::AssetObjects.AnimationBindingSet* pkBindingSet)
	{
		m_pmBindings = new List<IAnimationBinding>();
		m_pkBindingSet = pkBindingSet;
		base._002Ector();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Ebegin(m_pkBindingSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Eend(m_pkBindingSet, &iterator2)))
		{
			do
			{
				m_pmBindings.Add(new AnimationBinding(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Eend(m_pkBindingSet, &iterator2)));
		}
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0042: Expected I, but got I8
		List<IAnimationBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AnimationBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		m_pkBindingSet = null;
	}

	internal unsafe void ClearBindings()
	{
		List<IAnimationBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AnimationBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002EClearBindings(m_pkBindingSet);
	}

	private unsafe void ResolveReferences()
	{
		List<IAnimationBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AnimationBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Ebegin(m_pkBindingSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Eend(m_pkBindingSet, &iterator2)))
		{
			do
			{
				m_pmBindings.Add(new AnimationBinding(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAnimationBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EAnimationBindingSet_002Eend(m_pkBindingSet, &iterator2)));
		}
	}
}
