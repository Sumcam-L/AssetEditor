using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech.AssetPreviewer;

public interface ICachedAsset : IDisposable
{
	IInstanceEntity InstanceEntity { get; }

	string XMLText { get; }
}
