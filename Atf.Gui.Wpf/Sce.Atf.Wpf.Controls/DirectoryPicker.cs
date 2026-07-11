using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Controls;

public class DirectoryPicker : Control
{
	public static readonly DependencyProperty DirectoryProperty;

	public string Directory
	{
		get
		{
			return (string)GetValue(DirectoryProperty);
		}
		set
		{
			SetValue(DirectoryProperty, value);
		}
	}

	public ICommand BrowseCommand { get; set; }

	static DirectoryPicker()
	{
		DirectoryProperty = DependencyProperty.Register("Directory", typeof(string), typeof(DirectoryPicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(DirectoryPicker), new FrameworkPropertyMetadata(typeof(DirectoryPicker)));
	}

	public DirectoryPicker()
	{
		BrowseCommand = new DelegateCommand(Browse);
	}

	private void Browse()
	{
		BrowseForFolderDialog browseForFolderDialog = new BrowseForFolderDialog();
		browseForFolderDialog.InitialFolder = Directory;
		if (true == browseForFolderDialog.ShowDialog(Application.Current.MainWindow))
		{
			Directory = browseForFolderDialog.SelectedFolder;
		}
	}
}
