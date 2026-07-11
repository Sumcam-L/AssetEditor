using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyMaterial : GrannyBaseObjectContext, IGrannyMaterial
{
	private unsafe granny_material* m_pkMaterial = null;

	private unsafe granny_variant* m_pkExtendedData;

	private List<IGrannyMap> m_pkMaps = new List<IGrannyMap>();

	private IGrannyTexture m_kTexture;

	public unsafe virtual string Tex0
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			//IL_0032: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_04HNBCGJMD_0040Tex0_003F_0024AA_0040), &granny_variant2))
			{
				return Marshal.PtrToStringAnsi((IntPtr)(void*)(*(ulong*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8))));
			}
            return "FireGrafixMaterial";
        }
	}

	public unsafe virtual string typeName
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			//IL_0032: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08IPCLAMCO_0040typeName_003F_0024AA_0040), &granny_variant2))
			{
				return Marshal.PtrToStringAnsi((IntPtr)(void*)(*(ulong*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8))));
			}
            return "FireGrafixMaterial";
        }
	}

	public unsafe virtual int SkinBoneCount
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040OJGCDMCP_0040SkinBoneCount_003F_0024AA_0040), &granny_variant2))
			{
				return *(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
			}
			return 0;
		}
		set
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040OJGCDMCP_0040SkinBoneCount_003F_0024AA_0040), &granny_variant2))
			{
				*(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8)) = value;
			}
		}
	}

	public unsafe virtual int AlphaRef
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08OHHJEIMD_0040AlphaRef_003F_0024AA_0040), &granny_variant2))
			{
				return *(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
			}
			return -1;
		}
		set
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08OHHJEIMD_0040AlphaRef_003F_0024AA_0040), &granny_variant2))
			{
				*(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8)) = value;
			}
		}
	}

	public unsafe virtual int ZMode
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HHFKPCHO_0040ZMode_003F_0024AA_0040), &granny_variant2))
			{
				return *(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
			}
			return -1;
		}
		set
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HHFKPCHO_0040ZMode_003F_0024AA_0040), &granny_variant2))
			{
				*(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8)) = value;
			}
		}
	}

	public unsafe virtual int AlphaMode
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09JEIJEBPJ_0040AlphaMode_003F_0024AA_0040), &granny_variant2))
			{
				return *(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
			}
			return -1;
		}
		set
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09JEIJEBPJ_0040AlphaMode_003F_0024AA_0040), &granny_variant2))
			{
				*(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8)) = value;
			}
		}
	}

	public unsafe virtual string ShaderSet
	{
		get
		{
			//IL_0025: Expected I, but got I8
			//IL_0025: Expected I, but got I8
			//IL_0032: Expected I, but got I8
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09MNIJHJJO_0040ShaderSet_003F_0024AA_0040), &granny_variant2))
			{
				return Marshal.PtrToStringAnsi((IntPtr)(void*)(*(ulong*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8))));
			}
			return "";
		}
		set
		{
			//IL_0042: Expected I, but got I8
			//IL_0042: Expected I, but got I8
			//IL_004b: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			sbyte* ptr2 = global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr);
			granny_material* pkMaterial = m_pkMaterial;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09MNIJHJJO_0040ShaderSet_003F_0024AA_0040), &granny_variant2))
			{
				*(long*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8)) = (nint)ptr2;
			}
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}



	public virtual IGrannyTexture Texture => m_kTexture;

	public virtual List<IGrannyMap> Maps => m_pkMaps;

	public unsafe virtual string Name
	{
		get
		{
			//IL_001d->IL001d: Incompatible stack types: I8 vs Ref
			granny_material* pkMaterial = m_pkMaterial;
			sbyte* ptr;
			if (pkMaterial != null)
			{
                ulong num = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>(pkMaterial);
                ptr = (sbyte*)num;
            }
			else
			{
				ptr = (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040);
			}
			return Marshal.PtrToStringAnsi((IntPtr)ptr) ?? "";
		}
		set
		{
			//IL_0025: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_pkMaterial, (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr));
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe GrannyMaterial(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyMaterial()
	{
	}

	private void _0021GrannyMaterial()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_material*)GetGrannyObject());
		return result;
	}

	public unsafe virtual int GetExtendedDataInt(string FieldName)
	{
		//IL_0031: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(FieldName).ToPointer();
		granny_material* pkMaterial = m_pkMaterial;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
		if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), ptr, &granny_variant2))
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			return *(int*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
		}
		IntPtr hglobal2 = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal2);
		return 1;
	}

	public unsafe virtual float GetExtendedDataFloat(string FieldName)
	{
		//IL_0031: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(FieldName).ToPointer();
		granny_material* pkMaterial = m_pkMaterial;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
		if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), ptr, &granny_variant2))
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			return *(float*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8));
		}
		IntPtr hglobal2 = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal2);
		return 1f;
	}

	public unsafe virtual string GetExtendedDataString(string FieldName)
	{
		//IL_0031: Expected I, but got I8
		//IL_0031: Expected I, but got I8
		//IL_004d: Expected I, but got I8
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(FieldName).ToPointer();
		granny_material* pkMaterial = m_pkMaterial;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
		if (global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 28L)), (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMaterial + 36L)), ptr, &granny_variant2))
		{
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			return Marshal.PtrToStringAnsi((IntPtr)(void*)(*(ulong*)System.Runtime.CompilerServices.Unsafe.As<granny_variant, ulong>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref granny_variant2, 8))));
		}
		IntPtr hglobal2 = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal2);
		IntPtr ptr2 = new IntPtr(System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040));
		return Marshal.PtrToStringAnsi(ptr2);
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_material* pkMaterial)
	{
		//IL_0086: Expected I, but got I8
		//IL_0048: Expected I, but got I8
		if (pkMaterial == null)
		{
			return false;
		}
		m_pkMaterial = pkMaterial;
		m_pkMaps.Clear();
		int num = 0;
		if (0 < *(int*)((ulong)(nint)m_pkMaterial + 8uL))
		{
			do
			{
				GrannyMap grannyMap = new GrannyMap(this);
				if (grannyMap.Attach((granny_material_map*)((long)num * 16L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMaterial + 12L)))))
				{
					m_pkMaps.Add(grannyMap);
					num++;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)m_pkMaterial + 8uL));
		}
		GrannyTexture grannyTexture = new GrannyTexture(this);
		if (!grannyTexture.Attach((granny_texture*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkMaterial + 20L))))
		{
			return false;
		}
		m_kTexture = grannyTexture;
		return true;
	}

	internal unsafe granny_material* GetMaterial()
	{
		return m_pkMaterial;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyMaterialType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkMaterial;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyMaterial();
			return;
		}
		try
		{
			_0021GrannyMaterial();
		}
		finally
		{
			//base.Finalize();
		}
	}

	public virtual void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyMaterial()
	{
		Dispose(A_0: false);
	}
}
