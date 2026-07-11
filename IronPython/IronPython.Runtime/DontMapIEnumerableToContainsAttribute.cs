using System;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class DontMapIEnumerableToContainsAttribute : Attribute
{
}
