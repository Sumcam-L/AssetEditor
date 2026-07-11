using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Models;

internal sealed class ManageWindowLayoutsDialogViewModel : DialogViewModelBase
{
	private DirectoryInfo m_screenshotDirectory;

	private readonly ObservableCollection<LayoutSlot> m_layouts = new ObservableCollection<LayoutSlot>();

	private readonly List<string> m_deletedLayouts = new List<string>();

	private readonly List<LayoutSlot> m_screenshots = new List<LayoutSlot>();

	private readonly Dictionary<string, string> m_renamedLayouts = new Dictionary<string, string>(StringComparer.CurrentCulture);

	public ICommand DeleteCommand { get; private set; }

	public ICommand RenameCommand { get; private set; }

	public DirectoryInfo ScreenshotDirectory
	{
		get
		{
			return m_screenshotDirectory;
		}
		set
		{
			if (value == null || value == m_screenshotDirectory)
			{
				return;
			}
			m_screenshotDirectory = value;
			try
			{
				m_screenshots.Clear();
				if (!Directory.Exists(m_screenshotDirectory.FullName))
				{
					return;
				}
				string[] files = Directory.GetFiles(m_screenshotDirectory.FullName, "*.jpg", SearchOption.TopDirectoryOnly);
				if (!files.Any())
				{
					return;
				}
				string[] array = files;
				foreach (string path in array)
				{
					string name = Path.GetFileNameWithoutExtension(path);
					LayoutSlot layoutSlot = m_layouts.FirstOrDefault((LayoutSlot x) => x.Name.Equals(name));
					if (layoutSlot != null)
					{
						layoutSlot.Image = ImageUtil.CreateFromFile(path);
						m_screenshots.Add(layoutSlot);
					}
				}
			}
			catch (Exception ex)
			{
				Outputs.WriteLine(OutputMessageType.Error, "Manage Layouts: Exception parsing screenshot directory: {0}", ex.Message);
			}
		}
	}

	public ObservableCollection<LayoutSlot> Layouts => m_layouts;

	public ICollectionView LayoutView { get; private set; }

	public IEnumerable<KeyValuePair<string, string>> RenamedLayouts => m_renamedLayouts;

	public IEnumerable<string> DeletedLayouts => m_deletedLayouts;

	public ManageWindowLayoutsDialogViewModel(IEnumerable<Pair<string, Keys>> layouts)
	{
		base.Title = "Select a layout";
		RenameCommand = new DelegateCommand<LayoutSlot>(Rename, CanRename, isAutomaticRequeryDisabled: false);
		DeleteCommand = new DelegateCommand<LayoutSlot>(Delete, CanDelete, isAutomaticRequeryDisabled: false);
		foreach (Pair<string, Keys> layout in layouts)
		{
			LayoutSlot layoutSlot = new LayoutSlot(this, layout.First, layout.Second);
			layoutSlot.Renamed += SlotRenamed;
			Layouts.Add(layoutSlot);
		}
		LayoutView = CollectionViewSource.GetDefaultView(Layouts);
	}

	private void SlotRenamed(object sender, EventArgs e)
	{
		if (!(sender is LayoutSlot layoutSlot))
		{
			return;
		}
		if (string.IsNullOrEmpty(layoutSlot.Name) || LayoutSlot.IsValidName(layoutSlot.Name) != null)
		{
			layoutSlot.Name = layoutSlot.OldName;
			return;
		}
		m_renamedLayouts[layoutSlot.OldName] = layoutSlot.Name;
		if (m_screenshots.Contains(layoutSlot))
		{
			m_screenshots.Remove(layoutSlot);
		}
		string text = Path.Combine(m_screenshotDirectory.FullName + Path.DirectorySeparatorChar, layoutSlot.OldName + ".jpg");
		if (!File.Exists(text))
		{
			return;
		}
		string text2 = Path.Combine(m_screenshotDirectory.FullName + Path.DirectorySeparatorChar, layoutSlot.Name + ".jpg");
		if (text.Equals(text2))
		{
			return;
		}
		layoutSlot.Image = null;
		try
		{
			File.Copy(text, text2);
			if (File.Exists(text2))
			{
				m_screenshots.Add(layoutSlot);
			}
			File.Delete(text);
		}
		catch (IOException)
		{
		}
		layoutSlot.Image = ImageUtil.CreateFromFile(text2);
	}

	private bool CanDelete(LayoutSlot param)
	{
		LayoutSlot currentSlot = GetCurrentSlot();
		return currentSlot != null && currentSlot.Name != null;
	}

	private void Delete(LayoutSlot param)
	{
		LayoutSlot currentSlot = GetCurrentSlot();
		if (currentSlot == null)
		{
			return;
		}
		string oldName = currentSlot.OldName;
		m_renamedLayouts.Remove(oldName);
		m_deletedLayouts.Add(oldName);
		Layouts.Remove(currentSlot);
		try
		{
			currentSlot.Image = null;
			if (m_screenshots.Contains(currentSlot))
			{
				m_screenshots.Remove(currentSlot);
			}
			string path = Path.Combine(m_screenshotDirectory.FullName + Path.DirectorySeparatorChar, currentSlot.Name + ".jpg");
			if (File.Exists(path))
			{
				File.Delete(path);
			}
		}
		catch (Exception ex)
		{
			Outputs.WriteLine(OutputMessageType.Error, "Manage layouts: Exception deleting screenshot: {0}", ex.Message);
		}
	}

	private bool CanRename(LayoutSlot param)
	{
		LayoutSlot currentSlot = GetCurrentSlot();
		return currentSlot != null;
	}

	private void Rename(LayoutSlot param)
	{
		LayoutSlot currentSlot = GetCurrentSlot();
		if (currentSlot != null)
		{
			currentSlot.IsInEditMode = true;
		}
	}

	private LayoutSlot GetCurrentSlot()
	{
		return CollectionViewSource.GetDefaultView(Layouts).CurrentItem as LayoutSlot;
	}
}
