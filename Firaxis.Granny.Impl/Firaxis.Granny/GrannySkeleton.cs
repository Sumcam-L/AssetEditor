using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.Granny;

internal class GrannySkeleton : GrannyBaseObjectContext, IGrannySkeleton, IDisposable
{
	private unsafe granny_skeleton* m_pkSkeleton;

	private IList<IGrannyBone> m_lstBones = new List<IGrannyBone>();

	public unsafe virtual int LODType => *(int*)((ulong)(nint)m_pkSkeleton + 20uL);

	public virtual IList<IGrannyBone> Bones => m_lstBones;

	public unsafe virtual string Name => Marshal.PtrToStringAnsi((IntPtr)(void*)System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>(m_pkSkeleton));

	public GrannySkeleton(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}

	private void _007EGrannySkeleton()
	{
	}

	private void _0021GrannySkeleton()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe bool Attach(granny_skeleton* pkSkeleton)
	{
		//IL_003b: Expected I, but got I8
		if (pkSkeleton == null)
		{
			return false;
		}
		m_pkSkeleton = pkSkeleton;
		int num = 0;
		if (0 < *(int*)((ulong)(nint)pkSkeleton + 8uL))
		{
			do
			{
				GrannyBone grannyBone = new GrannyBone(this);
				if (grannyBone.Attach((granny_bone*)((long)num * 164L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkSkeleton + 12L)))))
				{
					m_lstBones.Add(grannyBone);
					num++;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)m_pkSkeleton + 8uL));
		}
		return true;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_skeleton*)GetGrannyObject());
		return result;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannySkeletonType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkSkeleton;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannySkeleton();
			return;
		}
		try
		{
			_0021GrannySkeleton();
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

	~GrannySkeleton()
	{
		Dispose(A_0: false);
	}
}
