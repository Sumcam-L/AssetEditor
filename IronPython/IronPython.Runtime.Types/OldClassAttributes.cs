using System;

namespace IronPython.Runtime.Types;

[Flags]
internal enum OldClassAttributes
{
	None = 0,
	HasFinalizer = 1,
	HasSetAttr = 2,
	HasDelAttr = 4
}
