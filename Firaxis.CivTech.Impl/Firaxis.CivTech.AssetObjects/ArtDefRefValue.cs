using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;
using Platform;

namespace Firaxis.CivTech.AssetObjects;

public class ArtDefRefValue : Value, IArtDefRefValue
{
	private string m_elementName;

	private string m_rootCollectionName;

	private string m_artDefPath;

	private string m_templateName;

	public override ValueType ParameterType => ValueType.VT_ARTDEF_REFERENCE;

	public unsafe virtual bool IsCollectionLocked
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetCollectionIsLocked((ArtDefReferenceValue*)m_pkValue);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetCollectionIsLocked((ArtDefReferenceValue*)m_pkValue, value);
		}
	}

	public unsafe virtual string TemplateName
	{
		get
		{
			return m_templateName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (m_templateName == value)
			{
				return;
			}
			m_templateName = value;
			ArtDefReferenceValue* pkValue = (ArtDefReferenceValue*)m_pkValue;
			if (pkValue != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetTemplateName(pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string ArtDefPath
	{
		get
		{
			return m_artDefPath;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (m_artDefPath == value)
			{
				return;
			}
			m_artDefPath = value;
			ArtDefReferenceValue* pkValue = (ArtDefReferenceValue*)m_pkValue;
			if (pkValue != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetArtDefPath(pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string RootCollectionName
	{
		get
		{
			return m_rootCollectionName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (m_rootCollectionName == value)
			{
				return;
			}
			m_rootCollectionName = value;
			ArtDefReferenceValue* pkValue = (ArtDefReferenceValue*)m_pkValue;
			if (pkValue != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetRootCollectionName(pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string ElementName
	{
		get
		{
			return m_elementName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			if (m_elementName == value)
			{
				return;
			}
			m_elementName = value;
			ArtDefReferenceValue* pkValue = (ArtDefReferenceValue*)m_pkValue;
			if (pkValue != null)
			{
				StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
				try
				{
					standardStringWrapper = standardStringWrapper2;
					global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetElementName(pkValue, standardStringWrapper.Value);
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

	public unsafe virtual void SetFromInfo(ArtDefReferenceInfo info)
	{
		ArtDefReferenceValue* pkValue = (ArtDefReferenceValue*)m_pkValue;
		if (pkValue != null)
		{
			RootCollectionName = info.collectionName;
			ElementName = info.elementName;
			ArtDefPath = info.artDefPath;
			TemplateName = info.templateName;
			global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002ESetCollectionIsLocked(pkValue, info.isCollectionLocked);
		}
	}

	public unsafe ArtDefRefValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003AArtDefReferenceValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkCollectionValue, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040), (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)))
	{
		m_elementName = string.Empty;
		m_rootCollectionName = string.Empty;
		m_artDefPath = string.Empty;
		m_templateName = string.Empty;
	}

	public unsafe ArtDefRefValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003AArtDefReferenceValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkValueSet, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040), (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)))
	{
		m_elementName = string.Empty;
		m_rootCollectionName = string.Empty;
		m_artDefPath = string.Empty;
		m_templateName = string.Empty;
	}

	public unsafe ArtDefRefValue(ArtDefReferenceValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
		//IL_002a: Expected I, but got I8
		if (!global::_003CModule_003E._003FA0x0248b55e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefRefValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefReferenceValue_00402_0040_0040Z_00404_NA && pkValue == null && global::_003CModule_003E.Platform_002EAssertDlg((sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_05MFEJDJP_0040value_003F_0024AA_0040), null, (sbyte*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_0HC_0040PMKENJGO_0040C_003F3_003F2BuildAgent_003F2work_003F2acf3423fb2e59_0040), 13u, (bool*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003FA0x0248b55e_002E_003FbIgnoreAlways_0040_003F2_003F_003F_003F0ArtDefRefValue_0040AssetObjects_0040CivTech_0040Firaxis_0040_0040QE_0024AAM_0040PEAVArtDefReferenceValue_00402_0040_0040Z_00404_NA), (Platform.ErrorType)0))
		{
			/*OpCode not supported: DebugBreak*/;
		}
		m_elementName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetElementName(pkValue));
		m_rootCollectionName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetRootCollectionName(pkValue));
		m_artDefPath = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetArtDefPath(pkValue));
		m_templateName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EArtDefReferenceValue_002EGetTemplateName(pkValue));
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_ARTDEF_REFERENCE)
		{
			IArtDefRefValue value = (IArtDefRefValue)otherValue;
			ArtDefReferenceInfo fromInfo = new ArtDefReferenceInfo(value);
			SetFromInfo(fromInfo);
		}
	}

	private unsafe ArtDefReferenceValue* GetValue()
	{
		return (ArtDefReferenceValue*)m_pkValue;
	}
}
