using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LibraryReferenceContainerAdapter : DomNodeAdapter
{
	private IList<LibraryReferenceAdapter> m_libraryReferences;

	private IArtConsumer _artConsumer;

	public IList<LibraryReferenceAdapter> LibraryReferences => m_libraryReferences;

	public IArtConsumer ArtConsumer
	{
		get
		{
			return _artConsumer;
		}
		set
		{
			if (_artConsumer == value)
			{
				return;
			}
			UnregisterFromDomChanges();
			_artConsumer = value;
			LibraryReferences.Clear();
			foreach (string referencedLibrary in value.ReferencedLibraries)
			{
				LibraryReferenceAdapter item = LibraryReferenceAdapter.Create(referencedLibrary);
				LibraryReferences.Add(item);
			}
			RegisterForDomChanges();
		}
	}

	protected override void OnNodeSet()
	{
		ChildInfo libraryReferencesChild = GameArtSpecificationSchema.LibraryReferenceContainerType.LibraryReferencesChild;
		m_libraryReferences = new DomNodeListAdapter<LibraryReferenceAdapter>(base.DomNode, libraryReferencesChild);
		RegisterForDomChanges();
		base.OnNodeSet();
	}

	public void AddLibraryReferences(IEnumerable<string> libraryNames)
	{
		foreach (string libraryName in libraryNames)
		{
			LibraryReferenceAdapter item = LibraryReferenceAdapter.Create(libraryName);
			LibraryReferences.Add(item);
		}
	}

	public void RemoveLibraryReferences(IEnumerable<string> libraryNames)
	{
		foreach (string libraryName in libraryNames)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = LibraryReferences.FirstOrDefault((LibraryReferenceAdapter adp) => adp.LibraryName == libraryName);
			if (libraryReferenceAdapter != null)
			{
				LibraryReferences.Remove(libraryReferenceAdapter);
			}
		}
	}

	protected virtual void RegisterForDomChanges()
	{
		base.DomNode.AttributeChanged += HandleChildAttributeChanged;
		base.DomNode.ChildInserted += HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved += HandleDomNodeChildRemoved;
	}

	protected virtual void UnregisterFromDomChanges()
	{
		base.DomNode.AttributeChanged -= HandleChildAttributeChanged;
		base.DomNode.ChildInserted -= HandleDomNodeChildInserted;
		base.DomNode.ChildRemoved -= HandleDomNodeChildRemoved;
	}

	protected virtual void HandleDomNodeChildInserted(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.LibraryReferenceContainerType.LibraryReferencesChild)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = e.Child.As<LibraryReferenceAdapter>();
			if (libraryReferenceAdapter != null)
			{
				ArtConsumer.AddLibrary(libraryReferenceAdapter.LibraryName);
			}
		}
	}

	protected virtual void HandleDomNodeChildRemoved(object sender, ChildEventArgs e)
	{
		if (e.ChildInfo == GameArtSpecificationSchema.LibraryReferenceContainerType.LibraryReferencesChild)
		{
			LibraryReferenceAdapter libraryReferenceAdapter = e.Child.As<LibraryReferenceAdapter>();
			if (libraryReferenceAdapter != null)
			{
				ArtConsumer.RemoveLibrary(libraryReferenceAdapter.LibraryName);
			}
		}
	}

	protected virtual void HandleChildAttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == GameArtSpecificationSchema.RelativePathType.RelativePathAttribute)
		{
			string libraryName = e.OldValue.ToString();
			string libraryName2 = e.NewValue.ToString();
			ArtConsumer.AddLibrary(libraryName2);
			ArtConsumer.RemoveLibrary(libraryName);
		}
	}
}
