using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;

namespace Sce.Atf.Controls.Adaptable;

public class SelectionAdapter : ControlAdapter, ISelectionAdapter, IItemDragAdapter, ISelectionPathProvider
{
	private Keys m_toggleModifierKey = Keys.Control;

	private Keys m_extendModifierKey = Keys.Shift;

	private IPickingAdapter[] m_pickingAdapters;

	private IPickingAdapter2[] m_pickingAdapters2;

	private IDragSelector m_dragSelector;

	private ISelectionContext m_selectionContext;

	private DiagramHitRecord m_hitRecord;

	private SelectionPathProviderInfo m_selectionPathProviderInfo = new SelectionPathProviderInfo();

	private Dictionary<object, AdaptablePath<object>> m_selectionPathMap = new Dictionary<object, AdaptablePath<object>>();

	public Keys ToggleModifierKey
	{
		get
		{
			return m_toggleModifierKey;
		}
		set
		{
			m_toggleModifierKey = value;
		}
	}

	public Keys ExtendModifierKey
	{
		get
		{
			return m_extendModifierKey;
		}
		set
		{
			m_extendModifierKey = value;
		}
	}

	public SelectionPathProviderInfo Info => m_selectionPathProviderInfo;

	public event EventHandler<DiagramHitEventArgs> SelectedItemHit;

	public bool RemoveSelectionPath(object item)
	{
		if (m_selectionPathMap.ContainsKey(item))
		{
			m_selectionPathMap.Remove(item);
			return true;
		}
		return false;
	}

	public void UpdateSelectionPath(object item, AdaptablePath<object> path)
	{
		if (m_selectionContext.SelectionContains(item))
		{
			if (m_selectionPathMap.ContainsKey(item))
			{
				m_selectionPathMap[item] = path;
			}
			else
			{
				m_selectionPathMap.Add(item, path);
			}
		}
	}

	protected override void Bind(AdaptableControl control)
	{
		m_pickingAdapters = control.AsAll<IPickingAdapter>().ToArray();
		Array.Reverse(m_pickingAdapters);
		m_pickingAdapters2 = control.AsAll<IPickingAdapter2>().ToArray();
		Array.Reverse(m_pickingAdapters2);
		m_dragSelector = control.As<IDragSelector>();
		if (m_dragSelector != null)
		{
			m_dragSelector.Selected += dragSelector_Selected;
		}
		control.ContextChanged += control_ContextChanged;
	}

	protected override void BindReverse(AdaptableControl control)
	{
		control.MouseUp += control_MouseUp;
		control.MouseDown += control_MouseDown;
		control.DragDrop += control_DragDrop;
	}

	protected override void Unbind(AdaptableControl control)
	{
		if (m_dragSelector != null)
		{
			m_dragSelector.Selected -= dragSelector_Selected;
		}
		control.ContextChanged -= control_ContextChanged;
		control.MouseUp -= control_MouseUp;
		control.MouseDown -= control_MouseDown;
		control.DragDrop -= control_DragDrop;
		m_pickingAdapters2 = null;
		m_pickingAdapters = null;
	}

	private void control_ContextChanged(object sender, EventArgs e)
	{
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged -= selection_Changed;
		}
		m_selectionContext = base.AdaptedControl.ContextCast<ISelectionContext>();
		if (m_selectionContext != null)
		{
			m_selectionContext.SelectionChanged += selection_Changed;
		}
		m_selectionPathProviderInfo.SelectionContext = m_selectionContext;
	}

	private void control_MouseDown(object sender, MouseEventArgs e)
	{
		if ((e.Button != MouseButtons.Left && e.Button != MouseButtons.Right) || e.Clicks != 1 || (Control.ModifierKeys & Keys.Alt) != Keys.None || base.AdaptedControl.Capture)
		{
			return;
		}
		Point p = new Point(e.X, e.Y);
		m_hitRecord = Pick(p);
		Keys modifierKeys = Control.ModifierKeys;
		if (m_hitRecord.Item != null)
		{
			object item = ((m_hitRecord.SubItem != null) ? m_hitRecord.SubItem : m_hitRecord.Item);
			bool isSelected = m_selectionContext.SelectionContains(item);
			if (UpdateSelection(item, modifierKeys, isSelected, m_hitRecord.HitPath))
			{
				this.SelectedItemHit.Raise(this, new DiagramHitEventArgs(m_hitRecord));
			}
		}
	}

	private void control_MouseUp(object sender, MouseEventArgs e)
	{
		if (!base.AdaptedControl.Capture && e.Button == MouseButtons.Left && m_hitRecord != null && m_hitRecord.Item == null)
		{
			Keys modifierKeys = Control.ModifierKeys;
			if ((modifierKeys & (m_toggleModifierKey | m_extendModifierKey)) == 0)
			{
				m_selectionContext.Clear();
			}
		}
	}

	private void control_DragDrop(object sender, DragEventArgs e)
	{
		if (m_selectionContext != null || !m_selectionContext.Selection.Any())
		{
			Point p = base.AdaptedControl.PointToClient(new Point(e.X + 20, e.Y + 20));
			m_hitRecord = Pick(p);
			if (m_hitRecord != null)
			{
				this.SelectedItemHit.Raise(this, new DiagramHitEventArgs(m_hitRecord));
			}
		}
	}

	private void dragSelector_Selected(object sender, DragSelectionEventArgs e)
	{
		List<object> list = new List<object>();
		Region region = new Region(e.Bounds);
		IPickingAdapter[] pickingAdapters = m_pickingAdapters;
		foreach (IPickingAdapter pickingAdapter in pickingAdapters)
		{
			list.AddRange(pickingAdapter.Pick(region));
		}
		region.Dispose();
		IPickingAdapter2[] pickingAdapters2 = m_pickingAdapters2;
		foreach (IPickingAdapter2 pickingAdapter2 in pickingAdapters2)
		{
			list.AddRange(pickingAdapter2.Pick(e.Bounds));
		}
		Keys modifierKeys = Control.ModifierKeys;
		if ((modifierKeys & m_toggleModifierKey) != Keys.None)
		{
			m_selectionContext.ToggleRange(list);
		}
		else if ((modifierKeys & m_extendModifierKey) != Keys.None)
		{
			m_selectionContext.AddRange(list);
		}
		else
		{
			m_selectionContext.SetRange(list);
		}
	}

	private DiagramHitRecord Pick(Point p)
	{
		DiagramHitRecord diagramHitRecord = null;
		IPickingAdapter[] pickingAdapters = m_pickingAdapters;
		foreach (IPickingAdapter pickingAdapter in pickingAdapters)
		{
			diagramHitRecord = pickingAdapter.Pick(p);
			if (diagramHitRecord.Item != null)
			{
				break;
			}
		}
		if (diagramHitRecord == null || diagramHitRecord.Item == null)
		{
			IPickingAdapter2[] pickingAdapters2 = m_pickingAdapters2;
			foreach (IPickingAdapter2 pickingAdapter2 in pickingAdapters2)
			{
				diagramHitRecord = pickingAdapter2.Pick(p);
				if (diagramHitRecord.Item != null)
				{
					break;
				}
			}
		}
		return diagramHitRecord;
	}

	private bool UpdateSelection(object item, Keys modifiers, bool isSelected, AdaptablePath<object> hitPath)
	{
		bool flag = false;
		if ((modifiers & m_toggleModifierKey) != Keys.None)
		{
			m_selectionContext.Toggle(item);
			flag = !isSelected;
		}
		else if ((modifiers & m_extendModifierKey) != Keys.None)
		{
			m_selectionContext.Add(item);
			flag = true;
		}
		else
		{
			if (isSelected)
			{
				m_selectionContext.Add(item);
			}
			else
			{
				m_selectionContext.Set(item);
			}
			flag = true;
		}
		UpdateSelectionPath(item, hitPath);
		return flag;
	}

	void IItemDragAdapter.BeginDrag(ControlAdapter initiator)
	{
	}

	void IItemDragAdapter.EndingDrag()
	{
	}

	void IItemDragAdapter.EndDrag()
	{
		if (m_hitRecord != null && m_hitRecord.DefaultPart != null)
		{
			Point p = base.AdaptedControl.PointToClient(Cursor.Position);
			DiagramHitRecord diagramHitRecord = Pick(p);
			if (diagramHitRecord.Item == m_hitRecord.Item)
			{
				this.SelectedItemHit.Raise(this, new DiagramHitEventArgs(diagramHitRecord));
			}
		}
	}

	public AdaptablePath<object> GetSelectionPath(object item)
	{
		if (item != null && m_selectionPathMap.ContainsKey(item))
		{
			return m_selectionPathMap[item];
		}
		return null;
	}

	public AdaptablePath<object> IncludedPath(object item)
	{
		foreach (KeyValuePair<object, AdaptablePath<object>> item2 in m_selectionPathMap)
		{
			if (item2.Value != null && item2.Value.IndexOf(item) >= 0)
			{
				return item2.Value;
			}
		}
		return null;
	}

	private void selection_Changed(object sender, EventArgs e)
	{
		object[] array = m_selectionPathMap.Keys.ToArray();
		foreach (object obj in array)
		{
			if (!m_selectionContext.SelectionContains(obj))
			{
				m_selectionPathMap.Remove(obj);
			}
		}
	}
}
