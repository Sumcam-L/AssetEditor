using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.ContentExporters.Implementations;

public class EnvironmentLightExporter : IContentExporter
{
	private List<string> _sourceObjectNames;

	private List<InstanceType> _supportedInstanceTypes;

	private IEnvironmentMapImporter FileExporter { get; set; }

	private IEnumerable<string> SourceObjectNames
	{
		get
		{
			if (_sourceObjectNames == null)
			{
				_sourceObjectNames = new List<string>();
				_sourceObjectNames.Add(string.Empty);
			}
			return _sourceObjectNames;
		}
	}

	public IEnumerable<string> SupportedFileTypes => FileExporter.SupportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public EnvironmentLightExporter()
	{
		FileExporter = Context.EnsureCreated<CivTechContext>().CreateInstance<IEnvironmentMapImporter>();
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_ENVIRONMENT_LIGHT);
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity entityClass)
	{
		DateTime now = DateTime.Now;
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		IEnvironmentLightInstance environmentLightInstance = entity.Entity as IEnvironmentLightInstance;
		IEnvironmentLightClass environmentLightClass = entityClass as IEnvironmentLightClass;
		if (environmentLightInstance == null)
		{
			entity.Result = new ResultCode("The Loose Texture Exporter can only operate on textures.  Entity {0} has the type {1}.", entity.Entity.Name, entity.Entity.Type.ToString());
			return;
		}
		if (string.IsNullOrEmpty(environmentLightInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) does not have a source file path set.", environmentLightInstance.Name);
			return;
		}
		if (!File.Exists(environmentLightInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) has a source file path set that does not exist on disk.  Invalid path: ({1})", environmentLightInstance.Name, environmentLightInstance.SourceFilePath);
			return;
		}
		string text = StaticMethods.PantryRootForInstanceType(project.Paths.GamePantry, environmentLightInstance.Type);
		foreach (IClassDataFile dataFile in entityClass.DataFiles)
		{
			text = Path.Combine(text, environmentLightInstance.Name + dataFile.Extension);
			text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			bool flag = false;
			string text2 = string.Empty;
			try
			{
				IEnvironmentSource environmentSource = FileExporter.OpenSourceFile(environmentLightInstance.SourceFilePath);
				EnvironmentMapParameterization eSourceParametrization = (environmentSource.IsCubeMap ? EnvironmentMapParameterization.ENVMAP_CUBE : EnvironmentMapParameterization.ENVMAP_LATLONG);
				ICubeMap cubeMap = FileExporter.CreateCube(environmentSource, eSourceParametrization, environmentLightClass.ImportOptions.MaxWidth, environmentLightClass.ImportOptions, useIdentityBasis: false, environmentLightClass.ImportOptions.MaxWidth);
				cubeMap.SaveDDS(text, environmentLightClass.ImportOptions.PixelFormat);
			}
			catch (Exception ex)
			{
				flag = true;
				text2 = ex.Message;
			}
			if (!flag)
			{
				if (File.Exists(text))
				{
					FileInfo fileInfo = new FileInfo(text);
					if (fileInfo.LastWriteTimeUtc >= now.ToUniversalTime())
					{
						ProcessExportedEntity(entity.Entity, entityClass, project.Paths.GamePantry);
					}
					else
					{
						flag = true;
						text2 = $"The file was not written to disk.  Expected last write time: ({now.ToLocalTime().ToShortTimeString()}).  Actual last write time: ({fileInfo.LastWriteTime.ToLocalTime().ToShortTimeString()})";
					}
				}
				else
				{
					flag = true;
					text2 = $"The file was not written to disk and does not exist.  File Path: ({text})";
				}
			}
			if (flag)
			{
				entity.Result = new ResultCode("Export failed for entity ({0}).  Reason: {1}", environmentLightInstance.Name, text2);
			}
		}
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode success = ResultCode.Success;
		IEnvironmentLightInstance environmentLightInstance = instanceEntity as IEnvironmentLightInstance;
		environmentLightInstance.PopulateDataFiles(classEntity);
		environmentLightInstance.UpdateExportedTime();
		return success;
	}

	public void Export(ICivTechService civTechSvc, IEnumerable<Tuple<ImportOperationResult, IClassEntity>> entities)
	{
		foreach (Tuple<ImportOperationResult, IClassEntity> entity in entities)
		{
			Export(civTechSvc, entity.Item1, entity.Item2);
		}
	}

	public ResultCode ValidateClass(IImportedEntity entity, IClassEntity entityClass)
	{
		return ExportUtilityFunctions.ValidateClass(entity, entityClass);
	}

	public ResultCode Validate(IImportedEntity entity, IClassEntity entityClass, string localPantry)
	{
		return ExportUtilityFunctions.Validate(entity, entityClass, localPantry);
	}

	public ResultCode RebuildExportedEntity(ICivTechService civTechSvc, IImportedEntity entity, IClassEntity entityClass)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity, ref project);
		if (!entityProject)
		{
			return entityProject;
		}
		return ProcessExportedEntity(entity, entityClass, project.Paths.GamePantry);
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		return SourceObjectNames;
	}
}
