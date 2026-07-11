using System;
using System.Runtime.CompilerServices;
using _003CCppImplementationDetails_003E;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BLPEntryValue : Value, IBLPEntryValue
{
	private string m_entryName;

	private string m_xlpPath;

	private string m_blpPackage;

	private string m_xlpClass;

	private string m_libraryName;

	public override ValueType ParameterType => ValueType.VT_BLP_ENTRY;

	public unsafe virtual string LibraryName
	{
		get
		{
			return m_libraryName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_libraryName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetLibraryName((global::AssetObjects.BLPEntryValue*)m_pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string XLPClass
	{
		get
		{
			return m_xlpClass;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_xlpClass = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetXLPClass((global::AssetObjects.BLPEntryValue*)m_pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string BLPPackage
	{
		get
		{
			return m_blpPackage;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_blpPackage = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetBLPPackage((global::AssetObjects.BLPEntryValue*)m_pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string XLPPath
	{
		get
		{
			return m_xlpPath;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_xlpPath = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetXLPPath((global::AssetObjects.BLPEntryValue*)m_pkValue, standardStringWrapper.Value);
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

	public unsafe virtual string EntryName
	{
		get
		{
			return m_entryName;
		}
		set
		{
			StandardStringWrapper standardStringWrapper = null;
			m_entryName = value;
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(value);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002ESetEntryName((global::AssetObjects.BLPEntryValue*)m_pkValue, standardStringWrapper.Value);
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

	public unsafe BLPEntryValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ABLPEntryValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkCollectionValue, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)))
	{
		CacheValues();
	}

	public unsafe BLPEntryValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
		: base((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ABLPEntryValue_002Cchar_0020const_0020_0028_0026_0029_005B1_005D_003E(pkValueSet, szName, (_0024ArrayType_0024_0024_0024BY00_0024_0024CBD*)System.Runtime.CompilerServices.Unsafe.AsPointer(ref global::_003CModule_003E._003F_003F_C_0040_00CNPNBAHC_0040_003F_0024AA_0040)))
	{
		CacheValues();
	}

	public unsafe BLPEntryValue(global::AssetObjects.BLPEntryValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
		CacheValues();
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_BLP_ENTRY)
		{
			IBLPEntryValue iBLPEntryValue = (IBLPEntryValue)otherValue;
			EntryName = iBLPEntryValue.EntryName;
			BLPPackage = iBLPEntryValue.BLPPackage;
			LibraryName = iBLPEntryValue.LibraryName;
			XLPPath = iBLPEntryValue.XLPPath;
			XLPClass = iBLPEntryValue.XLPClass;
		}
	}

	private unsafe global::AssetObjects.BLPEntryValue* GetValue()
	{
		return (global::AssetObjects.BLPEntryValue*)m_pkValue;
	}

	private unsafe void CacheValues()
	{
		global::AssetObjects.BLPEntryValue* pkValue = (global::AssetObjects.BLPEntryValue*)m_pkValue;
		if (pkValue != null)
		{
			m_entryName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002EGetEntryName(pkValue));
			m_xlpPath = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002EGetXLPPath(pkValue));
			m_blpPackage = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002EGetBLPPackage(pkValue));
			m_xlpClass = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002EGetXLPClass(pkValue));
			m_libraryName = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EBLPEntryValue_002EGetLibraryName(pkValue));
		}
	}
}
