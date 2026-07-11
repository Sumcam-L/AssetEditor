using System;
using System.ComponentModel;
using System.Drawing;
using Sce.Atf.Dom;

namespace Sce.Atf.Applications;

internal class FontDescriptor : Sce.Atf.Dom.PropertyDescriptor
{
	private DomNode m_domObject;

	private Font m_font;

	public FontDescriptor(DomNode domObj, string name, string category, string description, object editor, TypeConverter typeConverter)
		: base(name, typeof(Font), category, description, isReadOnly: false, editor, typeConverter)
	{
		m_domObject = domObj;
	}

	public override DomNode GetNode(object component)
	{
		return m_domObject;
	}

	public override bool Equals(object obj)
	{
		FontDescriptor fontDescriptor = obj as FontDescriptor;
		if (!base.Equals((object)fontDescriptor))
		{
			return false;
		}
		if (m_domObject != fontDescriptor.m_domObject)
		{
			return false;
		}
		if (Name != fontDescriptor.Name)
		{
			return false;
		}
		return true;
	}

	public override int GetHashCode()
	{
		return m_domObject.GetHashCode();
	}

	public override bool CanResetValue(object component)
	{
		return false;
	}

	public override void ResetValue(object component)
	{
	}

	public override object GetValue(object component)
	{
		try
		{
			if (m_font == null)
			{
				DomNode child = m_domObject.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
				string familyName = null;
				float emSize = 1f;
				FontStyle style = FontStyle.Regular;
				foreach (DomNode child2 in child.GetChildren(SkinSchema.constructorParamsType.valueInfoChild))
				{
					string typeName = (string)child2.GetAttribute(SkinSchema.valueInfoType.typeAttribute);
					Type type = SkinUtil.GetType(typeName);
					string text = (string)child2.GetAttribute(SkinSchema.valueInfoType.valueAttribute);
					if (type == typeof(string))
					{
						familyName = text;
					}
					else if (type == typeof(float))
					{
						emSize = float.Parse(text);
					}
					else if (type == typeof(FontStyle))
					{
						style = (FontStyle)Enum.Parse(typeof(FontStyle), text);
					}
				}
				m_font = new Font(familyName, emSize, style);
			}
		}
		catch
		{
		}
		return m_font;
	}

	public override void SetValue(object component, object value)
	{
		if (m_font != null && m_font.Equals(value))
		{
			return;
		}
		if (m_font != null)
		{
			m_font.Dispose();
		}
		m_font = (Font)value;
		DomNode child = m_domObject.GetChild(SkinSchema.valueInfoType.constructorParamsChild);
		foreach (DomNode child2 in child.GetChildren(SkinSchema.constructorParamsType.valueInfoChild))
		{
			string typeName = (string)child2.GetAttribute(SkinSchema.valueInfoType.typeAttribute);
			Type type = SkinUtil.GetType(typeName);
			if (type == typeof(string))
			{
				child2.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.FontFamily.Name);
			}
			else if (type == typeof(float))
			{
				child2.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.Size.ToString());
			}
			else if (type == typeof(FontStyle))
			{
				child2.SetAttribute(SkinSchema.valueInfoType.valueAttribute, m_font.Style.ToString());
			}
		}
	}
}
