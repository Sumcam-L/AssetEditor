using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Error;
using Firaxis.Granny;
using Firaxis.IO;
using Firaxis.Utility;

namespace Firaxis.ContentExporters.Implementations;

public class FiraxisGeometryExporter : IContentExporter
{
	private readonly ICollection<string> _supportedFileTypes = new List<string>();

	private readonly ICollection<InstanceType> _supportedInstanceTypes = new List<InstanceType>();

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	private IFBXInterface FBXInterface { get; set; }

	public FiraxisGeometryExporter()
	{
		Context.EnsureCreated<GrannyContext>();
		FBXInterface = Context.EnsureCreated<CivTechContext>().CreateInstance<IFBXInterface>();
		_supportedFileTypes.Add(".fbx");
		_supportedInstanceTypes.Add(InstanceType.IT_GEOMETRY);
		_supportedInstanceTypes.Add(InstanceType.IT_ANIMATION);
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
		IImportedEntity entity2 = entity.Entity;
		if (ExportFromFBX(entity2, classEntity, project.Paths.GamePantry))
		{
			entity.Result = ProcessExportedEntity(entity2, classEntity, project.Paths.GamePantry);
			return;
		}
		entity.Result = new ResultCode("Export failed while trying to export {0} using FBX.", entity2.Name);
	}

	private bool ExportFromFBX(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		bool flag = false;
		string formattedPantryRoot = GetFormattedPantryRoot(localPantry, instanceEntity.Type);
		WindowsPath windowsPath = new WindowsPath(instanceEntity.SourceFilePath);
		WindowsPath windowsPath2 = new WindowsPath(formattedPantryRoot);
		string sourceObjectName = instanceEntity.SourceObjectName;
		string name = instanceEntity.Name;
		if (instanceEntity.Type == InstanceType.IT_GEOMETRY || instanceEntity.Type == InstanceType.IT_ANALYTIC_LIGHT)
		{
			flag = FBXInterface.ExportGeometry(windowsPath.FullPath, windowsPath2.FullPath, sourceObjectName, name);
			if (flag)
			{
				flag = FBXInterface.ExportWIG(windowsPath.FullPath, windowsPath2.FullPath, sourceObjectName, name);
			}
		}
		else if (instanceEntity.Type == InstanceType.IT_ANIMATION)
		{
			IEnumerable<string> sourceObjectNames = new MaxExporter().GetSourceObjectNames("E:\\Sumcam\\Desktop\\aaa.max", InstanceType.IT_GEOMETRY);
			if (sourceObjectNames.Contains("a"))
			{
				sourceObjectNames.Contains("a");
			}
			flag = FBXInterface.ExportAnimation(windowsPath.FullPath, windowsPath2.FullPath, sourceObjectName, name);
		}
		return flag;
	}

	private string GetFormattedPantryRoot(string localPantry, InstanceType type)
	{
		return StaticMethods.PantryRootForInstanceType(localPantry, type).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode result = ResultCode.Success;
		StringBuilder stringBuilder = new StringBuilder();
		instanceEntity.PopulateDataFiles(classEntity);
		if (instanceEntity.DataFiles.Any((IInstanceDataFile df) => df.ID == "GR2"))
		{
			ResultCode resultCode = ExportUtilityFunctions.ParseGrannyFiles(instanceEntity, localPantry);
			ResultCode resultCode2 = ExportUtilityFunctions.ParseWigFiles(instanceEntity as IGeometryInstanceBuildable, localPantry);
			if (!resultCode)
			{
				stringBuilder.AppendLine(resultCode.Message);
			}
			if (!resultCode2)
			{
				stringBuilder.AppendLine(resultCode2.Message);
			}
		}
		if (classEntity.CookParameters.FindByName("LOD") != null)
		{
			ResultCode resultCode3 = SetLodCookParameters(instanceEntity, classEntity);
			if (!resultCode3)
			{
				stringBuilder.AppendLine(resultCode3.Message);
			}
		}
		if (stringBuilder.Length > 0)
		{
			result = new ResultCode(stringBuilder.ToString());
		}
		return result;
	}

	private ResultCode SetLodCookParameters(IImportedEntity instanceEntity, IClassEntity classEntity)
	{
		if (!(instanceEntity is IGeometryInstance geometryInstance))
		{
			return new ResultCode("Failed to cast IImportedEntity to IGeometryInstance");
		}
		if (!(instanceEntity.CookParameters.FindValue("LOD") is ICollectionValue collectionValue))
		{
			return new ResultCode("GeometryInstance does not possess LOD cook parameters");
		}
		int i = 0;
		for (int num = geometryInstance.Lods.Count(); i < num; i++)
		{
			Lod lod = geometryInstance.Lods.ElementAt(i);
			ITupleValue tupleValue = collectionValue.Push<ITupleValue>("LOD" + i);
			IFloatValue floatValue = tupleValue.Elements.Push<IFloatValue>("Reduction");
			floatValue.ParameterValue = lod.Reduction;
			IFloatValue floatValue2 = tupleValue.Elements.Push<IFloatValue>("Transition");
			floatValue2.ParameterValue = lod.TransitionArea;
			IStringValue stringValue = tupleValue.Elements.Push<IStringValue>("Source");
			stringValue.ParameterValue = geometryInstance.DataFiles.First((IInstanceDataFile df) => df.ID == "GR2").RelativePath;
		}
		return ResultCode.Success;
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

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		return entityType switch
		{
			InstanceType.IT_ANIMATION => FBXInterface.GetRootModels(filePath), 
			InstanceType.IT_GEOMETRY => FBXInterface.GetRootModels(filePath), 
			_ => Enumerable.Empty<string>(), 
		};
	}
}
