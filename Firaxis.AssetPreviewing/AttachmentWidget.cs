using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.AssetEditing;
using Firaxis.AssetPreviewing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

public class AttachmentWidget : IDisposable
{
	private IEntityChangeList m_changeList;

	private IValueSet m_valueSet;

	private TransactionContext m_transactionContext;

	private bool m_disposedValue = false;

	public APWidgetDriver Driver { get; private set; }

	public bool IsReadOnly { get; set; }

	public bool IsWorldSpace { get; set; }

	private IWidget Widget { get; set; }

	private AttachmentWidgetEditor AWE { get; set; }

	private IPreviewWindow PreviewWindow { get; set; }

	public AttachmentWidget(AttachmentWidgetEditor awe, IPreviewWindow window, APWidgetDriver driver, bool isReadOnly, bool isWorldSpace)
	{
		PreviewWindow = window;
		AWE = awe;
		Driver = driver;
		IsReadOnly = isReadOnly;
		IsWorldSpace = isWorldSpace;
		BugSubmitter.Assert(PreviewWindow != null, "Attempted to create a widget for attachment point @summary Created widget when no previewer was active @assign bwhitman");
		BugSubmitter.Assert(Driver.AttachmentList.Any(), "AttachmentWidget expected nonempty list of target attachments. @assign tmaselko");
		m_transactionContext = Driver.AttachmentList.Last().DomNode.GetRoot().As<TransactionContext>();
		CivTechContext civTechContext = Context.EnsureCreated<CivTechContext>();
		m_changeList = civTechContext.CreateInstance<IEntityChangeList>();
		m_valueSet = civTechContext.CreateInstance<IValueSet>();
		m_valueSet.Push<IStringValue>("AttachmentPoint").ParameterValue = Driver.AttachmentList.Last().Name;
		m_valueSet.Push<IBoolValue>("IsReadOnly").ParameterValue = IsReadOnly;
		m_valueSet.Push<IBoolValue>("UseWorldspace").ParameterValue = IsWorldSpace;
		Driver.GetCustomArguments(m_valueSet);
		Widget = PreviewWindow.CreateWidget(Driver.GetNativeWidgetName(), m_valueSet, Driver);
		Widget.OnEdit += AttachmentWidget_OnEdit;
		Widget.OnStartEdit += AttachmentWidget_OnStartEdit;
		Widget.OnFinishEdit += AttachmentWidget_OnFinishEdit;
	}

	public void RetargetWidget(IEnumerable<AttachmentPointAdapter> newaps)
	{
		Driver.AttachmentList.Clear();
		Driver.AttachmentList.AddRange(newaps);
		BugSubmitter.Assert(Driver.AttachmentList.Any(), "AttachmentWidget expected nonempty list of target attachments. @assign tmaselko");
		m_transactionContext = Driver.AttachmentList.Last().DomNode.GetRoot().As<TransactionContext>();
		m_valueSet.Clear();
		m_valueSet.Push<IStringValue>("AttachmentPoint").ParameterValue = Driver.AttachmentList.Last().Name;
		Widget.Alter(m_valueSet);
	}

	public void UpdateNativeWidgetParameters()
	{
		m_valueSet.Clear();
		m_valueSet.Push<IBoolValue>("IsReadOnly").ParameterValue = IsReadOnly;
		m_valueSet.Push<IBoolValue>("UseWorldspace").ParameterValue = IsWorldSpace;
		Driver.GetCustomArguments(m_valueSet);
		Widget.Alter(m_valueSet);
	}

	private void AttachmentWidget_OnEdit(object sender, EventArgs e)
	{
		BugSubmitter.Assert(m_transactionContext.InTransaction, "AttachmentWidget_OnEdit triggered while not in a transaction!");
		Driver.OnWidgetEdit(m_changeList);
		if (m_changeList.EntityChanges.Any())
		{
			PreviewWindow.UpdateAsset(m_changeList.EntityChanges, 0);
			m_changeList.Clear();
		}
	}

	private void AttachmentWidget_OnStartEdit(object sender, EventArgs e)
	{
		string text = Driver.GetNativeWidgetName() + " Attachment";
		if (AWE.IsShiftCopyPrimed())
		{
			m_transactionContext.Begin("Shift-copy " + text);
			AttachmentPointSetWidgetAdapter attachmentPointSetWidgetAdapter = AWE.ActiveAttachmentPointSet.As<AttachmentPointSetWidgetAdapter>();
			using (attachmentPointSetWidgetAdapter.ScopedSelectionInFlight())
			{
				AttachmentPointSetAdapter.DuplicatedSelectedAttachmentsCommandTag.DoCommand(AWE.ActiveAttachmentPointSet);
			}
			IEntityDocument entityDocument = AWE.ActiveAttachmentPointSet.DomNode.GetRoot().As<IEntityDocument>();
			if (FiraxisATFRegistry.PreviewerCacheService != null)
			{
				if (FiraxisATFRegistry.PreviewerCacheService.IsCachedEntity(entityDocument))
				{
					FiraxisATFRegistry.PreviewerCacheService.RemoveFromCache(entityDocument);
				}
				FiraxisATFRegistry.PreviewerCacheService.AddToCache(entityDocument);
			}
			IEnumerable<AttachmentPointAdapter> enumerable = AWE.ActiveAttachmentPointSet.Selection.OfType<AttachmentPointAdapter>();
			BugSubmitter.Assert(enumerable.Any(), "Nothing selected after attachment shift-copy?");
			AWE.SetSelectionFromWidget(enumerable);
			RetargetWidget(enumerable);
			foreach (AttachmentPointAdapter item in enumerable)
			{
				m_changeList.CreateAttachmentChangedEvent(item.EntityAdapter.InstanceEntity, null, item.Name, item.ModelInstanceName, item.BoneName, item.Position, item.Orientation, item.Scale);
				foreach (IFieldValueAdapter field in item.CookParameterSet.Fields)
				{
					m_changeList.CreateAttachmentCookParameterChangedEvent(item.EntityAdapter.InstanceEntity, item.Name, field.Name, field.Value);
				}
			}
			PreviewWindow.UpdateAsset(m_changeList.EntityChanges, 0);
			m_changeList.Clear();
		}
		else
		{
			m_transactionContext.Begin(text);
		}
	}

	private void AttachmentWidget_OnFinishEdit(object sender, EventArgs e)
	{
		Driver.OnWidgetFinish();
		m_transactionContext.End();
	}

	private void AttachmentWidget_OnCancelEdit(object sender, EventArgs e)
	{
		m_transactionContext.Cancel();
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposedValue)
		{
			m_disposedValue = true;
			if (disposing)
			{
				Widget.CancelPendingEdits();
				Widget.OnEdit -= AttachmentWidget_OnEdit;
				Widget.OnStartEdit -= AttachmentWidget_OnStartEdit;
				Widget.OnFinishEdit -= AttachmentWidget_OnFinishEdit;
				Widget.OnCancelEdit -= AttachmentWidget_OnCancelEdit;
				Widget.Dispose();
			}
		}
	}
}
