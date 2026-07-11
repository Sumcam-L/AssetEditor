using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

internal class RootMenuModel : MenuModel
{
	private readonly List<IMenuItem> m_items = new List<IMenuItem>();

	private bool m_requiresRefresh = true;

	private static readonly PropertyChangedEventArgs s_isVisibleArgs = ObservableUtil.CreateArgs((RootMenuModel x) => x.IsVisible);

	private static readonly PropertyChangedEventArgs s_childrenArgs = ObservableUtil.CreateArgs((RootMenuModel x) => x.Children);

	public object MenuTag { get; private set; }

	public RootMenuModel(IMenu def)
		: this(def.MenuTag, def.Text, def.Description)
	{
	}

	public RootMenuModel(object menuTag, string text, string description)
		: base(null, text, description)
	{
		MenuTag = menuTag;
	}

	public void AddItem(IMenuItem item)
	{
		m_items.Add(item);
		Invalidate();
	}

	public void RemoveItem(IMenuItem item)
	{
		if (m_items.Remove(item))
		{
			Invalidate();
		}
	}

	protected override ObservableCollection<object> GetChildren()
	{
		if (m_requiresRefresh)
		{
			m_requiresRefresh = false;
			Rebuild();
		}
		return base.GetChildren();
	}

	private void Invalidate()
	{
		m_requiresRefresh = true;
		OnPropertyChanged(s_childrenArgs);
		OnPropertyChanged(s_isVisibleArgs);
	}

	private void Rebuild()
	{
		ObservableCollection<object> children = base.GetChildren();
		if (children.Count > 0)
		{
			children.Clear();
		}
		if (m_items.Count <= 0)
		{
			return;
		}
		foreach (IMenuItem item in m_items)
		{
			BuildSubMenus(item);
		}
		InsertGroupSeparators();
	}

	private void BuildSubMenus(IMenuItem menuItem)
	{
		ObservableCollection<object> children = base.GetChildren();
		MenuModel parent = this;
		foreach (string segment in menuItem.MenuPath)
		{
			MenuModel menuModel = (MenuModel)children.FirstOrDefault((object x) => x is MenuModel && ((MenuModel)x).Text == segment);
			if (menuModel == null)
			{
				menuModel = new MenuModel(parent, segment, segment);
				children.Add(menuModel);
			}
			children = menuModel.Children;
			parent = menuModel;
		}
		children.Add(menuItem);
	}

	private void InsertGroupSeparators()
	{
		List<Tuple<ICommandItem, MenuModel>> list = new List<Tuple<ICommandItem, MenuModel>>();
		GetCommandsInSubtree(this, list);
		for (int i = 1; i < list.Count; i++)
		{
			Tuple<ICommandItem, MenuModel> tuple = list[i - 1];
			Tuple<ICommandItem, MenuModel> tuple2 = list[i];
			if (!CommandComparer.TagsEqual(tuple.Item1.GroupTag, tuple2.Item1.GroupTag))
			{
				InsertSeparator(tuple, tuple2);
			}
		}
	}

	private static void GetCommandsInSubtree(MenuModel menuModel, IList<Tuple<ICommandItem, MenuModel>> commands)
	{
		foreach (object child in menuModel.Children)
		{
			if (child is ICommandItem)
			{
				commands.Add(new Tuple<ICommandItem, MenuModel>((ICommandItem)child, menuModel));
			}
			else if (child is MenuModel)
			{
				GetCommandsInSubtree((MenuModel)child, commands);
			}
		}
	}

	private static void InsertSeparator(Tuple<ICommandItem, MenuModel> previous, Tuple<ICommandItem, MenuModel> current)
	{
		MenuModel[] array = GetLineage(previous.Item2).Reverse().ToArray();
		MenuModel[] array2 = GetLineage(current.Item2).Reverse().ToArray();
		int num = Math.Min(array.Length, array2.Length);
		object obj = null;
		IList<object> list = null;
		for (int i = 0; i < num; i++)
		{
			if (array[i] != array2[i])
			{
				obj = array2[i];
				list = array2[i - 1].Children;
			}
		}
		if (obj == null)
		{
			obj = ((num >= array2.Length) ? ((INotifyPropertyChanged)current.Item1) : ((INotifyPropertyChanged)array2[num]));
			list = array2[num - 1].Children;
		}
		int index = list.IndexOf(obj);
		list.Insert(index, new Separator());
	}

	private static IEnumerable<MenuModel> GetLineage(MenuModel menuModel)
	{
		while (menuModel != null)
		{
			yield return menuModel;
			menuModel = menuModel.Parent;
		}
	}
}
