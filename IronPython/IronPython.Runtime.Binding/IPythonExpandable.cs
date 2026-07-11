using System.Collections.Generic;

namespace IronPython.Runtime.Binding;

public interface IPythonExpandable
{
	IDictionary<string, object> CustomAttributes { get; }

	CodeContext Context { get; }

	IDictionary<string, object> EnsureCustomAttributes();
}
