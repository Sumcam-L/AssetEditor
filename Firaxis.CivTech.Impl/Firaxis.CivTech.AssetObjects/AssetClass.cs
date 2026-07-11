using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AssetClass : ClassEntity, IAssetClass
{
	private ParameterSet m_pmAttachmentParams;

	private List<IAssetClassState> m_pmStates;

	private List<IAssetArtDefReference> m_pmArtDefReferences;

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

	public unsafe virtual IEnumerable<string> AllowedSplineClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Espline_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Espline_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Espline_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedBehaviorClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Ebhv_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Ebhv_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Ebhv_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedParticleEffectClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Eptl_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eptl_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eptl_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedGeometryClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Egeo_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Egeo_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Egeo_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedDSGClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Edsg_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Edsg_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Edsg_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public unsafe virtual IEnumerable<string> AllowedAnimationClasses
	{
		get
		{
			global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
			List<string> list = new List<string>();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EAssetClass_002Eanm_classes_begin(pkEntity, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eanm_classes_end(pkEntity, &const_iterator2)))
			{
				do
				{
					IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					list.Add(Marshal.PtrToStringAnsi(ptr));
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eanm_classes_end(pkEntity, &const_iterator2)));
			}
			return list;
		}
	}

	public virtual IEnumerable<IAssetArtDefReference> ArtDefReferences => new List<IAssetArtDefReference>(m_pmArtDefReferences);

	public virtual IEnumerable<IAssetClassState> States => new List<IAssetClassState>(m_pmStates);

	public virtual IParameterSet AttachmentParams => m_pmAttachmentParams;

	public unsafe AssetClass(global::AssetObjects.ClassSet* pkClassSet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmStates = new List<IAssetClassState>();
		m_pmArtDefReferences = new List<IAssetArtDefReference>();
		base._002Ector((global::AssetObjects.ClassEntity*)global::_003CModule_003E.AssetObjects_002EClassSet_002EPush_003Cclass_0020AssetObjects_003A_003AAssetClass_003E(pkClassSet), pkDeserializer);
	}

	public unsafe AssetClass(global::AssetObjects.AssetClass* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_pmAttachmentParams = null;
		m_pmStates = new List<IAssetClassState>();
		m_pmArtDefReferences = new List<IAssetArtDefReference>();
		base._002Ector((global::AssetObjects.ClassEntity*)pkClassEntity, pkDeserializer);
	}

	public unsafe virtual IAssetClassState AddState(string pmName, string pmDescription)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmDescription).ToPointer();
		global::AssetObjects.AssetClassState* pkAssetClassState = global::_003CModule_003E.AssetObjects_002EAssetClass_002ENewState((global::AssetObjects.AssetClass*)m_pkEntity, ptr, ptr2);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		IntPtr hglobal2 = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal2);
		AssetClassState assetClassState = new AssetClassState(pkAssetClassState);
		m_pmStates.Add(assetClassState);
		return assetClassState;
	}

	public virtual IAssetClassState AddState(string pmName)
	{
		return AddState(pmName, string.Empty);
	}

	public unsafe virtual IAssetArtDefReference AddArtDefReference(string pmTemplateName, string pmCollectionName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmTemplateName).ToPointer();
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmCollectionName).ToPointer();
		global::AssetObjects.AssetArtDefReference* pkAssetArtDefReference = global::_003CModule_003E.AssetObjects_002EAssetClass_002ENewArtDefReference((global::AssetObjects.AssetClass*)m_pkEntity, ptr, ptr2);
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

	public unsafe virtual void AllowGeometryClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowGeometryClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowAnimationClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowAnimationClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowDSGClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowDSGClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowParticleEffectClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowParticleEffectClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowBehaviorClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowBehaviorClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void AllowSplineClass(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowSplineClass((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
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

	public unsafe virtual void AllowPlatform(Platforms ePlatform)
	{
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EAllowPlatform((global::AssetObjects.AssetClass*)m_pkEntity, (global::AssetObjects.Platforms)ePlatform);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsGeometryClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsGeometryClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsAnimationClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsAnimationClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsDSGClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsDSGClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsParticleEffectClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsParticleEffectClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsBehaviorClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsBehaviorClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsSplineClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsSplineClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsTriggerClassAllowed(string pmName)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmName).ToPointer();
		bool result = global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsTriggerClassAllowed((global::AssetObjects.AssetClass*)m_pkEntity, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool IsPlatformAllowed(Platforms ePlatform)
	{
		return global::_003CModule_003E.AssetObjects_002EAssetClass_002EIsPlatformAllowed((global::AssetObjects.AssetClass*)m_pkEntity, (global::AssetObjects.Platforms)ePlatform);
	}

	public virtual IEnumerable<string> GetAllowedClasses(InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_PARTICLE_EFFECT => AllowedParticleEffectClasses, 
			InstanceType.IT_DSG => AllowedDSGClasses, 
			InstanceType.IT_ANIMATION => AllowedAnimationClasses, 
			InstanceType.IT_GEOMETRY => AllowedGeometryClasses, 
			_ => null, 
		};
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool IsClassAllowed(string className, InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_PARTICLE_EFFECT => IsParticleEffectClassAllowed(className), 
			InstanceType.IT_DSG => IsDSGClassAllowed(className), 
			InstanceType.IT_ANIMATION => IsAnimationClassAllowed(className), 
			InstanceType.IT_GEOMETRY => IsGeometryClassAllowed(className), 
			_ => false, 
		};
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool AllowClass(string className, InstanceType entityType)
	{
		switch (entityType)
		{
		default:
			return false;
		case InstanceType.IT_PARTICLE_EFFECT:
			AllowParticleEffectClass(className);
			return true;
		case InstanceType.IT_DSG:
			AllowDSGClass(className);
			return true;
		case InstanceType.IT_ANIMATION:
			AllowAnimationClass(className);
			return true;
		case InstanceType.IT_GEOMETRY:
			AllowGeometryClass(className);
			return true;
		}
	}

	public unsafe virtual void ClearAllowedPlatformsAndClasses()
	{
		global::_003CModule_003E.AssetObjects_002EAssetClass_002EClearAllowedPlatformsAndClasses((global::AssetObjects.AssetClass*)m_pkEntity);
	}

	public unsafe virtual void RemoveState(IAssetClassState state)
	{
		//IL_0064: Expected I, but got I8
		global::AssetObjects.AssetClassState* unmanaged = ((AssetClassState)state).GetUnmanaged();
		global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_begin(pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity, &const_iterator2)))
		{
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator) != unmanaged)
			{
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				if (!global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity, &const_iterator2)))
				{
					break;
				}
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator3);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_003D_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity, &const_iterator3)))
		{
			return;
		}
		((AssetClassState)state).SetUnmanaged((global::AssetObjects.AssetClassState*)null);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002ERemoveState(pkEntity, unmanaged);
		m_pmStates.Remove(state);
		uint num = 0u;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator4);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_begin(pkEntity, &const_iterator4);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator5);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity, &const_iterator5)))
		{
			do
			{
				((AssetClassState)m_pmStates[(int)num]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator4));
				num++;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator4);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity, &const_iterator5)));
		}
	}

	public unsafe virtual void RemoveArtDefReference(IAssetArtDefReference pmArtDefReference)
	{
		//IL_0064: Expected I, but got I8
		global::AssetObjects.AssetArtDefReference* unmanaged = ((AssetArtDefReference)pmArtDefReference).GetUnmanaged();
		global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_begin(pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity, &const_iterator2)))
		{
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator) != unmanaged)
			{
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				if (!global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity, &const_iterator2)))
				{
					break;
				}
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator3);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_003D_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity, &const_iterator3)))
		{
			return;
		}
		((AssetArtDefReference)pmArtDefReference).SetUnmanaged((global::AssetObjects.AssetArtDefReference*)null);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002ERemoveArtDefReference(pkEntity, unmanaged);
		m_pmArtDefReferences.Remove(pmArtDefReference);
		uint num = 0u;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator4);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_begin(pkEntity, &const_iterator4);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator5);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity, &const_iterator5)))
		{
			do
			{
				((AssetArtDefReference)m_pmArtDefReferences[(int)num]).SetUnmanaged(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator4));
				num++;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator4);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator4, global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity, &const_iterator5)));
		}
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.AssetClass* pkEntity = (global::AssetObjects.AssetClass*)m_pkEntity;
		m_pmAttachmentParams = new ParameterSet(global::_003CModule_003E.AssetObjects_002EAssetClass_002EGetAttachmentParams(pkEntity));
		global::AssetObjects.AssetClass* pkEntity2 = (global::AssetObjects.AssetClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_begin(pkEntity2, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E.const_iterator const_iterator2);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Estates_end(pkEntity2, &const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2))
		{
			do
			{
				global::AssetObjects.AssetClassState* pkAssetClassState = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator);
				m_pmStates.Add(new AssetClassState(pkAssetClassState));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetClassState_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, &const_iterator2));
		}
		global::AssetObjects.AssetClass* pkEntity3 = (global::AssetObjects.AssetClass*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator3);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_begin(pkEntity3, &const_iterator3);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E.const_iterator const_iterator4);
		global::_003CModule_003E.AssetObjects_002EAssetClass_002Eartdefs_end(pkEntity3, &const_iterator4);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4))
		{
			do
			{
				global::AssetObjects.AssetArtDefReference* pkAssetArtDefReference = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator3);
				m_pmArtDefReferences.Add(new AssetArtDefReference(pkAssetArtDefReference));
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator3);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AAssetArtDefReference_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator3, &const_iterator4));
		}
	}

	internal override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		m_pmAttachmentParams?.RemoveReferences();
		List<IAssetClassState> pmStates = m_pmStates;
		if (pmStates != null)
		{
			List<IAssetClassState>.Enumerator enumerator = pmStates.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					((AssetClassState)enumerator.Current).RemoveReferences();
				}
				while (enumerator.MoveNext());
			}
			m_pmStates.Clear();
		}
		List<IAssetArtDefReference> pmArtDefReferences = m_pmArtDefReferences;
		if (pmArtDefReferences != null)
		{
			List<IAssetArtDefReference>.Enumerator enumerator2 = pmArtDefReferences.GetEnumerator();
			if (enumerator2.MoveNext())
			{
				do
				{
					((AssetArtDefReference)enumerator2.Current).RemoveReferences();
				}
				while (enumerator2.MoveNext());
			}
			m_pmArtDefReferences.Clear();
		}
		if (bDisposing)
		{
			m_pmAttachmentParams = null;
			m_pmStates = null;
			m_pmArtDefReferences = null;
		}
		base.RemoveReferences(bDisposing);
	}
}
