using System;
using System.Collections.Generic;

namespace Firaxis.CivTech.AssetObjects;

public interface IFloatVector3 : IAssemblyInstance, IDisposable
{
	float X { get; set; }

	float Y { get; set; }

	float Z { get; set; }

	bool IsEqualTo(IList<float> values);
}
