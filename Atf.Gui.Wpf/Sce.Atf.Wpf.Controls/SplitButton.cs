using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public class SplitButton : Button
{
	public static readonly DependencyProperty DropDownArrowBrushProperty;

	public static readonly DependencyProperty DropDownMenuProperty;

	public static readonly DependencyProperty PreDropDownCommandProperty;

	private bool m_dropDownMenuOpen;

	private UIElement m_dropDownButton;

	public Brush DropDownArrowBrush
	{
		get
		{
			return (Brush)GetValue(DropDownArrowBrushProperty);
		}
		set
		{
			SetValue(DropDownArrowBrushProperty, value);
		}
	}

	public ContextMenu DropDownMenu
	{
		get
		{
			return (ContextMenu)GetValue(DropDownMenuProperty);
		}
		set
		{
			SetValue(DropDownMenuProperty, value);
		}
	}

	public ICommand PreDropDownCommand
	{
		get
		{
			return (ICommand)GetValue(PreDropDownCommandProperty);
		}
		set
		{
			SetValue(PreDropDownCommandProperty, value);
		}
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		m_dropDownButton = GetTemplateChild("PART_DropDownButton") as UIElement;
		if (m_dropDownButton == null)
		{
			throw new InvalidOperationException("PART_DropDownButton does not exist");
		}
		m_dropDownButton.PreviewMouseLeftButtonDown += DropDownButtonPreviewMouseLeftButtonDown;
	}

	protected override void OnInitialized(EventArgs e)
	{
		base.OnInitialized(e);
		base.Loaded += MenuButtonLoaded;
		base.Unloaded += MenuButtonUnloaded;
	}

	private void DropDownButtonPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (DropDownMenu == null)
		{
			return;
		}
		if (m_dropDownMenuOpen)
		{
			DropDownMenu.IsOpen = false;
			m_dropDownMenuOpen = false;
		}
		else
		{
			if (PreDropDownCommand != null && PreDropDownCommand.CanExecute(base.CommandParameter))
			{
				PreDropDownCommand.Execute(base.CommandParameter);
			}
			DropDownMenu.PlacementTarget = this;
			DropDownMenu.Placement = PlacementMode.Bottom;
			DropDownMenu.IsOpen = true;
			m_dropDownMenuOpen = true;
		}
		e.Handled = true;
	}

	private void DropDownMenuClosed(object sender, RoutedEventArgs e)
	{
		m_dropDownMenuOpen = false;
	}

	private void MenuButtonLoaded(object sender, RoutedEventArgs e)
	{
		if (DropDownMenu != null)
		{
			DropDownMenu.Closed += DropDownMenuClosed;
		}
	}

	static SplitButton()
	{
		DropDownArrowBrushProperty = DependencyProperty.Register("DropDownArrowBrush", typeof(Brush), typeof(SplitButton));
		DropDownMenuProperty = DependencyProperty.Register("DropDownMenu", typeof(ContextMenu), typeof(SplitButton));
		PreDropDownCommandProperty = DependencyProperty.Register("PreDropDownCommand", typeof(ICommand), typeof(SplitButton));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(SplitButton), new FrameworkPropertyMetadata(typeof(SplitButton)));
	}

	private void MenuButtonUnloaded(object sender, RoutedEventArgs e)
	{
		if (DropDownMenu != null)
		{
			DropDownMenu.Closed -= DropDownMenuClosed;
		}
	}
}
