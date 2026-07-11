using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

[ExportViewModel("MainMenuViewModel")]
public class MainMenuViewModel : NotifyPropertyChangedBase
{
	private class RootMenuComparer : IComparer
	{
		private static RootMenuComparer s_instance;

		public static RootMenuComparer Instance
		{
			get
			{
				if (s_instance == null)
				{
					s_instance = new RootMenuComparer();
				}
				return s_instance;
			}
		}

		public int Compare(object x, object y)
		{
			RootMenuModel rootMenuModel = x as RootMenuModel;
			RootMenuModel rootMenuModel2 = y as RootMenuModel;
			if (rootMenuModel != null && rootMenuModel2 != null)
			{
				return CommandComparer.CompareTags(rootMenuModel.MenuTag, rootMenuModel2.MenuTag);
			}
			return 0;
		}
	}

	private ObservableCollection<IMenuModel> m_rootMenuModels;

	private IMenu[] m_menuDefinitions;

	private IMenuItem[] m_menuItems;

	private bool m_requiresRefresh;

	public IEnumerable<IMenuModel> Menus
	{
		get
		{
			if (m_requiresRefresh)
			{
				m_requiresRefresh = false;
				m_rootMenuModels = new ObservableCollection<IMenuModel>();
				if (m_menuDefinitions != null && m_menuDefinitions.Length != 0 && m_menuItems != null && m_menuItems.Length != 0)
				{
					IMenu[] menuDefinitions = m_menuDefinitions;
					foreach (IMenu def in menuDefinitions)
					{
						m_rootMenuModels.Add(new RootMenuModel(def));
					}
					List<IMenuItem> list = new List<IMenuItem>(m_menuItems);
					list.Sort(new CommandComparer());
					foreach (IMenuItem menuItem in list.Where((IMenuItem x) => x.IsVisible))
					{
						if (m_rootMenuModels.FirstOrDefault((IMenuModel x) => CommandComparer.TagsEqual(((RootMenuModel)x).MenuTag, menuItem.MenuTag)) is RootMenuModel rootMenuModel)
						{
							rootMenuModel.AddItem(menuItem);
						}
					}
					Application.Current.Dispatcher.InvokeIfRequired(delegate
					{
						ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(m_rootMenuModels);
						listCollectionView.CustomSort = RootMenuComparer.Instance;
					});
				}
			}
			return m_rootMenuModels;
		}
	}

	[ImportMany(AllowRecomposition = true)]
	private IMenu[] MenuDefinitions
	{
		set
		{
			m_menuDefinitions = value;
			m_requiresRefresh = true;
			OnPropertyChanged(new PropertyChangedEventArgs("Menus"));
		}
	}

	[ImportMany(AllowRecomposition = true)]
	private IMenuItem[] MenuItems
	{
		set
		{
			m_menuItems = value;
			m_requiresRefresh = true;
			OnPropertyChanged(new PropertyChangedEventArgs("Menus"));
		}
	}
}
