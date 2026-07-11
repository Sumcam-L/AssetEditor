using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public class Index
{
	private readonly object _value;

	internal object Value => _value;

	public Index(object value)
	{
		ContractUtils.RequiresNotNull(value, "value");
		_value = value;
	}
}
