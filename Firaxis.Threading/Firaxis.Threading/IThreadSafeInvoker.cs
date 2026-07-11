using System;

namespace Firaxis.Threading;

public interface IThreadSafeInvoker
{
	bool Invoke(Delegate method);

	bool Invoke(Delegate method, params object[] args);
}
