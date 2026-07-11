using System;
using System.Collections.Generic;

namespace IronPython.Modules;

internal static class HKeyHandleCache
{
	internal static Dictionary<int, WeakReference> cache = new Dictionary<int, WeakReference>();
}
