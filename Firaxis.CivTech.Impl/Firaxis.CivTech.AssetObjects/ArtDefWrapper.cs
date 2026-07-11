using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using msclr;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefWrapper : IDisposable
{
	private unsafe ArtDefSet* m_artDefSet;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	private object m_shutdownLock;

	public unsafe global::AssetObjects.Deserializer* Deserializer => m_deserializer;

	public unsafe ArtDefSet* ArtDefSet => m_artDefSet;

	public unsafe ArtDefWrapper()
	{
		//IL_0030: Expected I, but got I8
		//IL_0079: Expected I, but got I8
		int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		ArtDefSet* ptr = (ArtDefSet*)global::_003CModule_003E.@new(240uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 11, 23, 0);
		ArtDefSet* artDefSet;
		try
		{
			artDefSet = ((ptr == null) ? null : global::_003CModule_003E.AssetObjects_002EArtDefSet_002E_007Bctor_007D(ptr));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 11, 23, 0);
			throw;
		}
		m_artDefSet = artDefSet;
		int num2 = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
		global::AssetObjects.Deserializer* ptr2 = (global::AssetObjects.Deserializer*)global::_003CModule_003E.@new(384uL, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
		global::AssetObjects.Deserializer* deserializer;
		try
		{
			deserializer = ((ptr2 == null) ? null : global::_003CModule_003E.AssetObjects_002EDeserializer_002E_007Bctor_007D(ptr2));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.delete(ptr2, num2, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 12, 23, 0);
			throw;
		}
		m_deserializer = deserializer;
		m_shutdownLock = new object();
		base._002Ector();
	}

	private unsafe void _007EArtDefWrapper()
	{
		//IL_0028: Expected I, but got I8
		//IL_0053: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x1f2970d2_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F1ArtDefWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_0040XZ_00404_NA && m_artDefSet != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BH_0040OOMNGJDN_0040m_artDefSet_003F5_003F_0024DN_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 21u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1f2970d2_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F1ArtDefWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_0040XZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x1f2970d2_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F1ArtDefWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_0040XZ_00404_NA && m_deserializer != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BK_0040DKFHHED_0040m_deserializer_003F5_003F_0024DN_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HA_0040DBIMOFD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 22u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x1f2970d2_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F1ArtDefWrapper_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAM_0040XZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}

	public unsafe void Destroy()
	{
		//IL_0038: Expected I, but got I8
		//IL_0054: Expected I, but got I8
		@lock obj = null;
		@lock obj2 = new @lock(m_shutdownLock);
		try
		{
			obj = obj2;
			ArtDefSet* artDefSet = m_artDefSet;
			if (artDefSet != null)
			{
				ArtDefSet* ptr = artDefSet;
				global::_003CModule_003E.AssetObjects_002EArtDefSet_002E_007Bdtor_007D(ptr);
				global::_003CModule_003E.delete(ptr, 240uL);
				m_artDefSet = null;
			}
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
	public unsafe ArtDefSet* op_MemberSelection()
	{
		return m_artDefSet;
	}

	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_007EArtDefWrapper();
		}
		else
		{
			base.Finalize();
		}
	}

	public virtual sealed void Dispose()
	{
		Dispose(A_0: true);
		GC.SuppressFinalize(this);
	}
}
