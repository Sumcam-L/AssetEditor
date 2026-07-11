using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(AnalyticLightEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class AnalyticLightEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Analytic Light".Localize(), new string[1] { ".lit" }, new InstanceType[1] { InstanceType.IT_ANALYTIC_LIGHT }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewAnalyticLight", multiDocument: true, Resources.AnalyticLightFileIcon);

	private IDictionary<IEntityDocument, string> m_originalSources = new Dictionary<IEntityDocument, string>();

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.AnalyticLightEntityType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.AnalyticLightEntityRootElement;

	[ImportingConstructor]
	public AnalyticLightEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	protected override string GetEditorName()
	{
		return "analyticLightInstanceEditor";
	}

	protected override void OnDocumentSaved(IEntityDocument document, Uri uri)
	{
		if (m_originalSources.ContainsKey(document))
		{
			(document.InstanceEntity as IImportedEntity).SourceFilePath = m_originalSources[document];
			m_originalSources.Remove(document);
		}
		base.OnDocumentSaved(document, uri);
	}

	protected override void OnDocumentSaving(IEntityDocument document, Uri uri)
	{
		IImportedEntity importedEntity = document.InstanceEntity as IImportedEntity;
		string sourceFilePath = importedEntity.SourceFilePath;
		if (!string.IsNullOrEmpty(sourceFilePath))
		{
			string depotPath = base.CivTechService.PrimaryProject.VersionControl.GetDepotPath(sourceFilePath);
			importedEntity.SourceFilePath = (string.IsNullOrEmpty(depotPath) ? sourceFilePath : depotPath);
			m_originalSources.Add(document, sourceFilePath);
		}
		base.OnDocumentSaving(document, uri);
	}
}
