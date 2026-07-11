using System.Collections.Generic;
using System.Linq;

namespace Firaxis.CivTech.AssetObjects;

public static class IObjectValueExtensions
{
	public static bool IsBound(this IObjectValue objValue)
	{
		string boundObjectName = objValue.GetBoundObjectName();
		InstanceType boundObjectType = objValue.GetBoundObjectType();
		return !string.IsNullOrEmpty(boundObjectName) && boundObjectType != InstanceType.IT_INVALID;
	}

	public static bool IsBoundWith(this IObjectValue objValue, InstanceType testType)
	{
		string boundObjectName = objValue.GetBoundObjectName();
		InstanceType boundObjectType = objValue.GetBoundObjectType();
		return !string.IsNullOrEmpty(boundObjectName) && boundObjectType == testType;
	}

	public static IEnumerable<IObjectValue> GetObjectValues(this IObjectCollectionValue value)
	{
		return value.Items.OfType<IObjectValue>();
	}

	public static IEnumerable<IObjectValue> GetBoundObjectValues(this IEnumerable<IObjectValue> objectValues)
	{
		return objectValues.Where((IObjectValue val) => val.IsBound());
	}

	public static IEnumerable<IObjectValue> GetBoundObjectValues(this IEnumerable<IObjectValue> objectValues, InstanceType testType)
	{
		return objectValues.Where((IObjectValue val) => val.IsBoundWith(testType));
	}
}
