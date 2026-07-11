using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyAnimation : GrannyBaseObjectContext, IGrannyAnimation, IDisposable
{
	private unsafe granny_animation* m_pkAnimation = null;

	private List<IGrannyTrackGroup> m_lstTrackGroups;

	private List<int> m_lstEventCodes;

	public virtual List<int> EventCodes => m_lstEventCodes;

	public virtual List<IGrannyTrackGroup> TrackGroups => m_lstTrackGroups;

	public unsafe virtual float Oversampling => *(float*)((ulong)(nint)m_pkAnimation + 16uL);

	public unsafe virtual float TimeStep => *(float*)((ulong)(nint)m_pkAnimation + 12uL);

	public unsafe virtual float Duration => *(float*)((ulong)(nint)m_pkAnimation + 8uL);

	public unsafe virtual string Name
	{
        get
        {
            if (m_pkAnimation == null) return null;
            ulong ptr = *(ulong*)m_pkAnimation;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
        set
        {
            IntPtr hglobal = Marshal.StringToHGlobalAnsi(value ?? "");
            *(ulong*)m_pkAnimation = (ulong)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, (sbyte*)hglobal.ToPointer());
            Marshal.FreeHGlobal(hglobal);
        }
    }

	public unsafe GrannyAnimation(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyAnimation()
	{
	}

	private void _0021GrannyAnimation()
	{
	}

	public unsafe virtual float[] SampleBone(IGrannyModel kModel, string szBoneName, float fTime)
	{
		//IL_0042: Expected I, but got I8
		sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(szBoneName).ToPointer();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BA_0040M _0024ArrayType_0024_0024_0024BY0BA_0040M2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0BA_0040M _0024ArrayType_0024_0024_0024BY0BA_0040M3);
		global::_003CModule_003E.Firaxis_002EGranny_002ESampleAnimation(((GrannyModel)kModel).GetModel(), m_pkAnimation, ptr, fTime, (float*)(&_0024ArrayType_0024_0024_0024BY0BA_0040M2), (float*)(&_0024ArrayType_0024_0024_0024BY0BA_0040M3));
		float[] array = new float[16];
		int num = 0;
		_0024ArrayType_0024_0024_0024BY0BA_0040M* ptr2 = &_0024ArrayType_0024_0024_0024BY0BA_0040M2;
		do
		{
			array[num] = *(float*)ptr2;
			num++;
			ptr2 = (_0024ArrayType_0024_0024_0024BY0BA_0040M*)((ulong)(nint)ptr2 + 4uL);
		}
		while (num < 16);
		Marshal.FreeHGlobal((IntPtr)ptr);
		return array;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach(m_pkAnimation);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_animation* pkAnimation)
	{
		//IL_0053: Expected I, but got I8
		if (pkAnimation == null)
		{
			return false;
		}
		m_pkAnimation = pkAnimation;
		m_lstTrackGroups = new List<IGrannyTrackGroup>();
		m_lstEventCodes = new List<int>();
		int num = 0;
		if (0 < *(int*)((ulong)(nint)m_pkAnimation + 20uL))
		{
			long num2 = 0L;
			do
			{
				GrannyTrackGroup grannyTrackGroup = new GrannyTrackGroup(this);
				if (grannyTrackGroup.Attach((granny_track_group*)(*(ulong*)(System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>((void*)((long)(nint)m_pkAnimation + 24L)) + num2))))
				{
					m_lstTrackGroups.Add(grannyTrackGroup);
					num++;
					num2 += 8;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)m_pkAnimation + 20uL));
		}
		return true;
	}

	internal unsafe granny_animation* GetAnimation()
	{
		return m_pkAnimation;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyAnimationType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkAnimation;
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

	~GrannyAnimation()
	{
		Dispose(A_0: false);
	}
}
