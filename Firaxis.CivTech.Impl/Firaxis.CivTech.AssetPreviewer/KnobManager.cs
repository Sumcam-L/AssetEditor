using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using AssetPreviewer;
using Platform;

namespace Firaxis.CivTech.AssetPreviewer;

public class KnobManager : IKnobManager
{
	private KnobGroupChangedEventHandler _003Cbacking_store_003EKnobGroupChanged;

	private KnobGroupClearedEventHandler _003Cbacking_store_003EKnobGroupCleared;

	private EventHandler _003Cbacking_store_003EKnobChangesComplete;

	private unsafe KnobDatabase* m_pKnobDatabase;

	private Control m_pmOwningControl;

	[SpecialName]
	public event EventHandler KnobChangesComplete
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobChangesComplete = (EventHandler)Delegate.Combine(_003Cbacking_store_003EKnobChangesComplete, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobChangesComplete = (EventHandler)Delegate.Remove(_003Cbacking_store_003EKnobChangesComplete, value);
		}
	}

	[SpecialName]
	public virtual event KnobGroupClearedEventHandler KnobGroupCleared
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobGroupCleared = (KnobGroupClearedEventHandler)Delegate.Combine(_003Cbacking_store_003EKnobGroupCleared, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobGroupCleared = (KnobGroupClearedEventHandler)Delegate.Remove(_003Cbacking_store_003EKnobGroupCleared, value);
		}
	}

	[SpecialName]
	public virtual event KnobGroupChangedEventHandler KnobGroupChanged
	{
		[MethodImpl(MethodImplOptions.Synchronized)]
		add
		{
			_003Cbacking_store_003EKnobGroupChanged = (KnobGroupChangedEventHandler)Delegate.Combine(_003Cbacking_store_003EKnobGroupChanged, value);
		}
		[MethodImpl(MethodImplOptions.Synchronized)]
		remove
		{
			_003Cbacking_store_003EKnobGroupChanged = (KnobGroupChangedEventHandler)Delegate.Remove(_003Cbacking_store_003EKnobGroupChanged, value);
		}
	}

	[SpecialName]
	protected virtual void raise_KnobGroupChanged(string value0)
	{
		_003Cbacking_store_003EKnobGroupChanged?.Invoke(value0);
	}

	[SpecialName]
	protected virtual void raise_KnobGroupCleared(string value0)
	{
		_003Cbacking_store_003EKnobGroupCleared?.Invoke(value0);
	}

	[SpecialName]
	protected void raise_KnobChangesComplete(object value0, EventArgs value1)
	{
		_003Cbacking_store_003EKnobChangesComplete?.Invoke(value0, value1);
	}

	public unsafe KnobManager(Control owningControl)
	{
		//IL_0008: Expected I, but got I8
		m_pKnobDatabase = null;
		m_pmOwningControl = owningControl;
		base._002Ector();
	}

	private void _007EKnobManager()
	{
		_0021KnobManager();
	}

	private unsafe void _0021KnobManager()
	{
		//IL_0024: Expected I, but got I8
		KnobDatabase* pKnobDatabase = m_pKnobDatabase;
		if (pKnobDatabase != null)
		{
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002E_007Bdtor_007D(pKnobDatabase);
			global::_003CModule_003E.delete(pKnobDatabase, 256uL);
		}
		m_pKnobDatabase = null;
	}

	public unsafe virtual IKnobSet GetKnobSet(string pmGroupName)
	{
		List<IKnob> pmKnobs = null;
		KnobDatabase* pKnobDatabase = m_pKnobDatabase;
		if (pKnobDatabase != null)
		{
			pmKnobs = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002EGetKnobs(pKnobDatabase, pmGroupName);
		}
		return new KnobSet(pmGroupName, pmKnobs, this);
	}

	internal unsafe void SetKnobMessenger(IKnobMessenger* pMessenger)
	{
		//IL_002b: Expected I, but got I8
		//IL_0066: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x4d14e2b2_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetKnobMessenger_0040KnobManager_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIKnobMessenger_00403_0040_0040Z_00404_NA && m_pKnobDatabase != null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040HIJMKMCL_0040_003F_0024CBm_pKnobDatabase_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GI_0040EKJANFJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 458u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x4d14e2b2_002E_003FbIgnoreAlways_0040_003F2_003F_003FSetKnobMessenger_0040KnobManager_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIKnobMessenger_00403_0040_0040Z_00404_NA), (ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_pKnobDatabase == null)
		{
			int num = (int)global::_003CModule_003E.Platform_002EGetMemBlockType();
			KnobDatabase* ptr = (KnobDatabase*)global::_003CModule_003E.@new(256uL, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GI_0040EKJANFJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 460, 23, 0);
			KnobDatabase* pKnobDatabase;
			try
			{
				pKnobDatabase = ((ptr == null) ? null : global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002E_007Bctor_007D(ptr, pMessenger));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.delete(ptr, num, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GI_0040EKJANFJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 460, 23, 0);
				throw;
			}
			m_pKnobDatabase = pKnobDatabase;
		}
	}

	internal unsafe void SendNotificationPacketToUIThread(IKnobNotificationPacket* pPacket)
	{
		//IL_0030: Expected I, but got I8
		KnobDatabase* pKnobDatabase = m_pKnobDatabase;
		if (pKnobDatabase == null)
		{
			return;
		}
		if (!global::_003CModule_003E._003FA0x4d14e2b2_002E_003FbIgnoreAlways_0040_003F4_003F_003FSendNotificationPacketToUIThread_0040KnobManager_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIKnobNotificationPacket_00403_0040_0040Z_00404_NA)
		{
			if (pPacket == null)
			{
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07OBAKEMOI_0040pPacket_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GI_0040EKJANFJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 473u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x4d14e2b2_002E_003FbIgnoreAlways_0040_003F4_003F_003FSendNotificationPacketToUIThread_0040KnobManager_0040AssetPreviewer_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVIKnobNotificationPacket_00403_0040_0040Z_00404_NA), (ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
				return;
			}
		}
		else if (pPacket == null)
		{
			return;
		}
		global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002EProcessPacket(pKnobDatabase, pPacket);
		Delegate method = new KnobNotificationHandler(ProcessNotificationPacket);
		m_pmOwningControl.BeginInvoke(method);
	}

	internal unsafe void ProcessNotificationPacket()
	{
		if (m_pKnobDatabase != null)
		{
			try
			{
				global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002EDispatchUpdateEvents(m_pKnobDatabase, this);
			}
			catch (Exception ex)
			{
				throw ex;
			}
		}
	}

	internal unsafe IList<IKnob> GetKnobs(string groupName)
	{
		object result = null;
		KnobDatabase* pKnobDatabase = m_pKnobDatabase;
		if (pKnobDatabase != null)
		{
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002EGetKnobs(pKnobDatabase, groupName);
		}
		return (IList<IKnob>)result;
	}

	internal unsafe IKnobSet GetEntityKnobSet(string pmWindowName, string assetName, int slotID)
	{
		string text = $"{pmWindowName}_{assetName}_{slotID}";
		List<IKnob> pmKnobs = null;
		KnobDatabase* pKnobDatabase = m_pKnobDatabase;
		if (pKnobDatabase != null)
		{
			pmKnobs = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetPreviewer_002EKnobDatabase_002EGetKnobs(pKnobDatabase, text);
		}
		return new KnobSet(text, pmKnobs, this);
	}

	internal void RaiseKnobGroupChanged(string pmName)
	{
		raise_KnobGroupChanged(pmName);
	}

	internal void RaiseKnobGroupCleared(string pmName)
	{
		raise_KnobGroupCleared(pmName);
	}

	internal void RaiseKnobChangesDone()
	{
		EventArgs empty = EventArgs.Empty;
		_003Cbacking_store_003EKnobChangesComplete?.Invoke(this, empty);
	}

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			_0021KnobManager();
			return;
		}
		try
		{
			_0021KnobManager();
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

	~KnobManager()
	{
		Dispose(A_0: false);
	}
}
