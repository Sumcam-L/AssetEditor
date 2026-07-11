using System;

namespace Firaxis.CivTech.AssetObjects;

public interface IGameArtID : IEquatable<IGameArtID>
{
	string Name { get; set; }

	string ID { get; set; }
}
