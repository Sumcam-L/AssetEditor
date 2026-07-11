using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.AssetEditing;

public class ArtDefPathHost : IRelativePathHost
{
	private readonly IArtConsumer _consumer;

	public string Name => _consumer.ConsumerName;

	public HostType Type => HostType.ArtConsumer;

	public IEnumerable<string> RelativePaths => _consumer.RelativeArtDefPaths;

	public ArtDefPathHost(IArtConsumer consumer)
	{
		_consumer = consumer;
	}

	public void AddRelativePath(string path)
	{
		_consumer.AddArtDefPath(path);
	}

	public void RemoveRelativePath(string path)
	{
		_consumer.RemoveArtDefPath(path);
	}
}
