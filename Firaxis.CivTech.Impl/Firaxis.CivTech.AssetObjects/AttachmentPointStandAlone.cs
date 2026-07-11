using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class AttachmentPointStandAlone : IAttachmentPointStandalone
{
	private unsafe global::AssetObjects.AttachmentPoint* m_pkAttachmentPoint;

	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private IValueSet m_pmValueSet;

	private bool m_isStandalone;

	public unsafe virtual float Scale
	{
		get
		{
			return *global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetScale(m_pkAttachmentPoint);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetScale(m_pkAttachmentPoint, &value);
		}
	}

	public unsafe virtual IFloatVector3 Orientation
	{
		get
		{
			FGXVector3* ptr = global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetOrientation(m_pkAttachmentPoint);
			return new FloatVector3(*(float*)ptr, *(float*)((ulong)(nint)ptr + 4uL), *(float*)((ulong)(nint)ptr + 8uL));
		}
		set
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, value.X, value.Y, value.Z);
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetOrientation(m_pkAttachmentPoint, &fGXVector);
		}
	}

	public unsafe virtual IFloatVector3 Position
	{
		get
		{
			FGXVector3* ptr = global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetPosition(m_pkAttachmentPoint);
			return new FloatVector3(*(float*)ptr, *(float*)((ulong)(nint)ptr + 4uL), *(float*)((ulong)(nint)ptr + 8uL));
		}
		set
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FGXVector3 fGXVector);
			global::_003CModule_003E.FGXVector3_002E_007Bctor_007D(&fGXVector, value.X, value.Y, value.Z);
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetPosition(m_pkAttachmentPoint, &fGXVector);
		}
	}

	public virtual IValueSet CookParameters => m_pmValueSet;

	public unsafe virtual string ModelInstanceName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetModelInstanceName(m_pkAttachmentPoint));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetModelInstanceName(m_pkAttachmentPoint, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string BoneName
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetBoneName(m_pkAttachmentPoint));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetBoneName(m_pkAttachmentPoint, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			IntPtr ptr = new IntPtr(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetName(m_pkAttachmentPoint));
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002ESetName(m_pkAttachmentPoint, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe AttachmentPointStandAlone()
	{
		//IL_002f: Expected I, but got I8
		//IL_0076: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.AttachmentPoint* ptr = (global::AssetObjects.AttachmentPoint*)global::_003CModule_003E.@new(168uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IA_0040OFDLCKJP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 16, 0, 0);
		global::AssetObjects.AttachmentPoint* pkAttachmentPoint;
		try
		{
			pkAttachmentPoint = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IA_0040OFDLCKJP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 16, 0, 0);
			throw;
		}
		m_pkAttachmentPoint = pkAttachmentPoint;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr2 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IA_0040OFDLCKJP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 0, 0);
		global::AssetObjects.Deserializer* pkDeserializer;
		try
		{
			pkDeserializer = ((ptr2 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0IA_0040OFDLCKJP_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17, 0, 0);
			throw;
		}
		m_pkDeserializer = pkDeserializer;
		m_pmValueSet = new ValueSet(global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002EGetCookParameters(m_pkAttachmentPoint), pkDeserializer);
		base._002Ector();
	}

	private void _007EAttachmentPointStandAlone()
	{
		_0021AttachmentPointStandAlone();
		GC.SuppressFinalize(this);
	}

	private unsafe void _0021AttachmentPointStandAlone()
	{
		global::AssetObjects.AttachmentPoint* pkAttachmentPoint = m_pkAttachmentPoint;
		if (pkAttachmentPoint != null)
		{
			global::_003CModule_003E.AssetObjects_002EAttachmentPoint_002E_007Bdtor_007D(pkAttachmentPoint);
			global::_003CModule_003E.delete(pkAttachmentPoint, 168uL);
		}
		global::AssetObjects.Deserializer* pkDeserializer = m_pkDeserializer;
		if (pkDeserializer != null)
		{
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(pkDeserializer, 1u);
		}
	}

	internal unsafe global::AssetObjects.AttachmentPoint* GetNativeObject()
	{
		return m_pkAttachmentPoint;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021AttachmentPointStandAlone();
			GC.SuppressFinalize(this);
			return;
		}
		try
		{
			_0021AttachmentPointStandAlone();
		}
		finally
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}

	~AttachmentPointStandAlone()
	{
		Dispose(A_0: false);
	}
}
