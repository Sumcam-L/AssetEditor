using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.IO;
using Firaxis.MayaInterface;
using Sce.Atf;

namespace Firaxis.ContentExporters.Implementations;

public class MayaExporter : IContentExporter, IContentCreationTool
{
	private List<string> _supportedFileTypes;

	private List<InstanceType> _supportedInstanceTypes;

	private AutodeskMayaInterface MayaInterface { get; set; }

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public MayaExporter()
	{
		MayaInterface = new AutodeskMayaInterface();
		_supportedFileTypes = new List<string>();
		_supportedFileTypes.Add(".ma");
		_supportedFileTypes.Add(".mb");
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_GEOMETRY);
		_supportedInstanceTypes.Add(InstanceType.IT_ANIMATION);
		_supportedInstanceTypes.Add(InstanceType.IT_ANALYTIC_LIGHT);
	}

	public void OpenFile(string filePath)
	{
		MayaInterface.OpenFile(filePath);
	}

	public void RenderSourceObjectFromFile(string filePath, string sourceObject, string outputFolder)
	{
		MayaInterface.ExportAnimationPlaybasts("", new WindowsPath(filePath), new WindowsPath(outputFolder), sourceObject);
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		if (!SupportedInstanceTypes.Contains(entityType))
		{
			return Enumerable.Empty<string>();
		}
		if (!MayaInterface.EstablishConnection())
		{
			return Enumerable.Empty<string>();
		}
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
		{
			if (entityType == InstanceType.IT_ANIMATION)
			{
				list.AddRange(MayaInterface.GetAnimationNames(new WindowsPath(filePath)));
			}
			else
			{
				list.AddRange(MayaInterface.GetModelNames(new WindowsPath(filePath)));
			}
		}
		if (list.Count() == 1 && string.IsNullOrEmpty(list[0]))
		{
			list.RemoveAt(0);
		}
		list.RemoveAll((string str) => string.IsNullOrEmpty(str));
		return list;
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity classEntity)
	{
		IImportedEntity entity2 = entity.Entity;
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		if (!MayaInterface.EstablishConnection())
		{
			entity.Result = new ResultCode("Could not establish a connection to Autodesk Maya.");
			return;
		}
		if (string.IsNullOrEmpty(entity2.SourceFilePath))
		{
			entity.Result = new ResultCode("Entity ({0}) does not have a source file path set and cannot be exported.", entity2.Name);
			return;
		}
		if (!File.Exists(entity2.SourceFilePath))
		{
			entity.Result = new ResultCode("Entity's ({0}) source file does not exist on disk.  Invalid path: ({1}).", entity2.Name, entity2.SourceFilePath);
			return;
		}
		string path = StaticMethods.PantryRootForInstanceType(project.Paths.GamePantry, entity2.Type).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		ResultCode success = ResultCode.Success;
		if (entity2.Type == InstanceType.IT_ANIMATION)
		{
			success = MayaInterface.ExportAnimation("Civ6", new WindowsPath(entity2.SourceFilePath), new WindowsPath(path), entity2.SourceObjectName, entity2.Name);
		}
		else
		{
			if (entity2.Type != InstanceType.IT_GEOMETRY && entity2.Type != InstanceType.IT_ANALYTIC_LIGHT)
			{
				entity.Result = new ResultCode("The Maya exporter only supports geometries and animations.  Passed in entity has the following type: {0}", entity2.Type.ToString());
				return;
			}
			bool exportWigFile = false;
			foreach (IClassDataFile dataFile in classEntity.DataFiles)
			{
				if (dataFile.ID == "WIG")
				{
					exportWigFile = true;
				}
			}
			success = MayaInterface.ExportGeometry("Civ6", new WindowsPath(entity2.SourceFilePath), new WindowsPath(path), entity2.SourceObjectName, entity2.Name, null, exportWigFile);
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (success != null && success.Message.Equals("OK"))
		{
			ResultCode resultCode = ProcessExportedEntity(entity2, classEntity, project.Paths.GamePantry);
			if (!resultCode)
			{
				stringBuilder.AppendLine(resultCode.Message);
			}
		}
		else
		{
			entity.Result = new ResultCode("Export failed while trying to export {0} using Maya.", entity2.Name);
			BugSubmitter.SilentReport(entity.Result.Message + " @assign agould");
		}
		if (stringBuilder.Length > 0)
		{
			entity.Result = new ResultCode(stringBuilder.ToString());
			BugSubmitter.SilentReport(entity.Result.Message + " @assign agould");
		}
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode resultCode = ResultCode.Success;
		instanceEntity.PopulateDataFiles(classEntity);
		if (instanceEntity is IGeometryInstanceBuildable)
		{
			ResultCode success = ResultCode.Success;
			resultCode = ExportUtilityFunctions.ParseGrannyFiles(instanceEntity, localPantry);
			success = ExportUtilityFunctions.ParseWigFiles(instanceEntity as IGeometryInstanceBuildable, localPantry);
			if (!resultCode)
			{
				if (!success)
				{
					resultCode = new ResultCode(resultCode.Message + " " + success.Message);
				}
			}
			else
			{
				resultCode = success;
			}
		}
		else if (instanceEntity is IAnimationInstance)
		{
			resultCode = ExportUtilityFunctions.ParseGrannyFiles(instanceEntity, localPantry);
		}
		instanceEntity.UpdateExportedTime();
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
		List<string> list = new List<string>();
		List<string> list2 = new List<string>();
		IDictionary<string, Tuple<InstanceType, ISet<string>>> dictionary = new Dictionary<string, Tuple<InstanceType, ISet<string>>>();
		string empty = string.Empty;
		string empty2 = string.Empty;
		if (!entities.Any())
		{
			return;
		}
		foreach (IImportedEntity item in entities.Select((Tuple<ImportOperationResult, IClassEntity> tup) => tup.Item1.Entity))
		{
			if (!dictionary.ContainsKey(item.SourceFilePath))
			{
				dictionary[item.SourceFilePath] = new Tuple<InstanceType, ISet<string>>(item.Type, new HashSet<string>());
			}
			ProjectEnvironment project = null;
			ResultCode entityProject = civTechSvc.GetEntityProject(item, ref project);
			if ((bool)entityProject)
			{
				dictionary[item.SourceFilePath].Item2.Add(project.Paths.GamePantry);
			}
			else
			{
				Outputs.WriteLine(OutputMessageType.Error, entityProject.Message);
			}
		}
		foreach (KeyValuePair<string, Tuple<InstanceType, ISet<string>>> sourceFileInfo in dictionary)
		{
			IEnumerable<Tuple<ImportOperationResult, IClassEntity>> enumerable = entities.Where((Tuple<ImportOperationResult, IClassEntity> entity) => entity.Item1.Entity.SourceFilePath == sourceFileInfo.Key);
			bool exportWigFile = false;
			foreach (Tuple<ImportOperationResult, IClassEntity> item2 in enumerable)
			{
				list.Add(item2.Item1.Entity.SourceObjectName);
				list2.Add(item2.Item1.Entity.Name);
				foreach (IClassDataFile dataFile in item2.Item2.DataFiles)
				{
					if (dataFile.ID == "WIG")
					{
						exportWigFile = true;
					}
				}
			}
			empty = string.Join(", ", list);
			empty2 = string.Join(", ", list2);
			foreach (string item3 in sourceFileInfo.Value.Item2)
			{
				string path = StaticMethods.PantryRootForInstanceType(item3, sourceFileInfo.Value.Item1).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
				ResultCode resultCode = ResultCode.Success;
				if (sourceFileInfo.Value.Item1 == InstanceType.IT_ANIMATION)
				{
					resultCode = MayaInterface.ExportAnimation("Civ6", new WindowsPath(sourceFileInfo.Key), new WindowsPath(path), empty, empty2);
				}
				else if (sourceFileInfo.Value.Item1 == InstanceType.IT_GEOMETRY || sourceFileInfo.Value.Item1 == InstanceType.IT_ANALYTIC_LIGHT)
				{
					resultCode = MayaInterface.ExportGeometry("Civ6", new WindowsPath(sourceFileInfo.Key), new WindowsPath(path), empty, empty2, null, exportWigFile);
				}
				if (resultCode.Message.Equals("OK"))
				{
					StringBuilder stringBuilder = new StringBuilder();
					foreach (Tuple<ImportOperationResult, IClassEntity> item4 in enumerable)
					{
						ResultCode resultCode2 = ProcessExportedEntity(item4.Item1.Entity, item4.Item2, item3);
						if (!resultCode2)
						{
							stringBuilder.AppendLine(resultCode2.Message);
						}
					}
					continue;
				}
				foreach (ImportOperationResult item5 in entities.Select((Tuple<ImportOperationResult, IClassEntity> tup) => tup.Item1))
				{
					item5.Result = new ResultCode(string.Format("Export failed while trying to export multiple files using Maya.\n\nError: \"{0}\"", string.IsNullOrEmpty(resultCode.Message) ? "Failure cause unknown" : resultCode.Message));
				}
			}
			list.Clear();
			list2.Clear();
		}
	}
}
