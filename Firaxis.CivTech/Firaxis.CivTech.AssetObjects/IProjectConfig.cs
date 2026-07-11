using System;
using Firaxis.CivTech.Packages;

namespace Firaxis.CivTech.AssetObjects;

public interface IProjectConfig : ISerializable, IAssemblyInstance, IDisposable, IVersionedData
{
	string Name { get; set; }

	IClassSet Classes { get; }

	IArtDefTemplateSet ArtDefTemplates { get; }

	IXLPClassSet XLPClasses { get; }
}
