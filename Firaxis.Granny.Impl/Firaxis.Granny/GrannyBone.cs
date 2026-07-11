using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyBone : GrannyBaseObjectContext, IGrannyBone, IDisposable
{
	private unsafe granny_bone* m_pkBone = null;

	private float[] m_kInverseWorld = null;

	private IGrannyTransform m_kLocalTransform = null;

	public unsafe virtual float LODError => *(float*)((ulong)(nint)m_pkBone + 144uL);

	public virtual float[] InverseWorldTransform => m_kInverseWorld;

	public virtual IGrannyTransform LocalTransform => m_kLocalTransform;

	public unsafe virtual int ParentIndex => *(int*)((ulong)(nint)m_pkBone + 8uL);

	public unsafe virtual string Name
	{
		get
		{
			//IL_001d->IL001d: Incompatible stack types: I8 vs Ref
			granny_bone* pkBone = m_pkBone;
            if (m_pkBone == null) return null;
            ulong ptr = *(ulong*)m_pkBone;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
		}
	}

	public unsafe GrannyBone(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyBone()
	{
	}

	private void _0021GrannyBone()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_bone*)GetGrannyObject());
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_bone* pkBone)
	{
		//IL_001e: Expected I, but got I8
		//IL_0040: Expected I, but got I8
		//IL_004e: Expected I, but got I8
		if (pkBone == null)
		{
			return false;
		}
		m_pkBone = pkBone;
		GrannyTransform grannyTransform = new GrannyTransform(this);
		if (!grannyTransform.Attach((granny_transform*)((ulong)(nint)pkBone + 12uL)))
		{
			return false;
		}
		m_kLocalTransform = grannyTransform;
		float[] array = (m_kInverseWorld = new float[16]);
		long num = (long)(nint)pkBone + 80L;
		int num2 = 0;
		float* ptr = (float*)num;
		do
		{
			array[num2] = *ptr;
			num2++;
			ptr = (float*)((ulong)(nint)ptr + 4uL);
		}
		while (num2 < 16);
		return true;
	}

	internal unsafe granny_bone* GetBone()
	{
		return m_pkBone;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyBoneType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkBone;
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

	~GrannyBone()
	{
		Dispose(A_0: false);
	}
}
