using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Firaxis.AssetEditing;
using Firaxis.ATF;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.CivTech.AssetPreviewer;
using Firaxis.Utility;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Dom;

namespace Firaxis.AssetPreviewing;

public class AttachmentWidgetEditor : IWidgetEditor, IDisposable
{
	public abstract class WidgetStateBase : IWidgetState, IDisposable
	{
		private AttachmentWidgetEditor m_awe;

		private Image m_image;

		private bool disposedValue = false;

		public abstract Keys HotKey { get; }

		public abstract string Name { get; }

		public abstract string Text { get; }

		public abstract string Description { get; }

		public Image Image => m_image;

		public bool CanActivate => !m_awe.IsWidgetActive(this);

		public abstract APWidgetDriver CreateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist);

		public WidgetStateBase(AttachmentWidgetEditor awe, Image img)
		{
			m_awe = awe;
			m_image = img;
		}

		public void Activate()
		{
			m_awe.ActivateWidget(this);
		}

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					m_image?.Dispose();
				}
				m_image = null;
				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(disposing: true);
		}
	}

	private class AttachmentMoveWidgetState : WidgetStateBase
	{
		public override Keys HotKey => Keys.W;

		public override string Name => "Move";

		public override string Text => "Move";

		public override string Description => "Move Widget";

		public AttachmentMoveWidgetState(AttachmentWidgetEditor awe)
			: base(awe, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.MoveWidgetIcon))
		{
		}

		public override APWidgetDriver CreateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		{
			return new AttachmentMoveDriver(flags, aplist);
		}
	}

	private class AttachmentRotateWidgetState : WidgetStateBase
	{
		public override Keys HotKey => Keys.E;

		public override string Name => "Rotate";

		public override string Text => "Rotate";

		public override string Description => "Rotate Widget";

		public AttachmentRotateWidgetState(AttachmentWidgetEditor awe)
			: base(awe, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.RotateWidgetIcon))
		{
		}

		public override APWidgetDriver CreateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		{
			return new AttachmentRotateDriver(flags, aplist);
		}
	}

	private class AttachmentScaleWidgetState : WidgetStateBase
	{
		public override Keys HotKey => Keys.R;

		public override string Name => "Scale";

		public override string Text => "Scale";

		public override string Description => "Scale Widget";

		public AttachmentScaleWidgetState(AttachmentWidgetEditor awe)
			: base(awe, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.ScaleWidgetIcon))
		{
		}

		public override APWidgetDriver CreateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		{
			return new AttachmentScaleDriver(flags, aplist);
		}
	}

	private class AttachmentSelectWidgetState : WidgetStateBase
	{
		public override Keys HotKey => Keys.Q;

		public override string Name => "Select";

		public override string Text => "Select";

		public override string Description => "Select Widget";

		public AttachmentSelectWidgetState(AttachmentWidgetEditor awe)
			: base(awe, ResourceUtil.GetImage16(Firaxis.AssetEditing.Resources.SelectWidgetIcon))
		{
		}

		public override APWidgetDriver CreateDriver(WidgetFlags flags, IEnumerable<AttachmentPointAdapter> aplist)
		{
			return null;
		}
	}

	private IPreviewWindow m_Window = null;

	private IEntityDocument m_Document = null;

	private List<IWidget> m_SelectionWidgets = new List<IWidget>();

	private List<AttachmentPointAdapter> m_Selection = new List<AttachmentPointAdapter>();

	private List<AttachmentPointAdapter> SelectionChain = new List<AttachmentPointAdapter>();

	private bool m_ShiftDown = false;

	private bool m_MultiselectUnionKeyDown = false;

	private bool m_MultiselectToggleKeyDown = false;

	private WidgetStateBase m_activeState = null;

	public bool Widget_IsReadOnly = true;

	private WidgetFlags SharedWidgetState = new WidgetFlags();

	private static Color COLOR_SELECTED;

	public static string WidgetName;

	private bool m_disposedValue = false;

	private CivTechContext CivTechFactory { get; set; }

	private AttachmentWidget ActiveWidget { get; set; }

	public AttachmentPointSetAdapter ActiveAttachmentPointSet { get; set; }

	private int NextInChain { get; set; }

	public string Name => WidgetName;

	public IEnumerable<IWidgetState> States { get; private set; }

	public event EventHandler SelectionChanged;

	public void SetSelection(IEnumerable<AttachmentPointAdapter> aplist)
	{
		m_Selection.Clear();
		m_Selection.AddRange(aplist);
		HandleSelectionChanges();
	}

	public void SetSelectionFromWidget(IEnumerable<AttachmentPointAdapter> aplist)
	{
		m_Selection.Clear();
		m_Selection.AddRange(aplist);
		SetBoneLocatorTargets(aplist);
	}

	public IEnumerable<AttachmentPointAdapter> GetSelection()
	{
		return m_Selection.AsIEnumerable<AttachmentPointAdapter>();
	}

	private void HandleSelectionChanges()
	{
		SetBoneLocatorTargets(m_Selection);
		if (m_Selection.Any())
		{
			if (ActiveWidget == null)
			{
				CreateActiveWidget();
			}
			else
			{
				ActiveWidget.RetargetWidget(m_Selection);
			}
		}
		else
		{
			ActiveWidget?.Dispose();
			ActiveWidget = null;
		}
		this.SelectionChanged?.Raise(this, EventArgs.Empty);
	}

	public AttachmentWidgetEditor()
	{
		CivTechFactory = Context.EnsureCreated<CivTechContext>();
		States = new List<IWidgetState>
		{
			new AttachmentSelectWidgetState(this),
			new AttachmentMoveWidgetState(this),
			new AttachmentRotateWidgetState(this),
			new AttachmentScaleWidgetState(this)
		};
	}

	public bool IsShiftCopyPrimed()
	{
		return m_ShiftDown;
	}

	public void SetBoneLocatorTargets(IEnumerable<AttachmentPointAdapter> aplist)
	{
		while (m_SelectionWidgets.Count > aplist.Count())
		{
			m_SelectionWidgets[m_SelectionWidgets.Count - 1]?.Dispose();
			m_SelectionWidgets.RemoveAt(m_SelectionWidgets.Count - 1);
		}
		if (!aplist.Any())
		{
			return;
		}
		BugSubmitter.SilentAssert(m_Window != null, $"Invalid PreviewWindow when creating selection widget @summary Invalid PreviewContext when creating widget @assign bwhitman");
		if (m_Window == null)
		{
			return;
		}
		int num = 0;
		foreach (AttachmentPointAdapter item in aplist)
		{
			IValueSet valueSet = CivTechFactory.CreateInstance<IValueSet>();
			valueSet.Push<IRGBValue>("Color").ParameterValue = COLOR_SELECTED;
			valueSet.Push<IStringValue>("AttachmentPoint").ParameterValue = item.Name;
			valueSet.Push<IStringValue>("Model").ParameterValue = item.ModelInstanceName;
			if (num < m_SelectionWidgets.Count)
			{
				m_SelectionWidgets[num].Alter(valueSet);
				m_SelectionWidgets[num].BoundObject = item;
			}
			else
			{
				m_SelectionWidgets.Add(m_Window.CreateWidget("BoneLocator", valueSet, item));
			}
			num++;
		}
	}

	public void SetTarget(IPreviewWindow window, IEntityDocument target)
	{
		if (m_Window == null || m_Document == null || !m_Window.Equals(window) || !m_Document.Equals(target))
		{
			m_Window = window;
			m_Document = target;
			UpdateAttachmentPointSetAdapter();
		}
	}

	public void Activate(bool isReadOnly)
	{
		Widget_IsReadOnly = isReadOnly;
		if (States.Any())
		{
			IWidgetState activeState = m_activeState;
			ActivateWidget(activeState ?? States.First());
		}
	}

	public void Deactivate()
	{
		SetBoneLocatorTargets(Enumerable.Empty<AttachmentPointAdapter>());
		ActiveWidget?.Dispose();
		ActiveWidget = null;
	}

	public void OnKeyDown(KeyEventArgs evt)
	{
		if (evt.KeyCode == Keys.ShiftKey)
		{
			m_ShiftDown = true;
		}
		else if (evt.KeyCode == Keys.D1)
		{
			m_MultiselectToggleKeyDown = true;
		}
		else if (evt.KeyCode == Keys.D2)
		{
			m_MultiselectUnionKeyDown = true;
		}
	}

	public void OnKeyUp(KeyEventArgs evt)
	{
		if (evt.KeyCode == Keys.S)
		{
			SharedWidgetState.AttachmentSnapping = !SharedWidgetState.AttachmentSnapping;
			if (SharedWidgetState.GridSnappingPrecision == 0f)
			{
				SharedWidgetState.GridSnappingPrecision = 15f;
			}
			else
			{
				SharedWidgetState.GridSnappingPrecision = 0f;
			}
			ActiveWidget?.UpdateNativeWidgetParameters();
		}
		else if (evt.KeyCode == Keys.G)
		{
			SharedWidgetState.UseWorldspace = !SharedWidgetState.UseWorldspace;
			if (ActiveWidget != null)
			{
				ActiveWidget.IsWorldSpace = SharedWidgetState.UseWorldspace;
				ActiveWidget.UpdateNativeWidgetParameters();
			}
		}
		else if (evt.KeyCode == Keys.ShiftKey)
		{
			m_ShiftDown = false;
		}
		else if (evt.KeyCode == Keys.D1)
		{
			m_MultiselectToggleKeyDown = false;
		}
		else if (evt.KeyCode == Keys.D2)
		{
			m_MultiselectUnionKeyDown = false;
		}
		else if (evt.KeyCode == Keys.Delete)
		{
			AttachmentPointSetAdapter.RemoveAttachmentPointCommandTag.DoCommand(ActiveAttachmentPointSet);
		}
	}

	public void OnMouseMove(MouseEventArgs evt)
	{
	}

	public void OnMouseDown(MouseEventArgs evt)
	{
	}

	public void OnMouseUp(MouseEventArgs evt)
	{
		if (evt.Button != MouseButtons.Left)
		{
			return;
		}
		List<AttachmentPointAdapter> list = new List<AttachmentPointAdapter>();
		IEnumerable<PickResult> enumerable = m_Window?.PickPoint(evt.X, evt.Y, 1f);
		if (enumerable != null)
		{
			List<AttachmentPickResult> list2 = new List<AttachmentPickResult>();
			list2.AddRange(enumerable.OfType<AttachmentPickResult>());
			list2.Sort((AttachmentPickResult r1, AttachmentPickResult r2) => r1.DistanceToRayImpact.CompareTo(r2.DistanceToRayImpact));
			foreach (AttachmentPickResult pick in list2)
			{
				AttachmentPointAdapter attachmentPointAdapter = ActiveAttachmentPointSet.AttachmentPoints.FirstOrDefault((AttachmentPointAdapter ap) => ap.Name == pick.Name);
				if (attachmentPointAdapter != null)
				{
					list.Add(attachmentPointAdapter);
				}
			}
		}
		if (!IsChainEquivalent(SelectionChain, list))
		{
			SelectionChain = list;
			NextInChain = 0;
			for (int num = 0; num < SelectionChain.Count; num++)
			{
				if (m_Selection.Contains(SelectionChain[num]))
				{
					NextInChain = (num + 1) % SelectionChain.Count;
				}
			}
		}
		AttachmentPointAdapter attachmentPointAdapter2 = ((SelectionChain.Count > NextInChain) ? SelectionChain[NextInChain] : null);
		NextInChain = ((SelectionChain.Count > 0) ? ((NextInChain + 1) % SelectionChain.Count) : 0);
		if (attachmentPointAdapter2 == null)
		{
			if (!IsWidgetPicked(enumerable) && !m_MultiselectUnionKeyDown && !m_MultiselectToggleKeyDown)
			{
				SetSelection(Enumerable.Empty<AttachmentPointAdapter>());
			}
		}
		else if (m_MultiselectUnionKeyDown)
		{
			if (m_Selection.Contains(attachmentPointAdapter2))
			{
				m_Selection.Remove(attachmentPointAdapter2);
			}
			m_Selection.Add(attachmentPointAdapter2);
			HandleSelectionChanges();
		}
		else if (m_MultiselectToggleKeyDown)
		{
			if (!m_Selection.Contains(attachmentPointAdapter2))
			{
				m_Selection.Add(attachmentPointAdapter2);
			}
			else
			{
				m_Selection.Remove(attachmentPointAdapter2);
			}
			HandleSelectionChanges();
		}
		else
		{
			SetSelection(new AttachmentPointAdapter[1] { attachmentPointAdapter2 });
		}
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		if (m_disposedValue)
		{
			return;
		}
		if (disposing)
		{
			ActiveWidget?.Dispose();
			foreach (IWidget selectionWidget in m_SelectionWidgets)
			{
				selectionWidget.Dispose();
			}
		}
		m_disposedValue = true;
	}

	private static bool IsChainEquivalent(List<AttachmentPointAdapter> chain, List<AttachmentPointAdapter> results)
	{
		if (results.Count != chain.Count)
		{
			return false;
		}
		for (int i = 0; i < results.Count; i++)
		{
			if (chain[i] != results[i])
			{
				return false;
			}
		}
		return true;
	}

	private AttachmentPickResult FindNearestAttachmentPick(IEnumerable<PickResult> picks)
	{
		AttachmentPickResult attachmentPickResult = null;
		foreach (AttachmentPickResult item in picks.OfType<AttachmentPickResult>())
		{
			if (attachmentPickResult == null || item.DistanceToRayImpact < attachmentPickResult.DistanceToRayImpact)
			{
				attachmentPickResult = item;
			}
		}
		return attachmentPickResult;
	}

	private bool IsWidgetPicked(IEnumerable<PickResult> picks)
	{
		WidgetPickResult widgetPickResult = null;
		foreach (WidgetPickResult item in picks.OfType<WidgetPickResult>())
		{
			if (widgetPickResult == null || item.DistanceToRayImpact < widgetPickResult.DistanceToRayImpact)
			{
				widgetPickResult = item;
			}
		}
		return widgetPickResult != null;
	}

	private void UpdateAttachmentPointSetAdapter()
	{
		if (ActiveAttachmentPointSet != null)
		{
			ActiveAttachmentPointSet.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		}
		ActiveAttachmentPointSet = (m_Document?.As<IBehaviorProviderAdapter>())?.AttachmentPointSet;
		ActiveAttachmentPointSet.DomNode.AttributeChanged += DomNode_AttributeChanged;
		BugSubmitter.SilentAssert(m_Document == null || ActiveAttachmentPointSet != null, $"No attachment point set adapter found when creating attachment widget editor for entity type {m_Document?.InstanceEntity.Type} and name {m_Document?.InstanceEntity.Name} @summary Constructed AttachmentWidgetEditor with no active AttachmentPointSetAdapter! @assign bwhitman");
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (!m_Selection.Any() || !(e.AttributeInfo.Name == "Name"))
		{
			return;
		}
		foreach (AttachmentPointAdapter item in m_Selection)
		{
			if (item.Name.Equals(e.NewValue))
			{
				SetBoneLocatorTargets(m_Selection);
				ActiveWidget?.RetargetWidget(m_Selection);
				break;
			}
		}
	}

	private bool IsWidgetActive(IWidgetState actState)
	{
		return m_activeState == actState;
	}

	private void ActivateWidget(IWidgetState actState)
	{
		if (m_activeState != actState.As<WidgetStateBase>())
		{
			m_activeState = actState.As<WidgetStateBase>();
			ActiveWidget?.Dispose();
			ActiveWidget = null;
			CreateActiveWidget();
		}
	}

	private void CreateActiveWidget()
	{
		if (!m_Selection.Any() || ActiveWidget != null)
		{
			return;
		}
		BugSubmitter.SilentAssert(m_Window != null, $"Invalid PreviewWindow when creating active widget @summary Invalid PreviewContext when creating widget @assign bwhitman");
		if (m_Window != null)
		{
			APWidgetDriver aPWidgetDriver = m_activeState.CreateDriver(SharedWidgetState, m_Selection.AsIEnumerable<AttachmentPointAdapter>());
			if (aPWidgetDriver != null)
			{
				ActiveWidget = new AttachmentWidget(this, m_Window, aPWidgetDriver, Widget_IsReadOnly, SharedWidgetState.UseWorldspace);
			}
		}
	}

	static AttachmentWidgetEditor()
	{
		COLOR_SELECTED = Color.FromArgb(255, 0, 196, 196);
		WidgetName = "Attachment";
	}
}
