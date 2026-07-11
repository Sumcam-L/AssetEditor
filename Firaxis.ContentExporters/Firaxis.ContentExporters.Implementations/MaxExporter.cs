using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.Granny;
using Firaxis.IO;
using Firaxis.MaxInterface;
using Firaxis.Utility;

namespace Firaxis.ContentExporters.Implementations;

public class MaxExporter : IContentExporter, IContentCreationTool
{
	private List<string> _supportedFileTypes;

	private List<InstanceType> _supportedInstanceTypes;

	private Autodesk_3DSMaxInterface MaxInterface { get; set; }

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public MaxExporter()
	{
		MaxInterface = new Autodesk_3DSMaxInterface();
		Context.EnsureCreated<GrannyContext>();
		_supportedFileTypes = new List<string>();
		_supportedFileTypes.Add(".max");
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_GEOMETRY);
		_supportedInstanceTypes.Add(InstanceType.IT_ANIMATION);
		_supportedInstanceTypes.Add(InstanceType.IT_ANALYTIC_LIGHT);
	}

	public void OpenFile(string filePath)
	{
		if (MaxInterface.EstablishConnection())
		{
			WindowsPath imageFilename = new WindowsPath(filePath);
			MaxInterface.OpenFile(imageFilename);
		}
	}

	public void RenderSourceObjectFromFile(string filePath, string sourceObject, string outputFolder)
	{
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		if (!SupportedInstanceTypes.Contains(entityType))
		{
			return Enumerable.Empty<string>();
		}
		if (string.IsNullOrEmpty(filePath) || !File.Exists(filePath))
		{
			return Enumerable.Empty<string>();
		}
		if (!MaxInterface.EstablishConnection())
		{
			return Enumerable.Empty<string>();
		}
		IEnumerable<string> result = Enumerable.Empty<string>();
		string[] sourceObjectNamesFromMax = GetSourceObjectNamesFromMax(filePath, entityType);
		if (!SourceObjectsEmpty(sourceObjectNamesFromMax))
		{
			TrimObjectNames(sourceObjectNamesFromMax);
			result = sourceObjectNamesFromMax;
		}
		return result;
	}

	private string[] GetSourceObjectNamesFromMax(string filePath, InstanceType entityType)
	{
		string text = null;
		text = ((entityType != InstanceType.IT_ANIMATION) ? MaxInterface.GetModelNames("Civ6", new WindowsPath(filePath)) : MaxInterface.GetAnimationNames("Civ6", new WindowsPath(filePath)));
		return text.Split(',');
	}

	private void TrimObjectNames(string[] sourceObjectNames)
	{
		for (int i = 0; i < sourceObjectNames.Length; i++)
		{
			sourceObjectNames[i] = sourceObjectNames[i].Trim();
		}
	}

	private bool SourceObjectsEmpty(string[] sourceObjectNames)
	{
		return sourceObjectNames.Length == 1 && string.IsNullOrEmpty(sourceObjectNames[0]);
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity classEntity)
	{
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		if (!MaxInterface.EstablishConnection())
		{
			entity.Result = new ResultCode("Could not establish a connection to 3DS Max.");
			return;
		}
		IImportedEntity entity2 = entity.Entity;
		ResultCode resultCode = ExportThroughMax(entity2, project.Paths.GamePantry);
		if (resultCode != null && resultCode.Message.Equals("OK"))
		{
			entity.Result = ProcessExportedEntity(entity2, classEntity, project.Paths.GamePantry);
			return;
		}
		string text = string.Format("Export failed while trying to export {0} using 3DS Max.\n" + resultCode.Message, entity2.Name);
		entity.Result = new ResultCode(text);
		BugSubmitter.SilentReport(text + " @assign agould");
	}

	private ResultCode ExportThroughMax(IImportedEntity instanceEntity, string localPantry)
	{
		ResultCode result = ResultCode.Success;
		string formattedPantryRoot = GetFormattedPantryRoot(localPantry, instanceEntity.Type);
		WindowsPath filePathToExport = new WindowsPath(instanceEntity.SourceFilePath);
		WindowsPath outputFolderPath = new WindowsPath(formattedPantryRoot);
		string sourceObjectName = instanceEntity.SourceObjectName;
		string name = instanceEntity.Name;
		if (instanceEntity.Type == InstanceType.IT_GEOMETRY || instanceEntity.Type == InstanceType.IT_ANALYTIC_LIGHT)
		{
			result = MaxInterface.ExportGeometry("Civ6", filePathToExport, outputFolderPath, sourceObjectName, name);
		}
		else if (instanceEntity.Type == InstanceType.IT_ANIMATION)
		{
			result = MaxInterface.ExportAnimation("Civ6", filePathToExport, outputFolderPath, sourceObjectName, name);
		}
		return result;
	}

	private string GetFormattedPantryRoot(string localPantry, InstanceType type)
	{
		return StaticMethods.PantryRootForInstanceType(localPantry, type).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode resultCode = ResultCode.Success;
		instanceEntity.PopulateDataFiles(classEntity);
		if (instanceEntity.DataFiles.Any((IInstanceDataFile df) => df.ID == "GR2"))
		{
			resultCode = ExportUtilityFunctions.ParseGrannyFiles(instanceEntity, localPantry);
			if ((bool)resultCode)
			{
				instanceEntity.UpdateExportedTime();
			}
		}
		return resultCode;
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

	public void Export(ICivTechService civTechSvc, IEnumerable<Tuple<ImportOperationResult, IClassEntity>> entities)
	{
		foreach (Tuple<ImportOperationResult, IClassEntity> entity in entities)
		{
			Export(civTechSvc, entity.Item1, entity.Item2);
		}
	}
}
