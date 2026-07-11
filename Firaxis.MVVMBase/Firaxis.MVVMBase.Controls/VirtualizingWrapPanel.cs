using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Firaxis.MVVMBase.Controls;

public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
{
	private const double DEFAULT_SIZE = 16.0;

	private const double DEFAULT_SCROLL_PIXELS = 16.0;

	private ItemContainerGenerator _generator;

	private int _itemsPerWrap;

	private int _wraps;

	private int _maxWraps;

	private int _maxViewportWraps;

	private Rect _viewport;

	private double _horizontalPixelOffset;

	private double _verticalPixelOffset;

	private double _childWidth = 16.0;

	private double _childHeight = 16.0;

	public static readonly DependencyProperty ScrollOwnerProperty = DependencyProperty.Register("ScrollOwner", typeof(ScrollViewer), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(null, ScrollOwnerChanged));

	public static readonly DependencyProperty ChildWidthProperty = DependencyProperty.Register("ChildWidth", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.NaN, ChildMeasurementChanged));

	public static readonly DependencyProperty ChildHeightProperty = DependencyProperty.Register("ChildHeight", typeof(double), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(double.NaN, ChildMeasurementChanged));

	public static readonly DependencyProperty WrapOrientationProperty = DependencyProperty.Register("WrapOrientation", typeof(Orientation), typeof(VirtualizingWrapPanel), new FrameworkPropertyMetadata(Orientation.Horizontal, InvalidateOnChanged));

	public double ChildWidth
	{
		get
		{
			return (double)GetValue(ChildWidthProperty);
		}
		set
		{
			SetValue(ChildWidthProperty, value);
		}
	}

	public double ChildHeight
	{
		get
		{
			return (double)GetValue(ChildHeightProperty);
		}
		set
		{
			SetValue(ChildHeightProperty, value);
		}
	}

	public Orientation WrapOrientation
	{
		get
		{
			return (Orientation)GetValue(WrapOrientationProperty);
		}
		set
		{
			SetValue(WrapOrientationProperty, value);
		}
	}

	public bool CanVerticallyScroll { get; set; }

	public bool CanHorizontallyScroll { get; set; }

	public double ExtentWidth { get; private set; }

	public double ExtentHeight { get; private set; }

	public double ViewportWidth { get; private set; }

	public double ViewportHeight { get; private set; }

	public double HorizontalOffset { get; private set; }

	public double VerticalOffset { get; private set; }

	public ScrollViewer ScrollOwner
	{
		get
		{
			return (ScrollViewer)GetValue(ScrollOwnerProperty);
		}
		set
		{
			SetValue(ScrollOwnerProperty, value);
		}
	}

	private static void ScrollOwnerChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is VirtualizingWrapPanel virtualizingWrapPanel)
		{
			virtualizingWrapPanel._generator = virtualizingWrapPanel.ItemContainerGenerator as ItemContainerGenerator;
			virtualizingWrapPanel.Invalidate();
		}
	}

	public void Invalidate()
	{
		_viewport = Rect.Empty;
		_itemsPerWrap = 0;
		_wraps = 0;
		_maxWraps = 0;
		_maxViewportWraps = 0;
		_horizontalPixelOffset = 0.0;
		_verticalPixelOffset = 0.0;
		HorizontalOffset = 0.0;
		VerticalOffset = 0.0;
		ExtentWidth = 0.0;
		ExtentHeight = 0.0;
		ViewportWidth = 0.0;
		ViewportHeight = 0.0;
		OnScrollChange();
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.SizeChanged += Resizing;
		base.OnInitialized(e);
	}

	private void Resizing(object sender, EventArgs e)
	{
		if (sender is VirtualizingWrapPanel virtualizingWrapPanel)
		{
			virtualizingWrapPanel.Invalidate();
		}
	}

	private void UpdateWraps(bool wrapHorizontally, bool byItem)
	{
		if (byItem)
		{
			if (wrapHorizontally)
			{
				_wraps = (int)VerticalOffset;
			}
			else
			{
				_wraps = (int)HorizontalOffset;
			}
		}
		else if (double.IsNaN(_childWidth))
		{
			_wraps = 0;
		}
		else if (wrapHorizontally)
		{
			_wraps = (int)(VerticalOffset / _childHeight);
		}
		else
		{
			_wraps = (int)(HorizontalOffset / _childHeight);
		}
	}

	private void GetMinMaxVisibleIndices(out int min, out int max)
	{
		GetMinMaxVisibleIndices(_generator.Items.Count, out min, out max);
	}

	private void GetMinMaxVisibleIndices(int count, out int min, out int max)
	{
		min = _itemsPerWrap * _wraps;
		max = Math.Min(min + _maxViewportWraps * _itemsPerWrap, count);
	}

	private void OnScrollChange()
	{
		InvalidateMeasure();
		ScrollOwner?.InvalidateScrollInfo();
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (_generator == null)
		{
			return availableSize;
		}
		if (double.IsNaN(_childWidth))
		{
			return availableSize;
		}
		_viewport = new Rect(_horizontalPixelOffset, _verticalPixelOffset, availableSize.Width, availableSize.Height);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		int count = _generator.Items.Count;
		if (flag)
		{
			_itemsPerWrap = (int)(_viewport.Width / _childWidth);
			_maxWraps = (int)Math.Ceiling((double)count / (double)_itemsPerWrap);
			_maxViewportWraps = (int)Math.Ceiling(_viewport.Height / _childHeight);
			ViewportWidth = _viewport.Width;
			ViewportHeight = (flag2 ? ((double)_maxViewportWraps) : _viewport.Height);
			ExtentWidth = _childWidth * (double)_itemsPerWrap;
			ExtentHeight = (flag2 ? ((double)(_maxWraps + ((_childHeight < _viewport.Height && _viewport.Height % _childHeight < _childHeight * 0.99) ? 1 : 0))) : (_childHeight * (double)_maxWraps));
		}
		else
		{
			_itemsPerWrap = (int)(_viewport.Height / _childHeight);
			_maxWraps = (int)Math.Ceiling((double)count / (double)_itemsPerWrap);
			_maxViewportWraps = (int)Math.Ceiling(_viewport.Width / _childWidth);
			ViewportWidth = (flag2 ? ((double)_maxViewportWraps) : _viewport.Width);
			ViewportHeight = _viewport.Height;
			ExtentWidth = (flag2 ? ((double)(_maxWraps + ((_childWidth < _viewport.Width && _viewport.Width % _childWidth < _childWidth * 0.99) ? 1 : 0))) : (_childWidth * (double)_maxWraps));
			ExtentHeight = _childHeight * (double)_itemsPerWrap;
		}
		if (!flag2)
		{
			_maxViewportWraps++;
		}
		Size size = new Size(_childWidth, _childHeight);
		if (count == 0)
		{
			return size;
		}
		GetMinMaxVisibleIndices(count, out var min, out var max);
		IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
		for (int num = base.InternalChildren.Count - 1; num >= 0; num--)
		{
			GeneratorPosition position = new GeneratorPosition(num, 0);
			int num2 = itemContainerGenerator.IndexFromGeneratorPosition(position);
			if (num2 < min || num2 > max)
			{
				itemContainerGenerator.Remove(position, 1);
				RemoveInternalChildRange(num, 1);
			}
		}
		using (_generator.GenerateBatches())
		{
			GeneratorPosition position2 = itemContainerGenerator.GeneratorPositionFromIndex(min);
			int num3 = ((position2.Offset == 0) ? position2.Index : (position2.Index + 1));
			using (itemContainerGenerator.StartAt(position2, GeneratorDirection.Forward, allowStartAtRealizedItem: true))
			{
				int num4 = min;
				while (num4 < max)
				{
					if (itemContainerGenerator.GenerateNext(out var isNewlyRealized) is UIElement uIElement)
					{
						if (isNewlyRealized)
						{
							if (num3 >= base.InternalChildren.Count)
							{
								AddInternalChild(uIElement);
							}
							else
							{
								InsertInternalChild(num3, uIElement);
							}
							itemContainerGenerator.PrepareItemContainer(uIElement);
						}
						uIElement.Measure(size);
					}
					num4++;
					num3++;
				}
			}
		}
		Size result = ((!flag) ? new Size((double)_maxViewportWraps * _childWidth, _childHeight * (double)Math.Min(_itemsPerWrap, count)) : new Size(_childWidth * (double)Math.Min(_itemsPerWrap, count), (double)_maxViewportWraps * _childHeight));
		if ((!CanVerticallyScroll && ScrollOwner.ScrollableHeight <= 0.01) || (!CanHorizontallyScroll && ScrollOwner.ScrollableWidth <= 0.01))
		{
			ScrollOwner?.InvalidateScrollInfo();
		}
		return result;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		int count = _generator.Items.Count;
		GetMinMaxVisibleIndices(count, out var min, out var max);
		double num = 0.0;
		double num2 = 0.0;
		if (!flag2)
		{
			if (flag)
			{
				num = _horizontalPixelOffset;
				num2 = (double)_wraps * _childHeight - _verticalPixelOffset;
			}
			else
			{
				num = (double)_wraps * _childWidth - _horizontalPixelOffset;
				num2 = _verticalPixelOffset;
			}
		}
		IItemContainerGenerator itemContainerGenerator = base.ItemContainerGenerator;
		for (int i = 0; i < base.InternalChildren.Count; i++)
		{
			UIElement uIElement = base.InternalChildren[i];
			int num3 = itemContainerGenerator.IndexFromGeneratorPosition(new GeneratorPosition(i, 0));
			if (num3 >= min && num3 <= max)
			{
				double x;
				double y;
				if (flag)
				{
					x = num + (double)(num3 % _itemsPerWrap) * _childWidth;
					y = num2 + (double)(num3 / _itemsPerWrap - _wraps) * _childHeight;
				}
				else
				{
					x = num + (double)(num3 / _itemsPerWrap - _wraps) * _childWidth;
					y = num2 + (double)(num3 % _itemsPerWrap) * _childHeight;
				}
				uIElement.Arrange(new Rect(x, y, _childWidth, _childHeight));
			}
		}
		return finalSize;
	}

	protected override void OnIsItemsHostChanged(bool oldIsItemsHost, bool newIsItemsHost)
	{
		UIElementCollection internalChildren = base.InternalChildren;
		_generator = base.ItemContainerGenerator as ItemContainerGenerator;
		OnScrollChange();
	}

	private static void InvalidateOnChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is VirtualizingWrapPanel virtualizingWrapPanel)
		{
			virtualizingWrapPanel.Invalidate();
		}
	}

	private static void ChildMeasurementChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (!(sender is VirtualizingWrapPanel virtualizingWrapPanel))
		{
			return;
		}
		virtualizingWrapPanel._childWidth = virtualizingWrapPanel.ChildWidth;
		virtualizingWrapPanel._childHeight = virtualizingWrapPanel.ChildHeight;
		if (double.IsNaN(virtualizingWrapPanel._childWidth))
		{
			if (double.IsNaN(virtualizingWrapPanel._childHeight))
			{
				virtualizingWrapPanel._childWidth = 16.0;
				virtualizingWrapPanel._childHeight = 16.0;
			}
			virtualizingWrapPanel._childWidth = virtualizingWrapPanel._childHeight;
		}
		else if (double.IsNaN(virtualizingWrapPanel._childHeight))
		{
			virtualizingWrapPanel._childHeight = virtualizingWrapPanel._childWidth;
		}
		virtualizingWrapPanel.Invalidate();
	}

	public void LineUp()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		double offset = ((!(flag && flag2)) ? (VerticalOffset - (double)num * 16.0) : ((double)((int)VerticalOffset - num)));
		SetVerticalOffset(offset, flag, flag2);
	}

	public void LineDown()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		double offset = ((!(flag && flag2)) ? (VerticalOffset + (double)num * 16.0) : ((double)((int)VerticalOffset + num)));
		SetVerticalOffset(offset, flag, flag2);
	}

	public void LineLeft()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		double offset = ((!(!flag && flag2)) ? (HorizontalOffset - (double)num * 16.0) : ((double)((int)HorizontalOffset - num)));
		SetHorizontalOffset(offset, flag, flag2);
	}

	public void LineRight()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		double offset = ((!(!flag && flag2)) ? (HorizontalOffset + (double)num * 16.0) : ((double)((int)HorizontalOffset + num)));
		SetHorizontalOffset(offset, flag, flag2);
	}

	public void PageUp()
	{
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool byItem = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		if (flag)
		{
			double offset = VerticalOffset - ViewportHeight;
			SetVerticalOffset(offset, wrapHorizontal: true, byItem);
		}
		else
		{
			double offset = HorizontalOffset - ViewportWidth;
			SetHorizontalOffset(offset, wrapHorizontal: true, byItem);
		}
	}

	public void PageDown()
	{
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool byItem = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		if (flag)
		{
			double offset = VerticalOffset + ViewportHeight;
			SetVerticalOffset(offset, wrapHorizontal: true, byItem);
		}
		else
		{
			double offset = HorizontalOffset + ViewportWidth;
			SetHorizontalOffset(offset, wrapHorizontal: true, byItem);
		}
	}

	public void PageLeft()
	{
		PageUp();
	}

	public void PageRight()
	{
		PageDown();
	}

	public void MouseWheelUp()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		if (flag)
		{
			double offset = ((!flag2) ? (VerticalOffset - (double)num * 16.0) : ((double)((int)VerticalOffset - num)));
			SetVerticalOffset(offset, wrapHorizontal: true, flag2);
		}
		else
		{
			double offset = ((!flag2) ? (HorizontalOffset - (double)num * 16.0) : ((double)((int)HorizontalOffset - num)));
			SetHorizontalOffset(offset, wrapHorizontal: false, flag2);
		}
	}

	public void MouseWheelDown()
	{
		int num = Math.Max(SystemParameters.WheelScrollLines, 1);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		if (flag)
		{
			double offset = ((!flag2) ? (VerticalOffset + (double)num * 16.0) : ((double)((int)VerticalOffset + num)));
			SetVerticalOffset(offset, wrapHorizontal: true, flag2);
		}
		else
		{
			double offset = ((!flag2) ? (HorizontalOffset + (double)num * 16.0) : ((double)((int)HorizontalOffset + num)));
			SetHorizontalOffset(offset, wrapHorizontal: false, flag2);
		}
	}

	public void MouseWheelLeft()
	{
		MouseWheelUp();
	}

	public void MouseWheelRight()
	{
		MouseWheelDown();
	}

	public void SetHorizontalOffset(double offset)
	{
		SetHorizontalOffset(offset, WrapOrientation == Orientation.Horizontal, VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item);
	}

	private void SetHorizontalOffset(double offset, bool wrapHorizontal, bool byItem, bool triggerOnScrollChange = true)
	{
		if (offset < 0.0 || ViewportWidth >= ExtentWidth)
		{
			offset = 0.0;
		}
		else if (offset + ViewportWidth >= ExtentWidth)
		{
			offset = ExtentWidth - ViewportWidth;
		}
		HorizontalOffset = offset;
		if (!wrapHorizontal)
		{
			UpdateWraps(wrapHorizontally: false, byItem);
		}
		if (!wrapHorizontal && byItem)
		{
			_horizontalPixelOffset = _childWidth * (double)_wraps;
		}
		else
		{
			_horizontalPixelOffset = offset;
		}
		if (triggerOnScrollChange)
		{
			OnScrollChange();
		}
	}

	public void SetVerticalOffset(double offset)
	{
		SetVerticalOffset(offset, WrapOrientation == Orientation.Horizontal, VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item);
	}

	private void SetVerticalOffset(double offset, bool wrapHorizontal, bool byItem, bool triggerOnScrollChange = true)
	{
		if (offset < 0.0 || ViewportHeight >= ExtentHeight)
		{
			offset = 0.0;
		}
		else if (offset + ViewportHeight >= ExtentHeight)
		{
			offset = ExtentHeight - ViewportHeight;
		}
		VerticalOffset = offset;
		if (wrapHorizontal)
		{
			UpdateWraps(wrapHorizontally: true, byItem);
		}
		if (wrapHorizontal && byItem)
		{
			_verticalPixelOffset = _childHeight * (double)_wraps;
		}
		else
		{
			_verticalPixelOffset = offset;
		}
		if (triggerOnScrollChange)
		{
			OnScrollChange();
		}
	}

	public Rect MakeVisible(Visual visual, Rect rectangle)
	{
		if (rectangle.IsEmpty || visual == null || visual == this || !IsAncestorOf(visual))
		{
			return Rect.Empty;
		}
		rectangle = visual.TransformToAncestor(this).TransformBounds(rectangle);
		Rect viewport = _viewport;
		rectangle.X += viewport.X;
		rectangle.Y += viewport.Y;
		viewport.X = CalculateNewScrollOffset(viewport.Left, viewport.Right, rectangle.Left, rectangle.Right);
		viewport.Y = CalculateNewScrollOffset(viewport.Top, viewport.Bottom, rectangle.Top, rectangle.Bottom);
		bool flag = WrapOrientation == Orientation.Horizontal;
		bool flag2 = VirtualizingPanel.GetScrollUnit(this) == ScrollUnit.Item;
		if (flag2)
		{
			if (flag)
			{
				viewport.X -= viewport.X % _childWidth;
			}
			else
			{
				viewport.Y -= viewport.Y % _childHeight;
			}
		}
		SetHorizontalOffset(viewport.X, flag, flag2, triggerOnScrollChange: false);
		SetVerticalOffset(viewport.Y, flag, flag2, triggerOnScrollChange: false);
		rectangle.Intersect(viewport);
		rectangle.X -= viewport.X;
		rectangle.Y -= viewport.Y;
		return rectangle;
	}

	private static double CalculateNewScrollOffset(double topView, double bottomView, double topChild, double bottomChild)
	{
		bool flag = topChild < topView && bottomChild < bottomView;
		bool flag2 = bottomChild > bottomView && topChild > topView;
		bool flag3 = bottomChild - topChild > bottomView - topView;
		if (!flag && !flag2)
		{
			return topView;
		}
		if ((flag && !flag3) || (flag2 && flag3))
		{
			return topChild;
		}
		return bottomChild - (bottomView - topView);
	}
}
