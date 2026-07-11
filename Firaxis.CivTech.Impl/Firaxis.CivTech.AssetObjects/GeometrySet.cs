using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class GeometrySet : IGeometrySet
{
	private List<IModelInstance> m_modelInstances;

	private Dictionary<string, ModelInstance> m_modelDictionary;

	private unsafe global::AssetObjects.GeometrySet* m_geometrySet;

	private unsafe global::AssetObjects.Deserializer* m_deserializer;

	public virtual IEnumerable<IModelInstance> ModelInstances => m_modelInstances;

	public unsafe virtual uint ModelInstanceCount
	{
		get
		{
			//IL_0028: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ModelInstanceCount_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMIXZ_00404_NA && m_geometrySet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CFODHMBL_0040m_geometrySet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 38u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F2_003F_003Fget_0040ModelInstanceCount_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMIXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			return global::_003CModule_003E.AssetObjects_002EGeometrySet_002EGetModelInstanceCount(m_geometrySet);
		}
	}

	public virtual IModelInstance FindModelInstance(string name)
	{
		ModelInstance value = null;
		m_modelDictionary.TryGetValue(name, out value);
		return value;
	}

	public unsafe virtual IModelInstance AddModelInstance(string name, IGeometryInstance geo)
	{
		//IL_002a: Expected I, but got I8
		//IL_0055: Expected I, but got I8
		//IL_0080: Expected I, but got I8
		//IL_00a6: Expected I, but got I8
		//IL_010a: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA && m_geometrySet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BJ_0040OOFLJEEN_0040m_geometrySet_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 59u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA && m_modelDictionary == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BN_0040HOIMNGHM_0040m_modelDictionary_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 60u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA && string.IsNullOrWhiteSpace(name) && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0DC_0040NCBJAMBH_0040System_003F3_003F3String_003F3_003F3IsNullOrWhiteSpa_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 62u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FBB_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA && geo == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040ONDKDLAG_0040geo_003F5_003F_0024CB_003F_0024DN_003F5nullptr_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 63u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FBI_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (m_modelDictionary.ContainsKey(name))
		{
			return null;
		}
		GeometryInstance geometryInstance = (GeometryInstance)geo;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		ModelInstance modelInstance;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.GeometryInstance* assetObject = (global::AssetObjects.GeometryInstance*)geometryInstance.GetAssetObject();
			global::AssetObjects.ModelInstance* ptr = global::_003CModule_003E.AssetObjects_002EGeometrySet_002EAddModelInstance(m_geometrySet, standardStringWrapper.Value, assetObject);
			if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FCB_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CKDPEJDA_0040modelInstance_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 72u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FCB_0040_003F_003FAddModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIModelInstance_0040345_0040PE_0024AAVString_0040System_0040_0040PE_0024AAUIGeometryInstance_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			modelInstance = new ModelInstance(ptr, m_deserializer);
			m_modelInstances.Add(modelInstance);
			m_modelDictionary[modelInstance.Name] = modelInstance;
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return modelInstance;
	}

	public unsafe virtual void RemoveModelInstance(string name)
	{
		//IL_0036: Expected I, but got I8
		//IL_0067: Expected I, but got I8
		IModelInstance modelInstance = FindModelInstance(name);
		if (modelInstance == null)
		{
			return;
		}
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F4_003F_003FRemoveModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_geometrySet == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CFODHMBL_0040m_geometrySet_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 90u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003F4_003F_003FRemoveModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FRemoveModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && m_modelInstances.Count <= 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BM_0040DBEFPDJO_0040m_modelInstances_003F9_003F_0024DOCount_003F5_003F_0024DO_003F50_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040IODGBENN_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 91u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x530b248a_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FRemoveModelInstance_0040GeometrySet_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		ModelInstance modelInstance2 = (ModelInstance)modelInstance;
		global::AssetObjects.ModelInstance* assetObject = modelInstance2.GetAssetObject();
		modelInstance2.RemoveReferences(bDisposing: true);
		global::_003CModule_003E.AssetObjects_002EGeometrySet_002ERemoveModelInstance(m_geometrySet, assetObject);
		m_modelInstances.Remove(modelInstance2);
		List<IModelInstance>.Enumerator enumerator = m_modelInstances.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ModelInstance)enumerator.Current).FixupNativePointer(m_geometrySet);
			}
			while (enumerator.MoveNext());
		}
		m_modelDictionary.Remove(name);
	}

	public unsafe GeometrySet(global::AssetObjects.GeometrySet* pkGeometrySet, global::AssetObjects.Deserializer* pkDeserializer)
	{
		m_modelInstances = new List<IModelInstance>();
		m_modelDictionary = new Dictionary<string, ModelInstance>();
		m_geometrySet = pkGeometrySet;
		m_deserializer = pkDeserializer;
		base._002Ector();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EGeometrySet_002Emodels_begin(m_geometrySet, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometrySet_002Emodels_end(m_geometrySet, &iterator2)))
		{
			do
			{
				ModelInstance modelInstance = new ModelInstance(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E_002Eiterator_002E_002A(&iterator), pkDeserializer);
				m_modelInstances.Add(modelInstance);
				m_modelDictionary[modelInstance.Name] = modelInstance;
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AModelInstance_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EGeometrySet_002Emodels_end(m_geometrySet, &iterator2)));
		}
	}

	internal unsafe virtual void RemoveReferences()
	{
		//IL_004e: Expected I, but got I8
		List<IModelInstance>.Enumerator enumerator = m_modelInstances.GetEnumerator();
		if (enumerator.MoveNext())
		{
			do
			{
				((ModelInstance)enumerator.Current).RemoveReferences(bDisposing: true);
			}
			while (enumerator.MoveNext());
		}
		m_modelInstances.Clear();
		m_modelDictionary.Clear();
		m_geometrySet = null;
	}
}
