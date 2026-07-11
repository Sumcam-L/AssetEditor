using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class GameLibraryContainerAdapter : DomNodeAdapter
{
	private IList<GameLibraryAdapter> m_gameLibraries;

	public IList<GameLibraryAdapter> GameLibraries => m_gameLibraries;

	public IGameArtSpecification ArtSpecification { get; set; }

	protected override void OnNodeSet()
	{
		m_gameLibraries = new DomNodeListAdapter<GameLibraryAdapter>(base.DomNode, GameArtSpecificationSchema.GameLibraryContainerType.GameLibrariesChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	public void Initialize()
	{
		UnregisterFromDomChanges();
		foreach (IGameLibrary gameLibrary in ArtSpecification.GameLibraries)
		{
			GameLibraryAdapter item = GameLibraryAdapter.Create(gameLibrary);
			GameLibraries.Add(item);
		}
		RegisterForDomChanges();
	}

	public void AddLibraryReference(string libraryName)
	{
		UnregisterFromDomChanges();
		GameLibraryAdapter item = GameLibraryAdapter.Create(ArtSpecification, libraryName);
		GameLibraries.Add(item);
		RegisterForDomChanges();
	}

	public void RemoveLibraryReference(string libraryName)
	{
		for (int i = 0; i < GameLibraries.Count; i++)
		{
			if (GameLibraries[i].LibraryName == libraryName)
			{
				GameLibraries.RemoveAt(i);
				break;
			}
		}
	}

	public string GenerateNewLibraryName()
	{
		int num = 1;
		string name;
		string arg = (name = "NewLibrary");
		while (GameLibraries.Any((GameLibraryAdapter gl) => gl.LibraryName == name))
		{
			name = $"{arg}_{num:D3}";
			num++;
		}
		return name;
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
		if (e.ChildInfo == GameArtSpecificationSchema.GameLibraryContainerType.GameLibrariesChild)
		{
			GameLibraryAdapter gameLibraryAdapter = e.Child.As<GameLibraryAdapter>();
			if (gameLibraryAdapter != null)
			{
				gameLibraryAdapter.GameLibrary = ArtSpecification.AddGameLibrary(gameLibraryAdapter.LibraryName);
			}
		}
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.GameLibraryContainerType.GameLibrariesChild)
		{
			GameLibraryAdapter gameLibraryAdapter = e.Child.As<GameLibraryAdapter>();
			if (gameLibraryAdapter != null)
			{
				ArtSpecification.RemoveGameLibrary(gameLibraryAdapter.LibraryName);
			}
		}
	}
}
