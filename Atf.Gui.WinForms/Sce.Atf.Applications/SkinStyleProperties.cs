using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

public class SkinStyleProperties : CustomTypeDescriptorNodeAdapter, IDynamicTypeDescriptor, ICustomTypeDescriptor
{
	bool IDynamicTypeDescriptor.CacheableProperties => false;

	protected override System.ComponentModel.PropertyDescriptor[] GetPropertyDescriptors()
	{
		List<System.ComponentModel.PropertyDescriptor> list = new List<System.ComponentModel.PropertyDescriptor>();
		IList<DomNode> childList = base.DomNode.GetChildList(SkinSchema.styleType.setterChild);
		foreach (DomNode item in childList)
		{
			ProcessSetterType(item, "", list);
		}
		return list.ToArray();
	}

	private void ProcessSetterType(DomNode setter, string parentPropName, List<System.ComponentModel.PropertyDescriptor> descriptors)
	{
		string text = (string)setter.GetAttribute(SkinSchema.setterType.propertyNameAttribute);
		if (string.IsNullOrWhiteSpace(text))
		{
			return;
		}
		string propName = ((!string.IsNullOrEmpty(parentPropName)) ? (parentPropName + "->" + text) : text);
		DomNode child = setter.GetChild(SkinSchema.setterType.valueInfoChild);
		if (child != null)
		{
			ProcessValueInfo(child, propName, descriptors);
		}
		DomNode child2 = setter.GetChild(SkinSchema.setterType.listInfoChild);
		if (child2 == null)
		{
			return;
		}
		foreach (DomNode child3 in child2.GetChildList(SkinSchema.listInfoType.valueInfoChild))
		{
			ProcessValueInfo(child3, propName, descriptors);
		}
	}

	private void ProcessValueInfo(DomNode valInfo, string propName, List<System.ComponentModel.PropertyDescriptor> descriptors)
	{
		string text = (string)valInfo.GetAttribute(SkinSchema.valueInfoType.typeAttribute);
		Type type = SkinUtil.GetType(text);
		if (type == typeof(Font))
		{
			FontDescriptor item = new FontDescriptor(valInfo, propName, null, null, null, null);
			descriptors.Add(item);
			return;
		}
		GetEditorAndConverter(type, out var editor, out var converter);
		if (editor != null)
		{
			SkinSetterAttributePropertyDescriptor item2 = new SkinSetterAttributePropertyDescriptor(valInfo, propName, SkinSchema.valueInfoType.valueAttribute, null, null, isReadOnly: false, editor, converter);
			descriptors.Add(item2);
			return;
		}
		DomNode child = valInfo.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
		if (child != null)
		{
			IList<DomNode> childList = child.GetChildList(SkinSchema.constructorParamsType.valueInfoChild);
			if (childList.Count == 1)
			{
				ProcessValueInfo(childList[0], propName, descriptors);
			}
			else if (text == "Sce.Atf.Controls.SyntaxEditorControl.TextHighlightStyle")
			{
				string text2 = (string)childList[0].GetAttribute(SkinSchema.valueInfoType.valueAttribute);
				string propName2 = propName + "->" + text2;
				ProcessValueInfo(childList[1], propName2, descriptors);
			}
			else
			{
				int num = 1;
				string text3 = propName + " : Arg_";
				foreach (DomNode item3 in childList)
				{
					string propName3 = text3 + num;
					ProcessValueInfo(item3, propName3, descriptors);
					num++;
				}
			}
		}
		foreach (DomNode child2 in valInfo.GetChildList(SkinSchema.valueInfoType.setterChild))
		{
			ProcessSetterType(child2, propName, descriptors);
		}
	}

	private void GetEditorAndConverter(Type type, out object editor, out TypeConverter converter)
	{
		editor = null;
		converter = null;
		if (!(type == null))
		{
			if (type == typeof(Color))
			{
				ColorPickerEditor colorPickerEditor = new ColorPickerEditor();
				colorPickerEditor.EnableAlpha = true;
				editor = colorPickerEditor;
				converter = new StringColorConverter();
			}
			else if (type.IsEnum)
			{
				editor = new LongEnumEditor(type);
			}
		}
	}
}
