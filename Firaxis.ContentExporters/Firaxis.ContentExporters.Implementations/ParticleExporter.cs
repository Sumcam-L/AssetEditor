using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.ParticleExport;
using Firaxis.Error;
using Firaxis.Utility;
using Microsoft.Win32;

namespace Firaxis.ContentExporters.Implementations;

public class ParticleExporter : IContentExporter, IContentCreationTool
{
	private const string ExportModelsCookParameter = "ParticleModels";

	private const string ExportTexturesCookParameter = "ParticleTextures";

	private const string ForkParticleRegistryKey = "HKEY_CURRENT_USER\\SOFTWARE\\Firaxis\\Tools\\ForkParticleToolsCivTech";

	private const string ForkParticleStudioKey = "ParticleStudio";

	private const string ForkParticlePEB2PSB = "Peb2Psb";

	private const string ForkParticleRegistryKeyError = "The Fork Particle Studio registry key could not be found. Please ensure that ForkParticleToolsCivTech is installed.";

	private const string ForkParticlePeb2PsbMissingError = "The Fork Particle Studio command line tool could not be found. Please ensure that ForkParticleToolsCivTech is installed.";

	private const string ForkParticleParticleStudioMissingError = "Fork Particle Studio could not be found. Please ensure that ForkParticleToolsCivTech is installed.";

	private List<string> _supportedFileTypes;

	private List<InstanceType> _supportedInstanceTypes;

	private IParticleExportHelper ExportHelper { get; set; }

	public IEnumerable<string> SupportedFileTypes => _supportedFileTypes;

	public IEnumerable<InstanceType> SupportedInstanceTypes => _supportedInstanceTypes;

	public ParticleExporter()
	{
		Context.EnsureCreated<CivTechContext>();
		ExportHelper = Context.Get<CivTechContext>().CreateInstance<IParticleExportHelper>();
		_supportedFileTypes = new List<string>();
		_supportedFileTypes.Add(".peb");
		_supportedInstanceTypes = new List<InstanceType>();
		_supportedInstanceTypes.Add(InstanceType.IT_PARTICLE_EFFECT);
	}

	public void OpenFile(string filePath)
	{
		string text = (string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Firaxis\\Tools\\ForkParticleToolsCivTech", "ParticleStudio", string.Empty);
		if (string.IsNullOrEmpty(text))
		{
			MessageBox.Show(null, "The Fork Particle Studio registry key could not be found. Please ensure that ForkParticleToolsCivTech is installed.", "DCC Tool Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		if (!File.Exists(text))
		{
			MessageBox.Show(null, "Fork Particle Studio could not be found. Please ensure that ForkParticleToolsCivTech is installed.", "DCC Tool Unavailable", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		ProcessStartInfo processStartInfo = new ProcessStartInfo(text);
		processStartInfo.Arguments = filePath;
		Process.Start(processStartInfo);
	}

	public void RenderSourceObjectFromFile(string filePath, string sourceObject, string outputFolder)
	{
	}

	public IEnumerable<string> GetSourceObjectNames(string filePath, InstanceType entityType)
	{
		if (entityType != InstanceType.IT_PARTICLE_EFFECT)
		{
			return null;
		}
		List<string> list = new List<string>();
		if (!string.IsNullOrEmpty(filePath) && File.Exists(filePath))
		{
			list.Add(string.Empty);
		}
		return list;
	}

	public void Export(ICivTechService civTechSvc, ImportOperationResult entity, IClassEntity classEntity)
	{
		IImportedEntity entity2 = entity.Entity;
		DateTime now = DateTime.Now;
		ProjectEnvironment project = null;
		ResultCode entityProject = civTechSvc.GetEntityProject(entity.Entity, ref project);
		if (!entityProject)
		{
			entity.Result = entityProject;
			return;
		}
		string text = StaticMethods.PantryRootForInstanceType(project.Paths.GamePantry, entity2.Type);
		text = text.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
		if (string.IsNullOrEmpty(entity2.SourceFilePath))
		{
			entity.Result = new ResultCode("Particle Effect ({0}) cannot be exported because it does not have a source file set.", entity2.Name);
			return;
		}
		if (!File.Exists(entity2.SourceFilePath))
		{
			entity.Result = new ResultCode("Particle Effect ({0}) cannot be exported because its source file doesn't exist on disk.  Invalid path: ({1})", entity2.Name, entity2.SourceFilePath);
			return;
		}
		string text2 = (string)Registry.GetValue("HKEY_CURRENT_USER\\SOFTWARE\\Firaxis\\Tools\\ForkParticleToolsCivTech", "Peb2Psb", string.Empty);
		if (string.IsNullOrEmpty(text2))
		{
			entity.Result = new ResultCode("The Fork Particle Studio registry key could not be found. Please ensure that ForkParticleToolsCivTech is installed.");
			return;
		}
		if (!File.Exists(text2))
		{
			entity.Result = new ResultCode("The Fork Particle Studio command line tool could not be found. Please ensure that ForkParticleToolsCivTech is installed.");
			return;
		}
		StringBuilder errorMessage = new StringBuilder();
		errorMessage.Append("Command line exporter failed.  The output was:\n\n");
		ProcessStartInfo processStartInfo = new ProcessStartInfo(text2);
		processStartInfo.Arguments = "\"" + entity2.SourceFilePath + "\" /v1 /version /x64 /ox64 \"" + text + "\"";
		processStartInfo.RedirectStandardOutput = true;
		processStartInfo.UseShellExecute = false;
		processStartInfo.CreateNoWindow = true;
		Process process = Process.Start(processStartInfo);
		process.OutputDataReceived += delegate(object sender, DataReceivedEventArgs e)
		{
			if (!string.IsNullOrEmpty(e.Data))
			{
				errorMessage.Append(e.Data);
			}
		};
		process.BeginOutputReadLine();
		process.WaitForExit();
		if (process.ExitCode != 0)
		{
			entity.Result = new ResultCode(errorMessage.ToString());
			return;
		}
		string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(entity2.SourceFilePath);
		string text3 = Path.Combine(text, entity2.Name + ".psb");
		string text4 = Path.Combine(text, fileNameWithoutExtension + ".psb");
		if (!File.Exists(text3) && File.Exists(text4))
		{
			File.Move(text4, text3);
		}
		ProcessExportedEntity(entity2, classEntity, project.Paths.GamePantry);
		IObjectCollectionValue objectCollectionValue = entity2.CookParameters.FindValue("ParticleModels") as IObjectCollectionValue;
		IObjectCollectionValue objectCollectionValue2 = entity2.CookParameters.FindValue("ParticleTextures") as IObjectCollectionValue;
		if (objectCollectionValue == null || objectCollectionValue2 == null)
		{
			return;
		}
		objectCollectionValue.Clear();
		objectCollectionValue2.Clear();
		IInstanceDataFile instanceDataFile = entity2.DataFiles.FirstOrDefault();
		if (instanceDataFile == null)
		{
			return;
		}
		string dataFilePath = entity2.GetDataFilePath(instanceDataFile.RelativePath);
		if (!File.Exists(dataFilePath))
		{
			return;
		}
		foreach (KeyValuePair<ParticleAssetType, string> particleEffectAsset in ExportHelper.GetParticleEffectAssets(dataFilePath))
		{
			string fileNameWithoutExtension2 = Path.GetFileNameWithoutExtension(particleEffectAsset.Value);
			if (particleEffectAsset.Key == ParticleAssetType.Model)
			{
				string pantryPath = civTechSvc.ProjectMapService.LayeredPantry.GetPantryPath(fileNameWithoutExtension2, InstanceType.IT_ASSET);
				string objectName = (File.Exists(pantryPath) ? fileNameWithoutExtension2 : string.Empty);
				objectCollectionValue.Push<IObjectValue>(fileNameWithoutExtension2, InstanceType.IT_ASSET, objectName);
			}
			else if (particleEffectAsset.Key == ParticleAssetType.Texture)
			{
				string pantryPath2 = civTechSvc.ProjectMapService.LayeredPantry.GetPantryPath(fileNameWithoutExtension2, InstanceType.IT_TEXTURE);
				string objectName2 = (File.Exists(pantryPath2) ? fileNameWithoutExtension2 : string.Empty);
				objectCollectionValue2.Push<IObjectValue>(fileNameWithoutExtension2, InstanceType.IT_TEXTURE, objectName2);
			}
		}
	}

	private ResultCode ProcessExportedEntity(IImportedEntity instanceEntity, IClassEntity classEntity, string localPantry)
	{
		ResultCode success = ResultCode.Success;
		instanceEntity.PopulateDataFiles(classEntity);
		instanceEntity.CookParameters.AddDefaultValuesAsNecessary(classEntity.CookParameters);
		return success;
	}

	private string GetEntityFilePath(string entityName, string localPantry, InstanceType entityType)
	{
		string text = StaticMethods.ExtensionForInstanceType(entityType);
		string path = StaticMethods.PantryRootForInstanceType(localPantry, entityType);
		return Path.Combine(path, entityName + text);
	}

	public void Export(ICivTechService civTechSvc, IEnumerable<Tuple<ImportOperationResult, IClassEntity>> entities)
	{
		foreach (Tuple<ImportOperationResult, IClassEntity> entity in entities)
		{
			Export(civTechSvc, entity.Item1, entity.Item2);
		}
	}

	private bool ValidateClassObjectCollection(IImportedEntity entity, IClassEntity entityClass, string paramName, InstanceType entityObjectType, ref ResultCode resultCode)
	{
		bool result = true;
		IParameter parameter = entityClass.CookParameters.FindByName(paramName);
		if (parameter == null)
		{
			result = false;
			resultCode = new ResultCode("The entity is expecting a collection cook parameter named '{0}'", paramName);
		}
		else if (!(parameter is IObjectCollectionParameter objectCollectionParameter))
		{
			result = false;
			resultCode = new ResultCode("The entity is expecting a collection cook parameter named '{0}'", paramName);
		}
		else if (objectCollectionParameter.EntryObjectType != entityObjectType)
		{
			result = false;
			resultCode = new ResultCode("The entity is expecting a {0} collection cook parameter named '{1}'", entityObjectType.ToString(), paramName);
		}
		return result;
	}

	public ResultCode ValidateClass(IImportedEntity entity, IClassEntity entityClass)
	{
		ResultCode resultCode = ResultCode.Success;
		if (!ValidateClassObjectCollection(entity, entityClass, "ParticleModels", InstanceType.IT_ASSET, ref resultCode))
		{
			return resultCode;
		}
		if (!ValidateClassObjectCollection(entity, entityClass, "ParticleTextures", InstanceType.IT_TEXTURE, ref resultCode))
		{
			return resultCode;
		}
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
