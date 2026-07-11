using System.Collections;
using System.Runtime.CompilerServices;
using IronPython.Runtime.Types;
using Microsoft.Scripting.Actions;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Operations;

public static class TypeTrackerOps
{
	[SpecialName]
	[PropertyMethod]
	public static IDictionary Get__dict__(CodeContext context, TypeTracker self)
	{
		return new DictProxy(DynamicHelpers.GetPythonTypeFromType(self.Type));
	}
}
