using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyMap : GrannyBaseObjectContext, IGrannyMap
{
	private unsafe granny_material_map* m_pkMaterialMap = null;

	private IGrannyMaterial m_pkMaterial;

	public virtual IGrannyMaterial Material => m_pkMaterial;

	public unsafe virtual string Usage
	{
        get
        {
            if (m_pkMaterialMap == null) return null;
            ulong ptr = *(ulong*)m_pkMaterialMap;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
        set
        {
            IntPtr hglobal = Marshal.StringToHGlobalAnsi(value ?? "");
            *(ulong*)m_pkMaterialMap = (ulong)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, (sbyte*)hglobal.ToPointer());
            Marshal.FreeHGlobal(hglobal);
        }
    }

	public unsafe GrannyMap(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyMap()
	{
	}

	private void _0021GrannyMap()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_material_map*)GetGrannyObject());
		return result;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyMaterialMapType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkMaterialMap;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_material_map* pkMaterialMap)
	{
		//IL_0026: Expected I, but got I8
		if (pkMaterialMap == null)
		{
			return false;
		}
		m_pkMaterialMap = pkMaterialMap;
		GrannyMaterial grannyMaterial = new GrannyMaterial(this);
		if (!grannyMaterial.Attach((granny_material*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkMaterialMap + 8L))))
		{
			return false;
		}
		m_pkMaterial = grannyMaterial;
		return true;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (!A_0)
		{
			try
			{
			}
			finally
			{
				
			}
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyMap()
	{
		Dispose(A_0: false);
	}
}
