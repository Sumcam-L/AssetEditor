using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtConsumerContext : DomNodeAdapter, IArtConsumerEditingContext
{
	private ArtConsumerAdapter _adapter;

	private GameArtSpecificationContext _parentContext;

	public ArtConsumerAdapter Adapter
	{
		get
		{
			if (_adapter == null)
			{
				_adapter = base.DomNode.As<ArtConsumerAdapter>();
			}
			return _adapter;
		}
	}

	private GameArtSpecificationContext ParentContext
	{
		get
		{
			if (_parentContext == null)
			{
				_parentContext = base.DomNode.GetRoot().As<GameArtSpecificationContext>();
			}
			return _parentContext;
		}
	}

	public string[] LibraryNames => Adapter.ArtSpecification.GameLibraries.Select((IGameLibrary lib) => lib.LibraryName).ToArray();

	public string ConsumerName
	{
		get
		{
			return Adapter.ConsumerName;
		}
		set
		{
			if (Adapter.ConsumerName != value)
			{
				ParentContext.As<ITransactionContext>().DoTransaction(delegate
				{
					Adapter.ConsumerName = value;
				}, "Change consumer name.");
			}
		}
	}

	public bool LoadsLibraries
	{
		get
		{
			return Adapter.LoadsLibraries;
		}
		set
		{
			if (Adapter.LoadsLibraries != value)
			{
				ParentContext.As<ITransactionContext>().DoTransaction(delegate
				{
					Adapter.LoadsLibraries = value;
				}, "Change Loads Libraries.");
			}
		}
	}

	public IEnumerable<RelativePathAdapter> RelativePaths => Adapter.RelativeArtDefPaths.RelativePaths;

	public IEnumerable<LibraryReferenceAdapter> LibraryReferences => Adapter.LibraryReferences.LibraryReferences;

	DomNode IArtConsumerEditingContext.DomNode => base.DomNode;

	public bool IsValidName(string consumerName)
	{
		return Adapter.IsValidName(consumerName);
	}

	public void AddRelativeArtDefPath()
	{
		IEnumerable<string> paths = BrowseForArtDefFiles();
		if (paths.Any())
		{
			ParentContext.As<ITransactionContext>().DoTransaction(delegate
			{
				Adapter.RelativeArtDefPaths.AddRelativePath(paths);
			}, "Add relative paths.");
		}
	}

	private IEnumerable<string> BrowseForArtDefFiles()
	{
		string artDefRoot = ParentContext.ProjectMapService.PrimaryProject.Paths.ArtDefRoot;
		ParentContext.FileDialogService.ForcedInitialDirectory = artDefRoot;
		string[] pathNames = Enumerable.Empty<string>().ToArray();
		string filter = "Art Def Files (*.artdef)|*.artdef";
		ParentContext.FileDialogService.OpenFileNames(ref pathNames, filter);
		ParentContext.FileDialogService.ForcedInitialDirectory = null;
		List<string> list = new List<string>();
		string[] array = pathNames;
		foreach (string text in array)
		{
			string item = text;
			if (text.StartsWith(artDefRoot))
			{
				item = text.Replace(artDefRoot, "");
				item = item.Trim(Path.DirectorySeparatorChar);
			}
			list.Add(item);
		}
		return list;
	}

	public void RemoveRelativeArtDefPaths(IEnumerable<string> paths)
	{
		if (paths.Any())
		{
			ParentContext.As<ITransactionContext>().DoTransaction(delegate
			{
				Adapter.RelativeArtDefPaths.RemoveRelativePaths(paths);
			}, "Remove relative paths.");
		}
	}

	public void AddLibraryReferences(string libraryName)
	{
		ParentContext.As<ITransactionContext>().DoTransaction(delegate
		{
			List<string> libraryNames = new List<string> { libraryName };
			Adapter.LibraryReferences.AddLibraryReferences(libraryNames);
		}, "Add Library References");
	}

	public void RemoveLibraryReferences(IEnumerable<string> libraryReferences)
	{
		if (libraryReferences.Any())
		{
			ParentContext.As<ITransactionContext>().DoTransaction(delegate
			{
				Adapter.LibraryReferences.RemoveLibraryReferences(libraryReferences);
			}, "Remove Library References");
		}
	}
}
