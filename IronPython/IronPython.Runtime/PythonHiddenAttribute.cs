using System;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Event, AllowMultiple = false, Inherited = false)]
internal sealed class PythonHiddenAttribute : Attribute
{
}
