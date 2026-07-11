using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Data;
using Sce.Atf.Wpf.Applications;
using Sce.Atf.Wpf.Controls.Adaptable;

namespace Sce.Atf.Wpf.Models;

public class PaletteContent : NotifyPropertyChangedBase, IDragDropConverter
{
	private ObservableCollection<object> m_items = new ObservableCollection<object>();

	private IPaletteService m_paletteService;

	private ICollectionView m_dataView;

	public ICollectionView Data => m_dataView;

	public PaletteContent(IPaletteService service)
	{
		m_paletteService = service;
		m_dataView = CollectionViewSource.GetDefaultView(m_items);
		m_dataView.GroupDescriptions.Add(new PropertyGroupDescription("Category"));
	}

	public void AddItem(object item, string categoryName)
	{
		m_items.Add(item);
	}

	public void RemoveItem(object item)
	{
		m_items.Remove(item);
	}

	public IEnumerable<object> Convert(IEnumerable<object> items)
	{
		return m_paletteService.Convert(items);
	}
}
