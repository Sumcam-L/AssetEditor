using System;

namespace IronPython;

[AttributeUsage(AttributeTargets.ReturnValue)]
public sealed class MaybeNotImplementedAttribute : Attribute
{
}
