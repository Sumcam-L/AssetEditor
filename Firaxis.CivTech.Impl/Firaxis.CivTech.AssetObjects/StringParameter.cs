using System;
using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class StringParameter : Parameter, IStringParameter
{
	public unsafe virtual string UIHints
	{
		get
		{
			sbyte* value = global::_003CModule_003E.AssetObjects_002EStringParameter_002EGetUIHints((global::AssetObjects.StringParameter*)m_pkParameter);
			IntPtr ptr = new IntPtr(value);
			return Marshal.PtrToStringAnsi(ptr);
		}
		set
		{
			sbyte* ptr = (sbyte*)Marshal.StringToHGlobalAnsi(value).ToPointer();
			global::_003CModule_003E.AssetObjects_002EStringParameter_002ESetUIHints((global::AssetObjects.StringParameter*)m_pkParameter, ptr);
			IntPtr hglobal = new IntPtr(ptr);
			Marshal.FreeHGlobal(hglobal);
		}
	}

	public unsafe StringParameter(global::AssetObjects.ParameterSet* pkParameterSet, sbyte* szName)
		: base((global::AssetObjects.Parameter*)global::_003CModule_003E.AssetObjects_002EParameterSet_002EPush_003Cclass_0020AssetObjects_003A_003AStringParameter_003E(pkParameterSet, szName))
	{
	}

	public unsafe StringParameter(global::AssetObjects.StringParameter* pkParameter)
		: base((global::AssetObjects.Parameter*)pkParameter)
	{
	}

	private unsafe global::AssetObjects.StringParameter* GetParameter()
	{
		return (global::AssetObjects.StringParameter*)m_pkParameter;
	}
}
