using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Sce.Atf.Adaptation;

namespace Sce.Atf.Applications;

[Export(typeof(IInitializable))]
[Export(typeof(ISkinService))]
[Export(typeof(SkinService))]
[PartCreationPolicy(CreationPolicy.Shared)]
public class SkinService : IInitializable, ISkinService
{
	private static ISkin s_activeSkin = null;

	private Form m_mainForm;

	private static Dictionary<Form, FormNcRenderer> s_formNcRenderers = new Dictionary<Form, FormNcRenderer>();

	private static FormNcRenderer.SkinInfo s_ncSkin = new FormNcRenderer.SkinInfo();

	private static readonly HashSet<WeakKey<object>> s_skinnableObjects = new HashSet<WeakKey<object>>();

	private static readonly Dictionary<Tuple<WeakKey<object>, PropertyInfo>, object> s_originalPropertyValues = new Dictionary<Tuple<WeakKey<object>, PropertyInfo>, object>();

	private static readonly Type s_inertButtonType = Type.GetType("WeifenLuo.WinFormsUI.Docking.VS2005DockPaneCaption+InertButton, WeifenLuo.WinFormsUI.Docking");

	public ISkin ActiveSkin
	{
		get
		{
			return s_activeSkin;
		}
		set
		{
			if (s_activeSkin != value)
			{
				s_activeSkin = value;
				if (s_activeSkin != null)
				{
					ApplyActiveSkin();
				}
				else
				{
					ResetSkin();
				}
			}
		}
	}

	[Import(AllowDefault = true)]
	public Form MainForm
	{
		get
		{
			return m_mainForm;
		}
		set
		{
			if (m_mainForm != null && m_mainForm != value)
			{
				throw new InvalidOperationException("setting the MainForm multiple times is not currently supported");
			}
			m_mainForm = value;
			m_mainForm.Load += m_mainForm_Load;
			m_mainForm.FormClosed += m_mainForm_FormClosed;
			FormNcRenderer formNcRenderer = new FormNcRenderer(m_mainForm);
			formNcRenderer.Skin = s_ncSkin;
			formNcRenderer.CustomPaintDisabled = ActiveSkin == null;
			s_formNcRenderers.Add(m_mainForm, formNcRenderer);
		}
	}

	protected virtual IEnumerable<Control> SkinnableControls => SkinnableObjects.OfType<Control>();

	protected virtual IEnumerable<object> SkinnableObjects
	{
		get
		{
			if (MainForm != null)
			{
				yield return MainForm;
			}
			s_skinnableObjects.RemoveWhere((WeakKey<object> key) => !key.IsAlive);
			foreach (WeakKey<object> existing in s_skinnableObjects)
			{
				object target = existing.Target;
				Control ctrl = target as Control;
				if (target != null && (ctrl == null || !ctrl.IsDisposed))
				{
					yield return target;
				}
			}
		}
	}

	public event EventHandler SkinChangedOrApplied;

	public void Initialize()
	{
		WinFormsUtil.WindowCreated += WindowCreated;
		WinFormsUtil.WindowDestroyed += WindowDestroyed;
	}

	public static void ApplyActiveSkin(object control)
	{
		ApplyActiveSkin(control, null);
	}

	private void ApplyActiveSkin()
	{
		if (ActiveSkin == null)
		{
			return;
		}
		ApplySkinToNonClientArea();
		RestoreOriginalPropertyValues();
		s_originalPropertyValues.Clear();
		HashSet<object> skinnedControls = new HashSet<object>();
		foreach (object skinnableObject in SkinnableObjects)
		{
			ApplyActiveSkin(skinnableObject, skinnedControls);
		}
		this.SkinChangedOrApplied.Raise(this, EventArgs.Empty);
	}

	private void ResetSkin()
	{
		RestoreOriginalPropertyValues();
		s_originalPropertyValues.Clear();
		ActiveSkin = null;
		foreach (FormNcRenderer value in s_formNcRenderers.Values)
		{
			value.CustomPaintDisabled = true;
		}
		this.SkinChangedOrApplied.Raise(this, EventArgs.Empty);
	}

	private void m_mainForm_Load(object sender, EventArgs e)
	{
		ApplyActiveSkin();
	}

	private void m_mainForm_FormClosed(object sender, FormClosedEventArgs e)
	{
		WinFormsUtil.WindowCreated -= WindowCreated;
		WinFormsUtil.WindowDestroyed -= WindowDestroyed;
	}

	private static void WindowCreated(Form form)
	{
		ApplyActiveSkin(form, null);
		s_skinnableObjects.Add(new WeakKey<object>(form));
		FormNcRenderer formNcRenderer = new FormNcRenderer(form);
		formNcRenderer.Skin = s_ncSkin;
		formNcRenderer.CustomPaintDisabled = s_activeSkin == null;
		s_formNcRenderers.Add(form, formNcRenderer);
	}

	private static void WindowDestroyed(Form form)
	{
		s_skinnableObjects.Remove(new WeakKey<object>(form));
		s_formNcRenderers.Remove(form);
	}

	private static void RestoreOriginalPropertyValues()
	{
		List<Tuple<WeakKey<object>, PropertyInfo>> list = new List<Tuple<WeakKey<object>, PropertyInfo>>();
		foreach (KeyValuePair<Tuple<WeakKey<object>, PropertyInfo>, object> s_originalPropertyValue in s_originalPropertyValues)
		{
			Tuple<WeakKey<object>, PropertyInfo> key = s_originalPropertyValue.Key;
			object target = key.Item1.Target;
			if (target != null)
			{
				if (target.Is<Control>() && target.As<Control>().IsDisposed)
				{
					list.Add(key);
					continue;
				}
				key.Item2.SetValue(target, s_originalPropertyValue.Value, null);
				if (target is ToolStrip toolStrip)
				{
					ApplyCommonValuesToToolStrip(toolStrip);
				}
			}
			else
			{
				list.Add(key);
			}
		}
		foreach (Tuple<WeakKey<object>, PropertyInfo> item in list)
		{
			s_originalPropertyValues.Remove(item);
		}
	}

	private static void ApplyActiveSkin(object control, HashSet<object> skinnedControls)
	{
		s_skinnableObjects.Add(new WeakKey<object>(control));
		if (s_activeSkin != null)
		{
			SaveOriginalPropertyValues(control);
			ApplyNewPropertyValues(control, skinnedControls);
		}
	}

	private static void SaveOriginalPropertyValues(object obj)
	{
		if (obj == null)
		{
			return;
		}
		Control control = obj as Control;
		control?.SuspendLayout();
		Type type = obj.GetType();
		SkinStyle skinStyle = FindBestSkinStyle(type, s_activeSkin.Styles);
		if (skinStyle != null)
		{
			Type targetType = skinStyle.TargetType;
			foreach (Setter setter in skinStyle.Setters)
			{
				try
				{
					PropertyInfo property = targetType.GetProperty(setter.PropertyName, Skin.PropertyLookupType);
					if (property != null)
					{
						Tuple<WeakKey<object>, PropertyInfo> key = new Tuple<WeakKey<object>, PropertyInfo>(new WeakKey<object>(obj), property);
						if (!s_originalPropertyValues.ContainsKey(key))
						{
							object obj2 = property.GetValue(obj, null);
							if (obj2 != null && typeof(ICloneable).IsAssignableFrom(property.PropertyType))
							{
								obj2 = ((ICloneable)obj2).Clone();
							}
							s_originalPropertyValues.Add(key, obj2);
						}
					}
					else
					{
						Outputs.WriteLine(OutputMessageType.Warning, string.Concat("The skin ", s_activeSkin.Uri, " attempted to set a property on an object of type ", targetType, ", but this property, ", setter.PropertyName, ", doesn't exist."));
					}
				}
				catch (Exception)
				{
				}
			}
		}
		if (control == null)
		{
			return;
		}
		foreach (Control control2 in control.Controls)
		{
			SaveOriginalPropertyValues(control2);
		}
		control.ResumeLayout();
	}

	private void ApplySkinToNonClientArea()
	{
		if (ActiveSkin == null)
		{
			return;
		}
		SkinStyle skinStyle = null;
		foreach (SkinStyle style in ActiveSkin.Styles)
		{
			if (s_ncSkin.GetType() == style.TargetType)
			{
				skinStyle = style;
			}
		}
		if (skinStyle != null)
		{
			foreach (Setter setter in skinStyle.Setters)
			{
				PropertyInfo property = skinStyle.TargetType.GetProperty(setter.PropertyName, Skin.PropertyLookupType);
				object instance;
				if (setter.ValueInfo != null)
				{
					instance = GetInstance(setter.ValueInfo);
				}
				else
				{
					if (setter.ListInfo == null)
					{
						throw new Exception("Setter '" + setter.PropertyName + "' does not have its ValueInfo nor ListInfo set");
					}
					instance = GetInstance(setter.ListInfo);
				}
				property.SetValue(s_ncSkin, instance, null);
			}
			{
				foreach (FormNcRenderer value in s_formNcRenderers.Values)
				{
					value.Skin = s_ncSkin;
					value.CustomPaintDisabled = false;
				}
				return;
			}
		}
		foreach (FormNcRenderer value2 in s_formNcRenderers.Values)
		{
			value2.CustomPaintDisabled = true;
		}
	}

	private static void ApplyNewPropertyValues(object obj, HashSet<object> skinnedControls)
	{
		if (obj == null)
		{
			return;
		}
		Control control = obj as Control;
		control?.SuspendLayout();
		if (skinnedControls == null || skinnedControls.Add(obj))
		{
			Type type = obj.GetType();
			if (control == null || !control.IsDisposed)
			{
				SkinStyle skinStyle = FindBestSkinStyle(type, s_activeSkin.Styles);
				if (skinStyle != null)
				{
					foreach (Setter setter in skinStyle.Setters)
					{
						PropertyInfo property = skinStyle.TargetType.GetProperty(setter.PropertyName, Skin.PropertyLookupType);
						object instance;
						if (setter.ValueInfo != null)
						{
							instance = GetInstance(setter.ValueInfo);
						}
						else
						{
							if (setter.ListInfo == null)
							{
								throw new Exception("Setter '" + setter.PropertyName + "' does not have its ValueInfo nor ListInfo set");
							}
							instance = GetInstance(setter.ListInfo);
						}
						if (property != null)
						{
							property.SetValue(obj, instance, null);
							continue;
						}
						Outputs.WriteLine(OutputMessageType.Diagnostic, "Failed to apply skin \"{0}\" to property \"{1}\" on object type \"{2}\" using style associated with \"{3}\"", s_activeSkin.Uri, setter.PropertyName, type, skinStyle.TargetType);
					}
				}
				else
				{
					Outputs.WriteLine(OutputMessageType.Diagnostic, "Failed to apply skin \"{0}\" to control type \"{1}\" because no matching skin style was found", s_activeSkin.Uri, type);
				}
			}
			if (control != null && type == s_inertButtonType)
			{
				control.BackColor = Color.Transparent;
			}
			if (obj is ToolStrip)
			{
				ApplyCommonValuesToToolStrip((ToolStrip)obj);
			}
		}
		if (control == null)
		{
			return;
		}
		control.ControlAdded -= Control_ControlAdded;
		control.ControlAdded += Control_ControlAdded;
		foreach (Control control2 in control.Controls)
		{
			ApplyNewPropertyValues(control2, skinnedControls);
		}
		control.ResumeLayout();
	}

	private static void Control_ControlAdded(object sender, ControlEventArgs e)
	{
		if (s_activeSkin == null || e.Control == null || e.Control.IsDisposed)
		{
			return;
		}
		ApplyActiveSkin(e.Control, null);
	}

	private static SkinStyle FindBestSkinStyle(Type targetType, IList<SkinStyle> roots)
	{
		foreach (SkinStyle root in roots)
		{
			if (targetType == root.TargetType)
			{
				return root;
			}
			if (targetType.IsSubclassOf(root.TargetType))
			{
				SkinStyle skinStyle = FindBestSkinStyle(targetType, root.Dependents);
				if (skinStyle != null)
				{
					return skinStyle;
				}
				return root;
			}
		}
		return null;
	}

	private static void ApplyCommonValuesToToolStrip(ToolStrip toolStrip)
	{
		ApplyCommonValuesToToolStrips(toolStrip.Items, toolStrip.Font, toolStrip.BackColor, toolStrip.ForeColor);
	}

	private static void ApplyCommonValuesToToolStrips(IEnumerable items, Font font, Color backColor, Color foreColor)
	{
		foreach (ToolStripItem item in items)
		{
			item.ForeColor = foreColor;
			item.BackColor = backColor;
			item.Font = font;
			if (item is ToolStripDropDownItem)
			{
				ApplyCommonValuesToToolStrips(((ToolStripDropDownItem)item).DropDownItems, font, backColor, foreColor);
			}
		}
	}

	private static object GetInstance(ValueInfo valueInfo)
	{
		object obj = null;
		if (valueInfo.ConstructorParams.Count == 0)
		{
			obj = ((valueInfo.Type == typeof(string)) ? valueInfo.Value.Clone() : Activator.CreateInstance(valueInfo.Type));
		}
		else
		{
			Type[] types = valueInfo.ConstructorParams.Select((ValueInfo param) => param.Type).ToArray();
			ConstructorInfo constructor = valueInfo.Type.GetConstructor(types);
			if (constructor != null)
			{
				obj = constructor.Invoke(valueInfo.ConstructorParams.Select((ValueInfo param) => GetInstance(param)).ToArray());
			}
		}
		foreach (Setter setter in valueInfo.Setters)
		{
			PropertyInfo property = valueInfo.Type.GetProperty(setter.PropertyName, Skin.PropertyLookupType);
			if (obj == null)
			{
				throw new InvalidOperationException("Could not find a property named '" + setter.PropertyName + "'.");
			}
			if (setter.ValueInfo != null)
			{
				property.SetValue(obj, GetInstance(setter.ValueInfo), null);
				continue;
			}
			if (setter.ListInfo != null)
			{
				property.SetValue(obj, GetInstance(setter.ListInfo), null);
				continue;
			}
			throw new InvalidOperationException("Setter '" + setter.PropertyName + "' doesn't have a valueInfo, nor listInfo, specified.  Must have one (and only one) of either.");
		}
		if (valueInfo.Setters.Count == 0 && valueInfo.Value != null && valueInfo.Converter.CanConvertTo(null, valueInfo.Type))
		{
			obj = valueInfo.Converter.ConvertTo(null, CultureInfo.InvariantCulture, valueInfo.Value, valueInfo.Type);
		}
		return obj;
	}

	private static object GetInstance(ListInfo listInfo)
	{
		object result = null;
		if (listInfo.Values.Count < 1)
		{
			return result;
		}
		Type type = typeof(List<>).MakeGenericType(listInfo.Values[0].Type);
		result = Activator.CreateInstance(type);
		MethodInfo method = type.GetMethod("Add");
		foreach (ValueInfo value in listInfo.Values)
		{
			object instance = GetInstance(value);
			method.Invoke(result, new object[1] { instance });
		}
		return result;
	}
}
