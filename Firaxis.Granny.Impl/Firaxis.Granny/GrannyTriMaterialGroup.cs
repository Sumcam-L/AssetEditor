using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.Granny;

internal class GrannyTriMaterialGroup : GrannyBaseObjectContext, IGrannyTriMaterialGroup, IDisposable
{
	private unsafe granny_tri_material_group* m_pkTriMaterialGroup;

	public unsafe virtual int TriCount => *(int*)((ulong)(nint)m_pkTriMaterialGroup + 8uL);

	public unsafe virtual int TriFirst => *(int*)((ulong)(nint)m_pkTriMaterialGroup + 4uL);

	public unsafe virtual int MaterialIndex => *(int*)m_pkTriMaterialGroup;

	public GrannyTriMaterialGroup(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}

	private void _007EGrannyTriMaterialGroup()
	{
	}

	private void _0021GrannyTriMaterialGroup()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_tri_material_group* pkTriangleMaterialGroup)
	{
		if (pkTriangleMaterialGroup == null)
		{
			return false;
		}
		m_pkTriMaterialGroup = pkTriangleMaterialGroup;
		return true;
	}

	internal unsafe granny_tri_material_group* GetTriangleMaterialGroup()
	{
		return m_pkTriMaterialGroup;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyMaterialBindingType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkTriMaterialGroup;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyTriMaterialGroup();
			return;
		}
		try
		{
			_0021GrannyTriMaterialGroup();
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

	~GrannyTriMaterialGroup()
	{
		Dispose(A_0: false);
	}
}
