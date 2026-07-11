using System.Collections.Generic;
using Sce.Atf.Wpf.Models;

namespace Sce.Atf.Wpf.Controls.PropertyEditing;

public class StandardValuesEditorContext : NotifyPropertyChangedBase
{
	private PropertyNode m_node;

	private IEnumerable<object> m_standardValues;

	private IEnumerable<object> m_imageList;

	private StandardValuesAttributeBase m_attribute;

	public PropertyNode Node => m_node;

	public IEnumerable<object> StandardValues
	{
		get
		{
			IEnumerable<object> result;
			if (m_attribute != null)
			{
				IEnumerable<object> values = m_attribute.GetValues(m_node.Instances);
				result = values;
			}
			else
			{
				result = m_standardValues;
			}
			return result;
		}
	}

	public IEnumerable<object> ImageList => m_imageList;

	public StandardValuesEditorContext(PropertyNode node)
	{
		SetNode(node);
		m_attribute = m_node.Descriptor.Attributes[typeof(StandardValuesAttributeBase)] as StandardValuesAttributeBase;
	}

	public StandardValuesEditorContext(PropertyNode node, IEnumerable<object> standardValues)
	{
		SetNode(node);
		m_standardValues = standardValues;
	}

	private void SetNode(PropertyNode node)
	{
		m_node = node;
		if (m_node.Descriptor.Attributes[typeof(ImageListAttribute)] is ImageListAttribute imageListAttribute)
		{
			m_imageList = imageListAttribute.ImageKeys;
		}
	}
}
