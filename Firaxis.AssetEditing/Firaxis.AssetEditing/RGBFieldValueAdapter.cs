using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Firaxis.ATF;
using Firaxis.CivTech.AssetObjects;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Controls.PropertyEditing;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class RGBFieldValueAdapter : FieldValueAdapter
{
	private class ColorAttributePropertyDescriptor : AttributeFieldPropertyDescriptor, ICustomDrawProperty
	{
		public ColorAttributePropertyDescriptor(string name, AttributeInfo attributeInfo, string category, string description, Func<bool> isReadOnlyFunctor, object editor, TypeConverter typeConverter)
			: base(name, attributeInfo, category, description, isReadOnlyFunctor, editor, typeConverter)
		{
		}

		public virtual void DrawValue(Graphics g, Font cellFont, Brush cellBrush, Rectangle valueRect, System.ComponentModel.PropertyDescriptor valueProp, object valueObj, bool isSelected)
		{
			Rectangle rectangle = valueRect;
			RGBFieldValueAdapter rGBFieldValueAdapter = valueObj.As<RGBFieldValueAdapter>();
			if (rGBFieldValueAdapter == null)
			{
				DynamicFieldPropertyDescriptorBase dynamicFieldPropertyDescriptorBase = valueProp.As<DynamicFieldPropertyDescriptorBase>();
				if (dynamicFieldPropertyDescriptorBase != null)
				{
					rGBFieldValueAdapter = dynamicFieldPropertyDescriptorBase.GetFieldAdapter(valueObj).As<RGBFieldValueAdapter>();
				}
			}
			rectangle.Inflate(-2, -2);
			if (rGBFieldValueAdapter != null)
			{
				object obj = Converter?.ConvertTo(rGBFieldValueAdapter.ValueDataAsObject, typeof(Color));
				if (obj != null && obj is object color)
				{
					Rectangle rectangle2 = rectangle;
					rectangle2.Width = 4 * rectangle2.Height / 3;
					rectangle.Width -= rectangle2.Width;
					rectangle.Offset(rectangle2.Width + 1, 0);
					if (rectangle.Width <= 0)
					{
						rectangle = rectangle2;
						rectangle.Inflate(-1, 1);
					}
					using Brush brush = new SolidBrush((Color)color);
					g.FillRectangle(brush, rectangle2);
				}
			}
			string propertyText = PropertyUtils.GetPropertyText(valueObj, valueProp);
			StringFormat stringFormat = new StringFormat();
			stringFormat.Alignment = StringAlignment.Far;
			stringFormat.FormatFlags = StringFormatFlags.NoWrap;
			stringFormat.Trimming = StringTrimming.EllipsisPath;
			g.DrawString(propertyText, cellFont, cellBrush, rectangle, stringFormat);
		}
	}

	private IEnumerable<System.ComponentModel.PropertyDescriptor> m_descriptors;

	public override object DefaultDataAsObject => ColorToInt32(((IRGBValue)base.Parameter.DefaultValue).ParameterValue);

	public Color ParameterValue
	{
		get
		{
			object attribute = GetAttribute<object>(FieldSchema.RGBFieldValueType.ValueAttribute);
			if (attribute is int)
			{
				return Int32ToColor((int)attribute);
			}
			if (attribute is Color)
			{
				return (Color)attribute;
			}
			return Color.Black;
		}
		set
		{
			SetAttribute(FieldSchema.RGBFieldValueType.ValueAttribute, ColorToInt32(value));
		}
	}

	public override object ValueDataAsObject
	{
		get
		{
			return GetAttribute<int>(FieldSchema.RGBFieldValueType.ValueAttribute);
		}
		set
		{
			int num = 0;
			if (value is int)
			{
				num = (int)value;
			}
			else if (value is Color)
			{
				num = ColorToInt32((Color)value);
			}
			SetAttribute(FieldSchema.RGBFieldValueType.ValueAttribute, num);
		}
	}

	private IRGBParameter RGBParameter => base.Parameter as IRGBParameter;

	private IRGBValue RGBValue => base.Value as IRGBValue;

	public override IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors => m_descriptors;

	public override void InitializeEditor(Func<bool> readOnlyFunctor)
	{
		base.InitializeEditor(readOnlyFunctor);
		ColorPickerEditor colorPickerEditor = new ColorPickerEditor();
		colorPickerEditor.EnableAlpha = true;
		m_descriptors = new List<System.ComponentModel.PropertyDescriptor>(new System.ComponentModel.PropertyDescriptor[1] { CreateProxyPropertyDescriptorIfNeeded(new ColorAttributePropertyDescriptor(BindDynamicValueOrDefault(base.Parameter.Name, "RGB".Localize()), FieldSchema.RGBFieldValueType.ValueAttribute, BindDynamicValueOrDefault(base.Parameter.Category, "Value".Localize()), BindDynamicValueOrDefault(base.Parameter.Description, "RGB Value description".Localize()), readOnlyFunctor, colorPickerEditor, new IntColorConverter()), Name) });
	}

	public override void AddNativeField(IValueSet valSet, IParameter valParam)
	{
		base.AddNativeField(valSet, valParam);
		RGBValue.ParameterValue = ParameterValue;
	}

	public override void AssignDefaultValue()
	{
		ParameterValue = (RGBParameter.DefaultValue as IRGBValue).ParameterValue;
	}

	public override bool RequiresUpdate(IValue val)
	{
		bool num = base.RequiresUpdate(val);
		bool flag = false;
		if (!num && val is IRGBValue iRGBValue)
		{
			flag = iRGBValue.ParameterValue == ParameterValue;
		}
		if (!num)
		{
			return !flag;
		}
		return true;
	}

	public override void UpdateDomFromNative(IValue val)
	{
		base.UpdateDomFromNative(val);
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		CopyValue(val);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public override void UpdateNativeFromDom()
	{
		RGBValue.ParameterValue = ParameterValue;
	}

	public override void CopyValue(IValue val)
	{
		base.CopyValue(val);
		ParameterValue = ((IRGBValue)val).ParameterValue;
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private static int ColorToInt32(Color clr)
	{
		int a = clr.A;
		int num = 0 | (a << 24);
		a = clr.R;
		int num2 = num | (a << 16);
		a = clr.G;
		int num3 = num2 | (a << 8);
		a = clr.B;
		return num3 | a;
	}

	private void DomNode_AttributeChanged(object sender, AttributeEventArgs e)
	{
		if (e.AttributeInfo == FieldSchema.RGBFieldValueType.ValueAttribute)
		{
			if (e.NewValue is Color)
			{
				RGBValue.ParameterValue = (Color)e.NewValue;
			}
			else
			{
				RGBValue.ParameterValue = Int32ToColor((int)e.NewValue);
			}
		}
	}

	private static Color Int32ToColor(int clrInt)
	{
		int alpha = clrInt >>> 24;
		int red = (clrInt & 0xFF0000) >> 16;
		int green = (clrInt & 0xFF00) >> 8;
		int blue = clrInt & 0xFF;
		return Color.FromArgb(alpha, red, green, blue);
	}
}
