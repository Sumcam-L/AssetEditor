using System;
using System.Collections.Generic;
using System.Linq;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

public class AttachmentPointSetWidgetAdapter : BehaviorComponentAdapterBase
{
	private enum SelectionChangeSource
	{
		kNone,
		kWidgetInitiated
	}

	private class SelectionSuspender : IDisposable
	{
		private AttachmentPointSetWidgetAdapter Owner;

		public SelectionSuspender(AttachmentPointSetWidgetAdapter owner, SelectionChangeSource src)
		{
			Owner = owner;
			BugSubmitter.Assert(Owner.m_selectionInProgress == SelectionChangeSource.kNone, "Re-entrant selection suspension should not be possible");
			Owner.m_selectionInProgress = src;
		}

		public void Dispose()
		{
			Owner.m_selectionInProgress = SelectionChangeSource.kNone;
		}
	}

	private SelectionChangeSource m_selectionInProgress = SelectionChangeSource.kNone;

	private ISelectionContext AttachmentPointSelection { get; set; }

	private IPreviewerWidgetService PreviewerWidgetService { get; set; }

	private AttachmentWidgetEditor AttachmentWidgetEditor { get; set; }

	protected override void OnParentNodeSet()
	{
		base.OnParentNodeSet();
		AttachmentPointSelection = base.DomNode.As<ISelectionContext>();
		if (AttachmentPointSelection != null)
		{
			AttachmentPointSelection.SelectionChanging += AttachmentPointSet_SelectionChanging;
			AttachmentPointSelection.SelectionChanged += AttachmentPointSet_SelectionChanged;
		}
		PreviewerWidgetService = FiraxisATFRegistry.PreviewerWidgetService;
		if (PreviewerWidgetService != null)
		{
			PreviewerWidgetService.ActiveWidgetChanged += PreviewerWidgetService_ActiveWidgetChanged;
			PreviewerWidgetService_ActiveWidgetChanged(null, null);
		}
	}

	private void PreviewerWidgetService_ActiveWidgetChanged(object sender, EventArgs e)
	{
		if (AttachmentWidgetEditor != null)
		{
			AttachmentWidgetEditor.SelectionChanged -= WidgetEditor_SelectionChanged;
		}
		AttachmentWidgetEditor = PreviewerWidgetService.FindWidgetEditor(AttachmentWidgetEditor.WidgetName) as AttachmentWidgetEditor;
		if (AttachmentWidgetEditor != null)
		{
			AttachmentWidgetEditor.SelectionChanged += WidgetEditor_SelectionChanged;
		}
	}

	private void AttachmentPointSet_SelectionChanging(object sender, EventArgs e)
	{
		if (AttachmentWidgetEditor == null || IsSelectionInProgress(SelectionChangeSource.kWidgetInitiated))
		{
			return;
		}
		using (ScopedSelectionInFlight(SelectionChangeSource.kWidgetInitiated))
		{
			AttachmentWidgetEditor.SetSelection(Enumerable.Empty<AttachmentPointAdapter>());
		}
	}

	private void AttachmentPointSet_SelectionChanged(object sender, EventArgs e)
	{
		if (AttachmentWidgetEditor == null)
		{
			return;
		}
		IEnumerable<AttachmentPointAdapter> enumerable = AttachmentPointSelection?.Selection?.OfType<AttachmentPointAdapter>();
		if (!enumerable.Any() || IsSelectionInProgress(SelectionChangeSource.kWidgetInitiated))
		{
			return;
		}
		PreviewerWidgetService?.SetActiveWidgetEditor(AttachmentWidgetEditor.WidgetName);
		using (ScopedSelectionInFlight(SelectionChangeSource.kWidgetInitiated))
		{
			AttachmentWidgetEditor.SetSelection(enumerable);
		}
	}

	private void WidgetEditor_SelectionChanged(object sender, EventArgs e)
	{
		if (IsSelectionInProgress(SelectionChangeSource.kWidgetInitiated))
		{
			return;
		}
		using (ScopedSelectionInFlight(SelectionChangeSource.kWidgetInitiated))
		{
			AttachmentPointSelection?.SetRange(AttachmentWidgetEditor.GetSelection());
		}
	}

	private IDisposable ScopedSelectionInFlight(SelectionChangeSource src)
	{
		return new SelectionSuspender(this, src);
	}

	private bool IsSelectionInProgress(SelectionChangeSource src)
	{
		return m_selectionInProgress == src;
	}

	public IDisposable ScopedSelectionInFlight()
	{
		return new SelectionSuspender(this, SelectionChangeSource.kWidgetInitiated);
	}
}
