using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;

namespace Firaxis.Granny;

internal class GrannyCurve : GrannyBaseObjectContext, IGrannyCurve
{
	private unsafe granny_curve2* m_pkCurve = null;

	private ECurveType m_eCurveType = ECurveType.Other;

	public unsafe virtual bool IsConstant
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			granny_curve2* pkCurve = m_pkCurve;
			if (pkCurve == null)
			{
				return true;
			}
			return global::_003CModule_003E.GrannyCurveIsConstantOrIdentity(pkCurve);
		}
	}

	public unsafe virtual bool IsIdentity
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			granny_curve2* pkCurve = m_pkCurve;
			if (pkCurve == null)
			{
				return true;
			}
			return global::_003CModule_003E.GrannyCurveIsIdentity(pkCurve);
		}
	}

	public unsafe virtual int KnotCount
	{
		get
		{
			granny_curve2* pkCurve = m_pkCurve;
			if (pkCurve == null)
			{
				return 0;
			}
			return global::_003CModule_003E.GrannyCurveGetKnotCount(pkCurve);
		}
	}

	public virtual int Dimension
	{
		get
		{
			ECurveType eCurveType = m_eCurveType;
			if (eCurveType != ECurveType.Position)
			{
				if (eCurveType != ECurveType.Orientation)
				{
					return (eCurveType == ECurveType.ScaleShear) ? 9 : 0;
				}
				return 4;
			}
			return 3;
		}
	}

	public unsafe virtual int Degree
	{
		get
		{
			granny_curve2* pkCurve = m_pkCurve;
			if (pkCurve == null)
			{
				return 0;
			}
			return global::_003CModule_003E.GrannyCurveGetDegree(pkCurve);
		}
	}

	public virtual ECurveType Type => m_eCurveType;

	public unsafe GrannyCurve(GrannyBaseObjectContext kMemContext)
		: base(kMemContext)
	{
	}//IL_0008: Expected I, but got I8


	private void _007EGrannyCurve()
	{
	}

	private void _0021GrannyCurve()
	{
	}

	public virtual string ComponentName(int iDimension)
	{
		switch (m_eCurveType)
		{
		case ECurveType.ScaleShear:
			switch (iDimension)
			{
			case 0:
				return "a00";
			case 1:
				return "a01";
			case 2:
				return "a02";
			case 3:
				return "a10";
			case 4:
				return "a11";
			case 5:
				return "a12";
			case 6:
				return "a20";
			case 7:
				return "a21";
			case 8:
				return "a22";
			}
			break;
		case ECurveType.Orientation:
			switch (iDimension)
			{
			case 3:
				return "w";
			case 2:
				return "z";
			case 1:
				return "y";
			case 0:
				return "x";
			}
			break;
		case ECurveType.Position:
			switch (iDimension)
			{
			case 2:
				return "z";
			case 1:
				return "y";
			case 0:
				return "x";
			}
			break;
		}
		return "Unknown!";
	}

	public unsafe virtual float[] Sample(float fLocalClock, float fDuration, [MarshalAs(UnmanagedType.U1)] bool bNormalize, [MarshalAs(UnmanagedType.U1)] bool bBackLoop, [MarshalAs(UnmanagedType.U1)] bool bForwardLoop)
	{
		//IL_000f: Expected I, but got I8
		//IL_0073: Expected I, but got I8
		float[] array = new float[Dimension];
		float* ptr = null;
		switch (m_eCurveType)
		{
		case ECurveType.ScaleShear:
			ptr = global::_003CModule_003E.GrannyCurveIdentityScaleShear;
			break;
		case ECurveType.Orientation:
			ptr = global::_003CModule_003E.GrannyCurveIdentityOrientation;
			break;
		case ECurveType.Position:
			ptr = global::_003CModule_003E.GrannyCurveIdentityPosition;
			break;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY08M _0024ArrayType_0024_0024_0024BY08M2);
		global::_003CModule_003E.GrannyEvaluateCurveAtT(Dimension, false, false, m_pkCurve, false, fDuration, fLocalClock, (float*)(&_0024ArrayType_0024_0024_0024BY08M2), ptr);
		int num = 0;
		if (0 < Dimension)
		{
			_0024ArrayType_0024_0024_0024BY08M* ptr2 = &_0024ArrayType_0024_0024_0024BY08M2;
			do
			{
				array[num] = *(float*)ptr2;
				num++;
				ptr2 = (_0024ArrayType_0024_0024_0024BY08M*)((ulong)(nint)ptr2 + 4uL);
			}
			while (num < Dimension);
		}
		return array;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	internal unsafe bool Attach(granny_curve2* pkCurve, ECurveType eType)
	{
		if (pkCurve == null)
		{
			return false;
		}
		m_pkCurve = pkCurve;
		m_eCurveType = eType;
		return true;
	}

	protected unsafe override granny_data_type_definition* GetGrannyType()
	{
		return global::_003CModule_003E.GrannyCurve2Type;
	}

	protected unsafe override void* GetGrannyObject()
	{
		return m_pkCurve;
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

	~GrannyCurve()
	{
		Dispose(A_0: false);
	}
}
