using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Firaxis.ATF;
using Firaxis.Collections;
using Sce.Atf;
using Sce.Atf.Applications;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class ArtDefElementMultiSelectPropertyEditingContext : MultiSelectPropertyEditingContextBase
{
	private IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyList;

	private IEnumerable<object> SelectedItems { get; set; }

	public ArtDefElementMultiSelectPropertyEditingContext(ITransactionContext transContext, IEnumerable<object> selection)
		: base(transContext)
	{
		base.TransactionContext = transContext;
		SelectedItems = selection;
	}

	protected override IEnumerable<System.ComponentModel.PropertyDescriptor> GetPropertyDescriptors()
	{
		if (PropertyList == null)
		{
			IList<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
			if (base.Items.Any())
			{
				foreach (System.ComponentModel.PropertyDescriptor prop in GetSharedProperties())
				{
					if (prop is MultiPropertyDescriptor)
					{
						list.Add(prop);
					}
					else if (base.Items.Select((object item) => prop.GetValue(item)).UniqueCount() == 1)
					{
						list.Add(prop);
					}
					else if (prop.GetEditor(typeof(EmbeddedCollectionEditor)) == null)
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
		return SelectedItems;
	}

	private IEnumerable<System.ComponentModel.PropertyDescriptor> GetSharedProperties()
	{
		IPropertyEditingContext propertyEditingContext = base.Items.OfType<IPropertyEditingContext>().FirstOrDefault();
		if (propertyEditingContext == null)
		{
			return PropertyUtils.GetSharedProperties(base.Items);
		}
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>(propertyEditingContext.PropertyDescriptors);
		foreach (IPropertyEditingContext item in base.Items.OfType<IPropertyEditingContext>())
		{
			for (int i = 0; i < list.Count; i++)
			{
				if (!item.PropertyDescriptors.Contains(list[i]))
				{
					list.Remove(list[i]);
				}
			}
		}
		return list;
	}
}
