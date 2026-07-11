using System;

namespace Firaxis.Controls;

public interface IKey : IEquatable<IKey>
{
	float Time { get; set; }

	float Value { get; set; }
}
