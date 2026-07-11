using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class AssetDocument : BaseInstanceEntityDocument, IPreviewableDocument, IDocument, IResource, IPreviewContext
{
	private IDocumentService m_documentService;

	public IDocumentService DocumentService
	{
		get
		{
			return m_documentService;
		}
		set
		{
			if (m_documentService != value)
			{
				if (m_documentService != null)
				{
					m_documentService.DocumentSaved -= ReconcileGeometry;
				}
				m_documentService = value;
				if (m_documentService != null)
				{
					m_documentService.DocumentSaved += ReconcileGeometry;
				}
			}
		}
	}

	private IAssetInstance Asset => base.InstanceEntity as IAssetInstance;

	public virtual IInstanceEntityAdapter EntityAdapter => base.DomNode.As<IInstanceEntityAdapter>();

	public IEnumerable<string> AvailablePreviewModuleNames
	{
		get
		{
			yield return PreviewModule;
		}
	}

	public IPreviewWindow PreviewWindow { get; set; }

	public string PreviewModule
	{
		get
		{
			IClassEntity classEntity = base.CivTechService.PrimaryProject.Config.Classes.FindForInstance(Asset);
			if (classEntity == null)
			{
				return string.Empty;
			}
			return classEntity.PreviewModuleName;
		}
		set
		{
		}
	}

	public event EventHandler PreviewModuleChanged;

	public virtual void RaisePreviewModuleChanged()
	{
		this.PreviewModuleChanged?.Invoke(this, EventArgs.Empty);
	}

	public void UpdateAssetModels(IGeometryInstance changedGeometry)
	{
		AssetContext assetContext = base.DomNode.As<AssetContext>();
		AssetAdapter asset = base.DomNode.As<AssetAdapter>();
		ICivTechService civTechService = assetContext.CivTechService;
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		ReconcileAssetGeometrySet(asset, changedGeometry, civTechContext, civTechService);
	}

	protected override void Dispose(bool bDisposing)
	{
		if (bDisposing)
		{
			DocumentService = null;
		}
		base.Dispose(bDisposing);
	}

	private IEnumerable<AttachmentPointAdapter> GetAffectedAttachmentPoint(string modelName, IEnumerable<AttachmentPointAdapter> attachmentPointSetData)
	{
		return attachmentPointSetData.Where((AttachmentPointAdapter atPt) => atPt.ModelInstanceName == modelName);
	}

	private void ReconcileAssetGeometrySet(AssetAdapter asset, IGeometryInstance changedGeometry, CivTechContext civTechContext, ICivTechService civTechSvc)
	{
		ModelInstanceAdapter[] array = asset.GeometrySet.ModelInstances.Where((ModelInstanceAdapter model) => model.GeoName == changedGeometry.Name).ToArray();
		if (array.Length == 0)
		{
			return;
		}
		using IInstanceSet entitySet = civTechContext.CreateInstance<IInstanceSet>(new object[1] { civTechSvc.GetActivePantryPaths() });
		ModelInstanceAdapter[] array2 = array;
		foreach (ModelInstanceAdapter referringModel in array2)
		{
			IEnumerable<IPrimGroupStateInformation> primGroupStateInformation = StaticMethods.GetPrimGroupStateInformation(referringModel.ModelInstance, civTechContext);
			IEnumerable<AttachmentPointAdapter> attachmentPoints = asset.AttachmentPointSet.AttachmentPoints.Where((AttachmentPointAdapter ap) => ap.ModelInstanceName == referringModel.Name).ToArray();
			asset.GeometrySet.RemoveModelFromAsset(referringModel.Name);
			ModelInstanceAdapter modelInstanceAdapter = asset.GeometrySet.AddModelToAsset(changedGeometry, primGroupStateInformation, entitySet);
			if (modelInstanceAdapter != null)
			{
				RemoveNonExistentAttachmentPoints(attachmentPoints, asset.AttachmentPointSet, changedGeometry, modelInstanceAdapter.Name);
			}
		}
	}

	private void ReconcileGeometry(object sender, DocumentEventArgs e)
	{
		if (!(e.Document is IEntityDocument entityDocument) || entityDocument.InstanceEntity.Type != InstanceType.IT_GEOMETRY)
		{
			return;
		}
		IGeometryInstance geo = (IGeometryInstance)entityDocument.InstanceEntity;
		if (!base.DomNode.As<AssetAdapter>().GeometrySet.ModelInstances.Where((ModelInstanceAdapter modelAdp) => modelAdp.GeoName == geo.Name).ToArray().Any())
		{
			return;
		}
		TransactionContext context = base.DomNode.As<TransactionContext>();
		using (base.DomNode.As<HistoryContext>()?.SuspendRecording())
		{
			context.DoTransaction(delegate
			{
				UpdateAssetModels(geo);
				Dirty = true;
			}, "Update Model Instance");
		}
	}

	private void RemoveNonExistentAttachmentPoints(IEnumerable<AttachmentPointAdapter> attachmentPoints, AttachmentPointSetAdapter attachmentPointSet, IGeometryInstance geometry, string modelName)
	{
		foreach (AttachmentPointAdapter attachmentPoint in attachmentPoints)
		{
			if (!geometry.HasBone(attachmentPoint.BoneName))
			{
				attachmentPointSet.RemoveAttachmentPoint(attachmentPoint.Name);
			}
		}
	}
}
