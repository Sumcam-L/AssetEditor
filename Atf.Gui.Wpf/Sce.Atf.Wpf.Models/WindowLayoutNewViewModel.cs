using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Applications;

namespace Sce.Atf.Wpf.Models;

internal sealed class WindowLayoutNewViewModel : DialogViewModelBase, IDataErrorInfo
{
	private string m_layoutName = string.Empty;

	private readonly IEnumerable<string> m_existingItems;

	public string LayoutName
	{
		get
		{
			return m_layoutName.Trim();
		}
		set
		{
			m_layoutName = value;
			RaisePropertyChanged("LayoutName");
		}
	}

	public string Error => null;

	public string this[string columnName]
	{
		get
		{
			string result = null;
			if (columnName == "LayoutName" && !string.IsNullOrEmpty(LayoutName))
			{
				result = IsValidName(LayoutName);
			}
			return result;
		}
	}

	public WindowLayoutNewViewModel(IEnumerable<string> existing)
	{
		base.Title = "New Layout".Localize();
		m_existingItems = existing;
	}

	protected override bool CanExecuteOk()
	{
		return IsValidName(LayoutName) == null;
	}

	private string IsValidName(string layoutName)
	{
		if (string.IsNullOrEmpty(layoutName))
		{
			return "Layout name is empty";
		}
		if (!WindowLayoutService.IsValidLayoutName(layoutName))
		{
			return "Layout name contains invalid characters";
		}
		if (m_existingItems.Contains(layoutName))
		{
			return "Layout name already exists";
		}
		return null;
	}
}
