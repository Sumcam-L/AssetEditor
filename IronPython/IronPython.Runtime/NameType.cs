using System;

namespace IronPython.Runtime;

[Flags]
public enum NameType
{
	None = 0,
	Python = 1,
	Method = 2,
	Field = 4,
	Property = 8,
	Event = 0x10,
	Type = 0x20,
	BaseTypeMask = 0x3E,
	PythonMethod = 3,
	PythonField = 5,
	PythonProperty = 9,
	PythonEvent = 0x11,
	PythonType = 0x21,
	ClassMember = 0x40,
	ClassMethod = 0x43
}
