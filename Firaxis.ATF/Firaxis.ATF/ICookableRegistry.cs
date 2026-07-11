using System;
using System.Collections.Generic;

namespace Firaxis.ATF;

public interface ICookableRegistry
{
	IEnumerable<Uri> Cookables { get; }

	void EnableCooking(Uri cookableUri, bool enabled);

	bool IsCookingEnabled(Uri cookableUri);
}
