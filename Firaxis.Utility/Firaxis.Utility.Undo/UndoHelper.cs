using System.Collections.Generic;
using System.Reflection;
using Firaxis.Reflection;

namespace Firaxis.Utility.Undo;

public static class UndoHelper
{
	public static string AcquireValue(object obj, string field)
	{
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		PropertyInfo propertyInfo = list.Find((PropertyInfo a) => a.Name == field);
		if (propertyInfo != null)
		{
			return Transpose.ToString(propertyInfo.GetValue(obj, null));
		}
		return null;
	}

	public static void ApplyValue(object obj, string field, string value)
	{
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		PropertyInfo propertyInfo = list.Find((PropertyInfo a) => a.Name == field);
		if (propertyInfo != null)
		{
			propertyInfo.SetValue(obj, Transpose.FromString(value, propertyInfo.PropertyType), null);
		}
	}

	public static List<UndoField> AcquireFields(object obj)
	{
		List<UndoField> list = new List<UndoField>();
		List<PropertyInfo> list2 = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		foreach (PropertyInfo item in list2)
		{
			string value = Transpose.ToString(item.GetValue(obj, null));
			list.Add(new UndoField(item.Name, value));
		}
		return list;
	}

	public static void ApplyFields(object obj, List<UndoField> fields)
	{
		List<PropertyInfo> list = new List<PropertyInfo>(ReflectionHelper.CollectProperties(obj));
		foreach (PropertyInfo p in list)
		{
			UndoField undoField = fields.Find((UndoField a) => a.Field == p.Name);
			if (undoField != null)
			{
				p.SetValue(obj, Transpose.FromString(undoField.Value, p.PropertyType), null);
			}
		}
	}
}
