using System;

namespace IronPython.Runtime.Types;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = false)]
internal sealed class SlotFieldAttribute : Attribute
{
}
