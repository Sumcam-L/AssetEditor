using System;

namespace Sce.Atf.Direct2D;

public struct D2dResult : IEquatable<D2dResult>
{
	public static readonly D2dResult Ok;

	public static readonly D2dResult Abord;

	public static readonly D2dResult AccessDenied;

	public static readonly D2dResult Fail;

	public static readonly D2dResult Handle;

	public static D2dResult InvalidArg;

	public static D2dResult NoInterface;

	public static D2dResult NotImplemented;

	public static D2dResult OutOfMemory;

	public static D2dResult InvalidPointer;

	public static D2dResult UnexpectedFailure;

	private readonly int m_code;

	public int Code => m_code;

	public bool IsSuccess => Code >= 0;

	public bool IsFailure => Code < 0;

	internal D2dResult(int code)
	{
		m_code = code;
	}

	internal D2dResult(uint code)
	{
		m_code = (int)code;
	}

	public bool Equals(D2dResult other)
	{
		return Code == other.Code;
	}

	public override bool Equals(object obj)
	{
		return obj is D2dResult && Equals((D2dResult)obj);
	}

	public override int GetHashCode()
	{
		return Code;
	}

	public static bool operator ==(D2dResult left, D2dResult right)
	{
		return left.Code == right.Code;
	}

	public static bool operator !=(D2dResult left, D2dResult right)
	{
		return left.Code != right.Code;
	}

	public override string ToString()
	{
		return m_code switch
		{
			0 => "OK", 
			-2147467260 => "Abord? Is that supposed to be Abort?", 
			-2147024891 => "Access denied", 
			-2147467259 => "Fail", 
			-2147024890 => "Handle", 
			-2147024809 => "Invalid argument", 
			-2147467262 => "No interface", 
			-2147467263 => "Not implemented", 
			-2147024882 => "Out of memory", 
			-2147467261 => "Invalid pointer", 
			-2147418113 => "Unexpected failure", 
			_ => "Unknown error code".Localize(), 
		};
	}

	static D2dResult()
	{
		Ok = new D2dResult(0);
		Abord = new D2dResult(-2147467260);
		AccessDenied = new D2dResult(-2147024891);
		Fail = new D2dResult(-2147467259);
		Handle = new D2dResult(-2147024890);
		InvalidArg = new D2dResult(-2147024809);
		NoInterface = new D2dResult(-2147467262);
		NotImplemented = new D2dResult(-2147467263);
		OutOfMemory = new D2dResult(-2147024882);
		InvalidPointer = new D2dResult(-2147467261);
		UnexpectedFailure = new D2dResult(-2147418113);
	}
}
