using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

[Export(typeof(IDocumentClient))]
[Export(typeof(TextureEntityEditor))]
[Export(typeof(IInitializable))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class TextureEntityEditor : BaseEntityEditor
{
	public static DocumentClientInfo DocumentClientInfo = new EntityDocumentClientInfo("Texture".Localize(), new string[1] { ".tex" }, new InstanceType[1] { InstanceType.IT_TEXTURE }, Sce.Atf.Resources.DocumentImage, Sce.Atf.Resources.FolderImage, "NewTexture", multiDocument: true, Resources.TextureFileIcon);

	private IDictionary<IEntityDocument, string> m_originalSources = new Dictionary<IEntityDocument, string>();

	public override DocumentClientInfo Info => DocumentClientInfo;

	protected override DomNodeType DomNodeEntityType => EntitySchema.TextureEntityType.Type;

	protected override ChildInfo DomNodeRootElement => EntitySchema.TextureEntityRootElement;

	[ImportingConstructor]
	public TextureEntityEditor(IContextRegistry contextRegistry, EntitySchemaLoader importedEntitySchemaLoader, IDocumentRegistryMediator documentRegistryMediator)
		: base(contextRegistry, importedEntitySchemaLoader, documentRegistryMediator)
	{
	}

	public void SetSourceFilePath(IDocument doc, string srcPath)
	{
		ImportedEntityAdapter importedEntityAdapter = (doc as DomNodeAdapter).As<ImportedEntityAdapter>();
		if (importedEntityAdapter != null)
		{
			importedEntityAdapter.SourceFilePath = srcPath;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to TextureEntityEditor.SetSourceFilePath");
		}
	}

	public void SetTextureName(IDocument doc, string texName)
	{
		if (doc is IInstanceEntityDocument)
		{
			(doc as DomNodeAdapter).As<INamedAdapter>().Name = texName;
		}
		else
		{
			Outputs.WriteLine(OutputMessageType.Error, "Incorrect document type \"" + doc.GetType().ToString() + "\" passed to TextureEntityEditor.SetTextureName");
		}
	}

	protected override string GetEditorName()
	{
		return "textureInstanceEditor";
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
