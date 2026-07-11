using System.Collections.Generic;
using Sce.Atf.Adaptation;
using Sce.Atf.Collections;

namespace Sce.Atf.Wpf.Models;

public class AdaptableViewModelCollection<T, U> : AdaptableObservableCollection<T, U> where T : class where U : class, IAdapter, new()
{
	private IAdapterCreator m_adapterCreator;

	private Dictionary<T, U> m_adapterVieModels = new Dictionary<T, U>();

	public AdaptableViewModelCollection(IObservableCollection<T> collection)
		: this(collection, (IAdapterCreator)new AdapterCreator<U>())
	{
	}

	public AdaptableViewModelCollection(IObservableCollection<T> collection, IAdapterCreator adapterCreator)
		: base(collection)
	{
		m_adapterCreator = adapterCreator;
	}

	protected override U Convert(T item)
	{
		if (!m_adapterVieModels.TryGetValue(item, out var value))
		{
			value = m_adapterCreator.GetAdapter(item, typeof(U)) as U;
			m_adapterVieModels.Add(item, value);
		}
		return value;
	}
}
