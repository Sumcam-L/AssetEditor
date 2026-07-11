using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Win32;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Controls;

public class FilePicker : Control
{
	public static readonly DependencyProperty FilePathProperty;

	public static readonly DependencyProperty FilterProperty;

	public static readonly DependencyProperty DefaultExtensionProperty;

	public static readonly DependencyProperty ElementToFocusProperty;

	public static readonly DependencyProperty FileDialogServiceProperty;

	public string FilePath
	{
		get
		{
			return (string)GetValue(FilePathProperty);
		}
		set
		{
			SetValue(FilePathProperty, value);
		}
	}

	public string Filter
	{
		get
		{
			return (string)GetValue(FilterProperty);
		}
		set
		{
			SetValue(FilterProperty, value);
		}
	}

	public string DefaultExtension
	{
		get
		{
			return (string)GetValue(DefaultExtensionProperty);
		}
		set
		{
			SetValue(DefaultExtensionProperty, value);
		}
	}

	public FrameworkElement ElementToFocus
	{
		get
		{
			return (FrameworkElement)GetValue(ElementToFocusProperty);
		}
		set
		{
			SetValue(ElementToFocusProperty, value);
		}
	}

	public IFileDialogService FileDialogService
	{
		get
		{
			return (IFileDialogService)GetValue(FileDialogServiceProperty);
		}
		set
		{
			SetValue(FileDialogServiceProperty, value);
		}
	}

	public ICommand BrowseCommand { get; set; }

	static FilePicker()
	{
		FilePathProperty = DependencyProperty.Register("FilePath", typeof(string), typeof(FilePicker), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
		FilterProperty = DependencyProperty.Register("Filter", typeof(string), typeof(FilePicker), new UIPropertyMetadata(null));
		DefaultExtensionProperty = DependencyProperty.Register("DefaultExtension", typeof(string), typeof(FilePicker), new UIPropertyMetadata(null));
		ElementToFocusProperty = DependencyProperty.Register("ElementToFocus", typeof(FrameworkElement), typeof(FilePicker), new UIPropertyMetadata(null));
		FileDialogServiceProperty = DependencyProperty.Register("FileDialogService", typeof(IFileDialogService), typeof(FilePicker), new UIPropertyMetadata(null));
		FrameworkElement.DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePicker), new FrameworkPropertyMetadata(typeof(FilePicker)));
	}

	public FilePicker()
	{
		BrowseCommand = new DelegateCommand(Browse);
	}

	private void Browse()
	{
		FrameworkElement elementToFocus = ElementToFocus;
		if (elementToFocus != null)
		{
			elementToFocus.Focus();
			Keyboard.Focus(elementToFocus);
		}
		if (FileDialogService != null)
		{
			return;
		}
		try
		{
			OpenFileDialog openFileDialog = new OpenFileDialog
			{
				FileName = FilePath,
				Filter = Filter,
				DefaultExt = DefaultExtension
			};
			if (openFileDialog.ShowDialog() == true)
			{
				FilePath = openFileDialog.FileName;
			}
		}
		catch (ArgumentException ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, ex.Message);
		}
	}
}
