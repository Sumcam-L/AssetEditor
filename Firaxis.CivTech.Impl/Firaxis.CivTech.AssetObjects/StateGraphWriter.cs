using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using DDT.AnimationGraph;
using DDT.StateGraph;
using eastl;
using Platform;
using Serialization;
using String;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class StateGraphWriter : IStateGraphWriter
{
	private unsafe DDT.StateGraph.Root* m_pkRoot;

	private unsafe map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* m_pkDDTGraphNodeMap;

	private Dictionary<int, IAnimationGraphContainer> m_pmAnimationGraphs;

	public unsafe StateGraphWriter()
	{
		//IL_002d: Expected I, but got I8
		//IL_007f: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		DDT.StateGraph.Root* ptr = (DDT.StateGraph.Root*)global::_003CModule_003E.@new(48uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
		DDT.StateGraph.Root* pkRoot;
		try
		{
			pkRoot = ((ptr == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002ERoot_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
			throw;
		}
		m_pkRoot = pkRoot;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* ptr2 = (map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E*)global::_003CModule_003E.@new(48uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* pkDDTGraphNodeMap;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out EASTL_Allocator_003C19_002C0_003E eASTL_Allocator_003C19_002C0_003E);
			pkDDTGraphNodeMap = ((ptr2 == null) ? null : global::_003CModule_003E.eastl_002Emap_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E_002E_007Bctor_007D(ptr2, global::_003CModule_003E.Types_002EEASTL_Allocator_003C19_002C0_003E_002E_007Bctor_007D(&eASTL_Allocator_003C19_002C0_003E, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09LHABGHIE_0040EASTL_003F5map_003F_0024AA_0040))));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
			throw;
		}
		m_pkDDTGraphNodeMap = pkDDTGraphNodeMap;
		m_pmAnimationGraphs = new Dictionary<int, IAnimationGraphContainer>();
		base._002Ector();
		global::_003CModule_003E.DDT_002ERegisterTypeInfo();
	}

	private void _007EStateGraphWriter()
	{
		_0021StateGraphWriter();
	}

	private unsafe void _0021StateGraphWriter()
	{
		//IL_001f: Expected I, but got I8
		//IL_0049: Expected I, but got I8
		//IL_0051: Expected I, but got I8
		global::_003CModule_003E.DDT_002EReleaseStateGraph(m_pkRoot);
		DDT.StateGraph.Root* pkRoot = m_pkRoot;
		if (pkRoot != null)
		{
			global::_003CModule_003E.String_002EGlobal_002E_007Bdtor_007D((Global*)((ulong)(nint)pkRoot + 16uL));
			global::_003CModule_003E.delete(pkRoot, 48uL);
		}
		map_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_0020_003E* pkDDTGraphNodeMap = m_pkDDTGraphNodeMap;
		if (pkDDTGraphNodeMap != null)
		{
			global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002E_007Bdtor_007D((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)pkDDTGraphNodeMap);
			global::_003CModule_003E.delete(pkDDTGraphNodeMap, 48uL);
		}
		m_pkRoot = null;
		m_pkDDTGraphNodeMap = null;
		Dictionary<int, IAnimationGraphContainer>.Enumerator enumerator = m_pmAnimationGraphs.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((AnimationGraphWriter)((KeyValuePair<int, IAnimationGraphContainer>)(object)enumerator.Current).Value).RemoveReferences();
			}
			while (enumerator.MoveNext());
		}
		m_pmAnimationGraphs.Clear();
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool Save(string sFilename)
	{
		//IL_00b4: Expected I, but got I8
		if (string.IsNullOrEmpty(sFilename))
		{
			return false;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out FileOStream fileOStream);
		global::_003CModule_003E.Serialization_002EFileOStream_002E_007Bctor_007D(&fileOStream);
		bool result;
		try
		{
			char* ptr = (char*)Marshal.StringToHGlobalUni(sFilename).ToPointer();
			bool flag = global::_003CModule_003E.Serialization_002EFileOStream_002EOpen(&fileOStream, ptr) == (IO_RESULT)0;
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			if (!flag)
			{
				result = false;
			}
			else
			{
				uint num = 0u;
				Dictionary<int, IAnimationGraphContainer>.Enumerator enumerator = m_pmAnimationGraphs.GetEnumerator();
				if (enumerator.MoveNext())
				{
					do
					{
						uint num2 = global::_003CModule_003E.DDT_002EGetDecisionCount(((AnimationGraphWriter)((KeyValuePair<int, IAnimationGraphContainer>)(object)enumerator.Current).Value).GetAnimationGraph());
						num = ((num2 > num) ? num2 : num);
					}
					while (enumerator.MoveNext());
				}
				*(uint*)((ulong)(nint)m_pkRoot + 40uL) = num;
				global::_003CModule_003E.XMLSerialization_002E_003FA0x9378923e_002ESerialize_003Cclass_0020DDT_003A_003AStateGraph_003A_003ARoot_003E(m_pkRoot, (OStream*)(&fileOStream), null);
				global::_003CModule_003E.Serialization_002EFileOStream_002EClose(&fileOStream);
				result = true;
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<FileOStream*, void>)(&global::_003CModule_003E.Serialization_002EFileOStream_002E_007Bdtor_007D), &fileOStream);
			throw;
		}
		global::_003CModule_003E.Serialization_002EFileOStream_002E_007Bdtor_007D(&fileOStream);
		return result;
	}

	public unsafe virtual void AddRoot(int iNodeID, string pmText, int iPosX, int iPosY)
	{
		*(int*)((ulong)(nint)m_pkRoot + 24uL) = iNodeID;
		void* pkRoot = m_pkRoot;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTGraphNode dDTGraphNode);
		*(int*)(&dDTGraphNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTGraphNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTGraphNode, 4)) = 0;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTGraphNode, &pkRoot));
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetGraphNodeText(iNodeID, (NodeType)0, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		SetGraphNodeLocation(iNodeID, (NodeType)0, iPosX, iPosY);
	}

	public unsafe virtual void AddOrphanRoot(int iNodeID)
	{
		*(int*)((ulong)(nint)m_pkRoot + 28uL) = iNodeID;
		void* pkRoot = m_pkRoot;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTGraphNode dDTGraphNode);
		*(int*)(&dDTGraphNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTGraphNode, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTGraphNode, 4)) = 0;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTGraphNode, &pkRoot));
	}

	public virtual void AddData(int iParentID, int iNodeID, string pmTypeName, string pmText, int iPosX, int iPosY)
	{
	}

	public unsafe virtual void AddSource(int iParentID, int iNodeID, string pmText, int iPosX, int iPosY)
	{
		AddGraphNode(iParentID, iNodeID, (NodeType)2);
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetGraphNodeText(iNodeID, (NodeType)2, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		SetGraphNodeLocation(iNodeID, (NodeType)2, iPosX, iPosY);
	}

	public unsafe virtual void AddDestination(int iParentID, int iNodeID, string pmText, int iPosX, int iPosY)
	{
		AddGraphNode(iParentID, iNodeID, (NodeType)3);
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetGraphNodeText(iNodeID, (NodeType)3, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		SetGraphNodeLocation(iNodeID, (NodeType)3, iPosX, iPosY);
	}

	public unsafe virtual void AddAnimationGraph(int iParentID, int iNodeID, string pmText, int iPosX, int iPosY)
	{
		//IL_0044: Expected I, but got I8
		void* intPtr = AddGraphNode(iParentID, iNodeID, (NodeType)4);
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(pmText).ToPointer();
		SetGraphNodeText(iNodeID, (NodeType)4, ptr);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		SetGraphNodeLocation(iNodeID, (NodeType)4, iPosX, iPosY);
		AnimationGraphWriter value = new AnimationGraphWriter((DDT.AnimationGraph.Root*)((ulong)(nint)intPtr + 48uL));
		m_pmAnimationGraphs.Add(iNodeID, value);
	}

	public unsafe virtual void SetSourceStateName(int iNodeID, string pmStateName)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_006b: Expected I, but got I8
		//IL_0049: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		SourceNode* ptr = (SourceNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 2) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSourceStateName_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 127u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetSourceStateName_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmStateName).ToPointer();
		global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)ptr + 48uL), ptr2);
		IntPtr hglobal = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetDestinationStateName(int iNodeID, string pmStateName)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_006e: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		DestinationNode* ptr = (DestinationNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 3) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationStateName_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 139u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationStateName_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		sbyte* ptr2 = (sbyte*)Marshal.StringToHGlobalAnsi(pmStateName).ToPointer();
		global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)ptr + 48uL), ptr2);
		IntPtr hglobal = new IntPtr(ptr2);
		Marshal.FreeHGlobal(hglobal);
	}

	public unsafe virtual void SetDestinationLoop(int iNodeID, [MarshalAs(UnmanagedType.U1)] bool bLoop)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		DestinationNode* ptr = (DestinationNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 3) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationLoop_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 151u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationLoop_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		*(bool*)((ulong)(nint)ptr + 64uL) = bLoop;
	}

	public unsafe virtual void SetDestinationBlendDuration(int iNodeID, float fBlendDuration)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		DestinationNode* ptr = (DestinationNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 3) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationBlendDuration_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHM_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 161u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationBlendDuration_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHM_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		*(float*)((ulong)(nint)ptr + 60uL) = fBlendDuration;
	}

	public unsafe virtual void SetDestinationRandomOffset(int iNodeID, [MarshalAs(UnmanagedType.U1)] bool bRandomOffset)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		DestinationNode* ptr = (DestinationNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 3) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationRandomOffset_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 171u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationRandomOffset_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		*(bool*)((ulong)(nint)ptr + 65uL) = bRandomOffset;
	}

	public unsafe virtual void SetDestinationContinueOffset(int iNodeID, [MarshalAs(UnmanagedType.U1)] bool bContinueOffset)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		DestinationNode* ptr = (DestinationNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 3) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationContinueOffset_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 181u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetDestinationContinueOffset_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXH_N_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		*(bool*)((ulong)(nint)ptr + 66uL) = bContinueOffset;
	}

	public unsafe virtual void SetAnimationGraphPercentChance(int iNodeID, float fPercentChance)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		AnimationGraphNode* ptr = (AnimationGraphNode*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 4) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetAnimationGraphPercentChance_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHM_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 191u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetAnimationGraphPercentChance_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXHM_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		*(float*)((ulong)(nint)ptr + 440uL) = fPercentChance * 0.01f;
	}

	public virtual IAnimationGraphContainer GetAnimationGraph(int iNodeID)
	{
		IAnimationGraphContainer value = null;
		if (m_pmAnimationGraphs.TryGetValue(iNodeID, out value))
		{
			return value;
		}
		return null;
	}

	private unsafe void* AddGraphNode(int iParentID, int iNodeID, NodeType eNodeType)
	{
		//IL_0026: Expected I, but got I8
		//IL_006f: Expected I, but got I8
		//IL_0063: Expected I, but got I8
		//IL_009b: Expected I, but got I8
		//IL_0093: Expected I, but got I8
		//IL_00d1: Expected I, but got I8
		//IL_00cc: Expected I, but got I8
		//IL_00c1: Expected I, but got I8
		//IL_010e: Expected I, but got I8
		//IL_015f: Expected I, but got I8
		//IL_01b0: Expected I, but got I8
		//IL_0247: Expected I, but got I8
		//IL_023c: Expected I, but got I8
		//IL_0207: Expected I, but got I8
		//IL_0294: Expected I8, but got I
		//IL_0272: Expected I, but got I8
		//IL_0279: Expected I, but got I8
		//IL_028f: Expected I8, but got I
		//IL_0285: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA && eNodeType == (NodeType)0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040PAJLBEEM_0040eNodeType_003F5_003F_0024CB_003F_0024DN_003F5_003F3_003F3DDT_003F3_003F3StateGraph_003F3_003F3_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 208u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		bool dDTNode = GetDDTNode(iParentID, &obj);
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA)
		{
			if (!dDTNode)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07EDBDKCNF_0040bExists_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 212u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				goto IL_006c;
			}
		}
		else if (!dDTNode)
		{
			goto IL_006c;
		}
		Node** ptr = (Node**)((System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != 0) ? System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)) : ((*(int*)(System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)) + 24) != iParentID) ? ((ulong)(System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)) + 8)) : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8))));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PGEBJENB_0040ppkFirstChild_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 228u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				goto IL_00c9;
			}
		}
		else if (ptr == null)
		{
			goto IL_00c9;
		}
		uint num = 0u;
		Node* ptr2 = null;
		switch (eNodeType)
		{
		case (NodeType)1:
		{
			num = 104u;
			void* ptr12 = global::_003CModule_003E.Platform_002EMalloc(104uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 239, 23, 0);
			DataNode* ptr13 = (DataNode*)global::_003CModule_003E.@new(104uL, ptr12);
			DataNode* ptr14;
			try
			{
				ptr14 = ((ptr13 == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002EDataNode_002E_007Bctor_007D(ptr13));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr13, ptr12);
				throw;
			}
			ptr2 = (Node*)ptr14;
			break;
		}
		case (NodeType)2:
		{
			num = 64u;
			void* ptr6 = global::_003CModule_003E.Platform_002EMalloc(64uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 244, 23, 0);
			SourceNode* ptr7 = (SourceNode*)global::_003CModule_003E.@new(64uL, ptr6);
			SourceNode* ptr8;
			try
			{
				ptr8 = ((ptr7 == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002ESourceNode_002E_007Bctor_007D(ptr7));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr7, ptr6);
				throw;
			}
			ptr2 = (Node*)ptr8;
			break;
		}
		case (NodeType)3:
		{
			num = 72u;
			void* ptr9 = global::_003CModule_003E.Platform_002EMalloc(72uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 249, 23, 0);
			DestinationNode* ptr10 = (DestinationNode*)global::_003CModule_003E.@new(72uL, ptr9);
			DestinationNode* ptr11;
			try
			{
				ptr11 = ((ptr10 == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002EDestinationNode_002E_007Bctor_007D(ptr10));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr10, ptr9);
				throw;
			}
			ptr2 = (Node*)ptr11;
			break;
		}
		case (NodeType)4:
		{
			num = 448u;
			void* ptr3 = global::_003CModule_003E.Platform_002EMalloc(448uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 254, 23, 0);
			AnimationGraphNode* ptr4 = (AnimationGraphNode*)global::_003CModule_003E.@new(448uL, ptr3);
			AnimationGraphNode* ptr5;
			try
			{
				ptr5 = ((ptr4 == null) ? null : global::_003CModule_003E.DDT_002EStateGraph_002EAnimationGraphNode_002E_007Bctor_007D(ptr4));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr4, ptr3);
				throw;
			}
			ptr2 = (Node*)ptr5;
			break;
		}
		}
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003FDA_0040_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA)
		{
			if (ptr2 == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06DPBKPCFA_0040pkNode_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 257u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003FDA_0040_003F_003FAddGraphNode_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPEAXHHW4NodeType_0040StateGraph_0040DDT_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				goto IL_0244;
			}
		}
		else if (ptr2 == null)
		{
			goto IL_0244;
		}
		global::_003CModule_003E.Platform_002Ememset(ptr2, 0, num);
		*(int*)((ulong)(nint)ptr2 + 28uL) = iNodeID;
		*(NodeType*)((ulong)(nint)ptr2 + 24uL) = eNodeType;
		*(long*)((ulong)(nint)ptr2 + 8uL) = 0L;
		ulong num2 = *(ulong*)ptr;
		if (num2 != 0L)
		{
			Node* ptr15 = (Node*)num2;
			Node* ptr16 = (Node*)(*(ulong*)(num2 + 8));
			if (ptr16 != null)
			{
				do
				{
					ptr15 = ptr16;
					ptr16 = (Node*)(*(ulong*)((ulong)(nint)ptr16 + 8uL));
				}
				while (ptr16 != null);
			}
			*(long*)((ulong)(nint)ptr15 + 8uL) = (nint)ptr2;
		}
		else
		{
			*(long*)ptr = (nint)ptr2;
		}
		void* ptr17 = ptr2;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTGraphNode dDTGraphNode);
		*(int*)(&dDTGraphNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTGraphNode, NodeType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTGraphNode, 4)) = eNodeType;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E obj3);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj2, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj3, &dDTGraphNode, &ptr17));
		return ptr2;
		IL_006c:
		return null;
		IL_0244:
		return null;
		IL_00c9:
		return null;
	}

	private unsafe void SetGraphNodeText(int iNodeID, NodeType eNodeType, sbyte* szText)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		//IL_0070: Expected I, but got I8
		//IL_0062: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		void* ptr = (void*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != (int)eNodeType) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetGraphNodeText_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHW4NodeType_0040StateGraph_0040DDT_0040_0040PEBD_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06FCHHOKPD_0040pvData_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 295u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetGraphNodeText_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHW4NodeType_0040StateGraph_0040DDT_0040_0040PEBD_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		if (eNodeType == (NodeType)0)
		{
			global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)ptr + 16uL), szText);
		}
		else
		{
			global::_003CModule_003E.String_002EGlobal_002E_003D((Global*)((ulong)(nint)ptr + 16uL), szText);
		}
	}

	private unsafe void SetGraphNodeLocation(int iNodeID, NodeType eNodeType, int iPosX, int iPosY)
	{
		//IL_0026: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		//IL_004c: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		void* ptr = (void*)((!GetDDTNode(iNodeID, &obj) || System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) != (int)eNodeType) ? 0 : System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8)));
		if (!global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetGraphNodeLocation_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHW4NodeType_0040StateGraph_0040DDT_0040_0040HH_0040Z_00404_NA)
		{
			if (ptr == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06FCHHOKPD_0040pvData_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GL_0040DNGIEDCP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 308u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x9378923e_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetGraphNodeLocation_0040StateGraphWriter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXHW4NodeType_0040StateGraph_0040DDT_0040_0040HH_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (ptr == null)
		{
			return;
		}
		if (eNodeType == (NodeType)0)
		{
			*(int*)((ulong)(nint)ptr + 32uL) = iPosX;
			*(int*)((ulong)(nint)ptr + 36uL) = iPosY;
		}
		else
		{
			*(int*)((ulong)(nint)ptr + 32uL) = iPosX;
			*(int*)((ulong)(nint)ptr + 36uL) = iPosY;
		}
	}

	private unsafe void* GetDDTNode(int iNodeID, NodeType eNodeType)
	{
		//IL_0024: Expected I, but got I8
		//IL_0021: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj);
		global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj);
		if (GetDDTNode(iNodeID, &obj) && System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 4)) == (int)eNodeType)
		{
			return (void*)System.Runtime.CompilerServices.Unsafe.As<pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref obj, 8));
		}
		return null;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool GetDDTNode(int iNodeID, pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E* kOutEntry)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTGraphNode dDTGraphNode);
		*(int*)(&dDTGraphNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out rbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E obj);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Efind((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj, &dDTGraphNode);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out rbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E obj2);
		if (global::_003CModule_003E.eastl_002Eoperator_0021_003D_003Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Cstruct_0020eastl_003A_003Apair_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E(&obj, global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Eend((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj2)))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E obj3);
			global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Cvoid_0020_002A_003E_002E_007Bctor_007D_003Cstruct_0020Firaxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E(&obj3, global::_003CModule_003E.eastl_002Erbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002E_002A(&obj));
			// IL cpblk instruction
			System.Runtime.CompilerServices.Unsafe.CopyBlock(kOutEntry, ref obj3, 16);
			return true;
		}
		return false;
	}

	private unsafe void AddDDTNode(int iNodeID, NodeType eNodeType, void* pvData)
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DDTGraphNode dDTGraphNode);
		*(int*)(&dDTGraphNode) = iNodeID;
		System.Runtime.CompilerServices.Unsafe.As<DDTGraphNode, NodeType>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dDTGraphNode, 4)) = eNodeType;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003Ceastl_003A_003Arbtree_iterator_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_002A_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_0026_003E_002Cbool_003E obj);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out pair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E obj2);
		global::_003CModule_003E.eastl_002Erbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E_002Einsert((rbtree_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_002Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002Ceastl_003A_003Aless_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_003E_002CTypes_003A_003AEASTL_Allocator_003C19_002C0_003E_002Ceastl_003A_003Ause_first_003Ceastl_003A_003Apair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_0020_003E_002C1_002C1_003E*)m_pkDDTGraphNodeMap, &obj, global::_003CModule_003E.eastl_002Epair_003CFiraxis_003A_003ACivTech_003A_003AAssetObjects_003A_003ADDTGraphNode_0020const_0020_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &dDTGraphNode, &pvData));
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021StateGraphWriter();
			return;
		}
		try
		{
			_0021StateGraphWriter();
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

	~StateGraphWriter()
	{
		Dispose(A_0: false);
	}
}
