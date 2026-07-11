using System;
using Firaxis.Controls;

namespace Firaxis.AssetEditing;

public interface ITriggerAdapterKey : IKey, IEquatable<IKey>
{
	TriggerAdapter Adapter { get; }
}
