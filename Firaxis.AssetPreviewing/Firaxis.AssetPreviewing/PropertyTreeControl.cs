using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using Firaxis.ATF;
using Firaxis.CivTech;
using Sce.Atf;
using Sce.Atf.Adaptation;
using Sce.Atf.Applications;
using WeifenLuo.WinFormsUI.Docking;

namespace Firaxis.AssetPreviewing;

public class PropertyTreeControl : Control
{
	private enum PropertyHitType
	{
		Label,
		Value
	}

	private class FindPropertyInfo
	{
		public readonly PropertyDescriptor Property;

		public readonly Rectangle LabelRect;

		public readonly Rectangle ValueRect;

		public readonly PropertyHitType HitType;

		public FindPropertyInfo(PropertyDescriptor prop, Rectangle lblRc, Rectangle valRc, PropertyHitType hit)
		{
			Property = prop;
			LabelRect = lblRc;
			ValueRect = valRc;
			HitType = hit;
		}
	}

	private class PropertyCategory
	{
		public IList<PropertyDescriptor> Descriptors = new List<PropertyDescriptor>();

		public bool Collapsed = false;
	}

	private static StringFormat LabelFormat = new StringFormat(StringFormatFlags.NoWrap)
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Near
	};

	private static StringFormat ValueFormat = new StringFormat(StringFormatFlags.NoWrap)
	{
		LineAlignment = StringAlignment.Center,
		Alignment = StringAlignment.Far
	};

	private static readonly int kTextSpacing = 10;

	private static readonly int kCategorySpacing = 5;

	private IThemeService ThemeService;

	private ICustomTypeDescriptor TypeDescriptor;

	private int ScrollOffset;

	private int ContentHeight;

	private int TextHeight;

	private int LabelSplitter;

	private bool UpdatingSplitter;

	private PaintedStyleInfo StyleInfo = new PaintedStyleInfo();

	private PropertyDescriptor SelectedProperty;

	private ScrollBar m_scrollBar;

	private IDictionary<string, PropertyCategory> PropertyByCategoryLookup = new Dictionary<string, PropertyCategory>();

	private PaintedState _state = default(PaintedState);

	public Color GridLineColor
	{
		get
		{
			return StyleInfo.GridLineColor;
		}
		set
		{
			StyleInfo.GridLineColor = value;
		}
	}

	public Color OddBackColor
	{
		get
		{
			return StyleInfo.OddBackColor;
		}
		set
		{
			StyleInfo.OddBackColor = value;
		}
	}

	public Color EvenBackColor
	{
		get
		{
			return StyleInfo.EvenBackColor;
		}
		set
		{
			StyleInfo.EvenBackColor = value;
		}
	}

	public Color SelectedForeColor
	{
		get
		{
			return StyleInfo.SelectedForeColor;
		}
		set
		{
			StyleInfo.SelectedForeColor = value;
		}
	}

	public Color SelectedBackColor
	{
		get
		{
			return StyleInfo.SelectedBackColor;
		}
		set
		{
			StyleInfo.SelectedBackColor = value;
		}
	}

	protected override CreateParams CreateParams
	{
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle |= 33554432;
			return createParams;
		}
	}

	public PropertyTreeControl(IThemeService themeSvc, ICustomTypeDescriptor typeDescriptor)
	{
		ThemeService = themeSvc;
		TypeDescriptor = typeDescriptor;
		base.Paint += PropertyTreeControl_Paint;
		base.MouseDown += PropertyTreeControl_MouseDown;
		base.MouseUp += PropertyTreeControl_MouseUp;
		base.MouseClick += PropertyTreeControl_MouseClick;
		base.MouseDoubleClick += PropertyTreeControl_MouseClick;
		base.MouseMove += PropertyTreeControl_MouseMove;
		base.MouseWheel += PropertyTreeControl_MouseWheel;
		base.SizeChanged += PropertyTreeControl_SizeChanged;
		StyleInfo.Font = Font;
		base.FontChanged += PropertyTreeControl_FontChanged;
		base.BackColorChanged += PropertyTreeControl_BackColorChanged;
		StyleInfo.BackColor = BackColor;
		base.ForeColorChanged += PropertyTreeControl_ForeColorChanged;
		StyleInfo.ForeColor = ForeColor;
	}

	public void SetTypeDescriptor(ICustomTypeDescriptor typeDescriptor)
	{
		if (TypeDescriptor == typeDescriptor)
		{
			return;
		}
		TypeDescriptor = typeDescriptor;
		SelectedProperty = null;
		UpdatePropertyCache();
		Invalidate();
	}

	private void PropertyTreeControl_SizeChanged(object sender, EventArgs e)
	{
		LabelSplitter = (int)((double)(base.ClientRectangle.Width - 1) * 0.333 + 0.5);
	}

	private void PropertyTreeControl_ForeColorChanged(object sender, EventArgs e)
	{
		StyleInfo.ForeColor = ForeColor;
	}

	private void PropertyTreeControl_BackColorChanged(object sender, EventArgs e)
	{
		StyleInfo.BackColor = BackColor;
	}

	private void PropertyTreeControl_FontChanged(object sender, EventArgs e)
	{
		StyleInfo.Font = Font;
		using Graphics graphics = CreateGraphics();
		TextHeight = (int)graphics.MeasureString("jJ", Font).Height;
		LabelSplitter = (int)((double)(base.ClientRectangle.Width - 1) * 0.333 + 0.5);
	}

	private bool ArePropertiesEqual(PropertyDescriptor prop1, PropertyDescriptor prop2)
	{
		if (prop1 == null && prop2 == null)
		{
			return true;
		}
		return prop1?.Equals(prop2) ?? false;
	}

	private bool IsPainter(PropertyDescriptor prop)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		return reference.Is<IPaintedPropertyEditor>();
	}

	private Rectangle HandlePropertyMouseDown(PropertyDescriptor prop, MouseButtons btns, Rectangle lblRc, Rectangle valRc, Point clkPt)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IPaintedPropertyEditor paintedPropertyEditor = reference.As<IPaintedPropertyEditor>();
		return paintedPropertyEditor.HandleMouseDown(btns, lblRc, valRc, clkPt);
	}

	private Rectangle HandlePropertyMouseUp(PropertyDescriptor prop, MouseButtons btns, Rectangle lblRc, Rectangle valRc, Point clkPt)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IPaintedPropertyEditor paintedPropertyEditor = reference.As<IPaintedPropertyEditor>();
		return paintedPropertyEditor.HandleMouseUp(btns, lblRc, valRc, clkPt);
	}

	private Rectangle HandlePropertyMouseClick(PropertyDescriptor prop, MouseButtons btns, Rectangle lblRc, Rectangle valRc, Point clkPt)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IPaintedPropertyEditor paintedPropertyEditor = reference.As<IPaintedPropertyEditor>();
		return paintedPropertyEditor.HandleMouseClick(btns, lblRc, valRc, clkPt);
	}

	private Rectangle HandlePropertyMouseMove(PropertyDescriptor prop, MouseButtons btns, Rectangle lblRc, Rectangle valRc, Point movePt)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IPaintedPropertyEditor paintedPropertyEditor = reference.As<IPaintedPropertyEditor>();
		return paintedPropertyEditor.HandleMouseMove(btns, lblRc, valRc, movePt);
	}

	private bool IsInlineEditor(PropertyDescriptor prop)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		return reference.As<IActivatablePropertyEditor>().IsInline;
	}

	private bool IsActivatable(PropertyDescriptor prop)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		return reference.Is<IActivatablePropertyEditor>();
	}

	private bool IsActive(PropertyDescriptor prop)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		return reference.As<IActivatablePropertyEditor>().Active;
	}

	private void DeactivateProperty(PropertyDescriptor prop)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IActivatablePropertyEditor activatablePropertyEditor = reference.As<IActivatablePropertyEditor>();
		activatablePropertyEditor.ValueCommitted -= Activable_ValueCommitted;
		base.Controls.Remove(activatablePropertyEditor.PropertyControl);
		activatablePropertyEditor.DeactivateControl();
	}

	private void InvalidateProperty(PropertyDescriptor prop)
	{
		Rectangle propertyRect = GetPropertyRect(prop);
		Invalidate(propertyRect);
	}

	private void ActivateProperty(PropertyDescriptor prop, Rectangle rect)
	{
		object reference = prop?.GetEditor(TypeDescriptor.GetType());
		IActivatablePropertyEditor activatablePropertyEditor = reference.As<IActivatablePropertyEditor>();
		Control control = activatablePropertyEditor.ActivatePropertyControl(TypeDescriptor, prop);
		activatablePropertyEditor.ValueCommitted += Activable_ValueCommitted;
		Point location = rect.Location;
		Size size = rect.Size;
		control.Left = location.X + 1;
		control.Top = location.Y + 1;
		control.Width = size.Width - 1;
		control.Height = size.Height - 1;
		control.Visible = true;
		base.Controls.Add(control);
		SkinService.ApplyActiveSkin(control);
		control.Focus();
	}

	private void Activable_ValueCommitted(object sender, EventArgs e)
	{
		BugSubmitter.Assert(SelectedProperty != null, "Value committed with no active property!");
		DeactivateProperty(SelectedProperty);
		InvalidateProperty(SelectedProperty);
	}

	private void SelectProperty(PropertyDescriptor prop)
	{
		SelectedProperty = prop;
		Invalidate(GetPropertyRect(SelectedProperty));
	}

	private void ClearSelectedProperty()
	{
		if (IsActivatable(SelectedProperty) && IsActive(SelectedProperty))
		{
			DeactivateProperty(SelectedProperty);
		}
		Rectangle propertyRect = GetPropertyRect(SelectedProperty);
		SelectedProperty = null;
		Invalidate(propertyRect);
	}

	private void UpdatePropertyCache()
	{
		PropertyByCategoryLookup.ForEach(delegate(KeyValuePair<string, PropertyCategory> cat)
		{
			IList<PropertyDescriptor> descriptors = cat.Value.Descriptors;
			descriptors.ForEach(delegate(PropertyDescriptor des)
			{
				des.RemoveValueChanged(des, HandlePropertyChanged);
			});
			descriptors.Clear();
		});
		foreach (PropertyDescriptor item in TypeDescriptor.GetProperties().AsIEnumerable<PropertyDescriptor>())
		{
			if (item.IsBrowsable)
			{
				string key = "Base";
				if (!string.IsNullOrEmpty(item.Category))
				{
					key = item.Category;
				}
				if (!PropertyByCategoryLookup.ContainsKey(key))
				{
					PropertyByCategoryLookup[key] = new PropertyCategory();
				}
				PropertyByCategoryLookup[key].Descriptors.Add(item);
				item.AddValueChanged(item, HandlePropertyChanged);
			}
		}
		string[] that = (from sc in PropertyByCategoryLookup
			where sc.Value.Descriptors.Count == 0
			select sc.Key).ToArray();
		that.ForEach(delegate(string cat)
		{
			PropertyByCategoryLookup.Remove(cat);
		});
		ContentHeight = ComputeContentHeight();
		if (ContentHeight > base.ClientRectangle.Height)
		{
			if (m_scrollBar == null)
			{
				m_scrollBar = new VScrollBar();
				m_scrollBar.LargeChange = base.Height / 20;
				m_scrollBar.SmallChange = base.Height / 10;
				m_scrollBar.Dock = DockStyle.Right;
				m_scrollBar.Minimum = 0;
				m_scrollBar.Maximum = ContentHeight - base.ClientRectangle.Height + m_scrollBar.LargeChange;
				m_scrollBar.Scroll += ScrollBar_Scroll;
				base.Controls.Add(m_scrollBar);
			}
		}
		else if (m_scrollBar != null)
		{
			m_scrollBar.Scroll -= ScrollBar_Scroll;
			base.Controls.Remove(m_scrollBar);
			m_scrollBar.Dispose();
			m_scrollBar = null;
		}
	}

	private void HandlePropertyChanged(object sender, EventArgs args)
	{
		if (!(sender is PropertyDescriptor testProp))
		{
			return;
		}
		if (base.InvokeRequired)
		{
			BeginInvoke((Action)delegate
			{
				HandlePropertyChanged(sender, args);
			});
		}
		else
		{
			Rectangle propertyRect = GetPropertyRect(testProp);
			Invalidate(propertyRect);
		}
	}

	private Rectangle ComputeDropArea(FindPropertyInfo propInfo)
	{
		Rectangle valueRect = propInfo.ValueRect;
		valueRect.Offset(0, propInfo.ValueRect.Height + 1);
		valueRect.Height = 6 * propInfo.ValueRect.Height;
		valueRect.Intersect(base.ClientRectangle);
		return valueRect;
	}

	private int WheelDeltaToPropertyHeight(int delta)
	{
		return (int)((float)delta / (float)SystemInformation.MouseWheelScrollDelta * (float)(TextHeight + kTextSpacing));
	}

	private void ScrollContent(int scrollAmount, int scrollMax)
	{
		int val = Math.Max(ContentHeight - base.ClientRectangle.Height, 0);
		int num = Math.Min(Math.Max(ScrollOffset + scrollAmount, 0), val);
		if (num != ScrollOffset)
		{
			if (SelectedProperty != null && IsActivatable(SelectedProperty) && IsActive(SelectedProperty))
			{
				DeactivateProperty(SelectedProperty);
			}
			ScrollOffset = num;
			Invalidate();
		}
	}

	private void PropertyTreeControl_MouseWheel(object sender, MouseEventArgs e)
	{
		int scrollAmount = -WheelDeltaToPropertyHeight(e.Delta);
		int num = ContentHeight - base.ClientRectangle.Height + base.Height / 20;
		ScrollContent(scrollAmount, num);
		if (m_scrollBar != null)
		{
			m_scrollBar.Maximum = num;
			m_scrollBar.Value = ScrollOffset;
		}
	}

	private void ScrollBar_Scroll(object sender, ScrollEventArgs e)
	{
		int scrollAmount = e.NewValue - e.OldValue;
		int scrollMax = ContentHeight - base.ClientRectangle.Height + base.Height / 20;
		ScrollContent(scrollAmount, scrollMax);
	}

	private void PropertyTreeControl_MouseMove(object sender, MouseEventArgs e)
	{
		if ((SelectedProperty == null || !IsActivatable(SelectedProperty) || !IsActive(SelectedProperty)) && !Focused)
		{
			Focus();
		}
		UpdatePropertyCache();
		if (UpdatingSplitter)
		{
			int num = Math.Min(LabelSplitter, e.Location.X);
			int num2 = Math.Max(LabelSplitter, e.Location.X) - num;
			Rectangle rc = new Rectangle(num, base.Top, num2, base.Height);
			rc.Inflate(15, 0);
			LabelSplitter = Math.Min(e.Location.X, base.ClientRectangle.Right - (TextHeight + kTextSpacing) - 1);
			Invalidate(rc);
			Update();
			return;
		}
		if (e.Location.X >= LabelSplitter - 1 && e.Location.X <= LabelSplitter + 1)
		{
			Cursor.Current = Cursors.VSplit;
			return;
		}
		PropertyCategory propertyCategory = FindCategory(e.Location);
		if (propertyCategory != null)
		{
			Cursor.Current = Cursors.Hand;
			return;
		}
		if (Cursor.Current == Cursors.VSplit)
		{
			Cursor.Current = Cursors.Arrow;
		}
		FindPropertyInfo findPropertyInfo = PickProperty(e.Location);
		PropertyDescriptor propertyDescriptor = findPropertyInfo?.Property;
		if (propertyDescriptor != null && IsPainter(propertyDescriptor))
		{
			Rectangle rectangle = HandlePropertyMouseMove(propertyDescriptor, e.Button, findPropertyInfo.LabelRect, findPropertyInfo.ValueRect, e.Location);
			if (rectangle != Rectangle.Empty)
			{
				Invalidate(rectangle);
			}
		}
	}

	private void PropertyTreeControl_MouseDown(object sender, MouseEventArgs e)
	{
		UpdatePropertyCache();
		if (e.Location.X >= LabelSplitter - 1 && e.Location.X <= LabelSplitter + 1)
		{
			if (SelectedProperty != null && IsActivatable(SelectedProperty) && IsActive(SelectedProperty))
			{
				DeactivateProperty(SelectedProperty);
			}
			UpdatingSplitter = true;
			return;
		}
		if (Cursor.Current == Cursors.VSplit)
		{
			Cursor.Current = Cursors.Arrow;
		}
		FindPropertyInfo findPropertyInfo = PickProperty(e.Location);
		PropertyDescriptor propertyDescriptor = findPropertyInfo?.Property;
		if (propertyDescriptor != null && IsPainter(propertyDescriptor))
		{
			Rectangle rectangle = HandlePropertyMouseDown(propertyDescriptor, e.Button, findPropertyInfo.LabelRect, findPropertyInfo.ValueRect, e.Location);
			if (rectangle != Rectangle.Empty)
			{
				Invalidate();
				Update();
			}
		}
	}

	private void PropertyTreeControl_MouseUp(object sender, MouseEventArgs e)
	{
		UpdatePropertyCache();
		if (UpdatingSplitter)
		{
			LabelSplitter = Math.Min(e.Location.X, base.ClientRectangle.Right - (TextHeight + kTextSpacing) - 1);
			Invalidate();
		}
		else
		{
			if (Cursor.Current == Cursors.VSplit)
			{
				Cursor.Current = Cursors.Arrow;
			}
			FindPropertyInfo findPropertyInfo = PickProperty(e.Location);
			PropertyDescriptor propertyDescriptor = findPropertyInfo?.Property;
			if (propertyDescriptor != null && IsPainter(propertyDescriptor))
			{
				Rectangle rectangle = HandlePropertyMouseUp(propertyDescriptor, e.Button, findPropertyInfo.LabelRect, findPropertyInfo.ValueRect, e.Location);
				if (rectangle != Rectangle.Empty)
				{
					Invalidate();
					Update();
				}
			}
		}
		UpdatingSplitter = false;
	}

	private void PropertyTreeControl_MouseClick(object sender, MouseEventArgs e)
	{
		UpdatePropertyCache();
		PropertyCategory propertyCategory = FindCategory(e.Location);
		if (propertyCategory != null)
		{
			propertyCategory.Collapsed = !propertyCategory.Collapsed;
			if (SelectedProperty != null && propertyCategory.Descriptors.Contains(SelectedProperty))
			{
				ClearSelectedProperty();
			}
			Invalidate();
			return;
		}
		FindPropertyInfo findPropertyInfo = PickProperty(e.Location);
		PropertyDescriptor propertyDescriptor = findPropertyInfo?.Property;
		if (!ArePropertiesEqual(propertyDescriptor, SelectedProperty))
		{
			if (SelectedProperty != null)
			{
				ClearSelectedProperty();
			}
			if (propertyDescriptor != null)
			{
				SelectProperty(propertyDescriptor);
				if (IsActivatable(propertyDescriptor))
				{
					if (IsInlineEditor(propertyDescriptor))
					{
						ActivateProperty(propertyDescriptor, findPropertyInfo.ValueRect);
					}
					else
					{
						Rectangle rect = ComputeDropArea(findPropertyInfo);
						ActivateProperty(propertyDescriptor, rect);
					}
				}
			}
		}
		else if (IsActivatable(SelectedProperty))
		{
			if (IsActive(SelectedProperty))
			{
				DeactivateProperty(SelectedProperty);
			}
			else if (IsInlineEditor(SelectedProperty))
			{
				ActivateProperty(SelectedProperty, findPropertyInfo.ValueRect);
			}
			else
			{
				Rectangle rect2 = ComputeDropArea(findPropertyInfo);
				ActivateProperty(SelectedProperty, rect2);
			}
		}
		if (SelectedProperty != null && IsPainter(SelectedProperty))
		{
			Rectangle rectangle = HandlePropertyMouseClick(SelectedProperty, e.Button, findPropertyInfo.LabelRect, findPropertyInfo.ValueRect, e.Location);
			if (rectangle != Rectangle.Empty)
			{
				Invalidate();
				Update();
			}
		}
	}

	private void PropertyTreeControl_Paint(object sender, PaintEventArgs e)
	{
		UpdatePropertyCache();
		if (base.ClientRectangle.Width == 0)
		{
			return;
		}
		int num = TextHeight + kTextSpacing;
		float num2 = (float)Math.Floor((float)(num - 4) * 0.5f);
		Rectangle clientRectangle = base.ClientRectangle;
		int num3 = -ScrollOffset;
		int num4 = clientRectangle.Width - 1 - ((ContentHeight > clientRectangle.Height) ? SystemInformation.VerticalScrollBarWidth : 0);
		int labelSplitter = LabelSplitter;
		ThemeBase activeTheme = ThemeService.ActiveTheme;
		IPaintingService paintingService = activeTheme.PaintingService;
		SolidBrush brush = paintingService.GetBrush(StyleInfo.ForeColor);
		SolidBrush brush2 = paintingService.GetBrush(StyleInfo.SelectedForeColor);
		Pen pen = paintingService.GetPen(StyleInfo.GridLineColor);
		SolidBrush brush3 = paintingService.GetBrush(StyleInfo.SelectedBackColor);
		SolidBrush brush4 = paintingService.GetBrush(StyleInfo.BackColor);
		SolidBrush brush5 = paintingService.GetBrush(StyleInfo.EvenBackColor);
		SolidBrush brush6 = paintingService.GetBrush(StyleInfo.OddBackColor);
		pen.DashStyle = DashStyle.Solid;
		foreach (KeyValuePair<string, PropertyCategory> item in PropertyByCategoryLookup)
		{
			Rectangle rect = new Rectangle(0, num3, num4, num);
			Rectangle rectangle = new Rectangle(num, num3, num4 - num, num);
			if (item.Value.Collapsed)
			{
				float num5 = (float)Math.Floor((double)num2 * Math.Sqrt(2.0));
				float num6 = (float)Math.Floor(((float)num - num5) * 0.5f);
				float num7 = num5 * 0.5f;
				PointF[] points = new PointF[3]
				{
					new PointF(num6 + num2 / 2f, (float)num3 + num6),
					new PointF(num6 + num2 / 2f + num7, (float)num3 + num6 + num5 / 2f),
					new PointF(num6 + num2 / 2f, (float)num3 + num6 + num5)
				};
				e.Graphics.DrawRectangle(pen, rect);
				e.Graphics.FillPolygon(brush, points);
				e.Graphics.DrawString(item.Key, StyleInfo.Font, brush, rectangle, LabelFormat);
				num3 += num;
			}
			else
			{
				float num8 = (float)Math.Floor(((float)num - num2) * 0.5f);
				PointF[] points2 = new PointF[3]
				{
					new PointF(num8 + num2, (float)num3 + num8),
					new PointF(num8 + num2, (float)num3 + num8 + num2),
					new PointF(num8, (float)num3 + num8 + num2)
				};
				e.Graphics.DrawRectangle(pen, rect);
				e.Graphics.FillPolygon(brush, points2);
				e.Graphics.DrawString(item.Key, StyleInfo.Font, brush, rectangle, LabelFormat);
				num3 += num;
				int num9 = 0;
				foreach (PropertyDescriptor descriptor in item.Value.Descriptors)
				{
					if (!descriptor.IsBrowsable)
					{
						continue;
					}
					Rectangle rectangle2 = new Rectangle(0, num3, labelSplitter, num);
					Rectangle rectangle3 = new Rectangle(labelSplitter, num3, num4 - labelSplitter, num);
					string displayName = descriptor.DisplayName;
					object value = descriptor.GetValue(TypeDescriptor);
					e.Graphics.DrawRectangle(pen, rectangle2);
					e.Graphics.DrawRectangle(pen, rectangle3);
					rectangle2.Offset(1, 1);
					rectangle3.Offset(1, 1);
					rectangle2.Width--;
					rectangle3.Width--;
					rectangle2.Height--;
					rectangle3.Height--;
					bool flag = num9 % 2 == 0;
					SolidBrush brush7 = (flag ? brush5 : brush6);
					bool flag2 = ArePropertiesEqual(descriptor, SelectedProperty);
					if (flag2)
					{
						e.Graphics.FillRectangle(brush3, rectangle3);
						e.Graphics.FillRectangle(brush3, rectangle2);
					}
					else
					{
						e.Graphics.FillRectangle(brush7, rectangle3);
						e.Graphics.FillRectangle(brush7, rectangle2);
					}
					e.Graphics.DrawString(displayName, StyleInfo.Font, flag2 ? brush2 : brush, rectangle2, LabelFormat);
					object editor = descriptor.GetEditor(TypeDescriptor.GetType());
					IActivatablePropertyEditor activatablePropertyEditor = editor.As<IActivatablePropertyEditor>();
					IPaintedPropertyEditor paintedPropertyEditor = editor.As<IPaintedPropertyEditor>();
					if (activatablePropertyEditor == null || !activatablePropertyEditor.Active)
					{
						if (paintedPropertyEditor != null)
						{
							_state.Selected = flag2;
							_state.EvenRow = flag;
							paintedPropertyEditor.PaintValue(StyleInfo, _state, value, e.Graphics, rectangle3);
						}
						else
						{
							string text = value?.ToString();
							if (!string.IsNullOrEmpty(text))
							{
								e.Graphics.DrawString(text, Font, flag2 ? brush2 : brush, rectangle3, ValueFormat);
							}
						}
					}
					num9++;
					num3 += num;
				}
			}
			num3 += kCategorySpacing;
		}
	}

	private int ComputeContentHeight()
	{
		int num = TextHeight + kTextSpacing;
		int num2 = 0;
		foreach (KeyValuePair<string, PropertyCategory> item in PropertyByCategoryLookup)
		{
			num2 += num;
			if (!item.Value.Collapsed)
			{
				foreach (PropertyDescriptor descriptor in item.Value.Descriptors)
				{
					if (descriptor.IsBrowsable)
					{
						num2 += num;
					}
				}
			}
			num2 += kCategorySpacing;
		}
		return num2;
	}

	private PropertyCategory FindCategory(Point cur)
	{
		int num = TextHeight + kTextSpacing;
		Rectangle rectangle = new Rectangle(0, -ScrollOffset, base.Width, num);
		foreach (KeyValuePair<string, PropertyCategory> item in PropertyByCategoryLookup)
		{
			if (rectangle.Contains(cur))
			{
				return item.Value;
			}
			rectangle.Offset(0, num);
			if (!item.Value.Collapsed)
			{
				foreach (PropertyDescriptor descriptor in item.Value.Descriptors)
				{
					if (descriptor.IsBrowsable)
					{
						rectangle.Offset(0, num);
					}
				}
			}
			rectangle.Offset(0, kCategorySpacing);
		}
		return null;
	}

	private FindPropertyInfo PickProperty(Point cur)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int num = TextHeight + kTextSpacing;
		int num2 = -ScrollOffset;
		int num3 = clientRectangle.Width - 1 - ((ContentHeight > clientRectangle.Height) ? SystemInformation.VerticalScrollBarWidth : 0);
		int labelSplitter = LabelSplitter;
		Rectangle lblRc = new Rectangle(0, num2, labelSplitter, num);
		Rectangle valRc = new Rectangle(labelSplitter, num2, num3 - labelSplitter, num);
		foreach (KeyValuePair<string, PropertyCategory> item in PropertyByCategoryLookup)
		{
			lblRc.Offset(0, num);
			valRc.Offset(0, num);
			if (!item.Value.Collapsed)
			{
				foreach (PropertyDescriptor descriptor in item.Value.Descriptors)
				{
					if (descriptor.IsBrowsable)
					{
						if (lblRc.Contains(cur))
						{
							return new FindPropertyInfo(descriptor, lblRc, valRc, PropertyHitType.Label);
						}
						if (valRc.Contains(cur))
						{
							return new FindPropertyInfo(descriptor, lblRc, valRc, PropertyHitType.Value);
						}
						lblRc.Offset(0, num);
						valRc.Offset(0, num);
					}
				}
			}
			lblRc.Offset(0, kCategorySpacing);
			valRc.Offset(0, kCategorySpacing);
		}
		return null;
	}

	private Rectangle GetPropertyRect(PropertyDescriptor testProp)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int num = TextHeight + kTextSpacing;
		int num2 = -ScrollOffset;
		int num3 = clientRectangle.Width - 1 - ((ContentHeight > clientRectangle.Height) ? SystemInformation.VerticalScrollBarWidth : 0);
		int labelSplitter = LabelSplitter;
		Rectangle result = new Rectangle(0, num2, num3, num);
		foreach (KeyValuePair<string, PropertyCategory> item in PropertyByCategoryLookup)
		{
			result.Offset(0, num);
			if (!item.Value.Collapsed)
			{
				foreach (PropertyDescriptor descriptor in item.Value.Descriptors)
				{
					if (descriptor.IsBrowsable)
					{
						if (descriptor.Equals(testProp))
						{
							return result;
						}
						result.Offset(0, num);
					}
				}
			}
			result.Offset(0, kCategorySpacing);
		}
		return Rectangle.Empty;
	}
}
