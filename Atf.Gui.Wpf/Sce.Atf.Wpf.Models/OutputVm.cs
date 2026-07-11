using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using System.Windows.Input;

namespace Sce.Atf.Wpf.Models;

public class OutputVm : NotifyPropertyChangedBase
{
	private static readonly PropertyChangedEventArgs s_showWarningsArgs = ObservableUtil.CreateArgs((OutputVm x) => x.ShowWarnings);

	private static readonly PropertyChangedEventArgs s_showErrorsArgs = ObservableUtil.CreateArgs((OutputVm x) => x.ShowErrors);

	private static readonly PropertyChangedEventArgs s_showInfoArgs = ObservableUtil.CreateArgs((OutputVm x) => x.ShowInfo);

	private readonly Dictionary<OutputMessageType, bool> m_filterState = new Dictionary<OutputMessageType, bool>
	{
		{
			OutputMessageType.Info,
			true
		},
		{
			OutputMessageType.Warning,
			true
		},
		{
			OutputMessageType.Error,
			true
		}
	};

	public ObservableCollection<OutputItemVm> OutputItems { get; private set; }

	public ICommand ClearAllCommand { get; private set; }

	public bool ShowWarnings
	{
		get
		{
			return m_filterState[OutputMessageType.Warning];
		}
		set
		{
			m_filterState[OutputMessageType.Warning] = value;
			OnPropertyChanged(s_showWarningsArgs);
		}
	}

	public bool ShowErrors
	{
		get
		{
			return m_filterState[OutputMessageType.Error];
		}
		set
		{
			m_filterState[OutputMessageType.Error] = value;
			OnPropertyChanged(s_showErrorsArgs);
		}
	}

	public bool ShowInfo
	{
		get
		{
			return m_filterState[OutputMessageType.Info];
		}
		set
		{
			m_filterState[OutputMessageType.Info] = value;
			OnPropertyChanged(s_showInfoArgs);
		}
	}

	public OutputVm()
	{
		OutputItems = new ObservableCollection<OutputItemVm>();
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(OutputItems);
		defaultView.Filter = (Predicate<object>)Delegate.Combine(defaultView.Filter, (Predicate<object>)((object obj) => m_filterState[((OutputItemVm)obj).MessageType]));
		ClearAllCommand = new DelegateCommand(delegate
		{
			OutputItems.Clear();
		});
	}

	protected override void OnPropertyChanged(PropertyChangedEventArgs e)
	{
		CollectionViewSource.GetDefaultView(OutputItems).Refresh();
		base.OnPropertyChanged(e);
	}
}
