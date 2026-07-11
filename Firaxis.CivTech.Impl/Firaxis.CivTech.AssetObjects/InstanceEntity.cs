using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using fastdelegate;
using Firaxis.Error;
using Platform;
using String;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public abstract class InstanceEntity : CloudEntity, IInstanceEntity
{
	protected unsafe global::AssetObjects.Deserializer* m_pkDeserializer;

	protected unsafe Serializer* m_pkSerializer;

	protected ValueSet m_pmValueSet;

	private unsafe global::AssetObjects.VirtualPantry* m_pkVirtualPantry;

	private string m_className;

	private string m_xmlExtension;

	private string m_xmlPath;

	private global::AssetObjects.InstanceType m_instanceType;

	private IList<InstanceDataFile> m_dataFiles;

	private bool m_IsLocked;

	private unsafe global::AssetObjects.InstanceEntity* Entity => (global::AssetObjects.InstanceEntity*)m_pkEntity;

	public virtual bool IsLocked
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return m_IsLocked;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			m_IsLocked = value;
		}
	}

	public virtual IEnumerable<IInstanceDataFile> DataFiles
	{
		get
		{
			List<IInstanceDataFile> list = new List<IInstanceDataFile>();
			foreach (InstanceDataFile dataFile in m_dataFiles)
			{
				list.Add(dataFile);
			}
			return list;
		}
	}

	public virtual IValueSet CookParameters => m_pmValueSet;

	public unsafe virtual string ClassName
	{
		get
		{
			return m_className;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_className = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EInstanceEntity_002ESetClassName((global::AssetObjects.InstanceEntity*)m_pkEntity, standardStringWrapper.Value);
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

	public virtual string XMLExtension => m_xmlExtension;

	public virtual InstanceType Type => (InstanceType)m_instanceType;

	public unsafe virtual void AddDataFile(string ID, string relativePath)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(ID);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(relativePath);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EAddDataFile((global::AssetObjects.InstanceEntity*)m_pkEntity, standardStringWrapper.Value, standardStringWrapper2.Value);
				InstanceDataFile item = new InstanceDataFile(global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EFindDataFile((global::AssetObjects.InstanceEntity*)m_pkEntity, standardStringWrapper.Value));
				m_dataFiles.Add(item);
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

	public unsafe virtual void RemoveDataFile(string ID)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(ID);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EInstanceEntity_002ERemoveDataFile((global::AssetObjects.InstanceEntity*)m_pkEntity, standardStringWrapper.Value);
			m_dataFiles.Clear();
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_begin((global::AssetObjects.InstanceEntity*)m_pkEntity, &const_iterator);
			global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_end(pkEntity, &const_iterator2)))
			{
				do
				{
					InstanceDataFile item = new InstanceDataFile(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
					m_dataFiles.Add(item);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
					pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_end(pkEntity, &const_iterator2)));
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

	public unsafe virtual void ClearDataFiles()
	{
		global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EClearDataFiles((global::AssetObjects.InstanceEntity*)m_pkEntity);
		foreach (InstanceDataFile dataFile in m_dataFiles)
		{
			dataFile.RemoveReferences();
		}
		m_dataFiles.Clear();
	}

	public unsafe virtual void UpdateDataFile(string ID, string relativePath)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = null;
		foreach (InstanceDataFile dataFile in m_dataFiles)
		{
			if (dataFile.ID == ID)
			{
				dataFile.RelativePath = relativePath;
				return;
			}
		}
		StandardStringWrapper standardStringWrapper3 = new StandardStringWrapper(ID);
		try
		{
			standardStringWrapper = standardStringWrapper3;
			StandardStringWrapper standardStringWrapper4 = new StandardStringWrapper(m_className);
			try
			{
				standardStringWrapper2 = standardStringWrapper4;
				if (!global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FUpdateDataFile_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_00400_0040Z_00404_NA)
				{
					System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
					global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0FB_0040GJKCFJID_0040Attempted_003F5to_003F5update_003F5non_003F9existent_0040), __arglist(standardStringWrapper2.Value, standardStringWrapper.Value));
					if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05LAPONLG_0040false_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040JANEFIGI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 245u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003FM_0040_003F_003FUpdateDataFile_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_00400_0040Z_00404_NA), (Platform.ErrorType)0))
					{
						/*OpCode not supported: DebugBreak*/;
					}
				}
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

	public virtual void PopulateDataFiles(IClassEntity classEntity)
	{
		char[] anyOf = new char[2]
		{
			Path.AltDirectorySeparatorChar,
			Path.DirectorySeparatorChar
		};
		ClearDataFiles();
		using IEnumerator<IClassDataFile> enumerator = classEntity.DataFiles.GetEnumerator();
		IClassDataFile current;
		string text;
		for (; enumerator.MoveNext(); AddDataFile(current.ID, text + current.Extension))
		{
			current = enumerator.Current;
			text = Name;
			char altDirectorySeparatorChar = Path.AltDirectorySeparatorChar;
			if (!Name.Contains(altDirectorySeparatorChar.ToString()))
			{
				char directorySeparatorChar = Path.DirectorySeparatorChar;
				if (!Name.Contains(directorySeparatorChar.ToString()))
				{
					continue;
				}
			}
			int startIndex = Name.LastIndexOfAny(anyOf) + 1;
			text = Name.Substring(startIndex);
		}
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string pmXmlText)
	{
		//IL_0036: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(pmXmlText);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			m_xmlPath = string.Empty;
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

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string pmFilePath)
	{
		//IL_0043: Expected I, but got I8
		IOStringWrapper iOStringWrapper = null;
		RemoveReferences(bDisposing: false);
		m_xmlPath = string.Empty;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilePath);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			Entity* pkEntity = m_pkEntity;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode2);
			global::_003CModule_003E.Platform_002EResultCode_002E_007Bctor_007D(&resultCode, ((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, Platform.ResultCode*, char*, global::AssetObjects.Deserializer*, Platform.ResultCode*>)(*(ulong*)(*(long*)pkEntity + 40)))((nint)pkEntity, &resultCode2, iOStringWrapper.Value, m_pkDeserializer));
			if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
			{
				result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
				goto IL_006f;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		Firaxis.Error.ResultCode success;
		try
		{
			m_xmlPath = pmFilePath;
			AddReferences();
			success = Firaxis.Error.ResultCode.Success;
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return success;
		IL_006f:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilePath)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilePath);
		try
		{
			iOStringWrapper = iOStringWrapper2;
			if (global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AEntity_003E(m_pkSerializer, m_pkEntity, iOStringWrapper.Value))
			{
				m_xmlPath = pmFilePath;
				goto IL_0036;
			}
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return false;
		IL_0036:
		((IDisposable)iOStringWrapper).Dispose();
		return true;
	}

	public unsafe virtual string SerializeIntoXML()
	{
		m_xmlPath = string.Empty;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out MemoryBuffer memoryBuffer);
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bctor_007D(&memoryBuffer);
		string result;
		try
		{
			global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoMemory_003Cclass_0020AssetObjects_003A_003AEntity_003E(m_pkSerializer, &memoryBuffer, m_pkEntity);
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)global::_003CModule_003E.Platform_002EMemoryBuffer_002EGetBytes(&memoryBuffer));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<MemoryBuffer*, void>)(&global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D), &memoryBuffer);
			throw;
		}
		global::_003CModule_003E.Platform_002EMemoryBuffer_002E_007Bdtor_007D(&memoryBuffer);
		return result;
	}

	public unsafe virtual void CrawlCooktimeDependencies(Action<InstanceType, string> pmAction)
	{
		//IL_0010: Expected I8, but got I
		//IL_0031: Expected I, but got I8
		//IL_0066: Expected I, but got I8
		System.Runtime.CompilerServices.Unsafe.SkipInit(out DependencyCrawler dependencyCrawler);
		global::_003CModule_003E.AssetObjects_002EContainer_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_003E_002E_007Bctor_007D((Container_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_003E*)(&dependencyCrawler));
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024PTMType_0024P8DependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z _0024PTMType_0024P8DependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2);
			*(long*)(&_0024PTMType_0024P8DependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2) = (nint)global::_003CModule_003E.__unep_0040_003FOnDependency_0040DependencyCrawler_0040AssetObjects_0040_0040_0024_0024FQEAAXW4InstanceType_00402_0040PEBD_0040Z;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E obj);
			global::_003CModule_003E.fastdelegate_002EFastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E_002E_007Bctor_007D_003Cclass_0020AssetObjects_003A_003ADependencyCrawler_002Cclass_0020AssetObjects_003A_003ADependencyCrawler_003E(&obj, &dependencyCrawler, _0024PTMType_0024P8DependencyCrawler_0040AssetObjects_0040_0040EAAXW4InstanceType_00401_0040PEBD_0040Z2);
			Entity* pkEntity = m_pkEntity;
			((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, FastDelegate2_003Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_002Cvoid_003E*, void>)(*(ulong*)(*(long*)pkEntity + 64)))((nint)pkEntity, &obj);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E.const_iterator const_iterator);
			global::_003CModule_003E.AssetObjects_002EDependencyCrawler_002Ebegin(&dependencyCrawler, &const_iterator);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E.const_iterator const_iterator2);
			if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDependencyCrawler_002Eend(&dependencyCrawler, &const_iterator2)))
			{
				do
				{
					InstanceType arg = *(InstanceType*)((ulong)(nint)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator) + 8uL);
					string arg2 = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString((sbyte*)(*(ulong*)global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002Econst_iterator_002E_002D_003E(&const_iterator)));
					pmAction(arg, arg2);
					global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				}
				while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EDependencyCrawler_002Eend(&dependencyCrawler, &const_iterator2)));
			}
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<DependencyCrawler*, void>)(&global::_003CModule_003E.AssetObjects_002EDependencyCrawler_002E_007Bdtor_007D), &dependencyCrawler);
			throw;
		}
		try
		{
			global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E_002E_007Bdtor_007D((ChunkedVector_003CAssetObjects_003A_003ADependencyCrawler_003A_003ADependency_002C4096_003E*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref dependencyCrawler, 32)));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D), &dependencyCrawler);
			throw;
		}
		global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E_002E_007Bdtor_007D((ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C0_003E_002C1024_002C16_003E*)(&dependencyCrawler));
	}

	public unsafe virtual string GetXMLPath()
	{
		//IL_0024: Expected I8, but got I
		if (!string.IsNullOrEmpty(m_xmlPath))
		{
			return m_xmlPath;
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E chunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E);
		global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bctor_007D(&chunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E);
		string result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024PTMType_0024P8_003F_0024ChunkedAllocator_0040V_003F_0024StaticHeapAllocator_0040_00240DC_0040_002400_0040Platform_0040_0040_00240PPPP_0040_00240BA_0040_0040Types_0040_0040EAAPEAX_K0_0040Z _0024PTMType_0024P8_003F_0024ChunkedAllocator_0040V_003F_0024StaticHeapAllocator_0040_00240DC_0040_002400_0040Platform_0040_0040_00240PPPP_0040_00240BA_0040_0040Types_0040_0040EAAPEAX_K0_0040Z2);
			*(long*)(&_0024PTMType_0024P8_003F_0024ChunkedAllocator_0040V_003F_0024StaticHeapAllocator_0040_00240DC_0040_002400_0040Platform_0040_0040_00240PPPP_0040_00240BA_0040_0040Types_0040_0040EAAPEAX_K0_0040Z2) = (nint)global::_003CModule_003E.__unep_0040_003FAllocate_0040_003F_0024ChunkedAllocator_0040V_003F_0024StaticHeapAllocator_0040_00240DC_0040_002400_0040Platform_0040_0040_00240PPPP_0040_00240BA_0040_0040Types_0040_0040_0024_0024FQEAAPEAX_K0_0040Z;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E obj);
			global::_003CModule_003E.fastdelegate_002EFastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E_002E_007Bctor_007D_003Cclass_0020Types_003A_003AChunkedAllocator_003Cclass_0020Platform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002Cclass_0020Types_003A_003AChunkedAllocator_003Cclass_0020Platform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_0020_003E(&obj, &chunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E, _0024PTMType_0024P8_003F_0024ChunkedAllocator_0040V_003F_0024StaticHeapAllocator_0040_00240DC_0040_002400_0040Platform_0040_0040_00240PPPP_0040_00240BA_0040_0040Types_0040_0040EAAPEAX_K0_0040Z2);
			global::AssetObjects.InstanceType instanceType = global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetType((global::AssetObjects.InstanceEntity*)m_pkEntity);
			sbyte* ptr = global::_003CModule_003E.AssetObjects_002EEntity_002EGetName(m_pkEntity);
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E obj2);
			char* ptr2 = global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPantryPath(m_pkVirtualPantry, ptr, instanceType, global::_003CModule_003E.fastdelegate_002EFastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj2, &obj));
			if (ptr2 != null)
			{
				goto IL_0084;
			}
			System.Runtime.CompilerServices.Unsafe.SkipInit(out FastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E obj3);
			ptr2 = global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPrimaryPantryPath(m_pkVirtualPantry, ptr, instanceType, global::_003CModule_003E.fastdelegate_002EFastDelegate2_003Cunsigned_0020__int64_002Cunsigned_0020__int64_002Cvoid_0020_002A_003E_002E_007Bctor_007D(&obj3, &obj));
			if (ptr2 != null)
			{
				goto IL_0084;
			}
			result = string.Empty;
			goto end_IL_001c;
			IL_0084:
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(ptr2);
			end_IL_001c:;
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<ChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E*, void>)(&global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D), &chunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E);
			throw;
		}
		global::_003CModule_003E.Types_002EChunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E_002E_007Bdtor_007D(&chunkedAllocator_003CPlatform_003A_003AStaticHeapAllocator_003C50_002C1_003E_002C65535_002C16_003E);
		return result;
	}

	public unsafe virtual string GetActiveProjectXMLPath()
	{
		global::AssetObjects.InstanceType instanceType = global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetType((global::AssetObjects.InstanceEntity*)m_pkEntity);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bctor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		string result;
		try
		{
			sbyte* ptr = global::_003CModule_003E.AssetObjects_002EEntity_002EGetName(m_pkEntity);
			global::_003CModule_003E.AssetObjects_002EVirtualPantry_002EGetPrimaryPantryPath(m_pkVirtualPantry, ptr, instanceType, &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			result = ((!global::_003CModule_003E.String_002EBase_003C2_003E_002EIsEmpty((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))) ? global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.String_002EBase_003C2_003E_002Ec_str((Base_003C2_003E*)(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E))) : string.Empty);
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<BasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E*, void>)(&global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D), &basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
			throw;
		}
		global::_003CModule_003E.String_002EBasicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E_002E_007Bdtor_007D(&basicT_003CPlatform_003A_003AStaticHeapAllocator_003C5_002C0_003E_002C2_003E);
		return result;
	}

	public virtual string GetDataFilePath(string DataFileRelativePath)
	{
		string text = GetXMLPath();
		if (string.IsNullOrEmpty(text))
		{
			text = GetActiveProjectXMLPath();
		}
		return DoGetDataFilePath(text, DataFileRelativePath);
	}

	public virtual IInstanceDataFile FindDataFileByID(string id)
	{
		foreach (InstanceDataFile dataFile in m_dataFiles)
		{
			if (((IInstanceDataFile)dataFile).ID.Equals(id))
			{
				return dataFile;
			}
		}
		return null;
	}

	public override string ToString()
	{
		return Name;
	}

	public virtual string DetailString()
	{
		return $"Name: {Name}\nType: {((InstanceType)(object)Type).ToString()}";
	}

	public virtual IEnumerable<string> GetEntityPaths()
	{
		List<string> list = new List<string>();
		string text = GetXMLPath();
		if (string.IsNullOrEmpty(text))
		{
			text = GetActiveProjectXMLPath();
		}
		list.Add(text);
		foreach (IInstanceDataFile dataFile in DataFiles)
		{
			string dataFilePath = GetDataFilePath(dataFile.RelativePath);
			list.Add(dataFilePath);
		}
		return list;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public virtual bool Equals(EntityID entityID)
	{
		int num = ((Name == entityID.Name && Type == entityID.Type) ? 1 : 0);
		return (byte)num != 0;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool Equals(IInstanceEntity pmOtherEntity)
	{
		if (pmOtherEntity == null)
		{
			return false;
		}
		InstanceEntity instanceEntity = (InstanceEntity)pmOtherEntity;
		int num = ((instanceEntity.m_pkEntity == m_pkEntity || (instanceEntity.Name == Name && instanceEntity.Type == Type)) ? 1 : 0);
		return (byte)num != 0;
	}

	public virtual void PublishStats(IDictionary<string, int> stats)
	{
	}

	internal unsafe void ResolveNativePointer(global::AssetObjects.InstanceSet* pkInstanceSet)
	{
		StandardStringWrapper standardStringWrapper = null;
		if (!global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F2_003F_003FResolveNativePointer_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVInstanceSet_00403_0040_0040Z_00404_NA && pkInstanceSet == null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GE_0040JPPPDPBH_0040Attempting_003F5to_003F5resolve_003F5an_003F5asset_003F5p_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040PGMFMLLM_0040pkInstanceSet_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040JANEFIGI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 469u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F2_003F_003FResolveNativePointer_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVInstanceSet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(Name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.InstanceEntity* ptr = (global::AssetObjects.InstanceEntity*)(m_pkEntity = (Entity*)global::_003CModule_003E.AssetObjects_002EInstanceSet_002EFindByNameAndType(pkInstanceSet, standardStringWrapper.Value, m_instanceType));
			if (!global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F9_003F_003FResolveNativePointer_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVInstanceSet_00403_0040_0040Z_00404_NA && ptr == null)
			{
				System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
				global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GC_0040IPNCPOJK_0040After_003F5performing_003F5resolve_003F0_003F5unable_0040), __arglist());
				if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0L_0040PJLBPPJA_0040m_pkEntity_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040JANEFIGI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 473u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F9_003F_003FResolveNativePointer_0040InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAMXPEAVInstanceSet_00403_0040_0040Z_00404_NA), (Platform.ErrorType)0))
				{
					/*OpCode not supported: DebugBreak*/;
				}
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

	internal new unsafe global::AssetObjects.InstanceEntity* GetAssetObject()
	{
		return (global::AssetObjects.InstanceEntity*)m_pkEntity;
	}

	internal unsafe global::AssetObjects.VirtualPantry* GetVirtualPantry()
	{
		return m_pkVirtualPantry;
	}

	internal unsafe override void AddReferences()
	{
		base.AddReferences();
		global::AssetObjects.InstanceEntity* pkEntity = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		m_className = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetClassName(pkEntity));
		global::AssetObjects.InstanceEntity* pkEntity2 = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		m_instanceType = global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetType(pkEntity2);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E.const_iterator const_iterator);
		global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_begin((global::AssetObjects.InstanceEntity*)m_pkEntity, &const_iterator);
		global::AssetObjects.InstanceEntity* pkEntity3 = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E.const_iterator const_iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_end(pkEntity3, &const_iterator2)))
		{
			do
			{
				InstanceDataFile item = new InstanceDataFile(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_002A(&const_iterator));
				m_dataFiles.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_002B_002B(&const_iterator);
				pkEntity3 = (global::AssetObjects.InstanceEntity*)m_pkEntity;
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AInstanceDataFile_002C4096_003E_002Econst_iterator_002E_0021_003D(&const_iterator, global::_003CModule_003E.AssetObjects_002EInstanceEntity_002Efiles_end(pkEntity3, &const_iterator2)));
		}
		global::AssetObjects.InstanceEntity* pkEntity4 = (global::AssetObjects.InstanceEntity*)m_pkEntity;
		m_pmValueSet = new ValueSet(global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetCookParameters(pkEntity4), m_pkDeserializer);
	}

	internal unsafe override void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_0063: Expected I, but got I8
		//IL_006b: Expected I, but got I8
		ValueSet pmValueSet = m_pmValueSet;
		if (pmValueSet != null)
		{
			pmValueSet.RemoveReferences(bDisposing);
			m_pmValueSet = null;
		}
		foreach (InstanceDataFile dataFile in m_dataFiles)
		{
			dataFile.RemoveReferences();
		}
		m_dataFiles.Clear();
		m_instanceType = (global::AssetObjects.InstanceType)12u;
		if (bDisposing)
		{
			m_pkSerializer = null;
			m_pkDeserializer = null;
			m_pmValueSet = null;
			m_dataFiles = null;
		}
		base.RemoveReferences(bDisposing);
	}

	internal unsafe void SerializeInternal(MemoryBuffer* target)
	{
		global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoMemory_003Cclass_0020AssetObjects_003A_003AEntity_003E(m_pkSerializer, target, m_pkEntity);
	}

	protected unsafe InstanceEntity(global::AssetObjects.InstanceEntity* pkInstanceEntity, global::AssetObjects.Deserializer* pkDeserializer, Serializer* pkSerializer, global::AssetObjects.VirtualPantry* pkVirtualPantry)
	{
		//IL_005d: Expected I, but got I8
		//IL_0084: Expected I, but got I8
		m_pkDeserializer = pkDeserializer;
		m_pkSerializer = pkSerializer;
		m_pmValueSet = new ValueSet(global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetCookParameters(pkInstanceEntity), pkDeserializer);
		m_pkVirtualPantry = pkVirtualPantry;
		m_dataFiles = new List<InstanceDataFile>();
		base._002Ector((Entity*)pkInstanceEntity);
		if (!global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040PEAVSerializer_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA && pkInstanceEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040IKKGNMLF_0040pkInstanceEntity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040JANEFIGI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 55u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040PEAVSerializer_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		if (!global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040PEAVSerializer_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA && pkVirtualPantry == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BA_0040NHFLFBMI_0040pkVirtualPantry_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040JANEFIGI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 56u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xb66b4fae_002E_003FbIgnoreAlways_0040_003F9_003F_003F_003F0InstanceEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAV12_0040PEAVDeserializer_00402_0040PEAVSerializer_00402_0040PEAVVirtualPantry_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_xmlExtension = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetXMLExtension(pkInstanceEntity));
		m_instanceType = global::_003CModule_003E.AssetObjects_002EInstanceEntity_002EGetType(pkInstanceEntity);
	}

	private string DoGetDataFilePath(string entityXmlPath, string DataFileRelativePath)
	{
		string directoryName = Path.GetDirectoryName(entityXmlPath);
		char[] invalidPathChars = Path.InvalidPathChars;
		int num = 0;
		if (0 < (nint)invalidPathChars.LongLength)
		{
			do
			{
				char c = invalidPathChars[num];
				if (directoryName.IndexOf(c) < 0)
				{
					if (DataFileRelativePath.IndexOf(c) < 0)
					{
						num++;
						continue;
					}
					throw new ArgumentException($"The data file relative path \"{DataFileRelativePath}\" contains the invalid character \"{c}\".");
				}
				throw new ArgumentException($"The directory \"{directoryName}\" contains the invalid character \"{c}\".");
			}
			while (num < (nint)invalidPathChars.LongLength);
		}
		return Path.GetFullPath(Path.Combine(directoryName, DataFileRelativePath));
	}
}
