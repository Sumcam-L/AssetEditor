using IronPython.Runtime.Exceptions;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;

namespace IronPython.Runtime;

public sealed class ModuleGlobalCache
{
	private object _value;

	internal static readonly object NotCaching = new object();

	internal static readonly ModuleGlobalCache NoCache = new ModuleGlobalCache(NotCaching);

	public bool IsCaching => _value != NotCaching;

	public bool HasValue => _value != Uninitialized.Instance;

	public object Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_value == NotCaching)
			{
				throw new ValueErrorException("Cannot change non-caching value.");
			}
			_value = value;
		}
	}

	public ModuleGlobalCache(object value)
	{
		_value = value;
	}

	public void Changed(object sender, ModuleChangeEventArgs e)
	{
		ContractUtils.RequiresNotNull(e, "e");
		switch (e.ChangeType)
		{
		case ModuleChangeType.Delete:
			Value = Uninitialized.Instance;
			break;
		case ModuleChangeType.Set:
			Value = e.Value;
			break;
		}
	}
}
