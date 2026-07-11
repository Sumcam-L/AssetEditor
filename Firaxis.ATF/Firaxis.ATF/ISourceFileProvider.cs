using System;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.ATF;

public interface ISourceFileProvider
{
	EntityID ReferencingEntity { get; }

	Uri SourceFileUri { get; }
}
