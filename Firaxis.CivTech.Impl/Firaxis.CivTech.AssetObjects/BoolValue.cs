using System.Runtime.InteropServices;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class BoolValue : Value, IBoolValue
{
	public override ValueType ParameterType => ValueType.VT_BOOL;

	public unsafe virtual bool ParameterValue
	{
		[return: MarshalAs(UnmanagedType.U1)]
		get
		{
			return global::_003CModule_003E.AssetObjects_002EBoolValue_002EGetValue((global::AssetObjects.BoolValue*)m_pkValue);
		}
		[param: MarshalAs(UnmanagedType.U1)]
		set
		{
			global::_003CModule_003E.AssetObjects_002EBoolValue_002ESetValue((global::AssetObjects.BoolValue*)m_pkValue, value);
		}
	}

	public unsafe BoolValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		bool flag = false;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ABoolValue_002Cbool_003E(pkCollectionValue, szName, &flag));
	}

	public unsafe BoolValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		bool flag = false;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ABoolValue_002Cbool_003E(pkValueSet, szName, &flag));
	}

	public unsafe BoolValue(global::AssetObjects.BoolValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_BOOL)
		{
			IBoolValue boolValue = (IBoolValue)otherValue;
			ParameterValue = boolValue.ParameterValue;
		}
	}

	private unsafe global::AssetObjects.BoolValue* GetValue()
	{
		return (global::AssetObjects.BoolValue*)m_pkValue;
	}
}
