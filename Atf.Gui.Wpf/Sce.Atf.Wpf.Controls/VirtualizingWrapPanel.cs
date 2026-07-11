using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
{
	protected class ItemAbstraction
	{
		public readonly int Index;

		private int m_section = -1;

		private int m_sectionIndex = -1;

		private readonly WrapPanelAbstraction m_panel;

		public int SectionIndex
		{
			get
			{
				return (m_sectionIndex == -1) ? (Index % m_panel.AverageItemsPerSection - 1) : m_sectionIndex;
			}
			set
			{
				if (m_sectionIndex == -1)
				{
					m_sectionIndex = value;
				}
			}
		}

		public int Section
		{
			get
			{
				return (m_section == -1) ? (Index / m_panel.AverageItemsPerSection) : m_section;
			}
			set
			{
				if (m_section == -1)
				{
					m_section = value;
				}
			}
		}

		public ItemAbstraction(WrapPanelAbstraction panel, int index)
		{
			m_panel = panel;
			Index = index;
		}
	}

	protected class WrapPanelAbstraction : IEnumerable<ItemAbstraction>, IEnumerable
	{
		public readonly int ItemCount;

		public int AverageItemsPerSection;

		private int m_currentSetSection = -1;

		private int m_currentSetItemIndex = -1;

		private int m_itemsInCurrentSecction = 0;

		private readonly object m_syncRoot = new object();

		public int SectionCount
		{
			get
			{
				int num = m_currentSetSection + 1;
				if (m_currentSetItemIndex + 1 < Items.Count)
				{
					int num2 = Items.Count - m_currentSetItemIndex;
					num += num2 / AverageItemsPerSection + 1;
				}
				return num;
			}
		}

		public ItemAbstraction this[int index] => Items[index];

		private ReadOnlyCollection<ItemAbstraction> Items { get; set; }

		public WrapPanelAbstraction(int itemCount)
		{
			List<ItemAbstraction> list = new List<ItemAbstraction>(itemCount);
			for (int i = 0; i < itemCount; i++)
			{
				ItemAbstraction item = new ItemAbstraction(this, i);
				list.Add(item);
			}
			Items = new ReadOnlyCollection<ItemAbstraction>(list);
			AverageItemsPerSection = itemCount;
			ItemCount = itemCount;
		}

		public void SetItemSection(int index, int section)
		{
			lock (m_syncRoot)
			{
				if (section > m_currentSetSection + 1 || index != m_currentSetItemIndex + 1)
				{
					return;
				}
				m_currentSetItemIndex++;
				Items[index].Section = section;
				if (section == m_currentSetSection + 1)
				{
					m_currentSetSection = section;
					if (section > 0)
					{
						AverageItemsPerSection = index / section;
					}
					m_itemsInCurrentSecction = 1;
				}
				else
				{
					m_itemsInCurrentSecction++;
				}
				Items[index].SectionIndex = m_itemsInCurrentSecction - 1;
			}
		}

		public IEnumerator<ItemAbstraction> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}

	public static readonly DependencyProperty ItemHeightProperty = DependencyProperty.Register("ItemHeight", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	public static readonly DependencyProperty ItemWidthProperty = DependencyProperty.Register("ItemWidth", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.NaN, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	public static readonly DependencyProperty OrientationProperty = StackPanel.OrientationProperty.AddOwner(typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	public static readonly DependencyProperty ZoomDeltaProperty = DependencyProperty.RegisterAttached("ZoomDelta", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsMeasure | FrameworkPropertyMetadataOptions.AffectsArrange));

	private UIElementCollection m_children;

	private IItemContainerGenerator m_generator;

	private ItemsControl m_itemsControl;

	private Size m_pixelMeasuredViewport = new Size(0.0, 0.0);

	private Size m_childSize;

	private WrapPanelAbstraction m_abstractPanel;

	private Point m_offset = new Point(0.0, 0.0);

	private Size m_extent = new Size(0.0, 0.0);

	private Size m_viewport = new Size(0.0, 0.0);

	private int m_firstIndex = 0;

	private readonly Dictionary<UIElement, Rect> m_realizedChildLayout = new Dictionary<UIElement, Rect>();

	[TypeConverter(typeof(LengthConverter))]
	public double ItemHeight
	{
		get
		{
			return (double)GetValue(ItemHeightProperty);
		}
		set
		{
			SetValue(ItemHeightProperty, value);
		}
	}

	[TypeConverter(typeof(LengthConverter))]
	public double ItemWidth
	{
		get
		{
			return (double)GetValue(ItemWidthProperty);
		}
		set
		{
			SetValue(ItemWidthProperty, value);
		}
	}

	public Orientation Orientation
	{
		get
		{
			return (Orientation)GetValue(OrientationProperty);
		}
		set
		{
			SetValue(OrientationProperty, value);
		}
	}

	public double ZoomDelta
	{
		get
		{
			return (double)GetValue(ZoomDeltaProperty);
		}
		set
		{
			SetValue(ZoomDeltaProperty, value);
		}
	}

	public bool CanHorizontallyScroll { get; set; }

	public bool CanVerticallyScroll { get; set; }

	public double ExtentHeight => m_extent.Height;

	public double ExtentWidth => m_extent.Width;

	public double HorizontalOffset => m_offset.X;

	public double VerticalOffset => m_offset.Y;

	public ScrollViewer ScrollOwner { get; set; }

	public double ViewportHeight => m_viewport.Height;

	public double ViewportWidth => m_viewport.Width;

	protected Size ChildSlotSize
	{
		get
		{
			double itemWidth = ItemWidth;
			double itemHeight = ItemHeight;
			return new Size((!itemWidth.IsNaN()) ? itemWidth : double.PositiveInfinity, (!itemHeight.IsNaN()) ? itemHeight : double.PositiveInfinity);
		}
	}

	public void SetFirstRowViewItemIndex(int index)
	{
		SetVerticalOffset((double)index / Math.Floor(m_viewport.Width / m_childSize.Width));
		SetHorizontalOffset((double)index / Math.Floor(m_viewport.Height / m_childSize.Height));
	}

	public int GetFirstVisibleSection()
	{
		int val = ((Orientation == Orientation.Horizontal) ? ((int)m_offset.Y) : ((int)m_offset.X));
		return Math.Min(m_abstractPanel.Max((ItemAbstraction x) => x.Section), val);
	}

	public int GetFirstVisibleIndex()
	{
		int section = GetFirstVisibleSection();
		return m_abstractPanel.FirstOrDefault((ItemAbstraction x) => x.Section == section)?.Index ?? 0;
	}

	public void LineDown()
	{
		if (Orientation == Orientation.Vertical)
		{
			SetVerticalOffset(VerticalOffset + 20.0);
		}
		else
		{
			SetVerticalOffset(VerticalOffset + 1.0);
		}
	}

	public void LineLeft()
	{
		if (Orientation == Orientation.Horizontal)
		{
			SetHorizontalOffset(HorizontalOffset - 20.0);
		}
		else
		{
			SetHorizontalOffset(HorizontalOffset - 1.0);
		}
	}

	public void LineRight()
	{
		if (Orientation == Orientation.Horizontal)
		{
			SetHorizontalOffset(HorizontalOffset + 20.0);
		}
		else
		{
			SetHorizontalOffset(HorizontalOffset + 1.0);
		}
	}

	public void LineUp()
	{
		if (Orientation == Orientation.Vertical)
		{
			SetVerticalOffset(VerticalOffset - 20.0);
		}
		else
		{
			SetVerticalOffset(VerticalOffset - 1.0);
		}
	}

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		ItemContainerGenerator itemContainerGeneratorForPanel = m_generator.GetItemContainerGeneratorForPanel(this);
		UIElement uIElement = (UIElement)visual;
		for (int num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement); num == -1; num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement))
		{
			uIElement = (UIElement)VisualTreeHelper.GetParent(uIElement);
		}
		Rect result = m_realizedChildLayout[uIElement];
		if (Orientation == Orientation.Horizontal)
		{
			double height = m_pixelMeasuredViewport.Height;
			if (result.Bottom > height)
			{
				m_offset.Y += 1.0;
			}
			else if (result.Top < 0.0)
			{
				m_offset.Y -= 1.0;
			}
		}
		else
		{
			double width = m_pixelMeasuredViewport.Width;
			if (result.Right > width)
			{
				m_offset.X += 1.0;
			}
			else if (result.Left < 0.0)
			{
				m_offset.X -= 1.0;
			}
		}
		InvalidateMeasure();
		return result;
	}

	public void MouseWheelDown()
	{
		if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
		{
			ItemWidth -= ZoomDelta;
			ItemHeight -= ZoomDelta;
		}
		else
		{
			LineDown();
		}
	}

	public void MouseWheelLeft()
	{
		LineLeft();
	}

	public void MouseWheelRight()
	{
		LineRight();
	}

	public void MouseWheelUp()
	{
		if ((Keyboard.Modifiers & ModifierKeys.Control) == ModifierKeys.Control)
		{
			ItemWidth += ZoomDelta;
			ItemHeight += ZoomDelta;
		}
		else
		{
			LineUp();
		}
	}

	public void PageDown()
	{
		SetVerticalOffset(VerticalOffset + m_viewport.Height * 0.8);
	}

	public void PageLeft()
	{
		SetHorizontalOffset(HorizontalOffset - m_viewport.Width * 0.8);
	}

	public void PageRight()
	{
		SetHorizontalOffset(HorizontalOffset + m_viewport.Width * 0.8);
	}

	public void PageUp()
	{
		SetVerticalOffset(VerticalOffset - m_viewport.Height * 0.8);
	}

	public void SetHorizontalOffset(double offset)
	{
		if (offset < 0.0 || m_viewport.Width >= m_extent.Width)
		{
			offset = 0.0;
		}
		else if (offset + m_viewport.Width >= m_extent.Width)
		{
			offset = m_extent.Width - m_viewport.Width;
		}
		m_offset.X = offset;
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
		InvalidateMeasure();
		m_firstIndex = GetFirstVisibleIndex();
	}

	public void SetVerticalOffset(double offset)
	{
		if (offset < 0.0 || m_viewport.Height >= m_extent.Height)
		{
			offset = 0.0;
		}
		else if (offset + m_viewport.Height >= m_extent.Height)
		{
			offset = m_extent.Height - m_viewport.Height;
		}
		m_offset.Y = offset;
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
		InvalidateMeasure();
		m_firstIndex = GetFirstVisibleIndex();
	}

	protected void CleanUpItems(int minDesiredGenerated, int maxDesiredGenerated)
	{
		for (int num = m_children.Count - 1; num >= 0; num--)
		{
			GeneratorPosition position = new GeneratorPosition(num, 0);
			int num2 = m_generator.IndexFromGeneratorPosition(position);
			if (num2 < minDesiredGenerated || num2 > maxDesiredGenerated)
			{
				m_generator.Remove(position, 1);
				RemoveInternalChildRange(num, 1);
			}
		}
	}

	protected void ComputeExtentAndViewport(Size pixelMeasuredViewportSize, int visibleSections)
	{
		if (Orientation == Orientation.Horizontal)
		{
			m_viewport.Height = visibleSections;
			m_viewport.Width = pixelMeasuredViewportSize.Width;
		}
		else
		{
			m_viewport.Width = visibleSections;
			m_viewport.Height = pixelMeasuredViewportSize.Height;
		}
		if (Orientation == Orientation.Horizontal)
		{
			m_extent.Height = (double)m_abstractPanel.SectionCount + ViewportHeight - 1.0;
		}
		else
		{
			m_extent.Width = (double)m_abstractPanel.SectionCount + ViewportWidth - 1.0;
		}
		if (ScrollOwner != null)
		{
			ScrollOwner.InvalidateScrollInfo();
		}
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		switch (e.Key)
		{
		case Key.Down:
			NavigateDown();
			e.Handled = true;
			break;
		case Key.Left:
			NavigateLeft();
			e.Handled = true;
			break;
		case Key.Right:
			NavigateRight();
			e.Handled = true;
			break;
		case Key.Up:
			NavigateUp();
			e.Handled = true;
			break;
		default:
			base.OnKeyDown(e);
			break;
		}
	}

	protected override void OnItemsChanged(object sender, ItemsChangedEventArgs args)
	{
		base.OnItemsChanged(sender, args);
		m_abstractPanel = null;
		ResetScrollInfo();
		switch (args.Action)
		{
		case NotifyCollectionChangedAction.Remove:
		case NotifyCollectionChangedAction.Replace:
			RemoveInternalChildRange(args.Position.Index, args.ItemUICount);
			break;
		case NotifyCollectionChangedAction.Move:
			RemoveInternalChildRange(args.OldPosition.Index, args.ItemUICount);
			break;
		}
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.SizeChanged += Resizing;
		base.OnInitialized(e);
		m_itemsControl = ItemsControl.GetItemsOwner(this);
		m_children = base.InternalChildren;
		m_generator = base.ItemContainerGenerator;
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (m_itemsControl == null || m_itemsControl.Items.Count == 0)
		{
			return availableSize;
		}
		if (m_abstractPanel == null)
		{
			m_abstractPanel = new WrapPanelAbstraction(m_itemsControl.Items.Count);
		}
		m_pixelMeasuredViewport = availableSize;
		m_realizedChildLayout.Clear();
		Size size = availableSize;
		int count = m_itemsControl.Items.Count;
		int firstVisibleIndex = GetFirstVisibleIndex();
		GeneratorPosition position = m_generator.GeneratorPositionFromIndex(firstVisibleIndex);
		int num = ((position.Offset == 0) ? position.Index : (position.Index + 1));
		int num2 = firstVisibleIndex;
		int num3 = 1;
		using (m_generator.StartAt(position, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
		{
			bool flag = false;
			bool flag2 = Orientation == Orientation.Horizontal;
			double num4 = 0.0;
			double num5 = 0.0;
			double num6 = 0.0;
			Size childSlotSize = ChildSlotSize;
			int num7 = GetFirstVisibleSection();
			bool isNewlyRealized;
			while (num2 < count && m_generator.GenerateNext(out isNewlyRealized) is UIElement uIElement)
			{
				if (isNewlyRealized)
				{
					if (num >= m_children.Count)
					{
						AddInternalChild(uIElement);
					}
					else
					{
						InsertInternalChild(num, uIElement);
					}
					m_generator.PrepareItemContainer(uIElement);
					uIElement.Measure(childSlotSize);
				}
				else
				{
					uIElement.Measure(childSlotSize);
				}
				m_childSize = uIElement.DesiredSize;
				Rect value = new Rect(new Point(num4, num5), m_childSize);
				if (flag2)
				{
					num6 = Math.Max(num6, value.Height);
					if (value.Right > size.Width)
					{
						num5 += num6;
						num4 = 0.0;
						num6 = value.Height;
						value.X = num4;
						value.Y = num5;
						num7++;
						num3++;
					}
					if (num5 > size.Height)
					{
						flag = true;
					}
					num4 = value.Right;
				}
				else
				{
					num6 = Math.Max(num6, value.Width);
					if (value.Bottom > size.Height)
					{
						num4 += num6;
						num5 = 0.0;
						num6 = value.Width;
						value.X = num4;
						value.Y = num5;
						num7++;
						num3++;
					}
					if (num4 > size.Width)
					{
						flag = true;
					}
					num5 = value.Bottom;
				}
				m_realizedChildLayout.Add(uIElement, value);
				m_abstractPanel.SetItemSection(num2, num7);
				if (flag)
				{
					break;
				}
				num2++;
				num++;
			}
		}
		CleanUpItems(firstVisibleIndex, num2 - 1);
		ComputeExtentAndViewport(availableSize, num3);
		return availableSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		if (m_children != null)
		{
			foreach (UIElement child in m_children)
			{
				Rect finalRect = m_realizedChildLayout[child];
				child.Arrange(finalRect);
			}
		}
		return finalSize;
	}

	private void Resizing(object sender, EventArgs e)
	{
		if (m_viewport.Width > 0.0)
		{
			int firstIndex = m_firstIndex;
			m_abstractPanel = null;
			MeasureOverride(m_viewport);
			SetFirstRowViewItemIndex(m_firstIndex);
			m_firstIndex = firstIndex;
		}
	}

	private void ResetScrollInfo()
	{
		m_offset.X = 0.0;
		m_offset.Y = 0.0;
	}

	private int GetNextSectionClosestIndex(int itemIndex)
	{
		ItemAbstraction abstractItem = m_abstractPanel[itemIndex];
		if (abstractItem.Section < m_abstractPanel.SectionCount - 1)
		{
			return (from x in m_abstractPanel
				where x.Section == abstractItem.Section + 1
				orderby Math.Abs(x.SectionIndex - abstractItem.SectionIndex)
				select x).FirstOrDefault()?.Index ?? (-1);
		}
		return itemIndex;
	}

	private int GetLastSectionClosestIndex(int itemIndex)
	{
		ItemAbstraction abstractItem = m_abstractPanel[itemIndex];
		if (abstractItem.Section > 0)
		{
			return (from x in m_abstractPanel
				where x.Section == abstractItem.Section - 1
				orderby Math.Abs(x.SectionIndex - abstractItem.SectionIndex)
				select x).FirstOrDefault()?.Index ?? (-1);
		}
		return itemIndex;
	}

	private void NavigateDown()
	{
		ItemContainerGenerator itemContainerGeneratorForPanel = m_generator.GetItemContainerGeneratorForPanel(this);
		UIElement uIElement = (UIElement)Keyboard.FocusedElement;
		int num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
		int num2 = 0;
		while (num == -1)
		{
			uIElement = (UIElement)VisualTreeHelper.GetParent(uIElement);
			num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
			num2++;
		}
		DependencyObject dependencyObject = null;
		if (Orientation == Orientation.Horizontal)
		{
			int nextSectionClosestIndex = GetNextSectionClosestIndex(num);
			if (nextSectionClosestIndex < 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(nextSectionClosestIndex); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(nextSectionClosestIndex))
			{
				SetVerticalOffset(VerticalOffset + 1.0);
				UpdateLayout();
			}
		}
		else
		{
			if (num == m_abstractPanel.ItemCount - 1)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num + 1); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num + 1))
			{
				SetHorizontalOffset(HorizontalOffset + 1.0);
				UpdateLayout();
			}
		}
		while (num2 != 0)
		{
			dependencyObject = VisualTreeHelper.GetChild(dependencyObject, 0);
			num2--;
		}
		(dependencyObject as UIElement).Focus();
	}

	private void NavigateLeft()
	{
		ItemContainerGenerator itemContainerGeneratorForPanel = m_generator.GetItemContainerGeneratorForPanel(this);
		UIElement uIElement = (UIElement)Keyboard.FocusedElement;
		int num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
		int num2 = 0;
		while (num == -1)
		{
			uIElement = (UIElement)VisualTreeHelper.GetParent(uIElement);
			num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
			num2++;
		}
		DependencyObject dependencyObject = null;
		if (Orientation == Orientation.Vertical)
		{
			int lastSectionClosestIndex = GetLastSectionClosestIndex(num);
			if (lastSectionClosestIndex < 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(lastSectionClosestIndex); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(lastSectionClosestIndex))
			{
				SetHorizontalOffset(HorizontalOffset - 1.0);
				UpdateLayout();
			}
		}
		else
		{
			if (num == 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num - 1); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num - 1))
			{
				SetVerticalOffset(VerticalOffset - 1.0);
				UpdateLayout();
			}
		}
		while (num2 != 0)
		{
			dependencyObject = VisualTreeHelper.GetChild(dependencyObject, 0);
			num2--;
		}
		(dependencyObject as UIElement).Focus();
	}

	private void NavigateRight()
	{
		ItemContainerGenerator itemContainerGeneratorForPanel = m_generator.GetItemContainerGeneratorForPanel(this);
		UIElement uIElement = (UIElement)Keyboard.FocusedElement;
		int num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
		int num2 = 0;
		while (num == -1)
		{
			uIElement = (UIElement)VisualTreeHelper.GetParent(uIElement);
			num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
			num2++;
		}
		DependencyObject dependencyObject = null;
		if (Orientation == Orientation.Vertical)
		{
			int nextSectionClosestIndex = GetNextSectionClosestIndex(num);
			if (nextSectionClosestIndex < 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(nextSectionClosestIndex); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(nextSectionClosestIndex))
			{
				SetHorizontalOffset(HorizontalOffset + 1.0);
				UpdateLayout();
			}
		}
		else
		{
			if (num == m_abstractPanel.ItemCount - 1)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num + 1); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num + 1))
			{
				SetVerticalOffset(VerticalOffset + 1.0);
				UpdateLayout();
			}
		}
		while (num2 != 0)
		{
			dependencyObject = VisualTreeHelper.GetChild(dependencyObject, 0);
			num2--;
		}
		(dependencyObject as UIElement).Focus();
	}

	private void NavigateUp()
	{
		ItemContainerGenerator itemContainerGeneratorForPanel = m_generator.GetItemContainerGeneratorForPanel(this);
		UIElement uIElement = (UIElement)Keyboard.FocusedElement;
		int num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
		int num2 = 0;
		while (num == -1)
		{
			uIElement = (UIElement)VisualTreeHelper.GetParent(uIElement);
			num = itemContainerGeneratorForPanel.IndexFromContainer(uIElement);
			num2++;
		}
		DependencyObject dependencyObject = null;
		if (Orientation == Orientation.Horizontal)
		{
			int lastSectionClosestIndex = GetLastSectionClosestIndex(num);
			if (lastSectionClosestIndex < 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(lastSectionClosestIndex); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(lastSectionClosestIndex))
			{
				SetVerticalOffset(VerticalOffset - 1.0);
				UpdateLayout();
			}
		}
		else
		{
			if (num == 0)
			{
				return;
			}
			for (dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num - 1); dependencyObject == null; dependencyObject = itemContainerGeneratorForPanel.ContainerFromIndex(num - 1))
			{
				SetHorizontalOffset(HorizontalOffset - 1.0);
				UpdateLayout();
			}
		}
		while (num2 != 0)
		{
			dependencyObject = VisualTreeHelper.GetChild(dependencyObject, 0);
			num2--;
		}
		(dependencyObject as UIElement).Focus();
	}
}
