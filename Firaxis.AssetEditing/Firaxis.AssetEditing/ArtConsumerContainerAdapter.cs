using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtConsumerContainerAdapter : DomNodeAdapter
{
	private IList<ArtConsumerAdapter> m_artConsumers;

	public IList<ArtConsumerAdapter> ArtConsumers => m_artConsumers;

	public IGameArtSpecification ArtSpecification { get; set; }

	protected override void OnNodeSet()
	{
		m_artConsumers = new DomNodeListAdapter<ArtConsumerAdapter>(base.DomNode, GameArtSpecificationSchema.ArtConsumerContainerType.ArtConsumersChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	public void Initialize()
	{
		UnregisterFromDomChanges();
		foreach (IArtConsumer artConsumer in ArtSpecification.ArtConsumers)
		{
			ArtConsumerAdapter item = ArtConsumerAdapter.Create(ArtSpecification, artConsumer);
			ArtConsumers.Add(item);
		}
		RegisterForDomChanges();
	}

	public void AddConsumer(string consumerName)
	{
		UnregisterFromDomChanges();
		ArtConsumerAdapter item = ArtConsumerAdapter.Create(ArtSpecification, consumerName);
		ArtConsumers.Add(item);
		RegisterForDomChanges();
	}

	public void RemoveConsumer(string consumerName)
	{
		for (int i = 0; i < ArtConsumers.Count; i++)
		{
			if (ArtConsumers[i].ConsumerName == consumerName)
			{
				ArtConsumers.RemoveAt(i);
				break;
			}
		}
	}

	public string GenerateNewConsumerName()
	{
		int num = 1;
		string consumerName;
		string arg = (consumerName = "NewConsumer");
		while (ArtConsumers.FirstOrDefault((ArtConsumerAdapter c) => c.ConsumerName == consumerName) != null)
		{
			consumerName = $"{arg}_{num++:D3}";
		}
		return consumerName;
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.ArtConsumerContainerType.ArtConsumersChild)
		{
			ArtConsumerAdapter artConsumerAdapter = e.Child.As<ArtConsumerAdapter>();
			if (artConsumerAdapter != null)
			{
				artConsumerAdapter.ArtConsumer = ArtSpecification.AddConsumer(artConsumerAdapter.ConsumerName);
			}
		}
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.ArtConsumerContainerType.ArtConsumersChild)
		{
			ArtConsumerAdapter artConsumerAdapter = e.Child.As<ArtConsumerAdapter>();
			if (artConsumerAdapter != null)
			{
				ArtSpecification.RemoveConsumer(artConsumerAdapter.ConsumerName);
			}
		}
	}
}
