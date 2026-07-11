using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IPreviewerCacheService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerCacheService : IPreviewerCacheService
{
	private IDictionary<string, ICachedAsset> m_cachedEntities = new Dictionary<string, ICachedAsset>(new PathComparer());

	[Import(AllowDefault = true)]
	private IAssetPreviewer AssetPreviewer { get; set; }

	private IDocumentRegistry DocumentRegistry { get; set; }

	private IDocumentService DocumentService { get; set; }

	public event EventHandler<PreviewerCacheAdded> EntityAdded;

	public event EventHandler<PreviewerCacheRemoved> EntityRemoved;

	[ImportingConstructor]
	public PreviewerCacheService(IDocumentRegistry docReg, IDocumentService docSvc)
	{
		DocumentRegistry = docReg;
		DocumentService = docSvc;
		DocumentRegistry.DocumentAdded += DocumentRegistry_DocumentAdded;
		DocumentRegistry.DocumentRemoved += DocumentRegistry_DocumentRemoved;
		DocumentService.DocumentSaved += DocumentService_DocumentSaved;
		RegisterDocumentHandlers(DocumentRegistry.ActiveDocument);
	}

	public virtual void StartProjectChange()
	{
		ClearCache();
	}

	public virtual void FinishProjectChange()
	{
	}

	public string GetCachedXML(string localPath)
	{
		if (!m_cachedEntities.ContainsKey(localPath))
		{
			return string.Empty;
		}
		return m_cachedEntities[localPath].XMLText;
	}

	public void AddToCache(IEntityDocument entityDocument)
	{
		AddEntityDocumentImpl(entityDocument);
	}

	public bool RemoveFromCache(IEntityDocument entityDocument)
	{
		return RemoveEntityDocumentImpl(entityDocument);
	}

	public bool IsCachedEntity(IEntityDocument entDoc)
	{
		return m_cachedEntities.ContainsKey(entDoc.Uri.LocalPath);
	}

	private void RegisterDocumentHandlers(IDocument doc)
	{
		if (doc != null)
		{
			UnregisterDocumentHandlers(doc);
			doc.UriChanged += Document_UriChanged;
		}
	}

	private void UnregisterDocumentHandlers(IDocument doc)
	{
		if (doc != null)
		{
			doc.UriChanged -= Document_UriChanged;
		}
	}

	private void DocumentService_DocumentSaved(object sender, DocumentEventArgs e)
	{
		if (e.Document is IEntityDocument entDoc && IsCachedEntity(entDoc))
		{
			RemoveEntityDocumentImpl(entDoc);
		}
	}

	private void DocumentRegistry_DocumentAdded(object sender, ItemInsertedEventArgs<IDocument> e)
	{
		RegisterDocumentHandlers(e.Item);
	}

	private void DocumentRegistry_DocumentRemoved(object sender, ItemRemovedEventArgs<IDocument> e)
	{
		UnregisterDocumentHandlers(e.Item);
		if (e.Item is IEntityDocument entDoc && IsCachedEntity(entDoc))
		{
			RemoveEntityDocumentImpl(entDoc);
		}
	}

	private void Document_UriChanged(object sender, UriChangedEventArgs e)
	{
		if (m_cachedEntities.ContainsKey(e.OldUri.LocalPath))
		{
			IInstanceEntity instanceEntity = m_cachedEntities[e.OldUri.LocalPath].InstanceEntity;
			RemoveFromCacheImpl(e.OldUri);
			AddToCacheImpl(e.NewUri, instanceEntity);
		}
	}

	private void ClearCache()
	{
		foreach (ICachedAsset value in m_cachedEntities.Values)
		{
			value.Dispose();
		}
		m_cachedEntities.Clear();
	}

	private void AddEntityDocumentImpl(IEntityDocument entityDocument)
	{
		if (AssetPreviewer != null)
		{
			AddToCacheImpl(entityDocument.Uri, entityDocument.InstanceEntity);
		}
	}

	private bool RemoveEntityDocumentImpl(IEntityDocument entDoc)
	{
		if (AssetPreviewer == null)
		{
			return false;
		}
		return RemoveFromCacheImpl(entDoc.Uri);
	}

	private void AddToCacheImpl(Uri uri, IInstanceEntity entity)
	{
		BugSubmitter.SilentAssert(!m_cachedEntities.ContainsKey(uri.LocalPath), "Attempting to add duplicate cached entity for \"{0}\" to cached entities list! @summary Attempting to add duplication cached entity @assign bwhitman", uri.LocalPath);
		m_cachedEntities[uri.LocalPath] = AssetPreviewer?.CacheAsset(entity);
		this.EntityAdded.Raise(this, new PreviewerCacheAdded(uri));
	}

	private bool RemoveFromCacheImpl(Uri uri)
	{
		if (!m_cachedEntities.ContainsKey(uri.LocalPath))
		{
			BugSubmitter.SilentReport($"Attempting to remove non existent cached entity for \"{uri.LocalPath}\" from cached entities list! @summary Attempting to remove non-existent cached entity @assign bwhitman");
			return false;
		}
		m_cachedEntities[uri.LocalPath].Dispose();
		bool flag = m_cachedEntities.Remove(uri.LocalPath);
		BugSubmitter.SilentAssert(flag, "Failed to remove cached entity for \"{0}\" from cached entities list! @summary Attempting to remove cached entity @assign bwhitman", uri.LocalPath);
		this.EntityRemoved.Raise(this, new PreviewerCacheRemoved(uri));
		return flag;
	}
}
