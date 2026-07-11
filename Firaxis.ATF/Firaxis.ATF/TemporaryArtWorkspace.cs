using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.Packages;
using Firaxis.Utility;

namespace Firaxis.ATF;

public class TemporaryArtWorkspace : ITemporaryArtOutputPaths, IDisposable
{
	private bool disposedValue;

	public string ArtDefOutputDirectory => WorkspacePaths.ArtDefOutputDirectory;

	public string BLPOutputDirectory => WorkspacePaths.BLPOutputDirectory;

	public string TemporaryCookLocation
	{
		get
		{
			return WorkspacePaths.TemporaryCookLocation;
		}
		set
		{
			WorkspacePaths.TemporaryCookLocation = value;
		}
	}

	public string TemporaryPantryDirectory
	{
		get
		{
			return WorkspacePaths.TemporaryPantryDirectory;
		}
		set
		{
			WorkspacePaths.TemporaryPantryDirectory = value;
		}
	}

	public string TemporaryXLPPantry => WorkspacePaths.TemporaryXLPPantry;

	public string TemporaryArtSpecLocation => WorkspacePaths.TemporaryArtSpecLocation;

	private ProjectEnvironment HostProject { get; }

	private TemporaryArtWorkspacePaths WorkspacePaths { get; }

	public IGameArtSpecification GameArtSpecification { get; }

	public Dictionary<string, IXLP> TemporaryXLPDictionary { get; } = new Dictionary<string, IXLP>();

	public TemporaryArtWorkspace(ProjectEnvironment project, string temporaryCookLocation, string temporaryPantryRoot)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		HostProject = project;
		WorkspacePaths = new TemporaryArtWorkspacePaths(project, temporaryCookLocation, temporaryPantryRoot);
		GameArtSpecification = Context.EnsureCreated<CivTechContext>().CreateInstance<IGameArtSpecification>();
		GameArtSpecification.ID.Name = "temporary_workspace_" + project.Name;
		GameArtSpecification.ID.ID = Guid.NewGuid().ToString().ToUpper();
		IGameArtID iD = project.PrimaryArtSpecification.ID;
		GameArtSpecification.AddRequiredGameArtID(iD.Name, iD.ID);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (disposedValue)
		{
			return;
		}
		if (disposing)
		{
			GameArtSpecification.Dispose();
			foreach (IXLP value in TemporaryXLPDictionary.Values)
			{
				value.Dispose();
			}
			TemporaryXLPDictionary.Clear();
			WorkspacePaths.Dispose();
		}
		disposedValue = true;
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
