using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Sce.Atf.Wpf.Controls;

public class ClippingBorder : ContentControl
{
	public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(ClippingBorder), new PropertyMetadata(CornerRadius_Changed));

	public static readonly DependencyProperty ClipContentProperty = DependencyProperty.Register("ClipContent", typeof(bool), typeof(ClippingBorder), new PropertyMetadata(ClipContent_Changed));

	private ContentControl m_topLeftContentControl;

	private ContentControl m_topRightContentControl;

	private ContentControl m_bottomRightContentControl;

	private ContentControl m_bottomLeftContentControl;

	private RectangleGeometry m_topLeftClip;

	private RectangleGeometry m_topRightClip;

	private RectangleGeometry m_bottomRightClip;

	private RectangleGeometry m_bottomLeftClip;

	private Border m_border;

	[Category("Appearance")]
	[Description("Sets the corner radius on the border.")]
	public CornerRadius CornerRadius
	{
		get
		{
			return (CornerRadius)GetValue(CornerRadiusProperty);
		}
		set
		{
			SetValue(CornerRadiusProperty, value);
		}
	}

	[Category("Appearance")]
	[Description("Sets whether the content is clipped or not.")]
	public bool ClipContent
	{
		get
		{
			return (bool)GetValue(ClipContentProperty);
		}
		set
		{
			SetValue(ClipContentProperty, value);
		}
	}

	public ClippingBorder()
	{
		base.DefaultStyleKey = typeof(ClippingBorder);
		base.SizeChanged += ClippingBorder_SizeChanged;
	}

	public override void OnApplyTemplate()
	{
		base.OnApplyTemplate();
		m_border = GetTemplateChild("PART_Border") as Border;
		m_topLeftContentControl = GetTemplateChild("PART_TopLeftContentControl") as ContentControl;
		m_topRightContentControl = GetTemplateChild("PART_TopRightContentControl") as ContentControl;
		m_bottomRightContentControl = GetTemplateChild("PART_BottomRightContentControl") as ContentControl;
		m_bottomLeftContentControl = GetTemplateChild("PART_BottomLeftContentControl") as ContentControl;
		if (m_topLeftContentControl != null)
		{
			m_topLeftContentControl.SizeChanged += ContentControl_SizeChanged;
		}
		m_topLeftClip = GetTemplateChild("PART_TopLeftClip") as RectangleGeometry;
		m_topRightClip = GetTemplateChild("PART_TopRightClip") as RectangleGeometry;
		m_bottomRightClip = GetTemplateChild("PART_BottomRightClip") as RectangleGeometry;
		m_bottomLeftClip = GetTemplateChild("PART_BottomLeftClip") as RectangleGeometry;
		UpdateClipContent(ClipContent);
		UpdateCornerRadius(CornerRadius);
	}

	internal void UpdateCornerRadius(CornerRadius newCornerRadius)
	{
		if (m_border != null)
		{
			m_border.CornerRadius = newCornerRadius;
		}
		if (m_topLeftClip != null)
		{
			RectangleGeometry topLeftClip = m_topLeftClip;
			double radiusX = (m_topLeftClip.RadiusY = newCornerRadius.TopLeft - Math.Min(base.BorderThickness.Left, base.BorderThickness.Top) / 2.0);
			topLeftClip.RadiusX = radiusX;
		}
		if (m_topRightClip != null)
		{
			RectangleGeometry topRightClip = m_topRightClip;
			double radiusX = (m_topRightClip.RadiusY = newCornerRadius.TopRight - Math.Min(base.BorderThickness.Top, base.BorderThickness.Right) / 2.0);
			topRightClip.RadiusX = radiusX;
		}
		if (m_bottomRightClip != null)
		{
			RectangleGeometry bottomRightClip = m_bottomRightClip;
			double radiusX = (m_bottomRightClip.RadiusY = newCornerRadius.BottomRight - Math.Min(base.BorderThickness.Right, base.BorderThickness.Bottom) / 2.0);
			bottomRightClip.RadiusX = radiusX;
		}
		if (m_bottomLeftClip != null)
		{
			RectangleGeometry bottomLeftClip = m_bottomLeftClip;
			double radiusX = (m_bottomLeftClip.RadiusY = newCornerRadius.BottomLeft - Math.Min(base.BorderThickness.Bottom, base.BorderThickness.Left) / 2.0);
			bottomLeftClip.RadiusX = radiusX;
		}
		UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
	}

	internal void UpdateClipContent(bool clipContent)
	{
		if (clipContent)
		{
			if (m_topLeftContentControl != null)
			{
				m_topLeftContentControl.Clip = m_topLeftClip;
			}
			if (m_topRightContentControl != null)
			{
				m_topRightContentControl.Clip = m_topRightClip;
			}
			if (m_bottomRightContentControl != null)
			{
				m_bottomRightContentControl.Clip = m_bottomRightClip;
			}
			if (m_bottomLeftContentControl != null)
			{
				m_bottomLeftContentControl.Clip = m_bottomLeftClip;
			}
			UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
		}
		else
		{
			if (m_topLeftContentControl != null)
			{
				m_topLeftContentControl.Clip = null;
			}
			if (m_topRightContentControl != null)
			{
				m_topRightContentControl.Clip = null;
			}
			if (m_bottomRightContentControl != null)
			{
				m_bottomRightContentControl.Clip = null;
			}
			if (m_bottomLeftContentControl != null)
			{
				m_bottomLeftContentControl.Clip = null;
			}
		}
	}

	private static void CornerRadius_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		ClippingBorder clippingBorder = (ClippingBorder)dependencyObject;
		clippingBorder.UpdateCornerRadius((CornerRadius)eventArgs.NewValue);
	}

	private static void ClipContent_Changed(DependencyObject dependencyObject, DependencyPropertyChangedEventArgs eventArgs)
	{
		ClippingBorder clippingBorder = (ClippingBorder)dependencyObject;
		clippingBorder.UpdateClipContent((bool)eventArgs.NewValue);
	}

	private void ClippingBorder_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (ClipContent)
		{
			UpdateClipSize(e.NewSize);
		}
	}

	private void ContentControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
		if (ClipContent)
		{
			UpdateClipSize(new Size(base.ActualWidth, base.ActualHeight));
		}
	}

	private void UpdateClipSize(Size size)
	{
		if (size.Width > 0.0 || size.Height > 0.0)
		{
			double num = Math.Max(0.0, size.Width - base.BorderThickness.Left - base.BorderThickness.Right);
			double num2 = Math.Max(0.0, size.Height - base.BorderThickness.Top - base.BorderThickness.Bottom);
			if (m_topLeftClip != null)
			{
				m_topLeftClip.Rect = new Rect(0.0, 0.0, num + CornerRadius.TopLeft * 2.0, num2 + CornerRadius.TopLeft * 2.0);
			}
			if (m_topRightClip != null)
			{
				m_topRightClip.Rect = new Rect(0.0 - CornerRadius.TopRight, 0.0, num + CornerRadius.TopRight, num2 + CornerRadius.TopRight);
			}
			if (m_bottomRightClip != null)
			{
				m_bottomRightClip.Rect = new Rect(0.0 - CornerRadius.BottomRight, 0.0 - CornerRadius.BottomRight, num + CornerRadius.BottomRight, num2 + CornerRadius.BottomRight);
			}
			if (m_bottomLeftClip != null)
			{
				m_bottomLeftClip.Rect = new Rect(0.0, 0.0 - CornerRadius.BottomLeft, num + CornerRadius.BottomLeft, num2 + CornerRadius.BottomLeft);
			}
		}
	}
}
