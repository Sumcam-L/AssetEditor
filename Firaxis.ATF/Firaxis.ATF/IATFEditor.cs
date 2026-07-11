using System;

namespace Firaxis.ATF;

public interface IATFEditor : IDisposable
{
	void Bind(IATFEditingContext context);
}
