using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace Firaxis.AssetBrowser.Views;

public partial class FilterBuilderView : UserControl, IComponentConnector
{
	public FilterBuilderView()
	{
		InitializeComponent();
	}

	private void InputTextBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.Down)
		{
			return;
		}
		e.Handled = true;
		InputChoicesComboBox.IsDropDownOpen = true;
		InputChoicesComboBox.Focus();
		if (InputChoicesComboBox.Items.Count > 0)
		{
			int num = InputChoicesComboBox.SelectedIndex;
			if (num < 0)
			{
				num = 0;
			}
			if (InputChoicesComboBox.ItemContainerGenerator.ContainerFromIndex(num) is IInputElement element)
			{
				Keyboard.Focus(element);
			}
		}
	}

	private void InputChoicesComboBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Escape)
		{
			InputChoicesComboBox.IsDropDownOpen = false;
		}
	}
}
