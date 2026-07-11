using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ModelInstance : IModelInstance
{
	private string m_name;

	private string m_geoName;

	private uint m_groupCount;

	private List<IPrimGroupState> m_primGroups;

	private Dictionary<string, PrimGroupState> m_primGroupDictionary;

	private unsafe global::AssetObjects.ModelInstance* m_modelInstance;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	public virtual IEnumerable<IPrimGroupState> PrimGroups => m_primGroups;

	public virtual uint GroupCount => m_groupCount;

	public virtual string GeoName => m_geoName;

	public virtual string Name => m_name;

	private IPrimGroupState FindPrimGroupState(string primGroupKey)
	{
		PrimGroupState value = null;
		m_primGroupDictionary.TryGetValue(primGroupKey, out value);
		return value;
	}

	public virtual IPrimGroupState FindPrimGroupState(string meshName, string groupName, string stateName)
	{
		PrimGroupState primGroupState = null;
		string key = CreatePrimGroupKey(meshName, groupName, stateName);
		primGroupState = null;
		m_primGroupDictionary.TryGetValue(key, out primGroupState);
		return primGroupState;
	}

	public unsafe virtual IPrimGroupState AddPrimGroupState(string meshName, string groupName, string stateName)
	{
		//IL_0086: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = null;
		string key = CreatePrimGroupKey(meshName, groupName, stateName);
		if (m_primGroupDictionary.ContainsKey(key))
		{
			return null;
		}
		StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(meshName);
		PrimGroupState primGroupState;
		try
		{
			standardStringWrapper = standardStringWrapper4;
			StandardStringWrapper standardStringWrapper5 = new StandardStringWrapper(groupName);
			try
			{
				standardStringWrapper2 = standardStringWrapper5;
				StandardStringWrapper standardStringWrapper6 = new StandardStringWrapper(stateName);
				try
				{
					standardStringWrapper3 = standardStringWrapper6;
					global::AssetObjects.PrimGroupState* ptr = global::_003CModule_003E.AssetObjects_002EModelInstance_002EAddGroupState(m_modelInstance, standardStringWrapper.Value, standardStringWrapper2.Value, standardStringWrapper3.Value);
					if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F4_003F_003FAddPrimGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPrimGroupState_0040345_0040PE_0024AAVString_0040System_0040_004000_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040NCIHEHDB_0040primGroupState_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 97u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F4_003F_003FAddPrimGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIPrimGroupState_0040345_0040PE_0024AAVString_0040System_0040_004000_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
					primGroupState = new PrimGroupState(ptr, m_deserializer);
					m_primGroups.Add(primGroupState);
					m_primGroupDictionary[key] = primGroupState;
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
		return primGroupState;
	}

	public unsafe virtual void RemoveGroupState(IPrimGroupState PrimGroupState)
	{
		//IL_004b: Expected I, but got I8
		//IL_007c: Expected I, but got I8
		PrimGroupState primGroupState = null;
		string key = CreatePrimGroupKey(PrimGroupState);
		primGroupState = null;
		m_primGroupDictionary.TryGetValue(key, out primGroupState);
		IPrimGroupState primGroupState2 = primGroupState;
		if (primGroupState == null)
		{
			return;
		}
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F4_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA && m_modelInstance == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040PNCFJDND_0040m_modelInstance_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 116u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F4_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA && m_primGroups.Count <= 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BI_0040HHPJFDNE_0040m_primGroups_003F9_003F_0024DOCount_003F5_003F_0024DO_003F50_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 117u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA && primGroupState2 != PrimGroupState)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EP_0040EIPANGPD_0040ModelInstance_003F5given_003F5a_003F5primGroupS_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CD_0040NOKGFNJB_0040instanceToRemove_003F5_003F_0024DN_003F_0024DN_003F5primGroupSta_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 118u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003FBD_0040_003F_003FRemoveGroupState_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		PrimGroupState primGroupState3 = (PrimGroupState)primGroupState2;
		global::AssetObjects.PrimGroupState* assetObject = primGroupState3.GetAssetObject();
		primGroupState3.RemoveReferences(bDisposing: true);
		m_primGroups.Remove(primGroupState3);
		global::_003CModule_003E.AssetObjects_002EModelInstance_002ERemoveGroupState(m_modelInstance, assetObject);
		m_primGroupDictionary.Remove(key);
		List<IPrimGroupState>.Enumerator enumerator = m_primGroups.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((PrimGroupState)enumerator.Current).FixupNativePointer(m_modelInstance);
			}
			while (enumerator.MoveNext());
		}
	}

	public unsafe virtual void ClearPrimGroups()
	{
		Action<IPrimGroupState> body = global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EClearHelper_002EPGSRemoveReferences;
		Parallel.ForEach(m_primGroups, body);
		m_primGroups.Clear();
		m_primGroupDictionary.Clear();
		global::_003CModule_003E.AssetObjects_002EModelInstance_002EClearGroupStates(m_modelInstance);
	}

	public unsafe ModelInstance(global::AssetObjects.ModelInstance* pkModelInstance, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0052: Expected I, but got I8
		m_primGroups = new List<IPrimGroupState>();
		m_primGroupDictionary = new Dictionary<string, PrimGroupState>();
		m_modelInstance = pkModelInstance;
		m_deserializer = pkDeserializer;
		base._002Ector();
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && m_modelInstance == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040PNCFJDND_0040m_modelInstance_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 28u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EModelInstance_002EGetName(m_modelInstance));
		m_geoName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EModelInstance_002EGetGeoName(m_modelInstance));
		m_groupCount = global::_003CModule_003E.AssetObjects_002EModelInstance_002EGetGroupCount(m_modelInstance);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EModelInstance_002Egroups_begin(m_modelInstance, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EModelInstance_002Egroups_end(m_modelInstance, &iterator2)))
		{
			do
			{
				PrimGroupState primGroupState = new PrimGroupState(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E_002Eiterator_002E_002A(&iterator), pkDeserializer);
				m_primGroups.Add(primGroupState);
				string key = CreatePrimGroupKey(primGroupState);
				m_primGroupDictionary[key] = primGroupState;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003APrimGroupState_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EModelInstance_002Egroups_end(m_modelInstance, &iterator2)));
		}
	}

	internal unsafe void FixupNativePointer(global::AssetObjects.GeometrySet* geometrySet)
	{
		//IL_0028: Expected I, but got I8
		//IL_0073: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVGeometrySet_00403_0040_0040Z_00404_NA && geometrySet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040JKFANOIF_0040geometrySet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 164u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003FFixupNativePointer_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVGeometrySet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(Name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ModelInstance* ptr = (m_modelInstance = global::_003CModule_003E.AssetObjects_002EGeometrySet_002EFindModelInstance(geometrySet, standardStringWrapper.Value));
			if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F9_003F_003FFixupNativePointer_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVGeometrySet_00403_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040PNCFJDND_0040m_modelInstance_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 170u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F9_003F_003FFixupNativePointer_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVGeometrySet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			List<IPrimGroupState>.Enumerator enumerator = m_primGroups.GetEnumerator();
			if (enumerator.MoveNext())
			{
				do
				{
					((PrimGroupState)enumerator.Current).FixupNativePointer(m_modelInstance);
				}
				while (enumerator.MoveNext());
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	internal unsafe global::AssetObjects.ModelInstance* GetAssetObject()
	{
		return m_modelInstance;
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_004e: Expected I, but got I8
		List<IPrimGroupState>.Enumerator enumerator = m_primGroups.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((PrimGroupState)enumerator.Current).RemoveReferences(bDisposing);
			}
			while (enumerator.MoveNext());
		}
		m_primGroups.Clear();
		m_primGroupDictionary.Clear();
		m_modelInstance = null;
	}

	private string CreatePrimGroupKey(string meshName, string groupName, string stateName)
	{
		return string.Join("_", meshName, groupName, stateName);
	}

	private unsafe string CreatePrimGroupKey(IPrimGroupState primGroupState)
	{
		//IL_0026: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreatePrimGroupKey_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA && primGroupState == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040NCIHEHDB_0040primGroupState_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MONLJNIC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 185u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x971ea904_002E_003FbIgnoreAlways_0040_003F2_003F_003FCreatePrimGroupKey_0040ModelInstance_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMPE_0024AAVString_0040System_0040_0040PE_0024AAUIPrimGroupState_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		return CreatePrimGroupKey(primGroupState.MeshName, primGroupState.GroupName, primGroupState.StateName);
	}
}
