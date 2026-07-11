using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Threading;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

internal class FindTargetsViewModel : DialogViewModelBase
{
	private bool m_isScanning;

	private static readonly PropertyChangedEventArgs s_isScanningArgs = ObservableUtil.CreateArgs((FindTargetsViewModel x) => x.IsScanning);

	private DelegateCommand m_addFoundTargetCommand;

	private readonly TargetDialogViewModel m_parent;

	private readonly ICollectionView m_foundTargetCv;

	private BackgroundWorker m_worker;

	public ObservableCollection<TargetViewModel> FoundTargets { get; private set; }

	public bool IsScanning
	{
		get
		{
			return m_isScanning;
		}
		private set
		{
			m_isScanning = value;
			OnPropertyChanged(s_isScanningArgs);
			CommandManager.InvalidateRequerySuggested();
		}
	}

	public ICommand ToggleScanCommand { get; private set; }

	public ICommand AddFoundTargetCommand => m_addFoundTargetCommand ?? (m_addFoundTargetCommand = new DelegateCommand(AddFoundTarget, CanAddFoundTarget, isAutomaticRequeryDisabled: false));

	public ICommand AddAllFoundTargetsCommand { get; private set; }

	public FindTargetsViewModel(TargetDialogViewModel parent)
	{
		m_parent = parent;
		base.Title = "Find Targets".Localize();
		ToggleScanCommand = new DelegateCommand(ToggleScan);
		AddAllFoundTargetsCommand = new DelegateCommand(AddAllFoundTargets, CanAddAllFoundTargets, isAutomaticRequeryDisabled: false);
		FoundTargets = new ObservableCollection<TargetViewModel>();
		FoundTargets.CollectionChanged += delegate
		{
			CommandManager.InvalidateRequerySuggested();
		};
		m_foundTargetCv = CollectionViewSource.GetDefaultView(FoundTargets);
	}

	private void ToggleScan()
	{
		if (!IsScanning && m_worker == null)
		{
			IsScanning = true;
			FoundTargets.Clear();
			m_worker = new BackgroundWorker
			{
				WorkerSupportsCancellation = true
			};
			m_worker.DoWork += DoWork;
			m_worker.RunWorkerCompleted += RunWorkerCompleted;
			m_worker.RunWorkerAsync(this);
		}
		else if (IsScanning && m_worker != null)
		{
			m_worker.CancelAsync();
		}
	}

	private bool CanAddFoundTarget()
	{
		return m_foundTargetCv.CurrentItem is TargetViewModel;
	}

	private void AddFoundTarget()
	{
		TargetViewModel[] array = FoundTargets.Where((TargetViewModel x) => x.IsSelected).ToArray();
		TargetViewModel[] array2 = array;
		foreach (TargetViewModel target in array2)
		{
			AddFoundTarget(target);
		}
	}

	private void AddFoundTarget(TargetViewModel target)
	{
		if (target != null && !m_parent.Targets.Select((TargetViewModel x) => x.Target).Contains(target.Target))
		{
			FoundTargets.Remove(target);
			target.IsSelected = false;
			m_parent.Targets.Add(target);
		}
	}

	private bool CanAddAllFoundTargets()
	{
		return FoundTargets.Count > 0;
	}

	private void AddAllFoundTargets()
	{
		TargetViewModel[] array = FoundTargets.ToArray();
		foreach (TargetViewModel target in array)
		{
			AddFoundTarget(target);
		}
	}

	private void DoWork(object sender, DoWorkEventArgs e)
	{
		BackgroundWorker backgroundWorker = sender as BackgroundWorker;
		List<ITarget> list = new List<ITarget>();
		IProtocol selectedProtocol = m_parent.GetSelectedProtocol();
		foreach (ITarget item in selectedProtocol.FindTargets())
		{
			if (backgroundWorker.CancellationPending)
			{
				return;
			}
			list.Add(item);
		}
		e.Result = list;
	}

	private void RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
	{
		if (!e.Cancelled && e.Error == null && e.Result is IList<ITarget>)
		{
			Dispatcher.CurrentDispatcher.BeginInvoke((Action)delegate
			{
				foreach (ITarget target in (IList<ITarget>)e.Result)
				{
					if (target != null)
					{
						IProtocol protocol = m_parent.Protocols.FirstOrDefault((IProtocol x) => x.Id == target.ProtocolId);
						if (protocol != null)
						{
							FoundTargets.Add(new TargetViewModel(target, protocol));
						}
					}
				}
			});
		}
		m_worker.DoWork -= DoWork;
		m_worker.RunWorkerCompleted -= RunWorkerCompleted;
		m_worker = null;
		IsScanning = false;
	}
}
