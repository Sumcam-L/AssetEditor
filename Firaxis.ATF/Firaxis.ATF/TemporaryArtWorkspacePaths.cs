using System;
using System.IO;
using Firaxis.CivTech;

namespace Firaxis.ATF;

public class TemporaryArtWorkspacePaths : ITemporaryArtOutputPaths, IDisposable
{
	private string _temporaryCookLocation = string.Empty;

	private string _temporaryPantryDirectory = string.Empty;

	private string _temporaryXLPPantry = string.Empty;

	private bool disposedValue;

	public string TemporaryCookLocation
	{
		get
		{
			return _temporaryCookLocation;
		}
		set
		{
			value = ((value != null) ? Path.Combine(value, HostProject.Name) : string.Empty);
			if (!_temporaryCookLocation.Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				RemoveDirectory(_temporaryCookLocation);
				_temporaryCookLocation = value;
				RecreateDirectory(TemporaryCookLocation);
				RecreateDirectory(ArtDefOutputDirectory);
				RecreateDirectory(BLPOutputDirectory);
			}
		}
	}

	public string TemporaryPantryDirectory
	{
		get
		{
			return _temporaryPantryDirectory;
		}
		set
		{
			value = ((value != null) ? Path.Combine(value, HostProject.Name) : string.Empty);
			if (!_temporaryPantryDirectory.Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				RemoveDirectory(_temporaryPantryDirectory);
				_temporaryPantryDirectory = value;
				RecreateDirectory(TemporaryPantryDirectory);
				ProjectPaths paths = HostProject.Paths;
				string path = paths.XLPRoot.Replace(paths.GamePantry, "").TrimStart(Path.DirectorySeparatorChar);
				TemporaryXLPPantry = Path.Combine(TemporaryPantryDirectory, path);
				TemporaryArtSpecLocation = Path.Combine(TemporaryPantryDirectory, HostProject.Name + "_temp.Art.xml");
			}
		}
	}

	public string ArtDefOutputDirectory => Path.Combine(TemporaryCookLocation, "ArtDefs");

	public string BLPOutputDirectory => Path.Combine(TemporaryCookLocation, "Platforms/{PLATFORM}/BLPs");

	public string TemporaryXLPPantry
	{
		get
		{
			return _temporaryXLPPantry;
		}
		private set
		{
			if (!_temporaryXLPPantry.Equals(value, StringComparison.CurrentCultureIgnoreCase))
			{
				RemoveDirectory(_temporaryXLPPantry);
				_temporaryXLPPantry = value;
				RecreateDirectory(TemporaryXLPPantry);
			}
		}
	}

	public string TemporaryArtSpecLocation { get; private set; }

	private ProjectEnvironment HostProject { get; }

	public TemporaryArtWorkspacePaths(ProjectEnvironment project, string temporaryCookLocation, string temporaryPantryRoot)
	{
		if (project == null)
		{
			throw new ArgumentNullException("project");
		}
		HostProject = project;
		TemporaryCookLocation = temporaryCookLocation;
		TemporaryPantryDirectory = temporaryPantryRoot;
	}

	private void RecreateDirectory(string directoryPath)
	{
		RemoveDirectory(directoryPath);
		CreateDirectory(directoryPath);
		BugSubmitter.Assert(Directory.Exists(directoryPath), "Directory " + directoryPath + " does not exist after the call to recreate directory.  @summary temp_create_directory_failed  @assign dgurley");
	}

	private void RemoveDirectory(string directoryPath)
	{
		if (Directory.Exists(directoryPath))
		{
			try
			{
				Directory.Delete(directoryPath, recursive: true);
			}
			catch (System.Exception exObj)
			{
				BugSubmitter.SilentException(exObj);
			}
		}
		if (!Directory.Exists(directoryPath))
		{
			return;
		}
		string[] files = Directory.GetFiles(directoryPath, "*.*", SearchOption.AllDirectories);
		foreach (string path in files)
		{
			try
			{
				File.Delete(path);
			}
			catch (System.Exception exObj2)
			{
				BugSubmitter.SilentException(exObj2);
			}
		}
	}

	private bool CreateDirectory(string directoryPath)
	{
		if (!Directory.Exists(directoryPath))
		{
			try
			{
				Directory.CreateDirectory(directoryPath);
				return true;
			}
			catch (System.Exception exObj)
			{
				BugSubmitter.SilentException(exObj);
				return false;
			}
		}
		return true;
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!disposedValue)
		{
			if (disposing)
			{
				RemoveDirectory(TemporaryPantryDirectory);
				RemoveDirectory(TemporaryCookLocation);
			}
			disposedValue = true;
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}
}
