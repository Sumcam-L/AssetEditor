using System;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;

namespace Firaxis.Granny;

internal class GrannyTransform : GrannyBaseObjectContext, IGrannyTransform, IDisposable
{
	private unsafe granny_transform* m_pkTransform;

	public unsafe virtual float[] ScaleShear
	{
		get
		{
			//IL_0015: Expected I, but got I8
			//IL_0023: Expected I, but got I8
			float[] array = new float[9];
			int num = 0;
			granny_transform* ptr = (granny_transform*)((ulong)(nint)m_pkTransform + 16uL);
			do
			{
				array[num] = *(float*)ptr;
				num++;
				ptr = (granny_transform*)((ulong)(nint)ptr + 4uL);
			}
			while (num < 9);
			return array;
		}
	}

	public unsafe virtual float[] Orientation
	{
		get
		{
			//IL_0014: Expected I, but got I8
			//IL_0022: Expected I, but got I8
			float[] array = new float[4];
			int num = 0;
			granny_transform* ptr = (granny_transform*)((ulong)(nint)m_pkTransform + 16uL);
			do
			{
				array[num] = *(float*)ptr;
				num++;
				ptr = (granny_transform*)((ulong)(nint)ptr + 4uL);
			}
			while (num < 4);
			return array;
		}
	}

	public unsafe virtual float[] Position
	{
		get
		{
			//IL_0013: Expected I, but got I8
			//IL_0021: Expected I, but got I8
			float[] array = new float[3];
			int num = 0;
			granny_transform* ptr = (granny_transform*)((ulong)(nint)m_pkTransform + 4uL);
			do
			{
				array[num] = *(float*)ptr;
				num++;
				ptr = (granny_transform*)((ulong)(nint)ptr + 4uL);
			}
			while (num < 3);
			return array;
		}
	}

	public unsafe virtual ETransformFlags Flags => *(ETransformFlags*)m_pkTransform;

	public GrannyTransform(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}

	private void _007EGrannyTransform()
	{
	}

	private void _0021GrannyTransform()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_transform* pkTransform)
	{
		if (pkTransform == null)
		{
			return false;
		}
		m_pkTransform = pkTransform;
		return true;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyTransformType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkTransform;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyTransform();
			return;
		}
		try
		{
			_0021GrannyTransform();
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

	~GrannyTransform()
	{
		Dispose(A_0: false);
	}
}
