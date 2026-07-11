using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class EntityPreviewingSequencer : Validator
{
	public IPreviewerCacheService PreviewerCacheService { get; set; } = null;

	public IDocumentService DocumentService { get; set; } = null;

	public ICivTechService CivTechService { get; set; }

	private HashSet<IEntityDocument> UndirtiedOnUndoDocuments { get; set; } = new HashSet<IEntityDocument>();

	private IPreviewWindow PreviewWindow => FiraxisATFRegistry.PreviewerDocumentService.GetWindowForDocument(base.DomNode.As<IDocument>());

	protected override void OnNodeSet()
	{
		base.OnNodeSet();
		if (!base.DomNode.Is<IShadowDocument>())
		{
			PreviewerCacheService = FiraxisATFRegistry.PreviewerCacheService;
			DocumentService = FiraxisATFRegistry.DocumentService;
			CivTechService = CivTechRegistry.CivTechService;
			IEntityDocument entityDocument = base.DomNode.As<IEntityDocument>();
			if (entityDocument != null)
			{
				entityDocument.DirtyChanged += HandleDocumentDirtyChanged;
			}
			IObservableContext observableContext = base.DomNode.As<IObservableContext>();
			if (observableContext != null)
			{
				observableContext.Reloaded += ObservableContext_Reloaded;
			}
		}
	}

	private void HandleDocumentDirtyChanged(object sender, EventArgs e)
	{
		IEntityDocument entityDocument = sender.As<IEntityDocument>();
		BugSubmitter.SilentAssert(entityDocument != null, "We attached previewing to a non-entity document.  This is outside of the current expectation and will not work with caching.  @assign bwhitman");
		if (entityDocument == null)
		{
			return;
		}
		BugSubmitter.SilentAssert(!entityDocument.IsReadOnly, "Document dirty changed on a read-only document.  Path: '{0}'  Active Project: '{1}'  @summary Invalid dirty state change  @assign bwhitman", entityDocument.Uri.LocalPath, CivTechService.PrimaryProject.Name);
		if (entityDocument.IsReadOnly || entityDocument.Dirty)
		{
			return;
		}
		if (PreviewerCacheService != null)
		{
			BugSubmitter.SilentAssert(PreviewerCacheService.IsCachedEntity(entityDocument), "Entity document was dirty but not cached.  Path: '{0}'  @assign bwhitman @summary Invalid Previewer Cache State - Dirty Document Not Cached", entityDocument.Uri.LocalPath);
			if (PreviewerCacheService.IsCachedEntity(entityDocument))
			{
				PreviewerCacheService.RemoveFromCache(entityDocument);
			}
		}
		HistoryContext historyContext = sender.As<HistoryContext>();
		if (historyContext != null && historyContext.UndoingOrRedoing)
		{
			UndirtiedOnUndoDocuments.Add(entityDocument);
		}
	}

	private void ObservableContext_Reloaded(object sender, EventArgs e)
	{
		if (e is TimelineReloadEvent || PreviewerCacheService == null)
		{
			return;
		}
		IEntityDocument entityDocument = sender.As<IEntityDocument>();
		BugSubmitter.SilentAssert(entityDocument != null, "We attached previewing to a non-entity document.  This is outside of the current expectation and will not work with caching.  @assign bwhitman");
		if (entityDocument == null)
		{
			return;
		}
		if (!entityDocument.Dirty && PreviewerCacheService.IsCachedEntity(entityDocument))
		{
			PreviewerCacheService.RemoveFromCache(entityDocument);
		}
		IPreviewableDocument previewableDocument = sender.As<IPreviewableDocument>();
		if (previewableDocument == null)
		{
			return;
		}
		IPreviewWindow previewWindow = PreviewWindow;
		if (previewWindow != null)
		{
			string previewModule = previewWindow.GetPreviewModule();
			if (previewModule == previewableDocument.PreviewModule)
			{
				previewWindow.ForceRefreshAsset(0);
			}
		}
	}

	public void HandlePreviewModuleChanged(IDocument doc)
	{
		IEntityDocument entityDocument = doc.As<IEntityDocument>();
		BugSubmitter.SilentAssert(entityDocument != null, "We are previewing a non-entity document.  This is outside of the current expectation and will not work with caching.  @assign bwhitman");
		if (entityDocument == null)
		{
			return;
		}
		BugSubmitter.SilentAssert(!entityDocument.IsReadOnly, "The preview module changed on a read-only document.  This shouldn't happen.  Path: '{0}'  @assign bwhitman  @summary Preview Module Changed on Read-Only Document", entityDocument.Uri.LocalPath);
		if (!entityDocument.IsReadOnly)
		{
			IPreviewableDocument previewableDocument = doc.As<IPreviewableDocument>();
			BugSubmitter.SilentAssert(previewableDocument != null, "Responded to a PreviewModuleChanged event on a non-PreviewContext.  This shouldn't happen.  Path: '{0}'  @assign bwhitman @summary PreviewModuleChanged on Invalid Object", entityDocument.Uri.LocalPath);
			if (previewableDocument != null)
			{
				UpdatePreviewerCacheService(entityDocument);
				entityDocument.As<IEntityPreviewComponent>()?.EntityChanges.CreateEntityChangedEvent(entityDocument.InstanceEntity.Type, entityDocument.InstanceEntity.Name);
			}
		}
	}

	protected override void OnEnded(object sender, EventArgs e)
	{
		if (!sender.Is<IShadowDocument>())
		{
			IEntityDocument entityDocument = sender.As<IEntityDocument>();
			if (PreviewerCacheService != null)
			{
				UpdatePreviewerCacheService(entityDocument);
			}
			IEntityPreviewComponent entityPreviewComponent = sender.As<IEntityPreviewComponent>();
			if (entityPreviewComponent != null && entityPreviewComponent.EntityChanges.EntityChanges.Any())
			{
				PreviewWindow?.UpdateAsset(entityPreviewComponent.EntityChanges.EntityChanges, 0);
			}
		}
	}

	private void UpdatePreviewerCacheService(IEntityDocument entityDocument)
	{
		BugSubmitter.SilentAssert(entityDocument != null, "EntityPreviewingSequencer is operating on a DomNode that is not an entity document.  This is invalid.  @assign bwhitman");
		if (entityDocument == null)
		{
			return;
		}
		if (entityDocument.IsReadOnly)
		{
			BugSubmitter.SilentAssert(!PreviewerCacheService.IsCachedEntity(entityDocument), "PreviewerCacheService is caching {0} even though it is read-only and shouldn't have changed. @summary PreviewerCacheService is caching a read-only document @assign bwhitman", entityDocument.Uri.LocalPath);
			return;
		}
		if (UndirtiedOnUndoDocuments.Remove(entityDocument))
		{
			BugSubmitter.SilentAssert(!PreviewerCacheService.IsCachedEntity(entityDocument), "PreviewerCacheService is caching {0} even though it is no longer dirty in response to an Undo statement. @summary PreviewerCacheService is caching a non-dirty document after Undo @assign bwhitman", entityDocument.Uri.LocalPath);
			return;
		}
		if (PreviewerCacheService.IsCachedEntity(entityDocument))
		{
			PreviewerCacheService.RemoveFromCache(entityDocument);
		}
		PreviewerCacheService.AddToCache(entityDocument);
	}
}
