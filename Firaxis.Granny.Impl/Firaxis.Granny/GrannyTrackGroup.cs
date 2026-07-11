using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyTrackGroup : GrannyBaseObjectContext, IGrannyTrackGroup, IDisposable
{
	private unsafe granny_track_group* m_pkTrackGroup = null;

	private List<IGrannyTransformTrack> m_lstTransformTracks = new List<IGrannyTransformTrack>();

	public virtual List<IGrannyTransformTrack> TransformTracks => m_lstTransformTracks;

	public unsafe virtual string Name
	{
		get
		{
            //IL_001d->IL001d: Incompatible stack types: I8 vs Ref
            if (m_pkTrackGroup == null) return null;
            ulong ptr = *(ulong*)m_pkTrackGroup;
            return ptr == 0 ? "" : Marshal.PtrToStringAnsi((IntPtr)ptr);
        }
		set
		{
			//IL_0025: Expected I8, but got I
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			System.Runtime.CompilerServices.Unsafe.WriteUnaligned(m_pkTrackGroup, (long)(nint)global::_003CModule_003E.GrannyMemoryArenaPushString(m_pkMemoryArena, ptr));
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe GrannyTrackGroup(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyTrackGroup()
	{
	}

	private void _0021GrannyTrackGroup()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_track_group*)GetGrannyObject());
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_track_group* pkTrackGroup)
	{
		//IL_0028: Expected I, but got I8
		//IL_0041: Expected I, but got I8
		if (pkTrackGroup == null)
		{
			return false;
		}
		m_pkTrackGroup = pkTrackGroup;
		m_lstTransformTracks.Clear();
		int num = 0;
		if (0 < *(int*)((ulong)(nint)pkTrackGroup + 20uL))
		{
			granny_track_group* ptr = (granny_track_group*)((ulong)(nint)pkTrackGroup + 24uL);
			do
			{
				GrannyTransformTrack grannyTransformTrack = new GrannyTransformTrack(this);
				if (grannyTransformTrack.Attach((granny_transform_track*)((long)num * 60L + System.Runtime.CompilerServices.Unsafe.ReadUnaligned<long>(ptr))))
				{
					m_lstTransformTracks.Add(grannyTransformTrack);
					num++;
					continue;
				}
				return false;
			}
			while (num < *(int*)((ulong)(nint)pkTrackGroup + 20uL));
		}
		return true;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyTrackGroupType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkTrackGroup;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyTrackGroup();
			return;
		}
		try
		{
			_0021GrannyTrackGroup();
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

	~GrannyTrackGroup()
	{
		Dispose(A_0: false);
	}
}
