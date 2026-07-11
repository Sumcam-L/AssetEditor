using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;
using Types;

namespace Firaxis.CivTech.AssetObjects;

public class CloudEntity : ICloudEntity
{
	protected unsafe Entity* m_pkEntity;

	private ICollection<string> m_tags;

	private string m_name;

	private string m_description;

	public unsafe virtual Version Version
	{
		get
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out VersionInfo versionInfo);
			global::_003CModule_003E.AssetObjects_002EEntity_002EGetVersion(m_pkEntity, &versionInfo);
			return new Version(*(int*)(&versionInfo), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 4)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 8)), System.Runtime.CompilerServices.Unsafe.As<VersionInfo, int>(ref System.Runtime.CompilerServices.Unsafe.AddByteOffset(ref versionInfo, 12)));
		}
	}

	public virtual IEnumerable<string> Tags => m_tags;

	public unsafe virtual string Description
	{
		get
		{
			return m_description;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_description = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EEntity_002ESetDescription(m_pkEntity, standardStringWrapper.Value);
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

	public unsafe virtual string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_name = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EEntity_002ESetName(m_pkEntity, standardStringWrapper.Value);
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

	public unsafe virtual bool IsClassEntity
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EEntity_002EIsClass(m_pkEntity);
		}
	}

	public unsafe virtual void SetVersion(int major, int minor, int build, int revision)
	{
		global::_003CModule_003E.AssetObjects_002EEntity_002ESetVersion(m_pkEntity, major, minor, build, revision);
	}

	public unsafe virtual void SetVersion(string pmVersion)
	{
		Version result = null;
		if (Version.TryParse(pmVersion, out result))
		{
			global::_003CModule_003E.AssetObjects_002EEntity_002ESetVersion(m_pkEntity, result.Major, result.Minor, result.Build, result.Revision);
		}
	}

	public virtual string FlattenTagsToString()
	{
		return string.Join(", ", m_tags);
	}

	public virtual void SetTagsFromString(string tags)
	{
		string[] array = tags.Split(',');
		ClearTags();
		int num = 0;
		if (0 < (nint)array.LongLength)
		{
			do
			{
				string text = array[num];
				AddTag(text.Trim());
				num++;
			}
			while (num < (nint)array.LongLength);
		}
	}

	public unsafe virtual void AddTag(string tag)
	{
		StandardStringWrapper standardStringWrapper = null;
		m_tags.Add(tag);
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(tag);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EEntity_002EAddTag(m_pkEntity, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void RemoveTag(string tag)
	{
		StandardStringWrapper standardStringWrapper = null;
		m_tags.Remove(tag);
		StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(tag);
		try
		{
			standardStringWrapper = standardStringWrapper2;
			global::_003CModule_003E.AssetObjects_002EEntity_002ERemoveTag(m_pkEntity, standardStringWrapper.Value);
		}
		catch
		{
			//try-fault
			((IDisposable)standardStringWrapper).Dispose();
			throw;
		}
		((IDisposable)standardStringWrapper).Dispose();
	}

	public unsafe virtual void ClearTags()
	{
		m_tags.Clear();
		global::_003CModule_003E.AssetObjects_002EEntity_002EClearTags(m_pkEntity);
	}

	internal unsafe Entity* GetAssetObject()
	{
		return m_pkEntity;
	}

	internal unsafe virtual void AddReferences()
	{
		if (!global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA && !string.IsNullOrEmpty(m_name))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D2);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EE_0040CELMIJNH_0040Called_003F5add_003F5references_003F5without_003F5ca_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CG_0040MOHEAKGG_0040System_003F3_003F3String_003F3_003F3IsNullOrEmpty_003F_0024CIm__0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D2), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040IDHGDDPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 114u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F2_003F_003FAddReferences_0040CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		if (!global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA && !string.IsNullOrEmpty(m_description))
		{
			System.Runtime.CompilerServices.Unsafe.SkipInit(out _0024ArrayType_0024_0024_0024BY0IAA_0040D _0024ArrayType_0024_0024_0024BY0IAA_0040D3);
			global::_003CModule_003E.Platform_002Esnprintf((sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), 2048uL, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0EE_0040CELMIJNH_0040Called_003F5add_003F5references_003F5without_003F5ca_0040), __arglist());
			if (global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0CN_0040KJOKDHDP_0040System_003F3_003F3String_003F3_003F3IsNullOrEmpty_003F_0024CIm__0040), (sbyte*)(&_0024ArrayType_0024_0024_0024BY0IAA_0040D3), (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040IDHGDDPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 115u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F9_003F_003FAddReferences_0040CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXXZ_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
		}
		m_name = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EEntity_002EGetName(m_pkEntity));
		m_description = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EEntity_002EGetDescription(m_pkEntity));
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator);
		global::_003CModule_003E.AssetObjects_002EEntity_002Ebegin_tags(m_pkEntity, &iterator);
		System.Runtime.CompilerServices.Unsafe.SkipInit(out ChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E.iterator iterator2);
		if (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEntity_002Eend_tags(m_pkEntity, &iterator2)))
		{
			do
			{
				string item = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EString_002Ec_str(global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002D_003E(&iterator)));
				m_tags.Add(item);
				global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_002B_002B(&iterator);
			}
			while (global::_003CModule_003E.Types_002EChunkedVector_003CAssetObjects_003A_003AString_002C4096_003E_002Eiterator_002E_0021_003D(&iterator, global::_003CModule_003E.AssetObjects_002EEntity_002Eend_tags(m_pkEntity, &iterator2)));
		}
	}

	internal unsafe virtual void RemoveReferences([MarshalAs(UnmanagedType.U1)] bool bDisposing)
	{
		//IL_002c: Expected I, but got I8
		m_name = string.Empty;
		m_description = string.Empty;
		m_tags.Clear();
		if (bDisposing)
		{
			m_pkEntity = null;
		}
	}

	protected unsafe CloudEntity(Entity* pkEntity)
	{
		//IL_003b: Expected I, but got I8
		m_pkEntity = pkEntity;
		m_tags = new List<string>();
		base._002Ector();
		if (!global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAVEntity_00402_0040_0040Z_00404_NA && pkEntity == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_06GOJKNBFM_0040entity_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0GG_0040IDHGDDPC_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 18u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xf47d89a5_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0CloudEntity_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040IE_0024AAM_0040PEAVEntity_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		AddReferences();
	}
}
