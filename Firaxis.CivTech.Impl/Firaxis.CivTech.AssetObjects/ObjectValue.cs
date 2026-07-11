using System;
using AssetObjects;

namespace Firaxis.CivTech.AssetObjects;

public class ObjectValue : Value, IObjectValue
{
	public override ValueType ParameterType => ValueType.VT_OBJECT;

	public unsafe ObjectValue(global::AssetObjects.CollectionValue* pkCollecionValue, sbyte* szName, InstanceType eInstanceType, sbyte* szObjectName)
	{
		global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)eInstanceType;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002ECollectionValue_002EPush_003Cclass_0020AssetObjects_003A_003AObjectValue_002Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_0020_0026_003E(pkCollecionValue, szName, &instanceType, &szObjectName));
	}

	public unsafe ObjectValue(global::AssetObjects.ValueSet* pkValueSet, sbyte* szName, InstanceType eInstanceType, sbyte* szObjectName)
	{
		global::AssetObjects.InstanceType instanceType = (global::AssetObjects.InstanceType)eInstanceType;
		base._002Ector((global::AssetObjects.Value*)global::_003CModule_003E.AssetObjects_002EValueSet_002EPush_003Cclass_0020AssetObjects_003A_003AObjectValue_002Cenum_0020AssetObjects_003A_003AInstanceType_002Cchar_0020const_0020_002A_0020_0026_003E(pkValueSet, szName, &instanceType, &szObjectName));
	}

	public unsafe ObjectValue(global::AssetObjects.ObjectValue* pkValue)
		: base((global::AssetObjects.Value*)pkValue)
	{
	}

	public unsafe virtual InstanceType GetBoundObjectType()
	{
		int result = 12;
		global::AssetObjects.Value* pkValue = m_pkValue;
		if (pkValue != null)
		{
			result = (int)global::_003CModule_003E.AssetObjects_002EObjectValue_002EGetBoundObjectType((global::AssetObjects.ObjectValue*)pkValue);
		}
		return (InstanceType)result;
	}

	public unsafe virtual string GetBoundObjectName()
	{
		string result = string.Empty;
		global::AssetObjects.ObjectValue* pkValue = (global::AssetObjects.ObjectValue*)m_pkValue;
		if (pkValue != null)
		{
			result = global::_003CModule_003E.Firaxis_002ECivTech_002EGetManagedString(global::_003CModule_003E.AssetObjects_002EObjectValue_002EGetBoundObjectName(pkValue));
		}
		return result;
	}

	public unsafe virtual void BindObject(string objectName, InstanceType objectInstanceType)
	{
		StandardStringWrapper standardStringWrapper = null;
		global::AssetObjects.ObjectValue* pkValue = (global::AssetObjects.ObjectValue*)m_pkValue;
		if (pkValue != null)
		{
			StandardStringWrapper standardStringWrapper2 = new StandardStringWrapper(objectName);
			try
			{
				standardStringWrapper = standardStringWrapper2;
				global::_003CModule_003E.AssetObjects_002EObjectValue_002EBindObject(pkValue, standardStringWrapper.Value, (global::AssetObjects.InstanceType)objectInstanceType);
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

	public override void CopyDataFrom(IValue otherValue)
	{
		if (otherValue.ParameterType == ValueType.VT_OBJECT)
		{
			IObjectValue objectValue = (IObjectValue)otherValue;
			BindObject(objectValue.GetBoundObjectName(), objectValue.GetBoundObjectType());
		}
	}

	private unsafe global::AssetObjects.ObjectValue* GetValue()
	{
		return (global::AssetObjects.ObjectValue*)m_pkValue;
	}
}
