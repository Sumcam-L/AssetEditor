using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Firaxis.AssetPreviewing;

[Export(typeof(IPreviewerWidgetService))]
[Export(typeof(PreviewerWidgetService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class PreviewerWidgetService : IPreviewerWidgetService, IDisposable
{
	private IList<IWidgetEditor> WidgetEditors = new List<IWidgetEditor>();

	private IDictionary<EntityID, string> m_lastActiveEditor = new Dictionary<EntityID, string>();

	private bool m_disposedValue = false;

	private MouseEventArgs m_startDragEvt = null;

	private bool m_performingDrag = false;

	[Import(AllowDefault = false)]
	private IPreviewerControlHost PreviewerControlHost { get; set; }

	[Import(AllowDefault = false)]
	private IPreviewerWidgetHost PreviewerWidgetHost { get; set; }

	private IPreviewWindow PreviewWindow { get; set; }

	private IEntityDocument TargetDocument { get; set; }

	private IControlHostService ControlHostService { get; set; }

	private Control PreviewControl { get; set; }

	public virtual IWidgetEditor ActiveWidgetEditor { get; private set; }

	public IEnumerable<string> AvailableWidgetEditors => WidgetEditors.Select((IWidgetEditor editor) => editor.Name);

	public event EventHandler ActiveWidgetChanged;

	[ImportingConstructor]
	public PreviewerWidgetService(IControlHostService ctlHostSvc)
	{
		ControlHostService = ctlHostSvc;
		RegisterPersistentHandlers();
	}

	public void SetActiveWidgetEditor(string editorName)
	{
		string text = ActiveWidgetEditor?.Name;
		if (text != editorName)
		{
			ActiveWidgetEditor?.Deactivate();
			ActiveWidgetEditor = null;
		}
		IWidgetEditor widgetEditor = WidgetEditors.FirstOrDefault((IWidgetEditor ed) => ed.Name == editorName);
		BugSubmitter.SilentAssert(widgetEditor != null, $"Invalid widget editor \"{editorName}\" specified! ");
		if (widgetEditor != null)
		{
			ActiveWidgetEditor = widgetEditor;
			ActiveWidgetEditor.SetTarget(PreviewWindow, TargetDocument);
			ActiveWidgetEditor.Activate(TargetDocument.IsReadOnly);
		}
		PreviewerWidgetHost.SetActiveWidget(ActiveWidgetEditor);
		if (text != editorName)
		{
			this.ActiveWidgetChanged.Raise(this, EventArgs.Empty);
		}
	}

	public IWidgetEditor FindWidgetEditor(string name)
	{
		return WidgetEditors.FirstOrDefault((IWidgetEditor editor) => editor.Name == name);
	}

	public void StartProjectChange()
	{
		StoreLastActiveEditor(TargetDocument);
	}

	public void FinishProjectChange()
	{
		RestoreLastActiveEditor(TargetDocument);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (!m_disposedValue)
		{
			if (disposing)
			{
				UnregisterMessageHandlers();
				UnregisterWidgetEditors();
			}
			m_disposedValue = true;
		}
	}

	private void RegisterPersistentHandlers()
	{
		UnregisterPersistentHandlers();
		ControlHostService.ControlVisibilityChanged += ControlHostService_ControlVisibilityChanged;
	}

	private void UnregisterPersistentHandlers()
	{
		ControlHostService.ControlVisibilityChanged -= ControlHostService_ControlVisibilityChanged;
	}

	private void RegisterWidgetEditors()
	{
		if (TargetDocument == null)
		{
			return;
		}
		if (TargetDocument.Is<TextureEntityAdapter>())
		{
			TextureWidgetEditor twe = new TextureWidgetEditor();
			twe.SetTarget(PreviewWindow, TargetDocument);
			WidgetEditors.Add(twe);
			PreviewerWidgetHost.AddWidget(twe, twe.Name, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.SplineCategoryIcon), delegate
			{
				SetActiveWidgetEditor(twe.Name);
			});
		}
		if (TargetDocument.Is<IBehaviorProviderAdapter>())
		{
			AttachmentWidgetEditor awe = new AttachmentWidgetEditor();
			awe.SetTarget(PreviewWindow, TargetDocument);
			WidgetEditors.Add(awe);
			PreviewerWidgetHost.AddWidget(awe, awe.Name.Localize(), ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.AttachmentsCategoryIcon), delegate
			{
				SetActiveWidgetEditor(awe.Name);
			});
			SplineWidgetEditor swe = new SplineWidgetEditor();
			swe.SetTarget(PreviewWindow, TargetDocument);
			WidgetEditors.Add(swe);
			PreviewerWidgetHost.AddWidget(swe, swe.Name, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.SplineCategoryIcon), delegate
			{
				SetActiveWidgetEditor(swe.Name);
			});
		}
	}

	private void UnregisterWidgetEditors()
	{
		foreach (IWidgetEditor widgetEditor in WidgetEditors)
		{
			PreviewerWidgetHost.RemoveWidget(widgetEditor);
			widgetEditor?.Dispose();
		}
		WidgetEditors.Clear();
	}

	private void ControlHostService_ControlVisibilityChanged(object sender, EventArgs e)
	{
		if (PreviewerControlHost.PreviewerControl != null)
		{
			UnregisterMessageHandlers();
			RegisterMessageHandlers();
		}
	}

	private void StoreLastActiveEditor(IEntityDocument doc)
	{
		if (doc != null && ActiveWidgetEditor != null)
		{
			EntityID key = new EntityID(doc.InstanceEntity.Name, doc.InstanceEntity.Type);
			m_lastActiveEditor[key] = ActiveWidgetEditor.Name;
			ActiveWidgetEditor.Dispose();
			ActiveWidgetEditor = null;
		}
	}

	private void RestoreLastActiveEditor(IEntityDocument doc)
	{
		if (doc != null && (doc.Is<IBehaviorProviderAdapter>() || doc.Is<TextureEntityAdapter>()))
		{
			string activeWidgetEditor = NullWidgetEditor.WidgetName;
			if (WidgetEditors.FirstOrDefault() != null)
			{
				activeWidgetEditor = WidgetEditors.FirstOrDefault().Name;
			}
			EntityID key = new EntityID(doc.InstanceEntity.Name, doc.InstanceEntity.Type);
			if (m_lastActiveEditor.ContainsKey(key))
			{
				activeWidgetEditor = m_lastActiveEditor[key];
			}
			SetActiveWidgetEditor(activeWidgetEditor);
		}
	}

	private void PreviewControl_KeyDown(object sender, KeyEventArgs e)
	{
		ActiveWidgetEditor?.OnKeyDown(e);
	}

	private void PreviewControl_KeyUp(object sender, KeyEventArgs e)
	{
		ActiveWidgetEditor?.OnKeyUp(e);
	}

	private void PreviewControl_MouseDown(object sender, MouseEventArgs e)
	{
		m_performingDrag = false;
		m_startDragEvt = e;
		ActiveWidgetEditor?.OnMouseDown(e);
	}

	private void PreviewControl_MouseMove(object sender, MouseEventArgs e)
	{
		if (m_startDragEvt != null && e.Button != MouseButtons.None && (Math.Abs(m_startDragEvt.X - e.X) > 5 || Math.Abs(m_startDragEvt.Y - e.Y) > 5))
		{
			m_performingDrag = true;
			m_startDragEvt = null;
		}
		ActiveWidgetEditor?.OnMouseMove(e);
	}

	private void PreviewControl_MouseUp(object sender, MouseEventArgs e)
	{
		if (!m_performingDrag)
		{
			ActiveWidgetEditor?.OnMouseUp(e);
		}
		m_startDragEvt = null;
		m_performingDrag = false;
	}

	public void ClearIfActive(IPreviewWindow wnd, IEntityDocument doc)
	{
		if (wnd != null && doc != null && PreviewWindow == wnd && doc.Equals(TargetDocument))
		{
			SetActiveEntity(null, null);
		}
	}

	public void SetActiveEntity(IPreviewWindow wnd, IEntityDocument doc)
	{
		StoreLastActiveEditor(TargetDocument);
		UnregisterMessageHandlers();
		UnregisterWidgetEditors();
		PreviewWindow = wnd;
		TargetDocument = doc;
		RegisterWidgetEditors();
		RegisterMessageHandlers();
		RestoreLastActiveEditor(TargetDocument);
	}

	private void RegisterMessageHandlers()
	{
		UnregisterMessageHandlers();
		PreviewerControlHost.PreviewerControl.MouseDown += PreviewControl_MouseDown;
		PreviewerControlHost.PreviewerControl.MouseMove += PreviewControl_MouseMove;
		PreviewerControlHost.PreviewerControl.MouseUp += PreviewControl_MouseUp;
		PreviewerControlHost.PreviewerControl.KeyDown += PreviewControl_KeyDown;
		PreviewerControlHost.PreviewerControl.KeyUp += PreviewControl_KeyUp;
	}

	private void UnregisterMessageHandlers()
	{
		if (PreviewerControlHost.PreviewerControl != null)
		{
			PreviewerControlHost.PreviewerControl.MouseDown -= PreviewControl_MouseDown;
			PreviewerControlHost.PreviewerControl.MouseMove -= PreviewControl_MouseMove;
			PreviewerControlHost.PreviewerControl.MouseUp -= PreviewControl_MouseUp;
			PreviewerControlHost.PreviewerControl.KeyDown -= PreviewControl_KeyDown;
			PreviewerControlHost.PreviewerControl.KeyUp -= PreviewControl_KeyUp;
		}
	}
}
