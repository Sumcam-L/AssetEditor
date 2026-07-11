using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class FloatValue : Value, IFloatValue
{
	public override ValueType ParameterType => ValueType.VT_FLOAT;

	public unsafe virtual float ParameterValue
	{
		get
		{
			return global::_003CModule_003E.AssetObjects_002EFloatValue_002EGetValue((global::AssetObjects.FloatValue*)m_pkValue);
		}
		set
		{
			global::_003CModule_003E.AssetObjects_002EFloatValue_002ESetValue((global::AssetObjects.FloatValue*)m_pkValue, value);
		}
	}

	public unsafe FloatValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		float num = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003AFloatValue_002Cfloat_003E(pkCollectionValue, szName, &num));
	}

	public unsafe FloatValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		float num = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003AFloatValue_002Cfloat_003E(pkValueSet, szName, &num));
	}

	public unsafe FloatValue(global::AssetObjects.FloatValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_FLOAT)
		{
			IFloatValue floatValue = (IFloatValue)otherValue;
			ParameterValue = floatValue.ParameterValue;
		}
	}

	private unsafe global::AssetObjects.FloatValue* GetValue()
	{
		return (global::AssetObjects.FloatValue*)m_pkValue;
	}
}
