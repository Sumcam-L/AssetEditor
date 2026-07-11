using System;

namespace IronPython.Runtime.Types;

[Flags]
public enum FunctionType
{
	None = 0,
	Function = 1,
	Method = 2,
	FunctionMethodMask = 3,
	AlwaysVisible = 4,
	ReversedOperator = 0x20,
	BinaryOperator = 0x40,
	ModuleMethod = 0x80
}
