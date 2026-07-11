using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyModel : GrannyBaseObjectContext, IGrannyModel, IDisposable
{
	private unsafe granny_model* m_pkModel = null;

	private List<IGrannyMesh> m_lstMeshBindings = new List<IGrannyMesh>();

	private IGrannySkeleton m_pkSkeleton;

	private IGrannyTransform m_kInitialPlacement;

	private List<IGrannyLod> m_lods = new List<IGrannyLod>();

	public virtual List<IGrannyLod> Lods => m_lods;

	public virtual List<IGrannyMesh> MeshBindings => m_lstMeshBindings;

	public virtual IGrannySkeleton Skeleton => m_pkSkeleton;

	public virtual IGrannyTransform InitialPlacement => m_kInitialPlacement;

	public unsafe virtual string Name
	{
		get
		{
            //IL_001d->IL001d: Incompatible stack types: I8 vs Ref
            if (m_pkModel == null) return null;
            ulong ptr = *(ulong*)m_pkModel;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
		set
		{
			//IL_0025: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_pkModel, (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr));
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe GrannyModel(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyModel()
	{
	}

	private void _0021GrannyModel()
	{
	}

	public unsafe virtual void RemoveMeshBinding(IGrannyMesh kMesh)
	{
		//IL_008c: Expected I, but got I8
		//IL_00ab: Expected I8, but got I
		//IL_00c4: Expected I4, but got I8
		//IL_00f4: Expected I4, but got I8
		granny_mesh* mesh = ((GrannyMesh)kMesh).GetMesh();
		int num = 0;
		granny_model* pkModel = m_pkModel;
		int num2 = *(int*)((ulong)(nint)pkModel + 84uL);
		if (0 >= num2)
		{
			return;
		}
		long num3 = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkModel + 88L));
		while (System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)num3) != (nint)mesh)
		{
			num++;
			num3 += 8;
			if (num >= num2)
			{
				return;
			}
		}
		m_lstMeshBindings.RemoveAt(num);
		*(int*)((ulong)(nint)m_pkModel + 84uL) += -1;
		pkModel = m_pkModel;
		int num4 = *(int*)((ulong)(nint)pkModel + 84uL);
		if (num4 == 0)
		{
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)pkModel + 88L), 0L);
			return;
		}
		granny_model_mesh_binding* ptr = (granny_model_mesh_binding*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)pkModel + 88L));
		System.Runtime.CompilerServices.Unsafe.WriteUnaligned((void*)((long)(nint)m_pkModel + 88L), (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPush(m_pkMemoryArena, (ulong)num4 * 8uL));
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkModel + 88L)), ptr, (uint)(num * 8L));
		pkModel = m_pkModel;
		// IL cpblk instruction
		System.Runtime.CompilerServices.Unsafe.CopyBlockUnaligned((void*)((long)num * 8L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)pkModel + 88L))), (void*)((long)(num + 1) * 8L + (nint)ptr), (uint)((long)(*(int*)((ulong)(nint)pkModel + 84uL) - num) * 8L));
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach(m_pkModel);
		return result;
	}

	public override string ToString()
	{
		return Name;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_model* pkModel)
	{
		//IL_0028: Expected I, but got I8
		//IL_004b: Expected I, but got I8
		//IL_0095: Expected I, but got I8
		if (pkModel == null)
		{
			return false;
		}
		m_pkModel = pkModel;
		GrannySkeleton grannySkeleton = new GrannySkeleton(this);
		if (!grannySkeleton.Attach((granny_skeleton*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)((long)(nint)m_pkModel + 8L))))
		{
			return false;
		}
		m_pkSkeleton = grannySkeleton;
		GrannyTransform grannyTransform = new GrannyTransform(this);
		if (!grannyTransform.Attach((granny_transform*)((ulong)(nint)m_pkModel + 16uL)))
		{
			return false;
		}
		m_kInitialPlacement = grannyTransform;
		m_lstMeshBindings.Clear();
		int num = 0;
		if (0 < *(int*)((ulong)(nint)m_pkModel + 84uL))
		{
			long num2 = 0L;
			do
			{
				GrannyMesh grannyMesh = new GrannyMesh(this);
				if (grannyMesh.Attach((granny_mesh*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>((void*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkModel + 88L)) + num2))))
				{
					m_lstMeshBindings.Add(grannyMesh);
					num++;
					num2 += 8;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)m_pkModel + 84uL));
		}
		global::_003CModule_003E.Firaxis_002EGranny_002E_003FA0xe79110d2_002EGetLods(this, Lods);
		return true;
	}

	internal unsafe granny_model* GetModel()
	{
		return m_pkModel;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyModelType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkModel;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyModel();
			return;
		}
		try
		{
			_0021GrannyModel();
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

	~GrannyModel()
	{
		Dispose(A_0: false);
	}
}
