using System.Collections.Generic;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LibraryContext : DomNodeAdapter, ILibraryEditingContext
{
	private GameLibraryAdapter _adapter;

	private GameArtSpecificationContext _parentContext;

	private bool _nameChanged;

	public GameLibraryAdapter Adapter
	{
		get
		{
			if (_adapter == null)
			{
				_adapter = base.DomNode.As<GameLibraryAdapter>();
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

	public string LibraryName
	{
		get
		{
			return Adapter.LibraryName;
		}
		set
		{
			if (Adapter.LibraryName != value)
			{
				ParentContext.As<ITransactionContext>().DoTransaction(delegate
				{
					Adapter.LibraryName = value;
				}, "Change library name.");
				_nameChanged = true;
			}
		}
	}

	public IEnumerable<RelativePathAdapter> RelativePaths => Adapter.RelativePackagePaths.RelativePaths;

	public bool IsNewLibrary
	{
		get
		{
			if (!_nameChanged)
			{
				return !Adapter.HasBeenNamed();
			}
			return false;
		}
	}

	DomNode ILibraryEditingContext.DomNode => base.DomNode;

	public IEnumerable<string> GetReferencingConsumerNames()
	{
		return Adapter.GetReferencingConsumerNames();
	}

	public bool IsValidName(string libraryName)
	{
		return Adapter.IsValidName(libraryName);
	}

	public void AddRelativePath()
	{
		IEnumerable<string> paths = BrowseForPackageFiles();
		if (paths.Any())
		{
			ParentContext.As<ITransactionContext>().DoTransaction(delegate
			{
				Adapter.RelativePackagePaths.AddRelativePath(paths);
			}, "Add package paths.");
		}
	}

	private IEnumerable<string> BrowseForPackageFiles()
	{
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		string xLPRoot = ParentContext.ProjectMapService.PrimaryProject.Paths.XLPRoot;
		ParentContext.FileDialogService.ForcedInitialDirectory = xLPRoot;
		string[] pathNames = Enumerable.Empty<string>().ToArray();
		string filter = "XLP Files (*.xlp)|*.xlp";
		ParentContext.FileDialogService.OpenFileNames(ref pathNames, filter);
		ParentContext.FileDialogService.ForcedInitialDirectory = null;
		ICollection<string> collection = new List<string>();
		string[] array = pathNames;
		foreach (string text in array)
		{
			if (!text.StartsWith(xLPRoot))
			{
				continue;
			}
			using IXLP iXLP = civTechContext.CreateInstance<IXLP>();
			if ((bool)iXLP.DeserializeFromFile(text))
			{
				collection.Add(iXLP.Package);
			}
		}
		return collection;
	}

	public void RemoveRelativePaths(IEnumerable<string> paths)
	{
		if (paths.Any())
		{
			ParentContext.As<ITransactionContext>().DoTransaction(delegate
			{
				Adapter.RelativePackagePaths.RemoveRelativePaths(paths);
			}, "Remove package paths.");
		}
	}
}
