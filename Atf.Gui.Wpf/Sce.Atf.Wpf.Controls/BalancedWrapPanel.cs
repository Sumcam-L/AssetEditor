using System;
using System.Windows;
using System.Windows.Controls;

namespace Sce.Atf.Wpf.Controls;

public class BalancedWrapPanel : VirtualizingWrapPanel
{
	private struct UVSize
	{
		internal double U;

		internal double V;

		private Orientation _orientation;

		internal double Width
		{
			get
			{
				return (_orientation != Orientation.Horizontal) ? V : U;
			}
			set
			{
				if (_orientation == Orientation.Horizontal)
				{
					U = value;
				}
				else
				{
					V = value;
				}
			}
		}

		internal double Height
		{
			get
			{
				return (_orientation != Orientation.Horizontal) ? U : V;
			}
			set
			{
				if (_orientation == Orientation.Horizontal)
				{
					V = value;
				}
				else
				{
					U = value;
				}
			}
		}

		internal UVSize(Orientation orientation, double width, double height)
		{
			U = (V = 0.0);
			_orientation = orientation;
			Width = width;
			Height = height;
		}

		internal UVSize(Orientation orientation)
		{
			U = (V = 0.0);
			_orientation = orientation;
		}
	}

	public static readonly DependencyProperty AlignLastItemsProperty = DependencyProperty.Register("AlignLastItems", typeof(bool), typeof(BalancedWrapPanel), new PropertyMetadata(false, OnAlignLastItemsPropertyChanged));

	public bool AlignLastItems
	{
		get
		{
			return (bool)GetValue(AlignLastItemsProperty);
		}
		set
		{
			SetValue(AlignLastItemsProperty, value);
		}
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		UVSize uVSize = new UVSize(base.Orientation);
		UVSize uVSize2 = new UVSize(base.Orientation, finalSize.Width, finalSize.Height);
		double itemWidth = base.ItemWidth;
		double itemHeight = base.ItemHeight;
		double num = 0.0;
		double num2 = ((base.Orientation == Orientation.Horizontal) ? itemWidth : itemHeight);
		bool flag = !itemWidth.IsNaN();
		bool flag2 = !itemHeight.IsNaN();
		bool flag3 = ((base.Orientation == Orientation.Horizontal) ? flag : flag2);
		double? directDelta = ((base.Orientation != Orientation.Horizontal) ? (flag2 ? new double?(itemHeight) : ((double?)null)) : (flag ? new double?(itemWidth) : ((double?)null)));
		UIElementCollection internalChildren = base.InternalChildren;
		int count = internalChildren.Count;
		int num3 = 0;
		for (int i = 0; i < count; i++)
		{
			UIElement uIElement = internalChildren[i];
			if (uIElement == null)
			{
				continue;
			}
			UVSize uVSize3 = new UVSize(base.Orientation, flag ? itemWidth : uIElement.DesiredSize.Width, flag2 ? itemHeight : uIElement.DesiredSize.Height);
			if (NumericUtil.IsGreaterThan(uVSize.U + uVSize3.U, uVSize2.U))
			{
				ArrangeLine(num3, i, directDelta, uVSize2.U, num, uVSize.V);
				num += uVSize.V;
				uVSize = uVSize3;
				if (NumericUtil.IsGreaterThan(uVSize3.U, uVSize2.U))
				{
					ArrangeLine(i, ++i, directDelta, uVSize2.U, num, uVSize3.V);
					num += uVSize3.V;
					uVSize = new UVSize(base.Orientation);
				}
				num3 = i;
			}
			else
			{
				uVSize.U += uVSize3.U;
				uVSize.V = Math.Max(uVSize3.V, uVSize.V);
			}
		}
		if (num3 < internalChildren.Count)
		{
			ArrangeLine(num3, count, directDelta, uVSize2.U, num, uVSize.V);
		}
		return finalSize;
	}

	private void ArrangeLine(int lineStart, int lineEnd, double? directDelta, double directMaximum, double indirectOffset, double indirectGrowth)
	{
		bool flag = base.Orientation == Orientation.Horizontal;
		UIElementCollection internalChildren = base.InternalChildren;
		double num = 0.0;
		double num2 = 0.0;
		double num3 = (flag ? base.ItemWidth : base.ItemHeight);
		if (AlignLastItems && !num3.IsNaN())
		{
			num2 = Math.Floor(directMaximum / num3);
			num = num2 * num3;
		}
		else
		{
			num2 = lineEnd - lineStart;
			for (int i = lineStart; i < lineEnd; i++)
			{
				UIElement uIElement = internalChildren[i];
				UVSize uVSize = new UVSize(base.Orientation, uIElement.DesiredSize.Width, uIElement.DesiredSize.Height);
				double num4 = (directDelta.HasValue ? directDelta.Value : uVSize.U);
				num += num4;
			}
		}
		double num5 = directMaximum - num;
		double num6 = num5 / (num2 + 1.0);
		double num7 = num6;
		for (int j = lineStart; j < lineEnd; j++)
		{
			UIElement uIElement2 = internalChildren[j];
			UVSize uVSize2 = new UVSize(base.Orientation, uIElement2.DesiredSize.Width, uIElement2.DesiredSize.Height);
			double num8 = (directDelta.HasValue ? directDelta.Value : uVSize2.U);
			Rect finalRect = (flag ? new Rect(num7, indirectOffset, num8, indirectGrowth) : new Rect(indirectOffset, num7, indirectGrowth, num8));
			uIElement2.Arrange(finalRect);
			num7 += num8 + num6;
		}
	}

	private static void OnAlignLastItemsPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		BalancedWrapPanel balancedWrapPanel = (BalancedWrapPanel)d;
		balancedWrapPanel.InvalidateArrange();
	}
}
