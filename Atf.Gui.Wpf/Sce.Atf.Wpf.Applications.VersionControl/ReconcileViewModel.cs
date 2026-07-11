using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.VersionControl;

internal class ReconcileViewModel : DialogViewModelBase
{
	private readonly ISourceControlService m_sourceControlService;

	private readonly List<CheckableItem> m_modified;

	private readonly List<CheckableItem> m_notInDepot;

	public ICollectionView Modified { get; private set; }

	public ICollectionView NotInDepot { get; private set; }

	public ReconcileViewModel(SourceControlService sourceControlService, IEnumerable<Uri> modified, IEnumerable<Uri> notInDepot)
	{
		base.Title = "Reconcile Offline Work".Localize();
		m_sourceControlService = sourceControlService;
		m_modified = modified.Select((Uri x) => new CheckableItem(this, x)).ToList();
		m_notInDepot = notInDepot.Select((Uri x) => new CheckableItem(this, x)).ToList();
		Modified = new ListCollectionView(m_modified);
		NotInDepot = new ListCollectionView(m_notInDepot);
	}

	protected override void OnCloseDialog(CloseDialogEventArgs args)
	{
		base.OnCloseDialog(args);
		if (args.DialogResult != true)
		{
			return;
		}
		foreach (CheckableItem item in m_modified)
		{
			if (item.IsChecked)
			{
				m_sourceControlService.CheckOut(item.Uri);
			}
		}
		foreach (CheckableItem item2 in m_notInDepot)
		{
			if (item2.IsChecked)
			{
				m_sourceControlService.Add(item2.Uri);
			}
		}
	}

	public void CheckAllSelected(bool check)
	{
		m_modified.Where((CheckableItem x) => x.IsSelected).ForEach(delegate(CheckableItem x)
		{
			x.IsChecked = check;
		});
		m_notInDepot.Where((CheckableItem x) => x.IsSelected).ForEach(delegate(CheckableItem x)
		{
			x.IsChecked = check;
		});
	}
}
