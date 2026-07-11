using System;

namespace IronPython.Runtime.Types;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
internal sealed class DynamicBaseTypeAttribute : Attribute
{
}
