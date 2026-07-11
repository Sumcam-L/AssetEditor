using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ParticleBindingSet : IParticleBindingSet
{
	private List<IParticleBinding> m_pmBindings;

	private unsafe global::AssetObjects.ParticleBindingSet* m_pkBindingSet;

	public virtual IEnumerable<IParticleBinding> Bindings => new List<IParticleBinding>(m_pmBindings);

	private unsafe IParticleBinding FindBinding(global::AssetObjects.ParticleBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			List<IParticleBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					ParticleBinding particleBinding = (ParticleBinding)enumerator.Current;
					if (particleBinding.GetAssetObject() == pkBinding)
					{
						return particleBinding;
					}
				}
				while (enumerator.MoveNext());
			}
		}
		return null;
	}

	public unsafe virtual IParticleBinding FindBinding(string slotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		IParticleBinding result = FindBinding(global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002EFindBinding(m_pkBindingSet, ptr));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	private unsafe IParticleBinding Bind(global::AssetObjects.ParticleBinding* pkBinding)
	{
		if (pkBinding != null)
		{
			IParticleBinding particleBinding = FindBinding(pkBinding);
			if (particleBinding == null)
			{
				particleBinding = new ParticleBinding(pkBinding);
				m_pmBindings.Add(particleBinding);
			}
			return particleBinding;
		}
		return null;
	}

	public unsafe virtual IParticleBinding Bind(string slotName, string animationName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(slotName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(animationName).ToPointer();
		IParticleBinding result = Bind(global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002EBind(m_pkBindingSet, ptr, ptr2));
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
		return result;
	}

	private unsafe void Unbind(sbyte* szSlotName)
	{
		ParticleBinding particleBinding = (ParticleBinding)FindBinding(global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002EFindBinding(m_pkBindingSet, szSlotName));
		if (particleBinding != null)
		{
			particleBinding.RemoveReferences();
			m_pmBindings.Remove(particleBinding);
			global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002EUnbind(m_pkBindingSet, szSlotName);
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

	public unsafe ParticleBindingSet(global::AssetObjects.ParticleBindingSet* pkBindingSet)
	{
		m_pmBindings = new List<IParticleBinding>();
		m_pkBindingSet = pkBindingSet;
		base._002Ector();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Ebegin(m_pkBindingSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Eend(m_pkBindingSet, &iterator2)))
		{
			do
			{
				m_pmBindings.Add(new ParticleBinding(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Eend(m_pkBindingSet, &iterator2)));
		}
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0042: Expected I, but got I8
		List<IParticleBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ParticleBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		m_pkBindingSet = null;
	}

	private unsafe void ResolveReferences()
	{
		List<IParticleBinding>.Enumerator enumerator = m_pmBindings.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ParticleBinding)enumerator.Current).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmBindings.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Ebegin(m_pkBindingSet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Eend(m_pkBindingSet, &iterator2)))
		{
			do
			{
				m_pmBindings.Add(new ParticleBinding(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AParticleBinding_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EParticleBindingSet_002Eend(m_pkBindingSet, &iterator2)));
		}
	}
}
