using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using Firaxis.Collections;
using Firaxis.Controls.Resources;
using Firaxis.Controls.Scrollables;
using Firaxis.Utility;

namespace Firaxis.Controls;

[ToolboxBitmap(typeof(ResourceTag), "scrollablelist.bmp")]
[Description("List that can contain custom display items")]
public class ScrollableList : ScrollUserControl
{
	private class DisplayScrollableItem
	{
		public int Origin;

		public IScrollableItem Item;

		public DisplayScrollableItem(int origin, IScrollableItem item)
		{
			Origin = origin;
			Item = item;
		}
	}

	private class DisplayScrollableItemCollection : List<DisplayScrollableItem>
	{
	}

	private ScrollableItemCollection items = new ScrollableItemCollection();

	private ScrollableItemCollection selected = new ScrollableItemCollection();

	private DisplayScrollableItemCollection display = new DisplayScrollableItemCollection();

	private bool updating = false;

	private bool notifyChange = true;

	private int suspendCount = 0;

	private int prevSelection = -1;

	private int itemHeight = 16;

	private IScrollableItem hotItem;

	private bool dragMode;

	private ScrollableItemEventArgs dragArgs;

	public int SelectedIndex => items.IndexOf(SelectedItem);

	public bool AllowUserResize { get; set; }

	public bool Multiselect { get; set; }

	public bool AllowDragSource { get; set; }

	public ScrollableItemCollection Items => items;

	public IScrollableItem SelectedItem
	{
		get
		{
			return (selected.Count > 0) ? selected[0] : null;
		}
		set
		{
			SelectItem(value);
		}
	}

	public IScrollableItem HotItem
	{
		get
		{
			return hotItem;
		}
		set
		{
			if (hotItem != value)
			{
				hotItem = value;
				Invalidate();
			}
		}
	}

	public object SelectedItemTag => (selected.Count > 0) ? selected[0].Tag : null;

	public ScrollableItemCollection SelectedItems => selected;

	[Browsable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public int ItemHeight
	{
		get
		{
			return itemHeight;
		}
		set
		{
			itemHeight = Math.Max(value, 27);
			foreach (IScrollableItem item in items)
			{
				if (item is ScrollableItemImage)
				{
					((ScrollableItemImage)item).ImageHeight = itemHeight;
				}
			}
			BeginUpdate();
			RebuildDisplayList();
			EndUpdate();
		}
	}

	public IScrollableItem FirstSelected
	{
		get
		{
			if (selected.Count > 0)
			{
				return selected[0];
			}
			return null;
		}
	}

	public event EventHandler UpdateStarted;

	public event EventHandler UpdateFinished;

	public event EventHandler SelectedChanged;

	public event EventHandler SelectedIndexChanged;

	public event EventHandler<ScrollableItemEventArgs> DragItemBegin;

	public event EventHandler<ScrollableItemEventArgs> DragItemMove;

	public event EventHandler<ScrollableItemEventArgs> DragItemEnd;

	protected void OnUpdateStarted()
	{
		this.UpdateStarted?.Invoke(this, EventArgs.Empty);
	}

	protected void OnUpdateFinished()
	{
		this.UpdateFinished?.Invoke(this, EventArgs.Empty);
	}

	public void SuspendNotify()
	{
		suspendCount++;
	}

	public void ResumeNotify()
	{
		suspendCount--;
		if (suspendCount < 0)
		{
			throw new Exception("ResumeNotify called without mathing SuspendNotify");
		}
	}

	public ScrollableList()
	{
		InitializeComponent();
		items.AddedItem += items_OnAddItem;
		items.RemovedItem += items_OnRemoveItem;
		items.ClearedItems += items_OnClear;
		selected.AddedItem += selected_OnAddItem;
		selected.RemovedItem += selected_OnRemoveItem;
		selected.ClearedItems += selected_OnClear;
	}

	public void BeginUpdate()
	{
		updating = true;
		OnUpdateStarted();
	}

	public void EndUpdate()
	{
		OnUpdateFinished();
		updating = false;
		UpdateScrollSizes();
		Invalidate();
	}

	public void SelectItem(IScrollableItem item)
	{
		selected.Clear();
		selected.Add(item);
	}

	public void RebuildDisplayList()
	{
		display.Clear();
		int num = 0;
		SizeF size = new SizeF(base.ClientSize.Width, base.ClientSize.Height);
		using (Graphics g = Graphics.FromHwnd(base.Handle))
		{
			foreach (IScrollableItem item in items)
			{
				if (item.Visible)
				{
					item.CalcLayout(g, size);
					display.Add(new DisplayScrollableItem(num, item));
					num += item.ItemHeight;
				}
			}
		}
		base.VerticalScroll.MaxValue = num;
	}

	private void items_OnClear(object sender, EventArgs e)
	{
		if (suspendCount <= 0)
		{
			RebuildDisplayList();
			selected.Clear();
			Invalidate();
		}
	}

	public bool ShiftSelected(int delta)
	{
		if (delta == 0)
		{
			return false;
		}
		foreach (IScrollableItem item in selected)
		{
			int num = GetDisplayIndex(item) + delta;
			if (num < 0 || num >= items.Count)
			{
				return false;
			}
		}
		selected.Sort((IScrollableItem a, IScrollableItem b) => (delta < 0) ? (GetDisplayIndex(a) - GetDisplayIndex(b)) : (GetDisplayIndex(b) - GetDisplayIndex(a)));
		SuspendNotify();
		foreach (IScrollableItem item2 in selected)
		{
			int num2 = items.IndexOf(item2);
			items.MoveAt(item2, num2 + delta);
		}
		ResumeNotify();
		RebuildDisplayList();
		Invalidate();
		return true;
	}

	public override void UpdateScrollSizes()
	{
		RebuildDisplayList();
		base.UpdateScrollSizes();
	}

	private void items_OnRemoveItem(object sender, ListEvent<IScrollableItem>.ListEventArgs e)
	{
		if (suspendCount <= 0)
		{
			RebuildDisplayList();
			selected.Remove(e.Item);
			Invalidate();
		}
	}

	private void items_OnAddItem(object sender, EventArgs e)
	{
		if (suspendCount <= 0)
		{
			RebuildDisplayList();
			Invalidate();
		}
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		base.AutoScaleDimensions = new System.Drawing.SizeF(6f, 13f);
		this.DoubleBuffered = true;
		base.Name = "ScrollableList";
		base.Paint += new System.Windows.Forms.PaintEventHandler(ScrollableList_Paint);
		base.SizeChanged += new System.EventHandler(ScrollableList_SizeChanged);
		base.ResumeLayout(false);
	}

	private void ScrollableList_Paint(object sender, PaintEventArgs e)
	{
		if (!updating)
		{
			PaintItems(e.Graphics);
		}
	}

	private void PaintItems(Graphics g)
	{
		int value = base.VerticalScroll.Value;
		int num = value + base.ClientSize.Height;
		foreach (DisplayScrollableItem item in display)
		{
			if (item.Origin + item.Item.ItemHeight >= value || item.Origin < num)
			{
				Rectangle r = new Rectangle(0, item.Origin - value, base.ClientSize.Width, item.Item.ItemHeight);
				PaintItem(g, item.Item, r);
			}
		}
	}

	private void PaintItem(Graphics g, IScrollableItem item, Rectangle r)
	{
		ScrollableItemState scrollableItemState = (selected.Contains(item) ? ScrollableItemState.Selected : ScrollableItemState.Normal);
		ScrollableItemStyle scrollableItemStyle = ScrollableItemStyle.Normal;
		if (HotItem == item)
		{
			scrollableItemStyle |= ScrollableItemStyle.Hot;
		}
		item.PaintItem(this, new ScrollableItemPaintEventArgs(g, r, scrollableItemState, scrollableItemStyle));
	}

	private void selected_Changed()
	{
		int selectedIndex = SelectedIndex;
		if (notifyChange && selectedIndex != prevSelection)
		{
			prevSelection = selectedIndex;
			this.SelectedChanged?.Invoke(this, EventArgs.Empty);
			this.SelectedIndexChanged?.Invoke(this, EventArgs.Empty);
			Invalidate();
		}
	}

	private void selected_OnClear(object sender, EventArgs e)
	{
		selected_Changed();
	}

	private void selected_OnRemoveItem(object sender, EventArgs e)
	{
		selected_Changed();
	}

	private void selected_OnAddItem(object sender, EventArgs e)
	{
		selected_Changed();
	}

	private void ScrollableList_SizeChanged(object sender, EventArgs e)
	{
		RebuildDisplayList();
		Invalidate();
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		if (dragMode)
		{
			dragArgs.X = e.X;
			dragArgs.Y = e.Y;
			OnDragItemMove(dragArgs);
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		if (dragMode && e.Button == MouseButtons.Left)
		{
			dragArgs.X = e.X;
			dragArgs.Y = e.Y;
			OnDragItemEnd(dragArgs);
			dragMode = false;
			base.Capture = false;
			HotItem = null;
			Invalidate();
		}
		base.OnMouseUp(e);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		Focus();
		if (e.Button != MouseButtons.Left && e.Button != MouseButtons.Right)
		{
			return;
		}
		bool ctrlPressed = KeyHelper.CtrlPressed;
		bool shiftPressed = KeyHelper.ShiftPressed;
		BeginUpdate();
		IScrollableItem firstSelected = FirstSelected;
		IScrollableItem scrollableItem = FindItem(e.X, e.Y);
		if (scrollableItem != null)
		{
			notifyChange = false;
			if (Multiselect && shiftPressed && firstSelected != null && firstSelected != scrollableItem)
			{
				selected.Clear();
				int displayIndex = GetDisplayIndex(firstSelected);
				int displayIndex2 = GetDisplayIndex(scrollableItem);
				if (displayIndex < displayIndex2)
				{
					for (int i = displayIndex; i <= displayIndex2; i++)
					{
						selected.Add(display[i].Item);
					}
				}
				else
				{
					for (int num = displayIndex; num >= displayIndex2; num--)
					{
						selected.Add(display[num].Item);
					}
				}
			}
			else if (selected.Contains(scrollableItem))
			{
				if (Multiselect)
				{
					if (ctrlPressed)
					{
						selected.Remove(scrollableItem);
					}
					else
					{
						selected.Clear();
						selected.Add(scrollableItem);
						EnsureVisible(scrollableItem);
					}
				}
			}
			else
			{
				if (!Multiselect || !ctrlPressed)
				{
					selected.Clear();
				}
				selected.Add(scrollableItem);
				EnsureVisible(scrollableItem);
			}
			notifyChange = true;
			if (e.Button == MouseButtons.Left && AllowDragSource)
			{
				dragArgs = new ScrollableItemEventArgs(scrollableItem, e.X, e.Y);
				OnDragItemBegin(dragArgs);
				base.Capture = true;
				dragMode = true;
			}
		}
		else
		{
			selected.Clear();
		}
		selected_Changed();
		EndUpdate();
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		HotItem = null;
		base.OnMouseLeave(e);
	}

	protected virtual void OnDragItemBegin(ScrollableItemEventArgs e)
	{
		this.DragItemBegin?.Invoke(this, e);
	}

	protected virtual void OnDragItemMove(ScrollableItemEventArgs e)
	{
		this.DragItemMove?.Invoke(this, e);
	}

	protected virtual void OnDragItemEnd(ScrollableItemEventArgs e)
	{
		this.DragItemEnd?.Invoke(this, e);
	}

	public IScrollableItem FindItem(int x, int y)
	{
		x += base.HorizontalScroll.Value;
		y += base.VerticalScroll.Value;
		foreach (DisplayScrollableItem item in display)
		{
			if (y >= item.Origin && y < item.Origin + item.Item.ItemHeight)
			{
				return item.Item;
			}
		}
		return null;
	}

	public void SelectItemAtPoint()
	{
		SelectItemAtPoint(PointToClient(Cursor.Position));
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		if (AllowUserResize && KeyHelper.CtrlPressed)
		{
			ItemHeight += ((e.Delta < 0) ? (-4) : 2);
		}
		base.OnMouseWheel(e);
	}

	public void SelectItemAtPoint(Point location)
	{
		IScrollableItem scrollableItem = FindItem(location.X, location.Y);
		if (scrollableItem != null)
		{
			notifyChange = false;
			selected.Clear();
			notifyChange = true;
			selected.Add(scrollableItem);
		}
	}

	public int GetDisplayIndex(IScrollableItem item)
	{
		int num = 0;
		foreach (DisplayScrollableItem item2 in display)
		{
			if (item2.Item == item)
			{
				return num;
			}
			num++;
		}
		return -1;
	}

	public void EnsureVisible(IScrollableItem item)
	{
		int displayIndex = GetDisplayIndex(item);
		if (displayIndex != -1)
		{
			DisplayScrollableItem displayScrollableItem = display[displayIndex];
			if (displayScrollableItem.Origin <= base.VerticalScroll.Value)
			{
				base.VerticalScroll.Value = displayScrollableItem.Origin;
			}
			else if (displayScrollableItem.Origin + displayScrollableItem.Item.ItemHeight >= base.VerticalScroll.Value + base.ClientSize.Height)
			{
				base.VerticalScroll.Value = displayScrollableItem.Origin + displayScrollableItem.Item.ItemHeight - base.ClientSize.Height;
			}
			Invalidate();
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		switch (keyData)
		{
		case Keys.Left:
		case Keys.Up:
		case Keys.Right:
		case Keys.Down:
			return true;
		default:
			return base.IsInputKey(keyData);
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.KeyCode)
		{
		case Keys.Home:
			if (display.Count > 0)
			{
				IScrollableItem item2 = display[0].Item;
				SelectItem(item2);
				EnsureVisible(item2);
			}
			break;
		case Keys.Down:
			if (display.Count > 0)
			{
				int displayIndex2 = GetDisplayIndex(FirstSelected);
				IScrollableItem item4 = display[(displayIndex2 != -1) ? ((displayIndex2 == display.Count - 1) ? (display.Count - 1) : (displayIndex2 + 1)) : 0].Item;
				SelectItem(item4);
				EnsureVisible(item4);
			}
			break;
		case Keys.Up:
			if (display.Count > 0)
			{
				int displayIndex = GetDisplayIndex(FirstSelected);
				IScrollableItem item3 = display[displayIndex switch
				{
					0 => 0, 
					-1 => display.Count - 1, 
					_ => displayIndex - 1, 
				}].Item;
				SelectItem(item3);
				EnsureVisible(item3);
			}
			break;
		case Keys.End:
			if (display.Count > 0)
			{
				IScrollableItem item = display[display.Count - 1].Item;
				SelectItem(item);
				EnsureVisible(item);
			}
			break;
		case Keys.Left:
		case Keys.Right:
			break;
		}
	}
}
