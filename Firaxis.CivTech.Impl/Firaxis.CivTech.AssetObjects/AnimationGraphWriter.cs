using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using DDT.AnimationGraph;
using eastl;
using Platform;
using String;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class AnimationGraphWriter : IAnimationGraphContainer, IDisposable
{
	private unsafe Root* m_pkRoot;

	private unsafe map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* m_pkDDTNodeMap;

	public unsafe AnimationGraphWriter(Root* kRoot)
	{
		//IL_003c: Expected I, but got I8
		m_pkRoot = kRoot;
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* ptr = (map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E*)global::_003CModule_003E.@new(48uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 10, 23, 0);
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* pkDDTNodeMap;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out EASTL_Allocator_003C19_002C0_003E eASTL_Allocator_003C19_002C0_003E);
			pkDDTNodeMap = ((ptr == null) ? null : global::_003CModule_003E.eastl_002Emap_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E_002E_007Bctor_007D(ptr, global::_003CModule_003E.Types_002EEASTL_Allocator_003C19_002C0_003E_002E_007Bctor_007D(&eASTL_Allocator_003C19_002C0_003E, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09LHABGHIE_0040EASTL_003F5map_003F_0024AA_0040))));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 10, 23, 0);
			throw;
		}
		m_pkDDTNodeMap = pkDDTNodeMap;
		base._002Ector();
	}

	private void _007EAnimationGraphWriter()
	{
		_0021AnimationGraphWriter();
	}

	private unsafe void _0021AnimationGraphWriter()
	{
		//IL_0008: Expected I, but got I8
		//IL_0029: Expected I, but got I8
		m_pkRoot = null;
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* pkDDTNodeMap = m_pkDDTNodeMap;
		if (pkDDTNodeMap != null)
		{
			global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002E_007Bdtor_007D((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)pkDDTNodeMap);
			global::_003CModule_003E.delete(pkDDTNodeMap, 48uL);
		}
		m_pkDDTNodeMap = null;
	}

	private unsafe void AddRoot(int iNodeID, int nAnimationItems, int nTimelineItems, int nMaterialItems, int nStateItems, [MarshalAs(UnmanagedType.U1)] bool bOrphan)
	{
		//IL_0048: Expected I4, but got I8
		//IL_002d: Expected I, but got I8
		//IL_00a2: Expected I, but got I8
		//IL_00bc: Expected I, but got I8
		//IL_01b6: Expected I, but got I8
		//IL_01bc: Expected I, but got I8
		//IL_01c2: Expected I, but got I8
		//IL_00eb: Expected I8, but got I
		//IL_0108: Expected I8, but got I
		//IL_0147: Expected I, but got I8
		//IL_013a: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddRoot_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHH_N_0040Z_00404_NA && IsRootInitialized(bOrphan) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BM_0040CGDOEOAG_0040_003F_0024CBIsRootInitialized_003F_0024CIbOrphan_003F_0024CJ_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 208u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddRoot_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHH_N_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (IsRootInitialized(bOrphan))
		{
			return;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY03I _0024ArrayType_0024_0024_0024BY03I2);
		// IL initblk instruction
		System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(ref _0024ArrayType_0024_0024_0024BY03I2, 0, 16);
		int num = ((nAnimationItems > 0) ? nAnimationItems : 0);
		*(int*)(&_0024ArrayType_0024_0024_0024BY03I2) = num;
		int num2 = ((nTimelineItems > 0) ? nTimelineItems : 0);
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03I, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03I2, 4)) = num2;
		int num3 = ((nMaterialItems > 0) ? nMaterialItems : 0);
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03I, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03I2, 8)) = num3;
		int num4 = ((nStateItems > 0) ? nStateItems : 0);
		System.Runtime.CompilerServices.Unsafe.As<_0024ArrayType_0024_0024_0024BY03I, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref _0024ArrayType_0024_0024_0024BY03I2, 12)) = num4;
		Selector* ptr = ((!bOrphan) ? ((Selector*)m_pkRoot) : ((Selector*)((ulong)(nint)m_pkRoot + 192uL)));
		uint num5 = 0u;
		Selector* ptr2 = ptr;
		_0024ArrayType_0024_0024_0024BY03I* ptr3 = &_0024ArrayType_0024_0024_0024BY03I2;
		Selector* ptr4 = (Selector*)((ulong)(nint)ptr + 8uL);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTNode dDTNode);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		do
		{
			uint num6 = *(uint*)ptr3;
			*(int*)((ulong)(nint)ptr2 + 28uL) = iNodeID;
			if (num6 != 0)
			{
				ulong num7 = num6 * 48;
				*(long*)ptr2 = (nint)global::_003CModule_003E.Platform_002EMalloc(num7, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 228, 23, 0);
				void* ptr5 = global::_003CModule_003E.Platform_002EMalloc((ulong)num6 * 8uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 229, 23, 0);
				*(long*)ptr4 = (nint)ptr5;
				*(uint*)((ulong)(nint)ptr2 + 24uL) = num6;
				if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddRoot_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHH_N_0040Z_00404_NA && (*(long*)ptr2 == 0L || ptr5 == null) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CC_0040GPEPFFBL_0040kSelector_003F4Data_003F5_003F_0024CG_003F_0024CG_003F5kSelector_003F4Item_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 233u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddRoot_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHH_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				global::_003CModule_003E.Platform_002Ememset((void*)(*(ulong*)ptr2), 0, num7);
				if (0 < num6)
				{
					long num8 = 0L;
					uint num9 = 0u;
					uint num10 = num6;
					do
					{
						*(int*)(num8 + *(long*)ptr4) = 0;
						*(uint*)(*(long*)ptr4 + num8 + 4) = num9;
						num9 += 48;
						num8 += 8;
						num10 += uint.MaxValue;
					}
					while (num10 != 0);
				}
			}
			void* ptr6 = ptr2;
			*(int*)(&dDTNode) = iNodeID;
			System.Runtime.CompilerServices.Unsafe.As<DDTNode, uint>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 4)) = num5;
			System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 8)) = 1;
			global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTNode, &ptr6));
			num5++;
			ptr3 = (_0024ArrayType_0024_0024_0024BY03I*)((ulong)(nint)ptr3 + 4uL);
			ptr2 = (Selector*)((ulong)(nint)ptr2 + 48uL);
			ptr4 = (Selector*)((ulong)(nint)ptr4 + 48uL);
		}
		while (num5 < 4);
	}

	public virtual void AddRoot(int iNodeID, int nAnimationItems, int nTimelineItems, int nMaterialItems, int nStateItems)
	{
		AddRoot(iNodeID, nAnimationItems, nTimelineItems, nMaterialItems, nStateItems, bOrphan: false);
	}

	public virtual void AddOrphanRoot(int iNodeID, int nAnimationItems, int nTimelineItems, int nMaterialItems, int nStateItems)
	{
		AddRoot(iNodeID, nAnimationItems, nTimelineItems, nMaterialItems, nStateItems, bOrphan: true);
	}

	public virtual void AddAnimation(int iParentID, int iNodeID, int iItemInParent)
	{
		AddSequence(iParentID, iNodeID, iItemInParent, 0, (SequenceType)1);
	}

	public virtual void AddTimeline(int iParentID, int iNodeID, int iItemInParent)
	{
		AddSequence(iParentID, iNodeID, iItemInParent, 1, (SequenceType)2);
	}

	public virtual void AddMaterial(int iParentID, int iNodeID, int iItemInParent)
	{
		AddSequence(iParentID, iNodeID, iItemInParent, 2, (SequenceType)3);
	}

	public virtual void AddState(int iParentID, int iNodeID, int iItemInParent)
	{
		AddSequence(iParentID, iNodeID, iItemInParent, 3, (SequenceType)4);
	}

	public virtual void AddAnimationSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eType, int nItems)
	{
		AddSelector(iParentID, iNodeID, iItemInParent, 0, eType, nItems);
	}

	public virtual void AddTimelineSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eType, int nItems)
	{
		AddSelector(iParentID, iNodeID, iItemInParent, 1, eType, nItems);
	}

	public virtual void AddMaterialSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eType, int nItems)
	{
		AddSelector(iParentID, iNodeID, iItemInParent, 2, eType, nItems);
	}

	public virtual void AddStateSelector(int iParentID, int iNodeID, int iItemInParent, SelectorType eType, int nItems)
	{
		AddSelector(iParentID, iNodeID, iItemInParent, 3, eType, nItems);
	}

	public unsafe virtual void SetAnimationID(int iNodeID, string pmAnimationName, int iAnimationID)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmAnimationName).ToPointer();
		SetSequenceDataID(iNodeID, 0, ptr, iAnimationID);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetAnimationText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSequenceText(iNodeID, 0, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetAnimationLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSequenceLocation(iNodeID, 0, iPosX, iPosY);
	}

	public unsafe virtual void SetTimelineID(int iNodeID, string pmTimelineName, int iTimelineID)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmTimelineName).ToPointer();
		SetSequenceDataID(iNodeID, 1, ptr, iTimelineID);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetTimelineText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSequenceText(iNodeID, 1, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetTimelineLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSequenceLocation(iNodeID, 1, iPosX, iPosY);
	}

	public unsafe virtual void SetMaterialID(int iNodeID, string pmMaterialName, int iMaterialID)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmMaterialName).ToPointer();
		SetSequenceDataID(iNodeID, 2, ptr, iMaterialID);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetMaterialText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSequenceText(iNodeID, 2, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetMaterialLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSequenceLocation(iNodeID, 2, iPosX, iPosY);
	}

	public unsafe virtual void SetStateID(int iNodeID, string pmStateName, int iStateID)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmStateName).ToPointer();
		SetSequenceDataID(iNodeID, 3, ptr, iStateID);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetStateText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSequenceText(iNodeID, 3, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetStateLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSequenceLocation(iNodeID, 3, iPosX, iPosY);
	}

	public unsafe virtual void SetAnimationSelectorText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSelectorText(iNodeID, 0, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetAnimationSelectorLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSelectorLocation(iNodeID, 0, iPosX, iPosY);
	}

	public unsafe virtual void SetTimelineSelectorText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSelectorText(iNodeID, 1, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetTimelineSelectorLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSelectorLocation(iNodeID, 1, iPosX, iPosY);
	}

	public unsafe virtual void SetMaterialSelectorText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSelectorText(iNodeID, 2, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetMaterialSelectorLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSelectorLocation(iNodeID, 2, iPosX, iPosY);
	}

	public unsafe virtual void SetStateSelectorText(int iNodeID, string pmText)
	{
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetSelectorText(iNodeID, 3, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
	}

	public virtual void SetStateSelectorLocation(int iNodeID, int iPosX, int iPosY)
	{
		SetSelectorLocation(iNodeID, 3, iPosX, iPosY);
	}

	internal unsafe void RemoveReferences()
	{
		//IL_0008: Expected I, but got I8
		m_pkRoot = null;
	}

	internal unsafe Root* GetAnimationGraph()
	{
		return m_pkRoot;
	}

	private unsafe void AddSequence(int iParentID, int iNodeID, int iItemInParent, int iLayerInParent, SequenceType eSequenceType)
	{
		//IL_0116: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		//IL_006d: Expected I, but got I8
		//IL_00c4: Expected I, but got I8
		//IL_00eb: Expected I, but got I8
		//IL_00b4: Expected I, but got I8
		//IL_00fb: Expected I, but got I8
		Selector* dDTNode = (Selector*)GetDDTNode(iParentID, iLayerInParent, (SelectorItemType)1);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA)
		{
			if (dDTNode != null)
			{
				goto IL_003d;
			}
			if (!global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040JKJKGAEG_0040pkParentSelector_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 254u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				return;
			}
			/*OpCode not supported: DebugBreak*/;
		}
		if (dDTNode == null)
		{
			return;
		}
		goto IL_003d;
		IL_003d:
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA && (iItemInParent < 0 || iItemInParent >= *(int*)((ulong)(nint)dDTNode + 24uL)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FE_0040OMJIIPBE_0040iItemInParent_003F5_003F_0024DO_003F_0024DN_003F50_003F5_003F_0024CG_003F_0024CG_003F5iItemInPar_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 258u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (iItemInParent < 0 || iItemInParent >= *(int*)((ulong)(nint)dDTNode + 24uL))
		{
			return;
		}
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA && *(int*)((long)iItemInParent * 8L + *(long*)((ulong)(nint)dDTNode + 8uL)) != 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FN_0040MELADLCC_0040pkParentSelector_003F9_003F_0024DOItems_003F_0024FLiItemInP_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 262u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FAddSequence_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SequenceType_0040AnimationGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		long num = (long)iItemInParent * 8L;
		Selector* ptr = (Selector*)((ulong)(nint)dDTNode + 8uL);
		long num2 = *(long*)ptr + num;
		if (*(int*)num2 == 0)
		{
			*(int*)num2 = 2;
			Sequence* ptr2 = (Sequence*)global::_003CModule_003E.@new(40uL, (void*)((uint)(*(int*)(num + *(long*)ptr + 4)) + *(long*)dDTNode));
			Sequence* ptr3;
			try
			{
				ptr3 = ((ptr2 == null) ? null : global::_003CModule_003E.DDT_002EAnimationGraph_002ESequence_002E_007Bctor_007D(ptr2));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr2, (void*)(*(long*)dDTNode + (uint)(*(int*)(*(long*)((ulong)(nint)dDTNode + 8uL) + (long)iItemInParent * 8L + 4))));
				throw;
			}
			*(SequenceType*)((ulong)(nint)ptr3 + 16uL) = eSequenceType;
			*(int*)((ulong)(nint)ptr3 + 20uL) = iNodeID;
			if (iLayerInParent == 3)
			{
				*(int*)((ulong)(nint)ptr3 + 32uL) = -1;
			}
			void* ptr4 = ptr3;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTNode dDTNode2);
			*(int*)(&dDTNode2) = iNodeID;
			System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode2, 4)) = iLayerInParent;
			System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode2, 8)) = 2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
			global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTNode2, &ptr4));
		}
	}

	private unsafe void AddSelector(int iParentID, int iNodeID, int iItemInParent, int iLayerInParent, SelectorType eSelectorType, int nItems)
	{
		//IL_011d: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		//IL_006d: Expected I, but got I8
		//IL_00c4: Expected I, but got I8
		//IL_00ee: Expected I, but got I8
		//IL_00b4: Expected I, but got I8
		//IL_0101: Expected I, but got I8
		//IL_01b0: Expected I, but got I8
		//IL_019a: Expected I, but got I8
		//IL_017f: Expected I, but got I8
		//IL_01dc: Expected I8, but got I
		//IL_01fd: Expected I8, but got I
		//IL_023d: Expected I, but got I8
		//IL_0168: Expected I, but got I8
		//IL_0152: Expected I, but got I8
		//IL_0230: Expected I, but got I8
		Selector* dDTNode = (Selector*)GetDDTNode(iParentID, iLayerInParent, (SelectorItemType)1);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA)
		{
			if (dDTNode != null)
			{
				goto IL_003d;
			}
			if (!global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040JKJKGAEG_0040pkParentSelector_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 285u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				return;
			}
			/*OpCode not supported: DebugBreak*/;
		}
		if (dDTNode == null)
		{
			return;
		}
		goto IL_003d;
		IL_003d:
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA && (iItemInParent < 0 || iItemInParent >= *(int*)((ulong)(nint)dDTNode + 24uL)) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FE_0040OMJIIPBE_0040iItemInParent_003F5_003F_0024DO_003F_0024DN_003F50_003F5_003F_0024CG_003F_0024CG_003F5iItemInPar_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 289u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (iItemInParent < 0 || iItemInParent >= *(int*)((ulong)(nint)dDTNode + 24uL))
		{
			return;
		}
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA && *(int*)((long)iItemInParent * 8L + *(long*)((ulong)(nint)dDTNode + 8uL)) != 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FN_0040MELADLCC_0040pkParentSelector_003F9_003F_0024DOItems_003F_0024FLiItemInP_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 293u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FBF_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		long num = (long)iItemInParent * 8L;
		Selector* ptr = (Selector*)((ulong)(nint)dDTNode + 8uL);
		long num2 = *(long*)ptr + num;
		if (*(int*)num2 != 0)
		{
			return;
		}
		*(int*)num2 = 1;
		Selector* ptr2 = (Selector*)global::_003CModule_003E.@new(48uL, (void*)((uint)(*(int*)(num + *(long*)ptr + 4)) + *(long*)dDTNode));
		Selector* ptr3;
		try
		{
			ptr3 = ((ptr2 == null) ? null : global::_003CModule_003E.DDT_002EAnimationGraph_002ESelector_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, (void*)(*(long*)dDTNode + (uint)(*(int*)(*(long*)((ulong)(nint)dDTNode + 8uL) + (long)iItemInParent * 8L + 4))));
			throw;
		}
		*(int*)((ulong)(nint)ptr3 + 28uL) = iNodeID;
		*(SelectorType*)((ulong)(nint)ptr3 + 40uL) = eSelectorType;
		if (eSelectorType != SelectorType.Weighted)
		{
			if (eSelectorType != SelectorType.Composite)
			{
				if (eSelectorType == SelectorType.Additive)
				{
					Root* ptr4;
					if (nItems == 1)
					{
						ptr4 = (Root*)((ulong)(nint)m_pkRoot + 384uL);
						*(int*)ptr4 |= 8;
						goto IL_01be;
					}
					ptr4 = (Root*)((ulong)(nint)m_pkRoot + 384uL);
					*(int*)ptr4 |= 24;
				}
			}
			else
			{
				Root* ptr4 = (Root*)((ulong)(nint)m_pkRoot + 384uL);
				*(int*)ptr4 |= 1;
			}
		}
		else
		{
			Root* ptr4;
			if (nItems == 1)
			{
				ptr4 = (Root*)((ulong)(nint)m_pkRoot + 384uL);
				*(int*)ptr4 |= 2;
				goto IL_01be;
			}
			ptr4 = (Root*)((ulong)(nint)m_pkRoot + 384uL);
			*(int*)ptr4 |= 6;
		}
		if (nItems > 0)
		{
			goto IL_01be;
		}
		goto IL_027a;
		IL_01be:
		ulong num3 = (uint)(nItems * 48);
		*(long*)ptr3 = (nint)global::_003CModule_003E.Platform_002EMalloc(num3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 329, 23, 0);
		void* ptr5 = global::_003CModule_003E.Platform_002EMalloc((ulong)nItems * 8uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 330, 23, 0);
		*(long*)((ulong)(nint)ptr3 + 8uL) = (nint)ptr5;
		*(int*)((ulong)(nint)ptr3 + 24uL) = nItems;
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FCJ_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA && (*(long*)ptr3 == 0L || ptr5 == null) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CG_0040LJCCPIOO_0040pkSelector_003F9_003F_0024DOData_003F5_003F_0024CG_003F_0024CG_003F5pkSelector_003F9_003F_0024DO_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 334u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003FCJ_0040_003F_003FAddSelector_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHHW4SelectorType_0040345_0040H_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::_003CModule_003E.Platform_002Ememset((void*)(*(ulong*)ptr3), 0, num3);
		if (0u < (uint)nItems)
		{
			long num4 = 0L;
			uint num5 = 0u;
			int num6 = nItems;
			do
			{
				*(int*)(num4 + *(long*)((ulong)(nint)ptr3 + 8uL)) = 0;
				*(uint*)(*(long*)((ulong)(nint)ptr3 + 8uL) + num4 + 4) = num5;
				num5 += 48;
				num4 += 8;
				num6 += -1;
			}
			while (num6 != 0);
		}
		goto IL_027a;
		IL_027a:
		void* ptr6 = ptr3;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTNode dDTNode2);
		*(int*)(&dDTNode2) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode2, 4)) = iLayerInParent;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode2, 8)) = 1;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTNode2, &ptr6));
	}

	private unsafe void SetSequenceDataID(int iNodeID, int iLayerInParent, sbyte* szDataName, int iDataID)
	{
		//IL_004a: Expected I, but got I8
		//IL_0030: Expected I, but got I8
		Sequence* dDTNode = (Sequence*)GetDDTNode(iNodeID, iLayerInParent, (SelectorItemType)2);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceDataID_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBDH_0040Z_00404_NA)
		{
			if (dDTNode == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040FJEJOFHA_0040pkSequence_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 351u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceDataID_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBDH_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (dDTNode == null)
		{
			return;
		}
		*(int*)((ulong)(nint)dDTNode + 32uL) = iDataID;
		global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)dDTNode + 8uL), szDataName);
	}

	private unsafe void SetSelectorText(int iNodeID, int iLayerInParent, sbyte* szText)
	{
		//IL_0043: Expected I, but got I8
		//IL_0030: Expected I, but got I8
		Selector* dDTNode = (Selector*)GetDDTNode(iNodeID, iLayerInParent, (SelectorItemType)1);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSelectorText_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBD_0040Z_00404_NA)
		{
			if (dDTNode == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040OAOLCFEM_0040pkSelector_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 362u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSelectorText_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBD_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (dDTNode == null)
		{
			return;
		}
		global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)dDTNode + 16uL), szText);
	}

	private unsafe void SetSequenceText(int iNodeID, int iLayerInParent, sbyte* szText)
	{
		//IL_0030: Expected I, but got I8
		Sequence* dDTNode = (Sequence*)GetDDTNode(iNodeID, iLayerInParent, (SelectorItemType)2);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceText_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBD_0040Z_00404_NA)
		{
			if (dDTNode == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040FJEJOFHA_0040pkSequence_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 372u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceText_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHPEBD_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (dDTNode == null)
		{
			return;
		}
		global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)dDTNode, szText);
	}

	private unsafe void SetSelectorLocation(int iNodeID, int iLayerInParent, int iPosX, int iPosY)
	{
		//IL_0030: Expected I, but got I8
		Selector* dDTNode = (Selector*)GetDDTNode(iNodeID, iLayerInParent, (SelectorItemType)1);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSelectorLocation_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHH_0040Z_00404_NA)
		{
			if (dDTNode == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040OAOLCFEM_0040pkSelector_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 382u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSelectorLocation_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHH_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (dDTNode == null)
		{
			return;
		}
		*(int*)((ulong)(nint)dDTNode + 32uL) = iPosX;
		*(int*)((ulong)(nint)dDTNode + 36uL) = iPosY;
	}

	private unsafe void SetSequenceLocation(int iNodeID, int iLayerInParent, int iPosX, int iPosY)
	{
		//IL_0030: Expected I, but got I8
		Sequence* dDTNode = (Sequence*)GetDDTNode(iNodeID, iLayerInParent, (SelectorItemType)2);
		if (!global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceLocation_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHH_0040Z_00404_NA)
		{
			if (dDTNode == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040FJEJOFHA_0040pkSequence_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040JPICGCIL_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 393u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc93d12d1_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSequenceLocation_0040AnimationGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHHHH_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (dDTNode == null)
		{
			return;
		}
		*(int*)((ulong)(nint)dDTNode + 24uL) = iPosX;
		*(int*)((ulong)(nint)dDTNode + 28uL) = iPosY;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool IsRootInitialized([MarshalAs(UnmanagedType.U1)] bool bOrphan)
	{
		//IL_001f: Expected I, but got I8
		//IL_002e: Expected I, but got I8
		//IL_0018->IL0018: Incompatible stack types: I vs I8
		long num = (bOrphan ? ((long)(nint)m_pkRoot + 192L) : ((nint)m_pkRoot));
		uint num2 = 0u;
		Selector* ptr = (Selector*)(num + 24);
		do
		{
			if (*(int*)ptr == 0)
			{
				num2++;
				ptr = (Selector*)((ulong)(nint)ptr + 48uL);
				continue;
			}
			return true;
		}
		while (num2 < 4);
		return false;
	}

	private unsafe void* GetDDTNode(int iNodeID, int iLayer, SelectorItemType eSelectorItemType)
	{
		//IL_0062: Expected I, but got I8
		//IL_005f: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTNode dDTNode);
		*(int*)(&dDTNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 4)) = iLayer;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, SelectorItemType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 8)) = eSelectorItemType;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out rbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E obj);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Efind((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj, &dDTNode);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out rbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E obj2);
		if (global::_003CModule_003E.eastl_002Eoperator_0021_003D_003Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E(&obj, global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Eend((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj2)) && *(int*)((ulong)(nint)global::_003CModule_003E.eastl_002Erbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002E_002D_003E(&obj) + 4uL) == iLayer && *(int*)((ulong)(nint)global::_003CModule_003E.eastl_002Erbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002E_002D_003E(&obj) + 8uL) == (int)eSelectorItemType)
		{
			return (void*)(*(ulong*)((ulong)(nint)global::_003CModule_003E.eastl_002Erbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002E_002D_003E(&obj) + 16uL));
		}
		return null;
	}

	private unsafe void AddDDTNode(int iNodeID, int iLayer, SelectorItemType eSelectorItemType, void* pvData)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTNode dDTNode);
		*(int*)(&dDTNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 4)) = iLayer;
		System.Runtime.CompilerServices.Unsafe.As<DDTNode, SelectorItemType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTNode, 8)) = eSelectorItemType;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTNode, &pvData));
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021AnimationGraphWriter();
			return;
		}
		try
		{
			_0021AnimationGraphWriter();
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

	~AnimationGraphWriter()
	{
		Dispose(A_0: false);
	}
}
