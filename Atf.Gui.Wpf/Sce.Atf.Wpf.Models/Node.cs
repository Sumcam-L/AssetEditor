using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using Sce.Atf.Models;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class Node : AdapterViewModel
{
	private static readonly PropertyChangedEventArgs s_imageKeyArgs = ObservableUtil.CreateArgs((Node x) => x.ImageKey);

	private static readonly PropertyChangedEventArgs s_stateImageKeyArgs = ObservableUtil.CreateArgs((Node x) => x.StateImageKey);

	private static readonly PropertyChangedEventArgs s_hoverTextArgs = ObservableUtil.CreateArgs((Node x) => x.HoverText);

	private readonly WpfItemInfo m_itemInfo = new WpfItemInfo();

	private readonly TreeViewModel m_owner;

	private ObservableCollection<Node> m_children;

	private bool m_isFiltered;

	private bool m_isFilterCached;

	private string m_filterString;

	private bool m_isSelected;

	private bool m_isInLabelEditMode;

	public Node Parent { get; private set; }

	public IEnumerable<Node> Children => m_children ?? (m_children = m_owner.CreateChildren(this));

	public string Label
	{
		get
		{
			return m_itemInfo.Label;
		}
		set
		{
			ILabelEditingContext context = m_owner.As<ILabelEditingContext>();
			if (context != null && context.CanEditLabel(base.Adaptee))
			{
				ITransactionContext context2 = m_owner.As<ITransactionContext>();
				context2.DoTransaction(delegate
				{
					context.SetLabel(base.Adaptee, value);
				}, "Edit Label".Localize());
			}
		}
	}

	public bool IsSelected
	{
		get
		{
			return m_isSelected;
		}
		set
		{
			if (m_isSelected != value)
			{
				m_isSelected = value;
				OnPropertyChanged(new PropertyChangedEventArgs("IsSelected"));
				this.IsSelectedChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool Expanded
	{
		get
		{
			return m_itemInfo.IsExpandedInView;
		}
		set
		{
			if (m_itemInfo.IsExpandedInView != value)
			{
				m_itemInfo.IsExpandedInView = value;
				if (value)
				{
					m_children = m_owner.CreateChildren(this);
				}
				OnPropertyChanged(new PropertyChangedEventArgs("Expanded"));
			}
		}
	}

	public bool IsLeaf => m_itemInfo.IsLeaf;

	public bool IsInLabelEditMode
	{
		get
		{
			return m_isInLabelEditMode;
		}
		set
		{
			if (m_itemInfo.AllowLabelEdit)
			{
				m_isInLabelEditMode = value;
				OnPropertyChanged(new PropertyChangedEventArgs("IsInLabelEditMode"));
			}
		}
	}

	public object ImageKey => m_itemInfo.ImageKey;

	public object StateImageKey => m_itemInfo.StateImageKey;

	public FontWeight FontWeight => ItemInfo.FontWeight;

	public FontStyle FontItalicStyle => ItemInfo.FontItalicStyle;

	public string HoverText
	{
		get
		{
			return m_itemInfo.HoverText;
		}
		set
		{
			if (!(m_itemInfo.HoverText == value))
			{
				m_itemInfo.HoverText = value;
				OnPropertyChanged(s_hoverTextArgs);
				this.HoverTextChanged.Raise(this, EventArgs.Empty);
			}
		}
	}

	public bool HasCheck => m_itemInfo.HasCheck;

	public bool? CheckState
	{
		get
		{
			return m_itemInfo.CheckState;
		}
		set
		{
			m_itemInfo.CheckState = value;
			ICheckableItemView context = m_owner.As<ICheckableItemView>();
			if (context != null)
			{
				ITransactionContext context2 = m_owner.As<ITransactionContext>();
				context2.DoTransaction(delegate
				{
					context.SetIsChecked(base.Adaptee, value);
				}, "Check/Uncheck".Localize());
			}
		}
	}

	public bool IsEnabled => m_itemInfo.IsEnabled;

	public int Index
	{
		get
		{
			if (Parent == null)
			{
				return 0;
			}
			return Parent.ChildrenInternal.IndexOf(this);
		}
	}

	public bool IsVisible
	{
		get
		{
			if (this == m_owner.Root && !m_owner.ShowRoot)
			{
				return false;
			}
			if (string.IsNullOrEmpty(m_filterString))
			{
				return m_itemInfo.IsVisible;
			}
			if (!m_isFilterCached)
			{
				if (m_itemInfo.IsVisible && Label.ToUpperInvariant().Contains(m_filterString.ToUpperInvariant()))
				{
					m_isFiltered = false;
				}
				else
				{
					m_isFiltered = !Children.Any((Node node) => node.IsVisible);
				}
			}
			return !m_isFiltered;
		}
	}

	public WpfItemInfo ItemInfo => m_itemInfo;

	internal IList<Node> ChildrenInternal => m_children;

	internal event EventHandler IsSelectedChanged;

	internal event EventHandler HoverTextChanged;

	public Node(object adaptee, TreeViewModel owner, Node parent)
		: base(adaptee)
	{
		m_owner = owner;
		Parent = parent;
	}

	public void ResetVisibilityFilter(string filter, bool isNewFilterASubstring, bool isOldFilterASubstring)
	{
		if (m_isFilterCached)
		{
			if (isNewFilterASubstring && m_isFiltered)
			{
				m_isFilterCached = false;
			}
			else if (isOldFilterASubstring && !m_isFiltered)
			{
				m_isFilterCached = false;
			}
			else if (!isOldFilterASubstring && !isNewFilterASubstring)
			{
				m_isFilterCached = false;
			}
		}
		m_filterString = filter;
		OnPropertyChanged(new PropertyChangedEventArgs("IsVisible"));
	}

	internal void ItemInfoChanged()
	{
		OnPropertyChanged(ObservableUtil.AllChangedEventArgs);
	}
}
