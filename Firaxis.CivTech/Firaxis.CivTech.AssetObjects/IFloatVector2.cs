using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IFloatVector2 : IAssemblyInstance, IDisposable
{
	float X { get; set; }

	float Y { get; set; }

	bool IsEqualTo(IList<float> values);
}
