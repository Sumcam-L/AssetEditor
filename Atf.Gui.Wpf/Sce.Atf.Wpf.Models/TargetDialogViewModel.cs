using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using System.Windows.Input;
using Sce.Atf.Wpf.Applications;

namespace Sce.Atf.Wpf.Models;

public class TargetDialogViewModel : DialogViewModelBase
{
	private readonly ICollectionView m_targetCv;

	private readonly ICollectionView m_protocolsCv;

	public ObservableCollection<TargetViewModel> Targets { get; private set; }

	public ObservableCollection<IProtocol> Protocols { get; private set; }

	public ICommand AddUserTargetCommand { get; private set; }

	public ICommand DeleteTargetCommand { get; private set; }

	public ICommand EditUserTargetCommand { get; private set; }

	public ICommand FindTargetsCommand { get; private set; }

	public event EventHandler<ShowDialogEventArgs> ShowFindTargetsDialog;

	public TargetDialogViewModel(IEnumerable<TargetViewModel> targets, IEnumerable<IProtocol> protocols)
	{
		base.Title = "Targets".Localize();
		Targets = new ObservableCollection<TargetViewModel>(targets);
		m_targetCv = CollectionViewSource.GetDefaultView(Targets);
		Protocols = new ObservableCollection<IProtocol>(protocols);
		m_protocolsCv = CollectionViewSource.GetDefaultView(Protocols);
		AddUserTargetCommand = new DelegateCommand(AddUserTarget, CanAddUserTarget, isAutomaticRequeryDisabled: false);
		DeleteTargetCommand = new DelegateCommand(DeleteTarget, () => GetSelectedTarget() != null, isAutomaticRequeryDisabled: false);
		EditUserTargetCommand = new DelegateCommand(EditUserTarget, CanEditUserTarget, isAutomaticRequeryDisabled: false);
		FindTargetsCommand = new DelegateCommand(FindTargets, () => GetSelectedProtocol()?.CanFindTargets ?? false);
	}

	private bool CanAddUserTarget()
	{
		return GetSelectedProtocol()?.CanCreateUserTarget ?? false;
	}

	private void AddUserTarget()
	{
		IProtocol selectedProtocol = GetSelectedProtocol();
		if (selectedProtocol != null)
		{
			ITarget target = selectedProtocol.CreateUserTarget(null);
			if (selectedProtocol.EditUserTarget(target))
			{
				Targets.Add(new TargetViewModel(target, selectedProtocol));
				EnsureSelection();
			}
		}
	}

	private void DeleteTarget()
	{
		if (m_targetCv.CurrentItem is TargetViewModel item)
		{
			int val = Targets.IndexOf(item);
			Targets.Remove(item);
			val = Math.Min(val, Targets.Count - 1);
			if (val > 0)
			{
				Targets[val].IsSelected = true;
			}
		}
	}

	private bool CanEditUserTarget()
	{
		return m_targetCv.CurrentItem is TargetViewModel targetViewModel && targetViewModel.Protocol.CanCreateUserTarget;
	}

	private void EditUserTarget()
	{
		if (m_targetCv.CurrentItem is TargetViewModel targetViewModel)
		{
			targetViewModel.Protocol.EditUserTarget(targetViewModel.Target);
		}
	}

	private void FindTargets()
	{
		this.ShowFindTargetsDialog.Raise(this, new ShowDialogEventArgs(new FindTargetsViewModel(this)));
		EnsureSelection();
	}

	private ITarget GetSelectedTarget()
	{
		return (m_targetCv.CurrentItem is TargetViewModel targetViewModel) ? targetViewModel.Target : null;
	}

	internal IProtocol GetSelectedProtocol()
	{
		return m_protocolsCv.CurrentItem as IProtocol;
	}

	internal void EnsureSelection()
	{
		if (Targets.Count > 0 && !Targets.Any((TargetViewModel x) => x.IsSelected))
		{
			Targets[Targets.Count - 1].IsSelected = true;
		}
	}
}
