using System;
using System.Windows;

namespace Sce.Atf.Wpf.Behaviors;

public class ThemeStyleBehavior
{
	public static readonly DependencyProperty AutoMergeStyleProperty = DependencyProperty.RegisterAttached("AutoMergeStyle", typeof(bool), typeof(ThemeStyleBehavior), new FrameworkPropertyMetadata(false, OnAutoMergeStyleChanged));

	public static readonly DependencyProperty BaseOnStyleProperty = DependencyProperty.RegisterAttached("BaseOnStyle", typeof(Style), typeof(ThemeStyleBehavior), new FrameworkPropertyMetadata(null, OnBaseOnStyleChanged));

	public static readonly DependencyProperty OriginalStyleProperty = DependencyProperty.RegisterAttached("OriginalStyle", typeof(Style), typeof(ThemeStyleBehavior), new FrameworkPropertyMetadata((object)null));

	public static bool GetAutoMergeStyle(DependencyObject d)
	{
		return (bool)d.GetValue(AutoMergeStyleProperty);
	}

	public static void SetAutoMergeStyle(DependencyObject d, bool value)
	{
		d.SetValue(AutoMergeStyleProperty, value);
	}

	private static void OnAutoMergeStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue != e.NewValue)
		{
			if (!(d is FrameworkElement frameworkElement))
			{
				throw new NotSupportedException("AutoMergeStyle can only used in FrameworkElement");
			}
			if ((bool)e.NewValue)
			{
				Type type = d.GetType();
				frameworkElement.SetResourceReference(BaseOnStyleProperty, type);
			}
			else
			{
				frameworkElement.ClearValue(BaseOnStyleProperty);
			}
		}
	}

	public static Style GetBaseOnStyle(DependencyObject d)
	{
		return (Style)d.GetValue(BaseOnStyleProperty);
	}

	public static void SetBaseOnStyle(DependencyObject d, Style value)
	{
		d.SetValue(BaseOnStyleProperty, value);
	}

	private static void OnBaseOnStyleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		if (e.OldValue == e.NewValue)
		{
			return;
		}
		if (!(d is FrameworkElement frameworkElement))
		{
			throw new NotSupportedException("BaseOnStyle can only used in FrameworkElement");
		}
		Style basedOn = e.NewValue as Style;
		Style style = GetOriginalStyle(frameworkElement);
		if (style == null)
		{
			style = frameworkElement.Style;
			SetOriginalStyle(frameworkElement, style);
		}
		Style style2 = style;
		if (style.IsSealed)
		{
			style2 = new Style();
			style2.TargetType = style.TargetType;
			style2.Resources = style.Resources;
			foreach (SetterBase setter in style.Setters)
			{
				style2.Setters.Add(setter);
			}
			foreach (TriggerBase trigger in style.Triggers)
			{
				style2.Triggers.Add(trigger);
			}
			style2.BasedOn = basedOn;
		}
		else
		{
			style.BasedOn = basedOn;
		}
		frameworkElement.Style = style2;
	}

	public static Style GetOriginalStyle(DependencyObject d)
	{
		return (Style)d.GetValue(OriginalStyleProperty);
	}

	public static void SetOriginalStyle(DependencyObject d, Style value)
	{
		d.SetValue(OriginalStyleProperty, value);
	}
}
