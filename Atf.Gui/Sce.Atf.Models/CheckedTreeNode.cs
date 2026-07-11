using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Sce.Atf.Models;

public class CheckedTreeNode : INotifyPropertyChanged
{
	private CheckedTreeNode m_parent;

	private bool? m_isChecked = null;

	private static readonly PropertyChangedEventArgs s_isCheckedArgs = ObservableUtil.CreateArgs((CheckedTreeNode x) => x.IsChecked);

	private bool m_isEnabled;

	private static readonly PropertyChangedEventArgs s_isEnabledArgs = ObservableUtil.CreateArgs((CheckedTreeNode x) => x.IsEnabled);

	private string m_name;

	private static readonly PropertyChangedEventArgs s_nameArgs = ObservableUtil.CreateArgs((CheckedTreeNode x) => x.Name);

	public object Value { get; set; }

	public ObservableCollection<CheckedTreeNode> Children { get; private set; }

	public CheckedTreeNode Parent
	{
		get
		{
			return m_parent;
		}
		set
		{
			m_parent = value;
			if (m_parent != null)
			{
				m_parent.VerifyCheckState();
			}
		}
	}

	public bool? IsChecked
	{
		get
		{
			return m_isChecked;
		}
		set
		{
			SetIsChecked(value, updateChildren: true, updateParent: true);
		}
	}

	public bool IsEnabled
	{
		get
		{
			return m_isEnabled;
		}
		set
		{
			m_isEnabled = value;
			OnPropertyChanged(s_isEnabledArgs);
		}
	}

	public string Name
	{
		get
		{
			return m_name;
		}
		set
		{
			m_name = value;
			OnPropertyChanged(s_nameArgs);
		}
	}

	public event PropertyChangedEventHandler PropertyChanged;

	public CheckedTreeNode()
		: this(null, null, true, isEnabled: true)
	{
	}

	public CheckedTreeNode(object value)
		: this(value, null, true, isEnabled: true)
	{
	}

	public CheckedTreeNode(object value, string name, bool? isChecked, bool isEnabled)
	{
		Children = new ObservableCollection<CheckedTreeNode>();
		Children.CollectionChanged += Children_CollectionChanged;
		Value = value;
		Name = name;
		IsChecked = isChecked;
		IsEnabled = isEnabled;
	}

	protected virtual void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		if (this.PropertyChanged != null)
		{
			this.PropertyChanged(this, e);
		}
	}

	private void Children_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		switch (e.Action)
		{
		case NotifyCollectionChangedAction.Add:
		{
			foreach (object newItem in e.NewItems)
			{
				((CheckedTreeNode)newItem).Parent = this;
			}
			break;
		}
		case NotifyCollectionChangedAction.Remove:
		{
			foreach (object oldItem in e.OldItems)
			{
				((CheckedTreeNode)oldItem).Parent = null;
			}
			break;
		}
		case NotifyCollectionChangedAction.Reset:
		{
			foreach (object item in (IEnumerable)sender)
			{
				((CheckedTreeNode)item).Parent = null;
			}
			break;
		}
		default:
			throw new NotSupportedException();
		}
	}

	private void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
	{
		if (value == m_isChecked)
		{
			return;
		}
		m_isChecked = value;
		if (updateChildren && m_isChecked.HasValue)
		{
			foreach (CheckedTreeNode child in Children)
			{
				child.SetIsChecked(m_isChecked, updateChildren: true, updateParent: false);
			}
		}
		if (updateParent && Parent != null)
		{
			Parent.VerifyCheckState();
		}
		OnPropertyChanged(s_isCheckedArgs);
	}

	private void VerifyCheckState()
	{
		bool? flag = IsChecked;
		bool flag2 = true;
		foreach (CheckedTreeNode child in Children)
		{
			bool? isChecked = child.IsChecked;
			if (flag2)
			{
				flag = isChecked;
				flag2 = false;
			}
			else if (isChecked != flag)
			{
				flag = null;
				break;
			}
		}
		SetIsChecked(flag, updateChildren: false, updateParent: true);
	}
}
