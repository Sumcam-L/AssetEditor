using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public class TreeGridViewRowPresenter : GridViewRowPresenter
{
	public static DependencyProperty FirstColumnIndentProperty = DependencyProperty.Register("FirstColumnIndent", typeof(double), typeof(TreeGridViewRowPresenter), new PropertyMetadata(0.0));

	public static DependencyProperty ExpanderProperty = DependencyProperty.Register("Expander", typeof(UIElement), typeof(TreeGridViewRowPresenter), new FrameworkPropertyMetadata(null, OnExpanderChanged));

	private static PropertyInfo ActualIndexProperty = typeof(GridViewColumn).GetProperty("ActualIndex", BindingFlags.Instance | BindingFlags.NonPublic);

	private static PropertyInfo DesiredWidthProperty = typeof(GridViewColumn).GetProperty("DesiredWidth", BindingFlags.Instance | BindingFlags.NonPublic);

	private UIElementCollection m_children;

	public double FirstColumnIndent
	{
		get
		{
			return (double)GetValue(FirstColumnIndentProperty);
		}
		set
		{
			SetValue(FirstColumnIndentProperty, value);
		}
	}

	public UIElement Expander
	{
		get
		{
			return (UIElement)GetValue(ExpanderProperty);
		}
		set
		{
			SetValue(ExpanderProperty, value);
		}
	}

	protected override int VisualChildrenCount
	{
		get
		{
			if (Expander != null)
			{
				return base.VisualChildrenCount + 1;
			}
			return base.VisualChildrenCount;
		}
	}

	public TreeGridViewRowPresenter()
	{
		m_children = new UIElementCollection(this, this);
	}

	protected override Size ArrangeOverride(Size arrangeSize)
	{
		Size result = base.ArrangeOverride(arrangeSize);
		if (base.Columns == null || base.Columns.Count == 0)
		{
			return result;
		}
		UIElement expander = Expander;
		double num = 0.0;
		double num2 = arrangeSize.Width;
		for (int i = 0; i < base.Columns.Count; i++)
		{
			GridViewColumn gridViewColumn = base.Columns[i];
			UIElement uIElement = (UIElement)base.GetVisualChild((int)ActualIndexProperty.GetValue(gridViewColumn, null));
			double num3 = Math.Min(num2, double.IsNaN(gridViewColumn.Width) ? ((double)DesiredWidthProperty.GetValue(gridViewColumn, null)) : gridViewColumn.Width);
			if (i == 0 && expander != null)
			{
				double num4 = FirstColumnIndent + expander.DesiredSize.Width;
				uIElement.Arrange(new Rect(num + num4, 0.0, Math.Max(0.0, num3 - num4), arrangeSize.Height));
			}
			else
			{
				uIElement.Arrange(new Rect(num, 0.0, num3, arrangeSize.Height));
			}
			num2 -= num3;
			num += num3;
		}
		expander?.Arrange(new Rect(FirstColumnIndent, 0.0, expander.DesiredSize.Width, expander.DesiredSize.Height));
		return result;
	}

	protected override Size MeasureOverride(Size constraint)
	{
		Size result = base.MeasureOverride(constraint);
		UIElement expander = Expander;
		if (expander != null)
		{
			expander.Measure(constraint);
			result.Width = Math.Max(result.Width, expander.DesiredSize.Width);
			result.Height = Math.Max(result.Height, expander.DesiredSize.Height);
		}
		return result;
	}

	protected override Visual GetVisualChild(int index)
	{
		if (index < base.VisualChildrenCount)
		{
			return base.GetVisualChild(index);
		}
		return Expander;
	}

	private static void OnExpanderChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		TreeGridViewRowPresenter treeGridViewRowPresenter = (TreeGridViewRowPresenter)d;
		if (e.OldValue != null)
		{
			treeGridViewRowPresenter.m_children.Remove(e.OldValue as UIElement);
		}
		if (e.NewValue != null)
		{
			treeGridViewRowPresenter.m_children.Add((UIElement)e.NewValue);
		}
	}
}
