using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Data;
using Sce.Atf.Applications;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Applications.VersionControl;

internal class CheckInViewModel : DialogViewModelBase
{
	private readonly ISourceControlService m_sourceControlService;

	private readonly List<CheckInItem> m_checkInItems;

	public ICollectionView Items { get; private set; }

	public string Description { get; set; }

	public CheckInViewModel(SourceControlService sourceControlService, IEnumerable<IResource> toCheckIn)
	{
		base.Title = "Check In Files".Localize();
		m_sourceControlService = sourceControlService;
		m_checkInItems = toCheckIn.Select((IResource x) => new CheckInItem(this, x)).ToList();
		Items = new ListCollectionView(m_checkInItems);
	}

	protected override void OnCloseDialog(CloseDialogEventArgs args)
	{
		base.OnCloseDialog(args);
		if (args.DialogResult == true)
		{
			IEnumerable<Uri> uris = from item in m_checkInItems
				where item.IsChecked
				select item.Resource.Uri;
			m_sourceControlService.CheckIn(uris, Description);
		}
	}

	public void CheckAllSelected(bool check)
	{
		m_checkInItems.Where((CheckInItem x) => x.IsSelected).ForEach(delegate(CheckInItem x)
		{
			x.IsChecked = check;
		});
	}
}
