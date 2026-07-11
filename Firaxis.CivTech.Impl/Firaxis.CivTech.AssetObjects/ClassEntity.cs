using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ClassEntity : CloudEntity, IClassEntity
{
	private unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	private string m_previewModuleName;

	private ParameterSet m_cookParameters;

	private IList<IClassDataFile> m_dataFiles;

	private unsafe global::AssetObjects.ClassEntity* UnmanagedPtr => (global::AssetObjects.ClassEntity*)m_pkEntity;

	public virtual IEnumerable<IClassDataFile> DataFiles => m_dataFiles;

	public unsafe virtual string PreviewModuleName
	{
		get
		{
			return m_previewModuleName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_previewModuleName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EClassEntity_002ESetPreviewModuleName((global::AssetObjects.ClassEntity*)m_pkEntity, standardStringWrapper.Value);
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

	public unsafe virtual ClassType ClassTypeEnum => (ClassType)global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetClassType((global::AssetObjects.ClassEntity*)m_pkEntity);

	public unsafe virtual InstanceType InstanceTypeEnum => (InstanceType)global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetInstanceType((global::AssetObjects.ClassEntity*)m_pkEntity);

	public virtual IParameterSet CookParameters => m_cookParameters;

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string pmXmlText)
	{
		//IL_002b: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmXmlText);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			Entity* pkEntity = m_pkEntity;
			flag = ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, sbyte*, global::AssetObjects.Deserializer*, byte>)(*(ulong*)(*(long*)pkEntity + 32)))((nint)pkEntity, standardStringWrapper.Value, m_pkDeserializer) != 0;
			if (flag)
			{
				RemoveReferences(bDisposing: false);
				AddReferences();
			}
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return flag;
	}

	public unsafe virtual IClassDataFile AddDataFile(string pmID, string pmExtension)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(pmID);
		ClassDataFile classDataFile;
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(pmExtension);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				classDataFile = new ClassDataFile(global::_003CModule_003E.AssetObjects_002EClassEntity_002EAddDataFile((global::AssetObjects.ClassEntity*)m_pkEntity, standardStringWrapper.Value, standardStringWrapper2.Value));
				m_dataFiles.Add(classDataFile);
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
		return classDataFile;
	}

	public unsafe virtual void RemoveDataFile(IClassDataFile file)
	{
		foreach (ClassDataFile dataFile in m_dataFiles)
		{
			if (dataFile.ID == file.ID)
			{
				global::AssetObjects.ClassDataFile* unmanaged = dataFile.GetUnmanaged();
				global::_003CModule_003E.AssetObjects_002EClassEntity_002ERemoveDataFile((global::AssetObjects.ClassEntity*)m_pkEntity, unmanaged);
				break;
			}
		}
		m_dataFiles.Clear();
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_begin((global::AssetObjects.ClassEntity*)m_pkEntity, &const_iterator);
		global::AssetObjects.ClassEntity* pkEntity = (global::AssetObjects.ClassEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_end(pkEntity, &const_iterator2)))
		{
			do
			{
				ClassDataFile item = new ClassDataFile(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
				m_dataFiles.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				pkEntity = (global::AssetObjects.ClassEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_end(pkEntity, &const_iterator2)));
		}
	}

	public unsafe virtual void DisallowClass(ClassType eTypeEnum, string name)
	{
		//IL_0026: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			Entity* pkEntity = m_pkEntity;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, global::AssetObjects.ClassType, sbyte*, void>)(*(ulong*)(*(long*)pkEntity + 64)))((nint)pkEntity, (global::AssetObjects.ClassType)eTypeEnum, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	internal unsafe override void AddReferences()
	{
		//IL_0033: Expected I, but got I8
		base.AddReferences();
		if (!global::_003CModule_003E._003FA0xed4ef41a_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ClassEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA && m_dataFiles.Count != 0 && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BI_0040HGDPPKNM_0040m_dataFiles_003F9_003F_0024DOCount_003F5_003F_0024DN_003F_0024DN_003F50_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MLBAMHPD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 49u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xed4ef41a_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ClassEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ClassEntity* pkEntity = (global::AssetObjects.ClassEntity*)m_pkEntity;
		m_previewModuleName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetPreviewModuleName(pkEntity));
		m_cookParameters = new ParameterSet(global::_003CModule_003E.AssetObjects_002EClassEntity_002EGetCookParams(pkEntity));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_begin(pkEntity, &const_iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_end(pkEntity, &const_iterator2)))
		{
			do
			{
				ClassDataFile item = new ClassDataFile(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
				m_dataFiles.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AClassDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EClassEntity_002Efiles_end(pkEntity, &const_iterator2)));
		}
	}

	internal unsafe override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0068: Expected I, but got I8
		m_previewModuleName = string.Empty;
		m_cookParameters?.RemoveReferences();
		IList<IClassDataFile> dataFiles = m_dataFiles;
		if (dataFiles != null)
		{
			foreach (ClassDataFile item in dataFiles)
			{
				item.RemoveReferences();
			}
			m_dataFiles.Clear();
		}
		if (bDisposing)
		{
			m_pkDeserializer = null;
			m_cookParameters = null;
			m_dataFiles = null;
		}
		base.RemoveReferences(bDisposing);
	}

	protected unsafe ClassEntity(global::AssetObjects.ClassEntity* pkClassEntity, global::AssetObjects.Deserializer* pkDeserializer)
	{
		//IL_0043: Expected I, but got I8
		m_pkDeserializer = pkDeserializer;
		m_cookParameters = null;
		m_dataFiles = new List<IClassDataFile>();
		base._002Ector((Entity*)pkClassEntity);
		if (!global::_003CModule_003E._003FA0xed4ef41a_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ClassEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA && pkClassEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040CGECPPBD_0040pkClassEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HE_0040MLBAMHPD_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 13u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xed4ef41a_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ClassEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
	}
}
