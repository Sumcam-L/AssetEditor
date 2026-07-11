using System;
using System.ComponentModel;
using System.Windows.Input;
using System.Windows.Media;
using Sce.Atf.Applications;
using Sce.Atf.Input;

namespace Sce.Atf.Wpf.Models;

internal class LayoutSlot : NotifyPropertyChangedBase, IDataErrorInfo
{
	private string m_name;

	private bool m_isEditing;

	private ImageSource m_image;

	public ICommand RenameCommand { get; private set; }

	public ICommand DeleteCommand { get; private set; }

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			RaisePropertyChanged("Name");
		}
	}

	public string OldName { get; private set; }

	public Keys Shortcut { get; set; }

	public bool IsInEditMode
	{
		get
		{
			return m_isEditing;
		}
		set
		{
			if (m_isEditing != value)
			{
				m_isEditing = value;
				RaisePropertyChanged("IsInEditMode");
				if (!m_isEditing)
				{
					this.Renamed.Raise(this, EventArgs.Empty);
				}
			}
		}
	}

	public ImageSource Image
	{
		get
		{
			return m_image;
		}
		set
		{
			m_image = value;
			RaisePropertyChanged("Image");
		}
	}

	public string Error => null;

	public string this[string columnName]
	{
		get
		{
			string result = null;
			if (columnName == "Name" && !string.IsNullOrEmpty(Name))
			{
				result = IsValidName(Name);
			}
			return result;
		}
	}

	public event EventHandler Renamed;

	public LayoutSlot(ManageWindowLayoutsDialogViewModel parent, string name, Keys shortcut)
	{
		Name = name;
		OldName = name;
		Shortcut = shortcut;
		RenameCommand = parent.RenameCommand;
		DeleteCommand = parent.DeleteCommand;
	}

	public static string IsValidName(string layoutName)
	{
		if (string.IsNullOrEmpty(layoutName))
		{
			return "Layout name is empty";
		}
		if (!WindowLayoutService.IsValidLayoutName(layoutName))
		{
			return "Layout name contains invalid characters";
		}
		return null;
	}
}
