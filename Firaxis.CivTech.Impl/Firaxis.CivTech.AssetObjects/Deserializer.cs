using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using msclr;

namespace Firaxis.CivTech.AssetObjects;

public class Deserializer : IDisposable
{
	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private object m_shutdownLock;

	public unsafe global::AssetObjects.Deserializer* Value => m_deserializer;

	public unsafe Deserializer()
	{
		//IL_002c: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040GEOMHGIO_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 10, 23, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GH_0040GEOMHGIO_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 10, 23, 0);
			throw;
		}
		m_deserializer = deserializer;
		m_shutdownLock = new object();
		base._002Ector();
	}

	private void _007EDeserializer()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021Deserializer()
	{
		Destroy();
	}

	public unsafe void Destroy()
	{
		//IL_002c: Expected I, but got I8
		@lock obj = null;
		@lock obj2 = new @lock(m_shutdownLock);
		try
		{
			obj = obj2;
			global::AssetObjects.Deserializer* deserializer = m_deserializer;
			if (deserializer != null)
			{
				global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(deserializer, 1u);
				m_deserializer = null;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)obj).Dispose();
			throw;
		}
		((IDisposable)obj).Dispose();
	}

	[SpecialName]
	public unsafe global::AssetObjects.Deserializer* op_MemberSelection()
	{
		return m_deserializer;
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			Destroy();
			GC.SuppressFinalize(this);
			return;
		}
		try
		{
			Destroy();
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

	~Deserializer()
	{
		Dispose(A_0: false);
	}
}
