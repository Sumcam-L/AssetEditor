using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.ATF;

public static class DomNodeExtensions
{
	public static IEnumerable<System.ComponentModel.PropertyDescriptor> GetDefaultProperties(this DomNode domNode)
	{
		PropertyDescriptorCollection defaultProperties = PropertyUtils.GetDefaultProperties(domNode);
		if (defaultProperties.Count > 0)
		{
			return defaultProperties.AsIEnumerable<System.ComponentModel.PropertyDescriptor>();
		}
		return Enumerable.Empty<System.ComponentModel.PropertyDescriptor>();
	}
}
