using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(EnvironmentLightEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class EnvironmentLightEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("EnvironmentLight".Localize(), new string[1] { ".env" }, new InstanceType[1] { InstanceType.IT_ENVIRONMENT_LIGHT }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewEnvironmentLight", multiDocument: true, Resources.EnvironmentMapFileIcon);

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.EnvironmentLightEntityType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.EnvironmentLightEntityRootElement;

	[ImportingConstructor]
	public EnvironmentLightEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	protected override string GetEditorName()
	{
		return "environmentLightEditor";
	}

	protected override void OnDocumentSaving(IEntityDocument document, Uri uri)
	{
		EnvironmentLightContext environmentLightContext = document.As<EnvironmentLightContext>();
		IEnvironmentLightClass lightClass = environmentLightContext.LightClass;
		IEnvironmentLightInstance environmentLight = environmentLightContext.EnvironmentLight;
		if (lightClass == null)
		{
			string text = "No light class has been selected.  The file will not be saved.";
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		IEnumerable<string> dataFileList = GetDataFileList(environmentLightContext, environmentLight, lightClass);
		if (!dataFileList.Any())
		{
			return;
		}
		document.InstanceEntity.Name = StaticMethods.SanitizeEntityName(document.InstanceEntity.Name);
		base.SourceControlService?.OpenInPerforce(dataFileList);
		foreach (string item in dataFileList)
		{
			if (IsWriteableUri(item))
			{
				try
				{
					environmentLightContext.Cube.SaveDDS(item, lightClass.ImportOptions.PixelFormat);
				}
				catch (System.Exception ex)
				{
					string text2 = "Exception thrown when trying to save the Cube Map of Environment Light (" + environmentLightContext.EnvironmentLight.Name + ") to (" + item + ").  Exception message:\n(" + ex.Message + ")";
					Outputs.WriteLine(OutputMessageType.Error, text2);
					MessageBox.Show(text2, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
				}
			}
		}
	}

	private IEnumerable<string> GetDataFileList(EnvironmentLightContext context, IEnvironmentLightInstance environmentLight, IEnvironmentLightClass lightClass)
	{
		_ = context.CivTechService.PrimaryProject.Paths.GamePantry;
		IEnumerable<IClassDataFile> enumerable = lightClass.DataFiles.Where((IClassDataFile df) => df != null);
		List<string> list = new List<string>();
		foreach (IClassDataFile item in enumerable)
		{
			IInstanceDataFile instanceDataFile = VerifyDataFile(environmentLight, item);
			if (instanceDataFile != null)
			{
				Uri uri = new Uri(Path.Combine(StaticMethods.PantryRootForInstanceType(base.CivTechService.PrimaryProject.Paths.GamePantry, environmentLight.Type), instanceDataFile.RelativePath).Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar));
				list.Add(uri.LocalPath);
			}
		}
		return list;
	}

	private bool IsWriteableUri(string uri)
	{
		FileInfo fileInfo = new FileInfo(uri);
		if (fileInfo.IsReadOnly)
		{
			string text = "The data file located at path (" + uri + ") is read-only and cannot be exported.  Please check the file out of Perforce.";
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		return !fileInfo.IsReadOnly;
	}

	private IInstanceDataFile VerifyDataFile(IEnvironmentLightInstance environmentLight, IClassDataFile classDataFile)
	{
		if (string.IsNullOrEmpty(environmentLight.SourceFilePath))
		{
			return null;
		}
		IInstanceDataFile instanceDataFile = environmentLight.FindDataFileByID(classDataFile.ID);
		if (instanceDataFile == null)
		{
			string text = "EnvironmentLight (" + environmentLight.Name + ") is missing an expected data file with the ID of (" + classDataFile.ID + ").";
			Outputs.WriteLine(OutputMessageType.Error, text);
			MessageBox.Show(text, "Error", MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
		return instanceDataFile;
	}
}
