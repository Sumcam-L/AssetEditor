using System;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
public sealed class ClassMethodAttribute : Attribute
{
}
