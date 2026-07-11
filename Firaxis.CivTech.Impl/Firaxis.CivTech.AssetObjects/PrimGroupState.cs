using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class PrimGroupState : IPrimGroupState
{
	private ValueSet m_valueSet;

	private unsafe global::AssetObjects.PrimGroupState* m_state;

	private string m_meshName;

	private string m_groupName;

	private string m_stateName;

	public virtual IValueSet Values => m_valueSet;

	public virtual string StateName => m_stateName;

	public virtual string GroupName => m_groupName;

	public virtual string MeshName => m_meshName;

	public unsafe PrimGroupState(global::AssetObjects.PrimGroupState* pkPrimGroupState, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0030: Expected I, but got I8
		m_state = pkPrimGroupState;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && pkPrimGroupState == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040NCIHEHDB_0040primGroupState_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HF_0040NFBCDIOJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 19u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_valueSet = new ValueSet(global::_003CModule_003E.AssetObjects_002EPrimGroupState_002EGetValues(m_state), pkDeserializer);
		m_meshName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EPrimGroupState_002EGetMeshName(m_state));
		m_groupName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EPrimGroupState_002EGetGroupName(m_state));
		m_stateName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EPrimGroupState_002EGetStateName(m_state));
	}

	internal unsafe void FixupNativePointer(global::AssetObjects.ModelInstance* model)
	{
		//IL_0029: Expected I, but got I8
		//IL_009f: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		if (!global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVModelInstance_00403_0040_0040Z_00404_NA && model == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05NCCFOPHA_0040model_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HF_0040NFBCDIOJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 54u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVModelInstance_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(MeshName);
		try
		{
			standardStringWrapper = standardStringWrapper4;
			StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(GroupName);
			try
			{
				standardStringWrapper2 = standardStringWrapper5;
				StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(StateName);
				try
				{
					standardStringWrapper3 = standardStringWrapper6;
					global::AssetObjects.PrimGroupState* ptr = (m_state = global::_003CModule_003E.AssetObjects_002EModelInstance_002EFindGroupState(model, standardStringWrapper.Value, standardStringWrapper2.Value, standardStringWrapper3.Value));
					if (!global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F9_003F_003FFixupNativePointer_0040PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVModelInstance_00403_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_07ENKCPKMK_0040m_state_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HF_0040NFBCDIOJ_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 63u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb5709f89_002E_003FbIgnoreAlways_0040_003F9_003F_003FFixupNativePointer_0040PrimGroupState_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVModelInstance_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
					global::AssetObjects.ValueSet* valueSet = global::_003CModule_003E.AssetObjects_002EPrimGroupState_002EGetValues(m_state);
					m_valueSet.FixupNativePointer(valueSet);
				}
				catch
				{
					//try-fault
					((IDisposable)standardStringWrapper3).Dispose();
					throw;
				}
				((IDisposable)standardStringWrapper3).Dispose();
			}
			catch
			{
				//try-fault
				((IDisposable)standardStringWrapper2).Dispose();
				throw;
			}
			((IDisposable)standardStringWrapper2).Dispose();
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	internal unsafe global::AssetObjects.PrimGroupState* GetAssetObject()
	{
		return m_state;
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0026: Expected I, but got I8
		ValueSet valueSet = m_valueSet;
		if (valueSet != null)
		{
			valueSet.RemoveReferences(bDisposing);
			if (!bDisposing)
			{
				return;
			}
			m_valueSet = null;
		}
		if (bDisposing)
		{
			m_state = null;
		}
	}
}
