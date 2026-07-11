using System.Collections.Generic;
using System.Windows;

namespace Firaxis.MVVMBase;

public class ContextPassingList<F> : List<F> where F : Freezable
{
	private DependencyObject _owner;

	public DependencyObject Owner
	{
		get
		{
			return _owner;
		}
		set
		{
			if (value == _owner)
			{
				return;
			}
			if (value == null)
			{
				object[] array = new object[2] { _owner, null };
				using Enumerator enumerator = GetEnumerator();
				while (enumerator.MoveNext())
				{
					F current = enumerator.Current;
					array[1] = current;
					ContextPassingCollectionHelper.RemoveContextFromObject.Invoke(null, array);
				}
			}
			else
			{
				object[] array2 = new object[2] { value, null };
				using Enumerator enumerator2 = GetEnumerator();
				while (enumerator2.MoveNext())
				{
					F current2 = enumerator2.Current;
					array2[1] = current2;
					ContextPassingCollectionHelper.ProvideContextForObject.Invoke(null, array2);
				}
			}
			_owner = value;
		}
	}

	public ContextPassingList()
	{
	}

	public ContextPassingList(DependencyObject owner)
	{
		Owner = owner;
	}
}
