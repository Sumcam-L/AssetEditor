using System.Collections.Generic;
using Microsoft.Scripting.Runtime;

namespace IronPython.Runtime.Exceptions;

internal interface IPythonAwareException
{
	object PythonException { get; set; }

	List<DynamicStackFrame> Frames { get; set; }

	TraceBack TraceBack { get; set; }
}
