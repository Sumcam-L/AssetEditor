using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

internal class LightReferenceCollection : ILightReferenceCollection
{
	private unsafe global::AssetObjects.LightReferenceCollection* m_pUnmanaged;

	private List<ILightReference> m_pmAnalyticLightReferences;

	private List<ILightReference> m_pmEnvironmentLightReferences;

	public virtual IEnumerable<ILightReference> EnvironmentLightReferences => m_pmEnvironmentLightReferences;

	public virtual IEnumerable<ILightReference> AnalyticLightReferences => m_pmAnalyticLightReferences;

	public unsafe LightReferenceCollection(global::AssetObjects.LightReferenceCollection* pUnmanaged)
	{
		m_pUnmanaged = pUnmanaged;
		base._002Ector();
		m_pmAnalyticLightReferences = new List<ILightReference>();
		m_pmEnvironmentLightReferences = new List<ILightReference>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_begin(m_pUnmanaged, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_end(m_pUnmanaged, &iterator2)))
		{
			do
			{
				m_pmAnalyticLightReferences.Add(new LightReference(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002A(&iterator)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_end(m_pUnmanaged, &iterator2)));
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator3);
		global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_begin(m_pUnmanaged, &iterator3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator4);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_end(m_pUnmanaged, &iterator4)))
		{
			do
			{
				m_pmEnvironmentLightReferences.Add(new LightReference(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002A(&iterator3)));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_end(m_pUnmanaged, &iterator4)));
		}
	}

	public unsafe virtual ILightReference AddLightReference(ILightInstance light)
	{
		global::AssetObjects.InstanceEntity* assetObject = ((InstanceEntity)light).GetAssetObject();
		ILightReference lightReference;
		switch (global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetType(assetObject))
		{
		default:
			lightReference = null;
			break;
		case (global::AssetObjects.InstanceType)7u:
			lightReference = new LightReference(global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002EAddEnvironmentLight(m_pUnmanaged, global::_003CModule_003E.AssetObjects_002EEntity_002EGetName((Entity*)assetObject)));
			m_pmEnvironmentLightReferences.Add(lightReference);
			break;
		case (global::AssetObjects.InstanceType)6u:
			lightReference = new LightReference(global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002EAddAnalyticLight(m_pUnmanaged, global::_003CModule_003E.AssetObjects_002EEntity_002EGetName((Entity*)assetObject)));
			m_pmAnalyticLightReferences.Add(lightReference);
			break;
		}
		return lightReference;
	}

	public unsafe virtual void RemoveLightReference(ILightReference r)
	{
		global::AssetObjects.LightReference* unmanaged = ((LightReference)r).GetUnmanaged();
		if (m_pmEnvironmentLightReferences.Contains(r))
		{
			m_pmEnvironmentLightReferences.Remove(r);
			global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002ERemoveReference(m_pUnmanaged, unmanaged);
			uint num = 0u;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator);
			global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_begin(m_pUnmanaged, &iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_end(m_pUnmanaged, &iterator2)))
			{
				do
				{
					((LightReference)m_pmEnvironmentLightReferences[(int)num]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002A(&iterator));
					num++;
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eenvironment_lights_end(m_pUnmanaged, &iterator2)));
			}
			return;
		}
		if (m_pmAnalyticLightReferences.Contains(r))
		{
			m_pmAnalyticLightReferences.Remove(r);
			uint num2 = 0u;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator3);
			global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_begin(m_pUnmanaged, &iterator3);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E.iterator iterator4);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_end(m_pUnmanaged, &iterator4)))
			{
				do
				{
					((LightReference)m_pmAnalyticLightReferences[(int)num2]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002A(&iterator3));
					num2++;
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_002B_002B(&iterator3);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ALightReference_002C4096_003E_002Eiterator_002E_0021_003D(&iterator3, global::_003CModule_003E.AssetObjects_002ELightReferenceCollection_002Eanalytic_lights_end(m_pUnmanaged, &iterator4)));
			}
			return;
		}
		throw new Exception("Tried to remove non-existant light reference");
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_0094: Expected I, but got I8
		List<ILightReference> pmAnalyticLightReferences = m_pmAnalyticLightReferences;
		if (pmAnalyticLightReferences != null)
		{
			List<ILightReference>.Enumerator enumerator = pmAnalyticLightReferences.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					((LightReference)enumerator.Current).RemoveReferences();
				}
				while (enumerator.MoveNext());
			}
			m_pmAnalyticLightReferences.Clear();
		}
		List<ILightReference> pmEnvironmentLightReferences = m_pmEnvironmentLightReferences;
		if (pmEnvironmentLightReferences != null)
		{
			List<ILightReference>.Enumerator enumerator2 = pmEnvironmentLightReferences.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				do
				{
					((LightReference)enumerator2.Current).RemoveReferences();
				}
				while (enumerator2.MoveNext());
			}
			m_pmEnvironmentLightReferences.Clear();
		}
		m_pmAnalyticLightReferences = null;
		m_pmEnvironmentLightReferences = null;
		m_pUnmanaged = null;
	}
}
