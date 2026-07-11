using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using Firaxis.CivTech.AssetObjects;
using Firaxis.MathEx;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using Sce.Atf.Dom;

namespace Firaxis.AssetEditing;

public class LightDirectionTagAdapter : EntityComponentAdapterBase, IPropertyEditingContext
{
	private static readonly float m_gammaIn = 2.2f;

	private static readonly float m_gammaOut = 0.45f;

	private static readonly float m_lightMultiplier = 255f;

	public float AngularFalloff
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.AngularFalloffAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.AngularFalloffAttribute, value);
		}
	}

	public float Blue
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.BlueAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.BlueAttribute, value);
		}
	}

	public bool CastsShadows
	{
		get
		{
			return GetAttribute<bool>(EntitySchema.LightDirectionTagType.CastsShadowsAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.CastsShadowsAttribute, value);
		}
	}

	public float Diameter
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.DiameterAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.DiameterAttribute, value);
		}
	}

	public IEnvironmentLightDirectionTag DirectionTag { get; private set; }

	public float Green
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.GreenAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.GreenAttribute, value);
		}
	}

	public IEnumerable<object> Items
	{
		get
		{
			yield return base.DomNode;
		}
	}

	public Color LightColor
	{
		get
		{
			return Int32ToColor(GetAttribute<int>(EntitySchema.LightDirectionTagType.LightColorAttribute));
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.LightColorAttribute, ColorToInt32(value));
		}
	}

	public float LightIntensity
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.LightIntensityAttribute);
		}
		set
		{
			if (value <= 0f)
			{
				value = 0.1f;
			}
			SetAttribute(EntitySchema.LightDirectionTagType.LightIntensityAttribute, value);
		}
	}

	public string Name
	{
		get
		{
			return GetAttribute<string>(EntitySchema.LightDirectionTagType.NameAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.NameAttribute, value);
		}
	}

	public IEnumerable<System.ComponentModel.PropertyDescriptor> PropertyDescriptors
	{
		get
		{
			foreach (object item in EntitySchema.LightDirectionTagType.Type.GetTag<PropertyDescriptorCollection>())
			{
				yield return (System.ComponentModel.PropertyDescriptor)item;
			}
		}
	}

	public float Red
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.RedAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.RedAttribute, value);
		}
	}

	public float U
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.UAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.UAttribute, value);
		}
	}

	public float V
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.VAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.VAttribute, value);
		}
	}

	public float XPosition
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.XPositionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.XPositionAttribute, value);
		}
	}

	public float YPosition
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.YPositionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.YPositionAttribute, value);
		}
	}

	public float ZPosition
	{
		get
		{
			return GetAttribute<float>(EntitySchema.LightDirectionTagType.ZPositionAttribute);
		}
		set
		{
			SetAttribute(EntitySchema.LightDirectionTagType.ZPositionAttribute, value);
		}
	}

	public void SetColor(float redIntensity, float greenIntensity, float blueIntensity)
	{
		double num = MathExtension.Max(new double[3] { redIntensity, greenIntensity, blueIntensity });
		if (num <= 0.0)
		{
			num = 1.0;
		}
		double num2 = Math.Pow(MathExtension.Max<double>(redIntensity, 0.0) / num, m_gammaOut) * (double)m_lightMultiplier;
		double num3 = Math.Pow(MathExtension.Max<double>(greenIntensity, 0.0) / num, m_gammaOut) * (double)m_lightMultiplier;
		double num4 = Math.Pow(MathExtension.Max<double>(blueIntensity, 0.0) / num, m_gammaOut) * (double)m_lightMultiplier;
		LightIntensity = (float)num;
		LightColor = Color.FromArgb((int)num2, (int)num3, (int)num4);
	}

	public void Update(IEnvironmentLightDirectionTag tag)
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		DirectionTag = tag;
		SetAttributesFromTag(tag);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	public void Update(float newU, float newV, bool resampleLights)
	{
		base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
		IEnvironmentLightEditingContext context = base.DomNode.GetRoot().As<IEnvironmentLightEditingContext>();
		AddUndoTransaction(delegate
		{
			float[] array = context.Importer.ThumbnailUVToDirection(newU, newV);
			float num = (float)Math.Sqrt(array[0] * array[0] + array[1] * array[1] + array[2] * array[2]);
			array[0] /= num;
			array[1] /= num;
			array[2] /= num;
			U = newU;
			V = newV;
			DirectionTag.SetDirection(array[0], array[1], array[2]);
			if (resampleLights)
			{
				IFloatVector3 lightIntensity = context.Cube.GetLightIntensity(array[0], array[1], array[2]);
				Red = lightIntensity.X;
				Green = lightIntensity.Y;
				Blue = lightIntensity.Z;
				SetColor(Red, Green, Blue);
				DirectionTag.SetIntensity(lightIntensity.X, lightIntensity.Y, lightIntensity.Z);
			}
		});
		base.BatchChangelist?.CreateLightDirectionTagChangedEvent(base.EntityAdapter.InstanceEntity as IEnvironmentLightInstance, DirectionTag);
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
	}

	protected override void OnNodeSet()
	{
		base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		base.OnNodeSet();
	}

	private void AddUndoTransaction(Action action)
	{
		EnvironmentLightContext mainContext = base.DomNode.GetRoot().As<EnvironmentLightContext>();
		base.DomNode.GetRoot().As<IFieldSerializer>();
		mainContext.DoTransaction(delegate
		{
			mainContext.AddOperation(new EnvironmentLightTagChangingOperation(mainContext, DirectionTag, action));
		}, "Edit Property");
		mainContext.ApplyChanges();
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
		if (DirectionTag == null)
		{
			return;
		}
		IEnvironmentLightEditingContext environmentLightEditingContext = base.DomNode.GetRoot().As<IEnvironmentLightEditingContext>();
		if (e.AttributeInfo == EntitySchema.LightDirectionTagType.NameAttribute)
		{
			AddUndoTransaction(delegate
			{
				DirectionTag.Name = Name;
			});
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.CastsShadowsAttribute)
		{
			AddUndoTransaction(delegate
			{
				DirectionTag.CastsShadows = CastsShadows;
			});
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.DiameterAttribute)
		{
			AddUndoTransaction(delegate
			{
				DirectionTag.Diameter = Diameter;
			});
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.AngularFalloffAttribute)
		{
			AddUndoTransaction(delegate
			{
				DirectionTag.AngularFalloff = AngularFalloff;
			});
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.XPositionAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.YPositionAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.ZPositionAttribute)
		{
			AddUndoTransaction(delegate
			{
				DirectionTag.SetDirection(XPosition, YPosition, ZPosition);
			});
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.RedAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.GreenAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.BlueAttribute)
		{
			environmentLightEditingContext.SampleIntensityFromMap = false;
			AddUndoTransaction(delegate
			{
				DirectionTag.SetIntensity(Red, Green, Blue);
			});
			base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
			SetColor(Red, Green, Blue);
			base.DomNode.AttributeChanged += DomNode_AttributeChanged;
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.UAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.VAttribute)
		{
			Update(U, V, environmentLightEditingContext.SampleIntensityFromMap);
		}
		else if (e.AttributeInfo == EntitySchema.LightDirectionTagType.LightColorAttribute || e.AttributeInfo == EntitySchema.LightDirectionTagType.LightIntensityAttribute)
		{
			base.DomNode.AttributeChanged -= DomNode_AttributeChanged;
			SetColorIntensities(LightColor, LightIntensity);
			base.DomNode.AttributeChanged += DomNode_AttributeChanged;
			AddUndoTransaction(delegate
			{
				DirectionTag.SetIntensity(Red, Green, Blue);
			});
		}
		environmentLightEditingContext.ApplyChanges();
	}

	private static Color Int32ToColor(int clrInt)
	{
		return Color.FromArgb((clrInt & -16777216) >>> 24, (clrInt & 0xFF0000) >> 16, (clrInt & 0xFF00) >> 8, clrInt & 0xFF);
	}

	private void SetAttributesFromTag(IEnvironmentLightDirectionTag tag)
	{
		if (Name != tag.Name)
		{
			Name = tag.Name;
		}
		if (CastsShadows != tag.CastsShadows)
		{
			CastsShadows = tag.CastsShadows;
		}
		if (XPosition != tag.X)
		{
			XPosition = tag.X;
		}
		if (YPosition != tag.Y)
		{
			YPosition = tag.Y;
		}
		if (ZPosition != tag.Z)
		{
			ZPosition = tag.Z;
		}
		if (Diameter != tag.Diameter)
		{
			Diameter = tag.Diameter;
		}
		if (AngularFalloff != tag.AngularFalloff)
		{
			AngularFalloff = tag.AngularFalloff;
		}
		if (Red != tag.Intensity.X)
		{
			Red = tag.Intensity.X;
		}
		if (Green != tag.Intensity.Y)
		{
			Green = tag.Intensity.Y;
		}
		if (Blue != tag.Intensity.Z)
		{
			Blue = tag.Intensity.Z;
		}
		float[] array = base.DomNode.GetRoot().As<EnvironmentLightContext>().Importer.DirectionToThumbnailUV(XPosition, YPosition, ZPosition);
		if (U != array[0])
		{
			U = array[0];
		}
		if (V != array[1])
		{
			V = array[1];
		}
		SetColor(Red, Green, Blue);
	}

	private void SetColorIntensities(Color color, float lightIntensity)
	{
		double num = Math.Pow((double)(int)color.R / (double)m_lightMultiplier, m_gammaIn);
		Red = (float)num * lightIntensity;
		num = Math.Pow((double)(int)color.G / (double)m_lightMultiplier, m_gammaIn);
		Green = (float)num * lightIntensity;
		num = Math.Pow((double)(int)color.B / (double)m_lightMultiplier, m_gammaIn);
		Blue = (float)num * lightIntensity;
	}
}
