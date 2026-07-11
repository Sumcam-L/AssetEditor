using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyTransformTrack : GrannyBaseObjectContext, IGrannyTransformTrack, IDisposable
{
	private unsafe granny_transform_track* m_pkTransformTrack;

	private IGrannyCurve m_pkPosCurve;

	private IGrannyCurve m_pkOriCurve;

	private IGrannyCurve m_pkSSCurve;

	public virtual IGrannyCurve ScaleShearCurve => m_pkSSCurve;

	public virtual IGrannyCurve OrientationCurve => m_pkOriCurve;

	public virtual IGrannyCurve PositionCurve => m_pkPosCurve;

	public unsafe virtual int Flags
	{
		get
		{
			granny_transform_track* pkTransformTrack = m_pkTransformTrack;
			if (pkTransformTrack != null)
			{
				return *(int*)((ulong)(nint)pkTransformTrack + 8uL);
			}
			return 0;
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			//IL_001d->IL001d: Incompatible stack types: I8 vs Ref
			granny_transform_track* pkTransformTrack = m_pkTransformTrack;
			sbyte* ptr;
			if (pkTransformTrack != null)
			{
				ulong num = System.Runtime.CompilerServices.Unsafe.ReadUnaligned<ulong>(pkTransformTrack);
                ptr = (sbyte*)num;
            }
			else
			{
				ptr = (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040);
			}
			return Marshal.PtrToStringAnsi((IntPtr)ptr) ?? "";
		}
	}

	public GrannyTransformTrack(GrannyBaseObjectContext kMemContext) : base(kMemContext)
    {
		m_pkPosCurve = new GrannyCurve(kMemContext);
		m_pkOriCurve = new GrannyCurve(kMemContext);
		m_pkSSCurve = new GrannyCurve(kMemContext);
	}

	private void _007EGrannyTransformTrack()
	{
	}

	private void _0021GrannyTransformTrack()
	{
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe override bool RestoreReferences()
	{
		bool result = base.RestoreReferences();
		Attach((granny_transform_track*)GetGrannyObject());
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_transform_track* pkTransformTrack)
	{
		//IL_0024: Expected I, but got I8
		//IL_0047: Expected I, but got I8
		//IL_006a: Expected I, but got I8
		if (pkTransformTrack == null)
		{
			return false;
		}
		m_pkTransformTrack = pkTransformTrack;
		GrannyCurve grannyCurve = new GrannyCurve(this);
		if (!grannyCurve.Attach((granny_curve2*)((ulong)(nint)m_pkTransformTrack + 28uL), ECurveType.Position))
		{
			return false;
		}
		m_pkPosCurve = grannyCurve;
		grannyCurve = new GrannyCurve(this);
		if (!grannyCurve.Attach((granny_curve2*)((ulong)(nint)m_pkTransformTrack + 12uL), ECurveType.Orientation))
		{
			return false;
		}
		m_pkOriCurve = grannyCurve;
		grannyCurve = new GrannyCurve(this);
		if (!grannyCurve.Attach((granny_curve2*)((ulong)(nint)m_pkTransformTrack + 44uL), ECurveType.ScaleShear))
		{
			return false;
		}
		m_pkSSCurve = grannyCurve;
		return true;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyTransformTrackType;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkTransformTrack;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGrannyTransformTrack();
			return;
		}
		try
		{
			_0021GrannyTransformTrack();
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

	~GrannyTransformTrack()
	{
		Dispose(A_0: false);
	}
}
