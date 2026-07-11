using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class BehaviorClass : ClassEntity, IBehaviorClass
{
	private ParameterSet m_pmAttachmentParams;

	private List<IAssetArtDefReference> m_pmArtDefReferences;

	public unsafe virtual IEnumerable<string> AllowedTriggerClasses
	{
		get
		{
			global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Etrigger_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Etrigger_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Etrigger_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedGeometryClasses
	{
		get
		{
			global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Egeo_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Egeo_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Egeo_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedDSGClasses
	{
		get
		{
			global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Edsg_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Edsg_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Edsg_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedAnimationClasses
	{
		get
		{
			global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eanm_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eanm_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eanm_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public virtual IEnumerable<IAssetArtDefReference> ArtDefReferences => new List<IAssetArtDefReference>(m_pmArtDefReferences);

	public virtual IParameterSet AttachmentParams => m_pmAttachmentParams;

	public unsafe BehaviorClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmArtDefReferences = new List<IAssetArtDefReference>();
		base._002Ector((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003ABehaviorClass_003E(pkClassSet), pkDeserializer);
	}

	public unsafe BehaviorClass(global::AssetObjects.BehaviorClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmAttachmentParams = null;
		m_pmArtDefReferences = new List<IAssetArtDefReference>();
		base._002Ector((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer);
	}

	public unsafe virtual IAssetArtDefReference AddArtDefReference(string pmTemplateName, string pmCollectionName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmTemplateName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmCollectionName).ToPointer();
		global::AssetObjects.AssetArtDefReference* pkAssetArtDefReference = global::_003CModule_003E.AssetObjects_002EBehaviorClass_002ENewArtDefReference((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr, ptr2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
		AssetArtDefReference assetArtDefReference = new AssetArtDefReference(pkAssetArtDefReference);
		m_pmArtDefReferences.Add(assetArtDefReference);
		return assetArtDefReference;
	}

	public virtual IAssetArtDefReference AddArtDefReference(string pmTemplateName)
	{
		return AddArtDefReference(pmTemplateName, string.Empty);
	}

	public unsafe virtual void RemoveArtDefReference(IAssetArtDefReference pmArtDefReference)
	{
		//IL_0064: Expected I, but got I8
		global::AssetObjects.AssetArtDefReference* unmanaged = ((AssetArtDefReference)pmArtDefReference).GetUnmanaged();
		global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_begin(pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity, &const_iterator2)))
		{
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator) != unmanaged)
			{
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				if (!global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity, &const_iterator2)))
				{
					break;
				}
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator3);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_003D_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity, &const_iterator3)))
		{
			return;
		}
		((AssetArtDefReference)pmArtDefReference).SetUnmanaged((global::AssetObjects.AssetArtDefReference*)null);
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002ERemoveArtDefReference(pkEntity, unmanaged);
		m_pmArtDefReferences.Remove(pmArtDefReference);
		uint num = 0u;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator4);
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_begin(pkEntity, &const_iterator4);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator5);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity, &const_iterator5)))
		{
			do
			{
				((AssetArtDefReference)m_pmArtDefReferences[(int)num]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator4));
				num++;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator4);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity, &const_iterator5)));
		}
	}

	public unsafe virtual void AllowGeometryClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EAllowGeometryClass((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowAnimationClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EAllowAnimationClass((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowDSGClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EAllowDSGClass((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowTriggerClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EAllowTriggerClass((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsGeometryClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EIsGeometryClassAllowed((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsAnimationClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EIsAnimationClassAllowed((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsDSGClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EIsDSGClassAllowed((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsTriggerClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EIsTriggerClassAllowed((global::AssetObjects.BehaviorClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	public virtual IEnumerable<string> GetAllowedClasses(InstanceType entityType)
	{
		IEnumerable<string> result = null;
		switch (entityType)
		{
		case InstanceType.IT_DSG:
			result = AllowedDSGClasses;
			break;
		case InstanceType.IT_ANIMATION:
			result = AllowedAnimationClasses;
			break;
		case InstanceType.IT_GEOMETRY:
			result = AllowedGeometryClasses;
			break;
		}
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsClassAllowed(string className, InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_DSG => IsDSGClassAllowed(className), 
			InstanceType.IT_ANIMATION => IsAnimationClassAllowed(className), 
			_ => false, 
		};
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool AllowClass(string className, InstanceType entityType)
	{
		bool result = false;
		switch (entityType)
		{
		case InstanceType.IT_DSG:
			AllowDSGClass(className);
			result = true;
			break;
		case InstanceType.IT_ANIMATION:
			AllowAnimationClass(className);
			result = true;
			break;
		case InstanceType.IT_GEOMETRY:
			AllowGeometryClass(className);
			result = true;
			break;
		}
		return result;
	}

	public unsafe virtual void ClearAllowedClasses()
	{
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EClearAllowedClasses((global::AssetObjects.BehaviorClass*)m_pkEntity);
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.BehaviorClass* pkEntity = (global::AssetObjects.BehaviorClass*)m_pkEntity;
		m_pmAttachmentParams = new ParameterSet(global::_003CModule_003E.AssetObjects_002EBehaviorClass_002EGetAttachmentParams(pkEntity));
		global::AssetObjects.BehaviorClass* pkEntity2 = (global::AssetObjects.BehaviorClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_begin(pkEntity2, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator2);
		global::_003CModule_003E.AssetObjects_002EBehaviorClass_002Eartdefs_end(pkEntity2, &const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2))
		{
			do
			{
				global::AssetObjects.AssetArtDefReference* pkAssetArtDefReference = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator);
				m_pmArtDefReferences.Add(new AssetArtDefReference(pkAssetArtDefReference));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2));
		}
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmAttachmentParams?.RemoveReferences();
		List<IAssetArtDefReference> pmArtDefReferences = m_pmArtDefReferences;
		if (pmArtDefReferences != null)
		{
			List<IAssetArtDefReference>.Enumerator enumerator = pmArtDefReferences.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					((AssetArtDefReference)enumerator.Current).RemoveReferences();
				}
				while (enumerator.MoveNext());
			}
			m_pmArtDefReferences.Clear();
		}
		if (bDisposing)
		{
			m_pmAttachmentParams = null;
			m_pmArtDefReferences = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
