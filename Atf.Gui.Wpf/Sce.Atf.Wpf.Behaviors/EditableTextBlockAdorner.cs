using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Behaviors;

public class EditableTextBlockAdorner : Adorner, IDisposable
{
	private readonly VisualCollection m_collection;

	private readonly TextBox m_textBox;

	private readonly TextBlock m_textBlock;

	protected override int VisualChildrenCount => m_collection.Count;

	public EditableTextBlockAdorner(TextBlock adornedElement)
		: base(adornedElement)
	{
		m_collection = new VisualCollection(this);
		m_textBox = new TextBox();
		m_textBox.FontSize = adornedElement.FontSize;
		m_textBox.FontFamily = adornedElement.FontFamily;
		m_textBox.FontStretch = adornedElement.FontStretch;
		m_textBox.FontStyle = adornedElement.FontStyle;
		m_textBox.FontWeight = adornedElement.FontWeight;
		m_textBox.Width = adornedElement.Width;
		m_textBox.Height = adornedElement.Height;
		m_textBox.HorizontalAlignment = HorizontalAlignment.Left;
		m_textBox.VerticalAlignment = VerticalAlignment.Top;
		m_textBox.Padding = new Thickness(0.0);
		m_textBlock = adornedElement;
		Binding binding = new Binding("Text")
		{
			Source = adornedElement
		};
		m_textBox.SetBinding(TextBox.TextProperty, binding);
		m_textBox.AcceptsReturn = false;
		m_textBox.AcceptsTab = false;
		m_textBox.KeyUp += TextBox_KeyUp;
		m_textBox.LostFocus += TextBox_LostFocus;
		m_textBox.SelectAll();
		m_collection.Add(m_textBox);
	}

	private void TextBox_LostFocus(object sender, RoutedEventArgs e)
	{
		m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
	}

	private void TextBox_KeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return || e.Key == Key.Return)
		{
			m_textBox.Text = m_textBox.Text.Replace("\r\n", string.Empty);
			m_textBox.GetBindingExpression(TextBox.TextProperty)?.UpdateSource();
			m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
		}
		else if (e.Key == Key.Escape)
		{
			m_textBlock.SetValue(EditableTextBlockBehavior.IsInEditModeProperty, false);
		}
	}

	protected override Visual GetVisualChild(int index)
	{
		return m_collection[index];
	}

	protected override Size MeasureOverride(Size constraint)
	{
		m_textBox.MinWidth = Math.Max(45.0, base.AdornedElement.DesiredSize.Width + 4.0);
		m_textBox.Measure(new Size(double.PositiveInfinity, constraint.Height + 4.0));
		return m_textBox.DesiredSize;
	}

	protected override Size ArrangeOverride(Size finalSize)
	{
		Rect finalRect = new Rect(-4.0, -2.0, m_textBox.DesiredSize.Width, m_textBox.DesiredSize.Height + 2.0 + 4.0);
		m_textBox.Arrange(finalRect);
		m_textBox.Focus();
		return finalSize;
	}

	public void Dispose()
	{
		BindingOperations.ClearBinding(m_textBox, TextBox.TextProperty);
	}
}
