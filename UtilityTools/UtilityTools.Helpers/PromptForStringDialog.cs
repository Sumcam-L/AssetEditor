using System.Windows;
using System.Windows.Markup;

namespace UtilityTools.Helpers;

public partial class PromptForStringDialog : Window, IComponentConnector
{
	public string MessagePrompt
	{
		get
		{
			return TextMessage.Text;
		}
		set
		{
			TextMessage.Text = value;
		}
	}

	public string userString { get; set; }

	public PromptForStringDialog()
	{
		InitializeComponent();
	}

	private void OnCancelButtonClick(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}

	private void OnOKButtonClick(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Close();
	}

	private void OnTextChanged(object sender, RoutedEventArgs e)
	{
		userString = TxtBox.Text;
	}
}
