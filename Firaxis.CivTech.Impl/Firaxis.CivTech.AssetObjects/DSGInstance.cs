using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class DSGInstance : InstanceEntity, IDSGInstance
{
	public unsafe DSGInstance(global::AssetObjects.DSGInstance* pkInstance, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)pkInstance, pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe DSGInstance(global::AssetObjects.InstanceSet* pkInstanceSet, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
		: base((global::AssetObjects.InstanceEntity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EPush_003Cclass_0020AssetObjects_003A_003ADSGInstance_003E(pkInstanceSet), pkDeserializer, pkSerializer, pkVirtualPantry)
	{
	}

	public unsafe virtual IEnumerable<string> GetAnimationSlots()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002Eanimation_begin((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDSGInstance_002Eanimation_end((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDSGInstance_002Eanimation_end((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator2)));
		}
		return list;
	}

	public unsafe virtual void AddAnimationSlot(string AnimationSlotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(AnimationSlotName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002EAddAnimationSlot((global::AssetObjects.DSGInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void RemoveAnimationSlot(string AnimationSlotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(AnimationSlotName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002ERemoveAnimationSlot((global::AssetObjects.DSGInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void ClearAnimationSlots()
	{
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002EClearAnimationSlots((global::AssetObjects.DSGInstance*)m_pkEntity);
	}

	public unsafe virtual IEnumerable<string> GetTimelineSlots()
	{
		List<string> list = new List<string>();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002Etimeline_begin((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDSGInstance_002Etimeline_end((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator2)))
		{
			do
			{
				IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
				string item = Marshal.PtrToStringAnsi(ptr);
				list.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDSGInstance_002Etimeline_end((global::AssetObjects.DSGInstance*)m_pkEntity, &const_iterator2)));
		}
		return list;
	}

	public unsafe virtual void AddTimelineSlot(string TimelineSlotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(TimelineSlotName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002EAddTimelineSlot((global::AssetObjects.DSGInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void RemoveTimelineSlot(string TimelineSlotName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(TimelineSlotName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002ERemoveTimelineSlot((global::AssetObjects.DSGInstance*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void ClearTimelineSlots()
	{
		global::_003CModule_003E.AssetObjects_002EDSGInstance_002EClearTimelineSlots((global::AssetObjects.DSGInstance*)m_pkEntity);
	}
}
