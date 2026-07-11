using System;
using System.Collections.Generic;
using System.IO;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.TextureExport;
using Firaxis.Error;
using Firaxis.Utility;

namespace Firaxis.ContentExporters.Implementations;

public class LooseTextureExporter : IContentExporter
{
	private List<string> _supportedFileTypes;

	private List<InstanceType> _supportedInstanceTypes;

	private ILooseFileExporter FileExporter { get; set; }

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public LooseTextureExporter()
	{
		Context.EnsureCreated<CivTechContext>();
		FileExporter = Context.Get<CivTechContext>().CreateInstance<ILooseFileExporter>();
		_supportedFileTypes = new List<string>();
		_supportedFileTypes.Add(".tga");
		_supportedFileTypes.Add(".png");
		_supportedFileTypes.Add(".dds");
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_TEXTURE);
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		if (entityType != InstanceType.IT_TEXTURE)
		{
			return null;
		}
		List<string> list = new List<string>();
		list.Add(string.Empty);
		return list;
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity classEntity)
	{
		DateTime now = DateTime.Now;
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		if (!(entity.Entity is ITextureInstance textureInstance))
		{
			entity.Result = new ResultCode("The Loose Texture Exporter can only operate on textures.  Entity {0} has the type {1}.", entity.Entity.Name, entity.Entity.Type.ToString());
			return;
		}
		if (string.IsNullOrEmpty(textureInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) does not have a source file path set.", textureInstance.Name);
			return;
		}
		if (!File.Exists(textureInstance.SourceFilePath))
		{
			entity.Result = new ResultCode("Texture ({0}) has a source file path set that does not exist on disk.  Invalid path: ({1})", textureInstance.Name, textureInstance.SourceFilePath);
			return;
		}
		string text = StaticMethods.PantryRootForInstanceType(project.Paths.GamePantry, textureInstance.Type);
		foreach (IClassDataFile dataFile in classEntity.DataFiles)
		{
			text = Path.Combine(text, textureInstance.Name + dataFile.Extension);
			text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			if (FileExporter.ExportAsDDS(textureInstance.SourceFilePath, text, textureInstance.ExportSettings, out var errorMessage) && File.Exists(text))
			{
				entity.Result = ProcessExportedEntity(textureInstance, classEntity, project.Paths.GamePantry);
				continue;
			}
			entity.Result = new ResultCode("DDS Export failed for entity ({0}).  Reason: {1}", textureInstance.Name, errorMessage);
		}
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string pantryPath)
	{
		ResultCode success = ResultCode.Success;
		instanceEntity.PopulateDataFiles(classEntity);
		ResultCode resultCode = ExportUtilityFunctions.ParseDDSFiles(instanceEntity as ITextureInstance, pantryPath);
		instanceEntity.UpdateExportedTime();
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
		return ResultCode.Success;
	}
}
