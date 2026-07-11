using System;

namespace IronPython.Runtime.Binding;

[Flags]
internal enum GetMemberOptions
{
	None = 0,
	IsNoThrow = 1,
	IsCaseInsensitive = 2
}
