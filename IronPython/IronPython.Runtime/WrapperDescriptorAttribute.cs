using System;

namespace IronPython.Runtime;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true, Inherited = false)]
internal sealed class WrapperDescriptorAttribute : Attribute
{
}
