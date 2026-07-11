using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class GameArtSpecWrapper : IDisposable
{
	private unsafe global::AssetObjects.GameArtSpecification* m_gameArtSpecification;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	public unsafe global::AssetObjects.Deserializer* Deserializer => m_deserializer;

	public unsafe global::AssetObjects.GameArtSpecification* GameArtSpecification => m_gameArtSpecification;

	public unsafe GameArtSpecWrapper()
	{
		//IL_0033: Expected I, but got I8
		//IL_0082: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.GameArtSpecification* ptr = (global::AssetObjects.GameArtSpecification*)global::_003CModule_003E.@new(248uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 542, 23, 0);
		global::AssetObjects.GameArtSpecification* gameArtSpecification;
		try
		{
			gameArtSpecification = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EGameArtSpecification_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 542, 23, 0);
			throw;
		}
		m_gameArtSpecification = gameArtSpecification;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr2 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 543, 23, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr2 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GP_0040EPNPECGG_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 543, 23, 0);
			throw;
		}
		m_deserializer = deserializer;
		base._002Ector();
	}

	private void _007EGameArtSpecWrapper()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021GameArtSpecWrapper()
	{
		Destroy();
	}

	[SpecialName]
	public unsafe global::AssetObjects.GameArtSpecification* op_MemberSelection()
	{
		return m_gameArtSpecification;
	}

	private unsafe void Destroy()
	{
		//IL_0026: Expected I, but got I8
		//IL_0040: Expected I, but got I8
		global::AssetObjects.GameArtSpecification* gameArtSpecification = m_gameArtSpecification;
		if (gameArtSpecification != null)
		{
			global::AssetObjects.GameArtSpecification* ptr = gameArtSpecification;
			global::_003CModule_003E.AssetObjects_002EGameArtSpecification_002E_007Bdtor_007D(ptr);
			global::_003CModule_003E.delete(ptr, 248uL);
			m_gameArtSpecification = null;
		}
		global::AssetObjects.Deserializer* deserializer = m_deserializer;
		if (deserializer != null)
		{
			global::_003CModule_003E.AssetObjects_002EDeserializer_002E__delDtor(deserializer, 1u);
			m_deserializer = null;
		}
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EGameArtSpecWrapper();
			return;
		}
		try
		{
			_0021GameArtSpecWrapper();
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

	~GameArtSpecWrapper()
	{
		Dispose(A_0: false);
	}
}
