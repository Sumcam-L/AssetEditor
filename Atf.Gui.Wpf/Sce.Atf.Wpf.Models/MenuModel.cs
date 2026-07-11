using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class MenuModel : NotifyPropertyChangedBase, IMenuModel
{
	private readonly string m_text;

	private readonly string m_description;

	private readonly ObservableCollection<object> m_children = new ObservableCollection<object>();

	private readonly MenuModel m_parent;

	public string Text => m_text;

	public string Description => m_description;

	public ObservableCollection<object> Children => GetChildren();

	public bool IsVisible => GetSubtree().OfType<ICommandItem>().Any();

	public MenuModel Parent => m_parent;

	public MenuModel(MenuModel parent, string text, string description)
	{
		m_parent = parent;
		m_text = text;
		m_description = description;
	}

	protected IEnumerable<object> GetSubtree()
	{
		foreach (object child in Children)
		{
			yield return child;
			if (!(child is MenuModel menuModel))
			{
				continue;
			}
			foreach (object item in menuModel.GetSubtree())
			{
				yield return item;
			}
		}
	}

	protected virtual ObservableCollection<object> GetChildren()
	{
		return m_children;
	}
}
