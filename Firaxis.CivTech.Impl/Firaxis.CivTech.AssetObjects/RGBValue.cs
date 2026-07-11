using System.Drawing;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class RGBValue : Value, IRGBValue
{
	public override ValueType ParameterType => ValueType.VT_RGB;

	public unsafe virtual Color ParameterValue
	{
		get
		{
			global::AssetObjects.RGBValue* pkValue = (global::AssetObjects.RGBValue*)m_pkValue;
			int red = (int)(double)global::_003CModule_003E.AssetObjects_002ERGBValue_002EGetR(pkValue);
			int green = (int)(double)global::_003CModule_003E.AssetObjects_002ERGBValue_002EGetG(pkValue);
			int blue = (int)(double)global::_003CModule_003E.AssetObjects_002ERGBValue_002EGetB(pkValue);
			return Color.FromArgb(red, green, blue);
		}
		set
		{
			float num = (int)value.R;
			float num2 = (int)value.G;
			float num3 = (int)value.B;
			global::_003CModule_003E.AssetObjects_002ERGBValue_002ESetValue((global::AssetObjects.RGBValue*)m_pkValue, num, num2, num3);
		}
	}

	public unsafe RGBValue(global::AssetObjects.CollectionValue* pkCollectionValue, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003ARGBValue_002Cfloat_002Cfloat_002Cfloat_003E(pkCollectionValue, szName, &num3, &num2, &num));
	}

	public unsafe RGBValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName)
	{
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003ARGBValue_002Cfloat_002Cfloat_002Cfloat_003E(pkValueSet, szName, &num3, &num2, &num));
	}

	public unsafe RGBValue(global::AssetObjects.RGBValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_RGB)
		{
			Color parameterValue = ((IRGBValue)otherValue).ParameterValue;
			ParameterValue = parameterValue;
		}
	}

	private unsafe global::AssetObjects.RGBValue* GetValue()
	{
		return (global::AssetObjects.RGBValue*)m_pkValue;
	}
}
