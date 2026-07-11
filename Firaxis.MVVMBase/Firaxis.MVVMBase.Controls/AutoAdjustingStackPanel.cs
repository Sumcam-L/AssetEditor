using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Firaxis.CivTech;
using Firaxis.MVVMBase.Extensions;

namespace Firaxis.MVVMBase.Controls;

public class AutoAdjustingStackPanel : Panel
{
	private class MeasureInfo
	{
		public readonly UIElement Element;

		public readonly GridLength Width;

		public readonly GridLength Height;

		public Size Constraint;

		public MeasureInfo(UIElement element, GridLength width, GridLength height, Size constraint)
		{
			Element = element;
			Width = width;
			Height = height;
			Constraint = constraint;
		}

		public bool CanDoInitialMeasure(bool isHorizontal)
		{
			return (isHorizontal && (!Width.IsStar || double.IsNaN(Constraint.Width))) || (!isHorizontal && (!Height.IsStar || double.IsNaN(Constraint.Height)));
		}
	}

	private static readonly DependencyPropertyKey IsHorizontalPropertyKey = DependencyProperty.RegisterReadOnly("IsHorizontal", typeof(bool), typeof(AutoAdjustingStackPanel), new FrameworkPropertyMetadata(true));

	public static readonly DependencyProperty IsHorizontalProperty = IsHorizontalPropertyKey.DependencyProperty;

	public static readonly DependencyProperty ChildWidthProperty = DependencyProperty.RegisterAttached("ChildWidth", typeof(GridLength), typeof(AutoAdjustingStackPanel), new PropertyMetadata(GridLength.Auto, ChildWidthOrHeightChanged));

	public static readonly DependencyProperty ChildHeightProperty = DependencyProperty.RegisterAttached("ChildHeight", typeof(GridLength), typeof(AutoAdjustingStackPanel), new PropertyMetadata(GridLength.Auto, ChildWidthOrHeightChanged));

	private static readonly DependencyPropertyKey AdjustedChildWidthPropertyKey = DependencyProperty.RegisterAttachedReadOnly("AdjustedChildWidth", typeof(double), typeof(AutoAdjustingStackPanel), new PropertyMetadata(0.0));

	public static readonly DependencyProperty AdjustedChildWidthProperty = AdjustedChildWidthPropertyKey.DependencyProperty;

	private static readonly DependencyPropertyKey AdjustedChildHeightPropertyKey = DependencyProperty.RegisterAttachedReadOnly("AdjustedChildHeight", typeof(double), typeof(AutoAdjustingStackPanel), new PropertyMetadata(0.0));

	public static readonly DependencyProperty AdjustedChildHeightProperty = AdjustedChildHeightPropertyKey.DependencyProperty;

	public bool IsHorizontal
	{
		get
		{
			return (bool)GetValue(IsHorizontalProperty);
		}
		private set
		{
			SetValue(IsHorizontalPropertyKey, value);
		}
	}

	public static GridLength GetChildWidth(UIElement target)
	{
		return (GridLength)target.GetValue(ChildWidthProperty);
	}

	public static void SetChildWidth(UIElement target, GridLength value)
	{
		target.SetValue(ChildWidthProperty, value);
	}

	private static void ChildWidthOrHeightChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is UIElement original)
		{
			AutoAdjustingStackPanel visualTreeAncestorByType = original.GetVisualTreeAncestorByType<AutoAdjustingStackPanel>();
			visualTreeAncestorByType.InvalidateMeasure();
		}
	}

	public static GridLength GetChildHeight(UIElement target)
	{
		return (GridLength)target.GetValue(ChildHeightProperty);
	}

	public static void SetChildHeight(UIElement target, GridLength value)
	{
		target.SetValue(ChildHeightProperty, value);
	}

	public static double GetAdjustedChildWidth(UIElement target)
	{
		return (double)target.GetValue(AdjustedChildWidthProperty);
	}

	private static void SetAdjustedChildWidth(UIElement target, double value)
	{
		target.SetValue(AdjustedChildWidthPropertyKey, value);
	}

	public static double GetAdjustedChildHeight(UIElement target)
	{
		return (double)target.GetValue(AdjustedChildHeightProperty);
	}

	private static void SetAdjustedChildHeight(UIElement target, double value)
	{
		target.SetValue(AdjustedChildHeightPropertyKey, value);
	}

	public void ResizeSurroundingSplitter(AutoAdjustingStackPanelSplitter autoAdjustingStackPanelSplitter, double horizontalChange, double verticalChange)
	{
		int num = base.Children.IndexOf(autoAdjustingStackPanelSplitter);
		if (IsHorizontal)
		{
			double num2 = 0.0;
			double num3 = 0.0;
			foreach (FrameworkElement item in base.Children.OfType<FrameworkElement>())
			{
				GridLength childWidth = GetChildWidth(item);
				if (childWidth.IsStar)
				{
					num2 += childWidth.Value;
					num3 += item.ActualWidth;
				}
			}
			for (int i = 0; i < base.Children.Count; i++)
			{
				UIElement target = base.Children[i];
				GridLength childWidth2 = GetChildWidth(target);
				if (childWidth2.IsAbsolute)
				{
					if (i == num - 1)
					{
						SetChildWidth(target, new GridLength(childWidth2.Value + horizontalChange, GridUnitType.Pixel));
					}
					else if (i == num + 1)
					{
						SetChildWidth(target, new GridLength(childWidth2.Value - horizontalChange, GridUnitType.Pixel));
					}
				}
				else if (childWidth2.IsStar)
				{
					double num4 = childWidth2.Value / num2;
					double num5 = num4 * num3;
					if (i == num - 1)
					{
						num5 += horizontalChange;
					}
					else if (i == num + 1)
					{
						num5 -= horizontalChange;
					}
					SetChildWidth(target, new GridLength(num5, GridUnitType.Star));
				}
			}
		}
		else
		{
			double num6 = 0.0;
			double num7 = 0.0;
			foreach (FrameworkElement item2 in base.Children.OfType<FrameworkElement>())
			{
				GridLength childHeight = GetChildHeight(item2);
				if (childHeight.IsStar)
				{
					num6 += childHeight.Value;
					num7 += item2.ActualHeight;
				}
			}
			for (int j = 0; j < base.Children.Count; j++)
			{
				UIElement target2 = base.Children[j];
				GridLength childHeight2 = GetChildHeight(target2);
				if (childHeight2.IsAbsolute)
				{
					if (j == num - 1)
					{
						SetChildHeight(target2, new GridLength(childHeight2.Value + verticalChange, GridUnitType.Pixel));
					}
					else if (j == num + 1)
					{
						SetChildHeight(target2, new GridLength(childHeight2.Value - verticalChange, GridUnitType.Pixel));
					}
				}
				else if (childHeight2.IsStar)
				{
					double num8 = childHeight2.Value / num6;
					double num9 = num8 * num7;
					if (j == num - 1)
					{
						num9 += verticalChange;
					}
					else if (j == num + 1)
					{
						num9 -= verticalChange;
					}
					SetChildHeight(target2, new GridLength(num9, GridUnitType.Star));
				}
			}
		}
		InvalidateMeasure();
	}

	public void GetMinMaxDrag(AutoAdjustingStackPanelSplitter splitter, out double min, out double max)
	{
		min = 0.0;
		max = 0.0;
		int num = base.Children.IndexOf(splitter);
		if (num >= 0)
		{
			if (num > 0)
			{
				min = 0.0 - (IsHorizontal ? GetAdjustedChildWidth(base.Children[num - 1]) : GetAdjustedChildHeight(base.Children[num - 1]));
			}
			if (num + 1 < base.Children.Count)
			{
				max = (IsHorizontal ? GetAdjustedChildWidth(base.Children[num + 1]) : GetAdjustedChildHeight(base.Children[num + 1]));
			}
		}
	}

	protected override Size MeasureOverride(Size availableSize)
	{
		if (base.Children.Count == 0)
		{
			return availableSize;
		}
		bool flag = (IsHorizontal = availableSize.Width >= availableSize.Height);
		double num = 0.0;
		double num2 = (flag ? availableSize.Width : availableSize.Height);
		List<MeasureInfo> list = new List<MeasureInfo>(base.Children.Count);
		foreach (UIElement item in base.Children.OfType<UIElement>())
		{
			MeasureInfo measureInfo = new MeasureInfo(item, GetChildWidth(item), GetChildHeight(item), availableSize);
			if (measureInfo.Width.IsAbsolute)
			{
				measureInfo.Constraint.Width = measureInfo.Width.Value;
			}
			else if (flag && measureInfo.Width.IsStar && !double.IsNaN(availableSize.Width))
			{
				num += measureInfo.Width.Value;
			}
			if (measureInfo.Height.IsAbsolute)
			{
				measureInfo.Constraint.Height = measureInfo.Height.Value;
			}
			else if (!flag && measureInfo.Height.IsStar && !double.IsNaN(availableSize.Height))
			{
				num += measureInfo.Height.Value;
			}
			if (measureInfo.CanDoInitialMeasure(flag))
			{
				measureInfo.Element.Measure(measureInfo.Constraint);
				num2 -= (flag ? measureInfo.Element.DesiredSize.Width : measureInfo.Element.DesiredSize.Height);
			}
			list.Add(measureInfo);
			(item as AutoAdjustingStackPanelSplitter)?.UpdateCursor();
		}
		Size result = default(Size);
		foreach (MeasureInfo item2 in list)
		{
			if (!item2.CanDoInitialMeasure(flag))
			{
				if (flag)
				{
					item2.Constraint.Width = num2 / num * item2.Width.Value;
				}
				else
				{
					item2.Constraint.Height = num2 / num * item2.Height.Value;
				}
				item2.Element.Measure(item2.Constraint);
			}
			double num3 = ((!item2.Width.IsAuto) ? item2.Constraint.Width : item2.Element.DesiredSize.Width);
			if (num3 < 0.0)
			{
				BugSubmitter.SilentReport("Width < 0:\n" + $"measureInfo.Width.IsAuto = {item2.Width.IsAuto}\n" + $"measureInfo.Element.DesiredSize.Width = {item2.Element.DesiredSize.Width}\n" + $"isHorizontal = {flag} " + "@assign matthew.kelley @summary AutoAdjustingStackPanel MeasureOverride received invalid size information");
				num3 = 0.0;
			}
			SetAdjustedChildWidth(item2.Element, num3);
			double num4 = ((!item2.Height.IsAuto) ? item2.Constraint.Height : item2.Element.DesiredSize.Height);
			if (num4 < 0.0)
			{
				BugSubmitter.SilentReport("Height < 0:\n" + $"measureInfo.Height.IsAuto = {item2.Height.IsAuto}\n" + $"measureInfo.Element.DesiredSize.Height = {item2.Element.DesiredSize.Height}\n" + $"isHorizontal = {flag} " + "@assign matthew.kelley @summary AutoAdjustingStackPanel MeasureOverride received invalid size information");
				num4 = 0.0;
			}
			SetAdjustedChildHeight(item2.Element, num4);
			if (flag)
			{
				result.Width += num3;
				result.Height = Math.Max(result.Height, num4);
			}
			else
			{
				result.Width = Math.Max(result.Width, num3);
				result.Height += num4;
			}
		}
		return result;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		bool isHorizontal = IsHorizontal;
		double num = 0.0;
		Rect finalRect = new Rect(finalSize);
		for (int i = 0; i < base.Children.Count; i++)
		{
			UIElement uIElement = base.Children[i];
			double adjustedChildWidth = GetAdjustedChildWidth(uIElement);
			double adjustedChildHeight = GetAdjustedChildHeight(uIElement);
			if (isHorizontal)
			{
				finalRect.X += num;
				double num2 = (finalRect.Width = adjustedChildWidth);
				num = num2;
				finalRect.Height = Math.Max(finalSize.Height, adjustedChildHeight);
			}
			else
			{
				finalRect.Y += num;
				double num2 = (finalRect.Height = adjustedChildHeight);
				num = num2;
				finalRect.Width = Math.Max(finalSize.Width, adjustedChildWidth);
			}
			uIElement.Arrange(finalRect);
		}
		return finalSize;
	}
}
