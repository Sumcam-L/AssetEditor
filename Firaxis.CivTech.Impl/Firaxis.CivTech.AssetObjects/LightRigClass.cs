using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class LightRigClass : ClassEntity, ILightRigClass
{
	public unsafe virtual IEnumerable<string> AllowedTriggerClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Etrigger_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Etrigger_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Etrigger_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedLightClasses
	{
		get
		{
			global::AssetObjects.LightRigClass* pkEntity = (global::AssetObjects.LightRigClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002ELightRigClass_002Elit_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Elit_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Elit_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedDSGClasses
	{
		get
		{
			global::AssetObjects.LightRigClass* pkEntity = (global::AssetObjects.LightRigClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002ELightRigClass_002Edsg_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Edsg_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Edsg_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedAnimationClasses
	{
		get
		{
			global::AssetObjects.LightRigClass* pkEntity = (global::AssetObjects.LightRigClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002ELightRigClass_002Eanm_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Eanm_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002ELightRigClass_002Eanm_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe LightRigClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ALightRigClass_003E(pkClassSet), pkDeserializer)
	{
	}

	public unsafe LightRigClass(global::AssetObjects.LightRigClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
		: base((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer)
	{
	}

	public unsafe virtual void AllowLightClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002ELightRigClass_002EAllowLightClass((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowAnimationClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002ELightRigClass_002EAllowAnimationClass((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowDSGClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002ELightRigClass_002EAllowDSGClass((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowTriggerClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowTriggerClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void ClearAllowedClasses()
	{
		global::_003CModule_003E.AssetObjects_002ELightRigClass_002EClearAllowedClasses((global::AssetObjects.LightRigClass*)m_pkEntity);
	}

	public virtual IEnumerable<string> GetAllowedClasses(InstanceType entityType)
	{
		switch (entityType)
		{
		case InstanceType.IT_ANALYTIC_LIGHT:
		case InstanceType.IT_ENVIRONMENT_LIGHT:
			return AllowedLightClasses;
		default:
			return null;
		case InstanceType.IT_DSG:
			return AllowedDSGClasses;
		case InstanceType.IT_ANIMATION:
			return AllowedAnimationClasses;
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsClassAllowed(string className, InstanceType entityType)
	{
		switch (entityType)
		{
		case InstanceType.IT_ANALYTIC_LIGHT:
		case InstanceType.IT_ENVIRONMENT_LIGHT:
			return IsLightClassAllowed(className);
		default:
			return false;
		case InstanceType.IT_DSG:
			return IsDSGClassAllowed(className);
		case InstanceType.IT_ANIMATION:
			return IsAnimationClassAllowed(className);
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool AllowClass(string className, InstanceType entityType)
	{
		switch (entityType)
		{
		case InstanceType.IT_ANALYTIC_LIGHT:
		case InstanceType.IT_ENVIRONMENT_LIGHT:
			AllowLightClass(className);
			return true;
		default:
			return false;
		case InstanceType.IT_DSG:
			AllowDSGClass(className);
			return true;
		case InstanceType.IT_ANIMATION:
			AllowAnimationClass(className);
			return true;
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsLightClassAllowed(string name)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(name).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002ELightRigClass_002EIsLightClassAllowed((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsAnimationClassAllowed(string name)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(name).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002ELightRigClass_002EIsAnimationClassAllowed((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsDSGClassAllowed(string name)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(name).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002ELightRigClass_002EIsDSGClassAllowed((global::AssetObjects.LightRigClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsTriggerClassAllowed(string name)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(name).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsTriggerClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}
}
