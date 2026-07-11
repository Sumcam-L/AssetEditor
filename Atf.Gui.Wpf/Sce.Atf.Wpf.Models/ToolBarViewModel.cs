using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

[ExportViewModel("ToolBarViewModel")]
public class ToolBarViewModel : NotifyPropertyChangedBase
{
	private class ToolBarComparer : IComparer<IToolBar>, IComparer
	{
		private static ToolBarComparer s_instance;

		public static ToolBarComparer Instance => s_instance ?? (s_instance = new ToolBarComparer());

		public int Compare(IToolBar x, IToolBar y)
		{
			if (x != null && y != null)
			{
				return CommandComparer.CompareTags(x.Tag, y.Tag);
			}
			return 0;
		}

		public int Compare(object x, object y)
		{
			IToolBar toolBar = x as IToolBar;
			IToolBar toolBar2 = y as IToolBar;
			if (toolBar != null && toolBar2 != null)
			{
				return Compare(toolBar, toolBar2);
			}
			return 0;
		}
	}

	private class ToolBarItemComparer : IComparer<IToolBarItem>
	{
		private static ToolBarItemComparer s_instance;

		public static ToolBarItemComparer Instance => s_instance ?? (s_instance = new ToolBarItemComparer());

		public int Compare(IToolBarItem x, IToolBarItem y)
		{
			if (x != null && y != null)
			{
				ICommandItem commandItem = x as ICommandItem;
				ICommandItem commandItem2 = y as ICommandItem;
				if (commandItem != null && commandItem2 != null)
				{
					return CommandComparer.CompareCommands(commandItem, commandItem2);
				}
				return CommandComparer.CompareTags(x.Tag, y.Tag);
			}
			return 0;
		}
	}

	private IToolBar[] m_toolBars;

	private bool m_toolBarsRequireRefresh;

	private IToolBarItem[] m_toolBarItems;

	[ImportMany(AllowRecomposition = true)]
	public IToolBar[] ToolBars
	{
		get
		{
			if (m_toolBars != null && m_toolBars.Length != 0 && m_toolBarsRequireRefresh)
			{
				m_toolBarsRequireRefresh = false;
				Application.Current.Dispatcher.InvokeIfRequired(delegate
				{
					ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(m_toolBars);
					listCollectionView.CustomSort = ToolBarComparer.Instance;
				});
			}
			return m_toolBars;
		}
		set
		{
			m_toolBars = value;
			m_toolBarsRequireRefresh = true;
			OnPropertyChanged(new PropertyChangedEventArgs("ToolBars"));
		}
	}

	[ImportMany(AllowRecomposition = true)]
	private IToolBarItem[] ToolBarItems
	{
		get
		{
			return m_toolBarItems;
		}
		set
		{
			m_toolBarItems = value;
			OnPropertyChanged(new PropertyChangedEventArgs("ToolBars"));
		}
	}

	public IEnumerable<IToolBarItem> GetToolBarItems(IToolBar toolBar)
	{
		if (toolBar == null || toolBar.Tag == null)
		{
			return EmptyEnumerable<IToolBarItem>.Instance;
		}
		List<IToolBarItem> list = m_toolBarItems.Where((IToolBarItem x) => x.IsVisible && CommandComparer.TagsEqual(x.ToolBarTag, toolBar.Tag)).ToList();
		list.Sort(ToolBarItemComparer.Instance);
		if (list.Any())
		{
			List<IToolBarItem> list2 = new List<IToolBarItem>();
			object obj = ((list[0] is CommandItem commandItem) ? commandItem.GroupTag : null);
			list2.Add(list[0]);
			for (int num = 1; num < list.Count; num++)
			{
				if (!(list[num] is CommandItem { GroupTag: var groupTag } commandItem2))
				{
					list2.Add(list[num]);
					continue;
				}
				if (groupTag != null && obj != null && !CommandComparer.TagsEqual(groupTag, obj))
				{
					list2.Add(new ToolBarSeparator());
				}
				list2.Add(commandItem2);
				obj = groupTag;
			}
			list = list2;
		}
		return list;
	}
}
