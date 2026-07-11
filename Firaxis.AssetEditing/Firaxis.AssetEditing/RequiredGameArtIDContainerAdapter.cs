using System.Collections.Generic;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class RequiredGameArtIDContainerAdapter : DomNodeAdapter
{
	private IList<GameArtIDAdapter> m_gameArtIDs;

	public IList<GameArtIDAdapter> GameArtIDs => m_gameArtIDs;

	public IGameArtSpecification ArtSpecification { get; set; }

	private GameArtSpecificationDocument Document => base.DomNode.GetRoot().As<GameArtSpecificationDocument>();

	private TransactionContext Context => base.DomNode.GetRoot().As<TransactionContext>();

	protected override void OnNodeSet()
	{
		m_gameArtIDs = new DomNodeListAdapter<GameArtIDAdapter>(base.DomNode, GameArtSpecificationSchema.RequiredGameArtIDContainerType.RequiredGameArtIDsChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	public void Initialize()
	{
		UnregisterFromDomChanges();
		foreach (IGameArtID requiredGameArtID in ArtSpecification.RequiredGameArtIDs)
		{
			GameArtIDAdapter item = GameArtIDAdapter.Create(requiredGameArtID);
			GameArtIDs.Add(item);
		}
		RegisterForDomChanges();
	}

	public void AddRequiredGameArtID(string gameName, string ID)
	{
		GameArtIDAdapter item = GameArtIDAdapter.Create(gameName, ID);
		GameArtIDs.Add(item);
	}

	public void RemoveRequiredGameArtID(string ID)
	{
		for (int i = 0; i < GameArtIDs.Count; i++)
		{
			if (GameArtIDs[i].GameArtID == ID)
			{
				GameArtIDs.RemoveAt(i);
				break;
			}
		}
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
		GameArtIDAdapter gameArtIDAdapter = e.Child.As<GameArtIDAdapter>();
		if (gameArtIDAdapter != null)
		{
			ArtSpecification.AddRequiredGameArtID(gameArtIDAdapter.GameName, gameArtIDAdapter.GameArtID);
		}
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		GameArtIDAdapter gameArtIDAdapter = e.Child.As<GameArtIDAdapter>();
		if (gameArtIDAdapter != null)
		{
			ArtSpecification.RemoveRequiredGameArtID(gameArtIDAdapter.GameArtID);
		}
	}
}
