using System;

namespace IronPython.Runtime;

[Flags]
public enum ModuleOptions
{
	None = 0,
	TrueDivision = 1,
	ShowClsMethods = 2,
	Optimized = 4,
	Initialize = 8,
	WithStatement = 0x10,
	AbsoluteImports = 0x20,
	NoBuiltins = 0x40,
	ModuleBuiltins = 0x80,
	ExecOrEvalCode = 0x100,
	SkipFirstLine = 0x200,
	PrintFunction = 0x400,
	Interpret = 0x1000,
	UnicodeLiterals = 0x2000,
	Verbatim = 0x4000,
	LightThrow = 0x8000
}
