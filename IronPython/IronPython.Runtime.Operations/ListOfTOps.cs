using System.Collections.Generic;
using System.Text;
using IronPython.Runtime.Types;

namespace IronPython.Runtime.Operations;

public static class ListOfTOps<T>
{
	public static string __repr__(CodeContext context, List<T> self)
	{
		List<object> andCheckInfinite = PythonOps.GetAndCheckInfinite(self);
		if (andCheckInfinite == null)
		{
			return "[...]";
		}
		int count = andCheckInfinite.Count;
		andCheckInfinite.Add(self);
		try
		{
			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append("List[");
			stringBuilder.Append(DynamicHelpers.GetPythonTypeFromType(typeof(T)).Name);
			stringBuilder.Append("](");
			if (self.Count > 0)
			{
				stringBuilder.Append("[");
				string value = "";
				foreach (T item in self)
				{
					stringBuilder.Append(value);
					stringBuilder.Append(PythonOps.Repr(context, item));
					value = ", ";
				}
				stringBuilder.Append("]");
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
