using System;
using System.Collections.Generic;
using Firaxis.CivTech;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MVVMBase;

namespace UtilityTools.ViewModels;

public class InstanceEntityWindowViewModel : Notifier, IDisposable
{
	private InstanceEntityViewModel m_instanceEntityViewModel;

	public InstanceEntityViewModel InstanceEntityViewModel
	{
		get
		{
			return m_instanceEntityViewModel;
		}
		private set
		{
			if (m_instanceEntityViewModel != value)
			{
				m_instanceEntityViewModel = value;
				OnPropertyChanged("InstanceEntityViewModel");
			}
		}
	}

	public event EventHandler CloseEvent;

	public InstanceEntityWindowViewModel(InstanceEntityViewModel viewModel)
	{
		InstanceEntityViewModel = viewModel;
		InstanceEntityViewModel.CloseEvent += InstanceEntityViewModel_CloseEvent;
	}

	public InstanceEntityWindowViewModel(ICivTechService civTechSvc, IInstanceEntity instanceEntity, IEnumerable<string> availableClassList, IInstanceSet instances)
	{
		InstanceEntityViewModel = new InstanceEntityViewModel(civTechSvc, instanceEntity, availableClassList, instances);
		InstanceEntityViewModel.CloseEvent += InstanceEntityViewModel_CloseEvent;
	}

	public void Dispose()
	{
		if (InstanceEntityViewModel != null)
		{
			InstanceEntityViewModel.CloseEvent -= InstanceEntityViewModel_CloseEvent;
			InstanceEntityViewModel.Dispose();
			InstanceEntityViewModel = null;
		}
	}

	public void RevertTemporaryHiding()
	{
		if (InstanceEntityViewModel != null)
		{
			InstanceEntityViewModel.RevertTemporaryHiding();
		}
	}

	protected virtual void InstanceEntityViewModel_CloseEvent(object sender, EventArgs e)
	{
		OnCloseEvent();
	}

	protected virtual void OnCloseEvent()
	{
		this.CloseEvent?.Invoke(this, EventArgs.Empty);
	}
}
