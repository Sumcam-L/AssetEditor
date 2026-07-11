using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class IntValue : Value, IIntValue
{
	public override ValueType ParameterType => ValueType.VT_INT;

	public unsafe virtual int ParameterValue
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EIntValue_002EGetValue((global::AssetObjects.IntValue*)m_pkValue);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EIntValue_002ESetValue((global::AssetObjects.IntValue*)m_pkValue, value);
		}
	}

	public unsafe IntValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		int num = 0;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003AIntValue_002Cint_003E(pkCollectionValue, szName, &num));
	}

	public unsafe IntValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		int num = 0;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003AIntValue_002Cint_003E(pkValueSet, szName, &num));
	}

	public unsafe IntValue(global::AssetObjects.IntValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_INT)
		{
			IIntValue intValue = (IIntValue)otherValue;
			ParameterValue = intValue.ParameterValue;
		}
	}

	private unsafe global::AssetObjects.IntValue* GetValue()
	{
		return (global::AssetObjects.IntValue*)m_pkValue;
	}
}
