using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech.AssetObjects;

namespace Firaxis.CivTech;

public class ProjectEnvironment : IDisposable
{
	private IWorkspaceDependencyRegistry _dependencyRegistry;

	public string ActiveConfigPath
	{
		get
		{
			if (Settings.ModTools)
			{
				return Settings.ActiveConfigPath;
			}
			return Settings.UseLocalConfig ? Settings.ProjectConfigLocal : Path.Combine(VersionControl.WorkspaceRoot, Settings.ActiveConfigPath);
		}
	}

	public IList<string> Dependencies { get; private set; }

	public ProjectInfo Info { get; private set; }

	public string Name => Info.Name;

	public IGameArtSpecification PrimaryArtSpecification { get; private set; }

	public IEnumerable<IGameArtSpecification> ArtSpecifications { get; private set; }

	public IProjectConfig Config { get; set; }

	public IWorkspaceDependencyRegistry DependencyRegistry
	{
		get
		{
			return _dependencyRegistry;
		}
		set
		{
			BugSubmitter.SilentAssert(_dependencyRegistry == null || value == null, "Setting the Project Environment Dependency Registry after it has already been set!  @assign bwhitman");
			_dependencyRegistry = value;
		}
	}

	public IVersionControlService VersionControl { get; private set; }

	public IAssetCloudSettings Settings { get; private set; }

	public ProjectPaths Paths { get; private set; }

	public ProjectEnvironment(ProjectInfo info, IEnumerable<IGameArtSpecification> artSpecifications, IGameArtSpecification primaryArtSpec, IProjectConfig prjCfg, IWorkspaceDependencyRegistry workDepReg, IVersionControlService vcs, IAssetCloudSettings acs, ProjectPaths pp)
	{
		Dependencies = new List<string>();
		Info = info;
		ArtSpecifications = artSpecifications;
		Config = prjCfg;
		DependencyRegistry = workDepReg;
		VersionControl = vcs;
		Settings = acs;
		Paths = pp;
		PrimaryArtSpecification = primaryArtSpec;
	}

	public void Dispose()
	{
		(_dependencyRegistry as IWorkspaceDependencyWatcher)?.Dispose();
		_dependencyRegistry = null;
	}
}
