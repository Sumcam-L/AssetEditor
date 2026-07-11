using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using granny;
using Platform;
using std;

namespace Firaxis.Granny;

internal class GrannyFile : GrannyBaseObjectContext, IGrannyFile
{
	private string m_szFilename;

	private unsafe granny_file* m_file = null;

	private unsafe granny_file_info* m_info = null;

	private string m_src = "";

	private List<string> m_boneNames = new List<string>();

	private List<string> m_trackMaskNames = new List<string>();

	private List<IGrannyMesh> m_lstMeshes = new List<IGrannyMesh>();

	private List<IGrannyModel> m_lstModels = new List<IGrannyModel>();

	private List<IGrannyMaterial> m_lstMaterials = new List<IGrannyMaterial>();

	private List<IGrannyAnimation> m_lstAnimations = new List<IGrannyAnimation>();

	private unsafe vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* m_pCurvesToBeFreed;

	public virtual List<string> TrackMaskNames => m_trackMaskNames;

	public virtual List<string> BoneNames => m_boneNames;

	public virtual List<IGrannyAnimation> Animations => m_lstAnimations;

	public virtual List<IGrannyMaterial> Materials => m_lstMaterials;

	public virtual List<IGrannyMesh> Meshes => m_lstMeshes;

	public virtual List<IGrannyModel> Models => m_lstModels;

	public virtual string Source
	{
		get
		{
			return m_src;
		}
		set
		{
			m_src = value;
		}
	}

	public virtual string Filename
	{
		get
		{
			return m_szFilename;
		}
		set
		{
			m_szFilename = value;
		}
	}

	public unsafe GrannyFile()
		: base(global::_003CModule_003E.GrannyNewMemoryArena(), global::_003CModule_003E.GrannyNewStringTable())
	{
		//IL_0008: Expected I, but got I8
		//IL_0010: Expected I, but got I8
		//IL_0085: Expected I, but got I8
		vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* ptr = (vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E*)global::_003CModule_003E.@new(24uL);
		vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* pCurvesToBeFreed;
		try
		{
			pCurvesToBeFreed = ((ptr == null) ? null : global::_003CModule_003E.std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, 24uL);
			throw;
		}
		m_pCurvesToBeFreed = pCurvesToBeFreed;
	}

	private void _007EGrannyFile()
	{
		_0021GrannyFile();
	}

	private unsafe void _0021GrannyFile()
	{
		//IL_0018: Expected I, but got I8
		//IL_0082: Expected I, but got I8
		//IL_004e: Expected I, but got I8
		granny_file* file = m_file;
		if (file != null)
		{
			global::_003CModule_003E.GrannyFreeFile(file);
			m_file = null;
		}
		vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* pCurvesToBeFreed = m_pCurvesToBeFreed;
		if (pCurvesToBeFreed != null)
		{
			vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* ptr = pCurvesToBeFreed;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E obj);
			global::_003CModule_003E.std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Ebegin(ptr, &obj);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E obj2);
			global::_003CModule_003E.std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Eend(ptr, &obj2);
			if (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_0021_003D((_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E*)(&obj), (_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E*)(&obj2)))
			{
				do
				{
					global::_003CModule_003E.GrannyFreeCurve((granny_curve2*)(*(ulong*)global::_003CModule_003E.std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_002A(&obj)));
					global::_003CModule_003E.std_002E_Vector_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_002B_002B(&obj);
				}
				while (global::_003CModule_003E.std_002E_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E_002E_0021_003D((_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E*)(&obj), (_Vector_const_iterator_003Cstd_003A_003A_Vector_val_003Cstd_003A_003A_Simple_types_003Cgranny_curve2_0020_002A_003E_0020_003E_0020_003E*)(&obj2)));
			}
			vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* pCurvesToBeFreed2 = m_pCurvesToBeFreed;
			if (pCurvesToBeFreed2 != null)
			{
				global::_003CModule_003E.std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002E_007Bdtor_007D(pCurvesToBeFreed2);
				global::_003CModule_003E.delete(pCurvesToBeFreed2, 24uL);
			}
		}
		m_info = null;
		global::_003CModule_003E.GrannyFreeMemoryArena(m_pkMemoryArena);
		global::_003CModule_003E.GrannyFreeStringTable(m_pkStringTable);
	}

	public virtual IGrannyModel AddModel()
	{
		return new GrannyModel(this);
	}

	public unsafe virtual IGrannyMaterial AddMaterial()
	{
		//IL_003d: Expected I4, but got I8
		//IL_0056: Expected I4, but got I8
		//IL_0069: Expected I8, but got I
		//IL_0078: Expected I8, but got I
		//IL_00b5: Expected I, but got I8
		//IL_0141: Expected I, but got I8
		//IL_0141: Expected I, but got I8
		granny_material** ptr = (granny_material**)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)m_info + 36uL) + 1) * 8uL);
		granny_file_info* info = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock(ptr, (void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info + 40L)), (uint)((long)(*(int*)((ulong)(nint)info + 36uL)) * 8L));
		granny_material* ptr2 = (granny_material*)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, 44uL);
		// IL initblk instruction
		System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(ptr2, 0, 44);
		*(long*)((long)(*(int*)((ulong)(nint)m_info + 36uL)) * 8L + (nint)ptr) = (nint)ptr2;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 40L), (long)(nint)ptr);
		(*(int*)((ulong)(nint)m_info + 36uL))++;
		variant_builder* ptr3 = global::_003CModule_003E.GrannyBeginVariant(m_pkStringTable);
		global::_003CModule_003E.GrannyAddStringMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09MNIJHJJO_0040ShaderSet_003F_0024AA_0040), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040));
		global::_003CModule_003E.GrannyAddReferenceMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040JHOBELMO_0040FallbackMtl_003F_0024AA_0040), global::_003CModule_003E.GrannyMaterialType, null);
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040DCLCLCM_0040UseFallbackMtl_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09JEIJEBPJ_0040AlphaMode_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05HHFKPCHO_0040ZMode_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08OHHJEIMD_0040AlphaRef_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040OJGCDMCP_0040SkinBoneCount_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyAddStringMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_08IPCLAMCO_0040typeName_003F_0024AA_0040), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BD_0040KPGOALGA_0040FireGrafixMaterial_003F_0024AA_0040));
		global::_003CModule_003E.GrannyAddIntegerMember(ptr3, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BC_0040ILKEHOOB_0040ParameterSetCount_003F_0024AA_0040), 0);
		global::_003CModule_003E.GrannyEndVariantInPlace(ptr3, global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)global::_003CModule_003E.GrannyGetResultingVariantTypeSize(ptr3)), (granny_data_type_definition**)((ulong)(nint)ptr2 + 28uL), global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)global::_003CModule_003E.GrannyGetResultingVariantObjectSize(ptr3)), (void**)((ulong)(nint)ptr2 + 36uL));
		GrannyMaterial grannyMaterial = new GrannyMaterial(this);
		if (!grannyMaterial.Attach(ptr2))
		{
			return null;
		}
		m_lstMaterials.Add(grannyMaterial);
		return grannyMaterial;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveMaterial(IGrannyMaterial kMaterial)
	{
		//IL_0028: Expected I, but got I8
		//IL_003e: Expected I, but got I8
		//IL_0118: Expected I, but got I8
		//IL_0137: Expected I8, but got I
		//IL_014e: Expected I4, but got I8
		//IL_017c: Expected I4, but got I8
		//IL_004d: Expected I, but got I8
		int num = 0;
		granny_file_info* info = m_info;
		if (0 < *(int*)((ulong)(nint)info + 84uL))
		{
			long num2 = 0L;
			do
			{
				granny_mesh* ptr = (granny_mesh*)(*(ulong*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info + 88L)) + num2));
				if (ptr != null)
				{
					int num3 = 0;
					if (0 < *(int*)((ulong)(nint)ptr + 36uL))
					{
						granny_mesh* ptr2 = (granny_mesh*)((ulong)(nint)ptr + 40uL);
						long num4 = 0L;
						do
						{
							granny_material_binding* ptr3 = (granny_material_binding*)(num4 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr2));
							if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr3) == (nint)((GrannyMaterial)kMaterial).GetMaterial())
							{
								System.Runtime.CompilerServices.Unsafe.WriteUnaligned(ptr3, 0L);
							}
							num3++;
							num4 += 8;
						}
						while (num3 < *(int*)((ulong)(nint)ptr + 36uL));
					}
				}
				num++;
				num2 += 8;
				info = m_info;
			}
			while (num < *(int*)((ulong)(nint)info + 84uL));
		}
		int num5 = 0;
		if (*(int*)((ulong)(nint)m_info + 36uL) != 0)
		{
			long num6 = 0L;
			do
			{
				info = m_info;
				if (*(long*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info + 40L)) + num6) != (nint)((GrannyMaterial)kMaterial).GetMaterial())
				{
					num5++;
					num6 += 8;
					continue;
				}
				*(int*)((ulong)(nint)m_info + 36uL) += -1;
				info = m_info;
				int num7 = *(int*)((ulong)(nint)info + 36uL);
				if (num7 == 0)
				{
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)info + 40L), 0L);
					return true;
				}
				granny_material** ptr4 = (granny_material**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info + 40L));
				System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 40L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)num7 * 8uL));
				// IL cpblk instruction
				System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_info + 40L)), ptr4, (uint)((long)num5 * 8L));
				info = m_info;
				// IL cpblk instruction
				System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)((long)num5 * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info + 40L))), (void*)((long)(num5 + 1) * 8L + (nint)ptr4), (uint)((long)(*(int*)((ulong)(nint)info + 36uL) - num5) * 8L));
				return true;
			}
			while (*(int*)((ulong)(nint)info + 36uL) != 0);
		}
		return false;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(m_szFilename).ToPointer();
		Attach(ptr, m_file, m_info);
		IntPtr hglobal = new IntPtr(ptr);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddArtToolAndExporterReference(IGrannyFile kFile)
	{
		granny_file_info* info = ((GrannyFile)kFile).m_info;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_info, System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(info));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 8L), System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info + 8L)));
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddMeshReference(IGrannyMesh kMesh)
	{
		//IL_0019: Expected I, but got I8
		//IL_003e: Expected I8, but got I
		//IL_005b: Expected I4, but got I8
		//IL_0079: Expected I8, but got I
		granny_mesh* mesh = ((GrannyMesh)kMesh).GetMesh();
		granny_file_info* info = m_info;
		granny_mesh** ptr = (granny_mesh**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info + 88L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 88L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)info + 84uL) + 1) * 8uL));
		granny_file_info* info2 = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info2 + 88L)), ptr, (uint)((long)(*(int*)((ulong)(nint)info2 + 84uL)) * 8L));
		granny_file_info* info3 = m_info;
		*(long*)((long)(*(int*)((ulong)(nint)info3 + 84uL)) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info3 + 88L))) = (nint)mesh;
		(*(int*)((ulong)(nint)m_info + 84uL))++;
		m_lstMeshes.Add(kMesh);
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddMaterialReference(IGrannyMaterial kMaterial)
	{
		//IL_0019: Expected I, but got I8
		//IL_003e: Expected I8, but got I
		//IL_005b: Expected I4, but got I8
		//IL_0079: Expected I8, but got I
		granny_material* material = ((GrannyMaterial)kMaterial).GetMaterial();
		granny_file_info* info = m_info;
		granny_material** ptr = (granny_material**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info + 40L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 40L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)info + 36uL) + 1) * 8uL));
		granny_file_info* info2 = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info2 + 40L)), ptr, (uint)((long)(*(int*)((ulong)(nint)info2 + 36uL)) * 8L));
		granny_file_info* info3 = m_info;
		*(long*)((long)(*(int*)((ulong)(nint)info3 + 36uL)) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info3 + 40L))) = (nint)material;
		(*(int*)((ulong)(nint)m_info + 36uL))++;
		m_lstMaterials.Add(kMaterial);
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddModelReference(IGrannyModel kModel)
	{
		//IL_001c: Expected I, but got I8
		//IL_0042: Expected I8, but got I
		//IL_0063: Expected I4, but got I8
		//IL_0081: Expected I8, but got I
		//IL_00a2: Expected I, but got I8
		//IL_00c7: Expected I8, but got I
		//IL_00e5: Expected I4, but got I8
		granny_model* model = ((GrannyModel)kModel).GetModel();
		granny_file_info* info = m_info;
		granny_model** ptr = (granny_model**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info + 100L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 100L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)info + 96uL) + 1) * 8uL));
		granny_file_info* info2 = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info2 + 100L)), ptr, (uint)((long)(*(int*)((ulong)(nint)info2 + 96uL)) * 8L));
		granny_file_info* info3 = m_info;
		*(long*)((long)(*(int*)((ulong)(nint)info3 + 96uL)) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info3 + 100L))) = (nint)model;
		(*(int*)((ulong)(nint)m_info + 96uL))++;
		granny_file_info* info4 = m_info;
		granny_skeleton** ptr2 = (granny_skeleton**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info4 + 52L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 52L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)info4 + 48uL) + 1) * 8uL));
		granny_file_info* info5 = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info5 + 52L)), ptr2, (uint)((long)(*(int*)((ulong)(nint)info5 + 48uL)) * 8L));
		granny_file_info* info6 = m_info;
		*(long*)((long)(*(int*)((ulong)(nint)info6 + 48uL)) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info6 + 52L))) = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)model + 8L));
		(*(int*)((ulong)(nint)m_info + 48uL))++;
		m_lstModels.Add(kModel);
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddAnimationReference(IGrannyAnimation kAnimation)
	{
		//IL_0019: Expected I, but got I8
		//IL_003e: Expected I8, but got I
		//IL_005b: Expected I4, but got I8
		//IL_0079: Expected I8, but got I
		granny_animation* animation = ((GrannyAnimation)kAnimation).GetAnimation();
		granny_file_info* info = m_info;
		granny_animation** ptr = (granny_animation**)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)info + 124L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_info + 124L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)info + 120uL) + 1) * 8uL));
		granny_file_info* info2 = m_info;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlock((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info2 + 124L)), ptr, (uint)((long)(*(int*)((ulong)(nint)info2 + 120uL)) * 8L));
		granny_file_info* info3 = m_info;
		*(long*)((long)(*(int*)((ulong)(nint)info3 + 120uL)) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)info3 + 124L))) = (nint)animation;
		(*(int*)((ulong)(nint)m_info + 120uL))++;
		m_lstAnimations.Add(kAnimation);
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool CompressAnimations(CurveCompressionParameters positionParams, CurveCompressionParameters orientationParams, CurveCompressionParameters shearParams)
	{
		//IL_0074: Expected I, but got I8
		//IL_0083: Expected I, but got I8
		//IL_009f: Expected I, but got I8
		//IL_00ae: Expected I, but got I8
		//IL_00c9: Expected I, but got I8
		//IL_00d8: Expected I, but got I8
		//IL_00e0: Expected I, but got I8
		//IL_010e: Expected I, but got I8
		//IL_013c: Expected I, but got I8
		granny_file_info* info = m_info;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E obj);
		global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bctor_007D(&obj);
		bool result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_compress_curve_parameters granny_compress_curve_parameters2);
			if (!MakeGrannyCurveParams(positionParams, &granny_compress_curve_parameters2, &obj))
			{
				result = false;
			}
			else
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E obj2);
				global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bctor_007D(&obj2);
				try
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_compress_curve_parameters granny_compress_curve_parameters3);
					if (!MakeGrannyCurveParams(orientationParams, &granny_compress_curve_parameters3, &obj2))
					{
						result = false;
					}
					else
					{
						System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E obj3);
						global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bctor_007D(&obj3);
						try
						{
							System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_compress_curve_parameters granny_compress_curve_parameters4);
							if (!MakeGrannyCurveParams(shearParams, &granny_compress_curve_parameters4, &obj3))
							{
								result = false;
							}
							else
							{
								int num = 0;
								if (0 < *(int*)((ulong)(nint)info + 120uL))
								{
									granny_file_info* ptr = (granny_file_info*)((ulong)(nint)info + 124uL);
									long num2 = 0L;
									do
									{
										granny_animation* ptr2 = (granny_animation*)(*(ulong*)(num2 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr)));
										if (ptr2 != null)
										{
											int num3 = 0;
											if (0 < *(int*)((ulong)(nint)ptr2 + 20uL))
											{
												granny_animation* ptr3 = (granny_animation*)((ulong)(nint)ptr2 + 24uL);
												long num4 = 0L;
												do
												{
													granny_track_group* ptr4 = (granny_track_group*)(*(ulong*)(num4 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr3)));
													if (ptr4 != null)
													{
														int num5 = 0;
														if (0 < *(int*)((ulong)(nint)ptr4 + 20uL))
														{
															granny_track_group* ptr5 = (granny_track_group*)((ulong)(nint)ptr4 + 24uL);
															do
															{
																granny_transform_track* ptr6 = (granny_transform_track*)((long)num5 * 60L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr5));
																granny_transform_track* ptr7 = (granny_transform_track*)((ulong)(nint)ptr6 + 28uL);
																if (global::_003CModule_003E.GrannyCurveIsKeyframed((granny_curve2*)ptr7))
																{
																	RecompressCurve((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_03FCCPIJGJ_0040Pos_003F_0024AA_0040), (granny_curve2*)ptr7, *(float*)((ulong)(nint)ptr2 + 12uL), AsQuats: false, positionParams.MaxDegree, &granny_compress_curve_parameters2);
																}
																granny_transform_track* ptr8 = (granny_transform_track*)((ulong)(nint)ptr6 + 12uL);
																if (global::_003CModule_003E.GrannyCurveIsKeyframed((granny_curve2*)ptr8))
																{
																	RecompressCurve((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_03POIGAEFI_0040Ori_003F_0024AA_0040), (granny_curve2*)ptr8, *(float*)((ulong)(nint)ptr2 + 12uL), AsQuats: true, orientationParams.MaxDegree, &granny_compress_curve_parameters3);
																}
																granny_transform_track* ptr9 = (granny_transform_track*)((ulong)(nint)ptr6 + 44uL);
																if (global::_003CModule_003E.GrannyCurveIsKeyframed((granny_curve2*)ptr9))
																{
																	RecompressCurve((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_03NMAEPLEB_0040ScS_003F_0024AA_0040), (granny_curve2*)ptr9, *(float*)((ulong)(nint)ptr2 + 12uL), AsQuats: false, shearParams.MaxDegree, &granny_compress_curve_parameters4);
																}
																num5++;
															}
															while (num5 < *(int*)((ulong)(nint)ptr4 + 20uL));
														}
													}
													num3++;
													num4 += 8;
												}
												while (num3 < *(int*)((ulong)(nint)ptr2 + 20uL));
											}
										}
										num++;
										num2 += 8;
									}
									while (num < *(int*)((ulong)(nint)info + 120uL));
								}
								result = true;
							}
						}
						catch
						{
							//try-fault
							global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D), &obj3);
							throw;
						}
						global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D(&obj3);
					}
				}
				catch
				{
					//try-fault
					global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D), &obj2);
					throw;
				}
				global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D(&obj2);
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool Save()
	{
		granny_file* file = m_file;
		granny_file_info* info = m_info;
		int num = *(int*)((ulong)(nint)file + 20uL);
		num = ((num < 8) ? 8 : num);
		file_builder* ptr = global::_003CModule_003E.GrannyBeginFile(num, 4167204921u, global::_003CModule_003E.GrannyGRNFileMV_ThisPlatform, global::_003CModule_003E.GrannyGetTemporaryDirectory(), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06OINDDGEP_0040Prefix_003F_0024AA_0040));
		int num2 = 0;
		if (0 < num)
		{
			do
			{
				global::_003CModule_003E.GrannySetFileSectionFormat(ptr, num2, (granny_compression_type)2, 32);
				num2++;
			}
			while (num2 < num);
		}
		file_data_tree_writer* ptr2 = global::_003CModule_003E.GrannyBeginFileDataTreeWriting(global::_003CModule_003E.GrannyModFileInfoType, info, 6, 0);
		global::_003CModule_003E.GrannyPreserveObjectFileSections(ptr2, file);
		global::_003CModule_003E.GrannyWriteDataTreeToFileBuilder(ptr2, ptr);
		global::_003CModule_003E.GrannyEndFileDataTreeWriting(ptr2);
		sbyte* ptr3 = (sbyte*)Marshal.StringToHGlobalAnsi(Filename).ToPointer();
		bool result = global::_003CModule_003E.GrannyEndFile(ptr, ptr3);
		IntPtr hglobal = new IntPtr(ptr3);
		Marshal.FreeHGlobal(hglobal);
		return result;
	}

	internal unsafe granny_file_info* GetFileInfo()
	{
		return m_info;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(sbyte* szFilename, granny_file* file, granny_file_info* info)
	{
		//IL_0134: Expected I, but got I8
		//IL_005a: Expected I4, but got I8
		//IL_0065: Expected I8, but got I
		//IL_02d4: Expected I, but got I8
		//IL_0143: Expected I, but got I8
		//IL_0339: Expected I, but got I8
		//IL_02f1: Expected I, but got I8
		//IL_039e: Expected I, but got I8
		//IL_0356: Expected I, but got I8
		//IL_0403: Expected I, but got I8
		//IL_03bb: Expected I, but got I8
		//IL_0420: Expected I, but got I8
		//IL_0182: Expected I, but got I8
		//IL_01da: Expected I, but got I8
		//IL_0205: Expected I, but got I8
		//IL_0205: Expected I, but got I8
		//IL_0238: Expected I, but got I8
		//IL_027a: Expected I, but got I8
		//IL_027a: Expected I, but got I8
		//IL_028e: Expected I, but got I8
		//IL_0299: Expected I, but got I8
		m_file = file;
		m_info = info;
		IntPtr ptr = (IntPtr)szFilename;
		m_szFilename = Marshal.PtrToStringAnsi(ptr);
		if (m_file == null && m_info == null)
		{
			granny_file_info* ptr2 = (granny_file_info*)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, 148uL);
			// IL initblk instruction
			System.Runtime.CompilerServices.Unsafe.InitBlockUnaligned(ptr2, 0, 148);
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)ptr2 + 16L), (long)(nint)szFilename);
			file_builder* ptr3 = global::_003CModule_003E.GrannyBeginFileInMemory(8, 2147483705u, global::_003CModule_003E.GrannyGRNFileMV_32Bit_LittleEndian, 1024);
			file_data_tree_writer* ptr4 = global::_003CModule_003E.GrannyBeginFileDataTreeWriting(global::_003CModule_003E.GrannyFileInfoType, ptr2, 6, 0);
			global::_003CModule_003E.GrannyWriteDataTreeToFileBuilder(ptr4, ptr3);
			global::_003CModule_003E.GrannyEndFileDataTreeWriting(ptr4);
			granny_file_writer* ptr5 = global::_003CModule_003E.GrannyCreateMemoryFileWriter(1024);
			global::_003CModule_003E.GrannyEndFileToWriter(ptr3, ptr5);
            //System.Runtime.CompilerServices.Unsafe.SkipInit(out byte* ptr6);
            byte* ptr6 = null;
            System.Runtime.CompilerServices.Unsafe.SkipInit(out int num);
			global::_003CModule_003E.GrannyStealMemoryWriterBuffer(ptr5, &ptr6, &num);
			granny_file_info* info2 = ((!global::_003CModule_003E.GrannyIsMODFile(m_file = global::_003CModule_003E.GrannyReadEntireFileFromMemory(num, ptr6))) ? global::_003CModule_003E.GrannyGetFileInfo(m_file) : global::_003CModule_003E.GrannyGetModFileInfo(m_file));
			m_info = info2;
			global::_003CModule_003E.GrannyFreeMemoryWriterBuffer(ptr6);
			return true;
		}
		m_boneNames.Clear();
		m_trackMaskNames.Clear();
		int num2 = 0;
		if (0 < *(int*)((ulong)(nint)info + 96uL))
		{
			granny_file_info* ptr7 = (granny_file_info*)((ulong)(nint)info + 100uL);
			long num3 = 0L;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out granny_variant granny_variant2);
			do
			{
				granny_model* ptr8 = (granny_model*)(*(ulong*)(num3 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr7)));
				if (ptr8 != null)
				{
					ulong num4 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr8 + 8L));
					if (num4 != 0L)
					{
						int num5 = 0;
						if (0 < *(int*)(num4 + 8))
						{
							long num6 = 0L;
							do
							{
								m_boneNames.Add(new string((sbyte*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)(num4 + 12)) + num6))));
								num5++;
								num6 += 164;
								num4 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr8 + 8L));
							}
							while (num5 < *(int*)(num4 + 8));
						}
						int num7 = 0;
						ulong num8 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr8 + 8L));
						if (0 < *(int*)(num8 + 8))
						{
							long num9 = 0L;
							do
							{
								granny_variant* ptr9 = (granny_variant*)(num9 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)(num8 + 12)) + 148);
								ulong num10 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>(ptr9);
								if (num10 != 0L)
								{
									ulong num11 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr9 + 8L));
									if (num11 != 0L && global::_003CModule_003E.GrannyFindMatchingMember((granny_data_type_definition*)num10, (void*)num11, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040GKOPDPME_0040GrannyTrackMasks_003F_0024AA_0040), &granny_variant2))
									{
										granny_data_type_definition* ptr10 = (granny_data_type_definition*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)(*(long*)(&granny_variant2) + 12));
										if (ptr10 == null)
										{
											break;
										}
										while (*(int*)ptr10 != 0)
										{
											track_mask* intPtr = global::_003CModule_003E.GrannyNewTrackMask(0f, *(int*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr8 + 8L)) + 8));
											long num12 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr8 + 8L));
											global::_003CModule_003E.GrannyExtractTrackMask(intPtr, *(int*)(num12 + 8), (granny_skeleton*)num12, (sbyte*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr10 + 4L)), 0f, true);
											m_trackMaskNames.Add(new string((sbyte*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr10 + 4L))));
											ptr10 = (granny_data_type_definition*)((ulong)(nint)ptr10 + 44uL);
											if (ptr10 == null)
											{
												break;
											}
										}
										break;
									}
								}
								num7++;
								num9 += 164;
								num8 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)ptr8 + 8L));
							}
							while (num7 < *(int*)(num8 + 8));
						}
					}
				}
				num2++;
				num3 += 8;
			}
			while (num2 < *(int*)((ulong)(nint)info + 96uL));
		}
		m_lstModels.Clear();
		int num13 = 0;
		if (0 < *(int*)((ulong)(nint)info + 96uL))
		{
			granny_file_info* ptr11 = (granny_file_info*)((ulong)(nint)info + 100uL);
			long num14 = 0L;
			do
			{
				GrannyModel grannyModel = new GrannyModel(this);
				if (grannyModel.Attach((granny_model*)(*(ulong*)(num14 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr11)))))
				{
					m_lstModels.Add(grannyModel);
					num13++;
					num14 += 8;
					continue;
				}
				return false;
			}
			while (num13 < *(int*)((ulong)(nint)info + 96uL));
		}
		m_lstAnimations.Clear();
		int num15 = 0;
		if (0 < *(int*)((ulong)(nint)info + 120uL))
		{
			granny_file_info* ptr12 = (granny_file_info*)((ulong)(nint)info + 124uL);
			long num16 = 0L;
			do
			{
				GrannyAnimation grannyAnimation = new GrannyAnimation(this);
				if (grannyAnimation.Attach((granny_animation*)(*(ulong*)(num16 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr12)))))
				{
					m_lstAnimations.Add(grannyAnimation);
					num15++;
					num16 += 8;
					continue;
				}
				return false;
			}
			while (num15 < *(int*)((ulong)(nint)info + 120uL));
		}
		m_lstMeshes.Clear();
		int num17 = 0;
		if (0 < *(int*)((ulong)(nint)info + 84uL))
		{
			granny_file_info* ptr13 = (granny_file_info*)((ulong)(nint)info + 88uL);
			long num18 = 0L;
			do
			{
				GrannyMesh grannyMesh = new GrannyMesh(this);
				if (grannyMesh.Attach((granny_mesh*)(*(ulong*)(num18 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr13)))))
				{
					m_lstMeshes.Add(grannyMesh);
					num17++;
					num18 += 8;
					continue;
				}
				return false;
			}
			while (num17 < *(int*)((ulong)(nint)info + 84uL));
		}
		m_lstMaterials.Clear();
		int num19 = 0;
		if (0 < *(int*)((ulong)(nint)info + 36uL))
		{
			granny_file_info* ptr14 = (granny_file_info*)((ulong)(nint)info + 40uL);
			long num20 = 0L;
			do
			{
				GrannyMaterial grannyMaterial = new GrannyMaterial(this);
				if (grannyMaterial.Attach((granny_material*)(*(ulong*)(num20 + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr14)))))
				{
					m_lstMaterials.Add(grannyMaterial);
					num19++;
					num20 += 8;
					continue;
				}
				return false;
			}
			while (num19 < *(int*)((ulong)(nint)info + 36uL));
		}
		return true;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyFileInfoType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_info;
	}

	private unsafe granny_data_type_definition* GetStaticGrannyType(CurveDataType fmt)
	{
		//IL_00c9: Expected I, but got I8
		return fmt switch
		{
			CurveDataType.DaKeyframes32fType => global::_003CModule_003E.GrannyCurveDataDaKeyframes32fType, 
			CurveDataType.DaK32fC32fType => global::_003CModule_003E.GrannyCurveDataDaK32fC32fType, 
			CurveDataType.DaK16uC16uType => global::_003CModule_003E.GrannyCurveDataDaK16uC16uType, 
			CurveDataType.DaK8uC8uType => global::_003CModule_003E.GrannyCurveDataDaK8uC8uType, 
			CurveDataType.D3K16uC16uType => global::_003CModule_003E.GrannyCurveDataD3K16uC16uType, 
			CurveDataType.D3K8uC8uType => global::_003CModule_003E.GrannyCurveDataD3K8uC8uType, 
			CurveDataType.D4nK16uC15uType => global::_003CModule_003E.GrannyCurveDataD4nK16uC15uType, 
			CurveDataType.D4nK8uC7uType => global::_003CModule_003E.GrannyCurveDataD4nK8uC7uType, 
			CurveDataType.DaIdentityType => global::_003CModule_003E.GrannyCurveDataDaIdentityType, 
			CurveDataType.DaConstant32fType => global::_003CModule_003E.GrannyCurveDataDaConstant32fType, 
			CurveDataType.D3Constant32fType => global::_003CModule_003E.GrannyCurveDataD3Constant32fType, 
			CurveDataType.D4Constant32fType => global::_003CModule_003E.GrannyCurveDataD4Constant32fType, 
			CurveDataType.D9I1K16uC16uType => global::_003CModule_003E.GrannyCurveDataD9I1K16uC16uType, 
			CurveDataType.D9I3K16uC16uType => global::_003CModule_003E.GrannyCurveDataD9I3K16uC16uType, 
			CurveDataType.D9I1K8uC8uType => global::_003CModule_003E.GrannyCurveDataD9I1K8uC8uType, 
			CurveDataType.D9I3K8uC8uType => global::_003CModule_003E.GrannyCurveDataD9I3K8uC8uType, 
			CurveDataType.D3I1K32fC32fType => global::_003CModule_003E.GrannyCurveDataD3I1K32fC32fType, 
			CurveDataType.D3I1K16uC16uType => global::_003CModule_003E.GrannyCurveDataD3I1K16uC16uType, 
			CurveDataType.D3I1K8uC8uType => global::_003CModule_003E.GrannyCurveDataD3I1K8uC8uType, 
			_ => null, 
		};
	}

	private unsafe float* GetStaticGrannyConstant(CurveConstant cnst)
	{
		//IL_0047: Expected I, but got I8
		return cnst switch
		{
			CurveConstant.IdentityPosition => global::_003CModule_003E.GrannyCurveIdentityPosition, 
			CurveConstant.IdentityNormal => global::_003CModule_003E.GrannyCurveIdentityNormal, 
			CurveConstant.IdentityOrientation => global::_003CModule_003E.GrannyCurveIdentityOrientation, 
			CurveConstant.IdentityScaleShear => global::_003CModule_003E.GrannyCurveIdentityScaleShear, 
			CurveConstant.IdentityScale => global::_003CModule_003E.GrannyCurveIdentityScale, 
			CurveConstant.IdentityShear => global::_003CModule_003E.GrannyCurveIdentityShear, 
			_ => null, 
		};
	}

	private unsafe void GetStaticGrannyTypeArray(IEnumerable<CurveDataType> types, vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* definitionHolder)
	{
		//IL_0118: Expected I, but got I8
		foreach (CurveDataType type in types)
		{
			granny_data_type_definition* ptr;
			switch (type)
			{
			case CurveDataType.DaKeyframes32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaKeyframes32fType;
				break;
			case CurveDataType.DaK32fC32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaK32fC32fType;
				break;
			case CurveDataType.DaK16uC16uType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaK16uC16uType;
				break;
			case CurveDataType.DaK8uC8uType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaK8uC8uType;
				break;
			case CurveDataType.D3K16uC16uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3K16uC16uType;
				break;
			case CurveDataType.D3K8uC8uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3K8uC8uType;
				break;
			case CurveDataType.D4nK16uC15uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD4nK16uC15uType;
				break;
			case CurveDataType.D4nK8uC7uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD4nK8uC7uType;
				break;
			case CurveDataType.DaIdentityType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaIdentityType;
				break;
			case CurveDataType.DaConstant32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataDaConstant32fType;
				break;
			case CurveDataType.D3Constant32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3Constant32fType;
				break;
			case CurveDataType.D4Constant32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataD4Constant32fType;
				break;
			case CurveDataType.D9I1K16uC16uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD9I1K16uC16uType;
				break;
			case CurveDataType.D9I3K16uC16uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD9I3K16uC16uType;
				break;
			case CurveDataType.D9I1K8uC8uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD9I1K8uC8uType;
				break;
			case CurveDataType.D9I3K8uC8uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD9I3K8uC8uType;
				break;
			case CurveDataType.D3I1K32fC32fType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3I1K32fC32fType;
				break;
			case CurveDataType.D3I1K16uC16uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3I1K16uC16uType;
				break;
			case CurveDataType.D3I1K8uC8uType:
				ptr = global::_003CModule_003E.GrannyCurveDataD3I1K8uC8uType;
				break;
			default:
				ptr = null;
				continue;
			}
			if (ptr != null)
			{
				global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002Epush_back(definitionHolder, &ptr);
			}
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool MakeGrannyCurveParams(CurveCompressionParameters managedParam, granny_compress_curve_parameters* nativeParam, vector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E* definitionHolder)
	{
		//IL_013b: Expected I8, but got I
		//IL_0131: Expected I, but got I8
		//IL_025c: Expected I8, but got I
		//IL_0252: Expected I, but got I8
		//IL_02be: Expected I8, but got I
		//IL_02dc: Expected I8, but got I
		//IL_02b4: Expected I, but got I8
		*(int*)((ulong)(nint)nativeParam + 4uL) = (managedParam.AllowDegreeReduction ? 1 : 0);
		*(int*)((ulong)(nint)nativeParam + 8uL) = (managedParam.AllowReductionOnMissedTolerance ? 1 : 0);
		*(float*)((ulong)(nint)nativeParam + 16uL) = managedParam.C0Threshold;
		*(float*)((ulong)(nint)nativeParam + 20uL) = managedParam.C1Threshold;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)nativeParam + 36L), (long)(managedParam.ConstantCompressionType switch
		{
			CurveDataType.DaKeyframes32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaKeyframes32fType, 
			CurveDataType.DaK32fC32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaK32fC32fType, 
			CurveDataType.DaK16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataDaK16uC16uType, 
			CurveDataType.DaK8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataDaK8uC8uType, 
			CurveDataType.D3K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD3K16uC16uType, 
			CurveDataType.D3K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD3K8uC8uType, 
			CurveDataType.D4nK16uC15uType => (nint)global::_003CModule_003E.GrannyCurveDataD4nK16uC15uType, 
			CurveDataType.D4nK8uC7uType => (nint)global::_003CModule_003E.GrannyCurveDataD4nK8uC7uType, 
			CurveDataType.DaIdentityType => (nint)global::_003CModule_003E.GrannyCurveDataDaIdentityType, 
			CurveDataType.DaConstant32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaConstant32fType, 
			CurveDataType.D3Constant32fType => (nint)global::_003CModule_003E.GrannyCurveDataD3Constant32fType, 
			CurveDataType.D4Constant32fType => (nint)global::_003CModule_003E.GrannyCurveDataD4Constant32fType, 
			CurveDataType.D9I1K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I1K16uC16uType, 
			CurveDataType.D9I3K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I3K16uC16uType, 
			CurveDataType.D9I1K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I1K8uC8uType, 
			CurveDataType.D9I3K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I3K8uC8uType, 
			CurveDataType.D3I1K32fC32fType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K32fC32fType, 
			CurveDataType.D3I1K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K16uC16uType, 
			CurveDataType.D3I1K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K8uC8uType, 
			_ => 0, 
		}));
		*(int*)nativeParam = managedParam.DesiredDegree;
		*(float*)((ulong)(nint)nativeParam + 12uL) = managedParam.ErrorTolerance;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)nativeParam + 44L), (long)(managedParam.IdentityCompressionType switch
		{
			CurveDataType.DaKeyframes32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaKeyframes32fType, 
			CurveDataType.DaK32fC32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaK32fC32fType, 
			CurveDataType.DaK16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataDaK16uC16uType, 
			CurveDataType.DaK8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataDaK8uC8uType, 
			CurveDataType.D3K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD3K16uC16uType, 
			CurveDataType.D3K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD3K8uC8uType, 
			CurveDataType.D4nK16uC15uType => (nint)global::_003CModule_003E.GrannyCurveDataD4nK16uC15uType, 
			CurveDataType.D4nK8uC7uType => (nint)global::_003CModule_003E.GrannyCurveDataD4nK8uC7uType, 
			CurveDataType.DaIdentityType => (nint)global::_003CModule_003E.GrannyCurveDataDaIdentityType, 
			CurveDataType.DaConstant32fType => (nint)global::_003CModule_003E.GrannyCurveDataDaConstant32fType, 
			CurveDataType.D3Constant32fType => (nint)global::_003CModule_003E.GrannyCurveDataD3Constant32fType, 
			CurveDataType.D4Constant32fType => (nint)global::_003CModule_003E.GrannyCurveDataD4Constant32fType, 
			CurveDataType.D9I1K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I1K16uC16uType, 
			CurveDataType.D9I3K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I3K16uC16uType, 
			CurveDataType.D9I1K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I1K8uC8uType, 
			CurveDataType.D9I3K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD9I3K8uC8uType, 
			CurveDataType.D3I1K32fC32fType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K32fC32fType, 
			CurveDataType.D3I1K16uC16uType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K16uC16uType, 
			CurveDataType.D3I1K8uC8uType => (nint)global::_003CModule_003E.GrannyCurveDataD3I1K8uC8uType, 
			_ => 0, 
		}));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)nativeParam + 52L), (long)(managedParam.IdentityVector switch
		{
			CurveConstant.IdentityPosition => (nint)global::_003CModule_003E.GrannyCurveIdentityPosition, 
			CurveConstant.IdentityNormal => (nint)global::_003CModule_003E.GrannyCurveIdentityNormal, 
			CurveConstant.IdentityOrientation => (nint)global::_003CModule_003E.GrannyCurveIdentityOrientation, 
			CurveConstant.IdentityScaleShear => (nint)global::_003CModule_003E.GrannyCurveIdentityScaleShear, 
			CurveConstant.IdentityScale => (nint)global::_003CModule_003E.GrannyCurveIdentityScale, 
			CurveConstant.IdentityShear => (nint)global::_003CModule_003E.GrannyCurveIdentityShear, 
			_ => 0, 
		}));
		GetStaticGrannyTypeArray(managedParam.PossibleCompressionTypes, definitionHolder);
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)nativeParam + 24L), (long)(nint)global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002E_005B_005D(definitionHolder, 0uL));
		int num = (int)((!managedParam.UseQuantizedCurves) ? 1 : global::_003CModule_003E.std_002Evector_003Cgranny_data_type_definition_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_data_type_definition_0020_002A_003E_0020_003E_002Esize(definitionHolder));
		*(int*)((ulong)(nint)nativeParam + 32uL) = num;
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	private unsafe bool RecompressCurve(sbyte* ComponentName, granny_curve2* Curve, float dT, [MarshalAs(UnmanagedType.U1)] bool AsQuats, int MaxDegree, granny_compress_curve_parameters* Params)
	{
		//IL_005d: Expected I, but got I8
		//IL_0028: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x2cc7db78_002E_003FbIgnoreAlways_0040_003F2_003F_003FRecompressCurve_0040GrannyFile_0040Granny_0040Firaxis_0040_0040AE_0024AAM_NPEBDAEAUgranny_curve2_0040_0040M_NHAEBUgranny_compress_curve_parameters_0040_0040_0040Z_00404_NA && !global::_003CModule_003E.GrannyCurveIsKeyframed(Curve) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BP_0040HCKGLFHN_0040GrannyCurveIsKeyframed_003F_0024CI_003F_0024CGCurve_003F_0024CJ_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FH_0040IKCIJNIH_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 121u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x2cc7db78_002E_003FbIgnoreAlways_0040_003F2_003F_003FRecompressCurve_0040GrannyFile_0040Granny_0040Firaxis_0040_0040AE_0024AAM_NPEBDAEAUgranny_curve2_0040_0040M_NHAEBUgranny_compress_curve_parameters_0040_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		int num = global::_003CModule_003E.GrannyCurveGetKnotCount(Curve);
		int num2 = global::_003CModule_003E.GrannyCurveGetDimension(Curve);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E obj);
		global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_007Bctor_007D(&obj, (ulong)(num2 * num));
		bool result;
		try
		{
			global::_003CModule_003E.GrannyCurveExtractKnotValues(Curve, 0, num, null, global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_005B_005D(&obj, 0uL), global::_003CModule_003E.GrannyCurveIdentityPosition);
			int num3;
			if (AsQuats)
			{
				global::_003CModule_003E.GrannyEnsureQuaternionContinuity(num, global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_005B_005D(&obj, 0uL));
				num3 = 1;
			}
			else
			{
				num3 = 0;
			}
			int num4 = num3 | 0x30;
			bspline_solver* ptr = global::_003CModule_003E.GrannyAllocateBSplineSolver(MaxDegree, num, num2);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out bool flag);
			granny_curve2* ptr2 = global::_003CModule_003E.GrannyCompressCurve(ptr, (uint)num4, Params, global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_005B_005D(&obj, 0uL), num2, num, dT, &flag);
			global::_003CModule_003E.GrannyDeallocateBSplineSolver(ptr);
			if (ptr2 == null)
			{
				goto IL_00e2;
			}
			vector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E* pCurvesToBeFreed = m_pCurvesToBeFreed;
			if (pCurvesToBeFreed != null)
			{
				global::_003CModule_003E.std_002Evector_003Cgranny_curve2_0020_002A_002Cstd_003A_003Aallocator_003Cgranny_curve2_0020_002A_003E_0020_003E_002Epush_back(pCurvesToBeFreed, &ptr2);
			}
			if (ptr2 == null || !flag)
			{
				goto IL_00e2;
			}
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(Curve, System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr2));
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)Curve + 8L), System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)ptr2 + 8L)));
			result = true;
			goto end_IL_0045;
			IL_00e2:
			result = false;
			end_IL_0045:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<vector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.std_002Evector_003Cfloat_002Cstd_003A_003Aallocator_003Cfloat_003E_0020_003E_002E_007Bdtor_007D(&obj);
		return result;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021GrannyFile();
			return;
		}
		try
		{
			_0021GrannyFile();
		}
		finally
		{
			//base.Finalize();
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyFile()
	{
		Dispose(A_0: false);
	}
}
