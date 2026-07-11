using System;
using System.ComponentModel.Composition;
using System.IO;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(ShadowDocumentClient))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class ShadowDocumentClient : IDocumentClient
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("ShadowDocument".Localize(), new string[10] { ".tex", ".anm", ".geo", ".lit", ".ptl", ".env", ".ast", ".bhv", ".mtl", ".ffx" }, new InstanceType[10]
	{
		InstanceType.IT_TEXTURE,
		InstanceType.IT_ANIMATION,
		InstanceType.IT_GEOMETRY,
		InstanceType.IT_ANALYTIC_LIGHT,
		InstanceType.IT_PARTICLE_EFFECT,
		InstanceType.IT_ENVIRONMENT_LIGHT,
		InstanceType.IT_ASSET,
		InstanceType.IT_BEHAVIOR,
		InstanceType.IT_MATERIAL,
		InstanceType.IT_FIREFX
	}, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewShadowDocument", multiDocument: true, isHidden: true, "Hidden", Sce.Atf.Resources.DocumentImage);

	private readonly ICivTechService m_civtechService;

	public DocumentClientInfo Info => DocumentClientInfo;

	public bool AskWhenClosingDirtyDocument => false;

	[ImportingConstructor]
	public ShadowDocumentClient(ICivTechService civTechService)
	{
		Outputs.WriteLine(OutputMessageType.Info, "Starting up ShadowDocumentClient");
		m_civtechService = civTechService;
	}

	public bool CanOpen(Uri uri)
	{
		return Info.IsCompatibleUri(uri);
	}

	public void Close(IDocument document)
	{
	}

	public IDocument Open(Uri uri)
	{
		IInstanceSet instanceSet = Context.EnsureCreated<CivTechContext>().CreateInstance<IInstanceSet>(new object[1] { m_civtechService.GetActivePantryPaths() });
		InstanceType type = InstanceType.IT_COUNT;
		string instanceName = string.Empty;
		StaticMethods.GetInstanceNameAndType(m_civtechService.ProjectMapService, uri.LocalPath, out instanceName, out type);
		if (string.IsNullOrEmpty(instanceName))
		{
			return null;
		}
		IInstanceEntity instanceEntity = null;
		if (File.Exists(uri.LocalPath))
		{
			instanceEntity = instanceSet.LoadEntityByPath(uri.LocalPath, type);
			BugSubmitter.SilentAssert(instanceEntity == null || PathComparer.PathCompareWithCase.Equals(instanceEntity.Name, instanceName), "Opening an entity from file \"{0}\" resulted in an entity with the name of \"{1}\" instead of the expected name of \"{2}\" @summary Opening an entity with a given URI did not result in opening an entity of the same name @assign bwhitman", uri.LocalPath, instanceEntity.Name, instanceName);
		}
		if (instanceEntity == null)
		{
			instanceEntity = instanceSet.CreateEntityByName(instanceName, type);
		}
		if (instanceEntity == null)
		{
			return null;
		}
		return new EntityShadowDocument
		{
			Uri = uri,
			InstanceSet = instanceSet,
			CivTechService = m_civtechService,
			InstanceEntity = instanceEntity
		};
	}

	public void Reload(IDocument document)
	{
		throw new NotImplementedException();
	}

	public bool Save(IDocument document, Uri uri)
	{
		if (!(document is IShadowEntityDocument shadowEntityDocument))
		{
			return false;
		}
		shadowEntityDocument.InstanceEntity.Name = StaticMethods.SanitizeEntityName(shadowEntityDocument.InstanceEntity.Name);
		string text = string.Empty;
		IImportedEntity importedEntity = shadowEntityDocument.InstanceEntity as IImportedEntity;
		if (importedEntity != null)
		{
			text = importedEntity.SourceFilePath;
			string depotPath = m_civtechService.PrimaryProject.VersionControl.GetDepotPath(text);
			importedEntity.SourceFilePath = (string.IsNullOrEmpty(depotPath) ? text : depotPath);
		}
		bool result = shadowEntityDocument.InstanceEntity.SerializeIntoFile(uri.LocalPath);
		if (importedEntity != null)
		{
			importedEntity.SourceFilePath = text;
		}
		Outputs.WriteLine(OutputMessageType.Info, $"Saved shadow document {document.Uri} to {uri}");
		return result;
	}

	public void Show(IDocument document)
	{
		if (!(document is IShadowEntityDocument))
		{
			throw new ArgumentException("The document passed in to Show is not a Shadow Document.  Document Path: {0}", document.Uri.LocalPath);
		}
		throw new NotImplementedException();
	}
}
