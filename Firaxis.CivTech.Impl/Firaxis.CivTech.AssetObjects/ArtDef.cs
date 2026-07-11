using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Firaxis.Error;
using msclr;
using Platform;
using Serialization;
using std;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDef : IArtDef
{
	private string m_templateName;

	private readonly ArtDefWrapper m_artDefSet;

	private IProjectConfig m_projectConfig;

	private IList<IArtDefCollection> m_artDefCollections;

	private bool m_disposed;

	private object m_shutdownLock;

	internal unsafe global::AssetObjects.Deserializer* Deserializer
	{
		get
		{
			//IL_000b: Expected I, but got I8
			if (m_disposed)
			{
				return null;
			}
			return m_artDefSet.Deserializer;
		}
	}

	internal unsafe ArtDefSet* ArtDefSet
	{
		get
		{
			//IL_000b: Expected I, but got I8
			if (m_disposed)
			{
				return null;
			}
			return m_artDefSet.ArtDefSet;
		}
	}

	public unsafe virtual Version Version
	{
		get
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out VersionInfo versionInfo);
			global::_003CModule_003E.AssetObjects_002EArtDefSet_002EGetVersion(m_artDefSet.op_MemberSelection(), &versionInfo);
			return new Version(*(int*)(&versionInfo), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 4)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 8)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 12)));
		}
	}

	public virtual IEnumerable<IArtDefCollection> RootCollections => m_artDefCollections;

	public unsafe virtual string ArtDefTemplate
	{
		get
		{
			return m_templateName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_templateName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				sbyte* value2 = standardStringWrapper.Value;
				foreach (ArtDefTemplate item in m_projectConfig.ArtDefTemplates.Items)
				{
					if (item.Name == value)
					{
						global::AssetObjects.ArtDefTemplate* nativeTemplate = item.GetNativeTemplate();
					}
				}
				global::_003CModule_003E.AssetObjects_002EArtDefSet_002ESetTemplateName(ArtDefSet, value2);
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

	public ArtDef(IProjectConfig projectConfig)
	{
		ArtDefWrapper artDefSet = new ArtDefWrapper();
		try
		{
			m_artDefSet = artDefSet;
			m_projectConfig = projectConfig;
			m_artDefCollections = new List<IArtDefCollection>();
			m_disposed = false;
			m_shutdownLock = new object();
			base._002Ector();
			AddReferences();
			return;
		}
		catch
		{
			//try-fault
			((IDisposable)m_artDefSet).Dispose();
			throw;
		}
	}

	private void _007EArtDef()
	{
		Destroy();
		GC.SuppressFinalize(this);
	}

	private void _0021ArtDef()
	{
		Destroy();
	}

	public unsafe virtual void SetVersion(int major, int minor, int build, int revision)
	{
		global::_003CModule_003E.AssetObjects_002EArtDefSet_002ESetVersion(m_artDefSet.op_MemberSelection(), major, minor, build, revision);
	}

	public unsafe virtual void SetVersion(string pmVersion)
	{
		Version result = null;
		if (Version.TryParse(pmVersion, out result))
		{
			global::_003CModule_003E.AssetObjects_002EArtDefSet_002ESetVersion(m_artDefSet.op_MemberSelection(), result.Major, result.Minor, result.Build, result.Revision);
		}
	}

	public unsafe virtual IArtDefCollection AddCollection(string name)
	{
		//IL_0045: Expected I, but got I8
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		IArtDefCollection artDefCollection;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::AssetObjects.ArtDefCollection* ptr = global::_003CModule_003E.AssetObjects_002EArtDefSet_002EAddCollection(m_artDefSet.op_MemberSelection(), standardStringWrapper.Value);
			if (!global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && ptr == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0M_0040FHFCHFDH_0040pCollection_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040ECDAEDJE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 119u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddCollection_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMPE_0024AAUIArtDefCollection_0040345_0040PE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			artDefCollection = new ArtDefCollection(this, null, ptr);
			m_artDefCollections.Add(artDefCollection);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
		return artDefCollection;
	}

	public unsafe virtual void RemoveCollection(string name)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(name);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			foreach (IArtDefCollection artDefCollection in m_artDefCollections)
			{
				if (artDefCollection.CollectionName == name)
				{
					m_artDefCollections.Remove(artDefCollection);
					break;
				}
			}
			global::_003CModule_003E.AssetObjects_002EArtDefSet_002ERemoveCollection(m_artDefSet.op_MemberSelection(), standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void UpdateRootCollectionsFromTemplate(IArtDefTemplate artDefTmpl)
	{
		//IL_002a: Expected I, but got I8
		//IL_0057: Expected I, but got I8
		ArtDefTemplate artDefTemplate = (ArtDefTemplate)artDefTmpl;
		if (!global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateRootCollectionsFromTemplate_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA && artDefTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BE_0040HKFEMOJK_0040typedArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040ECDAEDJE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 59u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FUpdateRootCollectionsFromTemplate_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		global::AssetObjects.ArtDefTemplate* nativeTemplate = artDefTemplate.GetNativeTemplate();
		if (!global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F9_003F_003FUpdateRootCollectionsFromTemplate_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA && nativeTemplate == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BF_0040EEAHAIJN_0040nativeArtDefTemplate_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040ECDAEDJE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 62u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F9_003F_003FUpdateRootCollectionsFromTemplate_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAUIArtDefTemplate_0040345_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		RemoveReferences();
		global::_003CModule_003E.AssetObjects_002EArtDefSet_002EUpdateRootCollectionsFromTemplate(m_artDefSet.op_MemberSelection(), nativeTemplate);
		AddReferences();
	}

	public unsafe virtual string SerializeIntoXML()
	{
		System.Runtime.CompilerServices.Unsafe.SkipInit(out global::AssetObjects.String obj);
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bctor_007D(&obj);
		string result;
		try
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
			global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
			try
			{
				global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
				System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
				throw;
			}
			try
			{
				global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeIntoString_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(&serializer, &obj, m_artDefSet.ArtDefSet);
				result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(&obj));
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		}
		catch
		{
			//try-fault
			global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<global::AssetObjects.String*, void>)(&global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D), &obj);
			throw;
		}
		global::_003CModule_003E.AssetObjects_002EString_002E_007Bdtor_007D(&obj);
		return result;
	}

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool DeserializeFromXML(string xmlText)
	{
		StandardStringWrapper standardStringWrapper = null;
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(xmlText);
		bool flag;
		try
		{
			standardStringWrapper = standardStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			flag = global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserialize_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(m_artDefSet.Deserializer, &resultCode, m_artDefSet.ArtDefSet, standardStringWrapper.Value));
			if (flag)
			{
				RemoveReferences();
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

	[return: MarshalAs(UnmanagedType.U1)]
	public unsafe virtual bool SerializeIntoFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
		bool result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Serializer serializer);
			global::_003CModule_003E.Serialization_002EContext_002E_007Bctor_007D((Context*)(&serializer));
			try
			{
				global::_003CModule_003E.Platform_002EAllocator_002E_007Bctor_007D((Allocator*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)));
				System.Runtime.CompilerServices.Unsafe.As<Serializer, long>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref serializer, 312)) = (nint)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_7_003F_0024HeapAllocator_0040_00240BH_0040_00240A_0040_0040Platform_0040_00406B_0040);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Context*, void>)(&global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D), &serializer);
				throw;
			}
			try
			{
				result = global::_003CModule_003E.AssetObjects_002ESerializer_002ESerializeToFile_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(&serializer, m_artDefSet.ArtDefSet, iOStringWrapper.Value);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<Serializer*, void>)(&global::_003CModule_003E.AssetObjects_002ESerializer_002E_007Bdtor_007D), &serializer);
				throw;
			}
			global::_003CModule_003E.Serialization_002EContext_002E_007Bdtor_007D((Context*)(&serializer));
		}
		catch
		{
			//try-fault
			((IDisposable)iOStringWrapper).Dispose();
			throw;
		}
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe virtual Firaxis.Error.ResultCode DeserializeFromFile(string pmFilename)
	{
		IOStringWrapper iOStringWrapper = null;
		IOStringWrapper iOStringWrapper2 = new IOStringWrapper(pmFilename);
		Firaxis.Error.ResultCode result;
		try
		{
			iOStringWrapper = iOStringWrapper2;
			System.Runtime.CompilerServices.Unsafe.SkipInit(out Platform.ResultCode resultCode);
			global::_003CModule_003E.AssetObjects_002EDeserializer_002EDeserializeFromFile_003Cclass_0020AssetObjects_003A_003AArtDefSet_003E(m_artDefSet.Deserializer, &resultCode, m_artDefSet.ArtDefSet, iOStringWrapper.Value);
			if (!global::_003CModule_003E.Platform_002EResultCode_002E_002E_N(&resultCode))
			{
				result = new Firaxis.Error.ResultCode(new string(global::_003CModule_003E.Platform_002EResultCode_002EGetMessage(&resultCode)));
				goto IL_0055;
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
			RemoveReferences();
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
		IL_0055:
		((IDisposable)iOStringWrapper).Dispose();
		return result;
	}

	public unsafe override string ToString()
	{
		ArtDefSet* artDefSet = ArtDefSet;
		if (artDefSet != null)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E obj);
			global::_003CModule_003E.AssetObjects_002EArtDefSet_002EToString(artDefSet, &obj);
			string result;
			try
			{
				result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(&obj);
			}
			catch
			{
				//try-fault
				global::_003CModule_003E.___CxxCallUnwindDtor((delegate*<void*, void>)(delegate*<basic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E*, void>)(&global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D), &obj);
				throw;
			}
			global::_003CModule_003E.std_002Ebasic_string_003Cchar_002Cstd_003A_003Achar_traits_003Cchar_003E_002Cstd_003A_003Aallocator_003Cchar_003E_0020_003E_002E_007Bdtor_007D(&obj);
			return result;
		}
		return string.Empty;
	}

	internal unsafe global::AssetObjects.ArtDefCollection* FindChildCollection(IEnumerable<string> nameCollection)
	{
		//IL_0003: Expected I, but got I8
		//IL_0006: Expected I, but got I8
		global::AssetObjects.ArtDefCollection* result = null;
		global::AssetObjects.ArtDefElement* ptr = null;
		if (!m_disposed)
		{
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0xc8315dbb_002EFindChildItem(nameCollection, m_artDefSet.ArtDefSet, &result, &ptr);
		}
		return result;
	}

	internal unsafe global::AssetObjects.ArtDefElement* FindChildElement(IEnumerable<string> nameCollection)
	{
		//IL_0003: Expected I, but got I8
		//IL_0006: Expected I, but got I8
		global::AssetObjects.ArtDefCollection* ptr = null;
		global::AssetObjects.ArtDefElement* result = null;
		if (!m_disposed)
		{
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002E_003FA0xc8315dbb_002EFindChildItem(nameCollection, m_artDefSet.ArtDefSet, &ptr, &result);
		}
		return result;
	}

	private unsafe void AddReferences()
	{
		if (!global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA && m_artDefCollections.Count != 0)
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CO_0040IINOIHCE_0040Added_003F5references_003F5without_003F5clearin_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CA_0040OGGJNHGA_0040m_artDefCollections_003F9_003F_0024DOCount_003F5_003F_0024DN_003F_0024DN_003F50_003F_0024AA_0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GJ_0040ECDAEDJE_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 145u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xc8315dbb_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040ArtDef_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040AE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EArtDefSet_002Ebegin(m_artDefSet.op_MemberSelection(), &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefSet_002Eend(m_artDefSet.op_MemberSelection(), &iterator2)))
		{
			do
			{
				global::AssetObjects.ArtDefCollection* artDefCollection = global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002A(&iterator);
				IArtDefCollection item = new ArtDefCollection(this, null, artDefCollection);
				m_artDefCollections.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AArtDefCollection_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EArtDefSet_002Eend(m_artDefSet.op_MemberSelection(), &iterator2)));
		}
		sbyte* value = global::_003CModule_003E.AssetObjects_002EArtDefSet_002EGetTemplateName(m_artDefSet.op_MemberSelection());
		m_templateName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(value);
	}

	private void RemoveReferences()
	{
		m_artDefCollections.Clear();
		m_templateName = string.Empty;
	}

	private void Destroy()
	{
		@lock obj = null;
		@lock obj2 = new @lock(m_shutdownLock);
		try
		{
			obj = obj2;
			if (!m_disposed)
			{
				foreach (ArtDefCollection artDefCollection in m_artDefCollections)
				{
					artDefCollection.Destroy();
				}
				m_artDefCollections.Clear();
				m_templateName = string.Empty;
				m_artDefSet.Destroy();
				m_disposed = true;
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

	[HandleProcessCorruptedStateExceptions]
	protected virtual void Dispose([MarshalAs(UnmanagedType.U1)] bool A_0)
	{
		if (A_0)
		{
			try
			{
				_007EArtDef();
				return;
			}
			finally
			{
				((IDisposable)m_artDefSet).Dispose();
			}
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

	~ArtDef()
	{
		Dispose(A_0: false);
	}
}
