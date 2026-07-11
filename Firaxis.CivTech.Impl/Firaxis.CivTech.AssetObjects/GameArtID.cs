using System;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class GameArtID : IGameArtID, IDisposable
{
	private string m_name;

	private string m_id;

	private GameArtSpecification m_parent;

	private unsafe global::AssetObjects.GameArtID* m_lastGameArtID;

	public unsafe virtual string ID
	{
		get
		{
			global::AssetObjects.GameArtID* iDPointer = GetIDPointer();
			if (m_lastGameArtID != iDPointer)
			{
				m_lastGameArtID = iDPointer;
				CacheUnmanagedValues(iDPointer);
			}
			return m_id;
		}
		set
		{
			//IL_002c: Expected I, but got I8
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.GameArtID* iDPointer = GetIDPointer();
			m_id = value;
			if (iDPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)((ulong)(nint)iDPointer + 24uL), standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
		}
	}

	public unsafe virtual string Name
	{
		get
		{
			global::AssetObjects.GameArtID* iDPointer = GetIDPointer();
			if (m_lastGameArtID != iDPointer)
			{
				m_lastGameArtID = iDPointer;
				CacheUnmanagedValues(iDPointer);
			}
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			global::AssetObjects.GameArtID* iDPointer = GetIDPointer();
			m_name = value;
			if (iDPointer != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EString_002EAssign((global::AssetObjects.String*)iDPointer, standardStringWrapper.Value);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper).Dispose();
			}
		}
	}

	public unsafe GameArtID(GameArtSpecification parent, global::AssetObjects.GameArtID* pGameArtID)
	{
		//IL_0037: Expected I, but got I8
		//IL_005d: Expected I, but got I8
		m_parent = parent;
		m_lastGameArtID = pGameArtID;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x88bbb571_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0GameArtID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && parent == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06MLKDMCBD_0040parent_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GE_0040MGNLAAPF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 16u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x88bbb571_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0GameArtID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x88bbb571_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0GameArtID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA && pGameArtID == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_09LOCDKKKM_0040gameArtID_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GE_0040MGNLAAPF_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 17u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x88bbb571_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0GameArtID_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PE_0024AAVGameArtSpecification_0040234_0040PEAU12_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		CacheUnmanagedValues(pGameArtID);
	}

	private void _007EGameArtID()
	{
		_0021GameArtID();
	}

	private unsafe void _0021GameArtID()
	{
		//IL_001e: Expected I, but got I8
		m_name = string.Empty;
		m_id = string.Empty;
		m_lastGameArtID = null;
		m_parent = null;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool Equals(IGameArtID other)
	{
		int num = ((ID == other.ID && Name == other.Name) ? 1 : 0);
		return (byte)num != 0;
	}

	private unsafe void UpdateCachedValuesIfNecessary(global::AssetObjects.GameArtID* gameID)
	{
		if (m_lastGameArtID != gameID)
		{
			m_lastGameArtID = gameID;
			CacheUnmanagedValues(gameID);
		}
	}

	private unsafe void CacheUnmanagedValues(global::AssetObjects.GameArtID* gameID)
	{
		//IL_001f: Expected I, but got I8
		if (gameID != null)
		{
			m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)gameID));
			m_id = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str((global::AssetObjects.String*)((ulong)(nint)gameID + 24uL)));
		}
	}

	private unsafe global::AssetObjects.GameArtID* GetIDPointer()
	{
		return m_parent.FindGameIDPointer(m_name, m_id);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021GameArtID();
			return;
		}
		try
		{
			_0021GameArtID();
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

	~GameArtID()
	{
		Dispose(A_0: false);
	}
}
