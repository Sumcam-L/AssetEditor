using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.Granny;

internal class GrannyMesh : GrannyBaseObjectContext, IGrannyMesh
{
	private unsafe granny_mesh* m_pkMesh;

	private List<IGrannyMaterial> m_pkMaterialBindings = new List<IGrannyMaterial>();

	private List<IGrannyTriMaterialGroup> m_pkTriangleMaterialGroups = new List<IGrannyTriMaterialGroup>();

	private List<string> m_pkBoneBindings = new List<string>();

	public virtual List<string> BoneBindings => m_pkBoneBindings;

	public virtual List<IGrannyTriMaterialGroup> TriangleMaterialGroups => m_pkTriangleMaterialGroups;

	public virtual List<IGrannyMaterial> MaterialBindings => m_pkMaterialBindings;

	public unsafe virtual MeshBounds BoundingBox
	{
		get
		{
			//IL_0044: Expected I, but got I8
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXBoundBox fGXBoundBox);
			global::_003CModule_003E.FGXBoundBox_002EDegenerate(&fGXBoundBox);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector);
			ulong num = (ulong)(int)(*(uint*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMesh + 8L)) + 8));
			uint num2 = 0u;
			if (0 < num)
			{
				do
				{
					global::_003CModule_003E.GrannyGetSingleVertex((granny_vertex_data*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkMesh + 8L)), (int)num2, global::_003CModule_003E.GrannyP3VertexType, &fGXVector);
					global::_003CModule_003E.FGXBoundBox_002EExpand(&fGXBoundBox, &fGXVector);
					num2++;
				}
				while (num2 < num);
			}
			return new MeshBounds(new Point3F(*(float*)(&fGXBoundBox), System.Runtime.CompilerServices.Unsafe.As<FGXBoundBox, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXBoundBox, 4)), System.Runtime.CompilerServices.Unsafe.As<FGXBoundBox, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXBoundBox, 8))), new Point3F(System.Runtime.CompilerServices.Unsafe.As<FGXBoundBox, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXBoundBox, 12)), System.Runtime.CompilerServices.Unsafe.As<FGXBoundBox, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXBoundBox, 16)), System.Runtime.CompilerServices.Unsafe.As<FGXBoundBox, float>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref fGXBoundBox, 20))));
		}
	}

	public unsafe virtual int IndexCount
	{
		get
		{
			granny_mesh* pkMesh = m_pkMesh;
			if (pkMesh != null && System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkMesh + 28L)) != 0L)
			{
				return global::_003CModule_003E.GrannyGetMeshIndexCount(pkMesh);
			}
			return 0;
		}
	}

	public unsafe virtual int VertexCount
	{
		get
		{
			granny_mesh* pkMesh = m_pkMesh;
			if (pkMesh != null)
			{
				ulong num = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMesh + 8L));
				if (num != 0L)
				{
					return *(int*)(num + 8);
				}
			}
			return 0;
		}
	}

	public unsafe virtual string Name
	{
		get
		{
            //IL_001d->IL001d: Incompatible stack types: I8 vs Ref
            if (m_pkMesh == null) return null;
            ulong ptr = *(ulong*)m_pkMesh;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
		set
		{
			//IL_0025: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_pkMesh, (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr));
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public GrannyMesh(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}

	private void _007EGrannyMesh()
	{
	}

	private void _0021GrannyMesh()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool AddMaterialBinding(IGrannyMaterial kMaterial)
	{
		//IL_0022: Expected I, but got I8
		//IL_0016: Expected I, but got I8
		//IL_0054: Expected I8, but got I
		//IL_0076: Expected I4, but got I8
		//IL_009d: Expected I8, but got I
		granny_mesh* pkMesh = m_pkMesh;
		int num = *(int*)((ulong)(nint)pkMesh + 36uL);
		granny_material_binding* ptr = (granny_material_binding*)((num != 0) ? System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMesh + 40L)) : 0);
		*(int*)((ulong)(nint)pkMesh + 36uL) = num + 1;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_pkMesh + 40L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)(*(int*)((ulong)(nint)m_pkMesh + 36uL)) * 8uL));
		granny_mesh* pkMesh2 = m_pkMesh;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkMesh2 + 40L)), ptr, (uint)((long)(*(int*)((ulong)(nint)pkMesh2 + 36uL) - 1) * 8L));
		granny_mesh* pkMesh3 = m_pkMesh;
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(*(int*)((ulong)(nint)pkMesh3 + 36uL) - 1) * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkMesh3 + 40L))), (long)(nint)((GrannyMaterial)kMaterial).GetMaterial());
		m_pkMaterialBindings.Add(kMaterial);
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool RemoveMaterialBinding(IGrannyMaterial kMaterial)
	{
		//IL_0022: Expected I, but got I8
		//IL_0015: Expected I, but got I8
		//IL_00bb: Expected I8, but got I
		//IL_00d5: Expected I4, but got I8
		//IL_0106: Expected I4, but got I8
		granny_mesh* pkMesh = m_pkMesh;
		int num = *(int*)((ulong)(nint)pkMesh + 36uL);
		granny_material_binding* ptr = (granny_material_binding*)((num != 0) ? System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkMesh + 40L)) : 0);
		int num2 = 0;
		if (0 < num)
		{
			long num3 = 0L;
			do
			{
				pkMesh = m_pkMesh;
				if (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkMesh + 40L)) + num3)) != (nint)((GrannyMaterial)kMaterial).GetMaterial())
				{
					num2++;
					num3 += 8;
					continue;
				}
				m_pkMaterialBindings.Remove(kMaterial);
				*(int*)((ulong)(nint)m_pkMesh + 36uL) += -1;
				pkMesh = m_pkMesh;
				num = *(int*)((ulong)(nint)pkMesh + 36uL);
				if (num == 0)
				{
					System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)pkMesh + 40L), 0L);
					return true;
				}
				System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_pkMesh + 40L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)num * 8uL));
				// IL cpblk instruction
				System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMesh + 40L)), ptr, (uint)(num2 * 8L));
				pkMesh = m_pkMesh;
				// IL cpblk instruction
				System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)((long)num2 * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkMesh + 40L))), (void*)((long)(num2 + 1) * 8L + (nint)ptr), (uint)((long)(*(int*)((ulong)(nint)pkMesh + 36uL) - num2) * 8L));
				return true;
			}
			while (num2 < *(int*)((ulong)(nint)pkMesh + 36uL));
		}
		return false;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_mesh*)GetGrannyObject());
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_mesh* pkMesh)
	{
		//IL_00c0: Expected I, but got I8
		//IL_004e: Expected I, but got I8
		//IL_012f: Expected I, but got I8
		if (pkMesh == null)
		{
			return false;
		}
		m_pkMesh = pkMesh;
		m_pkMaterialBindings.Clear();
		int num = 0;
		if (0 < *(int*)((ulong)(nint)m_pkMesh + 36uL))
		{
			long num2 = 0L;
			do
			{
				GrannyMaterial grannyMaterial = new GrannyMaterial(this);
				if (grannyMaterial.Attach((granny_material*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMesh + 40L)) + num2))))
				{
					m_pkMaterialBindings.Add(grannyMaterial);
					num++;
					num2 += 8;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)m_pkMesh + 36uL));
		}
		m_pkTriangleMaterialGroups.Clear();
		int num3 = 0;
		if (0 < *(int*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkMesh + 28L)))
		{
			do
			{
				GrannyTriMaterialGroup grannyTriMaterialGroup = new GrannyTriMaterialGroup(this);
				granny_tri_material_group* pkTriangleMaterialGroup = (granny_tri_material_group*)((long)num3 * 12L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMesh + 28L)) + 4)));
				if (grannyTriMaterialGroup.Attach(pkTriangleMaterialGroup))
				{
					m_pkTriangleMaterialGroups.Add(grannyTriMaterialGroup);
					num3++;
					continue;
				}
				return false;
			}
			while (num3 < *(int*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkMesh + 28L)));
		}
		m_pkBoneBindings.Clear();
		int num4 = 0;
		if (0 < *(int*)((ulong)(nint)m_pkMesh + 48uL))
		{
			long num5 = 0L;
			do
			{
				m_pkBoneBindings.Add(new string((sbyte*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkMesh + 52L)) + num5))));
				num4++;
				num5 += 44;
			}
			while (num4 < *(int*)((ulong)(nint)m_pkMesh + 48uL));
		}
		return true;
	}

	internal unsafe granny_mesh* GetMesh()
	{
		return m_pkMesh;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyMeshType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkMesh;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyMesh();
			return;
		}
		try
		{
			_0021GrannyMesh();
		}
		finally
		{
			
		}
	}

	public virtual  void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~GrannyMesh()
	{
		Dispose(A_0: false);
	}
}
