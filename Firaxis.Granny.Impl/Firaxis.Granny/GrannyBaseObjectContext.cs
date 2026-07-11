#define DEBUG
using granny;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace Firaxis.Granny;

internal abstract class GrannyBaseObjectContext : IGrannyReferenceStorage
{
	protected unsafe memory_arena* m_pkMemoryArena;

	protected unsafe string_table* m_pkStringTable;

	private List<IntPtr> m_pvReferenceArray;

	private unsafe memory_arena* m_pkReferenceArena;

	public unsafe virtual bool StoredReferences
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return (byte)(((long)(nint)m_pkReferenceArena != 0) ? 1u : 0u) != 0;
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool StoreReferences()
	{
		//IL_0110: Expected I, but got I8
		//IL_0129: Expected I, but got I8
		//IL_012f: Expected I, but got I8
		//IL_00d3: Expected I4, but got I8
		byte condition = (((long)(nint)m_pkReferenceArena == 0) ? ((byte)1) : ((byte)0));
		Debug.Assert(condition != 0, "References are already stored.");
		if (m_pkReferenceArena != null)
		{
			return false;
		}
		m_pkReferenceArena = global::_003CModule_003E.GrannyNewMemoryArena();
		m_pvReferenceArray = new List<IntPtr>();
		granny_data_type_definition* ptr = GetGrannyType();
		byte* ptr2 = (byte*)GetGrannyObject();
		int num = *(int*)ptr;
		if (num != 0)
		{
			do
			{
				switch (num)
				{
				case 4:
					Debug.Assert(condition: false, "This was never used before...");
					break;
				case 3:
				{
					ArrayReferencer* ptr3 = (ArrayReferencer*)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkReferenceArena, (ulong)(global::_003CModule_003E.GrannyGetMemberUnitSize(ptr) * *(int*)ptr2) + 20uL);
					*(int*)ptr3 = *(int*)ptr2;
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)ptr3 + 4L), System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr2 + 4L)));
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)ptr3 + 12L), (long)(nint)ptr3 + 20L);
					// IL cpblk instruction
					System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 12L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 4L)), (uint)(global::_003CModule_003E.GrannyGetMemberUnitSize(ptr) * *(int*)ptr3));
					IntPtr item2 = (IntPtr)ptr3;
					m_pvReferenceArray.Add(item2);
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)ptr2 + 4L), System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 12L)));
					break;
				}
				case 2:
				{
					global::_003CModule_003E.GrannyMemoryArenaPush(m_pkReferenceArena, 8uL);
					IntPtr item = (IntPtr)(void*)(*(ulong*)ptr2);
					m_pvReferenceArray.Add(item);
					break;
				}
				}
				ptr2 = (byte*)((ulong)global::_003CModule_003E.GrannyGetMemberTypeSize(ptr) + (ulong)(nint)ptr2);
				ptr = (granny_data_type_definition*)((ulong)(nint)ptr + 44uL);
				num = *(int*)ptr;
			}
			while (num != 0);
		}
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RestoreReferences()
	{
		//IL_00e0: Expected I, but got I8
		//IL_00b4: Expected I8, but got I
		//IL_00be: Expected I, but got I8
		//IL_00c4: Expected I, but got I8
		byte condition = (byte)(((long)(nint)m_pkReferenceArena != 0) ? 1 : 0);
		Debug.Assert(condition != 0, "References are not stored");
		if (m_pkReferenceArena == null)
		{
			return false;
		}
		granny_data_type_definition* ptr = GetGrannyType();
		byte* ptr2 = (byte*)GetGrannyObject();
		int num = 0;
		int num2 = *(int*)ptr;
		if (num2 != 0)
		{
			do
			{
				switch (num2)
				{
				case 4:
					Debug.Assert(condition: false, "This was never used before...");
					break;
				case 3:
				{
					int index2 = num;
					num++;
					ArrayReferencer* ptr3 = (ArrayReferencer*)m_pvReferenceArray[index2].ToPointer();
					*(int*)ptr2 = *(int*)ptr3;
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)ptr2 + 4L), System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr3 + 4L)));
					break;
				}
				case 2:
				{
					int index = num;
					num++;
					IntPtr intPtr = m_pvReferenceArray[index];
					*(long*)ptr2 = (nint)(void*)intPtr;
					break;
				}
				}
				ptr2 = (byte*)((ulong)global::_003CModule_003E.GrannyGetMemberTypeSize(ptr) + (ulong)(nint)ptr2);
				ptr = (granny_data_type_definition*)((ulong)(nint)ptr + 44uL);
				num2 = *(int*)ptr;
			}
			while (num2 != 0);
		}
		global::_003CModule_003E.GrannyFreeMemoryArena(m_pkReferenceArena);
		m_pkReferenceArena = null;
		m_pvReferenceArray = null;
		return true;
	}

	protected unsafe abstract granny_data_type_definition* GetGrannyType();

	protected unsafe abstract void* GetGrannyObject();

	protected unsafe GrannyBaseObjectContext(GrannyBaseObjectContext kCopy)
        : base()
    {
		m_pkMemoryArena = kCopy.m_pkMemoryArena;
		m_pkStringTable = kCopy.m_pkStringTable;
	}

	protected unsafe GrannyBaseObjectContext(memory_arena* pkMemoryArena, string_table* pkStringTable)
		:base()
	{
		m_pkMemoryArena = pkMemoryArena;
		m_pkStringTable = pkStringTable;
	}
}
