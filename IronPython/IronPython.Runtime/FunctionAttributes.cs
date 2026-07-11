using System;

namespace IronPython.Runtime;

[Flags]
public enum FunctionAttributes
{
	None = 0,
	ArgumentList = 4,
	KeywordDictionary = 8,
	Generator = 0x20,
	FutureDivision = 0x2000,
	CanSetSysExcInfo = 0x4000,
	ContainsTryFinally = 0x8000
}
