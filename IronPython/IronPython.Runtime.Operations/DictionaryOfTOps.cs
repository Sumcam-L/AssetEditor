using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations;

public static class DictionaryOfTOps<K, V>
{
	public static string __repr__(CodeContext context, Dictionary<K, V> self)
	{
		List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(self);
		if (andCheckInfinite == null)
		{
			return "{...}";
		}
		int count = andCheckInfinite.Count;
		andCheckInfinite.Add(self);
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("Dictionary[");
			stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(typeof(K)).Name);
			stringBuilder.Append(", ");
			stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(typeof(V)).Name);
			stringBuilder.Append("](");
			if (self.Count > 0)
			{
				stringBuilder.Append("{");
				string value = "";
				foreach (KeyValuePair<K, V> item in self)
				{
					stringBuilder.Append(value);
					stringBuilder.Append(PythonOps.Repr(context, item.Key));
					stringBuilder.Append(" : ");
					stringBuilder.Append(PythonOps.Repr(context, item.Value));
					value = ", ";
				}
				stringBuilder.Append("}");
			}
			stringBuilder.Append(")");
			return stringBuilder.ToString();
		}
		finally
		{
			andCheckInfinite.RemoveAt(count);
		}
	}
}
