using System;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;

namespace Sce.Atf.Controls;

public class OpenFilteredFileDialog : FilteredFileDialogBase
{
	private static Size s_lastSize;

	private static Point s_lastLocation;

	private static int[] s_lastColumnWidths;

	private static string s_lastAccessedDirectory;

	private static bool s_settingsRegistered;

	public string[] FileNames => base.SelectedFileNames.ToArray();

	public string FileName => base.SelectedFileNames.FirstOrDefault();

	public ISettingsService SettingsService { get; set; }

	private Size LastSize
	{
		get
		{
			return s_lastSize;
		}
		set
		{
			s_lastSize = value;
		}
	}

	private Point LastLocation
	{
		get
		{
			return s_lastLocation;
		}
		set
		{
			s_lastLocation = value;
		}
	}

	private string LastAccessedDirectory
	{
		get
		{
			return s_lastAccessedDirectory;
		}
		set
		{
			s_lastAccessedDirectory = value;
		}
	}

	private int[] LastColumnWidths
	{
		get
		{
			return s_lastColumnWidths;
		}
		set
		{
			s_lastColumnWidths = value;
		}
	}

	public OpenFilteredFileDialog()
	{
		Text = "Open".Localize();
	}

	protected override void OnLoad(EventArgs e)
	{
		if (SettingsService != null && !s_settingsRegistered)
		{
			SettingsService.RegisterSettings("9DC15B28-D9F3-4B05-BFBB-FF707E94CEEA", new BoundPropertyDescriptor(this, () => LastSize, "Last Size", null, "Last size for OpenFilteredFileDialog"), new BoundPropertyDescriptor(this, () => LastLocation, "Last Location", null, "Last location for OpenFilteredFileDialog"), new BoundPropertyDescriptor(this, () => LastColumnWidths, "Column Widths", null, "Column Widths for OpenFilteredFileDialog"), new BoundPropertyDescriptor(this, () => LastAccessedDirectory, "Last Accessed Directory", null, "Last Accessed Directory"));
			s_settingsRegistered = true;
		}
		if (!LastSize.IsEmpty)
		{
			base.Size = LastSize;
			base.Location = LastLocation;
			base.ColumnWidths = LastColumnWidths;
		}
		if (string.IsNullOrEmpty(base.InitialDirectory))
		{
			base.InitialDirectory = LastAccessedDirectory;
		}
		base.OnLoad(e);
	}

	protected override void OnClosing(CancelEventArgs e)
	{
		s_lastColumnWidths = base.ColumnWidths;
		if (SettingsService != null && s_settingsRegistered)
		{
			if (base.WindowState == FormWindowState.Normal)
			{
				s_lastSize = base.Size;
				s_lastLocation = base.Location;
			}
			else
			{
				s_lastSize = base.RestoreBounds.Size;
				s_lastLocation = base.RestoreBounds.Location;
			}
			if (base.SelectedFileNames.Any())
			{
				LastAccessedDirectory = Path.GetDirectoryName(base.SelectedFileNames.First());
			}
		}
		base.OnClosing(e);
	}
}
