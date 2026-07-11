using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf;
using Sce.Atf.Controls.PropertyEditing;

namespace Firaxis.ATF;

public class MultiSelectPropertyEditingContext : MultiSelectPropertyEditingContextBase
{
	private IEnumerable<PropertyDescriptor> PropertyList;

	private Selection<object> MultiSelection { get; set; }

	public MultiSelectPropertyEditingContext(ITransactionContext transContext, Selection<object> selection)
		: base(transContext)
	{
		MultiSelection = selection;
	}

	protected override IEnumerable<PropertyDescriptor> GetPropertyDescriptors()
	{
		if (PropertyList == null)
		{
			IList<PropertyDescriptor> list = new List<PropertyDescriptor>();
			foreach (PropertyDescriptor prop in PropertyUtils.GetSharedProperties(base.Items))
			{
				object firstVal = prop.GetValue(base.Items.First());
				if (firstVal != null)
				{
					if (base.Items.All((object item) => prop.GetValue(item).Equals(firstVal)))
					{
						list.Add(prop);
					}
					else
					{
						list.Add(new MultiSelectProxyDescriptor(prop));
					}
				}
			}
			PropertyList = list;
		}
		return PropertyList;
	}

	protected override IEnumerable<object> GetItems()
	{
		return MultiSelection;
	}
}
