using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefRefParameter : Parameter, IArtDefRefParameter
{
	private ICollection<string> _allowedCollectionNames = new List<string>();

	private ICollection<string> _allowedArtDefTemplateNames = new List<string>();

	public virtual IEnumerable<string> AllowedArtDefTemplateNames => _allowedArtDefTemplateNames;

	public virtual IEnumerable<string> AllowedCollectionNames => _allowedCollectionNames;

	public unsafe virtual bool CollectionIsLocked
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return ((IArtDefRefValue)DefaultValue)?.IsCollectionLocked ?? false;
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			//IL_0012: Expected I, but got I8
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetCollectionIsLocked(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), value);
		}
	}

	public unsafe virtual string DefaultTemplateName
	{
		get
		{
			IArtDefRefValue artDefRefValue = (IArtDefRefValue)DefaultValue;
			if (artDefRefValue == null)
			{
				return string.Empty;
			}
			return artDefRefValue.TemplateName;
		}
		set
		{
			//IL_004a: Expected I, but got I8
			//IL_0026: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultTemplateName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0P_0040NLEIMIAF_0040pmTemplateName_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040BAKNPOOI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 137u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultTemplateName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetTemplateName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			AddAllowedArtDefTemplate(value);
		}
	}

	public unsafe virtual string DefaultArtDefPath
	{
		get
		{
			IArtDefRefValue artDefRefValue = (IArtDefRefValue)DefaultValue;
			if (artDefRefValue == null)
			{
				return string.Empty;
			}
			return artDefRefValue.ArtDefPath;
		}
		set
		{
			//IL_0047: Expected I, but got I8
			//IL_0023: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultArtDefPath_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0N_0040FLBGJENA_0040pmArtDefPath_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040BAKNPOOI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 118u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultArtDefPath_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetArtDefPath(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual string DefaultCollectionName
	{
		get
		{
			IArtDefRefValue artDefRefValue = (IArtDefRefValue)DefaultValue;
			if (artDefRefValue == null)
			{
				return string.Empty;
			}
			return artDefRefValue.RootCollectionName;
		}
		set
		{
			//IL_0047: Expected I, but got I8
			//IL_0023: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultCollectionName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0BB_0040NABFCIKB_0040pmCollectionName_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040BAKNPOOI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 97u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultCollectionName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetRootCollectionName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
			AddAllowedCollection(value);
		}
	}

	public unsafe virtual string DefaultElementName
	{
		get
		{
			IArtDefRefValue artDefRefValue = (IArtDefRefValue)DefaultValue;
			if (artDefRefValue == null)
			{
				return string.Empty;
			}
			return artDefRefValue.ElementName;
		}
		set
		{
			//IL_0047: Expected I, but got I8
			//IL_0023: Expected I, but got I8
			if (!global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultElementName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA && value == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0O_0040KPFIFOK_0040pmElementName_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HK_0040BAKNPOOI_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 78u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0xe782f356_002E_003FbIgnoreAlways_0040_003F2_003F_003Fset_0040DefaultElementName_0040ArtDefRefParameter_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040UE_0024AAMXPE_0024AAVString_0040System_0040_0040_0040Z_00404_NA), (Platform.ErrorType)0))
			{
				/*OpCode not supported: DebugBreak*/;
			}
			global::AssetObjects.Parameter* pkParameter = m_pkParameter;
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetElementName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter), ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual bool IsNullAllowed
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EIsNullAllowed((ArtDefReferenceParameter*)m_pkParameter);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002ESetNullAllowed((ArtDefReferenceParameter*)m_pkParameter, value);
		}
	}

	public unsafe ArtDefRefParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AArtDefReferenceParameter_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkParameterSet, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)))
	{
	}

	public unsafe ArtDefRefParameter(ArtDefReferenceParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
		//IL_0046: Expected I, but got I8
		//IL_0068: Expected I, but got I8
		if (pkParameter != null)
		{
			sbyte* ptr = global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EGetCollectionName(pkParameter);
			if (!global::_003CModule_003E.String_002E_003FA0xe782f356_002EIsNullOrEmpty(ptr))
			{
				global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EAddAllowedCollectionName(pkParameter, ptr);
			}
			sbyte* ptr2 = global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetRootCollectionName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter));
			if (!global::_003CModule_003E.String_002E_003FA0xe782f356_002EIsNullOrEmpty(ptr2))
			{
				global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EAddAllowedCollectionName(pkParameter, ptr2);
			}
			sbyte* ptr3 = global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetTemplateName(((delegate* unmanaged[Cdecl, Cdecl]<IntPtr, ArtDefReferenceValue*>)(*(ulong*)(*(long*)pkParameter + 24)))((nint)pkParameter));
			if (!global::_003CModule_003E.String_002E_003FA0xe782f356_002EIsNullOrEmpty(ptr3))
			{
				global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EAddAllowedTemplateName(pkParameter, ptr3);
			}
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EReconcileStringContainers(global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EGetAllowedTemplateNames(pkParameter), _allowedArtDefTemplateNames);
			global::_003CModule_003E.Firaxis_002ECivTech_002EAssetObjects_002EReconcileStringContainers(global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EGetAllowedCollectionNames(pkParameter), _allowedCollectionNames);
		}
	}

	public unsafe virtual void AddAllowedCollection(string collectionName)
	{
		if (!string.IsNullOrWhiteSpace(collectionName) && !_allowedCollectionNames.Contains(collectionName))
		{
			_allowedCollectionNames.Add(collectionName);
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(collectionName).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EAddAllowedCollectionName((ArtDefReferenceParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual void RemoveAllowedCollection(string collectionName)
	{
		if (_allowedCollectionNames.Remove(collectionName))
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(collectionName).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002ERemoveAllowedCollectionName((ArtDefReferenceParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual void ClearAllowedCollections()
	{
		_allowedCollectionNames.Clear();
		global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EClearAllowedCollectionNames((ArtDefReferenceParameter*)m_pkParameter);
	}

	public unsafe virtual void AddAllowedArtDefTemplate(string templateName)
	{
		if (!string.IsNullOrWhiteSpace(templateName) && !_allowedArtDefTemplateNames.Contains(templateName))
		{
			_allowedArtDefTemplateNames.Add(templateName);
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(templateName).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EAddAllowedTemplateName((ArtDefReferenceParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual void RemoveAllowedArtDefTemplate(string templateName)
	{
		if (_allowedArtDefTemplateNames.Remove(templateName))
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(templateName).ToPointer();
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002ERemoveAllowedTemplateName((ArtDefReferenceParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe virtual void ClearAllowedArtDefTemplates()
	{
		_allowedArtDefTemplateNames.Clear();
		global::_003CModule_003E.AssetObjects_002EArtDefReferenceParameter_002EClearAllowedTemplateNames((ArtDefReferenceParameter*)m_pkParameter);
	}

	private unsafe ArtDefReferenceParameter* GetParameter()
	{
		return (ArtDefReferenceParameter*)m_pkParameter;
	}
}
