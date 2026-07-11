using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class EnvironmentLightInstance : ImportedEntity, IEnvironmentLightInstance
{
	private List<IEnvironmentLightDirectionTag> m_pmDirections;

	public virtual IEnumerable<IEnvironmentLightDirectionTag> DirectionTags => m_pmDirections;

	public unsafe EnvironmentLightInstance(global::AssetObjects.EnvironmentLightInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe EnvironmentLightInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003AEnvironmentLightInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe virtual IEnvironmentLightDirectionTag AddDirectionTag(float x, float y, float z)
	{
		EnvironmentLightDirectionTag environmentLightDirectionTag = new EnvironmentLightDirectionTag(global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002EAddDirectionTag((global::AssetObjects.EnvironmentLightInstance*)m_pkEntity, x, y, z));
		m_pmDirections.Add(environmentLightDirectionTag);
		return environmentLightDirectionTag;
	}

	public unsafe virtual void RemoveDirectionTag(IEnvironmentLightDirectionTag m_pmTag)
	{
		m_pmDirections.Remove(m_pmTag);
		global::AssetObjects.EnvironmentLightDirectionTag* unmanaged = ((EnvironmentLightDirectionTag)m_pmTag).GetUnmanaged();
		global::AssetObjects.EnvironmentLightInstance* pkEntity = (global::AssetObjects.EnvironmentLightInstance*)m_pkEntity;
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002ERemoveDirectionTag(pkEntity, unmanaged);
		uint num = 0u;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_begin(pkEntity, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_end(pkEntity, &iterator2)))
		{
			do
			{
				((EnvironmentLightDirectionTag)m_pmDirections[(int)num]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_002A(&iterator));
				num++;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_end(pkEntity, &iterator2)));
		}
	}

	public unsafe virtual void ClearDirectionTags()
	{
		//IL_0028: Expected I, but got I8
		List<IEnvironmentLightDirectionTag>.Enumerator enumerator = m_pmDirections.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((EnvironmentLightDirectionTag)enumerator.Current).SetUnmanaged(null);
			}
			while (enumerator.MoveNext());
		}
		m_pmDirections.Clear();
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002EClearDirectionTags((global::AssetObjects.EnvironmentLightInstance*)m_pkEntity);
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		m_pmDirections = new List<IEnvironmentLightDirectionTag>();
		global::AssetObjects.EnvironmentLightInstance* pkEntity = (global::AssetObjects.EnvironmentLightInstance*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_begin(pkEntity, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_end(pkEntity, &iterator2)))
		{
			do
			{
				global::AssetObjects.EnvironmentLightDirectionTag* pTag = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_002A(&iterator);
				m_pmDirections.Add(new EnvironmentLightDirectionTag(pTag));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AEnvironmentLightDirectionTag_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEnvironmentLightInstance_002Edirection_end(pkEntity, &iterator2)));
		}
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		base.RemoveReferences(bDisposing);
		m_pmDirections = null;
	}
}
